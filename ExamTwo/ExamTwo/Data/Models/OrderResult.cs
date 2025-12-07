using System;

namespace ExamTwo.Data.Models
{
    public class OrderResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ChangeAmount { get; set; }
        public Dictionary<int, int> ChangeBreakdown { get; set; } = new();
        public List<Coffee> UpdatedCoffees { get; set; } = new();
    }
}