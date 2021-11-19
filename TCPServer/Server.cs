using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using Protocol;

namespace TcpServer
{
    class Server
    {
        private Socket socketServer;
        private List<ClientOnServer> clients;

        public Server()
        {
            socketServer = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            clients = new List<ClientOnServer>();
        }

        public void Bind(string sIp, int nPort)
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(sIp), nPort);
            socketServer.Bind(ep);
            socketServer.Listen(30);
        }

        public void Close()
        {
            socketServer.Close();
        }

        public void StartAccept()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadAccept));
        }
        public void Test() 
        {
            { 

                for (int i = 0; i < clients.Count; i++)
                    {

                        if (!clients[i].Test()) 
                            {
                                Console.WriteLine("Клиент " + clients[i].name + " покинул сервер");
                                clients[i].Close();      
                                clients.RemoveAt(i--);   

                                Packet packetList = new Packet(PacketType.ClientList, clients.Count);
                                for (int j = 0; j < clients.Count; j++)
                                    {
                                        packetList.SetItem(j, clients[j].name);
                                    }
                                for (int j = 0; j < clients.Count; j++)
                                    {
                                        clients[j].Send(packetList);
                                    }
                            }
                    }
            }
        }
        private void ThreadAccept(object ob)
        {
            while (true)
            {
                Socket socketClient = socketServer.Accept();

                ClientOnServer client = new ClientOnServer(socketClient, this);
                client.StartReceive();
                client.name = "Неизвестный пользователь";       
                clients.Add(client);
                Console.WriteLine("Клиент №" + clients.Count.ToString() + " присоединился");
            }
        }

        public void ProcessPacket(Packet packet, ClientOnServer client)
        {
            switch (packet.Type)
            {
                case PacketType.SimpleMessage:
                    {
                        string sName = packet.GetItem(0);
                        string sText = packet.GetItem(1);
                        int i;
                        for (i = 0; i < clients.Count; i++)
                            if (clients[i].name == sName)
                            {
                                packet.SetItem(0, client.name); 
                                clients[i].Send(packet);        
                                break;
                            }
                    }
                    break;
                case PacketType.Login:
                    {
                        
                        string newName = packet.GetItem(0); 
                        int i;
                        for (i = 0; i < clients.Count; i++)
                            if (clients[i].name == newName) break;

                        if (i < clients.Count)                           
                        {
                            client.name = newName;
                            Console.WriteLine("Повторная попытка занятия логина " + newName);
                            client.SendLoginResult(false); 
                            
                        }
                        else
                        {
                            client.name = newName;
                            Console.WriteLine("Клиент " + newName + " зарегистрирован");
                            client.SendLoginResult(true); 

                            Packet packetList = new Packet(PacketType.ClientList, clients.Count);
                            for (i = 0; i < clients.Count; i++)///
                                {
                                    packetList.SetItem(i, clients[i].name);
                                }

                            for (i = 0; i < clients.Count; i++)
                                {
                                    clients[i].Send(packetList);
                                }

                            Console.WriteLine("Отправлен список клиентов");
                        }
                    }
                    break;
            }

        }
    }
}
