import pickle
import socket
import threading
from dollarpy import Template, Recognizer, Point
import mediapipe as mp
import cv2
from collections import namedtuple

# Initialize MediaPipe hands
mp_hands = mp.solutions.hands
hands = mp_hands.Hands(max_num_hands=2, min_detection_confidence=0.5, min_tracking_confidence=0.5)
mp_draw = mp.solutions.drawing_utils


# Load templates with pickle
def load_templates(filename="templates.pkl"):
    try:
        with open(filename, 'rb') as f:
            templates = pickle.load(f)
        print("Templates loaded from", filename)
        return templates
    except FileNotFoundError:
        print("No templates found.")
        return []

# Function to start a socket server for communication with the C# application
def start_socket_server(host='localhost', port=5000):
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind((host, port))
    server_socket.listen(1)
    print("Socket server started, waiting for connection...")
    client_socket, address = server_socket.accept()
    print(f"Connected to C# client at {address}")
    return client_socket, server_socket

# Function to handle gesture recognition and send results to C# client
def recognize_gesture(client_socket):
    # Load saved templates
    templates = load_templates()
    
    # Initialize Recognizer with loaded templates
    # Initialize Recognizer with loaded templates
    recognizer = Recognizer(templates)
    cap = cv2.VideoCapture(0)
    
    buffer = []  # Buffer to store points
    buffer_size = 40  # Number of frames to store in the buffer
    
    with mp_hands.Hands(
        static_image_mode=False,  # For video, set to False
        max_num_hands=2,  # Process one hand at a time for gesture recognition
        min_detection_confidence=0.5,
        min_tracking_confidence=0.5,
    ) as hands:
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break

            # Convert the frame to RGB
            rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

            # Process the frame and detect hands
            results = hands.process(rgb_frame)

            if results.multi_hand_landmarks:
                for hand_landmarks in results.multi_hand_landmarks:
                    frame_points = []
                    for landmark in hand_landmarks.landmark:
                        # Collect (x, y) positions as Points
                        frame_points.append(Point(landmark.x, landmark.y))

                    # Update the buffer with the latest points
                    buffer.append(frame_points)
                    if len(buffer) > buffer_size:
                        buffer.pop(0)  # Remove the oldest frame when buffer is full

                    # Attempt recognition when the buffer is full
                    if len(buffer) == buffer_size:
                        # Flatten the buffer into a single list of points
                        gesture_points = [point for frame in buffer for point in frame]
                        result , score = recognizer.recognize(gesture_points)
                        
                        if result:
                            print(f"Recognized gesture: {result} with score {score}")
                        else:
                            print("No gesture recognized")
                        
                        buffer = []

                        try:
                            client_socket.send(f"{result}".encode('utf-8'))
                        except Exception as e:
                            print("Error sending data to C# client:", e)
                        

                    # Draw hand landmarks on the frame
                    mp_draw.draw_landmarks(
                        frame,
                        hand_landmarks,
                        mp_hands.HAND_CONNECTIONS,
                        mp_draw.DrawingSpec(color=(0, 255, 0), thickness=2, circle_radius=2),
                        mp_draw.DrawingSpec(color=(0, 0, 255), thickness=2, circle_radius=2),
                    )

            # Display the frame
            cv2.imshow("Hand Landmarks Detection", frame)

            # Exit on pressing 'q'
            if cv2.waitKey(1) & 0xFF == ord("q"):
                break

    cap.release()
    cv2.destroyAllWindows()


    # Release resources
    cap.release()
    cv2.destroyAllWindows()

# Thread function to handle socket connection and start gesture recognition
def socket_thread():
    client_socket, server_socket = start_socket_server()
    recognize_gesture(client_socket)
    client_socket.close()
    server_socket.close()
    print("Socket server closed.")

# Run the socket server in a separate thread
if __name__ == "__main__":
    thread = threading.Thread(target=socket_thread)
    thread.start()
