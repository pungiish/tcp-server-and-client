using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Threading;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string name;
        TcpClient client = new TcpClient();
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Connect(string name, string ip, int port)
        {
            Console.WriteLine("New Thread!");
            // Client connects to the server on the specified ip and port.
            try
            {
                client.Connect(ip, port);
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
                        Receive(stream);
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
            };
                
            
        }
        private void Add_Message(string message)
        {
            //Lambda, for accessing the main Threads GUI.
            this.Dispatcher.Invoke(() =>
            {
                CHAT.Text += "\n" + message;
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
            catch(Exception e)
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
            catch (Exception e)
            {
                Console.WriteLine("Exception caught! {0}\n {1}", e.Message, e.StackTrace);
                return null;
            }
        }
        private void Button_Connect(object sender, RoutedEventArgs e)
        {
            name = NAME.Text;
            string ip = IP.Text;
            Int32 port;
            port = Convert.ToInt32(PORT.Text);

            System.Console.WriteLine("Connecting to IP: {0} on PORT: {1}", ip, port);
            // () = delegate
            Thread t = new Thread(() => Connect(name, ip, port));

            t.Start()   ;
            //Connect(ip, port);

        }

        private void INPUT_Enter(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Console.WriteLine("enter Entered! {0}", INPUT.Text);
                Send(client.GetStream(), INPUT.Text, 1);
                INPUT.Text = "";
            }
        }
    }
}
