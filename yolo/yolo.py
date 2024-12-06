import cam
from ultralytics import YOLO

def detect_objects(image):

    model = YOLO("best.pt")

    results = model.predict(image)
    
    objects = set()
    for item in results:  # Iterate through the results objects
        name = item.to_df()["name"][0]
        objects.add(name)
    return list(objects)

if __name__ == "__main__":
    _, img = cam.capture_image()
    detect_objects(img)
