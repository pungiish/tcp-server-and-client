using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace Server
{

    class Program
    {
        static TcpListener listener = null;
        static List<TcpClient> clients = new List<TcpClient>();
        static List<Thread> threads = new List<Thread>();
        static Dictionary<TcpClient, string> clientsDict = new Dictionary<TcpClient, string>();


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
                    clients.Add(client);
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
                System.Console.WriteLine("Received message: {0}", message);
                // Header for connecting
                message = message.Remove(0, 1);
                clientsDict.Add(client, message);
                // Ustvari nov thread za pošiljanje?
                Send("Welcome, [" + message + "] has connected!");

                Console.WriteLine("Connected! {0}", client.Client.RemoteEndPoint);
                while (true)
                {
                    //if (client.Connected)
                    //{
                    message = Receive(s);
                    if (message == null || message == "")
                    {
                        string name;
                        clientsDict.TryGetValue(client, out name);
                        clientsDict.Remove(client);
                        Send("[" + name + "] has disconnected!");
                        Console.WriteLine("[{0}] has disconnected {1}", name ,client.Client.RemoteEndPoint);
                        Thread.CurrentThread.Join();
                    }
                    //}
                    //else
                    //{
                    //  Thread.CurrentThread.Join();
                    //break;
                    //}
                    try
                    {
                        switch (message[0])
                        {
                            case '1':
                                System.Console.WriteLine("Received message: {0}", message);
                                // Header for a message
                                message = message.Remove(0, 1);
                                string name;
                                clientsDict.TryGetValue(client, out name);

                                Send(name + " je rekel: " + message);
                                break;
                        }
                    }
                    catch (System.IndexOutOfRangeException e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine("Client Disconnected: " + client.Client.RemoteEndPoint);
                        clientsDict.Remove(client);
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
                    return System.Text.Encoding.UTF8.GetString(recv, 0, len);
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
                Byte[] vs = System.Text.Encoding.UTF8.GetBytes(message);

                foreach (KeyValuePair<TcpClient, string> client in clientsDict)
                {
                    NetworkStream ns = client.Key.GetStream();
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

    }
}
