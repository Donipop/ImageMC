using System;
using System.IO;
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
using System.Text.Json;
using Newtonsoft.Json.Linq;

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
        [DllImport("user32")]
        private static extern bool ClientToScreen(IntPtr windowHandle, ref Point screenPoint);
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
        public static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        [DllImport("user32")]
        public static extern int MoveWindow(int hwnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32")] 
        public static extern int GetWindowRect(int hwnd, ref RECT lpRect);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern int PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, IntPtr lParam);
        [DllImport("user32.dll")] public static extern int FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hWnd1, int hWnd2, string lpsz1, string lpsz2);
        [DllImport("user32.dll")] public static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        List<String> allPrcList;
        String savelabel = "0";
        IntPtr savehWnd;
        int saveint = 0;
        //부모[블루스택]의 핸들
        IntPtr gohWnd = IntPtr.Zero;
        MYITEM myitem = new MYITEM();
        List<ITEM> ScriptList = new List<ITEM>();
        string path = Directory.GetCurrentDirectory();
        public class ITEM
        {
            public string parentgroup { get; set; }//부모그룹 이름
            public string eventname { get; set; }//이벤트 이름
            public string whatevent { get; set; }//이벤트 [마우스or키보드or이미지체크]
            public string keyvalue { get; set; }//키보드 입력값[닉네임]
            public string position { get; set; }//마우스 클릭 위치, 이미지 인식위치 x,y/x,y
            public string imagepath { get; set; }//이미지 주소[path]
            public string imagepersent { get; set; }//이미지 일치율
            public string action { get; set; }//대기시간

        }
        public class MYITEM
        {
            //clearScreen(Graphics newGraphics, IntPtr hWnd, Pen redpen)
            public Graphics mygrahics { get; set; }
            public Pen mypen { get; set; }
            public float mywidth { get; set; }
            public float myheight { get; set; }
        }
        public Form1()
        {
            InitializeComponent();
            KeyboardHook.KeyDown += KeyboardHook_KeyDown;
            KeyboardHook.KeyUp += KeyboardHook_KeyUp;
            MouseHook.MouseDown += MouseHook_MouseDown;
            MouseHook.MouseUp += MouseHook_MouseUp;
            MouseHook.MouseMove += MouseHook_MouseMove;

            //KeyboardHook.HookStart();
            //MouseHook.HookStart();
            FormClosing += Form1_FormClosing;

        }

        private bool MouseHook_MouseMove(MouseEventType type, int x, int y)
        {
            try
            {
                if (timer4.Enabled == true)
                {
                    string ftext = textBox2.Text.Split('/')[0];
                    if (ftext != "")
                    {
                        
                        clearScreen(myitem.mygrahics, savehWnd, myitem.mypen);
                        textBox2.Text = ftext + "/" + getscreenMoustPosition();


                        string[] checktext = textBox2.Text.Split('/');

                        float wi = float.Parse(checktext[1].Split(',')[0]) - float.Parse(checktext[0].Split(',')[0]);
                        float he = float.Parse(checktext[1].Split(',')[1]) - float.Parse(checktext[0].Split(',')[1]);
                        myitem.mywidth = wi;
                        myitem.myheight = he;

                        printScreenRedMouse(savehWnd);
                        getScreen(savehWnd);

                    }

                }
                return true;
            }
            catch
            {
                return true;
            }

            return true;
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
            //MouseHook.HookEnd();
            if(timer4.Enabled == true)
            {
                clearScreen(myitem.mygrahics, savehWnd, myitem.mypen);
                timer4.Enabled = false;
                Console.WriteLine("타이머4 종료");
                MouseHook.HookEnd();
            }
            return true;
        }

        private bool MouseHook_MouseDown(MouseEventType type, int x, int y)
        {
            if(timer1.Enabled == true)
            {
                savelabel = "2";
                Console.WriteLine("크기조절");
                //크기조절
                RECT rc = new RECT();
                GetClientRect((IntPtr)gohWnd, ref rc);
                MoveWindow(gohWnd.ToInt32(), rc.left, rc.top, 674, 394, true);
                MouseHook.HookEnd();
                timer1.Enabled = false;
            }
            
            if(timer3.Enabled == true)
            {
                timer3.Enabled = false;
                MouseHook.HookEnd();
            }

            if(timer4.Enabled == true)
            {
                textBox2.Text = getscreenMoustPosition();
            }

            Console.WriteLine("마우스 다운");
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
        private void printScreenRedMouse(IntPtr hWnd)
        {
            try
            {
                using (Graphics newGraphics = Graphics.FromHwndInternal(hWnd))
                {
                    Rectangle rect = Rectangle.Round(newGraphics.VisibleClipBounds);
                    Pen redpen = new Pen(Color.Red, 1);
                    RECT rc = new RECT();

                    

                    string[] checktext = textBox2.Text.Split('/');

                    float wi = myitem.mywidth;
                    float he = myitem.myheight;

                    if (checktext[0] != null)
                    {
                        newGraphics.DrawRectangle(redpen, float.Parse(checktext[0].Split(',')[0]) -2, float.Parse(checktext[0].Split(',')[1]) -2, wi, he);
                        myitem.mygrahics = newGraphics;
                        myitem.mypen = redpen;
                        newGraphics.Dispose();
                        redpen.Dispose();
                    }
                }
            }
            catch
            {

            }
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
                        //
                        //Console.WriteLine(rc.left + "/" + rc.right);
                        GetWindowRect(hWnd.ToInt32(), ref rc);
      
                    }

                }
            }
            catch
            {

            }
            

                
        }
        private void clearScreen(Graphics newGraphics, IntPtr hWnd, Pen redpen)
        {
            try
            {
                IntPtr aa = savehWnd;

                //바뀔때마다 실행
                InvalidateRect(aa, (IntPtr)null, true);
                UpdateWindow(aa);

                savelabel = "1";
                uint pid;
                GetWindowThreadProcessId(hWnd,out pid);
                Process localById = Process.GetProcessById(Convert.ToInt32(pid));
                gohWnd = GetParent(hWnd);
                label2.Text = hWnd.ToString() + "/" + gohWnd + "/" + localById.MainWindowTitle;
                savehWnd = hWnd;
                //Console.WriteLine("세이브 핸들");

            }
            catch
            {

            }
            
        }
        public Bitmap cropAtRect(Bitmap orgImg, Rectangle sRect)
        {
            Rectangle destRect = new Rectangle(Point.Empty, sRect.Size);

            var cropImage = new Bitmap(destRect.Width, destRect.Height);
            using (var graphics = Graphics.FromImage(cropImage))
            {
                graphics.DrawImage(orgImg, destRect, sRect, GraphicsUnit.Pixel);
            }
            return cropImage;
        }//https://mangveloper.com/15

        private void getScreen(IntPtr hWnd)
        {
            Graphics graphics = Graphics.FromHwnd(hWnd);
            
            Rectangle rect = Rectangle.Round(graphics.VisibleClipBounds);
            //Bitmap bmp = new Bitmap(rect.Width, rect.Height);
            Bitmap bitmap = new Bitmap(rect.Width,rect.Height);

            string[] checktext = textBox2.Text.Split('/');

            Rectangle rect2 = new Rectangle(Convert.ToInt32(checktext[0].Split(',')[0]), Convert.ToInt32(checktext[0].Split(',')[1]), Convert.ToInt32(myitem.mywidth), Convert.ToInt32(myitem.myheight));
            
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                IntPtr hdc = g.GetHdc();
                PrintWindow(hWnd, hdc, 0x2);
                g.ReleaseHdc(hdc);
            }
            pictureBox1.Size = new Size(Convert.ToInt32(myitem.mywidth) -2, Convert.ToInt32(myitem.myheight)-2);
            pictureBox1.Image = cropAtRect(bitmap, rect2);


        }
        private void getAllPrc()
        {
            /*Process[] processes = Process.GetProcesses();
            Process currentProcess = Process.GetCurrentProcess();
            allPrcList = new List<string>();
            foreach (Process p in processes)
            {
                //Debug.WriteLine(p.Id + " " + p.ProcessName + " " + p.MainWindowTitle+ "/" + p.MainWindowHandle);
                // + "/" + p.ProcessName.ToString() + "/" + p.MainWindowTitle.ToString()
                //allPrcList.Add(p.MainWindowHandle.ToString() + "/" + p.ProcessName + "/" + p.Id + "/" + p.MainWindowTitle );
                Console.WriteLine(p.MainWindowHandle + "/" + p.ProcessName + "/" + p.Id);
            }*/
            Process localById = Process.GetProcessById(11584);
            Console.WriteLine(localById);
            
            
        }
        private String getscreenMoustPosition()
        {
            RECT rc = new RECT();
            GetWindowRect(savehWnd.ToInt32(), ref rc);
            Rectangle rect = new Rectangle(rc.left, rc.top, rc.right, rc.bottom);


            Point p = Cursor.Position;
            if (rect.Contains(p))
            {
                return (Cursor.Position.X - rc.left).ToString() + "," + (Cursor.Position.Y - rc.top).ToString();
            }
            return null;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            GetMousePointToWindowHandle();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            treeView1.CheckBoxes = true;
            //combobox
            comboBox1.Items.Add("마우스 클릭");
            comboBox1.Items.Add("키보드 입력");
            comboBox1.Items.Add("이미지 인식");
        }
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            KeyboardHook.HookEnd();
            MouseHook.HookEnd();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int c = 0;

            for (int i = 0; i < ScriptList.Count; i++)
            {
                if (ScriptList[i].eventname == grouptext.Text)
                {
                    c = 1;
                }
            }

            if (c == 0)
            {
                TreeNode node = new TreeNode(grouptext.Text, 0, 0);
                ITEM saveitem = new ITEM();
                saveitem.eventname = grouptext.Text;
                saveitem.parentgroup = grouptext.Text;
                treeView1.Nodes.Add(node);
                ScriptList.Add(saveitem);
                treeView1.ExpandAll();
            }
            else
            {
                MessageBox.Show("이미 있는 그룹이름");
            }

            
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
  
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //treeView1.GetNodeCount(true)
            int c = 0;
            try
            {
                
                ITEM saveitem = new ITEM();
                TreeNode node = treeView1.SelectedNode;
                //Console.WriteLine(node.Text);
                saveitem.eventname = textBox1.Text;
                saveitem.position = textBox2.Text;
                saveitem.keyvalue = textBox3.Text;
                saveitem.imagepersent = textBox4.Text;
                saveitem.whatevent = comboBox1.Text;
                saveitem.imagepath = button11.Text;
                saveitem.action = richTextBox1.Text;
                saveitem.parentgroup = node.Text;

                for(int i =0; i < ScriptList.Count; i++)
                {
                    if (ScriptList[i].eventname == saveitem.eventname)
                    {
                        c = 1;
                    }
                }

                    if (c == 0)
                    {
                        node.Nodes.Add(saveitem.eventname);
                        ScriptList.Add(saveitem);
                        treeView1.ExpandAll();
                    }
                    else
                    {
                        MessageBox.Show("이미 있는 그룹이름");
                    }
            }
            catch
            {

            }
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ITEM item = new ITEM()
            {
                parentgroup = "123",
                eventname = "[1]마우스 입력",
                imagepath = "1.png"
            };
            JsonSerializerOptions jso = new JsonSerializerOptions();
            jso.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

            string strJson = JsonSerializer.Serialize(item, jso);
            Console.WriteLine(strJson);
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            textBox2.Text = getscreenMoustPosition();
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MouseHook.HookStart();
            timer3.Enabled = true;
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            if(textBox2.Text != null)
            {
                //printScreenRedMouse(savehWnd);
            }
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            MouseHook.HookStart();
            timer4.Enabled = true;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;
            node.Remove();
            for(int i =0; i<ScriptList.Count; i++)
            {
                if(node.Text == ScriptList[i].eventname)
                {
                    ScriptList.RemoveAt(i);
                }
            }
            //ScriptList.Remove();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox1.SelectedIndex.ToString() != null)
            {
                if (comboBox1.SelectedItem.ToString() == "키보드 입력")
                {
                    textBox3.Enabled = true;
                    textBox2.Enabled = false;
                    textBox4.Enabled = false;
                    button11.Enabled = false;

                }

                if (comboBox1.SelectedItem.ToString() == "마우스 클릭")
                {
                    textBox3.Enabled = false;
                    textBox2.Enabled = true;
                    textBox4.Enabled = false;
                    button11.Enabled = false;
                }

                if (comboBox1.SelectedItem.ToString() == "이미지 인식")
                {
                    textBox3.Enabled = false;
                    textBox4.Enabled = true;
                    textBox2.Enabled = true;
                    button11.Enabled = true;
                }
            }
                
        }
        private void getimage()
        {
            string[] checktext = textBox2.Text.Split('/');
            float wi = float.Parse(checktext[1].Split(',')[0]) - float.Parse(checktext[0].Split(',')[0]);
            float he = float.Parse(checktext[1].Split(',')[1]) - float.Parse(checktext[0].Split(',')[1]);
            myitem.mywidth = wi;
            myitem.myheight = he;
            getScreen(savehWnd);
        }
        private void button10_Click(object sender, EventArgs e)
        {
            /*ITEM item = new ITEM();
            item.position = "57,51";
            item.keyvalue = "abc/안녕 하세요";
            Message(eventenum.Mouse, item);
            //Message(eventenum.Keyboard, item);*/
            //getAllPrc();

            //2번째테스트
            //getimage();
            ITEM item = new ITEM();
            item.position = "57,51";
            item.keyvalue = "abc/안녕 하세요";
            Message(eventenum.Mouse, item);

            //Console.WriteLine(GetParent(savehWnd));
            //197170 - > 바로 블루스택 부모핸들 가져옴

            /*void ChildWindows(HWND parentHwnd)
            {
                HWND childHwnd;
                childHwnd = GetWindow(parentHwnd, GW_CHILD);

                while (childHwnd)
                {
                    printf("%#08x\n", childHwnd);
                    ChildWindows(childHwnd);
                    childHwnd = GetNextWindow(childHwnd, GW_HWNDNEXT);
                }

                return;
            }
        출처: https://munggeun.tistory.com/7 [뭉근 : 느긋하게 타는 불:티스토리]*/
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;

            for(int i = 0; i< ScriptList.Count; i++)
            {
                if(ScriptList[i].eventname == node.Text){

                    textBox1.Text = ScriptList[i].eventname;
                    textBox2.Text = ScriptList[i].position;
                    textBox3.Text = ScriptList[i].keyvalue;
                    textBox4.Text = ScriptList[i].imagepersent;
                    comboBox1.Text = ScriptList[i].whatevent;
                    button11.Text = ScriptList[i].imagepath;
                    richTextBox1.Text = ScriptList[i].action;
                    grouptext.Text = ScriptList[i].parentgroup;
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            
            string fileName = "";

            SaveFileDialog saveFile = new SaveFileDialog();

            // 다이얼 로그가 Open되었을 때 최초의 경로 설정
            //saveFile.InitialDirectory = @"C:";   

            // 다이얼 로그의 제목
            saveFile.Title = "스크립트 저장위치 지정";

            // 기본 확장자
            saveFile.DefaultExt = "json";

            // 파일 목록 필터링
            saveFile.Filter = "Json files(*.json)|*.json";

            // OK버튼을 눌렀을때의 동작
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                // 경로와 파일명을 fileName에 저장
                fileName = saveFile.FileName.ToString();
                //Console.WriteLine(fileName);

                //StreamWriter w;
                string strJson = "";

                JObject json = new JObject();
                json["ImageMC"] = JToken.FromObject(ScriptList);

                File.WriteAllText(fileName, json.ToString());

            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            ScriptList.Clear();

            string filename = "";
            
            OpenFileDialog openFile = new OpenFileDialog();

            openFile.DefaultExt = "json";
            openFile.Filter = "Json files(*.json)|*.json";
            if(openFile.ShowDialog() == DialogResult.OK)
            {
                filename = openFile.FileName.ToString();
                textBox5.Text = filename;

                string str = null;
                using (StreamReader sr = new StreamReader(filename))
                {
                    str = sr.ReadToEnd();
                    sr.Close();
                }
                JObject json = JObject.Parse(str);
                JToken jtoken = json["ImageMC"];
                loadJTokenFnc(jtoken);
                //Console.WriteLine(jtoken.Count());
                //Console.WriteLine(jtoken[0]["eventname"].ToString());
                //Console.WriteLine(jtoken.ToString());

            }
        }

        private void loadJTokenFnc(JToken jToken)
        {


            //Console.WriteLine("jtokencount {0}", jToken.Count());
            for (int i = 0; i < jToken.Count(); i++)
            {
                ITEM saveitem = new ITEM();

                saveitem.eventname = jToken[i]["eventname"].ToString();
                saveitem.position = jToken[i]["position"].ToString();
                saveitem.keyvalue = jToken[i]["keyvalue"].ToString();
                saveitem.imagepersent = jToken[i]["imagepersent"].ToString();
                saveitem.whatevent = jToken[i]["whatevent"].ToString();
                saveitem.imagepath = jToken[i]["imagepath"].ToString();
                saveitem.action = jToken[i]["action"].ToString();
                saveitem.parentgroup = jToken[i]["parentgroup"].ToString();

                ScriptList.Add(saveitem);
            }

            for (int j = 0; j < ScriptList.Count; j++)
            {
                //Console.WriteLine(j);
                if(ScriptList[j].parentgroup == ScriptList[j].eventname)
                {
                    //부모객체 추가
                    //Console.WriteLine("부모객체 추가 {0}", ScriptList[j].eventname);

                    TreeNode node = new TreeNode(ScriptList[j].parentgroup, 0, 0);
                    treeView1.Nodes.Add(node);
                    treeView1.ExpandAll();

                    foreach(ITEM item in ScriptList)
                    {
                        if(item.parentgroup == node.Text && item.parentgroup != item.eventname)
                        {
                            //자식객체 추가
                           // Console.WriteLine("자식객체 추가 {0}", item.eventname);
                            node.Nodes.Add(item.eventname);
                        }
                    }

                }
            }


        }

        private void comboBox2_Click(object sender, EventArgs e)
        {
            string filename = null;

            OpenFileDialog openFile = new OpenFileDialog();

            openFile.DefaultExt = "png";
            openFile.Filter = "Png files(*.png)|*.png";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                filename = openFile.FileName.ToString();

                string[] aa = filename.Split(new string[] { ".png" }, StringSplitOptions.None);

                Console.WriteLine(aa[0]);
                comboBox2.Text = aa[0].Split(new string[] { "image\\" }, StringSplitOptions.None)[1];

                var bitmap = new Bitmap(filename);

                for(int x = 0; x < bitmap.Width; x++)
                {
                    for(int y=0; y <bitmap.Height; y++)
                    {
                        Color color = bitmap.GetPixel(x, y);
                        color = Color.FromArgb(0, color);
                        bitmap.SetPixel(x, y, color);
                    }
                }

                pictureBox1.Image = bitmap;
            }
        }
        enum eventenum
        {
            Mouse,Keyboard
        }
        private void Message(eventenum eventenum,ITEM item)
        {
            
            if (eventenum == eventenum.Mouse)
            {
                Console.WriteLine("마우스 이벤트");

                POINT pt = new POINT();

                pt.x = Convert.ToInt32(item.position.Split(',')[0]);
                pt.y = Convert.ToInt32(item.position.Split(',')[1]);

                int X = pt.x;
                int Y = pt.y;
                int lparm = (Y << 16) + X;
                //액티브
                PostMessage(savehWnd, 0x0006, 1, 0);
                PostMessage(savehWnd, 0x0201, 1, lparm);//다운
                System.Threading.Thread.Sleep(100);
                PostMessage(savehWnd, 0x0202, 0, lparm);//업

            }

            if(eventenum == eventenum.Keyboard)
            {

                foreach(char key in item.keyvalue)
                {
                    PostMessage(savehWnd, 0x102, key, IntPtr.Zero);
                }
                
            }
        }
        public static int MakeLParam(int x, int y) => (y << 16) | (x & 0xFFFF);
    }

    
}
