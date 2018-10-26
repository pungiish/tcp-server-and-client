using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace Server
{

    class Program
    {
        static Game gm = new Game();
        static TcpListener listener = null;
        static List<Thread> threads = new List<Thread>();
        static Dictionary<TcpClient, string> clientsDict = new Dictionary<TcpClient, string>();
        static Dictionary<TcpClient, int> scores = new Dictionary<TcpClient, int>();
        static Dictionary<TcpClient, Model.TcpClient> clientsId = new Dictionary<TcpClient, Model.TcpClient>();
        static void Main(string[] args)
        {
            try
            {
                IPAddress ip = IPAddress.Parse("127.0.0.1");
                int port = 1234;
                listener = new TcpListener(ip, port);
                listener.Start();
                Console.WriteLine("Listening on port: {0} \nHost: {1}", port, ip);
                while (true)
                {
                    // lahko bi tudi listener.AcceptSocket();
                    //accept, blokirni klic. čakamo na klienta.
                    //ko se client povezuje.
                    //Waiting for a new client [blocking call]
                    TcpClient client = listener.AcceptTcpClient();
                    // New thread for comm with the client
                    Thread t = new Thread(() => Client(client));
                    t.Start();
                    threads.Add(t);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                listener.Stop();
            }
        }

        static void Client(TcpClient client)
        {
            using (NetworkStream s = client.GetStream())
            {
                string message = Receive(s);
                Model.TcpClient tcp = new Model.TcpClient();
                System.Console.WriteLine("Received message: {0}", message);
                // Header for connecting
                message = message.Remove(0, 1);
                tcp.Name = message;
                tcp.Score = 0;
                clientsId.Add(client, tcp);
                Send("Welcome, [" + message + "] has connected!");
                if (gm.getGameHasStarted())
                {
                    Send("Game has already started, the word we're looking for is: " + gm.getHiddenWord());
                }

                Console.WriteLine("Connected! {0}", client.Client.RemoteEndPoint);
                while (true)
                {
                    if (gm.getGameHasStarted())
                    {
                        while (gm.getGameHasStarted())
                        {
                            string guess = Receive(s);
                            try
                            {
                                guess = guess.Remove(0, 1);
                            }
                            catch (Exception e)
                            {
                                Send("[" + tcp.Name + "] has disconnected!");
                                Console.WriteLine("[{0}] has disconnected {1}", tcp.Name, client.Client.RemoteEndPoint);
                                clientsId.Remove(client);
                                Thread.CurrentThread.Join();
                            }
                            if (guess == gm.getWord())
                            {
                                tcp.Score++;
                                Console.WriteLine("GUESSED!");
                                Send("[" + tcp.Name + "] found out the word was: " + gm.getWord());
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (var cl in clientsId)
                                {
                                    stringBuilder.AppendLine("[" + cl.Value.Name + "] Score: " + cl.Value.Score);
                                }
                                Send(stringBuilder.ToString());
                                gm.pickRandomWord();
                                Send(gm.getHiddenWord());
                                Console.WriteLine(gm.getWord());
                            }
                            else if (guess == "#GAMEEND")
                            {
                                Console.WriteLine("GAMEENDED");
                                Send("[" + tcp.Name + "] Ended the game..");
                                gm.setGameHasStarted(false);
                                break;
                            }
                            else
                            {
                                Send("[" + tcp.Name + "] je ugibal: " + guess);
                            }
                        }
                    }
                    message = Receive(s);
                    if (message == null || message == "")
                    {
                        tcp.Score = 0;
                        clientsId.Remove(client);
                        Send("[" + tcp.Name + "] has disconnected!");
                        Console.WriteLine("[{0}] has disconnected {1}", tcp.Name, client.Client.RemoteEndPoint);
                        Thread.CurrentThread.Join();
                    }
                    try
                    {
                        message.Trim();
                        switch (message[0])
                        {
                            case '1':
                                System.Console.WriteLine("Received message: {0}", message);
                                // Header for a message
                                message = message.Remove(0, 1);
                                if (gm.getGameHasStarted())
                                {
                                    if (message == gm.getWord())

                                    {
                                        scores.TryGetValue(client, out int value);
                                        scores[client] = value + 1;
                                        Console.WriteLine("GUESSED!");
                                        Send("[" + tcp.Name + "] found out the word was: " + gm.getWord());
                                        StringBuilder stringBuilder = new StringBuilder();
                                        foreach (var cl in clientsId)
                                        {
                                            stringBuilder.AppendLine("[" + cl.Value.Name + "] Score: " + cl.Value.Score);
                                        }
                                        Send(stringBuilder.ToString());
                                        gm.pickRandomWord();
                                        Send(gm.getHiddenWord());
                                        Console.WriteLine(gm.getWord());
                                    }
                                    else if (message == "#GAMEEND")
                                    {
                                        Console.WriteLine("GAMEENDED");
                                        Send("[" + tcp.Name + "] Ended the game..");
                                        gm.setGameHasStarted(false);
                                        break;
                                    }
                                    else
                                    {
                                        Send("[" + tcp.Name + "] je ugibal: " + message);
                                    }
                                }
                                else
                                {
                                    Send(tcp.Name + " je rekel: " + message);
                                }
                                break;
                            case '2':
                                message = message.Remove(0, 1);
                                Send("[" + tcp.Name + "] je začel igro");
                                gm.gameStart();
                                gm.setGameHasStarted(true);
                                gm.pickRandomWord();
                                string word = gm.getHiddenWord();
                                Console.WriteLine(gm.getWord());
                                Send(word);
                                break;
                            case '3':
                                //case for special commands
                                break;
                        }
                    }
                    catch (System.IndexOutOfRangeException e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine("Client Disconnected: " + client.Client.RemoteEndPoint);
                        clientsId.Remove(client);
                        s.Close();
                        client.Close();
                        Thread.CurrentThread.Join();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }

        static string Receive(NetworkStream ns)
        {
            try
            {
                Byte[] recv = new Byte[1024];
                do
                {
                    int len = ns.Read(recv, 0, recv.Length);
                    string message = System.Text.Encoding.UTF8.GetString(recv, 0, len);
                    message = ENCRYPT(message);
                    return message;
                } while (ns.DataAvailable);
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine("The underlying socket is closed! \n {0},\n {1}", e.Message, e.StackTrace);
                return null;

            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine("A client has disconnected!");
                Console.WriteLine("There was an error reading from network. \n {0}, \n {1}", e.Message, e.StackTrace);
                return null;

            }
            catch (Exception e)
            {
                Console.WriteLine("Error while receiving, {0}, {1}", e.Message, e.StackTrace);
                return null;
            }
        }

        static void Send(string message)
        {
            try
            {
                //inicializiraj byte arr. Dodaj notri message v Byteih
                Console.WriteLine(message);
                message = ENCRYPT(message);
                Byte[] vs = System.Text.Encoding.UTF8.GetBytes(message);
                Console.WriteLine(message);
                foreach (TcpClient cl in clientsId.Keys)
                {
                    NetworkStream ns = cl.GetStream();
                    ns.Write(vs, 0, vs.Length);
                }
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine("An error occurred when accessing the socket. \n {0} \n {1}", e.Message, e.StackTrace);
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine("The Network stream is closed. \n {0} \n {1}", e.Message, e.StackTrace);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Invalid operation exception {0} \n {1}", e.Message, e.StackTrace);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while sending: {0} , {1}", e.Message, e.StackTrace);
            }
        }

        static string ENCRYPT(String message)
        {
            Encryption encryption = new Encryption();
            return encryption.EncryptDecrypt(message);
        }

    }
}
