using ExamTwo.Controllers;
using ExamTwo.Data.Models;
using ExamTwo.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ExamTwo.Tests.Controllers
{
    public class CoffeeMachineControllerTests
    {
        private readonly Mock<ICoffeeMachineService> _mockService;
        private readonly CoffeeMachineController _controller;
        private readonly List<Coffee> _mockCoffees;
        private readonly List<Coin> _mockCoins;

        public CoffeeMachineControllerTests()
        {
            _mockService = new Mock<ICoffeeMachineService>();
            _controller = new CoffeeMachineController(_mockService.Object);

            _mockCoffees = new List<Coffee>
            {
                new Coffee { Name = "Americano", Price = 950, Quantity = 10 },
                new Coffee { Name = "Cappuccino", Price = 1200, Quantity = 8 }
            };

            _mockCoins = new List<Coin>
            {
                new Coin { Denomination = 500, Quantity = 20 },
                new Coin { Denomination = 100, Quantity = 30 }
            };
        }

        [Fact]
        public void GetCoffees_ReturnsOkWithCoffees()
        {
            // Arrange
            _mockService.Setup(s => s.GetAvailableCoffees()).Returns(_mockCoffees);

            // Act
            var result = _controller.GetCoffees();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(_mockCoffees);
        }

        [Fact]
        public void GetCoins_ReturnsOkWithCoins()
        {
            // Arrange
            _mockService.Setup(s => s.GetAvailableCoins()).Returns(_mockCoins);

            // Act
            var result = _controller.GetCoins();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(_mockCoins);
        }

        [Fact]
        public void CalculateTotal_ValidOrder_ReturnsTotal()
        {
            // Arrange
            var order = new Dictionary<string, int> { { "Americano", 2 } };
            _mockService.Setup(s => s.CalculateOrderTotal(order)).Returns(1900);

            // Act
            var result = _controller.CalculateTotal(order);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Value.Should().Be(1900);
        }

        [Fact]
        public void CalculateTotal_EmptyOrder_ReturnsBadRequest()
        {
            // Arrange
            Dictionary<string, int> order = null;

            // Act
            var result = _controller.CalculateTotal(order);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result.Result as BadRequestObjectResult;
            badRequest.Value.Should().Be("La orden no puede estar vacía");
        }

        [Fact]
        public void BuyCoffee_SuccessfulOrder_ReturnsOk()
        {
            // Arrange
            var request = new OrderRequest
            {
                Order = new Dictionary<string, int> { { "Americano", 1 } },
                Payment = new Payment { TotalAmount = 1000, Coins = new List<int> { 500, 500 } }
            };

            var orderResult = new OrderResult
            {
                Success = true,
                Message = "Su vuelto es de: 50 colones. Desglose: 1 moneda de 50",
                ChangeAmount = 50,
                ChangeBreakdown = new Dictionary<int, int> { { 50, 1 } },
                UpdatedCoffees = _mockCoffees
            };

            _mockService.Setup(s => s.ProcessOrder(request)).Returns(orderResult);

            // Act
            var result = _controller.BuyCoffee(request);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(new
            {
                success = true,
                message = orderResult.Message,
                change = orderResult.ChangeBreakdown,
                updatedCoffees = orderResult.UpdatedCoffees,
                totalChange = orderResult.ChangeAmount
            });
        }

        [Fact]
        public void BuyCoffee_FailedOrder_ReturnsBadRequest()
        {
            // Arrange
            var request = new OrderRequest
            {
                Order = new Dictionary<string, int> { { "Americano", 1 } },
                Payment = new Payment { TotalAmount = 500, Coins = new List<int> { 500 } }
            };

            var orderResult = new OrderResult
            {
                Success = false,
                Message = "Dinero insuficiente para realizar la compra."
            };

            _mockService.Setup(s => s.ProcessOrder(request)).Returns(orderResult);

            // Act
            var result = _controller.BuyCoffee(request);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = result.Result as BadRequestObjectResult;

            var json = System.Text.Json.JsonSerializer.Serialize(badRequest.Value);
            var response = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            response.Should().ContainKey("error");
            response["error"].Should().Be(orderResult.Message);
        }
    }
}