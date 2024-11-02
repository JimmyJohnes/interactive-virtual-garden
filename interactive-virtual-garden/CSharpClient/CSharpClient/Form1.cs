using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;

namespace CSharpClient
{
    public partial class Form1 : Form
    {
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
    }
}
