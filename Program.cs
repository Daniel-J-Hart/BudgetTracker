using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using BudgetTracker.Models;
using BudgetTracker.Helpers;

namespace BudgetTracker
{
    class Transaction
    {
        public required string Description { get; set; }
        public decimal Amount { get; set; }
    }

    class Program
    {
        // Paths to the JSON files for user credentials and transactions
        static string usersFile = "users.json";
    
        // Dictionary to hold all users transaction data in memory
        static Dictionary<string, TransactionData> allUserData = LoadAllUserData();
        // The currently logged in user
        static string currentUser = "";
        // Shortcut to access the current user's data
        static TransactionData CurrentUserData => allUserData[currentUser];

        // Load transaction data from JSON file for all users
        static Dictionary<string, TransactionData> LoadAllUserData()
        {
            // If the transactions.json file doesn't exist, return an empty dictionary
            if (!File.Exists("transactions.json"))
                return new Dictionary<string, TransactionData>();

            // Otherwise, read and deserialize the file into a dictionary
            string json = File.ReadAllText("transactions.json");
            return JsonConvert.DeserializeObject<Dictionary<string, TransactionData>>(json) // JsonConvert.DeserializeObject takes a JSON-formatted string, parses it, and Creates an instance of a C# class (or list, dictionary, etc.) filled with the data from that JSON
                ?? new Dictionary<string, TransactionData>(); // Ensure a valid return even if the deserialization results in null
        }

        // Save all users transaction data to the JSON file
        static void SaveAllUserData()
        {
            string json = JsonConvert.SerializeObject(allUserData, Formatting.Indented);
            File.WriteAllText("transactions.json", json);
        }

        // Gets the transaction file path for a specific user 
        static string GetTransactionFilePath(string username)
        {
            return $"transactions_{username}.json";
        }

        static void Main()
        {
            Console.WriteLine("Welcome to Budget Tracker Authentication.");
            Console.WriteLine("1. Register\n2. Login");
            Console.Write("Choose option: ");
            string? option = Console.ReadLine();

            if (string.IsNullOrEmpty(option))
            {
                Console.WriteLine("No input detected. Exiting program.");
                return;
            }

            bool success = false;

            // Decide between register or login
            if (option == "1")
            {
                success = Register();
            }
            else if (option == "2")
            {
                success = Login();
            }
            else
            {
                Console.WriteLine("Invalid option.");
                return;
            }

            // If login / register is successful. enter the main program
            if (success)
            {
                MainProgram();
            }
        }

        // Save user list (usernames + password hashes) to JSON file
        static void SaveUsers(List<User> users)
        {
            string json = JsonConvert.SerializeObject(users, Formatting.Indented);
            File.WriteAllText(usersFile, json);
        }

        // Handles user registration 
        static bool Register()
        {
            Console.Write("Enter username: ");
            string? username = Console.ReadLine();

            if (string.IsNullOrEmpty(username))
            {
                Console.WriteLine("Username cannot be empty.");
                return false;
                // Handle invalid input, e.g., ask again or exit
            }

            var users = LoadUsers();

            // Checks if the username is taken
            if (users.Any(u => u.Username == username))
            {
                Console.WriteLine("Username already exists.");
                return false;
            }

            Console.Write("Enter password: ");
            string? password = Console.ReadLine();

            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Password cannot be empty.");
                return false;
            }

            // Hash the password using PBKDF2 (safer than SHA256)
                string hash = PasswordHelper.HashPassword(password);
            users.Add(new User { Username = username, PasswordHash = hash });

            SaveUsers(users);

            // Set the current user
            currentUser = username;

            // Create an empty transaction data entry for the new user
            allUserData[currentUser] = new TransactionData();
            SaveAllUserData();

            Console.WriteLine("User registered successfully!");
            return true;
        }

        // Handles user login logic
        static bool Login()
        {
            Console.Write("Enter username: ");
            string? username = Console.ReadLine();

            if (string.IsNullOrEmpty(username))
            {
                Console.WriteLine("Username cannot be empty.");
                return false;  
            }

            Console.Write("Enter password: ");
            string? password = Console.ReadLine();

            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Username cannot be empty.");
                return false;
            }

            var users = LoadUsers();
            var user = users.FirstOrDefault(u => u.Username == username);

            // Check username and password hash
            if (user == null || !PasswordHelper.VerifyPassword(password, user.PasswordHash))
            {
                Console.WriteLine("Invalid username or password.");
                return false;
            }
            else
            {
                Console.WriteLine($"Welcome, {user.Username}!");
                currentUser = user.Username;

                // Ensure the user has a transaction entry
                if (!allUserData.ContainsKey(currentUser))
                {
                    allUserData[currentUser] = new TransactionData();
                    SaveAllUserData();
                }

                return true;
            }
        }

        // Load list of users from JSON file
        static List<User> LoadUsers()
        {
            if (!File.Exists(usersFile)) return new List<User>();
            string json = File.ReadAllText(usersFile);
            return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
        }

        // Holds all current session transactions
        static List<Transaction> Transactions = new List<Transaction>();

        // Main program loop after login
        static void MainProgram()
        {
            while (true)
            {
                Console.WriteLine("\n--- Budget Tracker ---");
                Console.WriteLine("1. Add Transaction");
                Console.WriteLine("2. View Balance");
                Console.WriteLine("3. View All Transactions");
                Console.WriteLine("4. Exit");
                Console.Write("Choose an option: ");

                string? choice = Console.ReadLine();
                if (string.IsNullOrEmpty(choice))
                {
                    Console.WriteLine("No input recieved.");
                    Environment.Exit(0);
                }
                Console.WriteLine();

                // Handle user menu choices
                switch (choice)
                {
                    case "1":
                        AddTransaction();
                        break;
                    case "2":
                        ShowBalance();
                        break;
                    case "3":
                        ShowTransactions();
                        break;
                    case "4":
                        return;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        // Load transaction data from the JSON file
        static TransactionData LoadTransactions(string path)
        {
            if (!File.Exists(path)) // return empty if the file doesnt exist yet
            {
                return new TransactionData();
            }

            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<TransactionData>(json) ?? new TransactionData();
        }

        // Save transaction data to the JSON file
        static void SaveTransactions(TransactionData data, string path)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        // Adds a new transaction for the current user
        static void AddTransaction()
        {
            Console.Write("Enter description: ");
            string? desc = Console.ReadLine();

            Console.Write("Enter amount (positive for income, negative for expense): ");
            if (decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                // Add transaction and update balance
                CurrentUserData.Transactions.Add(new Models.Transaction { Description = desc ?? "", Amount = amount });
                CurrentUserData.Balance += amount;

                // Save updated data
                SaveAllUserData();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Transaction added successfully!");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid amount.");
                Console.ResetColor();
            }
        }

        // Display current user's balance
        static void ShowBalance()
        {
            var balance = CurrentUserData.Balance;

            if (balance < 0)
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine($"Current Balance: ${balance:F2}");
            Console.ResetColor();
        }

        // Display all current user's transactions
        static void ShowTransactions()
        {
            Console.WriteLine("Transactions:");
            foreach (var t in CurrentUserData.Transactions)
            {
                Console.WriteLine($"{t.Description}: ${t.Amount:F2}");
            }
        }
    }
}