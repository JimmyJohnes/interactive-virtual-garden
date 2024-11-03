import asyncio
from bleak import BleakScanner

async def scan_bluetooth_devices():
    print("Scanning for bluetooth devices...")
    
    Scanner = BleakScanner()
    try:
        devices = await Scanner.discover()
        
        print(f"Found {len(devices)} devices:\n")
        return devices
            
    except Exception as e:
        print(f"An error occurred: {str(e)}")

async def main():
    print("Starting Bluetooth scan...")
    await scan_bluetooth_devices()

if __name__ == "__main__":
    # Run the async function
    asyncio.run(main())
