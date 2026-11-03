using System.Net.Sockets;
using ChatApp.Core.Protocol;

namespace ChatApp.Core.Client;

/// <summary>Sunucu reddederse veya bağlantı kurulamazsa fırlatılır; mesajı kullanıcıya gösterilebilir.</summary>
public sealed class ChatConnectionException(string message, Exception? innerException = null)
    : Exception(message, innerException);

/// <summary>
/// Sunucuya bağlanan istemci. Arka planda paketleri dinler ve olaylarla bildirir.
/// Olaylar arka plan iş parçacığından gelebilir; arayüz tarafı kendi iş parçacığına geçmelidir.
/// </summary>
public sealed class ChatClient
{
    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(5);

    private PacketConnection? _connection;
    private CancellationTokenSource? _stopSource;
    private Task? _receiveLoop;

    public bool IsConnected => _connection is not null;
    public string? UserName { get; private set; }

    public event EventHandler<ChatPacket>? PacketReceived;

    /// <summary>Bağlantı bittiğinde tetiklenir. Argüman true ise kullanıcı kendisi ayrılmıştır.</summary>
    public event EventHandler<bool>? Disconnected;

    public async Task ConnectAsync(string host, int port, string userName)
    {
        if (IsConnected)
        {
            throw new InvalidOperationException("İstemci zaten bağlı.");
        }

        var tcpClient = new TcpClient();
        PacketConnection? connection = null;
        try
        {
            using var timeout = new CancellationTokenSource(ConnectTimeout);
            await tcpClient.ConnectAsync(host, port, timeout.Token).ConfigureAwait(false);

            connection = new PacketConnection(tcpClient);
            await connection.WriteAsync(
                new ChatPacket { Type = PacketType.Join, Sender = userName },
                timeout.Token).ConfigureAwait(false);

            ChatPacket? reply = await connection.ReadAsync(timeout.Token).ConfigureAwait(false);
            if (reply?.Type == PacketType.JoinRejected)
            {
                throw new ChatConnectionException(reply.Text ?? "Sunucu bağlantıyı reddetti.");
            }
            if (reply?.Type != PacketType.JoinAccepted)
            {
                throw new ChatConnectionException("Sunucudan geçerli bir yanıt alınamadı.");
            }

            UserName = reply.Sender;
            _connection = connection;
            _stopSource = new CancellationTokenSource();
            _receiveLoop = ReceiveLoopAsync(connection, _stopSource.Token);
        }
        catch (Exception ex) when (ex is not ChatConnectionException)
        {
            DisposeFailedConnection(connection, tcpClient);

            throw ex switch
            {
                OperationCanceledException => new ChatConnectionException(
                    $"{host}:{port} adresine {ConnectTimeout.TotalSeconds:0} saniyede bağlanılamadı.", ex),
                SocketException or IOException => new ChatConnectionException(
                    $"{host}:{port} adresine bağlanılamadı. Sunucunun açık olduğunu kontrol edin.", ex),
                _ => ex,
            };
        }
        catch (ChatConnectionException)
        {
            DisposeFailedConnection(connection, tcpClient);
            throw;
        }
    }

    /// <summary>Herkese mesaj veya target verilmişse özel mesaj gönderir.</summary>
    public async Task SendAsync(string text, string? target = null)
    {
        PacketConnection connection = _connection
            ?? throw new ChatConnectionException("Sunucuya bağlı değilsiniz.");

        if (text.Length > ChatPacket.MaxTextLength)
        {
            throw new ChatConnectionException($"Mesaj en fazla {ChatPacket.MaxTextLength} karakter olabilir.");
        }

        var packet = new ChatPacket
        {
            Type = target is null ? PacketType.Message : PacketType.PrivateMessage,
            Target = target,
            Text = text,
        };

        try
        {
            await connection.WriteAsync(packet).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException)
        {
            throw new ChatConnectionException("Mesaj gönderilemedi: bağlantı kopmuş olabilir.", ex);
        }
    }

    public async Task DisconnectAsync()
    {
        PacketConnection? connection = _connection;
        if (connection is null)
        {
            return;
        }

        _stopSource?.Cancel();
        connection.Dispose();

        if (_receiveLoop is not null)
        {
            await _receiveLoop.ConfigureAwait(false);
        }
    }

    private async Task ReceiveLoopAsync(PacketConnection connection, CancellationToken cancellationToken)
    {
        bool userRequested = false;
        try
        {
            while (true)
            {
                ChatPacket? packet = await connection.ReadAsync(cancellationToken).ConfigureAwait(false);
                if (packet is null)
                {
                    break; // Sunucu bağlantıyı kapattı.
                }

                PacketReceived?.Invoke(this, packet);
            }
        }
        catch (OperationCanceledException)
        {
            userRequested = true;
        }
        catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException)
        {
            userRequested = cancellationToken.IsCancellationRequested;
        }

        _connection = null;
        UserName = null;
        connection.Dispose();
        Disconnected?.Invoke(this, userRequested);
    }

    private static void DisposeFailedConnection(PacketConnection? connection, TcpClient tcpClient)
    {
        if (connection is not null)
        {
            connection.Dispose();
        }
        else
        {
            tcpClient.Dispose();
        }
    }
}
