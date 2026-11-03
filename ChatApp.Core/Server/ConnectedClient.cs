using ChatApp.Core.Protocol;

namespace ChatApp.Core.Server;

/// <summary>Sunucuya bağlı tek bir istemcinin bağlantısı ve temel bilgileri.</summary>
public sealed class ConnectedClient
{
    internal PacketConnection Connection { get; }

    public string UserName { get; }
    public string RemoteAddress { get; }
    public DateTime ConnectedAt { get; }

    internal ConnectedClient(PacketConnection connection, string userName, string remoteAddress, DateTime connectedAt)
    {
        Connection = connection;
        UserName = userName;
        RemoteAddress = remoteAddress;
        ConnectedAt = connectedAt;
    }
}
