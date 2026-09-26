import os
import logging
from typing import List
from fastapi import FastAPI, Depends, HTTPException, Security
from fastapi.responses import JSONResponse
from fastapi.security.api_key import APIKeyHeader
from pydantic import BaseModel
from dotenv import load_dotenv

from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.neighbors import NearestNeighbors
import numpy as np

logging.basicConfig(level=logging.INFO, format="%(asctime)s - %(name)s - %(levelname)s - %(message)s")
logger = logging.getLogger("recsys_api")

load_dotenv()

API_KEY_NAME = "X-API-KEY"
SECRET_API_KEY = os.getenv("REC_API_KEY", "default-secret-key")
api_key_header = APIKeyHeader(name=API_KEY_NAME, auto_error=True)

app = FastAPI(title="Eduq+ Recommendation API", description="Microsserviço Python para recomendação de cursos via KNN")

def verify_api_key(api_key: str = Security(api_key_header)):
    if api_key != SECRET_API_KEY:
        raise HTTPException(status_code=403, detail="Acesso Negado: API Key Inválida.")
    return api_key

class CourseFeature(BaseModel):
    id: str
    text_content: str  
    trust_score: float

class RecommendationRequest(BaseModel):
    target_course_id: str
    courses: List[CourseFeature]
    k: int = 5

@app.post("/recommend/")
def recommend_courses(payload: RecommendationRequest, api_key: str = Depends(verify_api_key)):
    try:
        logger.info(f"[INÍCIO] Calculando recomendações para o curso alvo: {payload.target_course_id}")
        
        courses = payload.courses
        if len(courses) < 2:
            return JSONResponse(content={"success": True, "recommendations": []})
            
        course_ids = [c.id for c in courses]
        texts = [c.text_content for c in courses]
        scores = np.array([[c.trust_score] for c in courses])
        
        if payload.target_course_id not in course_ids:
             raise ValueError("O curso alvo não foi encontrado na lista de cursos fornecida.")
             
        target_index = course_ids.index(payload.target_course_id)
        
        vectorizer = TfidfVectorizer(max_features=1000)
        tfidf_matrix = vectorizer.fit_transform(texts).toarray()
        
        max_score = np.max(scores) if np.max(scores) > 0 else 1.0
        normalized_scores = scores / max_score
        
        final_features = np.hstack((tfidf_matrix, normalized_scores))
        
        n_neighbors = min(payload.k + 1, len(courses))
        knn = NearestNeighbors(n_neighbors=n_neighbors, metric='euclidean')
        knn.fit(final_features)
        
        distances, indices = knn.kneighbors([final_features[target_index]])
        
        recommendations = []
        
        for i in range(1, len(indices[0])):
            idx = indices[0][i]
            recommendations.append({
                "course_id": course_ids[idx],
                "distance": float(distances[0][i])
            })
            
        logger.info("[FIM] Recomendações calculadas com sucesso.")
        return JSONResponse(content={"success": True, "recommendations": recommendations})
        
    except Exception as e:
        logger.error(f"[ERRO CRÍTICO] Falha no sistema de recomendação: {str(e)}")
        return JSONResponse(status_code=500, content={"success": False, "error": str(e)})
