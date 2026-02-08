# NLab

Just a big messy area. These are not the codes you are looking for.

![owk](owk.jpg)


// lots from - https://markheath.net/post/starting-threads-in-dotnet


//One of the easier methods is to use Task.Run, very similar to your existing code.
//However, I do not recommend implementing a CalculateAsync method since that implies the
//processing is asynchronous (which it is not). Instead, use Task.Run at the point of the call.
//was async Task AwaitableBackgroundTask(string state)
//And if from your UI thread you want to offload work to a worker thread, and you use Task.Run to do so,
//you often typically want to do some work back on the UI thread once that background work is done,
//and these language features make that kind of coordination easy and seamless.
