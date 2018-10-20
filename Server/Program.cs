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
        static TcpClient client = null;
        static List<TcpClient> clients = new List<TcpClient>();
        static List<Thread> threads = new List<Thread>();
        static Dictionary<TcpClient,string> clientsDict = new Dictionary<TcpClient,string>();
        static void Client()
        {
            // lahko bi tudi listener.AcceptSocket();
            //accept, blokirni klic. čakamo na klienta.
            //ko se client povezuje.
            using (NetworkStream s = client.GetStream())
            {
                Console.WriteLine("Connected! {0}", client.Client.RemoteEndPoint);
                while (true)
                {
                    string message = null;
                    message = Receive(s);
                    switch (message[0])
                    {
                        case '0':
                            System.Console.WriteLine("Received message: {0}", message);
                            message = message.Remove(0, 1);
                            clientsDict.Add(client,message);
                            // Header for connecting
                            Send("Welcome, " + message + " has connected!\n");
                            break;
                        case '1':
                            System.Console.WriteLine("Received message: {0}", message);
                            // Header for a message
                            message = message.Remove(0, 1);
                            string name;
                            clientsDict.TryGetValue(client, out name);

                            Send(name + " je rekel: " + message + "\n");
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            try
            {
                IPAddress ip = IPAddress.Parse("127.0.0.1");
                int port = 1234;
                listener = new TcpListener(ip, port);
                listener.Start();
                Console.WriteLine("LISTENER STARTED");
                while (true)
                {
                    try
                    {

                        //Waiting for a new client [blocking call]
                        client = listener.AcceptTcpClient();
                        clients.Add(client);
                        Console.WriteLine(clients[0]);
                        Console.WriteLine("Listener accepted the client!");
                        // New thread
                        Thread t = new Thread(new ThreadStart(Client));
                        t.Start();
                        threads.Add(t);


                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message + "\n" + e.StackTrace);
                        client.Close();
                        listener.Stop();
                    }
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
        static string Receive(NetworkStream ns)
        {
            try
            {
                Byte[] recv = new Byte[1024];
                int len = ns.Read(recv, 0, recv.Length);
                return System.Text.Encoding.UTF8.GetString(recv, 0, len);
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


                foreach (TcpClient client in clients)
                {
                    NetworkStream ns = client.GetStream();
                    ns.Write(vs, 0, vs.Length);
                }
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
