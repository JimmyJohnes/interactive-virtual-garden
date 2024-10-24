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
using static TuioDemo;

public class TuioDemo : Form , TuioListener
{ 
		public class Pot
		{
			public Pot(string path_initial,string path_dug, string path_seeded, int x, int y, int width, int height, string State = "initial")
			{
				this.path_initial = path_initial;
				this.path_dug_version = path_dug;
				this.path_seeded = path_seeded;
				this.Rect = new Rectangle (x,y,width,height);
				this.x = x;
				this.y = y;
				this.width = width;
				this.height = height;
				this.State = State;
			} 
			public string path_initial;
			public string path_dug_version;
			public Rectangle Rect;
			public string path_seeded;
			public string State;
			public string seed;
			public int WateringNo;
			public int x;
			public int y;
		    public int width;
			public int height;
		}
    private TuioClient client;
		private Dictionary<long,TuioObject> objectList;
		private Dictionary<long,TuioCursor> cursorList;
		private Dictionary<long,TuioBlob> blobList;
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
		Font font = new Font("Arial", 10.0f);
		SolidBrush fntBrush = new SolidBrush(Color.White);
		SolidBrush bgrBrush = new SolidBrush(Color.FromArgb(0,0,64));
		SolidBrush curBrush = new SolidBrush(Color.FromArgb(192, 0, 192));
		SolidBrush objBrush = new SolidBrush(Color.FromArgb(64, 0, 0));
		SolidBrush blbBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
		Pen curPen = new Pen(new SolidBrush(Color.Blue), 1);

		public TuioDemo(int port) {

			int imgwidth = 90;
			int imgheight = 90;
			int y = 325;
			Pot Pot1 = new Pot("POT 1.png", "H1.png", "S1.png", 50, y - 100, imgwidth + 200, imgheight + 200);
			Pots.Add(Pot1);
			Pot Pot2 = new Pot("POT 2.png", "H2.png", "S2.png", 440, y + 325, imgwidth + 200, imgheight + 200);
			Pots.Add(Pot2);
			Pot Pot3 = new Pot("POT 3.png", "H3.png", "S3.png", 1210, y + 325, imgwidth + 200, imgheight + 200);
			Pots.Add(Pot3);
			Pot Pot4 = new Pot("POT 4.png", "H4.png", "S4.png", 1550, y - 100, imgwidth + 200, imgheight + 200);
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
			displayLabel.Font = new Font("Arial", 24);
			displayLabel.TextAlign = ContentAlignment.MiddleCenter;
			this.Controls.Add(displayLabel);
			this.ClientSize = new System.Drawing.Size(width, height);
			this.Name = "TuioDemo";
			this.Text = "TuioDemo";
			
			this.Closing+=new CancelEventHandler(Form_Closing);
			this.KeyDown+=new KeyEventHandler(Form_KeyDown);

			this.SetStyle( ControlStyles.AllPaintingInWmPaint |
							ControlStyles.UserPaint |
							ControlStyles.DoubleBuffer, true);

			objectList = new Dictionary<long,TuioObject>(128);
			cursorList = new Dictionary<long,TuioCursor>(128);
			blobList   = new Dictionary<long,TuioBlob>(128);
			
			client = new TuioClient(port);
			client.addTuioListener(this);

			client.connect();
		}

		private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {

 			if ( e.KeyData == Keys.F1) {
	 			if (fullscreen == false) {

					width = screen_width;
					height = screen_height;

					window_left = this.Left;
					window_top = this.Top;

					this.FormBorderStyle = FormBorderStyle.None;
		 			this.Left = 0;
		 			this.Top = 0;
		 			this.Width = screen_width;
		 			this.Height = screen_height;

		 			fullscreen = true;
	 			} else {

					width = window_width;
					height = window_height;

		 			this.FormBorderStyle = FormBorderStyle.Sizable;
		 			this.Left = window_left;
		 			this.Top = window_top;
		 			this.Width = window_width;
		 			this.Height = window_height;

		 			fullscreen = false;
	 			}
 			} else if ( e.KeyData == Keys.Escape) {
				this.Close();

 			} else if ( e.KeyData == Keys.V ) {
 				verbose=!verbose;
 			}

 		}

		private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			client.removeTuioListener(this);

			client.disconnect();
			System.Environment.Exit(0);
		}

		public void addTuioObject(TuioObject o) {
			lock(objectList) {
				objectList.Add(o.SessionID,o);
			} if (verbose) Console.WriteLine("add obj "+o.SymbolID+" ("+o.SessionID+") "+o.X+" "+o.Y+" "+o.Angle);
		}

		public void updateTuioObject(TuioObject o) {

			if (verbose) Console.WriteLine("set obj "+o.SymbolID+" "+o.SessionID+" "+o.X+" "+o.Y+" "+o.Angle+" "+o.MotionSpeed+" "+o.RotationSpeed+" "+o.MotionAccel+" "+o.RotationAccel);
		}

		public void removeTuioObject(TuioObject o) {
			lock(objectList) {
				objectList.Remove(o.SessionID);
			}
			if (verbose) Console.WriteLine("del obj "+o.SymbolID+" ("+o.SessionID+")");
		}

		public void addTuioCursor(TuioCursor c) {
			lock(cursorList) {
				cursorList.Add(c.SessionID,c);
			}
			if (verbose) Console.WriteLine("add cur "+c.CursorID + " ("+c.SessionID+") "+c.X+" "+c.Y);
		}

		public void updateTuioCursor(TuioCursor c) {
			if (verbose) Console.WriteLine("set cur "+c.CursorID + " ("+c.SessionID+") "+c.X+" "+c.Y+" "+c.MotionSpeed+" "+c.MotionAccel);
		}

		public void removeTuioCursor(TuioCursor c) {
			lock(cursorList) {
				cursorList.Remove(c.SessionID);
			}
			if (verbose) Console.WriteLine("del cur "+c.CursorID + " ("+c.SessionID+")");
 		}

		public void addTuioBlob(TuioBlob b) {
			lock(blobList) {
				blobList.Add(b.SessionID,b);
			}
			if (verbose) Console.WriteLine("add blb "+b.BlobID + " ("+b.SessionID+") "+b.X+" "+b.Y+" "+b.Angle+" "+b.Width+" "+b.Height+" "+b.Area);
		}

		public void updateTuioBlob(TuioBlob b) {
		
			if (verbose) Console.WriteLine("set blb "+b.BlobID + " ("+b.SessionID+") "+b.X+" "+b.Y+" "+b.Angle+" "+b.Width+" "+b.Height+" "+b.Area+" "+b.MotionSpeed+" "+b.RotationSpeed+" "+b.MotionAccel+" "+b.RotationAccel);
		}

		public void removeTuioBlob(TuioBlob b) {
			lock(blobList) {
				blobList.Remove(b.SessionID);
			}
			if (verbose) Console.WriteLine("del blb "+b.BlobID + " ("+b.SessionID+")");
		}

		public void refresh(TuioTime frameTime) {
			Invalidate();
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			
			// Getting the graphics object
			Bitmap small_shovel = new Bitmap("small_shovel.png");
			small_shovel.MakeTransparent();
			Graphics g = pevent.Graphics;
			g.DrawImage(Image.FromFile("background.jpg"), new Rectangle(new Point(0, 0), new Size(width, height)));
			 int imgwidth = 90;
			int imgheight = 90;
			int y = 325;
	        displayLabel.Text = Score.ToString();
		    Image img = Image.FromFile("PC.png");
			g.DrawImage(img, new Rectangle(-4, y + 500, 1922, imgheight + 60));
			img = Image.FromFile("PR.png");
			g.DrawImage(img, new Rectangle(1470, y + 100, imgwidth + 400, imgheight + 60));
			img = Image.FromFile("PL.png");
			g.DrawImage(img, new Rectangle(0, y + 100, imgwidth + 370, imgheight + 60));
			

			

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
                        g.DrawImage(Image.FromFile(pot.path_seeded), new Rectangle(pot.x, pot.y, pot.width, pot.height));
                        break;
                    }
				case "watered":
					{
                        g.DrawImage(Image.FromFile(pot.path_seeded), new Rectangle(pot.x, pot.y, pot.width, pot.height));

                        if (pot.WateringNo < 10 && pot.WateringNo >= 0)
                        {
                            g.DrawImage(Image.FromFile("LVL1.png"), new Rectangle(pot.x + 70, pot.y -100, pot.width/2, pot.height/2));
                        }
                        if (pot.WateringNo < 20 && pot.WateringNo >= 10)
                        {
                            g.DrawImage(Image.FromFile("LVL2.png"), new Rectangle(pot.x +70, (pot.y+30) - pot.height /2 , (pot.width+20) /2 , (pot.height+20) / 2));
                        }
                        if (pot.WateringNo < 30 && pot.WateringNo >= 20)
                        {
                            g.DrawImage(Image.FromFile("LVL3.png"), new Rectangle(pot.x-10, (pot.y - 150)- pot.height / 2, pot.width +50 , pot.height + 50));
                        }
                        if (pot.WateringNo <= 40 && pot.WateringNo >= 30)
                        {
                            g.DrawImage(Image.FromFile("LVL4.png"), new Rectangle(pot.x-35, pot.y - 200 - pot.height/2, pot.width, pot.height + 100));
                        }
                        break;					
					}

				} 
        }

			


        string objectImagePath;
			string backgroundImagePath;
            // draw the cursor path
            if (cursorList.Count > 0) {
 			 lock(cursorList) {
			 foreach (TuioCursor tcur in cursorList.Values) {
					List<TuioPoint> path = tcur.Path;
					TuioPoint current_point = path[0];

					for (int i = 0; i < path.Count; i++) {
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
								if (seedRect.IntersectsWith(pot.Rect) && pot.State == "initial")
								{
									pot.State = "dug";
									break;  
								}
							}
						}
						if (tobj.SymbolID == 1)
						{
						    Rectangle seedRect = new Rectangle(ox - size, oy - size, size / 4, size / 4);
						    foreach (var pot in Pots)
						    {
						        if (seedRect.IntersectsWith(pot.Rect) && pot.State == "dug")
						        {
						            pot.State = "seeded";
									pot.seed = "pumkin";
						            break;
						        }
						    }
						}
					    if (tobj.SymbolID == 2)
					    {
					        Rectangle seedRect = new Rectangle(ox - size, oy - size, size, size);
					        foreach (var pot in Pots)
					        {
								
					            if (seedRect.IntersectsWith(pot.Rect) && pot.State == "seeded" || pot.State == "watered")
					            {
					                pot.State = "watered";
									if (pot.WateringNo < 40){
										pot.WateringNo++;
									}
					                break;
					            }
					        }
					    }
						if (tobj.SymbolID == 3)
						{
						    Rectangle seedRect = new Rectangle(ox - size, oy - size, size, size);
						    foreach (var pot in Pots)
						    {
						        if (seedRect.IntersectsWith(pot.Rect) && pot.State == "watered" && pot.WateringNo >= 30)
						        {
						            pot.State = "initial";
									pot.WateringNo = 0;
									Score += 100;
						            break;
						        }
						    }
						}
                    switch (tobj.SymbolID)
                    {
                        case 0:
                            objectImagePath = Path.Combine(Environment.CurrentDirectory, "small_shovel.png");
							
                            //backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "bg1.jpg");
                            break;
                        case 1:
                            objectImagePath = Path.Combine(Environment.CurrentDirectory, "SEEDS.png");

                            //backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "bg2.jpg");
                            break;
                        case 2:
                            objectImagePath = Path.Combine(Environment.CurrentDirectory, "WATER.png");

							//backgroundImagePath = Path.Combine(Environment.CurrentDirectory, "bg3.jpg");
							break;
                        case 3:
                            objectImagePath = Path.Combine(Environment.CurrentDirectory, "HOE.png");

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
                            using (Bitmap objectImage = new Bitmap(objectImagePath))
                            {
                                // Save the current state of the graphics object
                                GraphicsState state = g.Save();

								objectImage.MakeTransparent();

                                // Apply transformations for rotation
                                g.TranslateTransform(ox, oy);
                                g.RotateTransform((float)(tobj.Angle / Math.PI * 180.0f));
                                g.TranslateTransform(-ox, -oy);

								// Draw the rotated object
								if (tobj.SymbolID == 1)
								{
									g.DrawImage(objectImage, new Rectangle(ox - size, oy - size, size / 4, size / 4));

								}
                                else if (tobj.SymbolID == 2)
                                {
                                    g.DrawImage(objectImage, new Rectangle(ox - size, oy - size, size + 100 , size + 100));

                                }
                                else
								{
									g.DrawImage(objectImage, new Rectangle(ox - size, oy - size, size, size));
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
					catch {

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
