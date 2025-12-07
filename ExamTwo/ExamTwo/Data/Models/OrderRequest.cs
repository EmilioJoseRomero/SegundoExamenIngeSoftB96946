using System;

namespace ExamTwo.Data.Models
{
    public class OrderRequest
    {
        public Dictionary<string, int> Order { get; set; } = new();
        public Payment Payment { get; set; } = new();
    }

    public class Payment
    {
        public int TotalAmount { get; set; }
        public List<int> Coins { get; set; } = new();
        public List<int> Bills { get; set; } = new();
    }
}