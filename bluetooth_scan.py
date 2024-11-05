import subprocess
import json

def get_connected_bluetooth_devices():
    # PowerShell command to get connected Bluetooth devices and output as JSON
    ps_command = (
        "Get-PnpDevice -Class Bluetooth | "
        "Where-Object {$_.Status -eq 'OK'} | "
        "Select-Object Name, Status, InstanceId | "
        "ConvertTo-Json"
    )
    
    try:
        # Execute the PowerShell command
        result = subprocess.run(
            [r"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe", "-Command", ps_command],
            capture_output=True,
            text=True,
            check=True
        )

        
        # Parse the JSON output
        devices = json.loads(result.stdout)
        
        # Handle single device (dict) vs multiple devices (list)
        if isinstance(devices, dict):
            devices = [devices]
        
        if devices:
            print("Connected Bluetooth devices:")
            for device in devices:
                name = device.get("Name", "Unknown")
                instance_id = device.get("InstanceId", "")
                
                # Attempt to extract MAC address from InstanceId
                # The format may vary; adjust the parsing as needed
                # Example InstanceId: "BTHENUM\\DEV_XXYYZZXXYYZZ_XXXXXXXXXXXX"
                try:
                    mac_part = instance_id.split("_")[1]
                    mac_addr = mac_part[:12]
                    formatted_mac = ":".join(mac_addr[i:i+2] for i in range(0, 12, 2))
                except (IndexError, ValueError):
                    formatted_mac = "N/A"
                
                print(f"Device Name: {name}, MAC Address: {formatted_mac}")
        else:
            print("No connected Bluetooth devices found.")
    
    except subprocess.CalledProcessError as e:
        print("An error occurred while executing PowerShell command:")
        print(e.stderr)

if __name__ == "__main__":
    get_connected_bluetooth_devices()
