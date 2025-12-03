using CMS.Commerce;

#pragma warning disable KXE0005 // Price calculation and order creation feature is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace DancingGoat.Commerce;

/// <summary>
/// Identifier for product with unqiue variants.
/// </summary>
/// <remarks>
/// Unique object is specified by both <see cref="ProductIdentifier.Identifier"/> and <see cref="VariantIdentifier"/>.
/// </remarks>
public record ProductVariantIdentifier : ProductIdentifier
{
    /// <summary>
    /// Variant identifier.
    /// </summary>
    public int? VariantIdentifier { get; init; }
}
#pragma warning restore KXE0005 // Price calculation and order creation feature is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
