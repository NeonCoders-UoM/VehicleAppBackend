using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    public interface ILoyaltyPointsService
    {
        int CalculateLoyaltyPoints(decimal basePrice, decimal? packagePercentage);
        decimal CalculateDiscountedPrice(decimal basePrice, decimal? packagePercentage);
    }

    public class LoyaltyPointsService : ILoyaltyPointsService
    {
        public int CalculateLoyaltyPoints(decimal basePrice, decimal? packagePercentage)
        {
            if (!packagePercentage.HasValue || packagePercentage.Value <= 0)
            {
                // No package or 0% discount - return base loyalty points (1 point per 10 currency units)
                return (int)Math.Floor(basePrice / 10);
            }

            // Calculate discounted price
            decimal discountedPrice = CalculateDiscountedPrice(basePrice, packagePercentage.Value);
            
            // Calculate loyalty points based on discounted price
            // Formula: 1 point per 10 currency units of the discounted price
            return (int)Math.Floor(discountedPrice / 10);
        }

        public decimal CalculateDiscountedPrice(decimal basePrice, decimal? packagePercentage)
        {
            if (!packagePercentage.HasValue || packagePercentage.Value <= 0)
            {
                return basePrice;
            }

            // Calculate discount amount
            decimal discountAmount = basePrice * (packagePercentage.Value / 100);
            
            // Return discounted price
            return basePrice - discountAmount;
        }
    }
} 