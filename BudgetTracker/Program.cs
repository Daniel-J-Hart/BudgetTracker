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
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }

    class Program
    {
        static string usersFile = "users.json";
        static string transactionsFile = "transactions.json";
        static Dictionary<string, TransactionData> allUserData = LoadAllUserData();
        static string currentUser = "";
        static TransactionData CurrentUserData => allUserData[currentUser];

        static Dictionary<string, TransactionData> LoadAllUserData()
        {
            // If the transactions.json file doesn't exist, return an empty dictionary
            if (!File.Exists("transactions.json"))
                return new Dictionary<string, TransactionData>();

            // Otherwise, read and deserialize the file into a dictionary
            string json = File.ReadAllText("transactions.json");
            return JsonConvert.DeserializeObject<Dictionary<string, TransactionData>>(json)
                ?? new Dictionary<string, TransactionData>(); // Ensure a valid return even if the deserialization results in null
        }

        static void SaveAllUserData()
        {
            string json = JsonConvert.SerializeObject(allUserData, Formatting.Indented);
            File.WriteAllText("transactions.json", json);
        }


        static string GetTransactionFilePath(string username)
        {
            return $"transactions_{username}.json";
        }

        static void Main()
        {
            Console.WriteLine("Welcome to Budget Tracker Authentication.");
            Console.WriteLine("1. Register\n2. Login");
            Console.Write("Choose option: ");
            string option = Console.ReadLine();

            bool success = false;

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

            if (success)
            {
                MainProgram();
            }
        }

        static void SaveUsers(List<User> users)
        {
            string json = JsonConvert.SerializeObject(users, Formatting.Indented);
            File.WriteAllText(usersFile, json);
        }

        static bool Register()
        {
            Console.Write("Enter username: ");
            string username = Console.ReadLine();

            var users = LoadUsers();
            if (users.Any(u => u.Username == username))
            {
                Console.WriteLine("Username already exists.");
                return false;
            }

            Console.Write("Enter password: ");
            string password = Console.ReadLine();

            string hash = PasswordHelper.HashPassword(password);
            users.Add(new User { Username = username, PasswordHash = hash });

            SaveUsers(users);

            // set current user
            currentUser = username;

            // Add an entry for this user in the transaction dictionary
            allUserData[currentUser] = new TransactionData();
            SaveAllUserData();

            Console.WriteLine("User registered successfully!");
            return true;
        }
        static bool Login()
        {
            Console.Write("Enter username: ");
            string username = Console.ReadLine();
            Console.Write("Enter password: ");
            string password = Console.ReadLine();

            var users = LoadUsers();
            var user = users.FirstOrDefault(u => u.Username == username);

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

        static List<User> LoadUsers()
        {
            if (!File.Exists(usersFile)) return new List<User>();
            string json = File.ReadAllText(usersFile);
            return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
        }
        static List<Transaction> Transactions = new List<Transaction>();
        static decimal balance = 0;

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

                string choice = Console.ReadLine();
                Console.WriteLine();

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

        static void AddTransaction()
        {
            Console.Write("Enter description: ");
            string desc = Console.ReadLine();

            Console.Write("Enter amount (positive for income, negative for expense): ");
            if (decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                CurrentUserData.Transactions.Add(new Models.Transaction { Description = desc, Amount = amount });
                CurrentUserData.Balance += amount;

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