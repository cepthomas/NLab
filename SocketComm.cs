using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Ephemera.NBagOfTricks;


// Holding tank for socket stuff.

namespace NLab
{
    public class TcpServer
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
        public TcpServer(int port, byte delim)
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

    public class UdpSender
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
        public UdpSender(int port, byte delim)
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

    internal class Test
    {
        #region Fields
        /// <summary>User input</summary>
        readonly ConcurrentQueue<string> _qUserCli = new();

        /// <summary>LF=10  CR=13  NUL=0</summary>
        byte _delim = 0;

        /// <summary>Config to use</summary>
        string _configFile = "???";

        /// <summary>Target executable</summary>
        string _ntermExe = "???";
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public void Run()
        {
            Console.WriteLine($"========= Test =========");
            _configFile = Path.Combine(MiscUtils.GetSourcePath(), "test_config.ini");
            _ntermExe = Path.Combine(MiscUtils.GetSourcePath(), "..", "bin", "net8.0-windows", "win-x64", "NTerm.exe");

            using CancellationTokenSource ts = new();
            //using Task taskKeyboard = Task.Run(() => _qUserCli.Enqueue(Console.ReadLine() ?? ""));

            try
            {
                // Target flavors run binary NTerm.exe.
                //DoBasicTarget(ts);
                //DoConfigTarget(ts);
                //DoTcpTarget(ts);
                DoUdpTarget(ts);

                // Debugger flavors require starting NTerm with matching cmd line.
                //DoTcpDebugger(ts);
                //DoUdpDebugger(ts);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Fatal!! {e}");
                //Task.WaitAll([taskKeyboard]);
            }
        }

        /// <summary>
        /// Simple first test from cmd line. TODO also tcp/udp?
        /// </summary>
        void DoBasicTarget(CancellationTokenSource ts)
        {
            Console.WriteLine($"DoBasicTarget()");
            List<string> config = ["[nterm]", "comm = null", "delim = NUL", "prompt = >", "meta = -"];
            File.WriteAllLines(_configFile, config);
            var proc = RunTarget(_configFile);
        }

        /// <summary>
        /// Test config functions.
        /// </summary>
        void DoConfigTarget(CancellationTokenSource ts)
        {
            Console.WriteLine($"DoConfigTarget()");
            List<string> config = [
                "[nterm]", "comm = null", "delim = NUL", "prompt = >", "meta = -",
                "info_color = darkcyan", "err_color = green",
            "[macros]", "dox = \"do xxxxxxx\"", "s3 = \"hey, send 333333333\"", "tm = \"  xmagentax   -yellow-  \"",
            "[matchers]", "\"mag\" = magenta", "\"yel\" = yellow"];
            File.WriteAllLines(_configFile, config);
            var proc = RunTarget(_configFile);
        }

        /// <summary>
        /// Test tcp in command/response mode.
        /// </summary>
        void DoTcpTarget(CancellationTokenSource ts)
        {
            Console.WriteLine($"DoTcpTarget()");
            // Tweak config.
            List<string> config = [
                "[nterm]", "comm = tcp 127.0.0.1 59120", "delim = NUL", "prompt = >", "meta = -",
                "info_color = darkcyan", "err_color = green",
            "[macros]", "dox = \"do xxxxxxx\"", "s3 = \"hey, send 333333333\"", "tm = \"  xmagentax   -yellow-  \"",
            "[matchers]", "\"mag\" = magenta", "\"yel\" = yellow"];
            File.WriteAllLines(_configFile, config);
            var proc = RunTarget(_configFile);
            TcpServer srv = new(59120, _delim);
            var err = srv.Run(ts);
        }

        /// <summary>
        /// Test udp in continuous mode.
        /// </summary>
        void DoUdpTarget(CancellationTokenSource ts)
        {
            Console.WriteLine($"DoUdpTarget()");
            // Tweak config.
            List<string> config = [
                "[nterm]", "comm = udp 127.0.0.1 59140", "delim = NUL", "prompt = >", "meta = -",
                "info_color = darkcyan", "err_color = green",
            "[macros]", "dox = \"do xxxxxxx\"", "s3 = \"hey, send 333333333\"", "tm = \"  xmagentax   -yellow-  \"",
            "[matchers]", "\"mag\" = magenta", "\"yel\" = yellow"];
            File.WriteAllLines(_configFile, config);
            var proc = RunTarget(_configFile);
            UdpSender srv = new(59140, _delim);
            srv.Run(ts);
        }

        /// <summary>
        /// Test tcp in command/response mode.
        /// </summary>
        void DoTcpDebugger(CancellationTokenSource ts)
        {
            Console.WriteLine($"DoTcpDebugger()");
            // Runs forever.
            TcpServer srv = new(59120, _delim);
            srv.Run(ts);
        }

        /// <summary>
        /// Test udp in continuous mode.
        /// </summary>
        void DoUdpDebugger(CancellationTokenSource ts)
        {
            Console.WriteLine($"DoUdpDebugger()");
            // Always do once.
            UdpSender srv = new(59140, _delim);
            srv.Run(ts);
        }

        /// <summary>
        /// Run the exe with full user cli.
        /// </summary>
        /// <param name="args"></param>
        Process RunTarget(string args, bool capture = false)
        {
            ProcessStartInfo pinfo = new(_ntermExe, args)
            {
                UseShellExecute = !capture,
                RedirectStandardOutput = capture,
                RedirectStandardError = capture,
            };

            using Process proc = new() { StartInfo = pinfo };

            Console.WriteLine("Start process...");
            proc.Start();

            // if (capture)
            // {
            //     // TIL: To avoid deadlocks, always read the output stream first and then wait.
            //     var stdout = proc.StandardOutput.ReadToEnd();
            //     var stderr = proc.StandardError.ReadToEnd();
            // }

            //Console.WriteLine("Wait for exit...");
            //proc.WaitForExit();
            //Console.WriteLine("Exited...");

            // if (capture)
            // {
            //     return new(proc.ExitCode, stdout, stderr);
            // }

            return proc;
        }
    }

}
