using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ClassHierarchy;


namespace NLab
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

        }

        void Print(string s)
        {
            Output.AppendText(s);
            Output.AppendText(Environment.NewLine);
        }
    }



    /// <summary>General definitions.</summary>
    public class Common
    {
        #region General definitions
        /// <summary>Midi constant.</summary>
        public const int MIDI_VAL_MIN = 0;

        /// <summary>Midi constant.</summary>
        public const int MIDI_VAL_MAX = 127;

        /// <summary>Per device.</summary>
        public const int NUM_MIDI_CHANNELS = 16;

        /// <summary>Corresponds to midi velocity = 0.</summary>
        public const double VOLUME_MIN = 0.0;

        /// <summary>Corresponds to midi velocity = 127.</summary>
        public const double VOLUME_MAX = 1.0;

        /// <summary>Default value.</summary>
        public const double VOLUME_DEFAULT = 0.8;

        /// <summary>Allow UI controls some more headroom.</summary>
        public const double MAX_GAIN = 2.0;
        #endregion
    }

    public interface IConsole
    {
        bool KeyAvailable { get; }
        string Title { get; set; }
        void Write(string text);
        void WriteLine(string text);
        string? ReadLine();
        ConsoleKeyInfo ReadKey(bool intercept);
    }
}
