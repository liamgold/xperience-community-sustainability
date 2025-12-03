using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CMS.Commerce;

#pragma warning disable KXE0005 // Price calculation and order creation feature is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace DancingGoat.Commerce;

public sealed class DancingGoatTaxPriceCalculationStep<TRequest, TResult> : ITaxPriceCalculationStep<TRequest, TResult>
    where TRequest : DancingGoatPriceCalculationRequest
    where TResult : DancingGoatPriceCalculationResult
{
    public Task Execute(IPriceCalculationData<TRequest, TResult> calculationData, CancellationToken cancellationToken)
    {
        if (calculationData.Result.Items.Any(item => item.ProductData == null))
        {
            throw new InvalidOperationException($"Some items are missing product data: {string.Join(", ", calculationData.Result.Items.Where(item => item.ProductData == null).Select(item => item.ProductIdentifier))}");
        }

        calculationData.Result.TotalTax = 0;

        foreach (var resultItem in calculationData.Result.Items)
        {
            resultItem.TaxRate = DancingGoatTaxRateConstants.TAX_RATE;
            resultItem.TaxAmount = resultItem.TaxRate * resultItem.ProductData.UnitPrice * resultItem.Quantity / 100m;
            calculationData.Result.TotalTax += resultItem.TaxAmount;
        }

        calculationData.Result.TotalTax += calculationData.Result.ShippingPrice * DancingGoatTaxRateConstants.TAX_RATE / 100m;

        return Task.CompletedTask;
    }
}
#pragma warning restore KXE0005 // Price calculation and order creation feature is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
