# Cybersecurity Awareness Bot (CyberBot) - ST10450618

CyberBot is a console-based application designed to educate users about cybersecurity topics and promote safe online practices. It provides interactive, dynamic guidance on topics such as phishing, password safety, malware, scams, and more.

---

## Features

- **Interactive Chat:** Type questions or topics, and CyberBot will respond with helpful, context-aware information.
- **Keyword Recognition:** Detects cybersecurity-related keywords (e.g., "password", "scam", "privacy") in user input and provides relevant tips or facts.
- **Randomized Responses:** For each topic, CyberBot selects a random tip or fact from a list, making conversations varied and engaging.
- **Natural Conversation Flow:** Maintains context, allowing for follow-up questions and clarifications without restarting the conversation.
- **Memory and Recall:** Remembers your name and favorite cybersecurity topic to personalize future responses and tips.
- **Sentiment Detection:** Detects user sentiment (e.g., "worried", "curious", "frustrated") and responds empathetically.
- **Audio Greetings:** Plays a greeting sound and voice message when the bot starts.
- **ASCII Art:** Displays a visually appealing ASCII art banner.
- **Typing Effect:** Simulates a typing effect for responses to enhance user experience.
- **Robust Error Handling:** Provides helpful fallback responses for unrecognized input, ensuring smooth conversation flow.
- **Efficient Data Structures:** Uses dictionaries and lists to organize topics, tips, and responses for optimal performance and easy expansion.

---

## Prerequisites

- **C# Version:** 7.3
- **.NET Framework:** 4.8
- **Dependencies:**
  - `System.Speech.Synthesis` (for text-to-speech functionality)
  - `System.Media` (for playing audio files)

---

## Installation

1. Clone the repository or download the source code.
2. Open the solution in Visual Studio 2022.
3. Ensure the required audio files (`greeting.wav` and `VoiceGreeting.wav`) are placed in the same directory as the executable.
4. Build the solution to restore dependencies.

---

## Usage

1. Run the application by pressing F5 in Visual Studio or executing the compiled `.exe` file.
2. Interact with CyberBot as follows:

   - **Step 1: Start the Bot**
     - The bot greets you with audio and displays ASCII art.
   - **Step 2: Enter Your Name**
     - You’ll be prompted for your name. If left blank, the bot will call you "User."
   - **Step 3: Explore Topics**
     - Type a topic or question (e.g., "phishing", "password safety") to learn more.
     - Type `topics` to see a list of all available topics.
     - Type `exit` to quit the application.
   - **Step 4: Interactive Responses**
     - CyberBot responds with helpful information, tips, or facts.
     - If your input is unrecognized, the bot will suggest typing `topics` for help.

---

## Available Topics

- Phishing: Learn how to identify and avoid phishing scams.
- Password Safety: Tips for creating and managing strong passwords.
- Safe Browsing: Best practices for staying safe online.
- Malware: Understand what malware is and how to protect yourself.
- Scams: Recognize and avoid common online scams.
- Cybersecurity: General tips to protect your devices and data.
- Data Protection: Safeguard your personal information effectively.
- Identity Theft: Tips to prevent and detect identity theft.
- Social Engineering: Avoid being tricked into giving away information.
- Ransomware: Learn how to protect yourself from ransomware attacks.
- And more—type `topics` to see the full list.

---

## Code Overview

### Main Components

1. **Program.cs**
   - Entry point of the application.
   - Instantiates and starts the CyberBot.

2. **CyberBot.cs**
   - Core functionality of the bot.
   - Handles user interaction, audio playback, and topic responses.

### Key Methods

- `Run()`: Initializes the bot and starts the main menu.
- `PlayGreeting()`, `PlayVoice()`: Play audio greetings.
- `ShowAsciiArt()`: Displays ASCII art in the console.
- `GreetUser()`: Prompts for and stores the user's name.
- `MainMenu()`:  
  - Main loop for user interaction.
  - Handles keyword recognition, sentiment detection, memory, random responses, and error handling.
- `Respond()`, `RespondRandom()`: Display responses with typing effect and random selection.
- `HandleTopic()`: Handles topic-specific requests and follow-up questions.

### Data Structures

- **Dictionaries:**  
  - `interestTips`: Maps topics to lists of tips.
  - `topicResponses`: Maps topics to lists of facts/explanations.
  - `sentimentResponses`: Maps sentiment keywords to empathetic responses.

---

## Troubleshooting

- **Audio Playback Issues:** Ensure `.wav` files are in the correct directory.
- **Unrecognized Input:** Type `topics` to see a list of supported topics.

---

## References

- ChatGPT was used to assist in generating prompts and responses for the bot.

---

**Enjoy learning about cybersecurity with CyberBot!**

---

# Part 2: Advanced Features & Testing

This section highlights the new features and how to test them.

## New Features in Part 2

- **Keyword Recognition:**  
  The bot recognizes keywords like "password", "scam", and "privacy" in user input and provides relevant cybersecurity guidance.

- **Random Responses:**  
  For topics such as phishing, password safety, and scams, the bot randomly selects from multiple tips or facts, making each response unique.

- **Conversation Flow:**  
  The bot maintains context, allowing for follow-up questions (e.g., "Can you explain more?") and continues the current topic without restarting.

- **Memory and Recall:**  
  The bot remembers your name and favorite cybersecurity topic (e.g., "I'm interested in privacy") and uses this information to personalize future responses.

- **Sentiment Detection:**  
  The bot detects simple sentiments like "worried", "curious", or "frustrated" and adjusts its responses to provide encouragement or support.

- **Error Handling:**  
  The bot provides a default response for unknown or unrecognized input, ensuring the conversation continues smoothly.

- **Code Optimization:**  
  Uses dictionaries and lists to organize responses, making the code modular, efficient, and easy to expand.

---

## How to Test the New Features

1. **Keyword Recognition**
   - Type: `Tell me about password safety`
   - Expected: The bot recognizes "password" and responds with a relevant tip.

2. **Random Responses**
   - Type: `Give me a phishing tip` multiple times.
   - Expected: The bot provides different tips each time.

3. **Conversation Flow**
   - Ask about a topic, then type: `Can you explain more?` or `I don't get it`
   - Expected: The bot continues with more information about the current topic.

4. **Memory and Recall**
   - Type: `I'm interested in privacy`
   - Expected: The bot remembers your interest and personalizes future tips.
   - Type: `remember`
   - Expected: The bot recalls your name and interest.

5. **Sentiment Detection**
   - Type: `I'm worried about online scams`
   - Expected: The bot responds empathetically and provides support or tips.

6. **Error Handling**
   - Type: `asdfghjkl` or an unrelated phrase.
   - Expected: The bot gives a default fallback response and suggests typing `topics` for help.

7. **Topic Listing and Help**
   - Type: `topics` or `help`
   - Expected: The bot lists all available topics or provides guidance.

8. **Exit**
   - Type: `exit` or `quit`
   - Expected: The bot says goodbye and closes the application gracefully.

---

# Part 3: WPF Application & Major Feature Expansion

This section highlights the new features and how to test them.

## New Features in Part 3

### 1. Task Management with Reminders
- **Add Tasks:** Users can add tasks with a title, description, and an optional reminder (date/time).
- **View Tasks:** All tasks are displayed in a list.
- **Complete/Delete Tasks:** Tasks can be marked as complete or deleted.
- **Reminders:** The bot can remind users about tasks at the specified date/time.

### 2. Cybersecurity Quiz
- **10+ Questions:** The quiz includes at least 10 questions in both true/false and multiple-choice formats.
- **One-at-a-Time:** Questions are presented one at a time.
- **Immediate Feedback:** Users receive instant feedback after each answer, including explanations.
- **Score Tracking:** The bot tracks the user's score and provides a summary and description at the end.

### 3. Advanced NLP (Natural Language Processing)
- **Flexible Keyword Detection:** The bot detects keywords and topics in full phrases or sentences, not just single words.
- **Synonym & Phrase Recognition:** Recognizes synonyms and varied phrasings for tasks, reminders, and cybersecurity topics.
- **Context Awareness:** Maintains conversation context for follow-up questions and clarifications.
- **Sentiment Detection:** Responds empathetically to user sentiment (e.g., worried, frustrated).
- **Memory:** Remembers user name and interests for personalized responses.

### 4. Activity Log / Chat History
- **Comprehensive Tracking:** Logs all user activities, including:
  - Tasks created and reminders set
  - Quiz attempts and scores
  - Last conversation topics or keywords identified
- **Paging:** Displays the last 5–10 activities by default, with a “Show More” button to view additional history in increments.

---

## How to Test the New Features

1. **Task Management**
   - Add a task: `Add a task to update my antivirus tomorrow.`
   - View tasks: Go to the Tasks panel or type `view tasks`.
   - Complete a task: `Complete task- update my antivirus`
   - Delete a task: `Delete task- update my antivirus`
   - Set a reminder: `Remind me to change my password in 7 days.`

2. **Cybersecurity Quiz**
   - Start the quiz: `Play the quiz game.`
   - Answer questions: Respond to each question as prompted.
   - View your score and feedback at the end.

3. **NLP & Conversation**
   - Try flexible phrases: `Can you remind me to check my email security next week?`
   - Use synonyms: `Create a task to backup my files.`
   - Ask for help: `What can you do?` or `help`
   - Test sentiment: `I'm worried about malware.`

4. **Activity Log**
   - View recent activity: Go to the Activity Log panel.
   - Click "Show More" to see additional history.

---

## Notes

- The application is now a WPF project targeting .NET 8 and C# 12.0.
- The UI is modernized for ease of use, with panels for ChatBot, Tasks, Game, and Activity Log.
- All previous features from Parts 1 and 2 are retained and improved.


