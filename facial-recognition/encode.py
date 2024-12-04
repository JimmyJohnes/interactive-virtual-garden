import face_recognition
import os

name = input("name of person: ")
pic_dir = input("directory of images: ")
encodings = []
for path, folders, files in os.walk(pic_dir):
    # Open file
    for filename in files:
        with open(os.path.join(pic_dir, filename)) as pic:
            encodings.append(face_recognition.face_encodings(face_recognition.load_image_file(pic))[0])

with open(f"encodings/{name}.csv","w") as file:
    for encoding in encodings:
        string = [str(val) for val in encoding.tolist()]
        file.write(",".join(string))
        file.write("\n")



