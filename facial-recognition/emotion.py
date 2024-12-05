import cv2
from deepface import DeepFace
import json
import sys

sys.path.insert(0, "../facial-recognition/")

import cam


# TODO: detect emotion using webcam
_, image = cam.capture_image


# Detect emotion
required_outputs =  ['emotion']
result = DeepFace.analyze(image,actions = required_outputs )

print(json.dumps(result))