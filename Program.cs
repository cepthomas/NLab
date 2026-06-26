using System;
using System.Linq;
using System.Windows.Forms;


namespace NLab
{
    internal static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Handle unexpected esceptions.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, e) => { HandleException(e.Exception, "UI Thread Exception"); };
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => { HandleException((Exception)e.ExceptionObject, "Background Thread Exception"); };

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                var cmd = args.Count() == 0 ? "" : args[0];

                switch (cmd)
                {
                    case "j": Application.Run(new JumpListEx()); break;
                    case "t": Application.Run(new TrayEx()); break;
                    case "":  Application.Run(new MainForm(args)); break;
                    default: throw new LabException($"Baaaad [{cmd}]");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "!!!");
                Environment.Exit(1);
            }
        }

        static void HandleException(Exception ex, string type)
        {
            MessageBox.Show(ex.ToString(), type);
            Environment.Exit(1);
        }
    }
}