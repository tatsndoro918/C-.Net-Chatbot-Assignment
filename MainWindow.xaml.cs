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

        //  SEND BUTTON
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string input = InputBox.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            SendButton.IsEnabled = false;
            InputBox.IsEnabled = false;
            SetStatus("🏀  Bot is thinking...");

            // collect name
            if (_nameStep == 0)
            {
                AppendUserMessage(input);
                InputBox.Clear();
                await TypingDelayAsync();

                Bot.UserName = input;
                _nameStep++;

                // Update the left panel user display
                UserInfoBlock.Text = $"🏀  Chatting with\n{Bot.UserName}";

                AppendBotMessage($"Got it, {Bot.UserName}! 🏀  Quick tip before we start: be cautious when sharing personal information online.\n\nWhat is your favourite cybersecurity topic? (e.g. passwords, phishing, privacy)");
            }

            // collect favourite topic
            else if (_favTopicStep == 0)
            {
                AppendUserMessage(input);
                InputBox.Clear();
                await TypingDelayAsync();

                Bot.FavouriteTopic = input.ToLower();
                _favTopicStep++;

                AppendBotMessage($"Great choice, {Bot.UserName}! I'll remember that you're interested in {Bot.FavouriteTopic}. It's a crucial part of staying safe online.\n\nYou can now ask me anything about cybersecurity. Press '📋 Topics' below to see everything I can help with!");
            }

            // Exit / goodbye via text
            else if (input.ToLower().Contains("exit") ||
                     input.ToLower().Contains("goodbye") ||
                     input.ToLower().Contains("bye") ||
                     input.ToLower().Contains("quit"))
            {
                AppendUserMessage(input);
                InputBox.Clear();
                await TypingDelayAsync();
                HandleExit();
                return;
            }

            // Normal conversation
            else
            {
                AppendUserMessage(input);
                InputBox.Clear();
                await TypingDelayAsync();
                string response = Bot.Conversation(input.ToLower());
                AppendBotMessage(response);
            }

            SendButton.IsEnabled = true;
            InputBox.IsEnabled = true;
            InputBox.Focus();
            SetStatus("🟢  Bot ready");
        }

        //  ACTION BUTTONS
        private void TopicsBtn_Click(object sender, RoutedEventArgs e)
        {
            AppendBotMessage("Here are all the topics I can help you with:\n\n" +
                             "• Password safety\n" +
                             "• Phishing\n" +
                             "• Safe browsing\n" +
                             "• MFA / Two-factor authentication\n" +
                             "• Software updates\n" +
                             "• Malware & Ransomware\n" +
                             "• Public Wi-Fi\n" +
                             "• Social engineering\n" +
                             "• Privacy & Personal information\n" +
                             "• Identity theft\n" +
                             "• Financial loss\n" +
                             "• Scams\n" +
                             "• VPN\n" +
                             "• Firewall\n" +
                             "• Encryption\n" +
                             "• Psychological / Emotional impact\n\n" +
                             "Just type any of these topics and I'll share tips and guidance!");
            InputBox.Focus();
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            ChatDisplay.Inlines.Clear();
            AppendBotMessage("Chat cleared! 🏀  How can I help you stay safe online?");
            InputBox.Focus();
        }

        private void HelpBtn_Click(object sender, RoutedEventArgs e)
        {
            AppendBotMessage("Here's how to use the Cybersecurity Awareness Bot:\n\n" +
                             "Type any cybersecurity topic to get tips and information.\n" +
                             "Say 'tell me more' or 'give me another tip' to continue on the same topic.\n" +
                             "Share how you're feeling (e.g. 'I'm worried about scams') and I'll respond with support.\n" +
                             "I remember your name and favourite topic throughout our chat.\n" +
                             "Click Topics to see everything I can help with.\n" +
                             "Click Clear to reset the chat window.\n" +
                             "Click Exit to close the program.");
            InputBox.Focus();
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            HandleExit();
        }

        //  EXIT LOGIC
        private void HandleExit()
        {
            string farewell = !string.IsNullOrEmpty(Bot.FavouriteTopic)
                ? $"Goodbye, {Bot.UserName}! Since you're interested in {Bot.FavouriteTopic}, keep practising safe habits around it. Stay safe out there! 🏀"
                : $"Goodbye, {Bot.UserName}! Stay safe online! 🏀";

            AppendBotMessage(farewell);
            SetStatus("🔴  Session ended");

            InputBox.IsEnabled = false;
            SendButton.IsEnabled = false;
            TopicsBtn.IsEnabled = false;
            ClearBtn.IsEnabled = false;
            HelpBtn.IsEnabled = false;
            ExitBtn.IsEnabled = false;
        }

        //  TYPING DELAY
        private async Task TypingDelayAsync()
        {
            SetStatus("🏀  Bot is typing...");
            await Task.Delay(500);
        }

        //  APPEND BOT MESSAGE  (gold label)
        private void AppendBotMessage(string message)
        {
            Run label = new Run("🤖  BOT ▶  ")
            {
                Foreground = _goldBrush,
                FontWeight = FontWeights.Bold,
                FontSize = 13
            };
            Run body = new Run(message + "\n\n")
            {
                Foreground = _textBrush,
                FontSize = 14
            };

            ChatDisplay.Inlines.Add(label);
            ChatDisplay.Inlines.Add(body);
            ScrollToBottom();
        }

        //  APPEND USER MESSAGE  (purple label)
        private void AppendUserMessage(string message)
        {
            string displayName = string.IsNullOrEmpty(Bot.UserName)
                ? "YOU"
                : Bot.UserName.ToUpper();

            Run label = new Run($"🏀  {displayName} ▶  ")
            {
                Foreground = _purpleBrush,
                FontWeight = FontWeights.Bold,
                FontSize = 13
            };
            Run body = new Run(message + "\n\n")
            {
                Foreground = _dimBrush,
                FontSize = 14,
                FontStyle = FontStyles.Italic
            };

            ChatDisplay.Inlines.Add(label);
            ChatDisplay.Inlines.Add(body);
            ScrollToBottom();
        }

        //  HELPERS
        private void ScrollToBottom()
        {
            ChatScroller.UpdateLayout();
            ChatScroller.ScrollToBottom();
        }

        private void SetStatus(string text)
        {
            StatusBar.Text = text;
        }

    }
}
