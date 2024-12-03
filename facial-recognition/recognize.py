import face_recognition
import numpy as np
import cam

#TODO: Implement a way to dynamically figure the names of users

def read_encodings(csv_dir: str):
    encodings = []
    with open(csv_dir, "r") as file:
        encoded_strings = file.readlines()
        for string in encoded_strings:
            string = string.replace("\n","")
            encodings.append(np.asarray(string.split(","),dtype=np.float64))
    return encodings


def determine_whos_in_the_pic(comparison_results) -> bool:
# TODO: Implement an algorithm to figure out who is in the picture
    acceptance_count = comparison_results.count(np.True_)
    if acceptance_count / len(comparison_results) >= 0.75:
        return True
    return False


def recogonize_face(image,encodings)-> str:
    """
        takes in image and encodings, returns who is in the picture
    """
    image_locations = face_recognition.face_locations(image)
    unknown_encoding = face_recognition.face_encodings(image,image_locations)
    if len(unknown_encoding)<=0:
        exit("can't find faces in provided picture")

    unknown_encoding = unknown_encoding[0]

    results = face_recognition.compare_faces(encodings, unknown_encoding, tolerance=0.5)
    acceptance = determine_whos_in_the_pic(results)
    if acceptance:
        return "He's adham"
    return "can't identify the person in the picture"



result, image = cam.capture_image()
encodings = read_encodings("encodings/adham")
recogonize_face(image,encodings)
