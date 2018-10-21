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
                this.Dispatcher.Invoke(() =>
                {
                    btn_connect.IsEnabled = false;
                    btn_disconnect.IsEnabled = true;
                });
            }
            catch (SocketException e)
            {
                Console.WriteLine("Socket exception occured! " + e.Message + "\n" + e.StackTrace);
                client.Close();
            }
            // Get client stream for reading and writing.
            using (NetworkStream stream = client.GetStream())
            {
                Send(stream, name, 0);
                try
                {
                    while (true)
                    {
                        if (client.Connected)
                        {
                            Receive(stream);
                        }
                        else
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                btn_connect.IsEnabled = true;
                                btn_disconnect.IsEnabled = false;
                            });
                            Thread.CurrentThread.Join();
                            break;
                        }
                    }
                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine("ArgumentNullException: {0}", e);
                }
                catch (SocketException e)
                {
                    Console.WriteLine("Exception");
                    Console.WriteLine("SocketException: {0}", e);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Undetermined exception \n {0} \n {1}", e.Message, e.StackTrace);

                }
            };


        }
        private void Add_Message(string message)
        {
            //Lambda, for accessing the main Threads GUI from another thread.
            this.Dispatcher.Invoke(() =>
            {
                CHAT.Text += "\n" + message;
                CHAT_SCROLL.ScrollToBottom();
            });
        }
        private void Send(NetworkStream stream, string message, int header)
        {
            try
            {
                message = message.Insert(0, header.ToString());
                // Parse the message to ASCII and store into a Byte array.
                Byte[] payload = System.Text.Encoding.UTF8.GetBytes(message);
                Console.WriteLine(payload[0]);
                // Send the message

                stream.Write(payload, 0, payload.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while sending! {0} \n {1} ", e.Message, e.StackTrace);
            }
        }
        private string Receive(NetworkStream ns)
        {
            try
            {
                // Byte array for storing the recv message.
                Byte[] data = new Byte[1024];
                // Read the data from the stream, save it's length(for encoding)
                Int32 len = ns.Read(data, 0, data.Length);
                string message = System.Text.Encoding.UTF8.GetString(data, 0, len);
                Console.WriteLine("Received Message: {0}", message);
                Add_Message(message);
                return message;
            }
            catch (System.IO.IOException e)
            {
                // User disconnected;
                // Close the stream, client, dispose
                Console.WriteLine("IOException caught! {0}\n {1}", e.Message, e.StackTrace);
                this.Dispatcher.Invoke(() =>
                {
                    btn_connect.IsEnabled = true;
                    btn_disconnect.IsEnabled = false;
                });
                ns.Close();
                client.Close();
                client.Dispose();
                Console.WriteLine("Joining thread");
                Thread.CurrentThread.Join();
                return null;
            }
            catch (System.Threading.Tasks.TaskCanceledException e)
            {
                // User forcibly closed their client;
                // Send disconnect notice, close the stream, client, dispose.


                Console.WriteLine("Threading Exception caught! {0}\n {1}", e.Message, e.StackTrace);
                ns.Close();
                client.Close();
                client.Dispose();
                Thread.CurrentThread.Join();
                return null;
            }
        }
        private void Button_Connect(object sender, RoutedEventArgs e)
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

        private void INPUT_Enter(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
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
            catch (Exception exception)
            {
                Console.WriteLine("Exception has been raised, { 0 }, \n { 1 }", exception.Message, exception.StackTrace);
                Thread.Sleep(1000);
                System.Windows.Application.Current.Shutdown(0);
            }

        }

        private void Button_Disconnect(object sender, RoutedEventArgs e)
        {
            client.GetStream().Close();
            client.Close();
        }
    }
}
