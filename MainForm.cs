using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using NAudio.Wave;
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfUis;
using W32 = Ephemera.Win32.Internals;
using WM = Ephemera.Win32.WindowManagement;
//using static NLab.Utils;


//public List<string> Dump()
//{
//    List<string> res = [];
//    _itemds.ForEach(itemd => res.Add(itemd.Item.ToString()));
//    return res;
//} >>>>
//public static IEnumerable<U> Map<T, U>(this IEnumerable<T> s, Func<T, U> f)
//{
//    foreach (var item in s)
//        yield return f(item);
//}


namespace NLab
{
    public partial class MainForm : Form
    {
        /// <summary>Handle to the LL key hook.</summary>
        readonly IntPtr _hHook1 = IntPtr.Zero;

        //[TypeConverter(typeof(ExpandableObjectConverter))]
        public HotKey HotKey { get; set; } = new();

        public MainForm(string[] args)
        {
            InitializeComponent();

            Move += (sender, e) => { Text = $"L:{Left} T:{Top} W:{Width} H:{Height}"; };

            BtnAsync.Click += AsyncClick;
            //BtnTasks.Click += TasksClick;
            BtnTracer.Click += TracerClick;
            //BtnJumplist.Click += JumplistClick;
            //BtnTray.Click += TrayClick;

            //// LL keyboard hook. from WinClip
            //using Process process = Process.GetCurrentProcess();
            //IntPtr hModule = W32.GetModuleHandle(process.MainModule!.ModuleName!);
            //_hHook2 = W32.SetWindowsHookEx(W32.WH_KEYBOARD_LL, KeyboardHookProc, hModule, 0);


            ///// Shell handlers for keys.
            _hHook1 = W32.RegisterShellHook(Handle);
            W32.RegisterHotKey(Handle, (int)Keys.Z, W32.MOD_ALT | W32.MOD_CTRL);
            W32.RegisterHotKey(Handle, (int)Keys.B, W32.MOD_CTRL);
        }

        protected override void OnLoad(EventArgs e)
        {
            BackColor = Color.Pink;

            //// Add items to lv.
            //for (int i = 0; i < 10; i++)
            //{
            //    listView1.Items.Add($"Item {i} AAA BBB CCC DDD", i % 2);
            //}
            //listView1.View = View.List; // List  Details  LargeIcon

            List<WaveInCapabilities> recin = [];
            for (int id = -1; id < WaveIn.DeviceCount; id++) // –1 indicates the default output device, while 0 is the first output device.
            {
                var cap = WaveIn.GetCapabilities(id);
                recin.Add(cap);

                Debug.WriteLine($"IN: {id} {cap.ProductName}");
            }

            List<WaveOutCapabilities> recout = [];
            for (int id = -1; id < WaveOut.DeviceCount; id++) // –1 indicates the default output device, while 0 is the first output device.
            {
                var cap = WaveOut.GetCapabilities(id);
                recout.Add(cap);

                Debug.WriteLine($"OUT: {id} {cap.ProductName}");
            }

            //IN: -1 Microsoft Sound Mapper
            //IN: 0 Microphone (Realtek(R) Audio)
            //IN: 1 Microphone Array (Intel® Smart 
            //OUT: -1 Microsoft Sound Mapper
            //OUT: 0 Headphones (Realtek(R) Audio)
            //OUT: 1 Speakers (Realtek(R) Audio)

            base.OnLoad(e);
        }

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                W32.DeregisterShellHook(Handle);
                W32.UnregisterHotKeys(Handle);
                W32.UnhookWindowsHookEx(_hHook1);
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        void Tell(string s)
        {
            Output.Append(s);
        }

        /// <summary>
        /// 
        /// </summary>
        void AddHotKey(HotKey hk)
        {
            // Listen for hot keys.
            var key = hk.Key[0] & ~0x20; // make it UC   // high-order word
            var mod = (hk.Ctrl ? W32.MOD_CTRL : 0) |  // low-order word
                (hk.Alt ? W32.MOD_ALT : 0) |
                (hk.Shift ? W32.MOD_SHIFT : 0) |
                (hk.Win ? W32.MOD_WIN : 0);
            W32.RegisterHotKey(Handle, key, mod);
        }

        async void AsyncClick(object? sender, EventArgs e)
        {
            Tell($"AsyncClick start");
            NewBGW bgw = new();
            await bgw.Run(3);
            Tell($"AsyncClick end");

            //Reset();
            //var x = new AsyncAwait();
            //var res = await x.Go();
            //Tell(INF, $"res:{res}");
        }

        /// <summary>
        /// 
        /// </summary>
        void TracerClick(object? sender, EventArgs e)
        {
            var x = new TracerTest();
            x.Go(12.34, new(50, 60, 70, 80));
            x.PlayWithAttribute();
        }

        #region Windows hooks
        /// <summary>
        /// Handle the hooked shell messages: shell window lifetime and hotkeys.
        /// </summary>
        /// <param name="message"></param>
        protected override void WndProc(ref Message message)
        {
            IntPtr handle = message.LParam;
            
            if (message.Msg == _hHook1)
            {
                var shellEvent = message.WParam.ToInt32();

                switch (shellEvent)
                {
                   case W32.HSHELL_WINDOWCREATED:
                       WM.AppWindowInfo wi = WM.GetAppWindowInfo(handle);
                       Output.Append($"WindowCreatedEvent:{handle} {wi.Title}");
                       break;

                   case W32.HSHELL_WINDOWDESTROYED:
                       Output.Append($"WindowDestroyedEvent:{handle}");
                       break;
                }
            }

            if (message.Msg == W32.WM_HOTKEY_MESSAGE_ID) // Decode key.
            {
               Keys key = Keys.None;
               int mod = (int)((long)message.LParam & 0xFFFF);
               int num = (int)(message.LParam >> 16);
               if (Enum.IsDefined(typeof(Keys), num))
               {
                   key = (Keys)Enum.ToObject(typeof(Keys), num);
               }
               // else do something?

               if ((key != Keys.None) && (mod & W32.MOD_ALT) > 0 && (mod & W32.MOD_CTRL) > 0)
               {
                    Output.Append($"Hotkey:{key}");
                   //switch (key) etc...
               }
            }

            base.WndProc(ref message);
        }

        #if _OTHER_KBDHOOK
        /// <summary>
        /// Low level keyboard hook function. Other way to implement hotkeys - from WinClip
        /// </summary>
        /// <param name="code">Virtual-key code in the range 1 to 254. If less than zero, pass the message to the CallNextHookEx function without further processing.</param>
        /// <param name="wParam">One of the following messages: WM_KEYDOWN WM_KEYUP WM_SYSKEYDOWN WM_SYSKEYUP.</param>
        /// <param name="lParam">Pointer to a KBDLLHOOKSTRUCT structure.</param>
        /// <returns>Return value from call to next in chain or >0 for handled locally</returns>
        int KeyboardHookProc(int code, int wParam, ref W32.KBDLLHOOKSTRUCT lParam)
        {
           bool handled = false;
           if (code >= 0)
           {
               Keys key = (Keys)lParam.vkCode;
               bool keyDown = wParam == W32.WM_KEYDOWN || wParam == W32.WM_SYSKEYDOWN;
               bool keyUp = wParam == W32.WM_KEYUP || wParam == W32.WM_SYSKEYUP;
               bool letterPressed = key == Keys.R && keyDown;
               bool winKey = (key == Keys.LWin || key == Keys.RWin) && keyDown;
               bool ctrlKey = (key & Keys.Control) > 0 && keyDown;
               bool altKey = (key & Keys.Alt) > 0 && keyDown;
           }
           if (handled)
           {
               // If the hook procedure processed the message, it may return a nonzero value to prevent
               // the system from passing the message to the rest of the hook chain or the target window procedure.
               return 1;
           }
           else
           {
               // Pass along chain.
               return W32.CallNextHookEx(_hHook, code, wParam, ref lParam);
           }
        }
        #endif
        #endregion
    }

    #region Bits and pieces
    /// <summary>Custom rectangle for this application.</summary>
    public class DisplayRect
    {
        public int Left { get; init; } = -1;
        public int Top { get; init; } = -1;
        public int Right { get; init; } = -1;
        public int Bottom { get; init; } = -1;
        public Rectangle WinRect { get { return new Rectangle(Left, Top, Right - Left, Bottom - Top); } }
        public bool IsValid { get; init; } = false;

        /// <summary>Default constructor - invalid.</summary>
        public DisplayRect()
        {
            IsValid = false;
        }

        /// <summary>Normal constructor.</summary>
        public DisplayRect(int left, int top, int width, int height)
        {
            IsValid = top >= 0 && left >= 0 && width >= 0 && height >= 0;
            if (!IsValid) throw new ArgumentException("Invalid args");
            Left = left;
            Top = top;
            Right = left + width;
            Bottom = top + height;
        }

        /// <summary>Read me.</summary>
        public override string ToString()
        {
            return IsValid ? $"L:{Left} T:{Top} R:{Right} B:{Bottom}" : "Invalid";
        }
    }

    [Serializable]
    public sealed class HotKey
    {
        public string Key { get; set; } = "?";
        public bool Ctrl { get; set; } = false;
        public bool Alt { get; set; } = false;
        public bool Shift { get; set; } = false;
        public bool Win { get; set; } = false;
    }
    #endregion

    class DelegateLambda // TODO1 absorb
    {
        // Delegates are really just structural typing for functions. You could do the same thing with nominal typing and 
        // implementing an anonymous class that implements an interface or abstract class, but that ends up being a lot of 
        // code when only one function is needed.

        // Lambda comes from the idea of lambda calculus of Alonzo Church in the 1930s. It is an anonymous way of creating 
        // functions. They become especially useful for composing functions

        // So while some might say lambda is syntactic sugar for delegates, I would says delegates are a bridge for easing 
        // people into lambdas in c#.

        // One difference is that an anonymous delegate can omit parameters while a lambda must match the exact signature. Given:
        public delegate string TestDelegate(int i);

        public void Test(TestDelegate d) { }

        // you can call it in the following four ways (note that the second line has an anonymous delegate that does not have any parameters):
        void Callit()
        {
            Test(delegate (int i) { return string.Empty; });
            Test(delegate { return string.Empty; });
            Test(i => string.Empty);
            Test(D);
        }

        private string D(int i)
        {
            return string.Empty;
        }

        private string D2()
        {
            return string.Empty;
        }

        // // You cannot pass in a lambda expression that has no parameters or a method that has no parameters. These are not allowed:
        // Test(() => String.Empty); // Not allowed, lambda must match signature
        // Test(D2); // Not allowed, method must match signature
    }
}
