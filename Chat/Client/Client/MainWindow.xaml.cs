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
using Client.Classes;
using SystemCommands;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Chat chat;
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
                    grLogIn.Visibility = Visibility.Hidden;
                }
                else
                {
                    grLogIn.Visibility = Visibility.Visible;
                }
            }
        }


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IsConnected = false;
        }

        private void txtboxMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                chat.SendMessage(txtboxMessage.Text);
                txtboxMessage.Text = String.Empty;
            }
        }

        private void btnLogIn_Click(object sender, RoutedEventArgs e)
        {
            if (txtboxNickname.Text != String.Empty)
            {
                chat = new Chat(txtboxNickname.Text, "DESKTOP-GNTLAUI", 11000, grLogIn, listboxMessages, listboxUsers);
                txtblockNickname.Text = txtboxNickname.Text;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsConnected)
            {
                chat?.Exit();
            }
        }

        private void listboxUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (chat == null || e.AddedItems.Count == 0)
                return;

            txtblockReceiver.Text = ((User)e.AddedItems[0]).nickname;

            if (listboxUsers.Items.IndexOf(e.AddedItems[0]) != 0)
            {
                chat.Receiver = ((User)e.AddedItems[0]).nickname;
            }
            else
            {
                chat.Receiver = Symbols.ALL_CHAT.ToString();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            chat.Exit();
        }
    }
}
