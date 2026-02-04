using System;
using System.Drawing;
using System.Windows.Forms;

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
            btnGo1 = new Button();
            label1 = new Label();
            SuspendLayout();
            // 
            // btnGo1
            // 
            btnGo1.Location = new Point(26, 12);
            btnGo1.Name = "btnGo1";
            btnGo1.Size = new Size(86, 26);
            btnGo1.TabIndex = 0;
            btnGo1.Text = "Go 1";
            btnGo1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BorderStyle = BorderStyle.FixedSingle;
            label1.Location = new Point(205, 8);
            label1.Name = "label1";
            label1.Size = new Size(47, 21);
            label1.TabIndex = 1;
            label1.Text = "label1";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(871, 571);
            Controls.Add(label1);
            Controls.Add(btnGo1);
            Name = "MainForm";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnGo1;
        private Label label1;
    }
}
