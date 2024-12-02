import face_recognition
mypics = ["/home/ijimmyi/Downloads/WhatsApp Image 2024-08-01 at 3.28.11 PM.jpeg", "/home/ijimmyi/Downloads/WhatsApp Image 2024-10-12 at 8.12.38 PM.jpeg", "/home/ijimmyi/Downloads/WhatsApp Image 2024-01-23 at 9.14.15 AM.jpeg"]
encodings = [face_recognition.face_encodings(face_recognition.load_image_file(mypic))[0] for mypic in mypics]

with open("encodings/adham.csv","w") as file:
    for encoding in encodings:
        string = [str(val) for val in encoding.tolist()]
        file.write(",".join(string))
        file.write("\n")



