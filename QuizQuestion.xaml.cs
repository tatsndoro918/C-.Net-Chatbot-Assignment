using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cybersecurity_Awareness_Bot
{
    public partial class QuizWindow : Window
    {
        private readonly List<QuizQuestion> _questionBank;
        private List<QuizQuestion> _questions;

        private int _currentQuestionIndex;
        private int _score;
        private int _selectedAnswerIndex = -1;
        private bool _answerSubmitted;

        public QuizWindow()
        {
            InitializeComponent();

            _questionBank = CreateQuestions();
            StartQuiz();
        }

        private List<QuizQuestion> CreateQuestions()
        {
            return new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Question = "What should you do if an email asks for your password?",
                    Options = new List<string>
                    {
                        "Reply with the password",
                        "Click the link and sign in",
                        "Report the email as phishing",
                        "Forward it to friends"
                    },
                    CorrectAnswerIndex = 2,
                    Explanation =
                        "Legitimate organisations should not request your password by email. Reporting it helps prevent phishing attacks."
                },

                new QuizQuestion
                {
                    Question = "True or False: Reusing one strong password on every website is safe.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 1,
                    Explanation =
                        "Password reuse is dangerous because one breached website can expose all your accounts."
                },

                new QuizQuestion
                {
                    Question = "What is the main purpose of multi-factor authentication?",
                    Options = new List<string>
                    {
                        "To make passwords shorter",
                        "To add another verification step",
                        "To remove the need for security",
                        "To speed up public Wi-Fi"
                    },
                    CorrectAnswerIndex = 1,
                    Explanation =
                        "MFA requires an additional verification factor, making stolen passwords less useful to attackers."
                },

                new QuizQuestion
                {
                    Question = "True or False: HTTPS guarantees that a website is completely trustworthy.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 1,
                    Explanation =
                        "HTTPS encrypts the connection, but a malicious website can also use HTTPS. Always check the domain."
                },

                new QuizQuestion
                {
                    Question = "What is safest when using public Wi-Fi?",
                    Options = new List<string>
                    {
                        "Access online banking normally",
                        "Disable all passwords",
                        "Use a VPN or mobile data",
                        "Share files with everyone"
                    },
                    CorrectAnswerIndex = 2,
                    Explanation =
                        "A VPN or trusted mobile connection reduces exposure to attackers on public networks."
                },

                new QuizQuestion
                {
                    Question = "What does ransomware normally do?",
                    Options = new List<string>
                    {
                        "Improves computer performance",
                        "Encrypts files and demands payment",
                        "Updates antivirus software",
                        "Creates stronger passwords"
                    },
                    CorrectAnswerIndex = 1,
                    Explanation =
                        "Ransomware locks or encrypts data and demands payment. Offline backups are an important defence."
                },

                new QuizQuestion
                {
                    Question = "Why are software updates important?",
                    Options = new List<string>
                    {
                        "They patch security vulnerabilities",
                        "They remove all passwords",
                        "They make phishing safe",
                        "They disable encryption"
                    },
                    CorrectAnswerIndex = 0,
                    Explanation =
                        "Updates fix known vulnerabilities before attackers can exploit them."
                },

                new QuizQuestion
                {
                    Question = "True or False: Social engineering mainly targets human behaviour.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 0,
                    Explanation =
                        "Social engineering manipulates people through trust, fear, urgency or deception."
                },

                new QuizQuestion
                {
                    Question = "Which backup is most useful against ransomware?",
                    Options = new List<string>
                    {
                        "A backup permanently connected to the infected computer",
                        "An offline or separate cloud backup",
                        "No backup",
                        "A copy in the same folder"
                    },
                    CorrectAnswerIndex = 1,
                    Explanation =
                        "Offline or isolated backups are harder for ransomware to encrypt."
                },

                new QuizQuestion
                {
                    Question = "Which information should you avoid posting publicly?",
                    Options = new List<string>
                    {
                        "Your favourite sport",
                        "Your password and identification number",
                        "A general cybersecurity tip",
                        "The name of a public website"
                    },
                    CorrectAnswerIndex = 1,
                    Explanation =
                        "Passwords and identity information can be used for fraud and account takeover."
                },

                new QuizQuestion
                {
                    Question = "True or False: A password manager can generate and store unique passwords.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswerIndex = 0,
                    Explanation =
                        "Password managers help users maintain strong, unique passwords without memorising every one."
                },

                new QuizQuestion
                {
                    Question = "What should you do with an unexpected email attachment?",
                    Options = new List<string>
                    {
                        "Open it immediately",
                        "Disable antivirus first",
                        "Verify the sender before opening it",
                        "Upload it publicly"
                    },
                    CorrectAnswerIndex = 2,
                    Explanation =
                        "Unexpected attachments may contain malware. Verify them through a trusted communication method."
                }
            };
        }

        private void StartQuiz()
        {
            Random random = new Random();

            _questions = _questionBank
                .OrderBy(question => random.Next())
                .ToList();

            _currentQuestionIndex = 0;
            _score = 0;

            QuizProgressBar.Minimum = 0;
            QuizProgressBar.Maximum = _questions.Count;

            RestartButton.Visibility = Visibility.Collapsed;

            ShowCurrentQuestion();
        }

        private void ShowCurrentQuestion()
        {
            if (_currentQuestionIndex >= _questions.Count)
            {
                ShowFinalScore();
                return;
            }

            QuizQuestion question = _questions[_currentQuestionIndex];

            _selectedAnswerIndex = -1;
            _answerSubmitted = false;

            QuestionTextBlock.Text = question.Question;
            OptionsPanel.Children.Clear();

            for (int index = 0; index < question.Options.Count; index++)
            {
                RadioButton optionButton = new RadioButton
                {
                    Content = question.Options[index],
                    Tag = index,
                    GroupName = "QuizAnswers",
                    Style = (Style)FindResource("QuizOptionStyle")
                };

                optionButton.Checked += OptionButton_Checked;
                OptionsPanel.Children.Add(optionButton);
            }

            ProgressTextBlock.Text =
                "Question " + (_currentQuestionIndex + 1) +
                " of " + _questions.Count;

            ScoreTextBlock.Text = "Score: " + _score;
            QuizProgressBar.Value = _currentQuestionIndex;

            FeedbackBorder.Visibility = Visibility.Collapsed;
            SubmitAnswerButton.Visibility = Visibility.Visible;
            NextQuestionButton.Visibility = Visibility.Collapsed;
        }

        private void OptionButton_Checked(object sender, RoutedEventArgs e)
        {
            if (_answerSubmitted)
            {
                return;
            }

            RadioButton selectedButton = sender as RadioButton;

            if (selectedButton != null)
            {
                _selectedAnswerIndex = (int)selectedButton.Tag;
            }
        }

        private void SubmitAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAnswerIndex < 0)
            {
                FeedbackBorder.Background =
                    new SolidColorBrush(Color.FromRgb(120, 70, 0));

                FeedbackTextBlock.Text =
                    "Select an answer before submitting.";

                FeedbackBorder.Visibility = Visibility.Visible;
                return;
            }

            QuizQuestion question = _questions[_currentQuestionIndex];
            bool correct =
                _selectedAnswerIndex == question.CorrectAnswerIndex;

            _answerSubmitted = true;

            if (correct)
            {
                _score++;

                FeedbackBorder.Background =
                    new SolidColorBrush(Color.FromRgb(30, 110, 60));

                FeedbackTextBlock.Text =
                    "Correct! 🏀\n\n" + question.Explanation;
            }
            else
            {
                FeedbackBorder.Background =
                    new SolidColorBrush(Color.FromRgb(150, 45, 45));

                FeedbackTextBlock.Text =
                    "Incorrect. The correct answer is: " +
                    question.Options[question.CorrectAnswerIndex] +
                    "\n\n" + question.Explanation;
            }

            foreach (RadioButton option in
                     OptionsPanel.Children.OfType<RadioButton>())
            {
                option.IsEnabled = false;
            }

            ScoreTextBlock.Text = "Score: " + _score;
            FeedbackBorder.Visibility = Visibility.Visible;
            SubmitAnswerButton.Visibility = Visibility.Collapsed;
            NextQuestionButton.Visibility = Visibility.Visible;

            NextQuestionButton.Content =
                _currentQuestionIndex == _questions.Count - 1
                    ? "View Final Score"
                    : "Next Question";
        }

        private void NextQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            _currentQuestionIndex++;
            ShowCurrentQuestion();
        }

        private void ShowFinalScore()
        {
            QuizProgressBar.Value = _questions.Count;
            ProgressTextBlock.Text = "Quiz completed";
            ScoreTextBlock.Text =
                "Final score: " + _score + "/" + _questions.Count;

            QuestionTextBlock.Text = "Final Result";
            OptionsPanel.Children.Clear();

            double percentage =
                (double)_score / _questions.Count * 100;

            string message;

            if (percentage >= 80)
            {
                message =
                    "Great job! You're a cybersecurity pro! 🏆";
            }
            else if (percentage >= 50)
            {
                message =
                    "Good effort! Keep learning to strengthen your defence.";
            }
            else
            {
                message =
                    "Keep learning to stay safe online. Review the explanations and try again.";
            }

            FeedbackBorder.Background =
                new SolidColorBrush(Color.FromRgb(85, 37, 131));

            FeedbackTextBlock.Text =
                "You scored " + _score + " out of " +
                _questions.Count + " (" +
                Math.Round(percentage) + "%).\n\n" + message;

            FeedbackBorder.Visibility = Visibility.Visible;
            SubmitAnswerButton.Visibility = Visibility.Collapsed;
            NextQuestionButton.Visibility = Visibility.Collapsed;
            RestartButton.Visibility = Visibility.Visible;
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            StartQuiz();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}