import socket
import asyncio
import cam
import recognize

def server():
    try:
        while True:
            print("Waiting for a new connection...")
            c, addr = s.accept()
            _,image = cam.capture_image()
            encodings = recognize.read_encodings("encodings/")
            result = recognize.recogonize_face(image,encodings)
            c.sendall(result.encode())
            
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
