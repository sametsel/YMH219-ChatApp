using ChatApp.Core.Client;
using ChatApp.Core.Protocol;

namespace ChatApp.ClientUI;

public partial class ClientForm : Form
{
    private const string EveryoneOption = "Herkes";

    private readonly ChatClient _client = new();
    private bool _isConnecting;

    public ClientForm()
    {
        InitializeComponent();

        _client.PacketReceived += Client_PacketReceived;
        _client.Disconnected += Client_Disconnected;

        ResetRecipients();
        UpdateUi();
    }

    private async void BtnConnect_Click(object? sender, EventArgs e)
    {
        string host = txtHost.Text.Trim();
        string userName = txtUserName.Text.Trim();

        if (host.Length == 0 || userName.Length == 0)
        {
            ShowError("Sunucu adresi ve kullanıcı adı boş bırakılamaz.");
            return;
        }

        _isConnecting = true;
        SetStatus("BAĞLANIYOR...", Color.FromArgb(180, 83, 9));
        UpdateConnectionControls();

        try
        {
            await _client.ConnectAsync(host, (int)numPort.Value, userName);
            AddLine($"[{DateTime.Now:HH:mm:ss}] Sunucuya {userName} olarak bağlanıldı.");
            txtMessage.Focus();
        }
        catch (ChatConnectionException ex)
        {
            // Bağlantı hatası uygulamayı kapatmaz; kullanıcıya bildirilir ve tekrar denenebilir.
            ShowError(ex.Message);
        }
        finally
        {
            _isConnecting = false;
            UpdateUi();
        }
    }

    private async void BtnDisconnect_Click(object? sender, EventArgs e) => await _client.DisconnectAsync();

    private async void BtnSend_Click(object? sender, EventArgs e) => await SendCurrentMessageAsync();

    private async void TxtMessage_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true; // Windows'un "ding" sesini engeller
            await SendCurrentMessageAsync();
        }
    }

    private async Task SendCurrentMessageAsync()
    {
        string text = txtMessage.Text.Trim();
        if (text.Length == 0 || !_client.IsConnected)
        {
            return;
        }

        string? target = cmbRecipient.SelectedItem as string;
        if (target == EveryoneOption)
        {
            target = null;
        }

        try
        {
            await _client.SendAsync(text, target);
            txtMessage.Clear();
            txtMessage.Focus();
        }
        catch (ChatConnectionException ex)
        {
            ShowError(ex.Message);
        }
    }

    private void BtnSaveHistory_Click(object? sender, EventArgs e)
    {
        if (lstMessages.Items.Count == 0)
        {
            ShowInfo("Kaydedilecek mesaj yok.");
            return;
        }

        saveFileDialog.FileName = $"sohbet-{DateTime.Now:yyyyMMdd-HHmmss}.txt";
        if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            ChatHistoryWriter.Write(saveFileDialog.FileName, lstMessages.Items.Cast<string>());
            ShowInfo($"Sohbet geçmişi kaydedildi:\n{saveFileDialog.FileName}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ShowError($"Dosya kaydedilemedi:\n{ex.Message}");
        }
    }

    private async void ClientForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        await _client.DisconnectAsync();
    }

    // Ağ olayları arka plan iş parçacığından gelir; arayüze BeginInvoke ile geçilir.
    private void RunOnUi(Action action)
    {
        if (IsDisposed || !IsHandleCreated)
        {
            return;
        }

        BeginInvoke(action);
    }

    private void Client_PacketReceived(object? sender, ChatPacket packet) => RunOnUi(() => HandlePacket(packet));

    private void Client_Disconnected(object? sender, bool userRequested) => RunOnUi(() =>
    {
        AddLine(userRequested
            ? $"[{DateTime.Now:HH:mm:ss}] Bağlantı kesildi."
            : $"[{DateTime.Now:HH:mm:ss}] Sunucu bağlantısı koptu.");

        lstUsers.Items.Clear();
        ResetRecipients();
        UpdateUi();

        if (!userRequested)
        {
            ShowError("Sunucu bağlantısı koptu. Sunucu kapanmış olabilir; yeniden bağlanmayı deneyin.");
        }
    });

    private void HandlePacket(ChatPacket packet)
    {
        string time = packet.Timestamp.ToString("HH:mm:ss");

        switch (packet.Type)
        {
            case PacketType.Message:
                AddLine($"[{time}] {packet.Sender}: {packet.Text}");
                break;

            case PacketType.PrivateMessage:
                AddLine(packet.Sender == _client.UserName
                    ? $"[{time}] (özel → {packet.Target}) {packet.Text}"
                    : $"[{time}] (özel ← {packet.Sender}) {packet.Text}");
                break;

            case PacketType.System:
                AddLine($"[{time}] * {packet.Text}");
                break;

            case PacketType.UserList:
                UpdateUsers(packet.Users ?? Array.Empty<string>());
                break;
        }
    }

    private void UpdateUsers(IReadOnlyList<string> users)
    {
        string? selected = cmbRecipient.SelectedItem as string;

        lstUsers.Items.Clear();
        lstUsers.Items.AddRange(users.Cast<object>().ToArray());
        grpUsers.Text = $"Çevrimiçi ({users.Count})";

        // Kişi kendine özel mesaj göndermesin diye alıcı listesine kendi adı eklenmez.
        cmbRecipient.Items.Clear();
        cmbRecipient.Items.Add(EveryoneOption);
        foreach (string user in users.Where(user => user != _client.UserName))
        {
            cmbRecipient.Items.Add(user);
        }

        // Seçili kişi ayrıldıysa yanlışlıkla başkasına yazılmasın diye "Herkes"e dönülür.
        cmbRecipient.SelectedItem = selected is not null && cmbRecipient.Items.Contains(selected)
            ? selected
            : EveryoneOption;
    }

    private void ResetRecipients()
    {
        grpUsers.Text = "Çevrimiçi (0)";
        cmbRecipient.Items.Clear();
        cmbRecipient.Items.Add(EveryoneOption);
        cmbRecipient.SelectedIndex = 0;
    }

    private void AddLine(string line)
    {
        lstMessages.Items.Add(line);
        lstMessages.TopIndex = lstMessages.Items.Count - 1;
    }

    private void UpdateUi()
    {
        if (_client.IsConnected)
        {
            SetStatus($"BAĞLI: {_client.UserName}", Color.FromArgb(21, 128, 61));
        }
        else
        {
            SetStatus("BAĞLI DEĞİL", Color.FromArgb(75, 85, 99));
        }

        UpdateConnectionControls();
    }

    private void UpdateConnectionControls()
    {
        bool connected = _client.IsConnected;
        bool canEditConnection = !connected && !_isConnecting;

        txtHost.Enabled = canEditConnection;
        numPort.Enabled = canEditConnection;
        txtUserName.Enabled = canEditConnection;
        btnConnect.Enabled = canEditConnection;
        btnDisconnect.Enabled = connected;
        btnSend.Enabled = connected;
        txtMessage.Enabled = connected;
        cmbRecipient.Enabled = connected;
    }

    private void SetStatus(string text, Color color)
    {
        lblStatus.Text = text;
        lblStatus.BackColor = color;
    }

    private void ShowError(string message) =>
        MessageBox.Show(this, message, "Chat", MessageBoxButtons.OK, MessageBoxIcon.Error);

    private void ShowInfo(string message) =>
        MessageBox.Show(this, message, "Chat", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
