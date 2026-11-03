using System.Net.Sockets;
using System.Text;

namespace ChatApp.Core.Protocol;

/// <summary>
/// Bir TcpClient üzerinde satır tabanlı paket okuma/yazma işini yapar.
/// Hem sunucu hem istemci aynı sınıfı kullanır; böylece protokol tek yerde kalır.
/// </summary>
public sealed class PacketConnection : IDisposable
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    private readonly TcpClient _client;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;

    // Aynı anda iki görev yazarsa paketler karışabilir; yazmalar sıraya alınır.
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public PacketConnection(TcpClient client)
    {
        _client = client;
        NetworkStream stream = client.GetStream();
        _reader = new StreamReader(stream, Utf8NoBom);
        _writer = new StreamWriter(stream, Utf8NoBom) { AutoFlush = true };
    }

    /// <summary>Sonraki paketi bekler. Karşı taraf bağlantıyı kapattıysa null döner.</summary>
    public async Task<ChatPacket?> ReadAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            string? line = await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line is null)
            {
                return null;
            }

            // Bozuk satır atlanır: tek hatalı mesaj bağlantıyı koparmamalı.
            if (ChatPacket.TryParse(line, out ChatPacket? packet))
            {
                return packet;
            }
        }
    }

    public async Task WriteAsync(ChatPacket packet, CancellationToken cancellationToken = default)
    {
        await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await _writer.WriteLineAsync(packet.Serialize().AsMemory(), cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public void Dispose()
    {
        // TcpClient kapatılınca bekleyen okuma/yazmalar IOException ile sonlanır.
        _client.Dispose();
        _writeLock.Dispose();
    }
}
