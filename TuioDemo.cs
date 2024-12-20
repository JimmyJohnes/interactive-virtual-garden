/*
	TUIO C# Demo - part of the reacTIVision project
	Copyright (c) 2005-2016 Martin Kaltenbrunner <martin@tuio.org>

	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

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
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using MongoDB.Driver;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;
using System.Security.Cryptography.X509Certificates;

public class TuioDemo : Form, TuioListener
{
	private readonly IMongoCollection<Device> _deviceCollection;

	public class Store_Items
	{
		public Rectangle Rect;
		public int x;
		public int y;
		public int width;
		public int height;
		public string selected_img;
		public string unselected_img;
		public string type;
		public Store_Items(int x, int y, int width, int height, string selected, string unselected, string type)
		{
			this.Rect = new Rectangle(x, y, width, height);
			this.x = x;
			this.y = y;
			this.width = width;
			this.selected_img = selected;
			this.unselected_img = unselected;
			this.height = height;
			this.type = type;
		}
	}	
	public class Button
	{
		public Rectangle Rect;
		public int x;
		public int y;
		public int width;
		public int height;
		public string selected_img;
		public string unselected_img;
		public string type;
		public Button(int x, int y, int width, int height, string selected, string unselected, string type="unselected")
		{
			this.Rect = new Rectangle(x, y, width, height);
			this.x = x;
			this.y = y;
			this.width = width;
			this.selected_img = selected;
			this.unselected_img = unselected;
			this.height = height;
		}
	}
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
		public DeviceJson(List<Device> devices)
		{
			this.devices = devices;
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

                byte[] messageReceived = new byte[1024];

                int byteRecv = sender.Receive(messageReceived);

                String Response = Encoding.ASCII.GetString(messageReceived, 0, byteRecv);
                Console.WriteLine(Response);
                DeviceJson deviceJson = JsonSerializer.Deserialize<DeviceJson>(Response);
                if (deviceJson?.devices != null)
                {
                    foreach (Device device in deviceJson.devices)
                    {
                        if (mongoDbOps.DoesAddressExist("users", device.address))
                        {
                            Console.WriteLine($"Device with address {device.address} Logged in.");
                            devices.Add(device);
                        }
                        else
                        {
                            var document = new BsonDocument
                         {
                            { "mac_address", device.address}
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
	//string connectionString = "mongodb+srv://omarhani423:GcX8zgZnPP9TCHBD@cluster0.eqr9u.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
	private MongoDBHandler mongoDbOps = new MongoDBHandler("mongodb+srv://abdelrahmannader:callofdirt1@cluster0.ytujf.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0", "Vitrula-garden");
	public int scene = 0;

	public Bitmap small_shovel;
	public Bitmap objectImage;
	public void AddUserMacAddress(string macAddress, string name)
	{
		// Create a BsonDocument with the MAC address
		var document = new BsonDocument
		{
			{"username",name},
			{ "mac_address", macAddress}
		};

		// Call the InsertDocument method to add the document to the "users" collection
		mongoDbOps.InsertDocument("users", document);
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


	public class Pot
	{
		public Pot(string path_initial, string path_dug, int x, int y, int width, int height, string position,int min_Y, int phase = 1, string State = "initial" )
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
			this.min_Y = min_Y;
		}
		public int min_Y;
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
	private TuioClient client;
	private Dictionary<long, TuioObject> objectList;
	private Dictionary<long, TuioCursor> cursorList;
	private Dictionary<long, TuioBlob> blobList;
	/// <summary>
	/// 
	/// </summary>
	/// 
	public Bitmap img;
	public Rectangle src, dest;
	public List<Pot> Pots = new List<Pot>();
	public List<Store_Items> StoreItems = new List<Store_Items>();
	public List<Button> Buttons = new List<Button>();
    public Button START = new Button(680, 500, 606, 99, "START.png", "HSTART.png");
    public Button STORE = new Button(1170, 70, 273, 99, "STORE.png", "HSTORE.png");
    List<Device> devices = new List<Device>();
    public Button RETURN = new Button(470, 70, 273, 99, "RETURN.png", "HRETURN.png");
    public Button RETURN2 = new Button(30, 70, 273, 99, "RETURN.png", "HRETURN.png");
	/// <summary>
	/// /
	/// </summary>
    public static int width, height;
	private int window_width = 1920;
	private int window_height = 1080;
	private int window_left = 0;
	private int window_top = 0;
	private int screen_width = Screen.PrimaryScreen.Bounds.Width;
	private int screen_height = Screen.PrimaryScreen.Bounds.Height;
	public int Score;
	public string current_Item = "W";
	private bool fullscreen;
	int time = 0;
	/// <summary>
	/// /////////////////
	/// </summary>
	/// 

	public Villager V = new Villager(1400, 450, 603 / 2 - 100, 700 / 2 - 50, "C");
	/// <summary>
	/// //////////
	/// </summary>
	private bool verbose;

	private Label displayLabel;
    private Label userLabel;
	public int shownuser = 0;
    Font font = new Font("Minecraft", 9.0f);
	SolidBrush fntBrush = new SolidBrush(Color.Transparent);
	SolidBrush bgrBrush = new SolidBrush(Color.FromArgb(0, 0, 64));
	SolidBrush curBrush = new SolidBrush(Color.FromArgb(192, 0, 192));
	SolidBrush objBrush = new SolidBrush(Color.FromArgb(64, 0, 0));
	SolidBrush blbBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
	Pen curPen = new Pen(new SolidBrush(Color.Blue), 1);
	public bool Intersect(Rectangle tool, Rectangle item)
	{
		if (tool.X > item.X && tool.Y > item.Y && tool.X < item.X + item.Width && tool.Y < item.Y + item.Height / 5) {
			return true;
		}
		return false;
	}
	public bool Store_Intersect(Rectangle tool, Rectangle item)
	{
		if (tool.X > item.X && tool.Y > item.Y && tool.X < item.X + item.Width && tool.Y < item.Y + item.Height)
		{
			return true;
		}
		return false;
	}
	public TuioDemo(int port) {

		//AddUserMacAddress("38:65:B2:D9:A7:DA");


		int y = 325;

		Pot Pot1 = new Pot("S1.png", "P1.png", 6, 652, this.Width + 125, this.Height + 60, "L", this.Height + 60);
		Pots.Add(Pot1);
		Pot Pot2 = new Pot("S2.png", "P2.png", 548, 657, this.Width + 5, this.Height + 45, "LC", this.Height + 45);
		Pots.Add(Pot2);
		Pot Pot3 = new Pot("S3.png", "P3.png", 1065, 657, this.Width + 5, this.Height + 45, "RC", this.Height + 45);
		Pots.Add(Pot3);
		Pot Pot4 = new Pot("S4.png", "P4.png", 1495, 652, this.Width + 125, this.Height + 60, "R", this.Height + 60);
		Pots.Add(Pot4);
		
		//  g.DrawImage(Image.FromFile("Wseed.png"), new Rectangle(new Point(400 - 40, 500), new Size(111, 111)));
		//  g.DrawImage(Image.FromFile("Bseed.png"), new Rectangle(new Point(600 - 40, 500), new Size(111, 111)));
		//  g.DrawImage(Image.FromFile("carrot.png"), new Rectangle(new Point(800 - 40, 500), new Size(111, 111)));
		//  g.DrawImage(Image.FromFile("potato.png"), new Rectangle(new Point(1000 - 40, 500), new Size(111, 111)));
		//  g.DrawImage(Image.FromFile("berry.png"), new Rectangle(new Point(1200 - 40, 500), new Size(111, 111)));

		Store_Items Item1 = new Store_Items(360, 500, 111, 111, "Wseed.png", "HWseed.png", "W");
		Store_Items Item2 = new Store_Items(560, 500, 111, 111, "Bseed.png", "HBseed.png", "B");
		Store_Items Item3 = new Store_Items(760, 500, 111, 111, "carrot.png", "Hcarrot.png", "C");
		Store_Items Item4 = new Store_Items(960, 500, 111, 111, "potato.png", "Hpotato.png", "P");
		Store_Items Item5 = new Store_Items(1160, 500, 111, 111, "berry.png", "Hberry.png", "R");
		StoreItems.Add(Item1);
		StoreItems.Add(Item2);
		StoreItems.Add(Item3);
		StoreItems.Add(Item4);
		StoreItems.Add(Item5);
        displayLabel = new Label();
        displayLabel.Text = Score.ToString();
        displayLabel.Location = new System.Drawing.Point(1920 / 2, 895);
        displayLabel.AutoSize = true;
        displayLabel.BackColor = Color.Transparent;
        displayLabel.MinimumSize = new System.Drawing.Size(100, 50);
        displayLabel.Font = new Font("Minecraft", 65);
        displayLabel.TextAlign = ContentAlignment.MiddleCenter;
		displayLabel.Visible = false;
        this.Controls.Add(displayLabel);

        userLabel = new Label();
        userLabel.Text = Score.ToString();
        userLabel.Location = new System.Drawing.Point(1920 / 2, 895);
        userLabel.AutoSize = true;
        userLabel.BackColor = Color.Transparent;
        userLabel.MinimumSize = new System.Drawing.Size(100, 50);
        userLabel.Font = new Font("Minecraft", 65);
        userLabel.TextAlign = ContentAlignment.MiddleCenter;
        this.Controls.Add(userLabel);

        verbose = true;
		fullscreen = true;
		width = window_width;
		height = window_height;
			
		this.ClientSize = new System.Drawing.Size(width, height);
		this.Name = "TuioDemo";
		this.Text = "TuioDemo";
		this.FormBorderStyle = FormBorderStyle.None;
		this.WindowState = FormWindowState.Maximized;


		this.Closing += new CancelEventHandler(Form_Closing);
		this.SetStyle(ControlStyles.AllPaintingInWmPaint |
						ControlStyles.UserPaint |
						ControlStyles.DoubleBuffer, true);

		objectList = new Dictionary<long, TuioObject>(128);
		cursorList = new Dictionary<long, TuioCursor>(128);
		blobList = new Dictionary<long, TuioBlob>(128);

		client = new TuioClient(port);
		client.addTuioListener(this);

		client.connect();
	}



	private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
	{
		client.removeTuioListener(this);

		client.disconnect();
		System.Environment.Exit(0);
	}

	public void addTuioObject(TuioObject o)
	{
		lock (objectList)
		{
			objectList.Add(o.SessionID, o);
		} if (verbose) Console.WriteLine("add obj " + o.SymbolID + " (" + o.SessionID + ") " + o.X + " " + o.Y + " " + o.Angle);
	}

	public void updateTuioObject(TuioObject o) {

		if (verbose) Console.WriteLine("set obj " + o.SymbolID + " " + o.SessionID + " " + o.X + " " + o.Y + " " + o.Angle + " " + o.MotionSpeed + " " + o.RotationSpeed + " " + o.MotionAccel + " " + o.RotationAccel);
	}

	public void removeTuioObject(TuioObject o) {
		lock (objectList) {
			objectList.Remove(o.SessionID);
		}
		if (verbose) Console.WriteLine("del obj " + o.SymbolID + " (" + o.SessionID + ")");
	}

	public void addTuioCursor(TuioCursor c) {
		lock (cursorList) {
			cursorList.Add(c.SessionID, c);
		}
		if (verbose) Console.WriteLine("add cur " + c.CursorID + " (" + c.SessionID + ") " + c.X + " " + c.Y);
	}

	public void updateTuioCursor(TuioCursor c) {
		if (verbose) Console.WriteLine("set cur " + c.CursorID + " (" + c.SessionID + ") " + c.X + " " + c.Y + " " + c.MotionSpeed + " " + c.MotionAccel);
	}

	public void removeTuioCursor(TuioCursor c) {
		lock (cursorList) {
			cursorList.Remove(c.SessionID);
		}
		if (verbose) Console.WriteLine("del cur " + c.CursorID + " (" + c.SessionID + ")");
	}

	public void addTuioBlob(TuioBlob b) {

		lock (blobList) {
			blobList.Add(b.SessionID, b);
		}
		if (verbose) Console.WriteLine("add blb " + b.BlobID + " (" + b.SessionID + ") " + b.X + " " + b.Y + " " + b.Angle + " " + b.Width + " " + b.Height + " " + b.Area);
	}

	public void updateTuioBlob(TuioBlob b) {

		if (verbose) Console.WriteLine("set blb " + b.BlobID + " (" + b.SessionID + ") " + b.X + " " + b.Y + " " + b.Angle + " " + b.Width + " " + b.Height + " " + b.Area + " " + b.MotionSpeed + " " + b.RotationSpeed + " " + b.MotionAccel + " " + b.RotationAccel);
	}

	public void removeTuioBlob(TuioBlob b) {
		lock (blobList) {
			blobList.Remove(b.SessionID);
		}
		if (verbose) Console.WriteLine("del blb " + b.BlobID + " (" + b.SessionID + ")");
	}

	public void refresh(TuioTime frameTime) {
		Invalidate();
	}

	protected override void OnPaintBackground(PaintEventArgs pevent)
	{
        // Getting the graphics object
        //getBluetoothDevicesAndUploadToDatabase();


        time++;
		if (small_shovel == null)
		{
			small_shovel = new Bitmap("SHOVEL.png");
		}
		small_shovel.MakeTransparent();
		Graphics g = pevent.Graphics;
		g.Clear(Color.White);
		if (scene == 0)
		{
			

			devices = getBluetoothDevicesAndLogin();
			if (devices.Count > 0) { 
				userLabel.Text = devices[shownuser].name;
			}
			else
			{
				userLabel.Text = "NOOOOOO";

            }

            g.DrawImage(Image.FromFile("FARMCRAFT2.png"), new Rectangle(new Point(0, 0), new Size(width, height)));
			 // if (getBluetoothDevicesAndLogin())
			 // {
			 //     scene = 1;
			 // }


		}
		else if (scene == 1)
		{
			g.DrawImage(Image.FromFile("FARM.png"), new Rectangle(new Point(0, 0), new Size(this.Width, this.Height)));

		}
		else if (scene == 2)
		{
			g.DrawImage(Image.FromFile("WALL.png"), new Rectangle(new Point(0, 0), new Size(this.Width, this.Height)));

		}
		int imgwidth = 90;
		int imgheight = 90;
		int y = 325;
		if (scene == 1 || scene == 2)
		{            
            displayLabel.Visible = true;
		}
		else
		{
			displayLabel.Visible= false;
		}

		if(scene ==0)
		{
            if (START.type == "unselected")
            {
                g.DrawImage(Image.FromFile(START.unselected_img), new Rectangle(new Point(START.x, START.y), new Size(START.width, START.height)));
            }
            else
            {
                g.DrawImage(Image.FromFile(START.selected_img), new Rectangle(new Point(START.x, START.y), new Size(START.width, START.height)));
            }
        }
		
		else if (scene == 1)
		{
            displayLabel.Location = new System.Drawing.Point(1920 / 2 +20, 55);

            if (STORE.type == "unselected")
            {
                g.DrawImage(Image.FromFile(STORE.unselected_img), new Rectangle(new Point(STORE.x, STORE.y), new Size(STORE.width, STORE.height)));
            }
            else
            {
                g.DrawImage(Image.FromFile(STORE.selected_img), new Rectangle(new Point(STORE.x, STORE.y), new Size(STORE.width, STORE.height)));
            }

            if (RETURN.type == "unselected")
            {
                g.DrawImage(Image.FromFile(RETURN.unselected_img), new Rectangle(new Point(RETURN.x, RETURN.y), new Size(RETURN.width, RETURN.height)));
            }
            else
            {
                g.DrawImage(Image.FromFile(RETURN.selected_img), new Rectangle(new Point(RETURN.x, RETURN.y), new Size(RETURN.width, RETURN.height)));
            }
			int i= 0; 
            foreach (var pot in Pots)
			{

				switch (pot.State)
				{
	
					case "initial":
						{
							g.DrawImage(Image.FromFile(pot.path_initial), new Rectangle(pot.x, pot.y, pot.width, pot.height));
							break;
						}
					case "dug":
						{
							g.DrawImage(Image.FromFile(pot.path_dug_version), new Rectangle(pot.x, pot.y, pot.width, pot.height));
							break;
						}
					case "seeded":
						{
							// Determine the phase value to use
							string phase = (pot.phase == 4 && pot.seed == "R") ? "3" : pot.phase.ToString();

							// Draw the image with the selected phase
							g.DrawImage(Image.FromFile("P" + pot.seed + pot.position + phase + ".png"), new Rectangle(pot.x, pot.y, pot.width, pot.height));

							break;
						}
					case "watered":
						{
						
							if (pot.WateringNo < 10 && pot.WateringNo >= 0)
							{
								pot.phase = 1;
                                img = new Bitmap("P" + pot.seed + pot.position + pot.phase + ".png");
								i = img.Height - pot.min_Y;
                                src = new Rectangle(0, 0, img.Width, img.Height);
                                dest = new Rectangle(pot.x, pot.y - i, pot.width, pot.height + i);
                                g.DrawImage(img, dest, src, GraphicsUnit.Pixel);


                                
							}
							if (pot.WateringNo < 20 && pot.WateringNo >= 10)
							{
								pot.phase = 2;
                                img = new Bitmap("P" + pot.seed + pot.position + pot.phase + ".png");
                                i = img.Height - pot.min_Y;
                                src = new Rectangle(0, 0, img.Width, img.Height);
                                dest = new Rectangle(pot.x, pot.y - i, pot.width, pot.height + i);
                                g.DrawImage(img, dest, src, GraphicsUnit.Pixel);
							}
							if (pot.WateringNo < 30 && pot.WateringNo >= 20)
							{
								pot.phase = 3;
                                img = new Bitmap("P" + pot.seed + pot.position + pot.phase + ".png");
                                i = img.Height - pot.min_Y;
                                src = new Rectangle(0, 0, img.Width, img.Height);
                                dest = new Rectangle(pot.x, pot.y - i, pot.width, pot.height + i);
                                g.DrawImage(img, dest, src, GraphicsUnit.Pixel);
							}
							if (pot.WateringNo <= 40 && pot.WateringNo >= 30)
							{
								pot.phase = 4;
                                img = new Bitmap("P" + pot.seed + pot.position + pot.phase + ".png");
                                i = img.Height - pot.min_Y;
                                src = new Rectangle(0, 0, img.Width, img.Height);
                                dest = new Rectangle(pot.x, pot.y - i, pot.width, pot.height + i);
                                g.DrawImage(img, dest, src, GraphicsUnit.Pixel);
							}
							break;
						}
				}
			}
		}
		else if (scene == 2)
		{

            displayLabel.Location = new System.Drawing.Point(1920 / 2, 895);

            foreach (var storeItem in StoreItems)
			{
				if(storeItem.type == current_Item)
				{
                    g.DrawImage(Image.FromFile(storeItem.unselected_img), new Rectangle(new Point(storeItem.x, storeItem.y), new Size(storeItem.width, storeItem.height)));
                }
				else
				{
                    g.DrawImage(Image.FromFile(storeItem.selected_img), new Rectangle(new Point(storeItem.x, storeItem.y), new Size(storeItem.width, storeItem.height)));
                }
            }

			g.DrawImage(Image.FromFile("Stare.png"), new Rectangle(new Point(V.x, V.y), new Size(V.width, V.height)));//VILLAGER

			g.DrawImage(Image.FromFile("TABLE.png"), new Rectangle(new Point(0, 1080 - 530), new Size(this.Width, 530)));

            if (RETURN2.type == "unselected")
            {
                g.DrawImage(Image.FromFile(RETURN2.unselected_img), new Rectangle(new Point(RETURN2.x, RETURN2.y), new Size(RETURN2.width, RETURN2.height)));
            }
            else
            {
                g.DrawImage(Image.FromFile(RETURN2.selected_img), new Rectangle(new Point(RETURN2.x, RETURN2.y), new Size(RETURN2.width, RETURN2.height)));
            }

            if (objectList.Count > 0)
			{
				lock (objectList)
				{
					foreach (TuioObject tobj in objectList.Values)
					{
						int ox = tobj.getScreenX(width);
						int oy = tobj.getScreenY(height);
						int size = height / 6;
						if (tobj.SymbolID == 0)
						{
						}
					
					}

				}
			}
		}

        string objectImagePath;
			string backgroundImagePath;
            // draw the cursor path
            if (cursorList.Count > 0)
			{
 				lock(cursorList) 
				{
					foreach (TuioCursor tcur in cursorList.Values) 
					{
					List<TuioPoint> path = tcur.Path;
					TuioPoint current_point = path[0];

					for (int i = 0; i < path.Count; i++) 
					{
						TuioPoint next_point = path[i];
						g.DrawLine(curPen, current_point.getScreenX(width), current_point.getScreenY(height), next_point.getScreenX(width), next_point.getScreenY(height));
						current_point = next_point;
					}
					g.FillEllipse(curBrush, current_point.getScreenX(width) - height / 100, current_point.getScreenY(height) - height / 100, height / 50, height / 50);
					g.DrawString(tcur.CursorID + "", font, fntBrush, new PointF(tcur.getScreenX(width) - 10, tcur.getScreenY(height) - 10));
					}
				}
			}

			// draw the objects
			if (objectList.Count > 0) {
 				lock(objectList) {
					foreach (TuioObject tobj in objectList.Values) {
						int ox = tobj.getScreenX(width);
						int oy = tobj.getScreenY(height);
						int size = height / 6;
						if (tobj.SymbolID == 0)
						{
							Rectangle seedRect = new Rectangle(ox - size / 4, oy - size / 4, size, size);
							foreach (var pot in Pots)
							{
								if (Intersect(seedRect, pot.Rect)  && pot.State == "initial")
								{
									pot.State = "dug";
									break;  
								}
							}
						}
					
					    if (tobj.SymbolID == 1)
					    {
					        Rectangle seedRect = new Rectangle(ox - size, oy - size, size, size);
					        foreach (var pot in Pots)
					        {
								
					            if (Intersect(seedRect,pot.Rect) && pot.State == "seeded" || pot.State == "watered")
					            {
					                pot.State = "watered";
									if (pot.WateringNo < 40){
										pot.WateringNo++;
									}
					                break;
					            }
					        }
					    }
						if (tobj.SymbolID == 2) // HARVEST
						{
						    Rectangle seedRect = new Rectangle(ox - size, oy - size, size, size);
						    foreach (var pot in Pots)
						    {
						        if (Intersect(seedRect, pot.Rect) && pot.State == "watered" && pot.WateringNo >= 30)
						        {
						            pot.State = "initial";
									pot.WateringNo = 0;
									pot.seed = null;
									pot.phase = 1;
									Score += 100;
								    displayLabel.Text=Score.ToString();
						            break;
						        }
						    }
						}
						if (tobj.SymbolID == 3)
						{
							Rectangle seedRect = new Rectangle(ox - size, oy - size, size , size);
							foreach (var pot in Pots)
							{
								if (Intersect(seedRect, pot.Rect) && pot.State == "dug")
								{
									pot.State = "seeded";
									pot.seed = "B";
									break;
								}
							}
						}
						if (tobj.SymbolID == 4)
						{
							Rectangle seedRect = new Rectangle(ox - size, oy - size, size , size );
							foreach (var pot in Pots)
							{
								if (Intersect(seedRect, pot.Rect) && pot.State == "dug")
								{
									pot.State = "seeded";
									pot.seed = "W";
									break;
								}
							}
						}
						if (tobj.SymbolID == 5)
						{
							Rectangle seedRect = new Rectangle(ox - size, oy - size, size, size);
							foreach (var pot in Pots)
							{
								if (Intersect(seedRect, pot.Rect) && pot.State == "dug")
								{
									pot.State = "seeded";
									pot.seed = "C";
									break;
								}
							}
						}
						if (tobj.SymbolID == 6)
						{
							Rectangle seedRect = new Rectangle(ox - size, oy - size, size, size);
							foreach (var pot in Pots)
							{
								if (Intersect(seedRect, pot.Rect) && pot.State == "dug")
							 {
							     pot.State = "seeded";
								    pot.seed = "P";
									break;
								}
							}
						}
						if (tobj.SymbolID == 7)
						{
							Rectangle seedRect = new Rectangle(ox - size, oy - size, size, size);
							foreach (var pot in Pots)
							{
								if (Intersect(seedRect, pot.Rect) && pot.State == "dug")
								{
									pot.State = "seeded";
									pot.seed = "R";
									break;
								}
							}
						}
					if (tobj.SymbolID==8)
					{
						if (scene == 2)
						{
							if (V.x > tobj.getScreenX(Width - 200))
							{
								V.x -= 30;
							}
							if (V.x < tobj.getScreenX(Width - 450))
							{
								V.x += 30;
							}

							Rectangle ItemRECT = new Rectangle(ox - size + 300, oy - size / 2, size, size);

							foreach (var StoreItem in StoreItems)
							{
								if (Store_Intersect(ItemRECT, StoreItem.Rect))
								{
									if (tobj.AngleDegrees > 60 && tobj.AngleDegrees < 270)
									{

										current_Item = StoreItem.type;
									}
									break;
								}
							}
						}
						if (scene == 0)
						{
                            Rectangle ItemRECT = new Rectangle(ox - size /2, oy - size / 2, size, size);

                            if (Store_Intersect(ItemRECT, START.Rect))
                            {
								START.type = "selected";
                                if (tobj.AngleDegrees > 30 && tobj.AngleDegrees < 270)
                                {
									scene = 1;
                                }
                                break;
                            }
							else
							{
								START.type = "unselected";
							}

							if (tobj.SymbolID == 9)
							{
								shownuser = (shownuser + 1);
							}
                            if (tobj.SymbolID == 10)
                            {
                                shownuser = (shownuser - 1);
                            }

                        }
						if (scene == 1)
						{
                            Rectangle ItemRECT = new Rectangle(ox - size / 2, oy - size / 2, size, size);

                            if (Store_Intersect(ItemRECT, RETURN.Rect))
                            {
								RETURN.type = "selected";
                                if (tobj.AngleDegrees > 30 && tobj.AngleDegrees < 270)
                                {
									scene --;
                                }
                                break;
                            }
							else
							{
								RETURN.type = "unselected";
							}

							if (Store_Intersect(ItemRECT, STORE.Rect))
                            {
								STORE.type = "selected";
                                if (tobj.AngleDegrees > 30 && tobj.AngleDegrees < 270)
                                {
									scene = 2;
                                }
                                break;
                            }
							else
							{
								STORE.type = "unselected";
							}
                        }
                        else if (scene == 2)
						{
                            Rectangle ItemRECT = new Rectangle(ox - size / 2, oy - size / 2, size, size);

                            if (Store_Intersect(ItemRECT, RETURN2.Rect))
                            {
                                RETURN2.type = "selected";
                                if (tobj.AngleDegrees > 30 && tobj.AngleDegrees < 270)
                                {
                                    scene--;
                                }
                                break;
                            }
                            else
                            {
                                RETURN2.type = "unselected";
                            }
                        }

					
					}
                    switch (tobj.SymbolID)
                    {
                        case 0:
                            objectImagePath = Path.Combine(Environment.CurrentDirectory, "SHOVEL.png");
							
                            //backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "bg1.jpg");
                            break;
                        case 1:
                            objectImagePath = Path.Combine(Environment.CurrentDirectory, "WATER.png");

                            //backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "bg2.jpg");
                            break;
                        case 2:
                            objectImagePath = Path.Combine(Environment.CurrentDirectory, "H1.png");

							//backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "bg3.jpg");
							break;
                        case 3:
                            objectImagePath = Path.Combine(Environment.CurrentDirectory, "BEETROOT.png");

                            //backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "bg3.jpg");
                            break;
                        case 4:
                            objectImagePath = Path.Combine(Environment.CurrentDirectory, "WHEAT.png");

                            //backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "bg3.jpg");
                            break;
                        case 5:
                            objectImagePath = Path.Combine(Environment.CurrentDirectory, "carrot.png");
							
                            //backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "bg3.jpg");
                            break;
                        case 6:
                            objectImagePath = Path.Combine(Environment.CurrentDirectory, "potato.png");

                            //backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "bg3.jpg");
                            break;
                        case 7:
                            objectImagePath = Path.Combine(Environment.CurrentDirectory, "berry.png");

                            //backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "bg3.jpg");
                            break; 
						case 8:
                            objectImagePath = Path.Combine(Environment.CurrentDirectory, "EMERALD.png");

                            //backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "bg3.jpg");
                            break;

                        default:
                            // Use default rectangle for other IDs
                            g.FillRectangle(objBrush, new Rectangle(ox - size, oy - size, size, size));
                            g.DrawString(tobj.SymbolID + "", font, fntBrush, new PointF(ox - 10, oy - 10));
                            continue;
                    }

                    try
                    {
                        // Draw background image without rotation
                       

                        // Draw object image with rotation
                        if (File.Exists(objectImagePath))
                        {
                            using (objectImage = new Bitmap(objectImagePath))
                            {
                                // Save the current state of the graphics object
                                GraphicsState state = g.Save();
								objectImage.MakeTransparent();
                                // Apply transformations for rotation
                                g.TranslateTransform(ox, oy);
                                g.RotateTransform((float)(tobj.Angle / Math.PI * 180.0f));
                                g.TranslateTransform(-ox, -oy);

								// Draw the rotated object
								if (tobj.SymbolID == 1)//WATER
								{
									g.DrawImage(objectImage, new Rectangle(ox - size, oy - size, size-25-50, size-50));

								}
                                else if (tobj.SymbolID == 2)
                                {
                                    g.DrawImage(objectImage, new Rectangle(ox - size, oy - size, size , size));

                                }
                                else if (tobj.SymbolID == 0) //SHOVEL
								{
                                    g.DrawImage(objectImage, new Rectangle(ox - size, oy - size, size - 25, size));
                                }
								else if(tobj.SymbolID == 8)
								{
                                    g.DrawImage(objectImage, new Rectangle(ox - size, oy - size, size - 90, size-70));
                                }
                                else //SEED N STUFF
								{
									g.DrawImage(objectImage, new Rectangle(ox - size, oy - size, size-100, size-100));
								}

                                // Restore the graphics state
                                g.Restore(state);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Object image not found: {objectImagePath}");
                            // Fall back to drawing a rectangle
                            g.FillRectangle(objBrush, new Rectangle(ox - size, oy - size, size, size));
                        }
						}
					catch 
					{
					}

				    }
				}
			}

			// draw the blobs
			if (blobList.Count > 0) {
				lock(blobList) {
					foreach (TuioBlob tblb in blobList.Values) {
						int bx = tblb.getScreenX(width);
						int by = tblb.getScreenY(height);
						float bw = tblb.Width*width;
						float bh = tblb.Height*height;

						g.TranslateTransform(bx, by);
						g.RotateTransform((float)(tblb.Angle / Math.PI * 180.0f));
						g.TranslateTransform(-bx, -by);

						g.FillEllipse(blbBrush, bx - bw / 2, by - bh / 2, bw, bh);

						g.TranslateTransform(bx, by);
						g.RotateTransform(-1 * (float)(tblb.Angle / Math.PI * 180.0f));
						g.TranslateTransform(-bx, -by);
						
						g.DrawString(tblb.BlobID + "", font, fntBrush, new PointF(bx, by));
					}
				}
			}

		}
    private void InitializeComponent()
    {
            this.SuspendLayout();
        // 
        // TuioDemo
        // 
			this.ClientSize = new System.Drawing.Size(441, 290);
            this.Name = "TuioDemo";
            this.Load += new System.EventHandler(this.TuioDemo_Load);
            this.ResumeLayout(false);

    }

    private void TuioDemo_Load(object sender, EventArgs e)
    {

    }

    public static void Main(String[] argv) {
	 		int port = 0;
            switch (argv.Length) {
				case 1:
					port = int.Parse(argv[0],null);
					if(port==0) goto default;
					break;
				case 0:
					port = 3333;
					break;
				default:
					Console.WriteLine("usage: mono TuioDemo [port]");
					System.Environment.Exit(0);
					break;
			}
			
			TuioDemo app = new TuioDemo(port);
			Application.Run(app);
		}
	}
