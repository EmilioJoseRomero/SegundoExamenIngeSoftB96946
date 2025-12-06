using ExamTwo.Data.Models;

namespace ExamTwo.Helpers
{
    public static class MachineStatusHelper
    {
        public static bool IsMachineOperational(List<Coin> coins)
        {
            return coins.Any(c => c.Quantity > 0);
        }

        public static string FormatChangeMessage(int changeAmount, Dictionary<int, int> changeBreakdown)
        {
            if (changeAmount == 0)
                return "No hay cambio necesario";

            var message = $"Su vuelto es de: {changeAmount} colones. Desglose:";

            foreach (var (denomination, count) in changeBreakdown.OrderByDescending(x => x.Key))
            {
                message += $" {count} moneda{(count > 1 ? "s" : "")} de {denomination},";
            }

            return message.TrimEnd(',');
        }
    }
}