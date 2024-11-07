
namespace MyApp
{
    public class Device
    {
        public String name { get; set; }
        public String address { get; set; }
        public Device()
        {
            this.name = "";
            this.address = "";
        }
        public Device(String name, String address)
        {
            this.name = name;
            this.address = address;
        }
    }
    public class DeviceJson
    {
        public List<Device> devices { get; set; }
        public DeviceJson()
        {
            devices = [];
        }
        public DeviceJson(List<Device> devices)
        {
            this.devices = devices;
        }
    }

}
