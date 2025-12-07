using ExamTwo.Data.Models;
using ExamTwo.Data.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;  
using Moq;                          
using Xunit;

namespace ExamTwo.Tests.Repositories
{
    public class InMemoryDatabaseTests
    {
        private readonly InMemoryDatabase _database;
        private readonly Mock<ILogger<InMemoryDatabase>> _mockLogger;

        public InMemoryDatabaseTests()
        {
            _mockLogger = new Mock<ILogger<InMemoryDatabase>>();
            _database = new InMemoryDatabase(_mockLogger.Object);
        }

        [Fact]
        public void GetAllCoffees_ReturnsInitialCoffees()
        {
            // Act
            var result = _database.GetAllCoffees();

            // Assert
            result.Should().HaveCount(4);
            result.Should().Contain(c => c.Name == "Americano" && c.Price == 950 && c.Quantity == 10);
            result.Should().Contain(c => c.Name == "Cappuccino" && c.Price == 1200 && c.Quantity == 8);
            result.Should().Contain(c => c.Name == "Lates" && c.Price == 1350 && c.Quantity == 10);
            result.Should().Contain(c => c.Name == "Mocaccino" && c.Price == 1500 && c.Quantity == 15);
        }

        [Fact]
        public void GetCoffeeByName_ExistingCoffee_ReturnsCoffee()
        {
            // Act
            var result = _database.GetCoffeeByName("Americano");

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Americano");
            result.Price.Should().Be(950);
            result.Quantity.Should().Be(10);
        }

        [Fact]
        public void GetCoffeeByName_NonExistentCoffee_ReturnsNull()
        {
            // Act
            var result = _database.GetCoffeeByName("NonExistent");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetCoffeeByName_CaseInsensitive_ReturnsCoffee()
        {
            // Act
            var result = _database.GetCoffeeByName("aMeRiCaNo");

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Americano");
        }

        [Fact]
        public void UpdateCoffee_ExistingCoffee_UpdatesQuantity()
        {
            // Arrange
            var updatedCoffee = new Coffee { Name = "Americano", Price = 950, Quantity = 5 };

            // Act
            _database.UpdateCoffee(updatedCoffee);
            var result = _database.GetCoffeeByName("Americano");

            // Assert
            result.Quantity.Should().Be(5);
        }

        [Fact]
        public void UpdateCoffee_NonExistentCoffee_DoesNothing()
        {
            // Arrange
            var originalCoffees = _database.GetAllCoffees().ToList();
            var nonExistentCoffee = new Coffee { Name = "NonExistent", Price = 100, Quantity = 5 };

            // Act
            _database.UpdateCoffee(nonExistentCoffee);
            var result = _database.GetAllCoffees();

            // Assert
            result.Should().BeEquivalentTo(originalCoffees);
        }

        [Fact]
        public void GetAllCoins_ReturnsInitialCoins()
        {
            // Act
            var result = _database.GetAllCoins();

            // Assert
            result.Should().HaveCount(5);
            result.Should().Contain(c => c.Denomination == 1000 && c.Quantity == 0);
            result.Should().Contain(c => c.Denomination == 500 && c.Quantity == 20);
            result.Should().Contain(c => c.Denomination == 100 && c.Quantity == 30);
            result.Should().Contain(c => c.Denomination == 50 && c.Quantity == 50);
            result.Should().Contain(c => c.Denomination == 25 && c.Quantity == 25);
        }

        [Fact]
        public void GetCoinByDenomination_ExistingCoin_ReturnsCoin()
        {
            // Act
            var result = _database.GetCoinByDenomination(500);

            // Assert
            result.Should().NotBeNull();
            result.Denomination.Should().Be(500);
            result.Quantity.Should().Be(20);
        }

        [Fact]
        public void GetCoinByDenomination_NonExistentCoin_ReturnsNull()
        {
            // Act
            var result = _database.GetCoinByDenomination(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void UpdateCoin_ExistingCoin_UpdatesQuantity()
        {
            // Arrange
            var updatedCoin = new Coin { Denomination = 500, Quantity = 15 };

            // Act
            _database.UpdateCoin(updatedCoin);
            var result = _database.GetCoinByDenomination(500);

            // Assert
            result.Quantity.Should().Be(15);
        }

        [Fact]
        public void UpdateCoin_NonExistentCoin_DoesNothing()
        {
            // Arrange
            var originalCoins = _database.GetAllCoins().ToList();
            var nonExistentCoin = new Coin { Denomination = 999, Quantity = 10 };

            // Act
            _database.UpdateCoin(nonExistentCoin);
            var result = _database.GetAllCoins();

            // Assert
            result.Should().BeEquivalentTo(originalCoins);
        }
    }
}