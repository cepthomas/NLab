using System;
using System.Linq;
using System.Windows.Forms;


namespace NLab
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            var cmd = args.Count() == 0 ? "" : args[0];

            switch (cmd)
            {
                case "j": Application.Run(new JumpListEx()); break;
                case "t": Application.Run(new TrayExApplicationContext()); break;
                case "":  Application.Run(new MainForm(args)); break;
                default: throw new LabException($"Baaaad [{cmd}]");
            }
        }
    }
}