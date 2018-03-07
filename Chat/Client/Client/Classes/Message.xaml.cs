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

namespace Client.Classes
{
    /// <summary>
    /// Логика взаимодействия для Message.xaml
    /// </summary>
    public partial class Message : UserControl
    {
        public string nickname;
        public string msg;
        public string time;

        public Message(string nickname, string msg, string time)
        {
            InitializeComponent();

            this.nickname = nickname;
            this.msg = msg;
            this.time = time;

            txtblockNickname.Text = nickname;
            txtblockTime.Text = time;
            txtblockMessage.Text = msg;
        }
    }
}
