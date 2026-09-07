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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            BtnAsync = new Button();
            BtnSomething = new Button();
            BtnTracer = new Button();
            Output = new TextViewer();
            ColorWheel1 = new ColorWheel();
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
            // BtnSomething
            // 
            BtnSomething.Location = new Point(115, 12);
            BtnSomething.Name = "BtnSomething";
            BtnSomething.Size = new Size(86, 26);
            BtnSomething.TabIndex = 2;
            BtnSomething.Text = "Something";
            BtnSomething.UseVisualStyleBackColor = true;
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
            // Output
            // 
            Output.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Output.BorderStyle = BorderStyle.FixedSingle;
            Output.Location = new Point(12, 147);
            Output.MatchUseBackground = true;
            Output.MaxText = 10000;
            Output.Name = "Output";
            Output.Prompt = "";
            Output.Size = new Size(838, 436);
            Output.TabIndex = 1;
            Output.WordWrap = true;
            // 
            // ColorWheel1
            // 
            ColorWheel1.Alpha = 1D;
            ColorWheel1.Color = Color.Black;
            ColorWheel1.ColorStep = 4;
            ColorWheel1.DisplayLightness = false;
            ColorWheel1.HslColor = (HslColor)resources.GetObject("ColorWheel1.HslColor");
            ColorWheel1.LargeChange = 5;
            ColorWheel1.Lightness = 0.5D;
            ColorWheel1.LineColor = Color.DimGray;
            ColorWheel1.Location = new Point(558, 12);
            ColorWheel1.Name = "ColorWheel1";
            ColorWheel1.SecondarySelectionSize = 8;
            ColorWheel1.SelectionSize = 10;
            ColorWheel1.ShowAngleArrow = false;
            ColorWheel1.ShowCenterLines = false;
            ColorWheel1.ShowSaturationRing = false;
            ColorWheel1.Size = new Size(179, 129);
            ColorWheel1.SmallChange = 1;
            ColorWheel1.TabIndex = 7;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 600);
            Controls.Add(ColorWheel1);
            Controls.Add(BtnTracer);
            Controls.Add(BtnSomething);
            Controls.Add(Output);
            Controls.Add(BtnAsync);
            Location = new Point(400, 100);
            Name = "MainForm";
            StartPosition = FormStartPosition.Manual;
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Button BtnAsync;
        private Button BtnSomething;
        private TextViewer Output;
        private Button BtnTracer;
        private ColorWheel ColorWheel1;
    }
}
