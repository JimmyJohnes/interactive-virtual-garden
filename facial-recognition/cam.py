import cv2

def capture_image():
    cam = cv2.VideoCapture(0)

    result, image= cam.read()
    cam.release()
    return result, image

