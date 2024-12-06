import cam
from ultralytics import YOLO

_ , image = cam.capture_image()

model = YOLO("best.pt")

results = model.predict(image)

for result in results:
    probs = result.probs
    if probs is not None:
        print(probs.top1)
    else:
        print(probs)
