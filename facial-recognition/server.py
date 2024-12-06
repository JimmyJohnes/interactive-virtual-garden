import socket
import threading
import asyncio
import cam
import recognize
import emotion
from thread_with_return_value import *
import json


def exec_face_recognition(image):
    encodings = recognize.read_encodings("encodings/")
    result = recognize.recogonize_face(image,encodings)
    return result

def exec_emotion_detection(image):
    result = emotion.detect_emotion(image)
    return result



def server():
    try:
        while True:
            print("Waiting for a new connection...")
            c, addr = s.accept()
            _,image = cam.capture_image()
            face_recognition_thread = ThreadWithReutrn(target=exec_face_recognition, args=(image,))
            emotion_detection_thread = ThreadWithReutrn(target=exec_emotion_detection, args=(image,))
            face_recognition_thread.start()
            emotion_detection_thread.start()
            detected_face = face_recognition_thread.join()
            detected_emotion = emotion_detection_thread.join()

            face_properties = {
                "identity": detected_face,
                "emotion": detected_emotion
            }
            
            c.sendall(json.dumps(face_properties).encode())
            
    except KeyboardInterrupt:
        print("Stopped Server: KeyboardInterrupt")
    finally:
        s.close()
        print("Socket closed")

if __name__ == "__main__":
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    port = 3001
    s.bind(('127.0.0.1', port))
    s.listen(1)
    print(f"Socket successfully created and listening on port {port}")
    
    try:
        server()
    except Exception as e:
        print(f"Server error: {e}")
