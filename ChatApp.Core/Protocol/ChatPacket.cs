using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatApp.Core.Protocol;

public enum PacketType
{
    /// <summary>İstemci → sunucu: kullanıcı adıyla katılma isteği.</summary>
    Join,

    /// <summary>Sunucu → istemci: katılma kabul edildi.</summary>
    JoinAccepted,

    /// <summary>Sunucu → istemci: katılma reddedildi; gerekçe Text alanındadır.</summary>
    JoinRejected,

    /// <summary>Herkese giden sohbet mesajı.</summary>
    Message,

    /// <summary>Yalnızca Target kullanıcısına giden özel mesaj.</summary>
    PrivateMessage,

    /// <summary>Sunucu → istemci: bilgilendirme (biri katıldı, ayrıldı, hata vb.).</summary>
    System,

    /// <summary>Sunucu → istemci: bağlı kullanıcı adları; Users alanındadır.</summary>
    UserList,
}

/// <summary>
/// Ağ üzerinden gönderilen tek bir mesaj. Her paket tek satırlık JSON'dur;
/// TCP bir bayt akışı olduğu için satır sonu, paketlerin sınırını belirler.
/// </summary>
public sealed record ChatPacket
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public const int MaxTextLength = 2000;

    public required PacketType Type { get; init; }
    public string? Sender { get; init; }
    public string? Target { get; init; }
    public string? Text { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public IReadOnlyList<string>? Users { get; init; }

    public string Serialize() => JsonSerializer.Serialize(this, JsonOptions);

    /// <summary>Satırı pakete çevirir. Geçersiz veri uygulamayı düşürmez; false döner.</summary>
    public static bool TryParse(string line, out ChatPacket? packet)
    {
        try
        {
            packet = JsonSerializer.Deserialize<ChatPacket>(line, JsonOptions);
            return packet is not null;
        }
        catch (JsonException)
        {
            packet = null;
            return false;
        }
    }
}
