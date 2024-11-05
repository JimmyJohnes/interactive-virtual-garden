<<<<<<< HEAD
﻿using System;
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

namespace CSharpClient
{
    public partial class Form1 : Form
    {

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
        float deltaTime;
        float hold_timer = 0;
        long error_Timer = 0;
        string error_msg = "";
        string command = "";
        string mode = "Farm";
        List<Pot> PotList = new List<Pot>();
        bool Connection_status;
        Stopwatch stopwatch = new Stopwatch();
        readonly Timer tt = new Timer();
        TcpClient client;
        NetworkStream stream;
        long lastTime;
        string msg = "";
        string last_msg = "";
        int CurrentPot = 0;
        Bitmap img;
        Rectangle src, dest;
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
            Invalidate();
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
                if(hold_timer > 0.08)
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
        void load_background()
        {

        }
        void Select( int step)
        {
            CurrentPot += step;

            // Wrap around if iterator goes out of bounds
            if (CurrentPot > 3)
            {
                CurrentPot = 0;
            }
            else if (CurrentPot < 0)
            {
                CurrentPot = 3;
            }
        }
        void Manage()
        {

            switch (mode)
            {
                case "Welcome":
                    break;
                case "Shop":

                    break;
                case "Farm":
                    switch (command)
                    {
                        case "left":
                            Select(-1);
                            break;
                        case "right":
                            Select(1);
                            break;
                        case "shop":
                        //    mode = "shop";
                            break;
                        case "harvest":
                            if (PotList[CurrentPot].State == "initial")
                            {
                                PotList[CurrentPot].State = "dug";
                            }
                            break;
                        case "seed":
                            if (PotList[CurrentPot].State == "dug")
                            {
                                PotList[CurrentPot].State = "seeded";
                            }
                            else
                            {
                                error_msg = "Cannot plant seed";
                            }
                            break;
                        case "water":
                            if (PotList[CurrentPot].State == "seeded" || PotList[CurrentPot].State == "watered")
                            {
                                PotList[CurrentPot].State = "watered";
                                if (PotList[CurrentPot].WateringNo < 40)
                                {
                                    PotList[CurrentPot].WateringNo++;
                                }
                            }
                            else {
                                error_msg = "no seed is planted";
                            }

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
            int i = 0;
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
                            src = new Rectangle(0, 0, img.Width, img.Height);
                            dest = new Rectangle(pot.x, pot.y, pot.width, pot.height);
                            g.DrawImage(img, dest, src, GraphicsUnit.Pixel);


                             break;
                        }
                    case "dug":
                        {

                            img = new Bitmap(Hovered + pot.path_dug_version);
                            src = new Rectangle(0, 0, img.Width, img.Height);
                            dest = new Rectangle(pot.x, pot.y, pot.width, pot.height);
                            g.DrawImage(img, dest, src, GraphicsUnit.Pixel);

                            break;
                        }
                    case "seeded":
                        {
                            // Determine the phase value to use
                            string phase = (pot.phase == 4 && pot.seed == "R") ? "3" : pot.phase.ToString();

                            // Draw the image with the selected phase

                            img = new Bitmap(Hovered + "P" + pot.seed + pot.position + phase + ".png");
                            src = new Rectangle(0, 0, img.Width, img.Height);
                            dest = new Rectangle(pot.x, pot.y, pot.width, pot.height);
                            g.DrawImage(img, dest, src, GraphicsUnit.Pixel);

                            break;
                        }
                    case "watered":
                        {
                            if (pot.WateringNo < 10 && pot.WateringNo >= 0)
                            {
                                pot.phase = 1;

                                img = new Bitmap(Hovered + "P" + pot.seed + pot.position + pot.phase + ".png");
                                src = new Rectangle(0, 0, img.Width, img.Height);
                                dest = new Rectangle(pot.x, pot.y, pot.width, pot.height);
                                g.DrawImage(img, dest, src, GraphicsUnit.Pixel);

                            }
                            if (pot.WateringNo < 20 && pot.WateringNo >= 10)
                            {
                                pot.phase = 2;

                                img = new Bitmap(Hovered + "P" + pot.seed + pot.position + pot.phase + ".png");
                                src = new Rectangle(0, 0, img.Width, img.Height);
                                dest = new Rectangle(pot.x, pot.y, pot.width, pot.height);
                                g.DrawImage(img, dest, src, GraphicsUnit.Pixel);

                            }
                            if (pot.WateringNo < 30 && pot.WateringNo >= 20)
                            {
                                pot.phase = 3;

                                img = new Bitmap(Hovered + "P" + pot.seed + pot.position + pot.phase + ".png");
                                src = new Rectangle(0, 0, img.Width, img.Height);
                                dest = new Rectangle(pot.x, pot.y, pot.width, pot.height);
                                g.DrawImage(img, dest, src, GraphicsUnit.Pixel);

                            }
                            if (pot.WateringNo <= 40 && pot.WateringNo >= 30)
                            {
                                pot.phase = 4;

                                img = new Bitmap(Hovered + "P" + pot.seed + pot.position + pot.phase + ".png");
                                src = new Rectangle(0, 0, img.Width, img.Height);
                                dest = new Rectangle(pot.x, pot.y, pot.width, pot.height);
                                g.DrawImage(img, dest, src, GraphicsUnit.Pixel);

                            }
                            break;
                        }

                }
            }

            
        }

        public void drawscene(Graphics g)
        {
            
            g.Clear(Color.Blue);

            img = new Bitmap("FARMCRAFT2.png");
            src = new Rectangle(0, 0, img.Width, img.Height);
            dest = new Rectangle(0, 0, this.Width, img.Height);
            g.DrawImage(img, dest, src, GraphicsUnit.Pixel);


           

            img = new Bitmap("FARM.png");
            src = new Rectangle(0, 0, img.Width, img.Height);
            dest = new Rectangle(0, 0, this.Width, img.Height);
            g.DrawImage(img, dest, src, GraphicsUnit.Pixel);


            g.DrawString(msg, new Font("Arial", 20, FontStyle.Regular), Brushes.Black, 20, 20);
            g.DrawString(hold_timer.ToString(), new Font("Arial", 20, FontStyle.Regular), Brushes.White, 20, 20);

            DrawingPots(g);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            drawscene(e.Graphics);
        }
    }
}
=======
﻿using System;
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

namespace CSharpClient
{
    public partial class Form1 : Form
    {

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
        float deltaTime;
        float hold_timer = 0;
        long error_Timer = 0;
        string error_msg = "";
        string command = "";
        string mode = "Farm";
        List<Pot> PotList = new List<Pot>();
        bool Connection_status;
        Stopwatch stopwatch = new Stopwatch();
        readonly Timer tt = new Timer();
        TcpClient client;
        NetworkStream stream;
        long lastTime;
        string msg = "";
        string last_msg = "";
        int CurrentPot = 0;
        Bitmap img;
        Rectangle src, dest;
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
            Invalidate();
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
                if(hold_timer > 0.08)
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
        void load_background()
        {

        }
        void Select( int step)
        {
            CurrentPot += step;

            // Wrap around if iterator goes out of bounds
            if (CurrentPot > 3)
            {
                CurrentPot = 0;
            }
            else if (CurrentPot < 0)
            {
                CurrentPot = 3;
            }
        }
        void Manage()
        {

            switch (mode)
            {
                case "Welcome":
                    break;
                case "Shop":

                    break;
                case "Farm":
                    switch (command)
                    {
                        case "left":
                            Select(-1);
                            break;
                        case "right":
                            Select(1);
                            break;
                        case "shop":
                        //    mode = "shop";
                            break;
                        case "harvest":
                            if (PotList[CurrentPot].State == "initial")
                            {
                                PotList[CurrentPot].State = "dug";
                            }
                            break;
                        case "seed":
                            if (PotList[CurrentPot].State == "dug")
                            {
                                PotList[CurrentPot].State = "seeded";
                            }
                            else
                            {
                                error_msg = "Cannot plant seed";
                            }
                            break;
                        case "water":
                            if (PotList[CurrentPot].State == "seeded" || PotList[CurrentPot].State == "watered")
                            {
                                PotList[CurrentPot].State = "watered";
                                if (PotList[CurrentPot].WateringNo < 40)
                                {
                                    PotList[CurrentPot].WateringNo++;
                                }
                            }
                            else {
                                error_msg = "no seed is planted";
                            }

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
            int i = 0;
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
                            src = new Rectangle(0, 0, img.Width, img.Height);
                            dest = new Rectangle(pot.x, pot.y, pot.width, pot.height);
                            g.DrawImage(img, dest, src, GraphicsUnit.Pixel);


                             break;
                        }
                    case "dug":
                        {

                            img = new Bitmap(Hovered + pot.path_dug_version);
                            src = new Rectangle(0, 0, img.Width, img.Height);
                            dest = new Rectangle(pot.x, pot.y, pot.width, pot.height);
                            g.DrawImage(img, dest, src, GraphicsUnit.Pixel);

                            break;
                        }
                    case "seeded":
                        {
                            // Determine the phase value to use
                            string phase = (pot.phase == 4 && pot.seed == "R") ? "3" : pot.phase.ToString();

                            // Draw the image with the selected phase

                            img = new Bitmap(Hovered + "P" + pot.seed + pot.position + phase + ".png");
                            src = new Rectangle(0, 0, img.Width, img.Height);
                            dest = new Rectangle(pot.x, pot.y, pot.width, pot.height);
                            g.DrawImage(img, dest, src, GraphicsUnit.Pixel);

                            break;
                        }
                    case "watered":
                        {
                            if (pot.WateringNo < 10 && pot.WateringNo >= 0)
                            {
                                pot.phase = 1;

                                img = new Bitmap(Hovered + "P" + pot.seed + pot.position + pot.phase + ".png");
                                src = new Rectangle(0, 0, img.Width, img.Height);
                                dest = new Rectangle(pot.x, pot.y, pot.width, pot.height);
                                g.DrawImage(img, dest, src, GraphicsUnit.Pixel);

                            }
                            if (pot.WateringNo < 20 && pot.WateringNo >= 10)
                            {
                                pot.phase = 2;

                                img = new Bitmap(Hovered + "P" + pot.seed + pot.position + pot.phase + ".png");
                                src = new Rectangle(0, 0, img.Width, img.Height);
                                dest = new Rectangle(pot.x, pot.y, pot.width, pot.height);
                                g.DrawImage(img, dest, src, GraphicsUnit.Pixel);

                            }
                            if (pot.WateringNo < 30 && pot.WateringNo >= 20)
                            {
                                pot.phase = 3;

                                img = new Bitmap(Hovered + "P" + pot.seed + pot.position + pot.phase + ".png");
                                src = new Rectangle(0, 0, img.Width, img.Height);
                                dest = new Rectangle(pot.x, pot.y, pot.width, pot.height);
                                g.DrawImage(img, dest, src, GraphicsUnit.Pixel);

                            }
                            if (pot.WateringNo <= 40 && pot.WateringNo >= 30)
                            {
                                pot.phase = 4;

                                img = new Bitmap(Hovered + "P" + pot.seed + pot.position + pot.phase + ".png");
                                src = new Rectangle(0, 0, img.Width, img.Height);
                                dest = new Rectangle(pot.x, pot.y, pot.width, pot.height);
                                g.DrawImage(img, dest, src, GraphicsUnit.Pixel);

                            }
                            break;
                        }

                }
            }

            
        }

        public void drawscene(Graphics g)
        {
            
            g.Clear(Color.Blue);

            img = new Bitmap("FARMCRAFT2.png");
            src = new Rectangle(0, 0, img.Width, img.Height);
            dest = new Rectangle(0, 0, this.Width, img.Height);
            g.DrawImage(img, dest, src, GraphicsUnit.Pixel);


           

            img = new Bitmap("FARM.png");
            src = new Rectangle(0, 0, img.Width, img.Height);
            dest = new Rectangle(0, 0, this.Width, img.Height);
            g.DrawImage(img, dest, src, GraphicsUnit.Pixel);


            g.DrawString(msg, new Font("Arial", 20, FontStyle.Regular), Brushes.Black, 20, 20);
            g.DrawString(hold_timer.ToString(), new Font("Arial", 20, FontStyle.Regular), Brushes.White, 20, 20);

            DrawingPots(g);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            drawscene(e.Graphics);
        }
    }
}
>>>>>>> 51086b8514d46648f605b42d3209a27eb9132964
