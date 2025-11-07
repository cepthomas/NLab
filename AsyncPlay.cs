using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace NLab
{

    public class AsyncPlay
    {

        public event EventHandler<NotifEventArgs>? Notif;

        readonly long _startTick = Stopwatch.GetTimestamp();


        public void Go()
        {
            //using var scp = new Scoper("Go");
            
            // IO bound op. Use Async versions of calls. Sys calls should have them.
            File.ReadAllLinesAsync("fffff");

        }



        //when you await a built-in awaitable (Task), then the awaitable will capture the current “context” and
        //later apply it to the remainder of the async method. What exactly is that “context”?
        //    If you’re on a UI thread, then it’s a UI context.
        //    If you’re responding to an ASP.NET request, then it’s an ASP.NET request context.
        //    Otherwise, it’s usually a thread pool context.
        //A good rule of thumb is to use ConfigureAwait(false) unless you know you do need the context.

        // WinForms example (it works exactly the same for WPF).
        private async void UiButton_Click(object sender, EventArgs e)
        {
            // Since we asynchronously wait, the UI thread is not blocked by the file download.
            // ConfigureAwaitOptions to not restore the original context.
            //            await MyFuncAsync("download fileNameTextBox.Text").ConfigureAwait(false); //  (ConfigureAwaitOptions.None);

            // Since we resume on the UI context, we can directly access UI elements.   boom?
            Print("File downloaded!");
        }


        // >>> do this instead:
        //One of the easier methods is to use Task.Run, very similar to your existing code.
        //However, I do not recommend implementing a CalculateAsync method since that implies the
        //processing is asynchronous (which it is not). Instead, use Task.Run at the point of the call:
        async Task MakeRequest()
        {
            // do some stuff
            int i = -1;
            var task = Task.Run(() => { return Calculate("myInput"); });

            // do other stuff
            var myOutput = await task;

            // some more stuff
        }
        int Calculate(string s)
        {
            return s.Length;
        }



        //You shouldn't make the async method exist at all. It's a fake async method that actually blocks.
        //Let the users choose if they want this to run on a different thread by doing Task.Run themselves.
        //Don't pretend it's async, you'd just be lying. 
        //Simply return Task.FromResult once method is done synchronously and there you have it.
        //an async task MAY complete asynchronously, but doesn't have to. 




        public void CpuBoundSyncOperation()
        {
            // CPU bound ops can be wrapped.
            // if you're writing CPU-bound methods provide both sync and async versions to be nice!

        }


        public void IoBoundSyncOperation()
        {
            //
            
        }


        // 
        public async Task<int> MyFunc1Async(string s)
        {
            Print($"MyFunc1Async() enter");

            await Task.Delay(100);

            Print($"MyFunc1Async() 2");

            return s.Length;
        }

        //And if from your UI thread you want to offload work to a worker thread, and you use Task.Run to do so,
        //you often typically want to do some work back on the UI thread once that background work is done,
        //and these language features make that kind of coordination easy and seamless.
        async Task RunOnThreadAsync()
        {

        }


        // We can await Tasks, regardless of where they come from.
        public async Task ComposeAsync()
        {
            await RunOnThreadAsync();
            await MyFunc1Async("gogogo");
        }

        //Alternatively, if it works well with your code, you can use the Parallel type, i.e., Parallel.For,
        //Parallel.ForEach, or Parallel.Invoke. The advantage to the Parallel code is that the request thread
        //is used as one of the parallel threads, and then resumes executing in the thread context
        //(there's less context switching than the async example):
        //void MakeRequest()
        //{
        //  Parallel.Invoke(() => Calculate(myInput1),
        //      () => Calculate(myInput2),
        //      () => Calculate(myInput3));
        //}

        void Print(string msg)
        {
            Notif?.Invoke(this, new(Cat.None, msg));
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////// ????? ///////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////

        //There are also other awaitable types: special methods such as “Task.Yield” return awaitables that are not Tasks.
        //You can also create your own awaitable(usually for performance reasons), or use extension methods to make a non-awaitable type awaitable.


        //By using concurrent composition(Task.WhenAll or Task.WhenAny), you can perform simple concurrent operations.
        //You can also use these methods along with Task.Run to do simple parallel computation.


        //Old           New         Description
        //task.Wait     await task  Wait/await for a task to complete
        //task.Result   await task  Get the result of a completed task
        //Task.WaitAny  await Task.WhenAny  Wait/await for one of a collection of tasks to complete
        //Task.WaitAll  await Task.WhenAll  Wait/await for every one of a collection of tasks to complete
        //Thread.Sleep  await Task.Delay    Wait/await for a period of time
        //Task ctor     Task.Run or TaskFactory.StartNew    Create a code-based task


        //The normal way to report errors from tasks is by placing an exception on the task.
        //In the most common scenario - an async method that returns Task - your code can just throw an exception
        //and the async machinery will catch that exception and place it on the returned Task for you.So your code
        //can just use throw, try, and catch exactly like normal and it will all work.
    }
}