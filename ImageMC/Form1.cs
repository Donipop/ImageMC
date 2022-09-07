using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Hook;
namespace ImageMC
{
    public partial class Form1 : Form
    {
        public struct POINT
        {
            public Int32 x;
            public Int32 y;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            internal int left;
            internal int top;
            internal int right;
            internal int bottom;
        }
        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT pt);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        internal static extern bool GetClientRect(IntPtr hwnd, ref RECT lpRect);

        [DllImport("user32")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        [DllImport("user32")]
        public static extern int MoveWindow(int hwnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32")] 
        public static extern int GetWindowRect(int hwnd, ref RECT lpRect);
        

        List<String> allPrcList;
        String savelabel = "0";
        IntPtr savehWnd;
        int saveint = 0;
        public class ITEM
        {
            public string parentgroup { get; set; }//부모그룹 이름
            public string eventname { get; set; }//이벤트 이름
            public string whatevent { get; set; }//이벤트 [마우스or키보드or이미지체크]
            public string keyvalue { get; set; }//키보드 입력값[닉네임]
            public int mouseposition { get; set; }//마우스 클릭 위치
            public string imagepath { get; set; }//이미지 주소[path]
            public int imageposition { get; set; }//이미지 위치
            public int imagepersent { get; set; }//이미지 일치율
            public float waittime { get; set; }//대기시간

        }
        public Form1()
        {
            InitializeComponent();
            KeyboardHook.KeyDown += KeyboardHook_KeyDown;
            KeyboardHook.KeyUp += KeyboardHook_KeyUp;
            MouseHook.MouseDown += MouseHook_MouseDown;
            MouseHook.MouseUp += MouseHook_MouseUp;
            // MouseHook.MouseMove += MouseHook_MouseMove;

            //KeyboardHook.HookStart();
            //MouseHook.HookStart();
            FormClosing += Form1_FormClosing;

        }

        private bool KeyboardHook_KeyDown(int vkCode)
        {
            throw new NotImplementedException();
        }

        private bool KeyboardHook_KeyUp(int vkCode)
        {
            throw new NotImplementedException();
        }

        private bool MouseHook_MouseUp(MouseEventType type, int x, int y)
        {
            return true;
        }

        private bool MouseHook_MouseDown(MouseEventType type, int x, int y)
        {
            savelabel = "2";
            
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MouseHook.HookStart();
            timer1.Enabled = true;
        }

        public void GetMousePointToWindowHandle()
        {
            POINT pt;
            //RECT rect = new RECT();
            GetCursorPos(out pt);
            IntPtr hWnd = WindowFromPoint(pt);
            printScreenRed(hWnd);

        }
        private void printScreenRed(IntPtr hWnd)
        {
            try
            {
                // Create new graphics object using handle to window.
                using (Graphics newGraphics = Graphics.FromHwndInternal(hWnd))
                {
                    Rectangle rect = Rectangle.Round(newGraphics.VisibleClipBounds);
                    Pen redpen = new Pen(Color.Red, 3);
                    RECT rc = new RECT();

                
                    if(savelabel == "1")
                    {
                            newGraphics.DrawRectangle(redpen, 0, 0, rect.Width, rect.Height);
                            newGraphics.Dispose();
                            redpen.Dispose();
                            savelabel = "0";
                    }
                    if (label2.Text != hWnd.ToString())
                    {
                            clearScreen(newGraphics, hWnd, redpen);
                    }
                        if (savelabel == "2")
                        {
                            clearScreen(newGraphics, hWnd, redpen);
                            savelabel = "0";
                            MouseHook.HookEnd();
                            timer1.Enabled = false;
                        //GetClientRect(hWnd, ref rc);
                        //Console.WriteLine(rc.left + "/" + rc.right);
                        GetWindowRect(hWnd.ToInt32(), ref rc);
                        MoveWindow(hWnd.ToInt32(), rc.left, rc.top, 500, 300, true);
                        }
                
                }
            }
            catch
            {

            }
            

                
        }
        private void clearScreen(Graphics newGraphics, IntPtr hWnd, Pen redpen)
        {
            IntPtr aa = savehWnd;

            //바뀔때마다 실행
            newGraphics.Dispose();
            redpen.Dispose();
            InvalidateRect(aa, (IntPtr)null, true);
            UpdateWindow(aa);

            savelabel = "1";
            label2.Text = hWnd.ToString();
            savehWnd = hWnd;
        }
        private void getScreen(IntPtr hWnd)
        {
            Graphics graphics = Graphics.FromHwnd(hWnd);
            
            Rectangle rect = Rectangle.Round(graphics.VisibleClipBounds);
            Bitmap bmp = new Bitmap(rect.Width, rect.Height);
            
            using (Graphics g = Graphics.FromImage(bmp))
            {
                IntPtr hdc = g.GetHdc();
                PrintWindow(hWnd, hdc, 0x2);
                g.ReleaseHdc(hdc);
            }
            //pictureBox1.Width = rect.Width;
            //pictureBox1.Height = rect.Height;

            this.Size = new Size(100+rect.Width,200+rect.Height);
            //pictureBox1.Image = bmp;


        }
        private void getAllPrc()
        {
            Process[] processes = Process.GetProcesses();
            allPrcList = new List<string>();
            foreach (Process p in processes)
            {
                //Debug.WriteLine(p.Id + " " + p.ProcessName + " " + p.MainWindowTitle+ "/" + p.MainWindowHandle);
                // + "/" + p.ProcessName.ToString() + "/" + p.MainWindowTitle.ToString()
                allPrcList.Add(p.MainWindowHandle.ToString() + "/" + p.ProcessName + "/" + p.Id + "/" + p.MainWindowTitle );
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            GetMousePointToWindowHandle();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            treeView1.CheckBoxes = true;
        }
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            KeyboardHook.HookEnd();
            MouseHook.HookEnd();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TreeNode node = new TreeNode(grouptext.Text, 0,0);
            //node.Nodes.Add("T1");
            treeView1.Nodes.Add(node);
            treeView1.ExpandAll();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;
            //node.Parent
            if (node != null)
            {
                Console.WriteLine();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //treeView1.GetNodeCount(true)
            TreeNode node = treeView1.SelectedNode;
            //Console.WriteLine(node.Text);
            node.Nodes.Add("자식");
            treeView1.ExpandAll();
        }
    }

    
}
