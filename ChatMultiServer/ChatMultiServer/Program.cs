using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static System.Console;
namespace ChatMultiServer
{
    class Program
    {
        public static Hashtable clientList = new Hashtable();
        static void Main(string[] args)
        {
            TcpListener serverSocket = new TcpListener(8888);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;

            serverSocket.Start();
            WriteLine("Chat Server Started... ");

            while (true)
            {
                clientSocket = serverSocket.AcceptTcpClient();
                counter++;

                byte[] byteFrom = new byte[(int)clientSocket.ReceiveBufferSize];
                string dataFromClient = null;

                NetworkStream networkStream = clientSocket.GetStream();
                networkStream.Read(byteFrom, 0, (int)clientSocket.ReceiveBufferSize);
                dataFromClient = System.Text.Encoding.ASCII.GetString(byteFrom);
                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                clientList.Add(dataFromClient, clientSocket);

                broadcast(dataFromClient + " Joined", dataFromClient, false);

                WriteLine(dataFromClient + " Joined chat room!!!");

                handleClient client = new handleClient();
                client.startClient(clientSocket,dataFromClient);
            }//while

        }//Main 
        public static void broadcast(string msg, string uName, bool flag)
        {
            foreach(DictionaryEntry item in clientList)
            {
                TcpClient broadcastSocket;
                broadcastSocket = (TcpClient)item.Value;
                NetworkStream broadcastStream = broadcastSocket.GetStream();
                byte[] broadcastByte = null;

                if (flag)
                    broadcastByte =  Encoding.ASCII.GetBytes(uName + " says : " + msg);
                else
                    broadcastByte = Encoding.ASCII.GetBytes(msg);
                broadcastStream.Write(broadcastByte, 0, broadcastByte.Length);
                broadcastStream.Flush();
                
            }
        }//broadcast

    }
    public class handleClient
    {
        TcpClient clientSocket;
        string clientId;
        public void startClient(TcpClient client, string clientId)
        {
            this.clientSocket = client;
            this.clientId = clientId;

            Thread ctThread =  new Thread(doChat);
            ctThread.Start();
        }
        public void doChat()
        {
            byte[] byteFrom = new byte[(int)clientSocket.ReceiveBufferSize];
            string dataFromClient = null;
            byte[] sendBytes = null;
            string serverResponse = null;

            while (true)
            {
                NetworkStream networkStream = clientSocket.GetStream();
                networkStream.Read(byteFrom,0,(int)clientSocket.ReceiveBufferSize);

                dataFromClient = System.Text.Encoding.ASCII.GetString(byteFrom);
                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                WriteLine("From client - " + clientId + " : " + dataFromClient);

                Program.broadcast(dataFromClient, clientId, true);

            }
        }
    }
}
