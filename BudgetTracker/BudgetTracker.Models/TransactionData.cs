namespace BudgetTracker.Models
{
    public class Transaction
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }

    public class TransactionData
    {
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
        public decimal Balance { get; set; }
    }
}