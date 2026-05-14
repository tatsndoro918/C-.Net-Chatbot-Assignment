using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Cybersecurity_Awareness_Bot
{
    public partial class MainWindow : Window
    {
        //  STATE FLAGS
        private int _nameStep = 0;
        private int _favTopicStep = 0;

        // Colour brushes
        private readonly SolidColorBrush _goldBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD700"));
        private readonly SolidColorBrush _purpleBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AA88FF"));
        private readonly SolidColorBrush _textBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F0E8"));
        private readonly SolidColorBrush _dimBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAAAAA"));

        //  CONSTRUCTOR
        public MainWindow()
        {
            InitializeComponent();
            Bot.PlayIntroSound();
            AppendBotMessage(Bot.Welcome());
            InputBox.Focus();
        }

        //  ENTER KEY SHORTCUT
        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendButton_Click(sender, e);
        }

    }
}
