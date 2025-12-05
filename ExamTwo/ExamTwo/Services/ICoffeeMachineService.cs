using System;
using ExamTwo.Data.Models;
using ExamTwo.Data.Repositories;

namespace ExamTwo.Services
{
    public interface ICoffeeMachineService
    {
        List<Coffee> GetAvailableCoffees();
        List<Coin> GetAvailableCoins();
        int CalculateOrderTotal(Dictionary<string, int> order);
        OrderResult ProcessOrder(OrderRequest request);
    }
}
