using System;
using ExamTwo.Data.Models;
using ExamTwo.Data.Repositories;

namespace ExamTwo.Services
{
    public class CoffeeMachineService : ICoffeeMachineService
    {
        private readonly IDatabase _database;
        private readonly ILogger<CoffeeMachineService> _logger;

        public CoffeeMachineService(IDatabase database, ILogger<CoffeeMachineService> logger)
        {
            _database = database;
            _logger = logger;
        }

        public List<Coffee> GetAvailableCoffees()
        {
            return _database.GetAllCoffees()
                .Where(c => c.Quantity > 0)
                .ToList();
        }

        public List<Coin> GetAvailableCoins()
        {
            return _database.GetAllCoins();
        }

        public int CalculateOrderTotal(Dictionary<string, int> order)
        {
            int total = 0;
            foreach (var item in order)
            {
                var coffee = _database.GetCoffeeByName(item.Key);
                if (coffee != null)
                {
                    total += coffee.Price * item.Value;
                }
            }
            return total;
        }

        public OrderResult ProcessOrder(OrderRequest request)
        {
            try
            {
                if (request.Order == null || request.Order.Count == 0)
                {
                    return new OrderResult
                    {
                        Success = false,
                        Message = "La orden no puede estar vacía."
                    };
                }

                int totalCost = CalculateOrderTotal(request.Order);

                if (request.Payment.TotalAmount < totalCost)
                {
                    return new OrderResult
                    {
                        Success = false,
                        Message = "Dinero insuficiente para realizar la compra."
                    };
                }

                foreach (var item in request.Order)
                {
                    var coffee = _database.GetCoffeeByName(item.Key);
                    if (coffee == null)
                    {
                        return new OrderResult
                        {
                            Success = false,
                            Message = $"El café {item.Key} no está disponible."
                        };
                    }

                    if (coffee.Quantity < item.Value)
                    {
                        return new OrderResult
                        {
                            Success = false,
                            Message = $"No hay suficientes {item.Key} en la máquina."
                        };
                    }
                }

                int changeAmount = request.Payment.TotalAmount - totalCost;
                var changeBreakdown = CalculateChange(changeAmount);

                if (changeBreakdown == null)
                {
                    return new OrderResult
                    {
                        Success = false,
                        Message = "No hay suficiente cambio en la máquina. Fallo al realizar la compra."
                    };
                }

                foreach (var item in request.Order)
                {
                    var coffee = _database.GetCoffeeByName(item.Key);
                    if (coffee != null)
                    {
                        coffee.Quantity -= item.Value;
                        _database.UpdateCoffee(coffee);
                    }
                }

                foreach (var coinValue in request.Payment.Coins)
                {
                    var coin = _database.GetCoinByDenomination(coinValue);
                    if (coin != null)
                    {
                        coin.Quantity += 1; 
                        _database.UpdateCoin(coin);
                    }
                }

                foreach (var breakdown in changeBreakdown)
                {
                    var coin = _database.GetCoinByDenomination(breakdown.Key);
                    if (coin != null)
                    {
                        coin.Quantity -= breakdown.Value;
                        _database.UpdateCoin(coin);
                    }
                }

                var updatedCoffees = _database.GetAllCoffees();

                string message;
                if (changeAmount == 0)
                {
                    message = "Compra exitosa. No hay vuelto.";
                }
                else
                {
                    message = $"Su vuelto es de: {changeAmount} colones. Desglose:";
                    foreach (var breakdown in changeBreakdown.OrderByDescending(x => x.Key))
                    {
                        message += $" {breakdown.Value} moneda{(breakdown.Value > 1 ? "s" : "")} de {breakdown.Key},";
                    }
                    message = message.TrimEnd(',');
                }

                return new OrderResult
                {
                    Success = true,
                    Message = message,
                    ChangeAmount = changeAmount,
                    ChangeBreakdown = changeBreakdown,
                    UpdatedCoffees = updatedCoffees
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando la orden");
                return new OrderResult
                {
                    Success = false,
                    Message = "Error interno del sistema."
                };
            }
        }

        private Dictionary<int, int>? CalculateChange(int changeAmount)
        {
            if (changeAmount == 0) return new Dictionary<int, int>();

            int remainingChange = changeAmount;
            var availableCoins = _database.GetAllCoins()
                .Where(c => c.Denomination != 1000) 
                .OrderByDescending(c => c.Denomination)
                .ToList();

            var changeBreakdown = new Dictionary<int, int>();

            foreach (var coin in availableCoins)
            {
                if (remainingChange <= 0) break;

                int coinsNeeded = remainingChange / coin.Denomination;
                int coinsToUse = Math.Min(coinsNeeded, coin.Quantity);

                if (coinsToUse > 0)
                {
                    changeBreakdown[coin.Denomination] = coinsToUse;
                    remainingChange -= coin.Denomination * coinsToUse;
                }
            }

            return remainingChange == 0 ? changeBreakdown : null;
        }
    }
}
