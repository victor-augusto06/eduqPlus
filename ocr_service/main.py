import os
from typing import List
from fastapi import FastAPI, UploadFile, File, Depends, HTTPException, Security
from fastapi.responses import JSONResponse
from fastapi.security.api_key import APIKeyHeader
from dotenv import load_dotenv

from image_processor import process_image

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
        raise HTTPException(status_code=403, detail="Access Denied: Invalid API Key.")
    return api_key

@app.post("/extract-text/")
async def extract_text_endpoint(
    files: List[UploadFile] = File(...), 
    api_key: str = Depends(verify_api_key) 
):
    try:
        resultados_textos = [] 
        
        for file in files:
            image_content = await file.read()
            
            texto_extraido = process_image(image_content)
            
            resultados_textos.append({
                "nome_arquivo": file.filename,
                "texto": texto_extraido
            })

        return JSONResponse(content={"success": True, "resultados": resultados_textos})
    
    except Exception as e:
        return JSONResponse(status_code=500, content={"success": False, "error": str(e)})