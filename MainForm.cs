using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassHierarchy;


namespace NLab
{
    public partial class MainForm : Form
    {
        readonly long _startTick = Stopwatch.GetTimestamp();

        public MainForm()
        {
            InitializeComponent();

        }


        //There are also other awaitable types: special methods such as “Task.Yield” return awaitables
        //that are not Tasks, and the WinRT runtime(coming in Windows 8) has an unmanaged awaitable type.
        //You can also create your own awaitable(usually for performance reasons), or use extension methods
        //to make a non-awaitable type awaitable.
        
        
        
        public async Task DoSomethingAsync()
        {
            // In the Real World, we would actually do something...
            // For this example, we're just going to (asynchronously) wait 100ms.


            await Task.Delay(100);

        }

        public async Task<int> MyFuncAsync(string s)
        {
            Print($"MyFuncAsync() 1");

            await Task.Delay(100);

            Print($"MyFuncAsync() 2");

            return s.Length;
        }




        public Task<string> SyncWrapperAsync()
        {
            // Note that this is not an async method, so we can't use await in here.
            //...

            return new Task<string>("abc");
        }

        public async Task ComposeAsync()
        {
            // We can await Tasks, regardless of where they come from.
            await DoSomethingAsync();
            await MyFuncAsync("gogogo");
        }


        //when you await a built-in awaitable (Task), then the awaitable will capture the current “context” and
        //later apply it to the remainder of the async method. What exactly is that “context”?
        //    If you’re on a UI thread, then it’s a UI context.
        //    If you’re responding to an ASP.NET request, then it’s an ASP.NET request context.
        //    Otherwise, it’s usually a thread pool context.
        //A good rule of thumb is to use ConfigureAwait(false) unless you know you do need the context.

        // WinForms example (it works exactly the same for WPF).
        private async void DownloadFileButton_Click(object sender, EventArgs e)
        {
            // Since we asynchronously wait, the UI thread is not blocked by the file download.
            // ConfigureAwaitOptions to not restore the original context.
            await MyFuncAsync("fileNameTextBox.Text").ConfigureAwait(false); //  (ConfigureAwaitOptions.None);

            // Since we resume on the UI context, we can directly access UI elements.
            Print("File downloaded!"); // boom?
        }

        //By using concurrent composition(Task.WhenAll or Task.WhenAny), you can perform simple concurrent operations.
        //You can also use these methods along with Task.Run to do simple parallel computation.


        //Old 	        New 	    Description
        //task.Wait 	await task 	Wait/await for a task to complete
        //task.Result 	await task 	Get the result of a completed task
        //Task.WaitAny 	await Task.WhenAny 	Wait/await for one of a collection of tasks to complete
        //Task.WaitAll 	await Task.WhenAll 	Wait/await for every one of a collection of tasks to complete
        //Thread.Sleep 	await Task.Delay 	Wait/await for a period of time
        //Task ctor 	Task.Run or TaskFactory.StartNew 	Create a code-based task



        //And if from your UI thread you want to offload work to a worker thread, and you use Task.Run to do so,
        //you often typically want to do some work back on the UI thread once that background work is done,
        //and these language features make that kind of coordination easy and seamless.


        //The normal way to report errors from tasks is by placing an exception on the task.
        //In the most common scenario - an async method that returns Task - your code can just throw an exception
        //and the async machinery will catch that exception and place it on the returned Task for you.So your code
        //can just use throw, try, and catch exactly like normal and it will all work.



        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

                void Print(string s)
        {
            long tick = Stopwatch.GetTimestamp();
            double sec = 1.0 * (tick - _startTick) / Stopwatch.Frequency;
            //double msec = 1000.0 * (tick - _startTick) / Stopwatch.Frequency;
            s = $"{sec:000.000} {s}{Environment.NewLine}";
            Output.AppendText(s);
        }
    }



    ///// <summary>General definitions.</summary>
    //public class Common
    //{
    //    #region General definitions
    //    /// <summary>Midi constant.</summary>
    //    public const int MIDI_VAL_MIN = 0;

    //    /// <summary>Midi constant.</summary>
    //    public const int MIDI_VAL_MAX = 127;

    //    /// <summary>Per device.</summary>
    //    public const int NUM_MIDI_CHANNELS = 16;

    //    /// <summary>Corresponds to midi velocity = 0.</summary>
    //    public const double VOLUME_MIN = 0.0;

    //    /// <summary>Corresponds to midi velocity = 127.</summary>
    //    public const double VOLUME_MAX = 1.0;

    //    /// <summary>Default value.</summary>
    //    public const double VOLUME_DEFAULT = 0.8;

    //    /// <summary>Allow UI controls some more headroom.</summary>
    //    public const double MAX_GAIN = 2.0;
    //    #endregion
    //}

    //public interface IConsole
    //{
    //    bool KeyAvailable { get; }
    //    string Title { get; set; }
    //    void Write(string text);
    //    void WriteLine(string text);
    //    string? ReadLine();
    //    ConsoleKeyInfo ReadKey(bool intercept);
    //}
}
