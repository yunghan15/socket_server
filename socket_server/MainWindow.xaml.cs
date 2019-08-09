using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace socket_server
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        string LocalIP = "127.0.0.1";
        int SPort = 8989;

        // socket
        static Socket serverSocket = null;
        static List<Socket> sockets = new List<Socket>();

        public MainWindow()
        {
            InitializeComponent();

        }


        //Listen to request from client  
        public void ListenClientConnect()
        {
            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                sockets.Add(clientSocket);
                
                // new thread
                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start(clientSocket);
            }
        }

        // recieve msg from client
        public void ReceiveMessage(object clientSocket)
        {
            Socket connection = (Socket)clientSocket;
            while (true)
            {
                try
                {
                    byte[] result = new byte[1024];
                    // recieve data through clientSocket  
                    int receiveNumber = connection.Receive(result);
                    // convert to string
                    String recStr = Encoding.ASCII.GetString(result, 0, receiveNumber);


                    //get client IP
                    IPAddress clientIP = (connection.RemoteEndPoint as IPEndPoint).Address;
                    //get client Port
                    int clientPort = (connection.RemoteEndPoint as IPEndPoint).Port;
                    String sendStr = clientIP + ":" + clientPort.ToString() + "--->" + recStr;
                    foreach (Socket socket in sockets)
                    {
                        socket.Send(Encoding.ASCII.GetBytes(sendStr));
                    }
                    // display
                    text1.Dispatcher.BeginInvoke(new Action(() => { text1.Text += "\r" + sendStr; }), null);

                }
                catch (Exception ex)
                {

                    connection.Shutdown(SocketShutdown.Both);
                    connection.Close();
                    break;
                }
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IPAddress ip = IPAddress.Parse(LocalIP);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ip, SPort));  //Port  
            serverSocket.Listen(10);             
            
            //Clientsoket start  
            Thread myThread = new Thread(ListenClientConnect);
            myThread.Start();

        }           
    }
}
