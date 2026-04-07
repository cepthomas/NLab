using System;
using System.Drawing;
using System.Windows.Forms;
using Ephemera.NBagOfUis;


namespace NLab
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            BtnAsync = new Button();
            BtnTasks = new Button();
            BtnTracer = new Button();
            tvOutput = new TextViewer();
            BtnJumplist = new Button();
            BtnTray = new Button();
            SuspendLayout();
            // 
            // BtnAsync
            // 
            BtnAsync.Location = new Point(12, 12);
            BtnAsync.Name = "BtnAsync";
            BtnAsync.Size = new Size(86, 26);
            BtnAsync.TabIndex = 0;
            BtnAsync.Text = "Async";
            BtnAsync.UseVisualStyleBackColor = true;
            // 
            // BtnTasks
            // 
            BtnTasks.Location = new Point(115, 12);
            BtnTasks.Name = "BtnTasks";
            BtnTasks.Size = new Size(86, 26);
            BtnTasks.TabIndex = 2;
            BtnTasks.Text = "Tasks";
            BtnTasks.UseVisualStyleBackColor = true;
            // 
            // BtnTracer
            // 
            BtnTracer.Location = new Point(222, 12);
            BtnTracer.Name = "BtnTracer";
            BtnTracer.Size = new Size(86, 26);
            BtnTracer.TabIndex = 3;
            BtnTracer.Text = "Tracer";
            BtnTracer.UseVisualStyleBackColor = true;
            // 
            // tvOutput
            // 
            tvOutput.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tvOutput.BorderStyle = BorderStyle.FixedSingle;
            tvOutput.Location = new Point(12, 59);
            tvOutput.MaxText = 10000;
            tvOutput.Name = "tvOutput";
            tvOutput.Prompt = "";
            tvOutput.Size = new Size(688, 231);
            tvOutput.TabIndex = 1;
            tvOutput.WordWrap = true;
            // 
            // BtnJumplist
            // 
            BtnJumplist.Location = new Point(329, 12);
            BtnJumplist.Name = "BtnJumplist";
            BtnJumplist.Size = new Size(86, 26);
            BtnJumplist.TabIndex = 5;
            BtnJumplist.Text = "Jumplist";
            BtnJumplist.UseVisualStyleBackColor = true;
            // 
            // BtnTray
            // 
            BtnTray.Location = new Point(434, 12);
            BtnTray.Name = "BtnTray";
            BtnTray.Size = new Size(86, 26);
            BtnTray.TabIndex = 6;
            BtnTray.Text = "Tray";
            BtnTray.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(711, 298);
            Controls.Add(BtnTray);
            Controls.Add(BtnJumplist);
            Controls.Add(BtnTracer);
            Controls.Add(BtnTasks);
            Controls.Add(tvOutput);
            Controls.Add(BtnAsync);
            Location = new Point(1000, 100);
            Name = "MainForm";
            StartPosition = FormStartPosition.Manual;
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Button BtnAsync;
        private Button BtnTasks;
        private TextViewer tvOutput;
        private Button BtnTracer;
        private Button BtnJumplist;
        private Button BtnTray;
    }
}
