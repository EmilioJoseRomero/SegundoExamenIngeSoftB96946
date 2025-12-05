using System;

namespace ExamTwo.Data.Repositories
{
    public interface IDatabase
    {
        List<Coffee> GetAllCoffees();
        Coffee? GetCoffeeByName(string name);
        void UpdateCoffee(Coffee coffee);
        List<Coin> GetAllCoins();
        Coin? GetCoinByDenomination(int denomination);
        void UpdateCoin(Coin coin);
    }
}
