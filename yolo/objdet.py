import cam
from ultralytics import YOLO

def detect_objects(image):

    model = YOLO("best.pt")

    results = model.predict(source="0",show=True)
    
    for item in results:  # Iterate through the results objects
        name = item.to_df()
        if len(name)<=0:
            return "None"
        name = name["name"][0]
        return name
    return "None"

if __name__ == "__main__":
    _, img = cam.capture_image()
    detect_objects(img)
