using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PROG6221_POE_PART_3
{
    // Represents a single task item with title, description, reminder, and completion status
    public class TaskItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? Reminder { get; set; }
        public bool Completed { get; set; }

        // Returns a formatted string representation of the task for display
        public override string ToString()
        {
            string status = Completed ? "(Completed)" : "(Pending)";
            string reminderText = "";
            if (Reminder.HasValue)
            {
                int amtOfDays = (int)Math.Ceiling((Reminder.Value.Date - DateTime.Now.Date).TotalDays);
                if (amtOfDays > 0)
                    reminderText = $"remind me in {amtOfDays} days";
                else if (amtOfDays == 0)
                    reminderText = "remind me today";
                else
                    reminderText = "Due";
            }
            else
            {
                reminderText = "No reminder";
            }
            return $"{status}: {Title}, {Description}, {reminderText}";
        }
    }

    // Main CyberBot class: handles chatbot logic, NLP, tasks, quiz, and activity log
    public class CyberBot
    {
        // Output delegate for sending messages to the UI
        private readonly Action<string> output;
        // Delegate to get the user's name
        private readonly Func<string> getUserName;
        // Stores the user's name
        private string userName;
        // Stores the current conversation topic
        private string currentTopic = null;
        // Stores the user's interest for personalized tips
        private string userInterest = null;
        // List of all user tasks
        private List<TaskItem> tasks = new List<TaskItem>();
        // Used for multi-step task creation
        private TaskItem pendingTask = null;
        // Flags for multi-step task creation
        private bool awaitingTaskDescription = false;
        private bool awaitingTaskReminder = false;
        // Used for periodic tip suggestion
        int userInputCount = 0;
        int nextTipTrigger = new Random().Next(5, 8);
        // Delegates for chat and game output
        private readonly Action<string> chatOutput;
        private readonly Action<string> gameOutput;
        // Expose tasks to UI
        public IEnumerable<TaskItem> GetTasks() => tasks;
        // Expose activity log to UI
        public IEnumerable<string> GetActivityLog() => activityLog;
        // Returns a page of activity log entries (most recent first)
        public IEnumerable<string> GetActivityLogPage(int page, int pageSize = 5)
        {
            // Show most recent first
            return activityLog
                .AsEnumerable()
                .Reverse()
                .Skip(page * pageSize)
                .Take(pageSize);
        }

        // Activity log for tracking user actions
        private List<string> activityLog = new List<string>();
        private const int MaxLogEntries = 10;

        // --- Quiz State ---
        private int quizQuestionIndex = -1;
        private int quizScore = 0;
        private bool quizActive = false;
        // List of quiz questions (multiple choice and true/false)
        private readonly List<(string Question, string[] Options, int CorrectIndex)> quizQuestions = new()
        {
            // Multiple Choice
            ("What is phishing?", new[] { "A type of malware", "A scam to steal information", "A secure website", "A password manager" }, 1),
            ("Which is a strong password?", new[] { "password123", "qwerty", "MyDog2020!", "123456" }, 2),
            ("What should you check before clicking a link in an email?", new[] { "Sender's address", "Link URL", "Spelling/grammar", "All of the above" }, 3),
            ("Which of these is a sign of a phishing email?", new[] { "Personalized greeting", "Spelling mistakes", "Official company logo", "Secure URL" }, 1),
            ("What does malware do?", new[] { "Protects your computer", "Slows down your internet", "Harms or steals data", "Improves battery life" }, 2),
            // True/False
            ("True or False: You should use the same password for all accounts.", new[] { "True", "False" }, 1),
            ("True or False: HTTPS means a website is always safe.", new[] { "True", "False" }, 1),
            ("True or False: Antivirus software can protect against all cyber threats.", new[] { "True", "False" }, 1),
            ("True or False: You should never share your passwords with anyone.", new[] { "True", "False" }, 0),
            ("True or False: Public Wi-Fi is always safe for online banking.", new[] { "True", "False" }, 1)
        };

        // Dictionary of tips for each interest/topic
        Dictionary<string, List<string>> interestTips = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "phishing", new List<string> { "Phishing emails often look like messages from trusted sources — always verify.", "Don't click links or download attachments from unknown senders.", "Look out for spelling mistakes and generic greetings in emails.", "When in doubt, contact the company directly through their website.", "Phishing attacks can also happen via text or social media — stay alert." } },
            { "password", new List<string> { "Use a mix of uppercase, lowercase, numbers, and symbols in your passwords.", "Avoid using the same password for multiple accounts.", "Never share your passwords — even with people you trust.", "Update your passwords regularly, especially after a breach.", "Use a password manager to store and generate strong passwords securely." } },
            { "safe browsing", new List<string> { "Only visit websites that use HTTPS for a secure connection.", "Avoid clicking on pop-up ads or unknown links.", "Keep your browser and extensions up to date.", "Use ad blockers to reduce risk from malicious ads.", "Clear your browser history and cookies regularly for privacy." } },
            { "malware", new List<string> { "Avoid downloading software from unofficial sources.", "Scan files with antivirus software before opening them.", "Keep your operating system and apps updated to fix vulnerabilities.", "Don't click suspicious links or email attachments.", "Use reputable antivirus and anti-malware tools." } },
            { "cyber security", new List<string> { "Cybersecurity starts with awareness — stay informed about threats.", "Strong passwords and two-factor authentication are your first line of defense.", "Back up your important files regularly to recover from attacks.", "Use security software to protect your devices and data.", "Lock your devices when not in use to prevent unauthorized access." } },
            { "cybersecurity", new List<string> { "Cybersecurity is everyone's responsibility — help others learn too.", "Beware of social engineering — don't share sensitive info without verifying.", "Think before you click: many attacks begin with a single careless action.", "Stay cautious on public Wi-Fi — avoid accessing sensitive accounts.", "Enable firewalls to block unwanted connections." } },
            { "cyber threats", new List<string> { "Common cyber threats include phishing, malware, and ransomware.", "Watch out for urgent or threatening messages — they might be scams.", "Keep your software patched to prevent vulnerabilities from being exploited.", "Don’t click suspicious links, even if they look official.", "Understand different types of threats so you can recognize and avoid them." } },
            { "ransomware", new List<string> { "Ransomware can lock your files until you pay — always back up important data.", "Don't open attachments from unexpected or suspicious emails.", "Avoid downloading cracked software — it's a common source of ransomware.", "Use cloud storage with version history to recover files safely.", "Keep security software updated to detect ransomware early." } },
            { "identity theft", new List<string> { "Shred personal documents before throwing them away.", "Be cautious about what you share on social media.", "Monitor your credit report regularly for unusual activity.", "Don't give out personal info over the phone unless you're sure who you're talking to.", "Use multi-factor authentication to protect sensitive accounts." } },
            { "social engineering", new List<string> { "Scammers may pose as trusted contacts to trick you — always verify.", "Don’t be pressured into quick decisions — it’s a red flag.", "If something feels off, it probably is — listen to your instincts.", "Avoid oversharing on social media — it makes you an easier target.", "Always double-check the identity of people asking for sensitive information." } },
            { "data protection", new List<string> { "Encrypt sensitive data to keep it secure from hackers.", "Back up your files regularly — both locally and in the cloud.", "Limit the personal data you share online.", "Use strong passwords and secure storage for important data.", "Be cautious about granting apps or websites access to your data." } },
            { "internet", new List<string> { "The internet is a powerful tool — use it with caution.", "Not all information online is reliable — verify your sources.", "Don’t share personal info on unfamiliar websites.", "Use privacy settings to control what others can see about you.", "Stay updated on the latest internet safety trends and threats." } },
            { "internet safety", new List<string> { "Always log out of shared devices when you're done.", "Use strong passwords and avoid public Wi-Fi for banking or shopping.", "Limit personal info on social media — oversharing can make you a target.", "Keep all your software and apps updated.", "Avoid engaging with suspicious messages or offers online." } },
            { "online safety", new List<string> { "Use two-factor authentication wherever possible.", "Be mindful of what you click — some links can download malware.", "Educate yourself on scams and phishing tactics.", "Install a reputable security suite on all your devices.", "Don’t open files or links from strangers — even if they look familiar." } },
            { "scam", new List<string> { "If it sounds too good to be true, it probably is.", "Scammers often impersonate banks, tech support, or government agencies.", "Don’t give out personal or payment info unless you initiated the contact.", "Be skeptical of messages asking you to act urgently.", "Double-check suspicious offers with official sources before acting." } },
            { "scams", new List<string> { "If it sounds too good to be true, it probably is.", "Scammers often impersonate banks, tech support, or government agencies.", "Don’t give out personal or payment info unless you initiated the contact.", "Be skeptical of messages asking you to act urgently.", "Double-check suspicious offers with official sources before acting." } },
        };

        // Dictionary of sentiment keywords and empathetic responses
        private Dictionary<string, string> sentimentResponses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "worried", "It's completely understandable to feel that way." },
            { "frustrated", "I know this stuff can be tricky. I'm here to help you every step of the way." },
            { "curious", "That's great! Curiosity is key to staying informed and safe online." },
            { "overwhelmed", "Take a deep breath. We can go through this at your pace." },
            { "confused", "No worries, I can explain things in a simpler way for you." }
        };

        // Dictionary of topic facts and explanations
        Dictionary<string, List<string>> topicResponses = new Dictionary<string, List<string>>
        {
            { "phishing", new List<string> { "Phishing is a form of cyber attack where attackers impersonate trusted sources to steal sensitive data.", "Phishing attacks commonly occur via email, text messages, or fake websites.", "The term 'phishing' was first coined in the 1990s by hackers stealing AOL accounts.", "Phishing relies on social engineering to trick users into providing personal information." } },
            { "malware", new List<string> { "Malware stands for 'malicious software' and includes viruses, worms, trojans, and spyware.", "Malware can be used to steal data, monitor user activity, or damage systems.", "The first malware ever created was the Creeper virus in the early 1970s.", "Malware can self-replicate or disguise itself as legitimate software." } },
            { "scam", new List<string> { "A scam is a dishonest scheme designed to trick people into giving up money or personal data.", "Online scams have evolved to include fake tech support, romance scams, and investment fraud.", "Many scams exploit emotional triggers like fear, urgency, or greed.", "Scammers often use fake identities and spoofed contact information." } },
            { "internet safety", new List<string> { "Internet safety refers to protecting personal information and avoiding online threats.", "Common risks include cyberbullying, identity theft, and exposure to inappropriate content.", "Safe internet practices are crucial for children, teens, and adults alike.", "Internet safety is a foundational component of digital literacy education." } },
            { "online safety", new List<string> { "Online safety includes protecting your identity, privacy, and data on the internet.", "It involves understanding threats like cyber predators, scams, and information leaks.", "Online safety is a shared responsibility between users, service providers, and educators.", "It encompasses both technical measures and awareness of risky behaviors." } },
            { "internet", new List<string> { "The internet is a global network that connects billions of devices for communication and data sharing.", "It originated from a U.S. military project called ARPANET in the late 1960s.", "The internet uses protocols like TCP/IP to transmit information.", "As of 2024, over 5 billion people globally use the internet." } },
            { "cybersecurity", new List<string> { "Cybersecurity involves protecting computer systems, networks, and data from unauthorized access or attacks.", "It includes practices like risk assessment, encryption, and network defense.", "Cybersecurity is essential for governments, businesses, and individuals alike.", "The field of cybersecurity is constantly evolving to combat new digital threats." } },
            { "cyber security", new List<string> { "Cyber security and cybersecurity are interchangeable terms in modern usage.", "It addresses threats ranging from hacking and data breaches to ransomware and insider threats.", "The global cyber security market is valued in the hundreds of billions of dollars.", "Cyber security includes both offensive (ethical hacking) and defensive strategies." } },
            { "cyber threats", new List<string> { "Cyber threats include any potential malicious attack that seeks to damage or steal data.", "Common threats include phishing, ransomware, spyware, and denial-of-service attacks.", "Threat actors may include hackers, cybercriminals, nation-states, or insiders.", "Cyber threat intelligence helps organizations prepare for and respond to attacks." } },
            { "data protection", new List<string> { "Data protection involves securing personal and sensitive data from corruption, compromise, or loss.", "It is governed by laws like the GDPR (Europe) and CCPA (California).", "Data breaches can lead to identity theft, financial loss, and reputational damage.", "Data protection measures include encryption, access control, and data minimization." } },
            { "identity theft", new List<string> { "Identity theft occurs when someone uses another person's personal information without permission.", "Stolen identities are often used for fraud, such as opening accounts or making purchases.", "Common sources of identity theft include data breaches and phishing scams.", "Victims may face financial loss and a lengthy recovery process." } },
            { "social engineering", new List<string> { "Social engineering is the use of manipulation to influence people into revealing confidential information.", "It often exploits human psychology rather than technical vulnerabilities.", "Examples include pretexting, baiting, and impersonation.", "Social engineering is a major component of many cyber attacks." } },
            { "ransomware", new List<string> { "Ransomware is a type of malware that encrypts files and demands payment for their release.", "Victims are often asked to pay in cryptocurrencies like Bitcoin.", "Ransomware attacks have targeted hospitals, governments, and large corporations.", "The first known ransomware attack was the 'AIDS Trojan' in 1989." } },
            { "safe browsing", new List<string> { "Safe browsing means avoiding risky sites and downloads that can harm your device.", "Always check the URL before entering personal information—it should start with 'https://'.", "Avoid clicking on pop-ups or ads from unfamiliar websites.", "Use a secure, updated browser and enable phishing protection features." } },
            { "password", new List<string> { "A strong password should be at least 12 characters long and include letters, numbers, and symbols.", "Avoid using personal information like birthdays or names in your passwords.", "Use a different password for each of your accounts to minimize risk.", "Consider using a password manager to generate and store strong, unique passwords." } }
        };

        // Constructor: sets up output delegates and user name callback
        public CyberBot(Action<string> chatOutputCallback, Action<string> gameOutputCallback, Func<string> getUserNameCallback)
        {
            chatOutput = chatOutputCallback;
            gameOutput = gameOutputCallback;
            getUserName = getUserNameCallback;
            output = chatOutputCallback;
            userName = null;
        }

        // Public method for MainWindow to log quiz actions
        public void LogQuizAction(string description)
        {
            LogAction(description);
        }

        // Adds an entry to the activity log with timestamp
        private void LogAction(string description)
        {
            string entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {description}";
            activityLog.Add(entry);
        }

        // Main method to process user input and route to correct logic
        public void ProcessInput(string input)
        {
            // Handle first input as user name
            if (userName == null)
            {
                userName = string.IsNullOrWhiteSpace(input) ? "User" : input;
                Respond($"Welcome, {userName}! Let me help you to browse safely on the internet.");
                Respond("Type a topic or question (e.g. 'phishing', 'password', 'safe browsing').");
                Respond("Type 'topics' to see suggestions of topics or 'exit' to quit.");
                Respond("You can also manage cybersecurity tasks: add/view/complete/delete task.");
                Respond("Or play the Cybersecurity Mini-Game: type 'play game'.");
                return;
            }

            input = input.ToLower().Trim();
            var (intent, details, reminderDate) = ParseUserInput(input);

            // Start quiz if requested
            if (intent == "quiz")
            {
                quizActive = true;
                quizScore = 0;
                quizQuestionIndex = 0;
                gameOutput("Let's play the Cybersecurity Quiz!");
                AskQuizQuestion();
                LogAction("Started quiz.");
                return;
            }

            // --- Quiz/Game logic ---
            if (quizActive)
            {
                // Handle answer
                if (quizQuestionIndex >= 0 && quizQuestionIndex < quizQuestions.Count)
                {
                    var q = quizQuestions[quizQuestionIndex];
                    int selected = -1;
                    // Accept "A", "B", "C", "D" or "1", "2", "3", "4"
                    if (input.Length == 1 && input[0] >= 'a' && input[0] <= 'd')
                        selected = input[0] - 'a';
                    else if (int.TryParse(input, out int num) && num >= 1 && num <= 4)
                        selected = num - 1;

                    if (selected == q.CorrectIndex)
                    {
                        quizScore++;
                        gameOutput("Correct!");
                    }
                    else
                    {
                        gameOutput($"Incorrect. The correct answer was: {q.Options[q.CorrectIndex]}");
                    }
                    quizQuestionIndex++;
                    if (quizQuestionIndex < quizQuestions.Count)
                    {
                        AskQuizQuestion();
                    }
                    else
                    {
                        gameOutput($"Quiz complete! Your score: {quizScore}/{quizQuestions.Count}");
                        LogAction($"Quiz completed. Score: {quizScore}/{quizQuestions.Count}");
                        quizActive = false;
                    }
                }
                return;
            }

            // Start quiz if requested (redundant, but safe)
            if (intent == "quiz")
            {
                quizActive = true;
                quizScore = 0;
                quizQuestionIndex = 0;
                Respond("Let's play the Cybersecurity Quiz!");
                AskQuizQuestion();
                LogAction("Started quiz.");
                return;
            }


            // Activity log command
            if (intent == "summary")
            {
                if (activityLog.Count == 0)
                {
                    Respond("No recent activity to show.");
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Here's a summary of recent actions:");
                    int count = 1;
                    foreach (var entry in activityLog)
                    {
                        sb.AppendLine($"{count++}. {entry}");
                    }
                    Respond(sb.ToString());
                }
                return;
            }

            // Add reminder as a task
            if (intent == "reminder" && !string.IsNullOrWhiteSpace(details))
            {
                var task = new TaskItem { Title = details, Description = details, Reminder = reminderDate, Completed = false };
                tasks.Add(task);
                LogAction(reminderDate.HasValue
                    ? $"Reminder set for \"{details}\" on {reminderDate.Value:g}."
                    : $"Reminder set for \"{details}\" (no specific date).");
                return;
            }
            // Add a new task
            if (intent == "addtask" && !string.IsNullOrWhiteSpace(details))
            {
                // Split details by comma for name, description, and reminder
                var parts = details.Split(',', StringSplitOptions.TrimEntries);
                string title = parts.Length > 0 ? parts[0] : "";
                string description = parts.Length > 1 ? parts[1] : "";
                string reminderPhrase = parts.Length > 2 ? parts[2] : "";

                DateTime? reminder = null;

                // Try to extract "remind me in X days" from the reminderPhrase
                var match = Regex.Match(reminderPhrase, @"remind me in (\d+) days", RegexOptions.IgnoreCase);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int days))
                {
                    reminder = DateTime.Now.AddDays(days);
                }

                var task = new TaskItem { Title = title, Description = description, Reminder = reminder, Completed = false };
                tasks.Add(task);
                LogAction($"Task added: \"{title}\".");
                return;
            }

            // Multi-step task creation: set description
            if (awaitingTaskDescription && pendingTask != null)
            {
                pendingTask.Description = input;
                awaitingTaskDescription = false;
                awaitingTaskReminder = true;
                LogAction($"Task description set for \"{pendingTask.Title}\".");
                return;
            }
            // Multi-step task creation: set reminder
            if (awaitingTaskReminder && pendingTask != null)
            {
                if (input.StartsWith("remind me in"))
                {
                    var parts = input.Split(new[] { "remind me in", "days" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0 && int.TryParse(parts[0].Trim(), out int days))
                    {
                        pendingTask.Reminder = DateTime.Now.AddDays(days);
                        LogAction($"Reminder set for \"{pendingTask.Title}\" in {days} days.");
                    }
                    else
                    {
                        LogAction($"Failed to set reminder for \"{pendingTask.Title}\".");
                    }
                }
                else if (input.StartsWith("remind me on"))
                {
                    var dateStr = input.Replace("remind me on", "").Trim();
                    if (DateTime.TryParse(dateStr, out DateTime date))
                    {
                        pendingTask.Reminder = date;
                        LogAction($"Reminder set for \"{pendingTask.Title}\" on {date:g}.");
                    }
                    else
                    {
                        LogAction($"Failed to set reminder for \"{pendingTask.Title}\".");
                    }
                }
                else
                {
                    LogAction($"No reminder set for \"{pendingTask.Title}\".");
                }
                tasks.Add(pendingTask);
                LogAction($"Task added: \"{pendingTask.Title}\".");
                pendingTask = null;
                awaitingTaskReminder = false;
                return;
            }

            // Start multi-step task creation
            if (input.StartsWith("add task"))
            {
                string title = input.Substring("add task".Length).Trim();
                if (string.IsNullOrEmpty(title))
                {
                    Respond("Please provide a title for your task.");
                    return;
                }
                pendingTask = new TaskItem { Title = title };
                awaitingTaskDescription = true;
                Respond($"Task \"{title}\" added. Please provide a description.");
                LogAction($"Task started: \"{title}\".");
                return;
            }
            // View all tasks
            if (input == "view tasks")
            {
                // No Respond() here; handled by UI
                LogAction("Viewed tasks.");
                return;
            }
            // Mark a task as complete
            if (input.StartsWith("complete task-"))
            {
                string title = input.Substring("complete task-".Length).Trim();
                var task = tasks.FirstOrDefault(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
                if (task != null)
                {
                    task.Completed = true;
                    LogAction($"Task completed: \"{title}\".");
                }
                else
                {
                    LogAction($"Task \"{title}\" not found.");
                }
                return;
            }
            // Delete a task
            if (input.StartsWith("delete task-"))
            {
                string title = input.Substring("delete task-".Length).Trim();
                var task = tasks.FirstOrDefault(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
                if (task != null)
                {
                    tasks.Remove(task);
                    LogAction($"Task deleted: \"{title}\".");
                }
                else
                {
                    LogAction($"Task \"{title}\" not found.");
                }
                return;
            }

            // Trigger reminders for due tasks
            foreach (var t in tasks.Where(t => t.Reminder.HasValue && !t.Completed && t.Reminder.Value <= DateTime.Now).ToList())
            {
                // No Respond() here; handled by UI if needed
                t.Reminder = null;
                LogAction($"Reminder triggered for \"{t.Title}\".");
            }

            // Sentiment detection and response
            foreach (var sentiment in sentimentResponses.Keys)
            {
                if (input.Contains(sentiment))
                {
                    Respond(sentimentResponses[sentiment]);
                    string foundTopic = interestTips.Keys.FirstOrDefault(topic => input.Contains(topic));
                    if (!string.IsNullOrEmpty(foundTopic))
                    {
                        Respond($"Here's something about {foundTopic} that might help:");
                        RespondRandom(interestTips[foundTopic]);
                    }
                    else
                    {
                        Respond($"What topic are you {sentiment} about? Since I'm a CyberBot, I can only help with cybersecurity-related topics.");
                    }
                    LogAction($"NLP/Keyword action: \"{input}\" (sentiment: {sentiment}).");
                    return;
                }
            }

            // Remember user interest
            if (input.Contains("i'm interested in") || input.Contains("i am interested in"))
            {
                int index = input.IndexOf("interested in") + "interested in".Length;
                userInterest = input.Substring(index).Trim();
                Respond($"Great! I'll remember that you're interested in {userInterest}. It's a crucial part of staying safe online.");
                LogAction($"User interest set: {userInterest}.");
                return;
            }

            bool matched = false;

            // Provide a tip for a topic if requested
            if (input.Contains("tip"))
            {
                foreach (var topic in interestTips.Keys)
                {
                    if (input.Contains(topic))
                    {
                        Respond($"Here's a tip about {topic}:");
                        RespondRandom(interestTips[topic]);
                        LogAction($"Tip provided for topic: {topic}.");
                        return;
                    }
                }
            }

            // Respond to topic queries
            foreach (var key in topicResponses.Keys)
            {
                if (input.Contains(key))
                {
                    currentTopic = key;
                    RespondRandom(topicResponses[key]);
                    matched = true;
                    LogAction($"Topic discussed: {key}.");
                    break;
                }
            }

            // Handle simple commands and greetings
            var actions = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase)
            {
                { "hey", () => { Respond("Hey there!"); LogAction("Greeted user."); } },
                { "hello", () => { Respond("Hello! I'm your friendly CyberBot, ready to guide you through the world of online safety."); LogAction("Greeted user."); } },
                { "how are you", () => { Respond("I'm just a bot, but I'm functioning perfectly!"); LogAction("Answered 'how are you'."); } },
                { "purpose", () => { Respond("I'm here to educate you about cybersecurity threats and how to protect yourself."); LogAction("Explained purpose."); } },
                { "why are you here", () => { Respond("I'm here to educate you about cybersecurity threats and how to protect yourself."); LogAction("Explained purpose."); } },
                { "what can you do", () => { Respond("I can help you learn about phishing, password safety, safe browsing, malware, scams, and more!"); LogAction("Explained capabilities."); } },
                { "what can i do", () => { Respond("I can help you learn about phishing, password safety, safe browsing, malware, scams, and more!"); LogAction("Explained capabilities."); } },
                { "what can you help with", () => { Respond("I can help you learn about phishing, password safety, safe browsing, malware, scams, and more!"); LogAction("Explained capabilities."); } },
                { "thank you", () => { Respond("You're welcome! I'm here to help you stay safe online."); LogAction("User thanked bot."); } },
                { "thanks", () => { Respond("You're welcome! I'm here to help you stay safe online."); LogAction("User thanked bot."); } },
                { "appreciate it", () => { Respond("You're welcome! I'm here to help you stay safe online."); LogAction("User thanked bot."); } },
                { "appreciate your help", () => { Respond("You're welcome! I'm here to help you stay safe online."); LogAction("User thanked bot."); } },
                { "help", () => { Respond("Type 'Topics' to see suggestions on what you can ask about."); LogAction("Help requested."); } },
                { "what can i ask", () => { Respond("Here are all the topics I can help you with:\n- Phishing\n- Password\n- Password Safety\n- Safe Browsing\n- Malware\n- Scams\n- Cybersecurity\n- Cyber Threats\n- Data Protection\n- Identity Theft\n- Social Engineering\n- Ransomware\nFeel free to type any of these topics to learn more!"); LogAction("Listed topics."); } },
                { "topics", () => { Respond("Here are some topics I can help you with:"); foreach (var topic in topicResponses.Keys) Respond($"- {topic}"); LogAction("Listed topics."); } },
                { "exit", () => { Respond($"Goodbye, {userName}! Stay safe out there."); LogAction("User exited."); } },
                { "quit", () => { Respond($"Goodbye, {userName}! Stay safe out there."); LogAction("User exited."); } },
                { "remember", () => {
                    string memory = $"I remember your name is {userName}.";
                    if (!string.IsNullOrEmpty(userInterest))
                        memory += $" You're interested in {userInterest}.";
                    else
                        memory += " You haven't told me your favorite cybersecurity topic yet.";
                    Respond(memory);
                    LogAction("Recalled user info.");
                } }
            };

            if (!matched)
            {
                foreach (var key in actions.Keys)
                {
                    if (input.Contains(key))
                    {
                        actions[key]();
                        currentTopic = key;
                        matched = true;
                        break;
                    }
                }
            }

            // Handle clarification/follow-up requests
            if (!matched && (
                input.Contains("more") ||
                input.Contains("explain") ||
                input.Contains("what do you mean") ||
                input.Contains("confused") ||
                input.Contains("i don't get it")))
            {
                if (!string.IsNullOrEmpty(currentTopic) && topicResponses.ContainsKey(currentTopic))
                {
                    RespondRandom(topicResponses[currentTopic]);
                    LogAction($"Clarified topic: {currentTopic}.");
                }
                else
                {
                    Respond("Can you tell me what you'd like me to explain more about?");
                    LogAction("Asked for clarification.");
                }
                matched = true;
            }

            // Fallback for unrecognized input
            if (!matched)
            {
                Respond("Hmm, that doesn’t ring a bell. You can ask me for tips or facts about topics like phishing, malware, or online safety. Try 'topics' to see the full list!");
                LogAction($"Unrecognized input: \"{input}\".");
            }

            // Occasionally provide a tip based on user interest
            if (!string.IsNullOrEmpty(userInterest) && userInputCount >= nextTipTrigger && interestTips.ContainsKey(userInterest))
            {
                Respond($"As someone interested in {userInterest}, here's a tip that may help you:");
                RespondRandom(interestTips[userInterest]);
                LogAction($"Interest-based tip for: {userInterest}.");
                userInputCount = 0;
                nextTipTrigger = new Random().Next(5, 8);
            }

            userInputCount++;
        }

        // Outputs the current quiz question and options to the game panel
        private void AskQuizQuestion()
        {
            var q = quizQuestions[quizQuestionIndex];
            var options = new[] { "A", "B", "C", "D" };
            var sb = new StringBuilder();
            sb.AppendLine($"Question {quizQuestionIndex + 1}: {q.Question}");
            for (int i = 0; i < q.Options.Length; i++)
            {
                sb.AppendLine($"{options[i]}) {q.Options[i]}");
            }
            gameOutput(sb.ToString().Trim());
        }

        // Sends a message to the chat output
        private void Respond(string message)
        {
            output(message);
        }

        // Sends a random message from a list to the chat output
        private void RespondRandom(List<string> responses)
        {
            Random rnd = new Random();
            int index = rnd.Next(responses.Count);
            Respond(responses[index]);
        }

        // NLP Simulation: Parses user input and returns intent, details, and optional reminder date
        private (string intent, string details, DateTime? reminderDate) ParseUserInput(string input)
        {
            input = input.ToLower();

            // 1. Recognize greetings
            if (Regex.IsMatch(input, @"\b(hi|hello|hey|greetings)\b"))
                return ("greeting", "", null);

            // 2. Recognize gratitude
            if (Regex.IsMatch(input, @"\b(thank(s| you)?|appreciate)\b"))
                return ("thanks", "", null);

            // 3. Recognize quiz/game
            if (Regex.IsMatch(input, @"\b(play|start|begin|launch)\b.*\b(quiz|game)\b") ||
                input.Contains("quiz") || input.Contains("mini-game") || input.Contains("play game") || input.Contains("start quiz"))
                return ("quiz", "", null);

            // 4. Recognize task creation
            if (Regex.IsMatch(input, @"\b(add|create|make|new)\b.*\b(task|reminder)\b") ||
                input.Contains("add a task to") || input.Contains("add task to") || input.Contains("create a task to") ||
                input.StartsWith("add task") || input.StartsWith("create task"))
            {
                string details = "";
                var match = Regex.Match(input, @"\b(add|create|make|new)\b.*\b(task|reminder)\b(.*)");
                if (match.Success)
                    details = match.Groups[3].Value.Trim();
                else if (input.Contains("add a task to"))
                    details = input[(input.IndexOf("add a task to") + "add a task to".Length)..].Trim();
                else if (input.Contains("add task to"))
                    details = input[(input.IndexOf("add task to") + "add task to".Length)..].Trim();
                else if (input.Contains("create a task to"))
                    details = input[(input.IndexOf("create a task to") + "create a task to".Length)..].Trim();
                else if (input.StartsWith("add task"))
                    details = input["add task".Length..].Trim(new[] { '-',' ' });
                else if (input.StartsWith("create task"))
                    details = input["create task".Length..].Trim(new[] { '-', ' ' });

                return ("addtask", details, null);
            }

            // 5. Recognize task completion
            if (Regex.IsMatch(input, @"\b(complete|finish|done|mark as complete)\b.*\btask\b") ||
                input.StartsWith("complete task-"))
            {
                string details = "";
                var match = Regex.Match(input, @"\btask\b(.*)");
                if (match.Success)
                    details = match.Groups[1].Value.Trim();
                else if (input.StartsWith("complete task-"))
                    details = input.Substring("complete task-".Length).Trim();
                return ("completetask", details, null);
            }

            // 6. Recognize task deletion
            if (Regex.IsMatch(input, @"\b(delete|remove|discard)\b.*\btask\b") ||
                input.StartsWith("delete task-"))
            {
                string details = "";
                var match = Regex.Match(input, @"\btask\b(.*)");
                if (match.Success)
                    details = match.Groups[1].Value.Trim();
                else if (input.StartsWith("delete task-"))
                    details = input.Substring("delete task-".Length).Trim();
                return ("deletetask", details, null);
            }

            // 7. Recognize reminders (with more flexible patterns)
            if (Regex.IsMatch(input, @"\b(remind|reminder|alert|notify)\b") ||
                input.Contains("remind me to") || input.Contains("set a reminder to") || input.Contains("remind me about"))
            {
                DateTime? date = null;
                if (input.Contains("tomorrow")) date = DateTime.Now.AddDays(1);
                var inDaysMatch = Regex.Match(input, @"in (\d+) days");
                if (inDaysMatch.Success && int.TryParse(inDaysMatch.Groups[1].Value, out int days))
                    date = DateTime.Now.AddDays(days);
                var onDateMatch = Regex.Match(input, @"on (\d{4}-\d{2}-\d{2}|\w+ \d{1,2}(, \d{4})?)");
                if (onDateMatch.Success && DateTime.TryParse(onDateMatch.Groups[1].Value, out DateTime parsedDate))
                    date = parsedDate;

                string details = "";
                var detailMatch = Regex.Match(input, @"remind(er)?( me)? (to|about)? (.+)", RegexOptions.IgnoreCase);
                if (detailMatch.Success)
                    details = detailMatch.Groups[4].Value.Trim();
                else if (input.Contains("remind me to"))
                    details = input[(input.IndexOf("remind me to") + "remind me to".Length)..].Trim();
                else if (input.Contains("set a reminder to"))
                    details = input[(input.IndexOf("set a reminder to") + "set a reminder to".Length)..].Trim();
                else if (input.Contains("remind me about"))
                    details = input[(input.IndexOf("remind me about") + "remind me about".Length)..].Trim();

                details = Regex.Replace(details, @"(in \d+ days|on \d{4}-\d{2}-\d{2}|on \w+ \d{1,2}(, \d{4})?|tomorrow)", "", RegexOptions.IgnoreCase).Trim();

                return ("reminder", details, date);
            }

            // 8. Recognize topic queries (with synonyms)
            var topicSynonyms = new Dictionary<string, string[]>
                {
                    { "phishing", new[] { "phishing", "scam email", "fake email" } },
                    { "password", new[] { "password", "passcode", "login code" } },
                    { "malware", new[] { "malware", "virus", "trojan", "worm", "spyware" } },
                    { "scam", new[] { "scam", "fraud", "con", "hoax" } },
                    { "internet safety", new[] { "internet safety", "web safety", "online safety" } },
                    { "cybersecurity", new[] { "cybersecurity", "cyber security", "cyber threats", "security online" } },
                    { "data protection", new[] { "data protection", "data privacy", "protect data" } },
                    { "identity theft", new[] { "identity theft", "stolen identity" } },
                    { "social engineering", new[] { "social engineering", "manipulation", "trick" } },
                    { "ransomware", new[] { "ransomware", "ransom virus" } },
                    { "safe browsing", new[] { "safe browsing", "safe web", "secure browsing" } }
                };
            foreach (var kvp in topicSynonyms)
            {
                foreach (var syn in kvp.Value)
                {
                    if (input.Contains(syn))
                        return ("topic", kvp.Key, null);
                }
            }

            // 9. Recognize help/assistance
            if (Regex.IsMatch(input, @"\b(help|assist|support|what can you do)\b"))
                return ("help", "", null);

            // 10. Recognize exit/quit
            if (Regex.IsMatch(input, @"\b(exit|quit|close|bye|goodbye)\b"))
                return ("exit", "", null);

            // 11. Activity log/summary detection
            if (input.Contains("show activity log") || input.Contains("what have you done for me") ||
                input.Contains("recent actions") || input.Contains("summary"))
                return ("summary", "", null);

            // 12. Fallback to original topic detection for any missed topics
            string[] topics = { "phishing", "password", "malware", "scam", "internet safety", "online safety", "cybersecurity", "cyber security", "cyber threats", "data protection", "identity theft", "social engineering", "ransomware", "safe browsing" };
            foreach (var topic in topics)
            {
                if (input.Contains(topic))
                    return ("topic", topic, null);
            }

            // Fallback
            return ("", "", null);
        }
    }
}
//------------------------------------------------------------------------------------------------------------------------------------------------------------//
                        // REFERENCES:
                        // ChatGPT was used to generate prompts and responses for the CyberBot class.
                        //---------------------------------------------------------------------------//