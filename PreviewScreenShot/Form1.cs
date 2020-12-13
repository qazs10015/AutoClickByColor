using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace PreviewScreenShot
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int LEFTUP = 0x0004;
        private const int LEFTDOWN = 0x0002;
        private Boolean isClick = false;
        public bool clickble = false;



        public Form1()
        {
            InitializeComponent();

            //this.Opacity = .5D; //Make trasparent
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true); // this is to avoid visual artifacts

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;

            //取得設定檔
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            //取得設定檔中的屬性資料
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("detectYaxis")))
            {
                txtYaxis.Text = config.AppSettings.Settings["detectYaxis"].Value;
                scrollYaxis.Value = Convert.ToInt32(config.AppSettings.Settings["detectYaxis"].Value);
            }
            else
            {
                config.AppSettings.Settings.Add("detectYaxis", "10");
                scrollYaxis.Value = 10;
            }

            config.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");

            
            backgroundWorker1.RunWorkerAsync();


        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            backgroundWorker1.Dispose();
            Environment.Exit(Environment.ExitCode);
        }
        // 滑鼠點擊事件
        private void MouseClickAction()
        {

            mouse_event(dwFlags: LEFTDOWN, dx: 0, dy: 0, cButtons: 0, dwExtraInfo: 0);
            Thread.Sleep(1);
            mouse_event(dwFlags: LEFTUP, dx: 0, dy: 0, cButtons: 0, dwExtraInfo: 0);
            Thread.Sleep(10);
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void OnPaint(PaintEventArgs e) // you can safely omit this method if you want
        {
            //e.Graphics.FillRectangle(Brushes.Green, Top);
            //e.Graphics.FillRectangle(Brushes.Green, Left);
            //e.Graphics.FillRectangle(Brushes.Green, Right);
            //e.Graphics.FillRectangle(Brushes.Green, Bottom);
        }

        // 背景執行事件
        private void BackgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (!string.IsNullOrEmpty(txtYaxis.Text))
            {
                Color color = GrabColor();

                // 勾選之後才能執行以下動作
                if (checkBox1.Checked)
                {
                    // 偵測綠區，如果有偵測到時就滑鼠點擊並睡5秒
                    if ((color.R > 160 && color.G > 190 && color.B > 90 && color.B < 150))
                    {

                        Console.WriteLine("detect");
                        Console.WriteLine(color.Name);
                        MouseClickAction();
                        isClick = true;
                        Thread.Sleep(5000);

                    }
                    // 當isClick設為true，就會執行滑鼠點擊功能
                    if (isClick)
                    {
                        Console.WriteLine("restart");
                        MouseClickAction();
                        isClick = false;
                    }
                }
                else
                {
                    isClick = false;
                }

                Thread.Sleep(5);
            }
        }
        
        // 取得顏色
        Color GrabColor()
        {
            Pen pen = new Pen(Color.Red, 0.5f);
            Bitmap bmp = new Bitmap(panel1.Width, panel1.Height);
            Graphics graphics = Graphics.FromImage(bmp);
            graphics.Clear(Color.Gray);
            graphics.CopyFromScreen(
                panel1.RectangleToScreen(panel1.ClientRectangle).Location,
                Point.Empty,
                panel1.Size);

            Graphics focus = panel1.CreateGraphics();
            focus.Clear(Color.Blue);
            focus.DrawRectangle(pen, panel1.Width / 2 - 5, scrollYaxis.Value - 5, 7, 7);

            Color color = bmp.GetPixel(panel1.Width / 2, scrollYaxis.Value);
            lblCoordinate.Text = color.ToString();
            lblColorName.Text = color.Name;
            lblCoordinate.BackColor = color;

            // panel2.BackgroundImage = bmp;



            //panel2.BackColor = pixel;
            //pictureBox1.Image = bmp;
            //pictureBox1.Update();
            //pictureBox2.BackColor = pixel;
            return color;
        }
    
        private void HScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            txtYaxis.Text = scrollYaxis.Value.ToString();
            Console.WriteLine(txtYaxis.Text);
        }
        // 更新設定檔的屬性資料

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            config.AppSettings.Settings["detectYaxis"].Value = txtYaxis.Text;

            config.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
