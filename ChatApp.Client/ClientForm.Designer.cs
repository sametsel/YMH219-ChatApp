namespace ChatApp.ClientUI;

partial class ClientForm
{
    private System.ComponentModel.IContainer components = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblStatus = new Label();
        lblHost = new Label();
        txtHost = new TextBox();
        lblPort = new Label();
        numPort = new NumericUpDown();
        lblUserName = new Label();
        txtUserName = new TextBox();
        btnConnect = new Button();
        btnDisconnect = new Button();
        lstMessages = new ListBox();
        grpUsers = new GroupBox();
        lstUsers = new ListBox();
        lblRecipient = new Label();
        cmbRecipient = new ComboBox();
        txtMessage = new TextBox();
        btnSend = new Button();
        btnSaveHistory = new Button();
        saveFileDialog = new SaveFileDialog();
        ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
        grpUsers.SuspendLayout();
        SuspendLayout();
        //
        // lblStatus
        //
        lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.BackColor = Color.FromArgb(75, 85, 99);
        lblStatus.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblStatus.ForeColor = Color.White;
        lblStatus.Location = new Point(12, 12);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(760, 34);
        lblStatus.Text = "BAĞLI DEĞİL";
        lblStatus.TextAlign = ContentAlignment.MiddleCenter;
        //
        // lblHost
        //
        lblHost.AutoSize = true;
        lblHost.Location = new Point(12, 64);
        lblHost.Name = "lblHost";
        lblHost.Text = "Sunucu:";
        //
        // txtHost
        //
        txtHost.Location = new Point(66, 60);
        txtHost.Name = "txtHost";
        txtHost.Size = new Size(120, 23);
        txtHost.TabIndex = 0;
        txtHost.Text = "127.0.0.1";
        //
        // lblPort
        //
        lblPort.AutoSize = true;
        lblPort.Location = new Point(196, 64);
        lblPort.Name = "lblPort";
        lblPort.Text = "Port:";
        //
        // numPort
        //
        numPort.Location = new Point(232, 60);
        numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
        numPort.Minimum = new decimal(new int[] { 1024, 0, 0, 0 });
        numPort.Name = "numPort";
        numPort.Size = new Size(70, 23);
        numPort.TabIndex = 1;
        numPort.Value = new decimal(new int[] { 5000, 0, 0, 0 });
        //
        // lblUserName
        //
        lblUserName.AutoSize = true;
        lblUserName.Location = new Point(312, 64);
        lblUserName.Name = "lblUserName";
        lblUserName.Text = "Kullanıcı adı:";
        //
        // txtUserName
        //
        txtUserName.Location = new Point(392, 60);
        txtUserName.MaxLength = 20;
        txtUserName.Name = "txtUserName";
        txtUserName.Size = new Size(140, 23);
        txtUserName.TabIndex = 2;
        //
        // btnConnect
        //
        btnConnect.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnConnect.Location = new Point(542, 56);
        btnConnect.Name = "btnConnect";
        btnConnect.Size = new Size(96, 32);
        btnConnect.TabIndex = 3;
        btnConnect.Text = "Bağlan";
        btnConnect.UseVisualStyleBackColor = true;
        btnConnect.Click += BtnConnect_Click;
        //
        // btnDisconnect
        //
        btnDisconnect.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnDisconnect.Location = new Point(644, 56);
        btnDisconnect.Name = "btnDisconnect";
        btnDisconnect.Size = new Size(128, 32);
        btnDisconnect.TabIndex = 4;
        btnDisconnect.Text = "Bağlantıyı Kes";
        btnDisconnect.UseVisualStyleBackColor = true;
        btnDisconnect.Click += BtnDisconnect_Click;
        //
        // lstMessages
        //
        lstMessages.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lstMessages.FormattingEnabled = true;
        lstMessages.HorizontalScrollbar = true;
        lstMessages.IntegralHeight = false;
        lstMessages.Location = new Point(12, 98);
        lstMessages.Name = "lstMessages";
        lstMessages.Size = new Size(570, 350);
        lstMessages.TabIndex = 5;
        //
        // grpUsers
        //
        grpUsers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
        grpUsers.Controls.Add(lstUsers);
        grpUsers.Location = new Point(592, 92);
        grpUsers.Name = "grpUsers";
        grpUsers.Size = new Size(180, 356);
        grpUsers.TabIndex = 6;
        grpUsers.TabStop = false;
        grpUsers.Text = "Çevrimiçi (0)";
        //
        // lstUsers
        //
        lstUsers.Dock = DockStyle.Fill;
        lstUsers.FormattingEnabled = true;
        lstUsers.IntegralHeight = false;
        lstUsers.Name = "lstUsers";
        lstUsers.TabIndex = 0;
        //
        // lblRecipient
        //
        lblRecipient.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblRecipient.AutoSize = true;
        lblRecipient.Location = new Point(12, 464);
        lblRecipient.Name = "lblRecipient";
        lblRecipient.Text = "Alıcı:";
        //
        // cmbRecipient
        //
        cmbRecipient.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        cmbRecipient.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbRecipient.Location = new Point(48, 460);
        cmbRecipient.Name = "cmbRecipient";
        cmbRecipient.Size = new Size(120, 23);
        cmbRecipient.TabIndex = 7;
        //
        // txtMessage
        //
        txtMessage.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtMessage.Location = new Point(176, 460);
        txtMessage.MaxLength = 2000;
        txtMessage.Name = "txtMessage";
        txtMessage.PlaceholderText = "Mesajınızı yazın ve Enter'a basın";
        txtMessage.Size = new Size(296, 23);
        txtMessage.TabIndex = 8;
        txtMessage.KeyDown += TxtMessage_KeyDown;
        //
        // btnSend
        //
        btnSend.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnSend.Location = new Point(480, 455);
        btnSend.Name = "btnSend";
        btnSend.Size = new Size(104, 32);
        btnSend.TabIndex = 9;
        btnSend.Text = "Mesaj Gönder";
        btnSend.UseVisualStyleBackColor = true;
        btnSend.Click += BtnSend_Click;
        //
        // btnSaveHistory
        //
        btnSaveHistory.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnSaveHistory.Location = new Point(592, 455);
        btnSaveHistory.Name = "btnSaveHistory";
        btnSaveHistory.Size = new Size(180, 32);
        btnSaveHistory.TabIndex = 10;
        btnSaveHistory.Text = "Geçmişi Kaydet";
        btnSaveHistory.UseVisualStyleBackColor = true;
        btnSaveHistory.Click += BtnSaveHistory_Click;
        //
        // saveFileDialog
        //
        saveFileDialog.DefaultExt = "txt";
        saveFileDialog.Filter = "Metin dosyası (*.txt)|*.txt";
        saveFileDialog.Title = "Sohbet geçmişini kaydet";
        //
        // ClientForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(784, 500);
        Controls.Add(lblStatus);
        Controls.Add(lblHost);
        Controls.Add(txtHost);
        Controls.Add(lblPort);
        Controls.Add(numPort);
        Controls.Add(lblUserName);
        Controls.Add(txtUserName);
        Controls.Add(btnConnect);
        Controls.Add(btnDisconnect);
        Controls.Add(lstMessages);
        Controls.Add(grpUsers);
        Controls.Add(lblRecipient);
        Controls.Add(cmbRecipient);
        Controls.Add(txtMessage);
        Controls.Add(btnSend);
        Controls.Add(btnSaveHistory);
        MinimumSize = new Size(800, 400);
        Name = "ClientForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Chat İstemcisi - YMH219";
        FormClosing += ClientForm_FormClosing;
        ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
        grpUsers.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    private Label lblStatus;
    private Label lblHost;
    private TextBox txtHost;
    private Label lblPort;
    private NumericUpDown numPort;
    private Label lblUserName;
    private TextBox txtUserName;
    private Button btnConnect;
    private Button btnDisconnect;
    private ListBox lstMessages;
    private GroupBox grpUsers;
    private ListBox lstUsers;
    private Label lblRecipient;
    private ComboBox cmbRecipient;
    private TextBox txtMessage;
    private Button btnSend;
    private Button btnSaveHistory;
    private SaveFileDialog saveFileDialog;
}
