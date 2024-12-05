import cv2
from deepface import DeepFace
import json


# TODO: detect emotion using webcam
img_path = ''
# read image
img = cv2.imread(img_path)


# Detect emotion
required_outputs =  ['emotion']
result = DeepFace.analyze(img,actions = required_outputs )

print(json.dumps(result))