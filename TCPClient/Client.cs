using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using Protocol;

namespace TcpClient
{
    class Client
    {
        
        public string client_login;   
        public int logState; 
        public bool marker = true;                       
        public List<string> clients; 

        private Socket socket;

        public Client()
        {
            socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            logState = 0;///
            clients = new List<string>();
        }

        public void Connect(string sIp, int nPort)
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(sIp), nPort);

            while (!socket.Connected)
            {
                try
                {
                    socket.Connect(ep);
                }
                catch (SocketException e)
                {
                    Thread.Sleep(50);
                }
            }
        }

        public void StartReceive()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadReceive));
        }

        void ThreadReceive(object ob)
        {
            try
            {
                byte[] buffer = new byte[1024];
                while (true)
                {
                    // receive message
                    int nRecv = socket.Receive(buffer);

                    Packet packet = Packet.ParseBytes(buffer);
                    ProcessPacket(packet);
                }
            }
            catch (SocketException) {; }// обрабатываю исключение (если сокета уже нет)

        }

        private void ProcessPacket(Packet packet)
        {
            switch (packet.Type)
            {
                case PacketType.SimpleMessage:
                    {
                        // считываем и выводим имя отправителя и текст сообщения
                        string client_login = packet.GetItem(0);
                        string sText = packet.GetItem(1);
                        Console.WriteLine("\n" + client_login + ": " + sText);
                    }
                    break;
                case PacketType.ClientList:
                    {
                        // вывод в консоль списка подключенных клиентов

                        clients.Clear();//////очищаю список клиентов
                        Console.WriteLine("\nПодключенные клиенты:");
                        for (int i = 0; i < packet.ItemCount; i++)
                        {
                            Console.WriteLine(packet.GetItem(i));
                            clients.Add(packet.GetItem(i));/////запоминаю список клиентов
                        }
                    }
                    break;
                case PacketType.Login:
                    {
                        // считать и обработать ответ сервера на запрос имени клиента
                        // если имя принято - allow, запомнить его и перейти к вводу сообщений
                        // если не принято - deny, остаться в цикле ввода имени
                        // to-do...
                        string status = packet.GetItem(0);

                        if (status == "allow") logState = 2;///loginState будет обработан в основном цикле
                        else
                            {
                                logState = 0;
                                marker = false;
                        }
                    }
                    break;

            }

        }

        public void Send(Packet packet)
        {
            byte[] bufferSend = packet.ToBytes();
            socket.Send(bufferSend);
        }

        // для удобства отправки простого сообщения
        public void SendSimpleMessage(string sTo, string sText)
        {
            Packet packet = new Packet(PacketType.SimpleMessage, 2);
            packet.SetItem(0, sTo);
            packet.SetItem(1, sText);
            Send(packet);
        }
        // для удобства отправки имени серверу на проверку
        public void SendLogin(string client_login)
        {
            Packet packet = new Packet(PacketType.Login, 1);
            packet.SetItem(0, client_login);
            Send(packet);
        }
        public bool Test() //2 мониторинг подключенного сервера
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0); // состояние сокета
            }
            catch (SocketException) { return false; } // обрабатываю исключение (если сокета нет)

        }
        public void Close()
        {
            socket.Close();
        }
    }
}
