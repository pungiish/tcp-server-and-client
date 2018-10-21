using System;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string name;
        TcpClient client;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Connect(string name, string ip, int port)
        {
            client = new TcpClient();
            Console.WriteLine("New Thread!");
            // Client connects to the server on the specified ip and port.
            try
            {
                client.Connect(ip, port);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                MessageBoxResult mbr = MessageBox.Show("IP was empty.");
                return;
                throw;
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                MessageBoxResult mbr = MessageBox.Show("Port isn't within the allowed range.");
                return;
                throw;
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                MessageBoxResult mbr = MessageBox.Show("Couldn't access the socket.");
                return;
                throw;
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                MessageBoxResult mbr = MessageBox.Show("TCP Client is closed.");
                return;
                throw;
            }
            try
            {

            }
            catch (System.Threading.Tasks.TaskCanceledException e)
            {
                Thread.CurrentThread.Join();
                throw;
            }

            this.Dispatcher.Invoke(() =>
            {
                btn_connect.IsEnabled = false;
                btn_disconnect.IsEnabled = true;
            });
            // Get client stream for reading and writing.
            // Using is a try finally, if an exception occurs it disposes of the stream.
            using (NetworkStream stream = client.GetStream())
            {
                Send(stream, name, 0);
                while (true)
                {
                    if (client.Connected)
                    {
                        Receive(stream);
                    }
                    else
                    {

                    }
                }
            };


        }
        private void Add_Message(string message)
        {
            try
            {
                //Lambda, for accessing the main Threads GUI from another thread.
                this.Dispatcher.Invoke(() =>
                {
                    CHAT.Text += "\n" + message;
                    CHAT_SCROLL.ScrollToBottom();
                });
            }
            catch (System.Threading.Tasks.TaskCanceledException e)
            {
                Thread.CurrentThread.Join();
                throw;
            }


        }
        private void Send(NetworkStream stream, string message, int header)
        {
            message = message.Insert(0, header.ToString());
            // Parse the message to ASCII and store into a Byte array.
            Byte[] payload = System.Text.Encoding.UTF8.GetBytes(message);
            Console.WriteLine(payload[0]);
            // Send the message
            try
            {
                if (stream.CanWrite)
                {
                    stream.Write(payload, 0, payload.Length);
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                MessageBoxResult mbr = MessageBox.Show("The message size is too big for the buffer.");
                return;
                throw;
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                MessageBoxResult mbr = MessageBox.Show("An error occurred when accessing the socket.");
                //client.GetStream().Flush();
                return;
                throw;
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                MessageBoxResult mbr = MessageBox.Show("There was a failure reading from the network.");
                return;
                throw;
            }
        }
        private string Receive(NetworkStream ns)
        {
            // Byte array for storing the recv message.
            Byte[] data = new Byte[1024];
            // Read the data from the stream, save it's length(for encoding)
            Int32 len;
            try
            {
                if (ns.CanRead)
                {
                    if (client.Connected)
                    {
                        len = ns.Read(data, 0, data.Length);
                        string message = System.Text.Encoding.UTF8.GetString(data, 0, len);
                        Console.WriteLine("Received Message: {0}", message);
                        Add_Message(message);
                        return message;
                    }
                    else
                    {
                        Console.WriteLine("not connected anymore");
                        return null;
                    }

                }
                else
                {
                    return null;
                }
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                MessageBoxResult mbr = MessageBox.Show("You are disconnected.");
                Close_Connection();
                return null;
                throw;
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                MessageBoxResult mbr = MessageBox.Show("There was a failure reading from the network.");
                Close_Connection();
                return null;
                throw;
            }

        }


        private void INPUT_Enter(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(INPUT.Text))
            {

            }
            else
            {

                if (e.Key == Key.Return)
                {
                    Console.WriteLine("enter Entered! {0}", INPUT.Text);
                    if (client.Connected)
                    {
                        Send(client.GetStream(), INPUT.Text, 1);
                        INPUT.Text = "";

                    }
                }
            }
        }
        private void Button_Connect(object sender, RoutedEventArgs e)
        {
            try
            {
                name = NAME.Text;
                string ip = IP.Text;
                Int32 port;
                port = Convert.ToInt32(PORT.Text);


                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBoxResult mbr = MessageBox.Show("Name can't be empty!");
                }
                else
                {
                    // () = delegate
                    Thread t = new Thread(() => Connect(name, ip, port));
                    t.Start();
                    //Connect(ip, port);
                }
            }
            catch (FormatException p)
            {
                Console.WriteLine(p.Message + "\n" + p.StackTrace);
                MessageBoxResult mbr = MessageBox.Show("The input string wasn't in the correct format!");
                return;
                throw;
            }

        }

        private void Button_Disconnect(object sender, RoutedEventArgs e)
        {
            Close_Connection();
        }
        private void Close_Connection()
        {
            client.Close();
            Console.WriteLine(client.Connected);
            this.Dispatcher.Invoke(() =>
            {
                btn_connect.IsEnabled = true;
                btn_disconnect.IsEnabled = false;
            });
        }
    }
}
