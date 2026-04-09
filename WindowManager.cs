using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

using System.Windows.Forms;


//using System.Threading;
using Ephemera.NBagOfTricks;
using W32 = Ephemera.Win32.Internals;
using WM = Ephemera.Win32.WindowManagement;



namespace NLab
{
    public class WindowManager : Form
    {
        #region Fields
        bool opt = false;
        string layout = "???";
        bool _running = true;
        #endregion

        /// <summary>Hook message processing.</summary>
        int _hookMsg;

        /// <summary>Build me one.</summary>
        public void DoIt()
        {
            // Default start location.
            string startDir = Environment.CurrentDirectory;
            var exe = Environment.GetEnvironmentVariable("TOOLS_PATH");
            var inifile = Path.Join(exe, "winmgr.ini");

            _hookMsg = W32.RegisterShellHook(Handle); // test for 0?
            W32.RegisterHotKey(Handle, (int)Keys.A, W32.MOD_ALT | W32.MOD_CTRL);
            W32.RegisterHotKey(Handle, (int)Keys.X, W32.MOD_CTRL);

            try
            {
                var fgHandle = WM.ForegroundWindow; // -> left pane
                WM.AppWindowInfo fginfo = WM.GetAppWindowInfo(fgHandle);

                // New explorer -> right pane.
                var path = @"C:\Dev\Misc\NLab\TestFiles\";
                W32.ShellExecute("explore", path);

                // Locate the new explorer window. Wait for it to be created. This is a bit klunky but there does not appear to be a more direct method.
                int tries = 0; // ~4
                WM.AppWindowInfo? rightPane = null;
                for (tries = 0; tries < 20 && rightPane is null; tries++)
                {
                    System.Threading.Thread.Sleep(50);
                    var wins = WM.GetAppWindows("explorer");
                    rightPane = wins.Where(w => w.Title == path).FirstOrDefault();
                }
                if (rightPane is null) throw new InvalidOperationException($"Couldn't create right pane for [{path}]");

                // Relocate/resize the windows to fit available real estate. TODO configurable? full screen?
                WM.AppWindowInfo desktop = WM.GetAppWindowInfo(WM.ShellWindow);
                Point loc = new(50, 50);
                Size sz = new(desktop.DisplayRectangle.Width * 45 / 100, desktop.DisplayRectangle.Height * 80 / 100);
                // Left pane.
                WM.MoveWindow(fgHandle, loc);
                WM.ResizeWindow(fgHandle, sz);
                WM.ForegroundWindow = fgHandle;
                // Right pane.
                loc.Offset(sz.Width, 0);
                WM.MoveWindow(rightPane.Handle, loc);
                WM.ResizeWindow(rightPane.Handle, sz);
                WM.ForegroundWindow = rightPane.Handle;
            }
            catch (Exception ex)
            {
                W32.MessageBox($"{ex.GetType()}: {ex.Message}", "Error", error:true);
                Environment.Exit(1);
            }

            Environment.Exit(0);
        }

        /// <summary>
        /// Handle the hooked shell messages: shell window lifetime and hotkeys.
        /// </summary>
        /// <param name="message"></param>
        protected override void WndProc(ref Message message)
        {
            IntPtr handle = message.LParam;

            if (message.Msg == _hookMsg)
            {
                var shellEvent = message.WParam.ToInt32();

                switch (shellEvent)
                {
                    case W32.HSHELL_WINDOWCREATED:
                        WM.AppWindowInfo wi = WM.GetAppWindowInfo(handle);
                        Console.WriteLine($"WindowCreatedEvent:{handle} {wi.Title}");
                        break;

                    case W32.HSHELL_WINDOWDESTROYED:
                        Console.WriteLine($"WindowDestroyedEvent:{handle}");
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

              //  if ((key != Keys.None) && (mod & W32.MOD_ALT) > 0 && (mod & W32.MOD_CTRL) > 0)
                if ((key != Keys.None) && (mod & W32.MOD_CTRL) > 0)
                {
                    Console.WriteLine($"Hotkey:{key}");
                    //switch (key) etc...

                    _running = false;
                }
            }

            base.WndProc(ref message);
        }
    }
}
