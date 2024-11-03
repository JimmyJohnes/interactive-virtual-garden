import socket
import asyncio
import json
from bleak_bluetooth import scan_bluetooth_devices


def convert_to_json(devices):
    device_json = {
        "devices": [

        ]
    }
    for device in devices:
        device_json["devices"].append({"name": device.name, "address":device.address})
    return device_json

def server():
    try:
        while True:
            c, addr = s.accept()	 
            if bool(addr):
                devices = asyncio.run(scan_bluetooth_devices())
                print ('Got connection from', addr )
                device_json = convert_to_json(devices)
                try:
                    c.send(json.dumps(device_json).encode()) 
                    print(f"sent {addr} current bluetooh devices")
                except Exception as e:
                    print(e)
            c.close()
    except KeyboardInterrupt:
        print("Stopped Server: KeyboardInterrupt")

if __name__ == "__main__":
    s = socket.socket()		 
    port = 3000
    print ("Socket successfully created")
    s.bind(('127.0.0.1', port))		 
    print ("socket binded to %s" %(port)) 
    s.listen(5)	 
    print ("socket is listening")		 
    try:
        server()
    except Exception as e:
        print(e)

