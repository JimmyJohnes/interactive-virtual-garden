import cv2
from deepface import DeepFace
from google.colab.patches import cv2_imshow


img_path = ''
# read image
img = cv2.imread(img_path)

cv2_imshow(img)

# Detect emotion
required_outputs =  ['emotion']
result = DeepFace.analyze(img,actions = required_outputs )