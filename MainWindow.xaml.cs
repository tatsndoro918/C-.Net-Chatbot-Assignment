using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Cybersecurity_Awareness_Bot
{
    public partial class MainWindow : Window
    {
        //  STATE FLAGS
        private int _nameStep = 0;
        private int _favTopicStep = 0;
        private readonly TaskRepository _taskRepository;
        private readonly DispatcherTimer _reminderTimer;
        private bool _isCheckingReminders;

        // Colour brushes
        private readonly SolidColorBrush _goldBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD700"));
        private readonly SolidColorBrush _purpleBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AA88FF"));
        private readonly SolidColorBrush _textBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F0E8"));
        private readonly SolidColorBrush _dimBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAAAAA"));

        //  CONSTRUCTOR
        public MainWindow()
        {
            InitializeComponent();
            _taskRepository = new TaskRepository();
            _reminderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _reminderTimer.Tick += ReminderTimer_Tick;
            Loaded += MainWindow_Loaded;
            Bot.PlayIntroSound();
            AppendBotMessage(Bot.Welcome());
            InputBox.Focus();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Keep the chatbot usable when MySQL has not been installed yet.
            try
            {
                await _taskRepository.EnsureDatabaseReadyAsync();
                _reminderTimer.Start();
                await CheckDueRemindersAsync();
            }
            catch
            {
                // The Tasks button gives detailed setup help when it is opened.
            }
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
                bool handledAsTaskCommand = await TryHandleTaskCommandAsync(input);
                if (!handledAsTaskCommand)
                {
                    string response = Bot.Conversation(input.ToLower());
                    AppendBotMessage(response);
                }
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

        private async void TasksBtn_Click(object sender, RoutedEventArgs e)
        {
            await OpenTaskAssistantAsync();
            InputBox.Focus();
        }
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
                             "Click Tasks to add, edit, complete, or delete cybersecurity tasks.\n" +
                             "You can also type 'show tasks' or 'add task - Review privacy settings'.\n" +
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
            TasksBtn.IsEnabled = false;
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

        //  TASK ASSISTANT AND CHAT COMMANDS
        private async Task OpenTaskAssistantAsync()
        {
            try
            {
                SetStatus("🏀  Connecting to the task database...");
                await _taskRepository.EnsureDatabaseReadyAsync();
                _reminderTimer.Start();

                TaskWindow taskWindow = new TaskWindow(_taskRepository)
                {
                    Owner = this
                };
                taskWindow.ShowDialog();
                SetStatus("🟢  Bot ready");
            }
            catch (Exception ex)
            {
                ShowTaskDatabaseError(ex);
            }
        }

        private async Task<bool> TryHandleTaskCommandAsync(string input)
        {
            string normalized = input.Trim().ToLowerInvariant();

            if (normalized == "tasks" || normalized == "show tasks" ||
                normalized == "show my tasks" || normalized == "list tasks" ||
                normalized == "view tasks")
            {
                try
                {
                    await _taskRepository.EnsureDatabaseReadyAsync();
                    List<CyberTask> tasks = await _taskRepository.GetAllAsync();
                    AppendBotMessage(BuildTaskSummary(tasks));
                    _reminderTimer.Start();
                }
                catch (Exception ex)
                {
                    AppendBotMessage(GetTaskDatabaseHelp(ex));
                }
                return true;
            }

            if (normalized == "manage tasks" ||
                normalized == "open task assistant" ||
                normalized == "task assistant")
            {
                await OpenTaskAssistantAsync();
                return true;
            }

            Match addMatch = Regex.Match(input,
                @"^\s*(?:add|create|new)\s+(?:a\s+)?task\s*(?:-|:)?\s*(.*)$",
                RegexOptions.IgnoreCase);
            if (addMatch.Success)
            {
                string details = addMatch.Groups[1].Value.Trim();
                if (string.IsNullOrWhiteSpace(details))
                {
                    AppendBotMessage(
                        "Tell me the task after a dash, for example:\n" +
                        "add task - Review privacy settings\n\n" +
                        "For descriptions and reminders, click the Tasks button.");
                    return true;
                }

                string[] parts = details.Split(new[] { '|' }, 2);
                CyberTask task = new CyberTask
                {
                    Title = parts[0].Trim(),
                    Description = parts.Length > 1
                        ? parts[1].Trim()
                        : "Cybersecurity task added through the chatbot."
                };

                if (string.IsNullOrWhiteSpace(task.Title))
                {
                    AppendBotMessage("Add a title after 'add task -'.");
                    return true;
                }

                if (task.Title.Length > 150)
                {
                    AppendBotMessage("Keep the task title under 150 characters.");
                    return true;
                }

                try
                {
                    await _taskRepository.EnsureDatabaseReadyAsync();
                    int id = await _taskRepository.AddAsync(task);
                    _reminderTimer.Start();
                    AppendBotMessage(
                        "Task #" + id + " added: “" + task.Title + "”.\n" +
                        "Open Tasks if you want to add a description or reminder.");
                }
                catch (Exception ex)
                {
                    AppendBotMessage(GetTaskDatabaseHelp(ex));
                }
                return true;
            }

            Match statusMatch = Regex.Match(input,
                @"^\s*(complete|finish|reopen)\s+task\s+#?(\d+)\s*$",
                RegexOptions.IgnoreCase);
            if (statusMatch.Success)
            {
                bool complete = !statusMatch.Groups[1].Value.Equals(
                    "reopen", StringComparison.OrdinalIgnoreCase);
                int taskId = int.Parse(statusMatch.Groups[2].Value);

                try
                {
                    await _taskRepository.EnsureDatabaseReadyAsync();
                    bool updated =
                        await _taskRepository.SetCompletionAsync(taskId, complete);
                    if (!updated)
                    {
                        AppendBotMessage(
                            "I couldn’t find task #" + taskId +
                            ". Type 'show tasks' to check the available IDs.");
                    }
                    else
                    {
                        AppendBotMessage(complete
                            ? "Task #" + taskId + " marked as completed. Great defensive play!"
                            : "Task #" + taskId + " reopened.");
                    }
                }
                catch (Exception ex)
                {
                    AppendBotMessage(GetTaskDatabaseHelp(ex));
                }
                return true;
            }

            if (Regex.IsMatch(input, @"^\s*delete\s+task\b",
                RegexOptions.IgnoreCase))
            {
                AppendBotMessage(
                    "Open the Tasks window to delete a task. " +
                    "The confirmation step prevents accidental deletion.");
                return true;
            }

            return false;
        }

        private static string BuildTaskSummary(List<CyberTask> tasks)
        {
            if (tasks.Count == 0)
            {
                return "You have no saved cybersecurity tasks. " +
                       "Type 'add task - Enable two-factor authentication' to create one.";
            }

            StringBuilder summary = new StringBuilder();
            summary.AppendLine("Here’s your cybersecurity task board:");
            summary.AppendLine();

            foreach (CyberTask task in tasks.Take(8))
            {
                summary.Append(task.IsCompleted ? "✓" : "○")
                    .Append(" #").Append(task.Id)
                    .Append(" — ").Append(task.Title);

                if (task.ReminderAt.HasValue)
                {
                    summary.Append(" (reminder: ")
                        .Append(task.ReminderAt.Value.ToString("dd MMM yyyy HH:mm"))
                        .Append(")");
                }
                summary.AppendLine();
            }

            if (tasks.Count > 8)
            {
                summary.AppendLine("…and " + (tasks.Count - 8) +
                    " more. Open Tasks to view the full list.");
            }

            summary.AppendLine();
            summary.Append("Use 'complete task #ID' or open Tasks to manage them.");
            return summary.ToString();
        }

        private async void ReminderTimer_Tick(object sender, EventArgs e)
        {
            await CheckDueRemindersAsync();
        }

        private async Task CheckDueRemindersAsync()
        {
            if (_isCheckingReminders)
            {
                return;
            }

            _isCheckingReminders = true;
            try
            {
                List<CyberTask> dueTasks =
                    await _taskRepository.GetDueRemindersAsync();

                foreach (CyberTask task in dueTasks)
                {
                    string message = "Reminder: " + task.Title;
                    if (!string.IsNullOrWhiteSpace(task.Description))
                    {
                        message += "\n\n" + task.Description;
                    }

                    AppendBotMessage(message);
                    MessageBox.Show(
                        message,
                        "Cybersecurity task reminder",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    await _taskRepository.MarkReminderNotifiedAsync(task.Id);
                }
            }
            catch
            {
                
            }
            finally
            {
                _isCheckingReminders = false;
            }
        }
        private void ShowTaskDatabaseError(Exception exception)
        {
            SetStatus("🔴  Task database unavailable");
            MessageBox.Show(
                GetTaskDatabaseHelp(exception),
                "Task database unavailable",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private static string GetTaskDatabaseHelp(Exception exception)
        {
            return "I couldn’t connect to the task database. The chatbot still works.\n\n" +
                   "To enable tasks:\n" +
                   "1. Install MySQL Community Server and MySQL Workbench.\n" +
                   "2. Run database\\setup.sql in Workbench.\n" +
                   "3. Check the CyberBotDatabase connection in App.config.\n\n" +
                   "Technical detail: " + exception.Message;
        }

    }
}
