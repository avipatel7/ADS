using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Transactions;

namespace ExpenseTracker
{
    class Program
    {
        static void Main(string[] args)
        {
            var expenseTracker = new ExpenseTracker();
            expenseTracker.Run();
        }
    }

    public class ExpenseTracker
    {
        private List<Transaction> transactions = new List<Transaction>();
        private int nextTransactionId = 1;
        private UserAuthentication userAuthentication = new UserAuthentication();
        private Dictionary<string, decimal> budgets = new Dictionary<string, decimal>();
        private List<string> categories = new List<string> { "Food", "Utilities", "Transport" }; // Preset categories

        public void Run()
        {
            Init();
            Console.WriteLine("\nWelcome to Expense Tracker Application. Please login to continue:");
            if (Login())
            {
                MainLoop();
            }
        }

        private void Init()
        {
            userAuthentication.AddUser("test", "test");
        }

        private bool Login()
        {
            Console.Write("Enter email id: ");
            string loginId = Console.ReadLine();
            Console.Write("Enter password: ");
            string pwd = Console.ReadLine();

            if (userAuthentication.AuthenticateUser(loginId, pwd))
            {
                Console.WriteLine("Authentication Successful..!!");
                return true;
            }
            else
            {
                Console.WriteLine("Invalid login. Please try again.");
                return false;
            }
        }

        private void MainLoop()
        {
            bool running = true;
            while (running)
            {
                Console.WriteLine("\n1. View Transactions");
                Console.WriteLine("2. Add Transaction");
                Console.WriteLine("3. Edit/Delete Transaction");
                Console.WriteLine("4. View/Add Categories");
                Console.WriteLine("5. Enter/View Budgets");
                Console.WriteLine("6. Track Spending");
                Console.WriteLine("7. Exit");
                Console.Write("Select an option: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        DisplayTransactions();
                        break;
                    case "2":
                        AddTransaction();
                        break;
                    case "3":
                        EditDeleteTransaction();
                        break;
                    case "4":
                        ViewAddCategories();
                        break;
                    case "5":
                        EnterViewBudgets();
                        break;
                    case "6":
                        TrackSpending();
                        break;
                    case "7":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please select a valid option.");
                        break;
                }
            }
        }

        private void AddTransaction()
        {
            Console.Write("Enter transaction amount: ");
            decimal amount = decimal.Parse(Console.ReadLine());
            Console.Write("Enter transaction type (Income/Expense): ");
            string type = Console.ReadLine();
            Console.Write("Enter category: ");
            string category = Console.ReadLine();

            if (!categories.Contains(category))
            {
                Console.WriteLine($"Category '{category}' does not exist. Please add it under the 'View/Add Categories' menu.");
                return;
            }

            transactions.Add(new Transaction
            {
                Id = nextTransactionId++,
                Amount = amount,
                Type = type.Equals("Income", StringComparison.OrdinalIgnoreCase) ? TransactionType.Income : TransactionType.Expense,
                Category = category,
                Date = DateTime.Now
            });

            Console.WriteLine("Transaction added successfully.");
        }

        private void DisplayTransactions()
        {
            if (transactions.Count == 0)
            {
                Console.WriteLine("No transactions to display.");
                return;
            }

            foreach (var transaction in transactions)
            {
                Console.WriteLine($"Id: {transaction.Id}, Date: {transaction.Date.ToShortDateString()}, Amount: {transaction.Amount:C}, Type: {transaction.Type}, Category: {transaction.Category}");
            }
        }

        private void EditDeleteTransaction()
        {
            Console.WriteLine("Enter transaction ID to edit/delete:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID. Please enter a numeric value.");
                return;
            }

            var transaction = transactions.FirstOrDefault(t => t.Id == id);
            if (transaction == null)
            {
                Console.WriteLine("Transaction not found.");
                return;
            }

            Console.WriteLine("Do you want to (E)dit or (D)elete this transaction? (E/D)");
            string action = Console.ReadLine().Trim().ToUpper();
            if (action == "E")
            {
                EditTransaction(transaction);
            }
            else if (action == "D")
            {
                transactions.Remove(transaction);
                Console.WriteLine("Transaction deleted.");
            }
            else
            {
                Console.WriteLine("Invalid action.");
            }
        }

        private void EditTransaction(Transaction transaction)
        {
            Console.Write("Enter new amount (leave blank to keep current): ");
            string amountInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(amountInput))
            {
                if (decimal.TryParse(amountInput, out decimal newAmount))
                {
                    transaction.Amount = newAmount;
                }
                else
                {
                    Console.WriteLine("Invalid amount. Transaction not updated.");
                    return;
                }
            }

            Console.Write("Enter new type (Income/Expense) or leave blank to keep current: ");
            string typeInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(typeInput))
            {
                transaction.Type = typeInput.Equals("Income", StringComparison.OrdinalIgnoreCase) ? TransactionType.Income : TransactionType.Expense;
            }

            Console.Write("Enter new category or leave blank to keep current: ");
            string categoryInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(categoryInput))
            {
                if (categories.Contains(categoryInput))
                {
                    transaction.Category = categoryInput;
                }
                else
                {
                    Console.WriteLine("Invalid category. Transaction not updated.");
                    return;
                }
            }

            Console.WriteLine("Transaction updated successfully.");
        }

        private void ViewAddCategories()
        {
            Console.WriteLine("Existing categories:");
            foreach (var category in categories)
            {
                Console.WriteLine(category);
            }
            Console.WriteLine("Do you want to add a new category? (Y/N)");
            if (Console.ReadLine().Trim().ToUpper() == "Y")
            {
                Console.Write("Enter new category name: ");
                string newCategory = Console.ReadLine().Trim();
                if (!categories.Contains(newCategory))
                {
                    categories.Add(newCategory);
                    Console.WriteLine($"{newCategory} added.");
                }
                else
                {
                    Console.WriteLine("Category already exists.");
                }
            }
        }

        private void EnterViewBudgets()
        {
            Console.WriteLine("1. Enter budget for a category");
            Console.WriteLine("2. View budgets");
            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    EnterBudget();
                    break;
                case "2":
                    ViewBudgets();
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }

        private void EnterBudget()
        {
            Console.Write("Enter category: ");
            string category = Console.ReadLine().Trim();
            if (!categories.Contains(category))
            {
                Console.WriteLine("Category does not exist.");
                return;
            }
            Console.Write("Enter budget amount: ");
            decimal amount = decimal.Parse(Console.ReadLine());
            budgets[category] = amount;
            Console.WriteLine($"Budget of {amount:C} set for {category}.");
        }

        private void ViewBudgets()
        {
            foreach (var budget in budgets)
            {
                Console.WriteLine($"{budget.Key}: {budget.Value:C}");
            }
        }

        private void TrackSpending()
        {
            foreach (var category in categories)
            {
                decimal spent = transactions.Where(t => t.Category == category).Sum(t => t.Amount);
                decimal budget = budgets.ContainsKey(category) ? budgets[category] : 0;
                Console.WriteLine($"{category} - Spent: {spent:C}, Budget: {budget:C}");
            }
        }
    }

    public class Transaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public TransactionType Type { get; set; }
    }

    public enum TransactionType
    {
        Income,
        Expense
    }

    public class UserAuthentication
    {
        private List<User> users = new List<User>();

        public void AddUser(string loginId, string password)
        {
            users.Add(new User { UserID = users.Count + 1, LoginID = loginId, Password = password });
        }

        public bool AuthenticateUser(string loginId, string password)
        {
            return users.Any(u => u.LoginID == loginId && u.Password == password);
        }
    }

    public class User
    {
        public int UserID { get; set; }
        public string LoginID { get; set; }
        public string Password { get; set; }
    }
}