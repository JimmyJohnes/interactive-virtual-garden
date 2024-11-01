import pickle
from dollarpy import Template
import mediapipe as mp
import cv2
from collections import namedtuple

# Initialize MediaPipe hands
mp_hands = mp.solutions.hands
hands = mp_hands.Hands(max_num_hands=2, min_detection_confidence=0.5, min_tracking_confidence=0.5)
mp_draw = mp.solutions.drawing_utils

# Define a Point structure with stroke_id
Point = namedtuple('Point', ['x', 'y', 'id', 'stroke_id'])
points = []
templates = []

# Save and load templates with pickle
def save_templates(filename="templates.pkl"):
    with open(filename, 'wb') as f:
        pickle.dump(templates, f)
    print("Templates saved to", filename)

def load_templates(filename="templates.pkl"):
    global templates
    try:
        with open(filename, 'rb') as f:
            templates = pickle.load(f)
        print("Templates loaded from", filename)
    except FileNotFoundError:
        print("No templates found, starting with an empty list.")
        templates = []

# Function to get points from video
def getPoints(videoURL, label):
    cap = cv2.VideoCapture(videoURL)

    # Initiate holistic model
    with mp.solutions.holistic.Holistic(min_detection_confidence=0.5, min_tracking_confidence=0.5) as holistic:
        points = []
        left_hand_landmarks = []
        right_hand_landmarks = []
        stroke_id = 0  # Adding stroke_id to each point set for compatibility

        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break

            # Recolor feed and process the image
            image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            image.flags.writeable = False
            results = holistic.process(image)
            image.flags.writeable = True
            image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)

            # Draw hand landmarks on the frame for visualization
            mp.solutions.drawing_utils.draw_landmarks(image, results.right_hand_landmarks, mp.solutions.holistic.HAND_CONNECTIONS)
            mp.solutions.drawing_utils.draw_landmarks(image, results.left_hand_landmarks, mp.solutions.holistic.HAND_CONNECTIONS)

            # Extract hand landmarks as points with stroke_id
            try:
                if results.right_hand_landmarks:
                    for i in range(21):
                        right_hand_landmarks.append(Point(results.right_hand_landmarks.landmark[i].x, results.right_hand_landmarks.landmark[i].y, i, stroke_id))

                if results.left_hand_landmarks:
                    for i in range(21):
                        left_hand_landmarks.append(Point(results.left_hand_landmarks.landmark[i].x, results.left_hand_landmarks.landmark[i].y, i, stroke_id))

            except Exception as e:
                print(e)

            # Display the frame with landmarks
            cv2.imshow(label, image)
            if cv2.waitKey(10) & 0xFF == ord('q'):
                break

        # Increment stroke_id for the next gesture or video
        stroke_id += 1

    cap.release()
    cv2.destroyAllWindows()

    # Combine points from both hands for this gesture
    points = left_hand_landmarks + right_hand_landmarks
    return points

# Load existing templates
load_templates()

# Define the video and label for the gesture
vid1 = r"C:\Users\Qasem\Desktop\Interactive-virtual-garden\peace.mp4"
points1 = getPoints(vid1, "peace")
tmpl_peace = Template("peace", points1)

# Check if the template already exists before adding
if not any(tmpl.name == tmpl_peace.name for tmpl in templates):
    templates.append(tmpl_peace)
    print(f"Template '{tmpl_peace.name}' added to templates.")
else:
    print(f"Template '{tmpl_peace.name}' already exists in templates.")

# Save templates after adding new ones
save_templates()

# Confirm templates
print("Current templates:", [tmpl.name for tmpl in templates])
