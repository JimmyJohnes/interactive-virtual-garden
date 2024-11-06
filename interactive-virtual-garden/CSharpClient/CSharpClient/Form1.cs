using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Net.Configuration;
using System.Collections.Generic;
using static CSharpClient.Form1;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using System.Security.Cryptography;
using MongoDBOperations;
using MongoDB.Bson;
using MongoDB.Driver;
using ZstdSharp.Unsafe;
using System.Text.Json;

namespace CSharpClient
{
    public partial class Form1 : Form
    {
        public class Device
        {
            public String name { get; set; }
            public String address { get; set; }
            public int score;
            public List<String> unlockables { get; set; }
            public Device()
            {
                this.name = "";
                this.address = "";
                this.score = 0;
                this.unlockables = new List<String>();
            }
            public Device(String name, String address, int score, List<String> unlockables)
            {
                this.name = name;
                this.address = address;
                this.unlockables.Add(name);
                this.score = score;
            }
        }
        public class DeviceJson
        {
            public List<Device> devices { get; set; }
            public DeviceJson(List<Device> devices)
            {
                this.devices = devices;
            }
        }

        private MongoDBHandler mongoDbOps = new MongoDBHandler("mongodb+srv://abdelrahmannader:callofdirt1@cluster0.ytujf.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0", "Vitrula-garden");
        public class Store_Items
        {
            public Rectangle Rect;
            public int x;
            public int y;
            public int width;
            public int height;
            public string location;
            public string type;
            public bool locked;
            public Store_Items(int x, int y, int width, int height, string location, string type , bool locked = true)
            {
                this.Rect = new Rectangle(x, y, width, height);
                this.x = x;
                this.y = y;
                this.width = width;
                this.location = location;
                this.height = height;
                this.type = type;
                this.locked = locked;
            }
        }
        public class Pot
        {
            public Pot(string path_initial, string path_dug, int x, int y, int width, int height, string position, long phase = 1, string State = "initial")
            {
                this.path_initial = path_initial;
                this.path_dug_version = path_dug;
                this.Rect = new Rectangle(x, y, width, height);
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
                this.State = State;
                this.position = position;
                this.phase = phase;
                this.seed = "C";
            }
            public string path_initial;
            public string path_dug_version;
            public Rectangle Rect;
            public string path_seeded;
            public string State;
            public string seed;
            public string position;
            public int WateringNo;
            public long phase;
            public int x;
            public int y;
            public int width;
            public int height;
        }
        public class Villager
        {

            public Rectangle Rect;
            public string position;
            public int x;
            public int y;
            public int width;
            public int height;
            public Villager(int x, int y, int width, int height, string position)
            {
                this.Rect = new Rectangle(x, y, width, height);
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
                this.position = position;//left or go right
            }
        }

        public List<Device> getBluetoothDevicesAndLogin()
        {
            List<Device> devices = new List<Device>();
            try
            {

                String ip = "127.0.0.1";
                int port = 3000;
                IPAddress ipAddr = IPAddress.Parse(ip);
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, port);
                Socket sender = new Socket(ipAddr.AddressFamily,
                          SocketType.Stream, ProtocolType.Tcp);

                try
                {

                    sender.Connect(localEndPoint);
                    Console.WriteLine($"Socket connected to {ip}:{port}");

                    byte[] messageSent = Encoding.ASCII.GetBytes("Test Client<EOF>");
                    int byteSent = sender.Send(messageSent);

                    byte[] messageReceived = new byte[5024];

                    int byteRecv = sender.Receive(messageReceived);

                    String Response = Encoding.ASCII.GetString(messageReceived, 0, byteRecv);
                    DeviceJson deviceJson = JsonSerializer.Deserialize<DeviceJson>(Response);
                    if (deviceJson?.devices != null)
                    {
                        foreach (Device device in deviceJson.devices)
                        {
                            Console.WriteLine("response:" + device.name + " " + device.address);
                            if (mongoDbOps.DoesAddressExist("users", device.address))
                            {
                                Console.WriteLine($"Device with address {device.address} Logged in.");
                                devices.Add(device);
                            }
                            else
                            {
                                var document = new BsonDocument
                         {
                                {"name",device.name },{ "address", device.address}
                        };
                                mongoDbOps.InsertDocument("users", document);
                                Console.WriteLine($"Device with address {device.address} inserted.");
                                Console.WriteLine($"Device with address {device.address} is not registered.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No devices found in the JSON response.");
                    }
                    Console.WriteLine(deviceJson.devices.Count);
                    Console.WriteLine("recieved");
                    foreach (Device device in deviceJson.devices)
                    {
                        Console.WriteLine($"name:{device.name}, address: {device.address}");
                    }

                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                    return devices;
                }
                catch
                {

                }

            }
            catch { }
            return devices;
        }
        public DeviceJson getBluetoothDevicesAndUploadToDatabase()
        {

            try
            {

                String ip = "127.0.0.1";
                int port = 3000;
                IPAddress ipAddr = IPAddress.Parse(ip);
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, port);

                Socket sender = new Socket(ipAddr.AddressFamily,
                          SocketType.Stream, ProtocolType.Tcp);

                try
                {

                    sender.Connect(localEndPoint);
                    Console.WriteLine($"Socket connected to {ip}:{port}");


                    byte[] messageSent = Encoding.ASCII.GetBytes("Test Client<EOF>");
                    int byteSent = sender.Send(messageSent);

                    byte[] messageReceived = new byte[1024];

                    int byteRecv = sender.Receive(messageReceived);

                    String Response = Encoding.ASCII.GetString(messageReceived, 0, byteRecv);
                    Console.WriteLine(Response);
                    DeviceJson deviceJson = JsonSerializer.Deserialize<DeviceJson>(Response);
                    if (deviceJson?.devices != null)
                    {
                        foreach (Device device in deviceJson.devices)
                        {
                            if (!mongoDbOps.DoesAddressExist("users", device.address))
                            {
                                var document = new BsonDocument
                             {
                                { "mac_address", device.address}
                            };
                                mongoDbOps.InsertDocument("users", document);
                                Console.WriteLine($"Device with address {device.address} inserted.");
                            }
                            else
                            {
                                Console.WriteLine($"Device with address {device.address} already exists. Skipping insertion.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No devices found in the JSON response.");
                    }
                    Console.WriteLine(deviceJson.devices.Count);
                    Console.WriteLine("recieved");
                    foreach (Device device in deviceJson.devices)
                    {
                        Console.WriteLine($"name:{device.name}, address: {device.address}");
                    }

                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                    return deviceJson;
                }

                catch (ArgumentNullException ane)
                {

                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                    return null;

                }

                catch (SocketException se)
                {

                    Console.WriteLine("SocketException : {0}", se.ToString());
                    return null;

                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                    return null;
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public List<Store_Items> StoreItems = new List<Store_Items>();
        List<Pot> PotList = new List<Pot>();
        Stopwatch stopwatch = new Stopwatch();
        readonly Timer tt = new Timer();
        List<Device> devices = new List<Device>();
        Villager villager;
        TcpClient client;
        NetworkStream stream;
        Rectangle src, dest;
        Bitmap img;
        int Score = 0 , Currentitem = 0, CurrentPot = 0 , Currentdevice = 0;
        float deltaTime , hold_timer = 0 , Blue_Buf = 0;
        long error_Timer = 0 , lastTime;
        string error_msg = "" , command = "" ,  mode = "Welcome";
        bool Connection_status;
        string msg = "" , last_msg = "";
        //string connectionString = "mongodb+srv://omarhani423:GcX8zgZnPP9TCHBD@cluster0.eqr9u.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
        public Form1()
        {

            this.DoubleBuffered = true;
            this.tt.Interval = 16;
            this.WindowState = FormWindowState.Maximized;
            this.tt.Tick += Tt_Tick;
            InitializeComponent();
            Load += Form1_Load;
            this.FormBorderStyle = FormBorderStyle.None;
        }

        async void Tt_Tick(object sender, EventArgs e)
        {
            long currentTime = stopwatch.ElapsedMilliseconds;
            deltaTime = (currentTime - lastTime) / 1000.0f;
            lastTime = currentTime;
            if (!await StreamAsync())
            {
                Console.WriteLine("Failed to connect to server.");
            }
            buffer();
            Manage();
            blue_buf();
            Invalidate();
        }

        void blue_buf()
        {
            if (mode == "Welcome")
            {
                if(Blue_Buf > 4) {

                    Blue_Buf = 0;
                    devices = getBluetoothDevicesAndLogin(); 
                }
                else
                {
                    Blue_Buf += deltaTime;
                }
            }
        }
        void buffer()
        {
            if(last_msg != msg)
            {
                hold_timer = 0;
            }
            else
            {
                hold_timer += deltaTime;
                if(hold_timer > 0.1)
                {
                    command = msg;
                    hold_timer = 0;
                }
            }
        }
        void menuHandler()
        {
            
        }
        void Form1_Load(object sender, EventArgs e)
        {
            Connection_status = ConnectToSocketAsync("localhost", 5000);
            stopwatch.Start();
            lastTime = stopwatch.ElapsedMilliseconds;
            tt.Start();
            load_pots();
            Load_store();
            devices = getBluetoothDevicesAndLogin();
        }
        void Load_store() {
            
            villager = new Villager(1400, 450, 201, 300, "C");
            Store_Items temp;
            temp = new Store_Items(360, 500, 111, 111, "Wseed.png", "W" , false);
            StoreItems.Add(temp);
            temp = new Store_Items(560, 500, 111, 111, "Bseed.png", "B");
            StoreItems.Add(temp);
            temp = new Store_Items(760, 500, 111, 111, "carrot.png", "C");
            StoreItems.Add(temp);
            temp = new Store_Items(960, 500, 111, 111, "potato.png", "P");
            StoreItems.Add(temp);
            temp = new Store_Items(1160, 500, 111, 111, "berry.png", "R");
            StoreItems.Add(temp);
        }
        void load_pots()
        {

            Pot temp = new Pot("S1.png", "P1.png", 6, 652, 425, 360, "L");
            PotList.Add(temp);
            temp = new Pot("S2.png", "P2.png", 548, 657, 305, 345, "LC");
            PotList.Add(temp);
            temp = new Pot("S3.png", "P3.png", 1065, 657, 305, 345, "RC");
            PotList.Add(temp);
            temp = new Pot("S4.png", "P4.png", 1495, 652, 425, 360, "R");
            PotList.Add(temp);
        }
        void Select( int step , int max , ref int iterator)
        {
            iterator += step;

            // Wrap around if iterator goes out of bounds
            if (iterator > max)
            {
                iterator = 0;
            }
            else if (iterator < 0)
            {
                iterator = max;
            }
        }
        void Manage()
        {

            switch (mode)
            {
                case "Welcome":
                    switch (command)
                    {
                        case "shop":
                            mode = "";
                            break;
                        case "left":
                            Select(-1, devices.Count, ref Currentdevice);
                                break;
                        case "right":
                            Select(1 , devices.Count, ref Currentdevice);
                            break;
                        default:
                            break;
                    }
                    break;
                case "shop":
                    switch (command)
                    {
                        case "left":
                            Select(-1, 4, ref Currentitem);
                            break;
                        case "right":
                            Select(1, 4, ref Currentitem);
                            break;
                        case "harvest":
                            StoreItems[Currentitem].locked = false;
                            break;
                        case "shop":
                            mode = "Farm";
                            break;
                        default:
                            break;
                    }
                    break;
                case "Farm":
                    switch (command)
                    {
                        case "left":
                            Select(-1 , 3 ,ref CurrentPot);
                            break;
                        case "right":
                            Select(1, 3, ref CurrentPot);
                            break;
                        case "harvest":
                            if (PotList[CurrentPot].State == "initial")
                            {
                                PotList[CurrentPot].State = "dug";
                            }
                            if (PotList[CurrentPot].State == "dug")
                            {
                                PotList[CurrentPot].State = "seeded";
                            }
                            else if (PotList[CurrentPot].State == "seeded" || PotList[CurrentPot].State == "watered")
                            {
                                PotList[CurrentPot].State = "watered";
                                if (PotList[CurrentPot].WateringNo < 4)
                                {
                                    PotList[CurrentPot].WateringNo++;
                                }
                            }
                            break;
                        case "hoe":
                            if (PotList[CurrentPot].State == "watered" && PotList[CurrentPot].WateringNo >= 3)
                            {
                                PotList[CurrentPot].State = "initial";
                                PotList[CurrentPot].WateringNo = 0;
                                PotList[CurrentPot].seed = null;
                                PotList[CurrentPot].phase = 1;
                                Score += 100;
                                break;
                            }
                            break;
                        case "shop":
                            mode = "shop";
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            command = "";
        }
        async Task<bool> StreamAsync()
        {


            msg = await ReceiveMessageAsync();
            if (msg == null)
            {
                msg = "no connection";
                Console.WriteLine("Connection lost or failed to receive message.");
                return false;
            }
            else
            {
                last_msg = msg;
            }
            if (msg == "q")
            {
                stream.Close();
                client.Close();
                Console.WriteLine("Connection Terminated!");
                return false;
            }
            
            return true;
        }
        bool ConnectToSocketAsync(string host, int portNumber)
        {
            try
            {
                client = new TcpClient(host, portNumber);
                stream = client.GetStream();
                Console.WriteLine("connection made ! with " + host);
                return true;
            }
            catch (System.Net.Sockets.SocketException e)
            {
                Console.WriteLine("Connection Failed: " + e.Message);
                return false;
            }
        }
        async Task<string> ReceiveMessageAsync()
        {
            if (stream == null)
            {
                Console.WriteLine("Stream is not initialized.");
                return null;
            }

            try
            {
                byte[] receiveBuffer = new byte[1024];
                int bytesReceived = await stream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);
                if (bytesReceived == 0) // Connection closed
                {
                    return null;
                }
                string data = Encoding.UTF8.GetString(receiveBuffer, 0, bytesReceived);
                Console.WriteLine(data);
                return data;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error receiving message: " + e.Message);
                return null;
            }
        }
        void DrawingPots(Graphics g)
        {
            int i = 0 , offset;
            string Hovered;
            foreach (var pot in PotList)
            {
                if(i == CurrentPot)
                {
                    Hovered = "H";
                }
                else
                {
                    Hovered = "";
                }
                i++;
                switch (pot.State)
                {
                    case "initial":
                        {
                            img = new Bitmap(Hovered + pot.path_initial);
                            offset = img.Height - pot.height;
                            src = new Rectangle(0, 0, img.Width, img.Height);
                            dest = new Rectangle(pot.x, pot.y - offset, pot.width, pot.height + offset);
                            g.DrawImage(img, dest, src, GraphicsUnit.Pixel);


                             break;
                        }
                    case "dug":
                        {

                            img = new Bitmap(Hovered + pot.path_dug_version);
                            offset = img.Height - pot.height;
                            src = new Rectangle(0, 0, img.Width, img.Height);
                            dest = new Rectangle(pot.x, pot.y - offset, pot.width, pot.height + offset);
                            g.DrawImage(img, dest, src, GraphicsUnit.Pixel);

                            break;
                        }
                    case "seeded":
                        {
                            // Determine the phase value to use
                            string phase = (pot.phase == 4 && pot.seed == "R") ? "3" : pot.phase.ToString();

                            // Draw the image with the selected phase

                            img = new Bitmap(Hovered + "P" + pot.seed + pot.position + phase + ".png");
                            offset = img.Height - pot.height;
                            src = new Rectangle(0, 0, img.Width, img.Height);
                            dest = new Rectangle(pot.x, pot.y - offset, pot.width, pot.height + offset);
                            g.DrawImage(img, dest, src, GraphicsUnit.Pixel);

                            break;
                        }
                    case "watered":
                        {
                            if (pot.WateringNo < 1 && pot.WateringNo >= 0)
                            {
                                pot.phase = 1;

                                img = new Bitmap(Hovered + "P" + pot.seed + pot.position + pot.phase + ".png");
                                offset = img.Height - pot.height;
                                src = new Rectangle(0, 0, img.Width, img.Height);
                                dest = new Rectangle(pot.x, pot.y - offset, pot.width, pot.height + offset);
                                g.DrawImage(img, dest, src, GraphicsUnit.Pixel);

                            }
                            if (pot.WateringNo < 2 && pot.WateringNo >= 1)
                            {
                                pot.phase = 2;

                                img = new Bitmap(Hovered + "P" + pot.seed + pot.position + pot.phase + ".png");
                                offset = img.Height - pot.height;
                                src = new Rectangle(0, 0, img.Width, img.Height);
                                dest = new Rectangle(pot.x, pot.y - offset, pot.width, pot.height + offset);
                                g.DrawImage(img, dest, src, GraphicsUnit.Pixel);

                            }
                            if (pot.WateringNo < 3 && pot.WateringNo >= 2)
                            {
                                pot.phase = 3;

                                img = new Bitmap(Hovered + "P" + pot.seed + pot.position + pot.phase + ".png");
                                offset = img.Height - pot.height;
                                src = new Rectangle(0, 0, img.Width, img.Height);
                                dest = new Rectangle(pot.x, pot.y - offset, pot.width, pot.height + offset);
                                g.DrawImage(img, dest, src, GraphicsUnit.Pixel);

                            }
                            if (pot.WateringNo <= 4 && pot.WateringNo >= 3)
                            {
                                pot.phase = 4;

                                img = new Bitmap(Hovered + "P" + pot.seed + pot.position + pot.phase + ".png");
                                offset = img.Height - pot.height;
                                src = new Rectangle(0, 0, img.Width, img.Height);
                                dest = new Rectangle(pot.x, pot.y - offset, pot.width, pot.height + offset);
                                g.DrawImage(img, dest, src, GraphicsUnit.Pixel);

                            }
                            break;
                        }

                }
            }

            
        }
        void DrawingItems(Graphics g)
        {
            int i = 0;
            string Hovered;
            foreach (var storeItem in StoreItems)
            {
                Hovered = (i == Currentitem) ? "H" : "";

                i++;

                img = new Bitmap(Hovered + storeItem.location);
                src = new Rectangle(0, 0, img.Width, img.Height);
                dest = new Rectangle(storeItem.x, storeItem.y, storeItem.width, storeItem.height);
                g.DrawImage(img, dest, src, GraphicsUnit.Pixel);
                
            }
        }
        public void drawscene(Graphics g)
        {
            
            g.Clear(Color.Blue);

            switch(mode)
            {
                case "Welcome":
                    img = new Bitmap("FARMCRAFT2.png");
                    src = new Rectangle(0, 0, img.Width, img.Height);
                    dest = new Rectangle(0, 0, this.Width, img.Height);
                    g.DrawImage(img, dest, src, GraphicsUnit.Pixel);


                    if (devices.Count > 0)
                    {
                        g.DrawString("Current Device: " + devices[Currentdevice].name, new Font("Arial", 20, FontStyle.Regular), Brushes.White, 200, 400);
                    }

                    break;
                case "Farm":
                    img = new Bitmap("FARM.png");
                    src = new Rectangle(0, 0, img.Width, img.Height);
                    dest = new Rectangle(0, 0, this.Width, img.Height);
                    g.DrawImage(img, dest, src, GraphicsUnit.Pixel);
                    DrawingPots(g);
                    break;
                case "shop":

                    img = new Bitmap("WALL.png");
                    src = new Rectangle(0, 0, img.Width, img.Height);
                    dest = new Rectangle(0, 0, this.Width, img.Height);
                    g.DrawImage(img, dest, src, GraphicsUnit.Pixel);


                    img = new Bitmap("Stare.png");
                    src = new Rectangle(0, 0, img.Width, img.Height);
                    dest = new Rectangle(villager.x, villager.y, villager.width, villager.height);
                    g.DrawImage(img, dest, src, GraphicsUnit.Pixel);

                    img = new Bitmap("TABLE.png");
                    src = new Rectangle(0, 0, img.Width, img.Height);
                    dest = new Rectangle(0, 550, this.Width, 530);
                    g.DrawImage(img, dest, src, GraphicsUnit.Pixel);


                    DrawingItems(g);
                    break;


            }
            g.DrawString(msg, new Font("Arial", 20, FontStyle.Regular), Brushes.White, 90, 20);
            
            g.DrawString(hold_timer.ToString(), new Font("Arial", 20, FontStyle.Regular), Brushes.White, 20, 20);


            
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            drawscene(e.Graphics);
        }
    }
}
