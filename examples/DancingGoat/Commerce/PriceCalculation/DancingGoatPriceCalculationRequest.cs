using CMS.Commerce;

#pragma warning disable KXE0005 // Price calculation and order creation feature is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace DancingGoat.Commerce;

public record DancingGoatPriceCalculationRequest : PriceCalculationRequestBase<DancingGoatPriceCalculationRequestItem, AddressDto>;
#pragma warning restore KXE0005 // Price calculation and order creation feature is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
