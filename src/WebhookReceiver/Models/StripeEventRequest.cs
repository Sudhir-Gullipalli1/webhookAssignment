using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebhookReceiver.Models;

// Shape of an incoming Stripe event. Only the fields we store are mapped;
// the full raw body is still saved as the payload.
public class StripeEventRequest
{
    [Required]
    [StringLength(255)]
    [RegularExpression(@"^evt_[A-Za-z0-9_]+$", ErrorMessage = "id must be a Stripe event id (evt_...)")]
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [Required]
    [RegularExpression("^event$", ErrorMessage = "object must be \"event\"")]
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [Required]
    [StringLength(255, MinimumLength = 1)]
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("livemode")]
    public bool? Livemode { get; set; }

    // Unix seconds; upper bound is the last second of year 9999.
    [Range(typeof(long), "1", "253402300799", ErrorMessage = "created must be a valid Unix timestamp")]
    [JsonPropertyName("created")]
    public long? Created { get; set; }
}
