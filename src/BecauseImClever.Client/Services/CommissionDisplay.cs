namespace BecauseImClever.Client.Services;

using System.Globalization;
using System.Net;
using BecauseImClever.Application.Commissions;

/// <summary>Shared presentation conventions for commission pages.</summary>
public static class CommissionDisplay
{
    public static string Money(decimal price) => price.ToString("C2", CultureInfo.GetCultureInfo("en-US"));

    public static string ImageUrl(Guid productId, Guid imageId) => $"api/v1/commissions/{productId}/images/{imageId}";

    public static string ContactUrl(string title) =>
        $"mailto:{CommissionDefaults.ContactEmail}?subject={Uri.EscapeDataString($"Commission inquiry: {title}")}";

    public static string Error(HttpRequestException exception) => exception.StatusCode switch
    {
        HttpStatusCode.BadRequest => "Please check the required fields, prices, and image format or size.",
        HttpStatusCode.Conflict => "This commission changed or its slug is already in use. Copy any unsaved edits before reloading.",
        HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "You do not have permission for this action. Please check that you are signed in as an administrator.",
        HttpStatusCode.NotFound => "This commission or image could not be found.",
        HttpStatusCode.RequestEntityTooLarge => "The image is too large. Choose an image no larger than 5 MiB.",
        HttpStatusCode.ServiceUnavailable => "Commissions are currently unavailable. Please try again later.",
        _ => "The request could not be completed. Please check your connection and try again.",
    };
}
