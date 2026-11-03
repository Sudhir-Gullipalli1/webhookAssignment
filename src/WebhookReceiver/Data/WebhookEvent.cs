using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebhookReceiver.Data;

[Table("webhook_events")]
public class WebhookEvent
{
    [Key]
    public string EventId { get; set; } = "";
    public string Type { get; set; } = "";
    public bool Livemode { get; set; }
    public DateTime? CreatedAtUtc { get; set; }
    public DateTime ReceivedAtUtc { get; set; }
    public string Payload { get; set; } = "";
}
