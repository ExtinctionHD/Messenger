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
    /// Логика взаимодействия для User.xaml
    /// </summary>
    public partial class User : UserControl
    {
        public string nickname;
        public int MessageCount
        {
            get
            {
                return messageCount;
            }
            set
            {
                messageCount = value;
                if (value == 0)
                {
                    borderMsgCount.Opacity = 0;
                    txtblockMsgCount.Text = string.Empty;
                }
                else
                {
                    borderMsgCount.Opacity = 1;
                    txtblockMsgCount.Text = "+" + messageCount;
                }
            }
        }
        private int messageCount;

        public User(string nickname)
        {
            InitializeComponent();

            this.nickname = nickname;
            txtblockNickname.Text = nickname;
            MessageCount = 0;
        }
    }
}
