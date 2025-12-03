using CMS.Commerce;

#pragma warning disable KXE0005 // Price calculation and order creation feature is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace DancingGoat.Commerce;

/// <summary>
/// Custom calculation result item with identifier specifying unique product based on identifier and variant identifier.
/// </summary>
public sealed class DancingGoatPriceCalculationResultItem : PriceCalculationResultItemBase<ProductVariantIdentifier, ProductData>
{
    /// <summary>
    /// Tax rate in %.
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Tax amount.
    /// </summary>
    public decimal TaxAmount { get; set; }
}
#pragma warning restore KXE0005 // Price calculation and order creation feature is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
