using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using ChatApp.Core.Protocol;

namespace ChatApp.Core.Server;

/// <summary>
/// Birden çok istemciyi aynı odada buluşturan TCP sunucusu.
/// Her istemci kendi asenkron görevinde dinlenir; mesajlar diğer istemcilere iletilir.
/// </summary>
public sealed class ChatServer
{
    public const int MaxUserNameLength = 20;

    private static readonly TimeSpan JoinTimeout = TimeSpan.FromSeconds(10);

    // Kullanıcı adı anahtardır; büyük/küçük harf farkı aynı kullanıcı sayılır.
    private readonly ConcurrentDictionary<string, ConnectedClient> _clients =
        new(StringComparer.OrdinalIgnoreCase);

    private TcpListener? _listener;
    private CancellationTokenSource? _stopSource;
    private Task? _acceptLoop;

    public bool IsRunning => _listener is not null;

    public event EventHandler<ClientEventArgs>? ClientConnected;
    public event EventHandler<ClientEventArgs>? ClientDisconnected;
    public event EventHandler<ServerLogEventArgs>? LogAdded;

    /// <summary>Dinlemeye başlar. Port kullanımdaysa SocketException fırlatır.</summary>
    public void Start(IPAddress address, int port)
    {
        if (IsRunning)
        {
            throw new InvalidOperationException("Sunucu zaten çalışıyor.");
        }

        var listener = new TcpListener(address, port);
        listener.Start();

        _listener = listener;
        _stopSource = new CancellationTokenSource();
        _acceptLoop = AcceptLoopAsync(listener, _stopSource.Token);

        Log($"Sunucu başlatıldı: {address}:{port}");
    }

    /// <summary>Dinlemeyi bırakır ve tüm istemcileri kapatır.</summary>
    public async Task StopAsync()
    {
        if (_listener is null || _stopSource is null)
        {
            return;
        }

        TcpListener listener = _listener;
        _listener = null;

        _stopSource.Cancel();
        listener.Stop();

        foreach (ConnectedClient client in _clients.Values)
        {
            client.Connection.Dispose();
        }

        if (_acceptLoop is not null)
        {
            await _acceptLoop.ConfigureAwait(false);
        }

        _stopSource.Dispose();
        _stopSource = null;
        Log("Sunucu durduruldu.");
    }

    private async Task AcceptLoopAsync(TcpListener listener, CancellationToken cancellationToken)
    {
        var sessions = new List<Task>();
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient tcpClient = await listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
                sessions.RemoveAll(task => task.IsCompleted);
                sessions.Add(HandleClientAsync(tcpClient, cancellationToken));
            }
        }
        catch (OperationCanceledException)
        {
            // Sunucu durduruldu: normal çıkış.
        }
        catch (SocketException ex)
        {
            Log($"Dinleme hatası: {ex.Message}");
        }

        await Task.WhenAll(sessions).ConfigureAwait(false);
    }

    private async Task HandleClientAsync(TcpClient tcpClient, CancellationToken cancellationToken)
    {
        string remoteAddress = tcpClient.Client.RemoteEndPoint?.ToString() ?? "bilinmiyor";
        using var connection = new PacketConnection(tcpClient);
        ConnectedClient? client = null;

        try
        {
            client = await AcceptJoinAsync(connection, remoteAddress, cancellationToken).ConfigureAwait(false);
            if (client is null)
            {
                return;
            }

            await ReceiveLoopAsync(client, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Sunucu durduruluyor veya katılma süresi doldu.
        }
        catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException)
        {
            // Bağlantı beklenmedik biçimde koptu; aşağıda normal ayrılma gibi ele alınır.
        }
        finally
        {
            if (client is not null)
            {
                await RemoveClientAsync(client).ConfigureAwait(false);
            }
        }
    }

    private async Task<ConnectedClient?> AcceptJoinAsync(
        PacketConnection connection,
        string remoteAddress,
        CancellationToken cancellationToken)
    {
        // Bağlanıp hiç kullanıcı adı göndermeyen istemci sonsuza kadar beklenmez.
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(JoinTimeout);

        ChatPacket? join = await connection.ReadAsync(timeout.Token).ConfigureAwait(false);
        if (join is null || join.Type != PacketType.Join)
        {
            return null;
        }

        string userName = join.Sender?.Trim() ?? string.Empty;
        string? error = ValidateUserName(userName);

        if (error is null)
        {
            var client = new ConnectedClient(connection, userName, remoteAddress, DateTime.Now);
            if (_clients.TryAdd(userName, client))
            {
                await connection.WriteAsync(
                    new ChatPacket { Type = PacketType.JoinAccepted, Sender = userName },
                    cancellationToken).ConfigureAwait(false);

                Log($"{userName} bağlandı ({remoteAddress})");
                ClientConnected?.Invoke(this, new ClientEventArgs(client));
                await BroadcastAsync(SystemPacket($"{userName} sohbete katıldı.")).ConfigureAwait(false);
                await BroadcastUserListAsync().ConfigureAwait(false);
                return client;
            }

            error = $"'{userName}' kullanıcı adı zaten kullanımda.";
        }

        Log($"Katılma reddedildi ({remoteAddress}): {error}");
        await connection.WriteAsync(
            new ChatPacket { Type = PacketType.JoinRejected, Text = error },
            cancellationToken).ConfigureAwait(false);
        return null;
    }

    private static string? ValidateUserName(string userName)
    {
        if (userName.Length == 0)
        {
            return "Kullanıcı adı boş olamaz.";
        }
        if (userName.Length > MaxUserNameLength)
        {
            return $"Kullanıcı adı en fazla {MaxUserNameLength} karakter olabilir.";
        }
        return null;
    }

    private async Task ReceiveLoopAsync(ConnectedClient client, CancellationToken cancellationToken)
    {
        while (true)
        {
            ChatPacket? packet = await client.Connection.ReadAsync(cancellationToken).ConfigureAwait(false);
            if (packet is null)
            {
                return; // İstemci bağlantıyı kapattı.
            }

            if (string.IsNullOrWhiteSpace(packet.Text) || packet.Text.Length > ChatPacket.MaxTextLength)
            {
                continue;
            }

            switch (packet.Type)
            {
                case PacketType.Message:
                    await RelayPublicAsync(client, packet.Text).ConfigureAwait(false);
                    break;
                case PacketType.PrivateMessage:
                    await RelayPrivateAsync(client, packet.Target, packet.Text).ConfigureAwait(false);
                    break;
            }
        }
    }

    // Gönderen adı ve zaman damgası sunucuda atanır; istemci başkasının adıyla mesaj atamaz.
    private async Task RelayPublicAsync(ConnectedClient sender, string text)
    {
        var message = new ChatPacket
        {
            Type = PacketType.Message,
            Sender = sender.UserName,
            Text = text,
            Timestamp = DateTime.Now,
        };

        Log($"{sender.UserName}: {text}");
        await BroadcastAsync(message).ConfigureAwait(false);
    }

    private async Task RelayPrivateAsync(ConnectedClient sender, string? target, string text)
    {
        if (target is null || !_clients.TryGetValue(target, out ConnectedClient? recipient))
        {
            await SafeWriteAsync(sender, SystemPacket($"'{target}' kullanıcısı bağlı değil.")).ConfigureAwait(false);
            return;
        }

        var message = new ChatPacket
        {
            Type = PacketType.PrivateMessage,
            Sender = sender.UserName,
            Target = recipient.UserName,
            Text = text,
            Timestamp = DateTime.Now,
        };

        // Özel mesajın içeriği sunucu günlüğüne yazılmaz, yalnızca gerçekleştiği belirtilir.
        Log($"{sender.UserName} → {recipient.UserName}: [özel mesaj]");
        await SafeWriteAsync(recipient, message).ConfigureAwait(false);
        if (!ReferenceEquals(recipient, sender))
        {
            await SafeWriteAsync(sender, message).ConfigureAwait(false);
        }
    }

    private async Task RemoveClientAsync(ConnectedClient client)
    {
        if (!_clients.TryRemove(new KeyValuePair<string, ConnectedClient>(client.UserName, client)))
        {
            return;
        }

        Log($"{client.UserName} ayrıldı.");
        ClientDisconnected?.Invoke(this, new ClientEventArgs(client));

        if (IsRunning)
        {
            await BroadcastAsync(SystemPacket($"{client.UserName} sohbetten ayrıldı.")).ConfigureAwait(false);
            await BroadcastUserListAsync().ConfigureAwait(false);
        }
    }

    private Task BroadcastUserListAsync()
    {
        string[] users = _clients.Keys.OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase).ToArray();
        return BroadcastAsync(new ChatPacket { Type = PacketType.UserList, Users = users });
    }

    private Task BroadcastAsync(ChatPacket packet) =>
        Task.WhenAll(_clients.Values.Select(client => SafeWriteAsync(client, packet)));

    // Kopmuş bir istemciye yazma hatası, diğerlerine iletimi engellememeli.
    // O istemcinin kendi okuma döngüsü kopmayı fark edip onu listeden çıkarır.
    private static async Task SafeWriteAsync(ConnectedClient client, ChatPacket packet)
    {
        try
        {
            await client.Connection.WriteAsync(packet).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException)
        {
        }
    }

    private static ChatPacket SystemPacket(string text) =>
        new() { Type = PacketType.System, Text = text, Timestamp = DateTime.Now };

    private void Log(string text) => LogAdded?.Invoke(this, new ServerLogEventArgs(DateTime.Now, text));
}

public sealed class ClientEventArgs(ConnectedClient client) : EventArgs
{
    public ConnectedClient Client { get; } = client;
}

public sealed class ServerLogEventArgs(DateTime timestamp, string text) : EventArgs
{
    public DateTime Timestamp { get; } = timestamp;
    public string Text { get; } = text;
}
