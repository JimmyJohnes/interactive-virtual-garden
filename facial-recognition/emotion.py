import cv2
from deepface import DeepFace
import json
import sys
import cam



def detect_emotion(image):
    # Detect emotion
    required_outputs =  ['emotion']
    result = DeepFace.analyze(image,actions = required_outputs )
    emotion_with_heighest_value = max(result[0]["emotion"], key=result[0]["emotion"].get)
    return emotion_with_heighest_value

if __name__ == "__main__":
    # TODO: detect emotion using webcam
    _, image = cam.capture_image
    result = detect_emotion(image)
    