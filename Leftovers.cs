using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ephemera.NBagOfTricks;


// TODO clean up all this

namespace NLab
{
    static public class Leftovers
    {
        static public List<string> DumpStack(string preamble = "")
        {
            // Get the caller info.
            var cinfo = new List<string>();
            var st = new StackTrace(true);
            int index = 0;
            while (index >= 0)
            {
                var frm = st.GetFrame(index);
                if (frm == null)
                {
                    index = -999; // done
                }
                else if (frm.GetFileName() is not null && frm.HasSource())
                {
                    var sfrm = $"{preamble}{index}: {frm.GetMethod().Name} in {frm.GetFileName()}({frm.GetFileLineNumber()})";
                    cinfo.Add(sfrm);
                    index++;
                }
                else
                {
                    index++;
                }
            }

            //return string.Join($"{Environment.NewLine}", cinfo);
            return cinfo;
        }

        public static List<string> Dump()
        {
           List<string> res = [];
           //_itemds.ForEach(itemd => res.Add(itemd.Item.ToString()));
           return res;
        }
        // >>>>
        public static IEnumerable<U> Map<T, U>(this IEnumerable<T> s, Func<T, U> f)
        {
           foreach (var item in s)
               yield return f(item);
        }
    }

    /// <summary>Custom rectangle for this application.</summary>
    public class DisplayRect
    {
        public int Left { get; init; } = -1;
        public int Top { get; init; } = -1;
        public int Right { get; init; } = -1;
        public int Bottom { get; init; } = -1;
        public Rectangle WinRect { get { return new Rectangle(Left, Top, Right - Left, Bottom - Top); } }
        public bool IsValid { get; init; } = false;

        /// <summary>Default constructor - invalid.</summary>
        public DisplayRect()
        {
            IsValid = false;
        }

        /// <summary>Normal constructor.</summary>
        public DisplayRect(int left, int top, int width, int height)
        {
            IsValid = top >= 0 && left >= 0 && width >= 0 && height >= 0;
            if (!IsValid) throw new ArgumentException("Invalid args");
            Left = left;
            Top = top;
            Right = left + width;
            Bottom = top + height;
        }

        /// <summary>Read me.</summary>
        public override string ToString()
        {
            return IsValid ? $"L:{Left} T:{Top} R:{Right} B:{Bottom}" : "Invalid";
        }
    }

    class DelegateLambda
    {
        // Delegates are really just structural typing for functions. You could do the same thing with nominal typing and 
        // implementing an anonymous class that implements an interface or abstract class, but that ends up being a lot of 
        // code when only one function is needed.

        // Lambda comes from the idea of lambda calculus of Alonzo Church in the 1930s. It is an anonymous way of creating 
        // functions. They become especially useful for composing functions

        // So while some might say lambda is syntactic sugar for delegates, I would says delegates are a bridge for easing 
        // people into lambdas in c#.

        // One difference is that an anonymous delegate can omit parameters while a lambda must match the exact signature. Given:
        public delegate string TestDelegate(int i);

        public void Test(TestDelegate d) { }

        // you can call it in the following four ways (note that the second line has an anonymous delegate that does not have any parameters):
        void Callit()
        {
            Test(delegate (int i) { return string.Empty; });
            Test(delegate { return string.Empty; });
            Test(i => string.Empty);
            Test(D);
        }

        private string D(int i)
        {
            return string.Empty;
        }

        private string D2()
        {
            return string.Empty;
        }

        // You cannot pass in a lambda expression that has no parameters or a method that has no parameters.
        // These are not allowed:
        //   Test(() => String.Empty); // Not allowed, lambda must match signature
        //   Test(D2); // Not allowed, method must match signature
    }


    class TcpServerAsync
    {
        //  https://stackoverflow.com/a/53403824   c# 7.0 in a nutshell
        const int packet_length = 2;  // user defined packet length

        void DoAsync()
        {
            RunServerAsync();
        }

        async void RunServerAsync()
        {
            var listner = new TcpListener(IPAddress.Any, 59120);
            listner.Start();
            try
            {
                while (true)
                {
                    TcpClient client = await listner.AcceptTcpClientAsync();
                    await Accept(client);
                }
            }
            finally
            {
                listner.Stop();
            }
        }

        async Task Accept(TcpClient client)
        {
            await Task.Yield();
            try
            {
                using (client)
                using (NetworkStream n = client.GetStream())
                {
                    byte[] data = new byte[packet_length];
                    int bytesRead = 0;
                    int chunkSize = 1;

                    while (bytesRead < data.Length && chunkSize > 0)
                    {
                        bytesRead += chunkSize = await n.ReadAsync(data, bytesRead, data.Length - bytesRead);
                    }

                    // get data
                    string str = Encoding.Default.GetString(data);
                    Console.WriteLine("[server] received : {0}", str);

                    // To do
                    // ...

                    // send the result to client
                    string send_str = "server_send_test";
                    byte[] send_data = Encoding.ASCII.GetBytes(send_str);
                    await n.WriteAsync(send_data, 0, send_data.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public class TcpServerStuff
    {
        #region Fields
        readonly string _host;
        readonly int _port;
        readonly byte _delim;
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="delim"></param>
        /// <param name="ts"></param>
        public TcpServerStuff(int port, byte delim)
        {
            _port = port;
            _delim = delim;
            _host = "127.0.0.1";

            Console.WriteLine($"Tcp using {_host}:{_port}");
        }

        /// <summary>
        /// Test tcp in command/response mode.
        /// </summary>
        public bool Run(CancellationTokenSource _ts)
        {
            bool err = false;

            while (!_ts.Token.IsCancellationRequested)
            {
                try
                {
                    //=========== Connect ============//
                    //https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener

                    using var server = TcpListener.Create(_port);
                    server.Start();

                    using var client = server.AcceptTcpClient(); // TODO? AcceptTcpClientAsync(token)
                    Console.WriteLine("Client has connected");
                    using var stream = client.GetStream();


                    //=========== Receive ============//
                    string? cmd = null;
                    var rx = new byte[256]; // Max rx message for test.
                    var numRead = stream.Read(rx, 0, rx.Length); // blocks

                    if (numRead > 0)
                    {
                        for (int i = 0; i < numRead; i++)
                        {
                            if (rx[i] == _delim)
                            {
                                // Convert the received data to a string.
                                cmd = Encoding.Default.GetString(rx, 0, i);
                            }
                        }
                    }


                    //=========== Respond ============//
                    List<string>? response = null;

                    switch (cmd)
                    {
                        case null:
                            response = ["Bad delimiter (probably)"];
                            break;

                        case "l": // large payload - continuous
                            var tf = Path.Combine(MiscUtils.GetSourcePath(), "ross_2.txt");
                            response = [.. File.ReadAllLines(tf).ToList()];
                            break;

                        case "s": // small payload
                            response = ["Everything's not great in life, but we can still find beauty in it."];
                            break;

                        case "e": // echo
                            response = [$"You sent [{cmd}]"];
                            break;

                        case "c": // ansi color
                            response = [$"Colors!!! \u001b[91mRED \u001b[92mGREEN \u001b[94mBLUE \u001b[0mNONE"];
                            break;

                        case "q":
                            response = ["Goodbye!"];
                            _ts.Cancel();
                            break;

                        default: // Always respond with something to prevent timeouts.
                            response = [$"Unknown cmd [{cmd}]"];
                            break;
                    }

                    Console.WriteLine($"cmd [{cmd}] response [{response[0]}]");

                    if (response is not null && response.Count > 0)
                    {
                        // Pace response messages. Simulates continuous operationn too.
                        int ind = 0;
                        while (!_ts.Token.IsCancellationRequested)
                        {
                            string send = response[ind];
                            byte[] bytes = [.. Encoding.Default.GetBytes(send), _delim];
                            stream.Write(bytes, 0, bytes.Length);
                            ind += 1;
                            if (ind >= response.Count)
                            {
                                //_ts.Cancel();
                                break;
                            }
                            else
                            {
                                // Pacing.
                                Thread.Sleep(ind % 10 == 0 ? 500 : 5);
                            }
                        }
                    }

                    // System.Threading.Thread.Sleep(10);
                }
                catch (Exception e)
                {
                    // Log, reset, keep going.
                    Console.WriteLine($"Exception: {e}");
                    //server?.Stop();
                    // err = true;
                    // _ts.Cancel();
                }
            }

            return err;
        }
    }

    public class UdpSenderStuff
    {
        #region Fields
        readonly string _host;
        readonly int _port;
        readonly byte _delim;
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="delim"></param>
        public UdpSenderStuff(int port, byte delim)
        {
            _port = port;
            _delim = delim;
            _host = "127.0.0.1";

            Console.WriteLine($"Udp using {_host}:{_port}");
        }

        /// <summary>
        /// Do one broadcast cycle.
        /// </summary>
        public void Run(CancellationTokenSource ts)
        {
            bool done = false;

            while (!done && !ts.Token.IsCancellationRequested)
            {
                try
                {
                    var tf = Path.Combine(MiscUtils.GetSourcePath(), "ross_2.txt");
                    var lines = File.ReadAllLines(tf).ToList();

                    //=========== Connect ============//
                    using UdpClient client = new();
                    client.Connect(_host, _port);
                    Console.WriteLine("Client has connected");

                    //=========== Send ===============//
                    // Pace response messages to simulate continuous operationn.
                    int ind = 0;
                    while (!done && !ts.Token.IsCancellationRequested)
                    {
                        string send = lines[ind];
                        byte[] bytes = [.. Encoding.Default.GetBytes(send), _delim];
                        client.Send(bytes, bytes.Length);
                        ind += 1;
                        if (ind >= lines.Count)
                        {
                            done = true;
                            //_ts.Cancel();
                        }
                        else
                        {
                            // Pacing.
                            Thread.Sleep(ind % 10 == 0 ? 500 : 5);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception: {e}");
                    done = true;
                    // _ts.Cancel();
                }
            }

            Console.WriteLine($"Udp done");
        }
    }



    
}
