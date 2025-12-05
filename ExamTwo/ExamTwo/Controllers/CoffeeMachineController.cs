using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExamTwo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoffeeMachineController : ControllerBase
    {
        private readonly ICoffeeMachineService _coffeeMachineService;

        public CoffeeMachineController(ICoffeeMachineService coffeeMachineService)
        {
            _coffeeMachineService = coffeeMachineService;
        }

        [HttpGet("coffees")]
        public ActionResult<List<Coffee>> GetCoffees()
        {
            var coffees = _coffeeMachineService.GetAvailableCoffees();
            return Ok(coffees);
        }

        [HttpGet("coins")]
        public ActionResult<List<Coin>> GetCoins()
        {
            var coins = _coffeeMachineService.GetAvailableCoins();
            return Ok(coins);
        }

        [HttpPost("calculate-total")]
        public ActionResult<int> CalculateTotal([FromBody] Dictionary<string, int> order)
        {
            if (order == null || order.Count == 0)
                return BadRequest("La orden no puede estar vacía");

            var total = _coffeeMachineService.CalculateOrderTotal(order);
            return Ok(total);
        }

        [HttpPost("buy")]
        public ActionResult<OrderResult> BuyCoffee([FromBody] OrderRequest request)
        {
            var result = _coffeeMachineService.ProcessOrder(request);

            if (!result.Success)
                return BadRequest(new { error = result.Message });

            return Ok(new
            {
                success = true,
                message = result.Message,
                change = result.ChangeBreakdown,
                updatedCoffees = result.UpdatedCoffees,
                totalChange = result.ChangeAmount
            });
        }
    }
}