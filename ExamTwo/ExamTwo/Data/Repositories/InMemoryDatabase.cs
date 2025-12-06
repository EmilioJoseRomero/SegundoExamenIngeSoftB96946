using System;
using ExamTwo.Data.Models;
using ExamTwo.Data.Repositories;

namespace ExamTwo.Data.Repositories
{
    public class InMemoryDatabase : IDatabase
    {
        private readonly List<Coffee> _coffees = new()
        {
            new Coffee { Name = "Americano", Price = 950, Quantity = 10 },
            new Coffee { Name = "Cappuccino", Price = 1200, Quantity = 8 },
            new Coffee { Name = "Lates", Price = 1350, Quantity = 10 },
            new Coffee { Name = "Mocaccino", Price = 1500, Quantity = 15 }
        };

        private readonly List<Coin> _coins = new()
        {
            new Coin { Denomination = 1000, Quantity = 0 },  //Billetes
            new Coin { Denomination = 500, Quantity = 20 },
            new Coin { Denomination = 100, Quantity = 30 },
            new Coin { Denomination = 50, Quantity = 50 },
            new Coin { Denomination = 25, Quantity = 25 }
        };

        public List<Coffee> GetAllCoffees() => _coffees;

        public Coffee? GetCoffeeByName(string name) =>
            _coffees.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public void UpdateCoffee(Coffee coffee)
        {
            var existing = GetCoffeeByName(coffee.Name);
            if (existing != null)
            {
                existing.Quantity = coffee.Quantity;
            }
        }

        public List<Coin> GetAllCoins() => _coins;

        public Coin? GetCoinByDenomination(int denomination) =>
            _coins.FirstOrDefault(c => c.Denomination == denomination);

        public void UpdateCoin(Coin updatedCoin)
        {
            var existing = _coins.FirstOrDefault(c => c.Denomination == updatedCoin.Denomination);
            if (existing != null)
            {
                existing.Quantity = updatedCoin.Quantity; 
            }
        }
    }
}