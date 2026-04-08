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
using W32 = Ephemera.Win32.Internals;
using WM  = Ephemera.Win32.WindowManagement;


namespace NLab
{
    public partial class TrayExForm : Form
    {
        #region Fields
        readonly RichTextBox rtbInfo;
        readonly Button btnKickMe;
        bool _disposed = false;
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public TrayExForm()
        {
            int BORDER = 5;

            // This form.
            Text = "===> TrayExForm <===";
            Size = new Size(200, 400);
            BackColor = Color.Gold;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            ShowIcon = false;
            ShowInTaskbar = false;
            Visible = false;

            btnKickMe = new()
            {
                Location = new(BORDER, BORDER),
                Size = new(ClientRectangle.Width - 2 * BORDER, 40),
                Text = "Kick Me!!",
            };
            btnKickMe.Click += (_, __) => rtbInfo.AppendText("Kick Me !!! ");
            Controls.Add(btnKickMe);

            rtbInfo = new()
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(BORDER, btnKickMe.Bottom + BORDER),
                Size = new(ClientRectangle.Width - 2 * BORDER, ClientRectangle.Bottom - btnKickMe.Bottom + BORDER)
            };
            Controls.Add(rtbInfo);

            // Hotkeys.
            W32.RegisterHotKey(Handle, (int)Keys.A, W32.MOD_ALT | W32.MOD_CTRL);
            W32.RegisterHotKey(Handle, (int)Keys.D9, W32.MOD_CTRL);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // TODO this doesn't work.
            base.OnLoad(e);
        }

        /// <summary>
        /// Fake closing by hiding the form.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            Log($"OnClosing()");
            Visible = false;
            e.Cancel = true;
            base.OnClosing(e);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            Log($"Dispose({disposing} {_disposed})");
            if (disposing)
            {
                W32.DeregisterShellHook(Handle);
                W32.UnregisterHotKeys(Handle);
            }
            _disposed = true;
            base.Dispose(disposing);
        }

        /// <summary>
        /// Handle hot keys.
        /// </summary>
        /// <param name="msg"></param>
        protected override void WndProc(ref Message msg)
        {
            bool handled = false;

            if (msg.Msg == W32.WM_HOTKEY_MESSAGE_ID)
            {
                var mod = (int)((long)msg.LParam & 0x0000FFFF);
                var kid = (int)((msg.LParam >> 16) & 0x0000FFFF);
                Keys key = Enum.IsDefined(typeof(Keys), kid) ? (Keys)Enum.ToObject(typeof(Keys), kid) : Keys.None;

                switch (key, mod)
                {
                    case (Keys.D9, W32.MOD_CTRL | W32.MOD_ALT):
                        // do something...
                        Tell($"key:ctrl-alt-9");
                        handled = true;
                        break;

                    case (Keys.None, _):
                        // do something...
                        Tell($"key:none");
                        break;

                    case (_, _):
                        // do something...
                        Tell($"key:????");
                        break;
                }
            }

            if (!handled)
            {
                base.WndProc(ref msg);
            }
        }

        /// <summary>
        /// Just for debugging.
        /// </summary>
        /// <param name="msg"></param>
        void Log(string msg)
        {
            string s = $"{DateTime.Now:mm\\:ss\\.fff} FORM {msg}";
            Debug.WriteLine(s);
        }

        /// <summary>
        /// Show the user.
        /// </summary>
        /// <param name="msg"></param>
        void Tell(string msg)
        {
            string s = $"{msg}{Environment.NewLine}";
            rtbInfo.AppendText(s);
            rtbInfo.ScrollToCaret();
        }
    }
}
