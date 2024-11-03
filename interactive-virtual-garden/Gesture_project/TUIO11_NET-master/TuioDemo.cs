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

public class TuioDemo : Form 
{
    //string connectionString = "mongodb+srv://omarhani423:GcX8zgZnPP9TCHBD@cluster0.eqr9u.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
    private MongoDBHandler mongoDbOps = new MongoDBHandler("mongodb+srv://omarhani423:GcX8zgZnPP9TCHBD@cluster0.eqr9u.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0", "Vitrula-garden");
    public int scene=1;
    public Bitmap small_shovel;
	public Bitmap objectImage;
    public void AddUserMacAddress(string macAddress,string name)
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
		public List<Pot> Pots = new List<Pot>();
		public static int width, height;
		private int window_width =  1920;
		private int window_height = 1080;
		private int window_left = 0;
		private int window_top = 0;
		private int screen_width = Screen.PrimaryScreen.Bounds.Width;
		private int screen_height = Screen.PrimaryScreen.Bounds.Height;
		public int Score;
		private bool fullscreen;
		private bool verbose;
		private Label displayLabel;
		Font font = new Font("Minecraft", 10.0f);
		SolidBrush fntBrush = new SolidBrush(Color.White);
		SolidBrush bgrBrush = new SolidBrush(Color.FromArgb(0,0,64));
		SolidBrush curBrush = new SolidBrush(Color.FromArgb(192, 0, 192));
		SolidBrush objBrush = new SolidBrush(Color.FromArgb(64, 0, 0));
		SolidBrush blbBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
		Pen curPen = new Pen(new SolidBrush(Color.Blue), 1);
		int currentPot = 0;
		string currentSeed = "";
    public bool Intersect(Rectangle tool, Rectangle item)
	{
		if(tool.X > item.X && tool.Y > item.Y && tool.X < item.X + item.Width && tool.Y < item.Y + item.Height/5) {
			return true; 
		}
		return false;
	}
	public TuioDemo(int port) {

        //AddUserMacAddress("38:65:B2:D9:A7:DA");


			int y = 325;
			Pot Pot1 = new Pot("S1.png", "P1.png", 6, 652, this.Width+125,this.Height+60,"L");
			Pots.Add(Pot1);		
			Pot Pot2 = new Pot("S2.png", "P2.png", 548, 657,  this.Width+5, this.Height+45, "LC");
			Pots.Add(Pot2);		
			Pot Pot3 = new Pot("S3.png", "P3.png", 1065, 657, this.Width+5, this.Height+45, "RC");
			Pots.Add(Pot3);		
			Pot Pot4 = new Pot("S4.png", "P4.png", 1495, 652, this.Width+125, this.Height+60, "R");
			Pots.Add(Pot4);
			 
			verbose = false;
			fullscreen = true;
			width = window_width;
			height = window_height;
			displayLabel = new Label();
			displayLabel.Text = Score.ToString();
			displayLabel.Location = new System.Drawing.Point(965, 50);
			displayLabel.AutoSize = true;
			displayLabel.MinimumSize = new System.Drawing.Size(100, 50);
			displayLabel.Font = new Font("Minecraft", 30);
			displayLabel.TextAlign = ContentAlignment.MiddleCenter;
			this.Controls.Add(displayLabel);
			this.ClientSize = new System.Drawing.Size(width, height);
			this.Name = "TuioDemo";
			this.Text = "TuioDemo";
			this.FormBorderStyle = FormBorderStyle.None;    
		    this.WindowState = FormWindowState.Maximized;   
			

            this.Closing+=new CancelEventHandler(Form_Closing);
			this.SetStyle( ControlStyles.AllPaintingInWmPaint |
							ControlStyles.UserPaint |
							ControlStyles.DoubleBuffer, true);

			

		}

		

		private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{

			System.Environment.Exit(0);
		}



		public void refresh(TuioTime frameTime) {
			Invalidate();
		}

	protected override void OnPaintBackground(PaintEventArgs pevent)
	{

		// Getting the graphics object
		if (small_shovel == null)
		{
			small_shovel = new Bitmap("SHOVEL.png");
		}
		small_shovel.MakeTransparent();
		Graphics g = pevent.Graphics;
		g.Clear(Color.White);
		if (scene == 0)
		{
			g.DrawImage(Image.FromFile("FARMCRAFT2.png"), new Rectangle(new Point(0, 0), new Size(width, height)));
		}
		else
		{
			g.DrawImage(Image.FromFile("FARM.png"), new Rectangle(new Point(0, 0), new Size(this.Width, this.Height)));

		}
		int imgwidth = 90;
		int imgheight = 90;
		int y = 325;
		displayLabel.Text = Score.ToString();




		
			switch (Pots[currentPot].State)
			{
				case "initial":
					{
					g.DrawImage(Image.FromFile(Pots[currentPot].path_initial), new Rectangle(Pots[currentPot].x, Pots[currentPot].y, Pots[currentPot].width, Pots[currentPot]	.height));
						break;
					}
				case "dug":
					{
						g.DrawImage(Image.FromFile(Pots[currentPot].path_dug_version), new Rectangle(Pots[currentPot].x, Pots[currentPot].y, Pots[currentPot].width, Pots[currentPot].height));
						break;
					}
				case "seeded":
					{
						// Determine the phase value to use
						string phase = (Pots[currentPot].phase == 4 && Pots[currentPot].seed == "R") ? "3" : Pots[currentPot].phase.ToString();

						// Draw the image with the selected phase
						g.DrawImage(Image.FromFile("P" + Pots[currentPot].seed + Pots[currentPot].position + phase + ".png"), new Rectangle(Pots[currentPot].x, Pots[currentPot].y, Pots[currentPot].width, Pots[currentPot].height));

						break;
					}
				case "watered":
					{
						if (Pots[currentPot].WateringNo < 10 && Pots[currentPot].WateringNo >= 0)
						{
							Pots[currentPot].phase = 1;
							g.DrawImage(Image.FromFile("P" + Pots[currentPot].seed + Pots[currentPot].position + Pots[currentPot].phase + ".png"), new Rectangle(Pots[currentPot].x, Pots[currentPot].y, Pots[currentPot].width, Pots[currentPot].height));
						}
						if (Pots[currentPot].WateringNo < 20 && Pots[currentPot].WateringNo >= 10)
						{
							Pots[currentPot].phase = 2;
							g.DrawImage(Image.FromFile("P" + Pots[currentPot].seed + Pots[currentPot].position + Pots[currentPot].phase + ".png"), new Rectangle(Pots[currentPot].x + 20, Pots[currentPot].y - 20, Pots[currentPot].width, Pots[currentPot].height));
						}
						if (Pots[currentPot].WateringNo < 30 && Pots[currentPot].WateringNo >= 20)
						{
							Pots[currentPot].phase = 3;
							g.DrawImage(Image.FromFile("P" + Pots[currentPot].seed + Pots[currentPot].position + Pots[currentPot].phase + ".png"), new Rectangle(Pots[currentPot].x + 20, Pots[currentPot].y - 40, Pots[currentPot].width, Pots[currentPot].height));
						}
						if (Pots[currentPot].WateringNo <= 40 && Pots[currentPot].WateringNo >= 30)
						{
							Pots[currentPot].phase = 4;
							g.DrawImage(Image.FromFile("P" + Pots[currentPot].seed + Pots[currentPot].position + Pots[currentPot].phase + ".png"), new Rectangle(Pots[currentPot].x + 20, Pots[currentPot].y - 60, Pots[currentPot].width, Pots[currentPot].height));
						}
						break;
					}

			}
		



		string objectImagePath;
		string backgroundImagePath;
		string gesture = "";
		int size = height / 6;
		if (gesture == "dig")
		{

			if (Pots[currentPot].State == "initial")
			{
				Pots[currentPot].State = "dug";
			}

		}

		if (gesture == "water")
		{
			

				if (Pots[currentPot].State == "seeded" || Pots[currentPot].State == "watered")
				{
					Pots[currentPot].State = "watered";
					if (Pots[currentPot].WateringNo < 40)
					{
                    Pots[currentPot].WateringNo++;
					}
			}
		}
		if (gesture == "")
		{
			foreach (var pot in Pots)
			{
				if (Pots[currentPot].State == "watered" && Pots[currentPot].WateringNo >= 30)
				{
					Pots[currentPot].State = "initial";
					Pots[currentPot].WateringNo = 0;
					Score += 100;
					break;
				}
			}
		}
		if (gesture == "")
		{
			
				if (Pots[currentPot].State == "dug")
				{
					Pots[currentPot].State = "seeded";
					Pots[currentPot].seed = "B";
				}
			
		}
		if (gesture == "")
		{

				if (Pots[currentPot].State == "dug")
				{
					Pots[currentPot].State = "seeded";
					Pots[currentPot].seed = "W";
					
				}
		}
		if (gesture == "")
		{

				if (Pots[currentPot].State == "dug")
				{
					Pots[currentPot].State = "seeded";
					Pots[currentPot].seed = "C";
					
				}
			
		}
		if (gesture == "")
		{

				if (Pots[currentPot].State == "dug")
				{
					Pots[currentPot].State = "seeded";
					Pots[currentPot].seed = "P";
					
				}
			
		}
		if (gesture == "S" && currentSeed == "R")
		{
			if (Pots[currentPot].State == "dug")
			{
				Pots[currentPot].State = "seeded";
				Pots[currentPot].seed = "R";		
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
