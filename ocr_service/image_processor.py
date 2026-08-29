import cv2
import numpy as np
import pytesseract
import os

if os.name == 'nt':
    pytesseract.pytesseract.tesseract_cmd = r'C:\Program Files\Tesseract-OCR\tesseract.exe'
    os.environ['TESSDATA_PREFIX'] = r'C:\Program Files\Tesseract-OCR\tessdata'

def process_image(image_bytes: bytes) -> str:
    nparr = np.frombuffer(image_bytes, np.uint8)
    original_image = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

    if original_image is None:
        raise ValueError("The uploaded file is not a valid image.")

    enlarged_image = cv2.resize(original_image, None, fx=2, fy=2, interpolation=cv2.INTER_CUBIC)
    gray_image = cv2.cvtColor(enlarged_image, cv2.COLOR_BGR2GRAY)
    
    sharpening_kernel = np.array([[-1, -1, -1], 
                                  [-1,  9, -1], 
                                  [-1, -1, -1]])
    sharpened_image = cv2.filter2D(gray_image, -1, sharpening_kernel)
    
    _, binary_image = cv2.threshold(sharpened_image, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
    
    kernel = np.ones((1, 1), np.uint8)
    clean_image = cv2.dilate(binary_image, kernel, iterations=1)
    
    ocr_config = '--psm 6'
    extracted_text = pytesseract.image_to_string(clean_image, lang='por', config=ocr_config)
    
    return extracted_text