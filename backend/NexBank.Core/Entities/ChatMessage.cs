using System.ComponentModel.DataAnnotations;

namespace NexBank.Core.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    
    // Mesajı gönderen
    public int SenderId { get; set; }
    public User? Sender { get; set; }

    // Alıcı (Müşteri personel ile, personel müşteri ile konuşur)
    // Nullable: Müşteri mesajlarında alıcı şube grubudur, belirli bir kişi değil
    public int? ReceiverId { get; set; }
    public User? Receiver { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    
    // Mesajın ait olduğu şubeyi takip edelim
    public string Branch { get; set; } = string.Empty;
}
