using ExamTwo.Data.Models;
using ExamTwo.Data.Repositories;
using ExamTwo.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ExamTwo.Tests.Services
{
    public class CoffeeMachineServiceTests
    {
        private readonly Mock<IDatabase> _mockDatabase;
        private readonly Mock<ILogger<CoffeeMachineService>> _mockLogger;
        private readonly CoffeeMachineService _service;
        private readonly List<Coffee> _mockCoffees;
        private readonly List<Coin> _mockCoins;

        public CoffeeMachineServiceTests()
        {
            _mockDatabase = new Mock<IDatabase>();
            _mockLogger = new Mock<ILogger<CoffeeMachineService>>();
            _service = new CoffeeMachineService(_mockDatabase.Object, _mockLogger.Object);

            _mockCoffees = new List<Coffee>
            {
                new Coffee { Name = "Americano", Price = 950, Quantity = 10 },
                new Coffee { Name = "Cappuccino", Price = 1200, Quantity = 8 },
                new Coffee { Name = "Lates", Price = 1350, Quantity = 10 },
                new Coffee { Name = "Mocaccino", Price = 1500, Quantity = 15 }
            };

            _mockCoins = new List<Coin>
            {
                new Coin { Denomination = 1000, Quantity = 0 },
                new Coin { Denomination = 500, Quantity = 20 },
                new Coin { Denomination = 100, Quantity = 30 },
                new Coin { Denomination = 50, Quantity = 50 },
                new Coin { Denomination = 25, Quantity = 25 }
            };

            _mockDatabase.Setup(db => db.GetAllCoffees()).Returns(_mockCoffees);
            _mockDatabase.Setup(db => db.GetCoffeeByName(It.IsAny<string>()))
                .Returns((string name) => _mockCoffees.FirstOrDefault(c => c.Name == name));

            _mockDatabase.Setup(db => db.GetAllCoins()).Returns(_mockCoins);
            _mockDatabase.Setup(db => db.GetCoinByDenomination(It.IsAny<int>()))
                .Returns((int denom) => _mockCoins.FirstOrDefault(c => c.Denomination == denom));
        }

        // CÁLCULOS BÁSICOS 
        [Fact]
        public void CalculateOrderTotal_ReturnsCorrectTotal()
        {
            // Arrange
            var order = new Dictionary<string, int>
            {
                { "Americano", 2 },  // 1900
                { "Cappuccino", 1 }  // 1200
            };

            // Act
            var result = _service.CalculateOrderTotal(order);

            // Assert
            result.Should().Be(3100);
        }

        [Fact]
        public void CalculateOrderTotal_EmptyOrder_ReturnsZero()
        {
            // Arrange
            var order = new Dictionary<string, int>();

            // Act
            var result = _service.CalculateOrderTotal(order);

            // Assert
            result.Should().Be(0);
        }

        // COMPRAS EXITOSAS 
        [Fact]
        public void ProcessOrder_ValidOrder_ReturnsSuccess()
        {
            // Arrange
            var request = new OrderRequest
            {
                Order = new Dictionary<string, int> { { "Americano", 1 } },
                Payment = new Payment
                {
                    TotalAmount = 1000,
                    Coins = new List<int> { 500, 500 },
                    Bills = new List<int>()
                }
            };

            // Act
            var result = _service.ProcessOrder(request);

            // Assert
            result.Success.Should().BeTrue();
            result.ChangeAmount.Should().Be(50);
            result.Message.Should().Contain("Su vuelto es de: 50 colones");
        }

        [Fact]
        public void ProcessOrder_ExactPayment_ReturnsNoChange()
        {
            // Arrange 
            var request = new OrderRequest
            {
                Order = new Dictionary<string, int> { { "Americano", 1 } },
                Payment = new Payment
                {
                    TotalAmount = 950,
                    Coins = new List<int> { 500, 100, 100, 100, 100, 50 },
                    Bills = new List<int>()
                }
            };

            // Act
            var result = _service.ProcessOrder(request);

            // Assert
            result.Success.Should().BeTrue();
            result.ChangeAmount.Should().Be(0);
            result.Message.Should().Be("Compra exitosa. No hay vuelto.");
        }

        [Fact]
        public void ProcessOrder_MultipleCoffees_ReturnsCorrectChange()
        {
            // Arrange
            var request = new OrderRequest
            {
                Order = new Dictionary<string, int>
                {
                    { "Americano", 2 },    // 1900
                    { "Cappuccino", 1 }    // 1200
                },
                Payment = new Payment
                {
                    TotalAmount = 5000,    // Total: 3100, Cambio: 1900
                    Coins = Enumerable.Repeat(500, 10).ToList(),
                    Bills = new List<int>()
                }
            };

            // Act
            var result = _service.ProcessOrder(request);

            // Assert
            result.Success.Should().BeTrue();
            result.ChangeAmount.Should().Be(1900);
            result.ChangeBreakdown.Should().ContainKey(500).WhoseValue.Should().Be(3);
            result.ChangeBreakdown.Should().ContainKey(100).WhoseValue.Should().Be(4);
        }

        [Fact]
        public void ProcessOrder_ChangeWith25Coins_ReturnsCorrectBreakdown()
        {
            // Arrange
            var request = new OrderRequest
            {
                Order = new Dictionary<string, int> { { "Americano", 1 } },
                Payment = new Payment
                {
                    TotalAmount = 1025, // Cambio: 75
                    Coins = new List<int> { 500, 500, 25 },
                    Bills = new List<int>()
                }
            };

            // Act
            var result = _service.ProcessOrder(request);

            // Assert
            result.Success.Should().BeTrue();
            result.ChangeAmount.Should().Be(75);
            result.ChangeBreakdown.Should().ContainKey(50).WhoseValue.Should().Be(1);
            result.ChangeBreakdown.Should().ContainKey(25).WhoseValue.Should().Be(1);
        }

        // VALIDACIONES DE ERROR
        [Fact]
        public void ProcessOrder_EmptyOrder_ReturnsError()
        {
            // Arrange
            var request = new OrderRequest
            {
                Order = new Dictionary<string, int>(),
                Payment = new Payment { TotalAmount = 1000 }
            };

            // Act
            var result = _service.ProcessOrder(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("La orden no puede estar vacía.");
        }

        [Fact]
        public void ProcessOrder_NullOrder_ReturnsError()
        {
            // Arrange
            var request = new OrderRequest
            {
                Order = null,
                Payment = new Payment { TotalAmount = 1000 }
            };

            // Act
            var result = _service.ProcessOrder(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("La orden no puede estar vacía.");
        }

        [Fact]
        public void ProcessOrder_InsufficientPayment_ReturnsError()
        {
            // Arrange
            var request = new OrderRequest
            {
                Order = new Dictionary<string, int> { { "Americano", 1 } },
                Payment = new Payment
                {
                    TotalAmount = 500, // Insuficiente
                    Coins = new List<int> { 500 },
                    Bills = new List<int>()
                }
            };

            // Act
            var result = _service.ProcessOrder(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Dinero insuficiente para realizar la compra.");
        }

        [Fact]
        public void ProcessOrder_CoffeeNotAvailable_ReturnsError()
        {
            // Arrange
            var request = new OrderRequest
            {
                Order = new Dictionary<string, int> { { "NonExistent", 1 } },
                Payment = new Payment
                {
                    TotalAmount = 1000,
                    Coins = new List<int> { 500, 500 },
                    Bills = new List<int>()
                }
            };

            // Act
            var result = _service.ProcessOrder(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("El café NonExistent no está disponible.");
        }

        [Fact]
        public void ProcessOrder_InsufficientCoffeeQuantity_ReturnsError()
        {
            // Arrange
            var request = new OrderRequest
            {
                Order = new Dictionary<string, int> { { "Americano", 15 } },
                Payment = new Payment
                {
                    TotalAmount = 20000,
                    Coins = Enumerable.Repeat(500, 40).ToList(),
                    Bills = new List<int>()
                }
            };

            // Act
            var result = _service.ProcessOrder(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No hay suficientes Americano en la máquina.");
        }

        [Fact]
        public void ProcessOrder_InvalidCoin_ReturnsError()
        {
            // Arrange
            var request = new OrderRequest
            {
                Order = new Dictionary<string, int> { { "Americano", 1 } },
                Payment = new Payment
                {
                    TotalAmount = 1000,
                    Coins = new List<int> { 500, 300, 200 }, // 300 y 200 inválidos
                    Bills = new List<int>()
                }
            };

            // Act
            var result = _service.ProcessOrder(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Moneda no válida");
        }

        [Fact]
        public void ProcessOrder_InvalidBill_ReturnsError()
        {
            // Arrange
            var request = new OrderRequest
            {
                Order = new Dictionary<string, int> { { "Americano", 1 } },
                Payment = new Payment
                {
                    TotalAmount = 5000,
                    Coins = new List<int>(),
                    Bills = new List<int> { 5000 } // Billete inválido
                }
            };

            // Act
            var result = _service.ProcessOrder(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Billete no válido");
        }

        [Fact]
        public void ProcessOrder_PaymentAmountMismatch_ReturnsError()
        {
            // Arrange
            var request = new OrderRequest
            {
                Order = new Dictionary<string, int> { { "Americano", 1 } },
                Payment = new Payment
                {
                    TotalAmount = 5000, 
                    Coins = new List<int> { 500 }, // Pero solo dio 500
                    Bills = new List<int>()
                }
            };

            // Act
            var result = _service.ProcessOrder(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("El monto total no coincide con las monedas/billetes ingresados.");
        }

        // CASOS LÍMITE 
        [Fact]
        public void ProcessOrder_MaxQuantityPurchase_Succeeds()
        {
            // Arrange
            var request = new OrderRequest
            {
                Order = new Dictionary<string, int> { { "Americano", 10 } },
                Payment = new Payment
                {
                    TotalAmount = 10000,
                    Coins = Enumerable.Repeat(500, 20).ToList(),
                    Bills = new List<int>()
                }
            };

            // Act
            var result = _service.ProcessOrder(request);

            // Assert
            result.Success.Should().BeTrue();
            _mockCoffees.First(c => c.Name == "Americano").Quantity.Should().Be(0);
        }

        [Fact]
        public void ProcessOrder_AfterCoffeeSoldOut_CannotBuyMore()
        {
            // Arrange - Primera compra agota el café
            var firstRequest = new OrderRequest
            {
                Order = new Dictionary<string, int> { { "Cappuccino", 8 } },
                Payment = new Payment
                {
                    TotalAmount = 10000,
                    Coins = Enumerable.Repeat(500, 20).ToList(),
                    Bills = new List<int>()
                }
            };

            _service.ProcessOrder(firstRequest);

            // Segunda compra del mismo café
            var secondRequest = new OrderRequest
            {
                Order = new Dictionary<string, int> { { "Cappuccino", 1 } },
                Payment = new Payment
                {
                    TotalAmount = 2000,
                    Coins = new List<int> { 500, 500, 500, 500 },
                    Bills = new List<int>()
                }
            };

            // Act
            var result = _service.ProcessOrder(secondRequest);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No hay suficientes Cappuccino en la máquina.");
        }

        // CÁLCULO DE CAMBIO
        [Fact]
        public void CalculateChange_SimpleAmounts_ReturnsCorrectBreakdown()
        {
            var method = typeof(CoffeeMachineService).GetMethod("CalculateChange",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Caso 1: 25 colones = 1 moneda de 25
            var result25 = method?.Invoke(_service, new object[] { 25 }) as Dictionary<int, int>;
            result25.Should().BeEquivalentTo(new Dictionary<int, int> { { 25, 1 } });

            // Caso 2: 75 colones = 50 + 25
            var result75 = method?.Invoke(_service, new object[] { 75 }) as Dictionary<int, int>;
            result75.Should().BeEquivalentTo(new Dictionary<int, int> { { 50, 1 }, { 25, 1 } });

            // Caso 3: 500 colones = 1 moneda de 500
            var result500 = method?.Invoke(_service, new object[] { 500 }) as Dictionary<int, int>;
            result500.Should().BeEquivalentTo(new Dictionary<int, int> { { 500, 1 } });

            // Caso 4: 1900 colones = 3×500 + 4×100
            var result1900 = method?.Invoke(_service, new object[] { 1900 }) as Dictionary<int, int>;
            result1900.Should().BeEquivalentTo(new Dictionary<int, int> { { 500, 3 }, { 100, 4 } });
        }

        [Fact]
        public void CalculateChange_InvalidAmount_ReturnsNull()
        {
            var method = typeof(CoffeeMachineService).GetMethod("CalculateChange",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = method?.Invoke(_service, new object[] { 30 }) as Dictionary<int, int>;

            result.Should().BeNull();
        }
    }
}