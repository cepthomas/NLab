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

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            BtnAsync = new Button();
            BtnTasks = new Button();
            BtnScoper = new Button();
            tvOutput = new TextViewer();
            SuspendLayout();
            // 
            // BtnAsync
            // 
            BtnAsync.Location = new Point(26, 12);
            BtnAsync.Name = "BtnAsync";
            BtnAsync.Size = new Size(86, 26);
            BtnAsync.TabIndex = 0;
            BtnAsync.Text = "Async";
            BtnAsync.UseVisualStyleBackColor = true;
            // 
            // BtnTasks
            // 
            BtnTasks.Location = new Point(157, 12);
            BtnTasks.Name = "BtnTasks";
            BtnTasks.Size = new Size(86, 26);
            BtnTasks.TabIndex = 2;
            BtnTasks.Text = "Tasks";
            BtnTasks.UseVisualStyleBackColor = true;
            // 
            // BtnScoper
            // 
            BtnScoper.Location = new Point(275, 12);
            BtnScoper.Name = "BtnScoper";
            BtnScoper.Size = new Size(86, 26);
            BtnScoper.TabIndex = 3;
            BtnScoper.Text = "Scoper";
            BtnScoper.UseVisualStyleBackColor = true;
            // 
            // tvOutput
            // 
            tvOutput.BorderStyle = BorderStyle.FixedSingle;
            tvOutput.Location = new Point(12, 61);
            tvOutput.MaxText = 10000;
            tvOutput.Name = "tvOutput";
            tvOutput.Prompt = "";
            tvOutput.Size = new Size(847, 498);
            tvOutput.TabIndex = 1;
            tvOutput.WordWrap = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(871, 571);
            Controls.Add(BtnScoper);
            Controls.Add(BtnTasks);
            Controls.Add(tvOutput);
            Controls.Add(BtnAsync);
            Name = "MainForm";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Button BtnAsync;
        private Button BtnTasks;
        private TextViewer tvOutput;
        private Button BtnScoper;
    }
}
