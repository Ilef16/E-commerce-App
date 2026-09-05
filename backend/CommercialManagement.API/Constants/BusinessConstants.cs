namespace CommercialManagement.API.Constants;

/// <summary>
/// Shared business constants used across the application.
/// These values are fixed by business rules and must not be changed without explicit business decision.
/// </summary>
public static class BusinessConstants
{
    /// <summary>TVA rate applied to all orders (19%).</summary>
    public const decimal TvaRate = 0.19m;

    /// <summary>Stock threshold below which a product is considered "faible" (low stock).</summary>
    public const int StockFaibleSeuil = 5;

    /// <summary>Maximum allowed page size for paginated queries.</summary>
    public const int MaxPageSize = 100;

    /// <summary>Default page size for paginated queries.</summary>
    public const int DefaultPageSize = 10;

    public const string ProductUploadFolder = "uploads/products";

    public static readonly string[] AllowedPhotoExtensions =
        [".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp", ".avif"];
}
