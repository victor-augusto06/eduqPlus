import os
import logging
import asyncio
from typing import List, Dict, Any
from contextlib import asynccontextmanager
from fastapi import FastAPI, Depends, HTTPException, Security
from fastapi.responses import JSONResponse
from fastapi.security.api_key import APIKeyHeader
from pydantic import BaseModel
from dotenv import load_dotenv
import pymysql
import pymysql.cursors

from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.neighbors import NearestNeighbors
import numpy as np

logging.basicConfig(level=logging.INFO, format="%(asctime)s - %(name)s - %(levelname)s - %(message)s")
logger = logging.getLogger("recsys_api")

load_dotenv()

API_KEY_NAME = "X-API-KEY"
SECRET_API_KEY = os.getenv("REC_API_KEY", "default-secret-key")
TRUST_SCORE_WEIGHT = float(os.getenv("TRUST_SCORE_WEIGHT", "2.0"))


api_key_header = APIKeyHeader(name=API_KEY_NAME, auto_error=True)

PORTUGUESE_STOPWORDS = [
    "o", "a", "os", "as", "um", "uma", "uns", "umas", 
    "de", "do", "da", "dos", "das", "em", "no", "na", 
    "nos", "nas", "por", "para", "com", "e", "é", "ou"
]

class RecSysState:
    course_ids: List[str] = []
    knn_model: NearestNeighbors = None
    final_features: np.ndarray = None
    is_trained: bool = False

state = RecSysState()

def fetch_courses_from_db() -> List[Dict[str, Any]]:
    db_host = os.getenv("DB_HOST", "localhost")
    db_user = os.getenv("DB_USER", "root")
    db_password = os.getenv("DB_PASSWORD", "")
    db_name = os.getenv("DB_NAME", "EduqPlus")
    db_port = int(os.getenv("DB_PORT", "3306"))
    
    courses = []
    try:
        connection = pymysql.connect(
            host=db_host,
            user=db_user,
            password=db_password,
            database=db_name,
            port=db_port,
            cursorclass=pymysql.cursors.DictCursor
        )
        
        with connection.cursor() as cursor:
            sql = "SELECT Id, Titulo, DescricaoOriginal, TrustScore FROM Curso"
            cursor.execute(sql)
            results = cursor.fetchall()
            
            for row in results:
                titulo = row.get("Titulo") or ""
                descricao = row.get("DescricaoOriginal") or ""
                text_content = f"{titulo} {descricao}".strip()
                
                trust_score = float(row.get("TrustScore") or 0.0)
                
                courses.append({
                    "id": row.get("Id"),
                    "text_content": text_content,
                    "trust_score": trust_score
                })
                
    except Exception as e:
        logger.error(f"[DB_ERROR] Falha ao coletar cursos do banco de dados MySQL: {str(e)}")
    finally:
        if 'connection' in locals() and connection.open:
            connection.close()
            
    return courses

async def training_routine():
    while True:
        try:
            logger.info("[JOB] Iniciando rotina de treinamento em background...")
            
            courses_data = fetch_courses_from_db()
            
            if len(courses_data) < 2:
                logger.warning("[JOB] Cursos insuficientes ou banco indisponível. Tentando novamente em 30 segundos...")
                await asyncio.sleep(30)
                continue
            
            course_ids = [str(c["id"]).lower() for c in courses_data]
            texts = [c["text_content"] for c in courses_data]
            scores = np.array([[c["trust_score"]] for c in courses_data])
            
            vectorizer = TfidfVectorizer(max_features=1000, stop_words=PORTUGUESE_STOPWORDS)
            tfidf_matrix = vectorizer.fit_transform(texts).toarray()
            
            max_score = np.max(scores) if np.max(scores) > 0 else 1.0
            normalized_scores = (scores / max_score) * TRUST_SCORE_WEIGHT
            
            final_features = np.hstack((tfidf_matrix, normalized_scores))
            
            knn = NearestNeighbors(metric='cosine')
            knn.fit(final_features)
            
            state.course_ids = course_ids
            state.final_features = final_features
            state.knn_model = knn
            state.is_trained = True
            
            logger.info("[JOB] Treinamento concluído com sucesso. Modelo atualizado na memória.")
            
        except Exception as e:
            logger.error(f"[JOB_ERROR] Falha na rotina de treinamento: {str(e)}")
            await asyncio.sleep(30)
            continue
            
        await asyncio.sleep(3600)

@asynccontextmanager
async def lifespan(app: FastAPI):
    task = asyncio.create_task(training_routine())
    yield
    task.cancel()

app = FastAPI(
    title="Eduq+ Recommendation API", 
    description="Microsserviço Python para recomendação de cursos via KNN",
    lifespan=lifespan
)

def verify_api_key(api_key: str = Security(api_key_header)):
    if api_key != SECRET_API_KEY:
        raise HTTPException(status_code=403, detail="Acesso Negado: API Key Inválida.")
    return api_key

class RecommendationRequest(BaseModel):
    target_course_id: str
    k: int = 5

@app.post("/recommend/")
def recommend_courses(payload: RecommendationRequest, api_key: str = Depends(verify_api_key)):
    try:
        target_id = payload.target_course_id.lower()
        logger.info(f"[INÍCIO] Calculando recomendações para o curso alvo: {target_id}")
        
        if not state.is_trained:
            raise HTTPException(status_code=503, detail="Modelo em treinamento inicial. Tente novamente em instantes.")
            
        if target_id not in state.course_ids:
             raise HTTPException(status_code=404, detail="O curso alvo não foi encontrado.")
             
        target_index = state.course_ids.index(target_id)
        target_features = state.final_features[target_index]
        
        n_neighbors = min(payload.k + 1, len(state.course_ids))
        
        distances, indices = state.knn_model.kneighbors(
            [target_features], 
            n_neighbors=n_neighbors
        )
        
        recommendations = []
        for i in range(1, len(indices[0])):
            idx = indices[0][i]
            recommendations.append({
                "course_id": state.course_ids[idx],
                "distance": float(distances[0][i])

            })
            
        logger.info("[FIM] Recomendações calculadas com sucesso.")
        return JSONResponse(content={"success": True, "recommendations": recommendations})
        
    except HTTPException as e:
        raise e
    except Exception as e:
        logger.error(f"[ERRO CRÍTICO] Falha no sistema de recomendação: {str(e)}")
        return JSONResponse(status_code=500, content={"success": False, "error": str(e)})
