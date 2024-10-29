import pickle
from dollarpy import Template, Recognizer
import mediapipe as mp
import cv2
from collections import namedtuple

# Initialize MediaPipe hands
mp_hands = mp.solutions.hands
hands = mp_hands.Hands(max_num_hands=2, min_detection_confidence=0.5, min_tracking_confidence=0.5)
mp_draw = mp.solutions.drawing_utils

# Define a Point structure
Point = namedtuple('Point', ['x', 'y', 'id', 'stroke_id'])

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

# Main function to recognize gestures in real-time
def recognize_gesture():
    # Load saved templates
    templates = load_templates()
    
    # Initialize Recognizer with loaded templates
    recognizer = Recognizer(templates)

    # Start webcam capture
    cap = cv2.VideoCapture(0)
    with mp.solutions.holistic.Holistic(min_detection_confidence=0.5, min_tracking_confidence=0.5) as holistic:
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break

            image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            image.flags.writeable = False        
            results = holistic.process(image)
            image.flags.writeable = True   
            image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)

            # Get hand landmarks points from the current frame
            points = []
            if results.right_hand_landmarks:
                for i in range(21):
                    points.append(Point(results.right_hand_landmarks.landmark[i].x, results.right_hand_landmarks.landmark[i].y, i, 0))
            if results.left_hand_landmarks:
                for i in range(21):
                    points.append(Point(results.left_hand_landmarks.landmark[i].x, results.left_hand_landmarks.landmark[i].y, i, 0))


            if points:
                current_template = Template("CurrentGesture", points)
                match_name, score = recognizer.recognize(current_template)

                # Display the match and confidence score
                cv2.putText(image, f"Match: {match_name} (Score: {score:.2f})",
                            (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2, cv2.LINE_AA)

            # Show the live video feed with recognition result
            cv2.imshow("Hand Gesture Recognition", image)

            if cv2.waitKey(10) & 0xFF == ord('q'):
                break

    # Release resources
    cap.release()
    cv2.destroyAllWindows()

# Run the gesture recognition function
if __name__ == "__main__":
    recognize_gesture()
