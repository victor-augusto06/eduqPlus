import cv2
import numpy as np
import pytesseract
import os
import logging
import fitz

logger = logging.getLogger("image_processor")

if os.name == 'nt':
    pytesseract.pytesseract.tesseract_cmd = r'C:\Program Files\Tesseract-OCR\tesseract.exe'
    os.environ['TESSDATA_PREFIX'] = r'C:\Program Files\Tesseract-OCR\tessdata'

def process_image(image_bytes: bytes) -> str:
    logger.info("Iniciando decodificação da imagem a partir da matriz de bytes.")
    nparr = np.frombuffer(image_bytes, np.uint8)
    original_image = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

    if original_image is None:
        logger.info("Decodificação falhou. Tentando ler como PDF.")
        try:
            doc = fitz.open(stream=image_bytes, filetype="pdf")
            if len(doc) > 0:
                page = doc.load_page(0)
                pix = page.get_pixmap(dpi=300)
                img_array = np.frombuffer(pix.samples, dtype=np.uint8).reshape(pix.h, pix.w, pix.n)
                if pix.n == 4:
                    original_image = cv2.cvtColor(img_array, cv2.COLOR_RGBA2BGR)
                else:
                    original_image = cv2.cvtColor(img_array, cv2.COLOR_RGB2BGR)
            else:
                raise ValueError("O PDF está vazio.")
        except Exception as e:
            logger.error(f"Falha ao ler como PDF: {str(e)}")
            raise ValueError("The uploaded file is not a valid image or PDF.")

    logger.info("Aplicando redimensionamento (Scale Up x2).")
    enlarged_image = cv2.resize(original_image, None, fx=2, fy=2, interpolation=cv2.INTER_CUBIC)
    
    logger.info("Convertendo imagem para tons de cinza.")
    gray_image = cv2.cvtColor(enlarged_image, cv2.COLOR_BGR2GRAY)
    
    logger.info("Aplicando filtro matemático de nitidez (Sharpening).")
    sharpening_kernel = np.array([[-1, -1, -1], 
                                  [-1,  9, -1], 
                                  [-1, -1, -1]])
    sharpened_image = cv2.filter2D(gray_image, -1, sharpening_kernel)
    
    logger.info("Binarizando a imagem com método de Otsu.")
    _, binary_image = cv2.threshold(sharpened_image, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
    
    logger.info("Limpando imagem com operação morfológica (Dilatação leve).")
    kernel = np.ones((1, 1), np.uint8)
    clean_image = cv2.dilate(binary_image, kernel, iterations=1)
    
    logger.info("Enviando imagem tratada para o motor do Tesseract OCR...")
    ocr_config = '--psm 6'
    extracted_text = pytesseract.image_to_string(clean_image, lang='por', config=ocr_config)
    
    logger.info(f"Leitura OCR finalizada! Foram extraídos {len(extracted_text)} caracteres da imagem.")
    
    return extracted_text