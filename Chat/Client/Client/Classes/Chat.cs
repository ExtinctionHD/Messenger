using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SystemCommands;

namespace Client.Classes
{
    class Chat
    {
        public string Receiver
        {
            get
            {
                return receiver;
            }
            set
            {
                receiver = value;
                if (receiver == Symbols.ALL_CHAT.ToString())
                {
                    ShowMessages(generalMessages, (User)userBox.Items[0]);
                }
                else
                {
                    int i = userList.FindIndex(x => x.nickname == receiver);
                    ShowMessages(userMessages[i], (User)userBox.Items[i + 1]);
                }
            }
        }
        private string receiver;

        private bool IsConnected
        {
            get
            {
                return grLogIn.Visibility != Visibility.Visible;
            }
            set
            {
                if (value)
                {
                    grLogIn.Dispatcher.Invoke(
                        new Action(() =>
                        {
                            grLogIn.Visibility = Visibility.Hidden;
                        })
                    );
                }
                else
                {
                    grLogIn.Dispatcher.Invoke(
                        new Action(() =>
                        {
                            grLogIn.Visibility = Visibility.Visible;
                        })
                    );
                }
            }
        }
            
        private Socket socket;
        private Thread receiveThread;

        private Grid grLogIn;
        private ListBox messageBox, userBox;
        private List<Message> generalMessages = new List<Message>();

        private List<List<Message>> userMessages = new List<List<Message>>();
        private List<User> userList = new List<User>();

        public Chat(string nickname, string hostNameOrAddress, int port, Grid grLogIn, ListBox messagesBox, ListBox usersBox)
        {
            this.grLogIn = grLogIn;

            this.messageBox = messagesBox;
            messagesBox.Items.Clear();

            this.userBox = usersBox;
            usersBox.Items.Clear();
            usersBox.Items.Add(new User("Общий чат"));
            usersBox.SelectedIndex = 0;

            Receiver = Symbols.ALL_CHAT.ToString();

            // Устанавливаем удаленную точку для сокета
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry("DESKTOP-GNTLAUI");
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

                // Подключение
                socket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipEndPoint);
                SendMsg(nickname);

                // Начало приема сообщений
                receiveThread = new Thread(ReceiveMsg);
                receiveThread.Start();
            }
            catch(SocketException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void SendMessage(string msg)
        {
            if (msg == string.Empty)
            {
                return;
            }

            msg = Receiver + Symbols.SEPARATOR + msg;

            SendMsg(msg);
        }

        private void SendMsg(string msg)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(msg);

            // Отправляем данные через сокет
            try
            {
                int bytesSent = socket.Send(bytes);
            }
            catch(SocketException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void ReceiveMsg()
        {
            byte[] bytes = new byte[1024];
            string buffer = string.Empty;
            
            // Получаем ответ от сервера
            while (true)
            {
                int count = 0;

                try
                {
                    count = socket.Receive(bytes);
                }
                catch(SocketException e)
                {
                    MessageBox.Show(e.Message);
                    IsConnected = false;
                    receiveThread.Abort();
                    break;
                }

                string message = Encoding.UTF8.GetString(bytes, 0, count);
                string[] messages = message.Split(new char[] { Symbols.MSG_END }, StringSplitOptions.RemoveEmptyEntries);

                if (buffer != string.Empty)
                {
                    messages[0] = buffer + messages[0];
                    buffer = string.Empty;
                }

                if (message[message.Length - 1] != Symbols.NICKNAME_TAKEN && message[message.Length - 1] != Symbols.MSG_END)
                {
                    buffer = messages.Last();
                    Array.Resize(ref messages, messages.Length - 1);
                }


                foreach (string msg in messages)
                {
                    if (!IsSystemMsg(msg))
                    {
                        switch (msg[0])
                        {
                            case Symbols.PRIVATE_CHAT:
                                msg.Remove(0, 1);
                                GetPrivateMsg(msg);
                                break;

                            case Symbols.SELF_MSG:
                                msg.Remove(0, 1);
                                GetSelfMsg(msg);
                                break;

                            default:
                                GetGeneralMsg(msg);
                                break;
                        }
                        
                    }
                }

                Thread.Sleep(0);
            }
        }

        private bool IsSystemMsg(string msg)
        {
            switch (msg[0])
            {
                case Symbols.USER_LIST:
                    ShowUsers(msg);
                    return true;

                case Symbols.NICKNAME_TAKEN:
                    NicknameAlredyTaken();
                    return true;

                default:
                    return false;
            }
        }

        private void GetGeneralMsg(string msg)
        {
            messageBox.Dispatcher.Invoke(
                new Action(() =>
                {
                    Message m = CreateMessage(msg);
                    generalMessages.Add(m);
                    if (Receiver == Symbols.ALL_CHAT.ToString())
                    {
                        ShowMsg(m);
                    }
                    else
                    {
                        ((User)userBox.Items[0]).MessageCount++;
                    }
                })
            );
        }

        private void GetPrivateMsg(string msg)
        {
            messageBox.Dispatcher.Invoke(
                new Action(() =>
                {
                    Message m = CreateMessage(msg);
                    int i = userList.FindIndex(x => x.nickname == m.nickname);
                    userMessages[i].Add(m);

                    if (Receiver == m.nickname)
                    {
                        ShowMsg(m);
                    }
                    else
                    {
                        userList[i].MessageCount++;
                    }
                })
            );
        }

        private void GetSelfMsg(string msg)
        {
            messageBox.Dispatcher.Invoke(
                new Action(() =>
                {
                    Message m = CreateMessage(msg);
                    userMessages[userList.FindIndex(x => x.nickname == Receiver)].Add(m);
                    ShowMsg(m);
                })
            );
        }

        private Message CreateMessage(string msg)
        {
            string[] data = msg.Split(Symbols.SEPARATOR);
            msg = data[0];
            string time = data[1];
            string nickname = data[2];

            return new Message(data[2], data[0], data[1]);
        }

        private void ShowUsers(string msg)
        {
            msg = msg.Remove(0, 1);
            string[] nicknames = msg.Split(new char[] { Symbols.SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);

            int oldUsersCount = userList.Count;
            int delta = nicknames.Length - oldUsersCount;

            if (delta > 0)
            {
                userBox.Dispatcher.Invoke(
                    new Action(() =>
                    {
                        for (int i = 0; i < delta; i++)
                        {
                            User user = new User(nicknames[oldUsersCount + i]);
                            userBox.Items.Add(user);
                            userList.Add(user);
                            userMessages.Add(new List<Message>()); 
                        }
                    })
                );
            }
            else
            {
                userBox.Dispatcher.Invoke(
                    new Action(() =>
                    {
                        User user = userList.Find(u => !Array.Exists(nicknames, a => a == u.nickname));
                        userBox.Items.Remove(user);
                        userMessages.Remove(userMessages[userList.IndexOf(user)]);
                        userList.Remove(user);

                        if (user.nickname == Receiver)
                        {
                            userBox.SelectedIndex = 0;
                        }
                    })
                );
            }

            if (!IsConnected)
            {
                IsConnected = true;
            }
        }

        private void ShowMessages(List<Message> messages, User user)
        {
            messageBox.Items.Clear();

            foreach(Message msg in messages)
            {
                messageBox.Items.Add(msg);
            }

            if (messages.Count != 0 && user.MessageCount > 0)
                messageBox.ScrollIntoView(messages.Last());

            user.MessageCount = 0;
        }

        private void ShowMsg(Message msg)
        {
            messageBox.Items.Add(msg);
            messageBox.ScrollIntoView(msg);
        }

        private void NicknameAlredyTaken()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            IsConnected = false;
            MessageBox.Show("Имя уже используется");
            receiveThread.Abort();
        }

        public void Exit()
        {
            try
            {
                SendMsg(Symbols.EXIT.ToString());
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch(SocketException e)
            {
                MessageBox.Show(e.Message);
            }
            receiveThread.Abort();
            IsConnected = false;
        }
    }
}
