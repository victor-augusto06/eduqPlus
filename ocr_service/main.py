import os
import logging 
from typing import List
from fastapi import FastAPI, UploadFile, File, Depends, HTTPException, Security
from fastapi.responses import JSONResponse
from fastapi.security.api_key import APIKeyHeader
from dotenv import load_dotenv

from image_processor import process_image

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger("ocr_api")

load_dotenv()

API_KEY_NAME = "X-API-KEY"
SECRET_API_KEY = os.getenv("OCR_API_KEY", "chave-secreta-desenvolvimento")
api_key_header = APIKeyHeader(name=API_KEY_NAME, auto_error=True)

app = FastAPI(
    title="Eduq+ OCR API", 
    description="Python microservice for image processing and text extraction"
)

def verify_api_key(api_key: str = Security(api_key_header)):
    if api_key != SECRET_API_KEY:
        logger.warning("Tentativa de acesso negada: API Key inválida.")
        raise HTTPException(status_code=403, detail="Access Denied: Invalid API Key.")
    return api_key

@app.post("/extract-text/")
async def extract_text_endpoint(
    files: List[UploadFile] = File(...), 
    api_key: str = Depends(verify_api_key) 
):
    try:
        logger.info(f"[INÍCIO] Recebida requisição POST para processar {len(files)} arquivo(s).")
        resultados_textos = [] 
        
        for file in files:
            logger.info(f"[LENDO ARQUIVO] Extraindo bytes de: {file.filename}")
            image_content = await file.read()
            
            texto_extraido = process_image(image_content)
            
            resultados_textos.append({
                "nome_arquivo": file.filename,
                "texto": texto_extraido
            })
            logger.info(f"[SUCESSO] Processamento de '{file.filename}' finalizado.")

        logger.info("[FIM] Requisição concluída com sucesso. Retornando JSON para o C#.")
        return JSONResponse(content={"success": True, "resultados": resultados_textos})
    
    except Exception as e:
        logger.error(f"[ERRO CRÍTICO] Falha na pipeline de OCR: {str(e)}")
        return JSONResponse(status_code=500, content={"success": False, "error": str(e)})