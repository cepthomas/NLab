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
using System.Threading.Tasks;
using Ephemera.NBagOfTricks;


// Holding tank for socket stuff.

namespace NLab
{
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
                    // was await Accept(await listner.AcceptTcpClientAsync());
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

    class TcpClientAsync
    {
        public async Task GoGo() // ==> was Main(string[] args)
        {
            string _host = "aaaa";
            int _port = 90909;

            using var cts = new CancellationTokenSource();

            // Handle Ctrl+C gracefully
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                await RunClientAsync(_host, _port, cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Client shutdown initiated by user.");
            }
        }

        async Task RunClientAsync(string ip, int port, CancellationToken cancellationToken)
        {
            // 1. Instantiate and connect asynchronously
            using TcpClient client = new TcpClient();
            Console.WriteLine($"Connecting to {ip}:{port}...");
            await client.ConnectAsync(ip, port, cancellationToken);
            Console.WriteLine("Connected to server!");

            // 2. Get the communication stream
            using NetworkStream stream = client.GetStream();

            // 3. Start a background task to continuously read server messages
            Task receiveTask = ReceiveMessagesAsync(stream, cancellationToken);

            // 4. Main loop for sending data from console input
            Console.WriteLine("Type messages and press Enter to send (or 'exit' to quit):");
            while (!cancellationToken.IsCancellationRequested)
            {
                string? message = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(message)) continue;
                if (message.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

                // Convert string to bytes and send
                byte[] data = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length, cancellationToken);
            }

            // Clean up connection
            client.Close();
        }

        async Task ReceiveMessagesAsync(NetworkStream stream, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Read incoming bytes asynchronously
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                    // If ReadAsync returns 0, the server closed the connection gracefully
                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Server disconnected.");
                        break;
                    }

                    // Decode and print the message
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"[Server]: {response}");
                }
            }
            catch (Exception ex) when (ex is ObjectDisposedException || ex is IOException)
            {
                // Expected exceptions when the connection drops or is closed intentionally
                Console.WriteLine("Connection lost.");
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
