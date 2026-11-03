using System.Net;
using System.Net.Sockets;
using ChatApp.Core.Server;

namespace ChatApp.ServerUI;

public partial class ServerForm : Form
{
    private readonly ChatServer _server = new();
    private bool _closingAfterStop;

    public ServerForm()
    {
        InitializeComponent();

        _server.LogAdded += Server_LogAdded;
        _server.ClientConnected += Server_ClientConnected;
        _server.ClientDisconnected += Server_ClientDisconnected;

        UpdateUi();
    }

    private void BtnStart_Click(object? sender, EventArgs e)
    {
        // Varsayılan olarak yalnızca bu bilgisayar bağlanabilir; yerel ağ açıkça seçilince açılır.
        IPAddress address = chkLan.Checked ? IPAddress.Any : IPAddress.Loopback;

        try
        {
            _server.Start(address, (int)numPort.Value);
        }
        catch (SocketException ex)
        {
            MessageBox.Show(this, $"Sunucu başlatılamadı. Port kullanımda olabilir.\n\n{ex.Message}",
                "Sunucu", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        UpdateUi();
    }

    private async void BtnStop_Click(object? sender, EventArgs e)
    {
        btnStop.Enabled = false;
        await _server.StopAsync();
        UpdateUi();
    }

    private async void ServerForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_closingAfterStop || !_server.IsRunning)
        {
            return;
        }

        // Kapanmadan önce istemcileri düzgünce bırak, sonra formu yeniden kapat.
        e.Cancel = true;
        _closingAfterStop = true;
        await _server.StopAsync();
        Close();
    }

    // Sunucu olayları arka plan iş parçacığından gelir; kontrollere yalnızca arayüz
    // iş parçacığı dokunabildiği için BeginInvoke ile oraya geçilir.
    private void RunOnUi(Action action)
    {
        if (IsDisposed || !IsHandleCreated)
        {
            return;
        }

        BeginInvoke(action);
    }

    private void Server_LogAdded(object? sender, ServerLogEventArgs e) => RunOnUi(() =>
    {
        lstLog.Items.Add($"[{e.Timestamp:HH:mm:ss}] {e.Text}");
        lstLog.TopIndex = lstLog.Items.Count - 1;
    });

    private void Server_ClientConnected(object? sender, ClientEventArgs e) => RunOnUi(() =>
    {
        var item = new ListViewItem(e.Client.UserName) { Tag = e.Client };
        item.SubItems.Add(e.Client.RemoteAddress);
        item.SubItems.Add(e.Client.ConnectedAt.ToString("HH:mm:ss"));
        lvClients.Items.Add(item);
        UpdateClientCount();
    });

    private void Server_ClientDisconnected(object? sender, ClientEventArgs e) => RunOnUi(() =>
    {
        foreach (ListViewItem item in lvClients.Items)
        {
            if (ReferenceEquals(item.Tag, e.Client))
            {
                lvClients.Items.Remove(item);
                break;
            }
        }
        UpdateClientCount();
    });

    private void UpdateClientCount() => grpClients.Text = $"Bağlı İstemciler ({lvClients.Items.Count})";

    private void UpdateUi()
    {
        bool running = _server.IsRunning;

        btnStart.Enabled = !running;
        btnStop.Enabled = running;
        numPort.Enabled = !running;
        chkLan.Enabled = !running;

        lblStatus.Text = running ? "SUNUCU ÇALIŞIYOR" : "SUNUCU KAPALI";
        lblStatus.BackColor = running ? Color.FromArgb(21, 128, 61) : Color.FromArgb(75, 85, 99);
    }
}
