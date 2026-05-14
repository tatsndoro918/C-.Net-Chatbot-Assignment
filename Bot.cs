using System;
using System.Collections.Generic;
using System.Media;

namespace Cybersecurity_Awareness_Bot
{
    public class Bot
    {
        //  MEMORY / STATE
        public static string UserName { get; set; }
        public static string FavouriteTopic { get; set; }
        public static string CurrentTopic { get; set; }

        //  KEYWORD → DEFINITION  (Dictionary)
        static Dictionary<string, string> topicDefinitions = new Dictionary<string, string>()
        {
            { "password",           "A strong password is your first line of defence. Use long, unique passphrases for every account and never reuse them across sites." },
            { "phishing",           "Phishing is when criminals impersonate trusted sources via email, SMS, or fake websites to steal your credentials or install malware." },
            { "safe browsing",      "Safe browsing means verifying HTTPS and the padlock icon before entering sensitive data, and avoiding suspicious downloads or pop-ups." },
            { "mfa",                "Multi-factor authentication (MFA) adds a second verification step — like a code or biometric — making account takeover far harder." },
            { "two-factor",         "Two-factor authentication requires something you know (password) plus something you have (phone code), doubling your account protection." },
            { "updates",            "Software updates patch security vulnerabilities before attackers can exploit them. Enable automatic updates wherever possible." },
            { "malware",            "Malware is malicious software — viruses, spyware, ransomware — that can steal data, spy on you, or lock you out of your own device." },
            { "public wi-fi",       "Public Wi-Fi can expose your traffic to eavesdroppers. Always look for HTTPS and avoid accessing banking or private accounts on open networks." },
            { "wifi",               "Unsecured Wi-Fi networks are a common attack vector. Use a VPN or mobile data when accessing sensitive services away from home." },
            { "social engineering", "Social engineering manipulates people — not machines — into revealing credentials or taking unsafe actions through pressure or deception." },
            { "privacy",            "Your personal data is valuable. Limit what you share online, review app permissions regularly, and read privacy policies before signing up." },
            { "personal information","Never share sensitive personal details — ID numbers, banking info, passwords — unless you fully trust the platform requesting them." },
            { "identity theft",     "Identity theft happens when someone uses your personal data fraudulently, potentially draining bank accounts or ruining your credit score." },
            { "financial loss",     "Cybercrime can cause direct financial loss. Report fraud immediately to your bank and relevant authorities to begin recovery as fast as possible." },
            { "scam",               "Online scams exploit urgency and trust. If an offer seems too good to be true, or a message demands immediate action, treat it with extreme suspicion." },
            { "ransomware",         "Ransomware encrypts your files and demands payment for the key. Regular offline backups are your best defence against this devastating attack." },
            { "vpn",                "A VPN (Virtual Private Network) encrypts your internet traffic, hiding your activity from your ISP and potential eavesdroppers on shared networks." },
            { "firewall",           "A firewall monitors and controls incoming and outgoing network traffic based on security rules, acting as a barrier between your device and threats." },
            { "encryption",         "Encryption converts your data into unreadable ciphertext. Even if attackers steal encrypted data, they cannot use it without the decryption key." },
            { "psychological harm",  "Cybercrime causes real emotional distress — stress, anxiety, and loss of trust. Seeking support quickly is just as important as practical recovery steps." },
            { "emotional impact",   "Falling victim to a cyber attack can be deeply unsettling. Remember: it can happen to anyone, and help is available to guide your recovery." }
        };

        //  KEYWORD → MULTIPLE RANDOM TIPS  (Dictionary of Lists)
        static Dictionary<string, List<string>> topicTips = new Dictionary<string, List<string>>()
        {
            { "password", new List<string> {
                "Use a passphrase of four or more random words — it's easier to remember and harder to crack than a complex short password.",
                "Never reuse a password. If one site is breached, attackers will try the same credentials everywhere else.",
                "A password manager generates and stores unique passwords for every account so you only need to remember one master password.",
                "Avoid using your name, birthday, or pet's name — attackers try personal details first in targeted attacks.",
                "Enable breach-monitoring services like Have I Been Pwned to get alerted if your credentials are ever leaked."
            }},
            { "phishing", new List<string> {
                "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations.",
                "Check the sender's actual email address, not just the display name — spoofed addresses are a classic phishing trick.",
                "Hover over links before clicking to preview the real URL. If it looks odd or unexpected, don't click.",
                "Legitimate organisations will never ask for your password or PIN via email or text message.",
                "When in doubt, go directly to the official website by typing the URL yourself rather than clicking any link."
            }},
            { "scam", new List<string> {
                "If someone contacts you out of the blue asking for money or gift cards, it is almost certainly a scam.",
                "Romance scams build fake relationships over months before requesting money. Be cautious of online relationships that never meet in person.",
                "Tech support scams often start with a pop-up claiming your computer is infected. Real companies don't contact you this way.",
                "Investment scams promise unrealistic returns. If it sounds too good to be true, it almost certainly is.",
                "Verify any unexpected prize, lottery win, or inheritance claim through official channels before sharing any personal details."
            }},
            { "malware", new List<string> {
                "Install reputable antivirus software and keep its definitions updated to catch the latest known threats.",
                "Never download software from unofficial sources — always use official app stores or the vendor's own website.",
                "Be wary of USB drives found in public places; they are a known malware delivery method.",
                "Ransomware spreads quickly through networks. Back up your critical files offline or to a separate cloud account regularly.",
                "If you suspect malware, disconnect from the internet immediately and run a full system scan before doing anything else."
            }},
            { "privacy", new List<string> {
                "Review your social media privacy settings regularly — platforms change their defaults and your exposure may have grown.",
                "Think before you post: birthdate, home address, and pet names are often used as security question answers.",
                "Use a separate email address for online shopping and newsletters to limit spam and data exposure.",
                "Delete apps you no longer use; they may still collect data in the background.",
                "Be cautious of free apps — if you are not paying for the product, your data is often the product."
            }},
            { "safe browsing", new List<string> {
                "Always check for HTTPS and the padlock icon in the address bar before entering any sensitive information.",
                "Use a browser extension like uBlock Origin to block malicious ads and tracking scripts.",
                "Avoid clicking on ads, especially those that appear in search results — malvertising is a real threat.",
                "Clear your browser cookies and cache regularly to reduce tracking across websites.",
                "Consider using a privacy-focused browser or search engine to minimise data collection on your browsing habits."
            }}
        };

        //  SENTIMENT RESPONSES  (Lists)
        static List<string> worriedResponses = new List<string>()
        {
            "It's completely understandable to feel that way — cyber threats are real and affect many people. You're already taking a great step by learning about them.",
            "Feeling worried shows you care about your safety, and that's a strength. Let me share some practical tips to help you feel more in control.",
            "Don't be discouraged — even small cybersecurity habits make a significant difference. We'll work through this together."
        };

        static List<string> frustratedResponses = new List<string>()
        {
            "I hear you — cybersecurity can feel overwhelming. Let's slow down and tackle it one topic at a time.",
            "Frustration is valid! Security advice can sometimes feel contradictory. Ask me about a specific topic and I'll give you clear, actionable guidance.",
            "Let's simplify this. Tell me what you're struggling with most and I'll focus on just that."
        };

        static List<string> confusedResponses = new List<string>()
        {
            "No worries at all — cybersecurity has a lot of jargon. Ask me to explain any term and I'll break it down simply.",
            "Confusion is the first step to understanding! Which part would you like me to clarify?",
            "It can be a lot to take in. Try asking about one specific topic — like passwords or phishing — and we'll go from there."
        };

        static List<string> curiousResponses = new List<string>()
        {
            "Love the curiosity! Staying informed is one of the best things you can do for your online safety. ",
            "Great question! Curiosity is exactly the right attitude when it comes to cybersecurity. ",
            "Asking questions is how you stay ahead of the threats. Keep it up! "
        };

        //  WELCOME
        public static string Welcome()
        {
            return "Welcome to the Cybersecurity Awareness Bot! What is your name?";
        }

        //  DELEGATE — PLAY INTRO SOUND
        public delegate void SoundAction();

        public static void PlayIntroSound()
        {
            SoundAction playSound = () =>
            {
                string filePath = "C:\\Users\\user\\Desktop\\SCHOO\\PROG Part 2 Final\\Cybersecurity_Awareness_Bot\\Cybersecurity_Awareness_Bot\\intro.wav";
                try
                {
                    SoundPlayer player = new SoundPlayer(filePath);
                    player.Load();
                    player.Play();
                }
                catch
                {
                    
                }
            };
            playSound();
        }

        //  MAIN CONVERSATION LOGIC
        public static string Conversation(string message)
        {
            message = message.ToLower().Trim();
            Random rand = new Random();
            string response = "";
            bool matched = false;

            // Purpose / general 
            if (message.Contains("purpose") || message.Contains("what do you do") || message.Contains("what can you do"))
            {
                response = $"My purpose is to help you stay safe online, {UserName}! I can answer questions about cybersecurity topics, share tips, and help you build safer digital habits. Try asking about passwords, phishing, malware, privacy, scams, or any other cybersecurity topic.";
                matched = true;
            }
            else if (message.Contains("how are you"))
            {
                response = "I'm running smoothly and ready to help you stay safe online! What cybersecurity topic can I help you with today?";
                matched = true;
            }

            // Sentiment detection
            else if (message.Contains("worried") || message.Contains("scared") || message.Contains("anxious") || message.Contains("nervous") || message.Contains("afraid"))
            {
                response = worriedResponses[rand.Next(worriedResponses.Count)];
                // Auto-follow-up tip if topic also mentioned in same message
                foreach (string key in topicDefinitions.Keys)
                {
                    if (message.Contains(key))
                    {
                        CurrentTopic = key;
                        response += $"\n\nHere's something helpful about {key}: {topicDefinitions[key]}";
                        break;
                    }
                }
                if (!response.Contains("\n\n"))
                    response += "\n\nTell me which topic concerns you most and I'll share some reassuring tips right away.";
                matched = true;
            }
            else if (message.Contains("frustrated") || message.Contains("angry") || message.Contains("annoyed") || message.Contains("mad"))
            {
                response = frustratedResponses[rand.Next(frustratedResponses.Count)];
                matched = true;
            }
            else if (message.Contains("confused") || message.Contains("don't understand") || message.Contains("lost") || message.Contains("unclear"))
            {
                response = confusedResponses[rand.Next(confusedResponses.Count)];
                matched = true;
            }
            else if (message.Contains("curious") || message.Contains("interested") || message.Contains("want to learn"))
            {
                response = curiousResponses[rand.Next(curiousResponses.Count)];
                // Fall through — also try to match a topic keyword below
            }

            // Conversation flow: follow-up requests
            if (message.Contains("tell me more") || message.Contains("explain more") ||
                message.Contains("give me another tip") || message.Contains("more info") ||
                message.Contains("go on") || message.Contains("another tip"))
            {
                if (!string.IsNullOrEmpty(CurrentTopic) && topicTips.ContainsKey(CurrentTopic))
                {
                    response += topicTips[CurrentTopic][rand.Next(topicTips[CurrentTopic].Count)];
                    matched = true;
                }
                else
                {
                    response += "Please ask about a specific topic first — for example, 'Tell me about phishing' — and then I can go deeper!";
                    matched = true;
                }
            }

            // Memory recall
            if (message.Contains("favourite topic") || message.Contains("favorite topic") ||
                message.Contains("what do i like") || message.Contains("my topic"))
            {
                if (!string.IsNullOrEmpty(FavouriteTopic))
                    response += $"You mentioned that your favourite cybersecurity topic is {FavouriteTopic}. That's a great one to keep exploring!";
                else
                    response += "I don't have a favourite topic saved for you yet. Just tell me what interests you most and I'll remember it!";
                matched = true;
            }

            if (message.Contains("my name") || message.Contains("who am i"))
            {
                response += $"Your name is {UserName}!";
                matched = true;
            }

            // Favourite topic memory store trigger
            if (message.Contains("i'm interested in") || message.Contains("i am interested in") ||
                message.Contains("i like") || message.Contains("my favourite is") || message.Contains("my favorite is"))
            {
                foreach (string key in topicDefinitions.Keys)
                {
                    if (message.Contains(key))
                    {
                        FavouriteTopic = key;
                        response += $"Great! I'll remember that you're interested in {key}. It's a crucial part of staying safe online.\n\n{topicDefinitions[key]}";
                        CurrentTopic = key;
                        matched = true;
                        break;
                    }
                }
            }

            // Keyword topic matching
            if (!matched || response.Trim() == "")
            {
                foreach (string key in topicDefinitions.Keys)
                {
                    if (message.Contains(key))
                    {
                        CurrentTopic = key;

                        // Random tip if available, else definition
                        if (topicTips.ContainsKey(key))
                            response += topicTips[key][rand.Next(topicTips[key].Count)];
                        else
                            response += topicDefinitions[key];

                        // Personalise if this is their saved favourite topic
                        if (!string.IsNullOrEmpty(FavouriteTopic) && FavouriteTopic == key)
                            response += $"\n\nAs someone interested in {FavouriteTopic}, you might also want to regularly review the security settings on your accounts.";

                        matched = true;
                        break;
                    }
                }
            }

            // Topics list
            if (message.Contains("topics") || message.Contains("what can i ask") || message.Contains("help"))
            {
                response += "\n\nHere are the topics I can help you with:\n" +
                            "• Password safety\n• Phishing\n• Safe browsing\n• MFA / Two-factor authentication\n" +
                            "• Software updates\n• Malware & Ransomware\n• Public Wi-Fi\n• Social engineering\n" +
                            "• Privacy & Personal information\n• Identity theft\n• Financial loss\n• Scams\n" +
                            "• VPN\n• Firewall\n• Encryption\n• Psychological / Emotional impact";
                matched = true;
            }

            // Default fallback
            if (!matched)
            {
                string[] fallbacks = {
                    $"I'm not sure I understand, {UserName}. Could you try rephrasing? You can ask me about passwords, phishing, malware, privacy, scams, and more.",
                    $"That one's outside my knowledge base right now. Try asking about password safety, safe browsing, or online scams!",
                    "I didn't quite catch that. Try asking: 'Tell me about phishing' or 'Give me a password tip'."
                };
                response = fallbacks[rand.Next(fallbacks.Length)];
            }

            return response;
        }
    }
}

