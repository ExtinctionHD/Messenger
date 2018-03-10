using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using SystemCommands;
using System.IO;

namespace ServerApp
{
    class User
    {
        public User(string nickname, Socket socket)
        {
            this.nickname = nickname;
            this.socket = socket;
        }

        public string nickname;
        public Socket socket;
    }

    class UserThread
    {
        private const string HISTORY_FILE = "messages.story";

        private bool isConnect = false;

        private Thread thread;
        private Socket socket;

        private List<User> users;
        private User self;

        public UserThread(Socket socket, List<User> users)
        {
            this.socket = socket;
            this.users = users;

            thread = new Thread(Service);
            thread.Start();
        }

        private void Service()
        {
            LogIn();

            if (isConnect)
            {
                SendHistory();
            }

            string msg = string.Empty;
            while(isConnect)
            {
                msg = ReceiveMsg();

                if (msg != string.Empty && !IsSystemMsg(msg))
                {
                    if (msg[0] == Symbols.ALL_CHAT)
                    {
                        SendPublicMsg(msg);
                    }
                    else
                    {
                        SendPrivateMsg(msg);
                    }
                }

                Thread.Sleep(0);
            }
        }

        private void LogIn()
        {
            string nickname = ReceiveMsg();
            if (users.Find(x => x.nickname == nickname) == null)
            {
                self = new User(nickname, socket);
                users.Add(self);
                Console.WriteLine("Пользователь {0} присоединился.", self.nickname);
                SendUsersList();
                isConnect = true;
            }
            else
            {
                socket.Send(new byte[] { (byte)Symbols.NICKNAME_TAKEN });
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

        private void SendHistory()
        {
            if (!File.Exists(HISTORY_FILE))
            {
                return;
            }
            
            self.socket.Send(File.ReadAllBytes(HISTORY_FILE));
        }

        private string ReceiveMsg()
        {
            byte[] bytes = new byte[1024];
            int count = socket.Receive(bytes);
            return Encoding.UTF8.GetString(bytes, 0, count);
        }

        private void SendPublicMsg(string msg)
        {
            msg = msg.Remove(0, 2);
            msg += Symbols.SEPARATOR + GetTime() + Symbols.SEPARATOR + self.nickname + Symbols.MSG_END;

            byte[] bytes = Encoding.UTF8.GetBytes(msg);
            SaveMsg(bytes);

            foreach (User user in users)
            {
                user.socket.Send(bytes);
            }
        }

        private void SaveMsg(byte[] bytes)
        {
            using (BinaryWriter bw = new BinaryWriter(File.Open(HISTORY_FILE, FileMode.Append)))
            {
                bw.Write(bytes);
            }
        }

        private void SendPrivateMsg(string msg)
        {
            string[] data = msg.Split(new char[] { Symbols.SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
            string nickname = data[0];

            Socket receiverSocket = users.Find(x => x.nickname == nickname).socket;
            msg = Symbols.PRIVATE_CHAT + data[1] + Symbols.SEPARATOR + GetTime() + Symbols.SEPARATOR + self.nickname + Symbols.MSG_END;

            byte[] bytes = Encoding.UTF8.GetBytes(msg);
            receiverSocket.Send(bytes);

            if (receiverSocket != self.socket)
            {
                bytes[0] = (byte)Symbols.SELF_MSG;
                self.socket.Send(bytes);
            }
        }

        private void SendUsersList()
        {
            string msg = Symbols.USER_LIST.ToString();
            foreach (User user in users)
            {
                msg += user.nickname + Symbols.SEPARATOR;
            }
            msg += Symbols.MSG_END;

            byte[] bytes = Encoding.UTF8.GetBytes(msg);
            foreach (User user in users)
            {
                user.socket.Send(bytes);
            }
        }

        private bool IsSystemMsg(string msg)
        {
            switch (msg[0])
            {
                case Symbols.EXIT:
                    Exit();
                    return true;

                default:
                    return false;
            }
        }

        private string GetTime()
        {
            string result;

            DateTime now = DateTime.Now;

            result = now.Hour.ToString();
            
            if (now.Minute > 9)
            {
                result += ":" + now.Minute;
            }
            else
            {
                result += ":0" + now.Minute;
            }

            return result;
        }

        private void Exit()
        {
            Console.WriteLine("Пользователь {0} вышел.", self.nickname);

            users.Remove(self);
            SendUsersList();

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            isConnect = false;
        }
    }

    class Program
    {
        static private List<User> users = new List<User>();

        static void Main(string[] args)
        {
            // Устанавливаем для сокета локальную конечную точку
            IPHostEntry ipHost = Dns.GetHostEntry("DESKTOP-GNTLAUI");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);

            // Создаем сокет TCP/IP
            Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
            try
            {
                listener.Bind(ipEndPoint);
                listener.Listen(10);
                Console.WriteLine("Ожидаем подключение клиента.");

                // Начинаем слушать соеденения
                while (true)
                {
                    // Программа приостанавливается, ожидая вхдящее соединение
                    UserThread userThread = new UserThread(listener.Accept(), users);
                    Thread.Sleep(0);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}
