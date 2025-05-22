namespace BudgetTracker.Models
{
    // Represents a single financial transaction (income or expense)
    public class Transaction
    {
        // A short description of the transaction (e.g., "Salary", "Groceries")
        public required string Description { get; set; }

        // The amount of the transaction (positive for income, negative for expenses)
        public decimal Amount { get; set; }
    }

    public class TransactionData
    {
        // A list of all transactions made by the user
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();

        // The running balance for the user
        public decimal Balance { get; set; }
    }
}