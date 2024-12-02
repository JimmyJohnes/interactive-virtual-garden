import face_recognition
import numpy as np
import cam


encodings = []
with open("encodings/adham.csv","r") as file:
    encoded_strings = file.readlines()
    for string in encoded_strings:
        string = string.replace("\n","")
        encodings.append(np.asarray(string.split(","),dtype=np.float64))


result, image = cam.capture_image()

image_locations = face_recognition.face_locations(image)
unknown_encoding = face_recognition.face_encodings(image,image_locations)
if len(unknown_encoding)<=0:
    exit("can't find faces in provided picture")

unknown_encoding = unknown_encoding[0]

results = face_recognition.compare_faces(encodings, unknown_encoding)
print(results)

recognized = False
for i in range(len(results)):
    if results[i]:
        recognized = True
if not recognized:
    print("I can't recognize that character")

