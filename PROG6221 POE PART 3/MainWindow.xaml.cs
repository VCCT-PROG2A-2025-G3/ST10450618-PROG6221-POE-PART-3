using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Media;

namespace PROG6221_POE_PART_3
{
    public partial class MainWindow : Window
    {
        // Instance of the CyberBot class for chatbot logic
        private CyberBot bot;
        // Stores the user's name
        private string userName = "";
        // Tracks the current page in the activity log (not used in this code)
        private int activityLogPage = 0;
        // Number of activity log entries per page (not used in this code)
        private const int ActivityLogPageSize = 5;
        // Flag to determine if all activity log entries should be shown
        private bool showAllActivityLog = false;

        // Constructor: Initializes the window and sets up the bot and UI
        public MainWindow()
        {
            InitializeComponent();
            bot = new CyberBot(AppendBotMessage, AppendGameMessage, GetUserName);
            ShowPanel(ChatBotPanel); // Show chatbot panel by default
            this.Loaded += MainWindow_Loaded; // Play audio after window is loaded
            AppendBotMessage($"Welcome!, What is your name?");
        }

        // Event handler: Plays greeting and voice audio after the window is loaded and visible
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PlayGreeting();
            PlayVoice();
        }

        // Plays the greeting audio (greeting1.wav) synchronously
        private void PlayGreeting()
        {
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting1.wav");
                if (!System.IO.File.Exists(path))
                {
                    MessageBox.Show("File not found: " + path);
                    return;
                }
                SoundPlayer player = new SoundPlayer(path);
                player.Load();
                player.PlaySync(); // Play audio and wait until finished
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error playing audio: " + ex.Message, "Audio Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Plays the voice greeting audio (VoiceGreeting.wav) synchronously
        private void PlayVoice()
        {
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VoiceGreeting.wav");
                if (!System.IO.File.Exists(path))
                {
                    MessageBox.Show("File not found: " + path);
                    return;
                }
                SoundPlayer player = new SoundPlayer(path);
                player.Load();
                player.PlaySync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error playing audio: " + ex.Message, "Audio Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Shows the specified panel and hides all others
        private void ShowPanel(UIElement panel)
        {
            ChatBotPanel.Visibility = Visibility.Collapsed;
            TasksPanel.Visibility = Visibility.Collapsed;
            GamePanel.Visibility = Visibility.Collapsed;
            ActivityLogPanel.Visibility = Visibility.Collapsed;
            panel.Visibility = Visibility.Visible;
            if (panel == ActivityLogPanel)
            {
                showAllActivityLog = false; // Reset to show only 10 on open
                LoadActivityLog();
            }
            if (panel == TasksPanel)
                LoadTasks();
        }

        // Navigation button event handlers to switch between panels
        private void ShowChatBot(object s, RoutedEventArgs e) => ShowPanel(ChatBotPanel);
        private void ShowTasks(object s, RoutedEventArgs e) => ShowPanel(TasksPanel);
        private void ShowGame(object s, RoutedEventArgs e) => ShowPanel(GamePanel);
        private void ShowActivityLog(object s, RoutedEventArgs e) => ShowPanel(ActivityLogPanel);

        // Handles sending user input to the chatbot
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string input = UserInput.Text.Trim();
            if (!string.IsNullOrEmpty(input))
            {
                AppendUserMessage(input); // Show user message in chat history
                bot.ProcessInput(input);  // Process input with CyberBot
                UserInput.Clear();        // Clear input box
            }
        }

        // Handles "Show More" button for activity log paging
        private void ShowMoreActivityLog_Click(object sender, RoutedEventArgs e)
        {
            showAllActivityLog = true;
            LoadActivityLog();
        }

        // Handles Enter key in the user input box to send message
        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SendButton_Click(sender, e);
        }

        // Appends a bot message to the chat history
        private void AppendBotMessage(string msg) => ChatHistory.Items.Add("CyberBot: " + msg);
        // Appends a user message to the chat history
        private void AppendUserMessage(string msg) => ChatHistory.Items.Add("You: " + msg);

        // Appends a message to the game (quiz) history and updates score if quiz is complete
        private void AppendGameMessage(string msg)
        {
            GameHistory.Items.Add("CyberBot: " + msg);

            // Check if the message contains the final score
            if (msg.StartsWith("Quiz complete! Your score:"))
            {
                // Example message: "Quiz complete! Your score: 2/3"
                var parts = msg.Split(':');
                if (parts.Length > 1)
                {
                    var scorePart = parts[1].Trim(); // "2/3"
                    ScoreTextBlock.Text = $"Score: {scorePart}";
                }
            }
        }

        // Returns the current user name (used by CyberBot)
        private string GetUserName() => userName;

        // --- Task Management ---

        // Handles adding a new task from the UI
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            string name = TaskNameBox.Text.Trim();
            string desc = TaskDescBox.Text.Trim();
            int days = 0;
            int.TryParse(TaskReminderBox.Text.Trim(), out days);
            if (!string.IsNullOrEmpty(name))
            {
                bot.ProcessInput($"add task- {name}");
                bot.ProcessInput(desc);
                if (days > 0)
                    bot.ProcessInput($"remind me in {days} days");
                LoadTasks();
            }
        }

        // Handles marking a selected task as complete
        private void CompleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TasksList.SelectedItem is string task)
            {
                var name = task.Split(':')[0].Split(']')[1].Trim();
                bot.ProcessInput($"complete task- {name}");
                LoadTasks();
            }
        }

        // Handles deleting a selected task
        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TasksList.SelectedItem is string task)
            {
                var name = task.Split(':')[0].Split(']')[1].Trim();
                bot.ProcessInput($"delete task- {name}");
                LoadTasks();
            }
        }

        // Loads all tasks from CyberBot and displays them in the UI
        private void LoadTasks()
        {
            TasksList.Items.Clear();
            var tasks = bot.GetTasks();
            foreach (var t in tasks)
                TasksList.Items.Add(t.ToString());
        }

        // --- Game (Quiz) ---

        // Starts the quiz game and clears previous game history
        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            GameHistory.Items.Clear();
            bot.ProcessInput("play game");
        }
        // Example: Call this after bot.ProcessInput("play game") if you want to show answer buttons

        // Handles quiz answer button clicks (A, B, C, D)
        private void QuizAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                string answer = btn.Content.ToString().ToLower(); // "a", "b", "c", or "d"
                                                                  // Send the answer to the bot as if the user typed it
                bot.ProcessInput(answer);
            }
        }

        // Dynamically creates answer buttons for quiz options
        private void ShowAnswerButtons(string[] options)
        {
            AnswerButtonsPanel.Children.Clear();
            foreach (var option in options)
            {
                var btn = new Button
                {
                    Content = option,
                    Margin = new Thickness(5),
                    Padding = new Thickness(10)
                };
                btn.Click += AnswerButton_Click;
                AnswerButtonsPanel.Children.Add(btn);
            }
        }

        // Handles answer button click, sends answer to bot, and clears buttons
        private void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string answer)
            {
                AppendUserMessage(answer);
                bot.ProcessInput(answer);
                // Optionally clear buttons or update UI for next question
                AnswerButtonsPanel.Children.Clear();
            }
        }

        // --- Activity Log ---

        // Loads activity log entries and displays them in the UI
        private void LoadActivityLog()
        {
            ActivityLogList.Items.Clear();
            var allEntries = bot.GetActivityLog().ToList();
            if (!showAllActivityLog && allEntries.Count > 10)
            {
                // Show only the most recent 10
                allEntries = allEntries.Skip(allEntries.Count - 10).ToList();
            }
            foreach (var entry in allEntries)
                ActivityLogList.Items.Add(entry);
        }

        // REFERENCES:
        // ChatGPT was used to generate prompts and responses for the CyberBot class.

    }
}
