using System.ComponentModel.DataAnnotations;

namespace Optivem.Shop.Monolith.Core.Dtos;

public class PublishCouponRequest : IValidatableObject
{
    [Required(ErrorMessage = "Coupon code must not be blank")]
    public string? Code { get; set; }

    [Required(ErrorMessage = "Discount rate must not be null")]
    public decimal? DiscountRate { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Usage limit must be positive")]
    public int? UsageLimit { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DiscountRate.HasValue)
        {
            if (DiscountRate.Value <= 0m)
            {
                yield return new ValidationResult("Discount rate must be greater than 0.00", new[] { nameof(DiscountRate) });
            }
            else if (DiscountRate.Value > 1.0m)
            {
                yield return new ValidationResult("Discount rate must be at most 1.00", new[] { nameof(DiscountRate) });
            }
        }
    }
}
