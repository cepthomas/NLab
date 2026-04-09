using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfUis;
// using W32 = Ephemera.Win32.Internals;
// using WM  = Ephemera.Win32.WindowManagement;


namespace NLab
{
    /// <summary>
    /// Demonstrates running a form application as a tray app.
    /// Provides a form for user input.
    /// Captures hotkeys.
    /// </summary>
    public class TrayEx : ApplicationContext
    {
        #region Fields
        readonly Icon? _icon1;
        readonly Icon? _icon2;
        readonly Container _components = new();
        readonly NotifyIcon? _notifyIcon;
        TrayExForm? _form = new();
        #endregion

        #region Lifecycle
        /// <summary>Start here.</summary>
        public TrayEx()
        {
            // Clean up resources.
            Application.ApplicationExit += ApplicationExit_Handler;
            // Or alternatively.
            ThreadExit += ThreadExit_Handler;

            // Context menu.
            ContextMenuStrip ctxm = new();
            ctxm.Items.Add("dialog", null, Menu_Click);
            ctxm.Items.Add("icon", null, Menu_Click);
            ctxm.Items.Add(new ToolStripSeparator());
            ctxm.Items.Add("exit", null, Menu_Click);
            ctxm.Opening += ContextMenu_Opening;

            // Icons.
            var sf = (Bitmap)Image.FromFile("glyphicons-22-snowflake.png");
            var img1 = sf.Colorize(Color.LightGreen);
            var img2 = sf.Colorize(Color.HotPink);
            _icon1 = GraphicsUtils.CreateIcon(img1);
            _icon2 = GraphicsUtils.CreateIcon(img2);

            // Notify icon.
            _notifyIcon = new(_components)
            {
                Icon = _icon1,
                Text = "I am a tray application!",
                ContextMenuStrip = ctxm,
                Visible = true,
                BalloonTipText = "OK",
            };
            _notifyIcon.MouseClick += (sender, e) => { Log($"You clicked icon:{e.Button}"); };
            _notifyIcon.MouseDoubleClick += (sender, e) => { Log($"You double clicked icon:{e.Button}"); }; ;

            // UI form.
            _form.StartPosition = FormStartPosition.Manual;
            _form.Location = new Point(1000, 500);
            _form.Show();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            Log($"Dispose({disposing})");

            if (disposing)
            {
                _notifyIcon.ContextMenuStrip?.Dispose();
                _notifyIcon.Dispose();
                _icon1.Dispose();
                _icon2.Dispose();

                _form?.Dispose();
                _form = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Clean up resources.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplicationExit_Handler(object? sender, EventArgs e)
        {
            Log($"ApplicationExit_Handler()");

            // Causes the thread's message loop to be terminated. This will call ExitThreadCore.
            ExitThread();
        }

        /// <summary>
        /// This may be useful too.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ThreadExit_Handler(object? sender, EventArgs e)
        {
            Log($"ThreadExit_Handler()");
        }

        /// <summary>
        /// This may be useful too.
        /// If we are presently showing a form, clean it up.
        /// </summary>
        protected override void ExitThreadCore()
        {
            Log($"ExitThreadCore()");
            base.ExitThreadCore();
        }
        #endregion

        #region UI Event Handling
        /// <summary>
        /// Handle context menu before shown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ContextMenu_Opening(object? sender, CancelEventArgs e)
        {
            // Could add more options here.
            var cms = _notifyIcon.ContextMenuStrip;
            if (cms.Items.Count < 5)
            {
                cms.Items.Add(new ToolStripSeparator());
                cms.Items.Add("extra!", null, Menu_Click);
            }

            e.Cancel = false;
        }

        /// <summary>
        /// Handle context menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Menu_Click(object? sender, EventArgs e)
        {
            var mi = (ToolStripMenuItem)sender!;
            Log($"You clicked menu:{mi.Text}");

            switch (mi.Text)
            {
                case "dialog":
                    //_form.WindowState = _form.WindowState == FormWindowState.Minimized ? FormWindowState.Normal : FormWindowState.Minimized;
                    _form.Visible = !_form.Visible;
                    break;

                case "icon":
                    _notifyIcon.Icon = _notifyIcon.Icon == _icon1 ? _icon2 : _icon1;
                    break;

                case "exit":
                    _notifyIcon.Visible = false; // remove lingering tray icon
                    Log($"exit");
                    ExitThread();
                    break;
            }
        }
        #endregion

        #region Internal Functions
        /// <summary>
        /// Just for debugging.
        /// </summary>
        /// <param name="msg"></param>
        void Log(string msg)
        {
            string s = $"{DateTime.Now:mm\\:ss\\.fff} TRAY {msg}";
            Debug.WriteLine(s);
        }
        #endregion
    }
}
