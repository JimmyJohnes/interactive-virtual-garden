using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using TUIO;
using System.IO;
using System.Drawing.Drawing2D;
using MongoDBOperations;
using static TuioDemo;
using MongoDB.Bson;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace CSharpClient
{   
    
    public partial class Form1 : Form
    {
        public class Pot
	{
		public Pot(string path_initial,string path_dug, int x, int y, int width, int height,string position,int phase = 1,string State = "initial")
		{
			this.path_initial = path_initial;
			this.path_dug_version = path_dug;
			this.Rect = new Rectangle (x,y,width,height);
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
		public int phase;
		public int x;
		public int y;
	    public int width;
		public int height;
	}
        Stopwatch stopwatch = new Stopwatch();
        readonly Timer tt = new Timer();
        TcpClient client;
        NetworkStream stream;
        long lastTime;
        string msg = "";

        public Form1()
        {
            this.DoubleBuffered = true;
            this.tt.Interval = 16;
            this.WindowState = FormWindowState.Maximized;
            this.tt.Tick += Tt_Tick;
            InitializeComponent();
            Load += Form1_Load;
        }

        async void Tt_Tick(object sender, EventArgs e)
        {
            long currentTime = stopwatch.ElapsedMilliseconds;
            float deltaTime = (currentTime - lastTime) / 1000.0f;
            lastTime = currentTime;
            if (!await StreamAsync())
            {
                Console.WriteLine("Failed to connect to server.");
            }
            Invalidate();
        }

        void Form1_Load(object sender, EventArgs e)
        {
            stopwatch.Start();
            lastTime = stopwatch.ElapsedMilliseconds;
            tt.Start();
        }

        async Task<bool> StreamAsync()
        {
            if (!await ConnectToSocketAsync("localhost", 6000))
            {
                return false;
            }

            while (true)
            {
                msg = await ReceiveMessageAsync();
                if (msg == null)
                {
                    Console.WriteLine("Connection lost or failed to receive message.");
                    break;
                }
                if (msg == "q")
                {
                    stream.Close();
                    client.Close();
                    Console.WriteLine("Connection Terminated!");
                    break;
                }
            }
            return true;
        }

        async Task<bool> ConnectToSocketAsync(string host, int portNumber)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(host, portNumber);
                stream = client.GetStream();
                Console.WriteLine("Connection made with " + host);
                return true;
            }
            catch (SocketException e)
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
        public void drawscene(Graphics g)
        {
            int i;
            g.Clear(Color.White);
            Color cl = Color.Blue;
            SolidBrush brush = new SolidBrush(cl);
            g.Clear(Color.White);

            

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }
    }
}
