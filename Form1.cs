

using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace MonitorSun
{
    public partial class Form1 : Form
    {

        byte[] bLevels; //array of valid level values
        string[] arguments;

        public Form1(string[] args)
        {
            float opval = 1f;
            imgShades = ImageUtils.ImageTransparency.ChangeOpacity(Properties.Resources.Shades, opval);

            initialSize = this.Size;
            try
            {

                if (args.Length > 0)
                {
                    byte level = byte.Parse(args[0]);
                    SetBrightness(level);

                }

                arguments = args;
                InitializeComponent();

            }

            catch (Exception ex)
            {
                MessageBox.Show("1Brightness control is only supported on Laptops", "form1 Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private static void RunAtStartup()
        {
            RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            reg.SetValue(Application.ProductName, Application.ExecutablePath);

            //reg.DeleteValue(Application.ProductName);

        }

        //In case of an incompatible system, the form has to be shown in order to close the app...as far as I know ^^
        private void Form1_Shown(object sender, EventArgs e)
        {
            try
            {
                bLevels = GetBrightnessLevels(); //get the level array for this system
                if (bLevels.Count() == 0) //"WmiMonitorBrightness" is not supported by the system
                {
                    Application.Exit();
                }
                else
                {
                    trackBar1.TickFrequency = bLevels.Count(); //adjust the trackbar ticks according the number of possible brightness levels
                    trackBar1.Maximum = bLevels.Count() - 1;
                    trackBar1.Update();
                    trackBar1.Refresh();
                    check_brightness();

                    //check the arguments
                    if (Array.FindIndex(arguments, item => item.Contains("%")) > -1)
                        startup_brightness();
                    //if (arguments.Length == 0 || Array.IndexOf(arguments, "hide")>-1) //hide the trackbar initially if no arguments are passed
                    //  this.Hide();

                }

                notifyIcon1.ShowBalloonTip(3000, "Monitor Brightness", "Use the slider to adjust monitor brightness", System.Windows.Forms.ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                MessageBox.Show("2Brightness control is only supported on Laptops", " shown Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                Application.Exit();
            }
        }
        private void check_brightness()
        {
            try
            {
                iBrightness = GetBrightness(); //get the actual value of brightness
                int i = Array.IndexOf(bLevels, (byte)iBrightness);
                if (i < 0) i = 1;
                change_icon(iBrightness);
                trackBar1.Value = i;
            }
            catch (Exception ex)
            {
                MessageBox.Show("3Brightness control is only supported on Laptops", " check brightness Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                Application.Exit();
            }



        }

        private void startup_brightness()
        {
            try
            {
                string sPercent = arguments[Array.FindIndex(arguments, item => item.Contains("%"))];
                if (sPercent.Length > 1)
                {
                    int iPercent = Convert.ToInt16(sPercent.Split('%').ElementAt(0));
                    if (iPercent >= 0 && iPercent <= bLevels[bLevels.Count() - 1])
                    {
                        byte level = 100;
                        foreach (byte item in bLevels)
                        {
                            if (item >= iPercent)
                            {
                                level = item;
                                break;
                            }
                        }
                        SetBrightness(level);
                        check_brightness();
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("4Brightness control is only supported on Laptops", "startup bright Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        //change the icon according to brightness
        Image imgShades;
        private Size initialSize;
        private int iBrightness;

        private void change_icon(int iBrightness)
        {
            try
            {
                float opval = (float)(iBrightness) / 100;
                pbGlasses.Image = ImageUtils.ImageTransparency.ChangeOpacity(Properties.Resources.Shades, opval);


                label1.Text = iBrightness.ToString() + "%";

            }
            catch (Exception ex)
            {
                //MessageBox.Show("Details:\n" + ex.Message, "Brightness controll not supported", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            change_icon(bLevels[trackBar1.Value]);
            SetBrightness(bLevels[trackBar1.Value]);

            //change_icon(iBrightness);
            //this.Refresh();

            //check_brightness();
            //pbSun.Image = Properties.Resources.Sun;
            //pbGlasses.Image = Properties.Resources.Shades;
            //pbGlasses.Refresh();





        }

        private void RoundForm(PaintEventArgs e, int iRoundFactor)
        {

            #region Bottom Line
            Graphics g = e.Graphics;
            Color cDark = Color.FromArgb(81, 81, 81);
            Color cLight = Color.FromArgb(232, 232, 232);
            Rectangle rBottom = new Rectangle
                (0,
                this.Height - 10,
                this.Width, 10);

            //Brush brGradient = new LinearGradientBrush(rBottom, cDark, cLight, 270, true);
            //g.FillRectangle(brGradient, rBottom);
            #endregion

            #region Outline
            GraphicsPath p = new GraphicsPath();
            p.StartFigure();
            p.AddArc(new Rectangle(0, 0, iRoundFactor, iRoundFactor), 180, 90);
            p.AddLine(iRoundFactor, 0, this.Width - iRoundFactor, 0);
            p.AddArc(new Rectangle(this.Width - iRoundFactor, 0, iRoundFactor, iRoundFactor), -90, 90);
            p.AddLine(this.Width, iRoundFactor, this.Width, this.Height - iRoundFactor);
            p.AddArc(new Rectangle(this.Width - iRoundFactor, this.Height - iRoundFactor, iRoundFactor, iRoundFactor), 0, 90);
            p.AddLine(this.Width - iRoundFactor, this.Height, iRoundFactor, this.Height);
            p.AddArc(new Rectangle(0, this.Height - iRoundFactor, iRoundFactor, iRoundFactor), 90, 90);
            p.CloseFigure();

            this.Region = new Region(p);
            //e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            SolidBrush _outline = new SolidBrush(Color.FromArgb(81, 81, 81));
            System.Drawing.Pen borderPen = new System.Drawing.Pen(_outline, 2);

            e.Graphics.DrawPath(borderPen, p);

            //e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            borderPen.Dispose();
            p.Dispose();
            #endregion



            g.Dispose();


        }



        //get the actual percentage of brightness
        static int GetBrightness()
        {

            try
            {
                //define scope (namespace)
                System.Management.ManagementScope s = new System.Management.ManagementScope("root\\WMI");

                //define query
                System.Management.SelectQuery q = new System.Management.SelectQuery("WmiMonitorBrightness");

                //output current brightness
                System.Management.ManagementObjectSearcher mos = new System.Management.ManagementObjectSearcher(s, q);

                System.Management.ManagementObjectCollection moc = mos.Get();

                try
                {
                    if (mos.Get().Count < 1)
                        Application.Exit();

                }
                catch (Exception e)
                {
                    MessageBox.Show("Brightness control is only supported on Laptops", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

                    Environment.Exit(1);
                }

                //----------------------------------- unless it runs on laptop, it ends here

                RunAtStartup();

                //store result
                byte curBrightness = 0;
                foreach (System.Management.ManagementObject o in moc)
                {
                    curBrightness = (byte)o.GetPropertyValue("CurrentBrightness");
                    break; //only work on the first object
                }

                moc.Dispose();
                mos.Dispose();

                return (int)curBrightness;
            }
            catch (Exception ex)
            {
                MessageBox.Show("6Brightness control is only supported on Laptops", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return 0;
                Application.Exit();
            }
        }

        //array of valid brightness values in percent
        static byte[] GetBrightnessLevels()
        {
            //define scope (namespace)
            System.Management.ManagementScope s = new System.Management.ManagementScope("root\\WMI");

            //define query
            System.Management.SelectQuery q = new System.Management.SelectQuery("WmiMonitorBrightness");

            //output current brightness
            System.Management.ManagementObjectSearcher mos = new System.Management.ManagementObjectSearcher(s, q);
            byte[] BrightnessLevels = new byte[0];

            try
            {
                System.Management.ManagementObjectCollection moc = mos.Get();

                //store result


                foreach (System.Management.ManagementObject o in moc)
                {
                    BrightnessLevels = (byte[])o.GetPropertyValue("Level");
                    break; //only work on the first object
                }

                moc.Dispose();
                mos.Dispose();

            }
            catch (Exception ex)
            {
                MessageBox.Show("7Brightness control is only supported on Laptops", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                Application.Exit();
            }

            return BrightnessLevels;
        }

        static void SetBrightness(byte targetBrightness)
        {
            try
            {
                //define scope (namespace)
                System.Management.ManagementScope s = new System.Management.ManagementScope("root\\WMI");

                //define query
                System.Management.SelectQuery q = new System.Management.SelectQuery("WmiMonitorBrightnessMethods");

                //output current brightness
                System.Management.ManagementObjectSearcher mos = new System.Management.ManagementObjectSearcher(s, q);

                System.Management.ManagementObjectCollection moc = mos.Get();

                foreach (System.Management.ManagementObject o in moc)
                {
                    o.InvokeMethod("WmiSetBrightness", new Object[] { UInt32.MaxValue, targetBrightness }); //note the reversed order - won't work otherwise!
                    break; //only work on the first object
                }

                moc.Dispose();
                mos.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("8Brightness control is only supported on Laptops", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                Application.Exit();
            }
        }



        private void notifyIcon1_MouseMove(object sender, MouseEventArgs e)
        {
            notifyIcon1.Text = "Monitor Sun: " + GetBrightness().ToString() + "%";
        }



        #region To move form with mouse
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                                 int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        #endregion
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (this.Cursor == Cursors.Default)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            }
        }

        private void exitToolStripMenuItem1_Click_1(object sender, System.EventArgs e)
        {
            Application.Exit();
        }


        private void exitToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            Process.Start(@"https://panettonegames.com/");
        }

        private void Form1_Deactivate(object sender, System.EventArgs e)
        {
            this.Hide();
        }

        private void notifyIcon1_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Point p = new Point(MousePosition.X, MousePosition.Y);
                Rectangle r = Screen.GetBounds(p);
                //find the right position next to the icon
                if (p.X > r.Width / 2)
                {
                    if (p.X + 140 > r.Width)
                        this.Left = r.Width - this.Width;
                    else
                        this.Left = p.X - 140;
                }
                else
                    this.Left = p.X;

                if (p.Y > r.Height / 2)
                    this.Top = p.Y - this.Height;
                else
                    this.Top = p.Y;
                check_brightness();
                this.Show();
                this.Activate();

            }
            else
                contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);

        }

        private void Form1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            RoundForm(e, 20);

            change_icon(GetBrightness());
            //this.Refresh();

        }



        private void trackBar1_MouseUp_1(object sender, MouseEventArgs e)
        {
            change_icon(iBrightness);
            this.Refresh();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Size = initialSize;

        }
    }
}

