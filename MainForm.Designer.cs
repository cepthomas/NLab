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
            Output = new RichTextBox();
            SuspendLayout();
            // 
            // Output
            // 
            Output.Font = new Font("Cascadia Code", 9.163636F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Output.Location = new Point(43, 45);
            Output.Name = "Output";
            Output.Size = new Size(679, 369);
            Output.TabIndex = 0;
            Output.Text = "";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(Output);
            Name = "MainForm";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox Output;
    }
}
