using System;
using System.Text;


namespace TcpClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client();
            client.Connect("192.168.0.102", 2002);
            Console.WriteLine("Добро пожаловать! Вы присоединились к серверу\n");
            client.StartReceive();
                       
            string name = "";     
            while (true)                
            {
                if (client.logState == 0)
                {
                    if (client.marker == false)
                        Console.WriteLine("Этот логин занят, введите другой");
                    else
                        Console.WriteLine("Ваш логин :");          
                    name = Console.ReadLine();
                    if (name == "") break;
                    client.logState = 1;
                    client.SendLogin(name);
                }
                else 
                    if (client.logState == 1)
                    {
                        System.Threading.Thread.Sleep(100); 
                        
                }    
 
                else
                    if (client.logState == 2)  
                    {
                        client.client_login = name;
                        break;
                    }
            }

            if (client.logState == 2) 
            {
                Console.Write("\n***Начало чата***\n");

                while (true)
                {
                    System.Threading.Thread.Sleep(100);
                    Console.Write("\nВведите имя адресата\n");
                    string recip = Console.ReadLine();
                    if (recip == "") 
                        break;     

                    if (client.clients.Contains(recip))
                                                         
                    {
                        Console.Write("\nВведите текст сообщения\n");
                        string Text = Console.ReadLine();
                        client.SendSimpleMessage(recip, Text);
                    }
                    else
                    {
                        Console.Write("Ошибка: клиента " + recip + " нет в сети\n");
                    }

                    if (!client.Test()) break;  
                }
            }


            client.Close();
        }


    }
}
