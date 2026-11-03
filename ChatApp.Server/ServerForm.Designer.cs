namespace ChatApp.ServerUI;

partial class ServerForm
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
        lblPort = new Label();
        numPort = new NumericUpDown();
        chkLan = new CheckBox();
        btnStart = new Button();
        btnStop = new Button();
        grpClients = new GroupBox();
        lvClients = new ListView();
        colUser = new ColumnHeader();
        colAddress = new ColumnHeader();
        colTime = new ColumnHeader();
        grpLog = new GroupBox();
        lstLog = new ListBox();
        ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
        grpClients.SuspendLayout();
        grpLog.SuspendLayout();
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
        lblStatus.Text = "SUNUCU KAPALI";
        lblStatus.TextAlign = ContentAlignment.MiddleCenter;
        //
        // lblPort
        //
        lblPort.AutoSize = true;
        lblPort.Location = new Point(12, 64);
        lblPort.Name = "lblPort";
        lblPort.Text = "Port:";
        //
        // numPort
        //
        numPort.Location = new Point(50, 60);
        numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
        numPort.Minimum = new decimal(new int[] { 1024, 0, 0, 0 });
        numPort.Name = "numPort";
        numPort.Size = new Size(80, 23);
        numPort.TabIndex = 0;
        numPort.Value = new decimal(new int[] { 5000, 0, 0, 0 });
        //
        // chkLan
        //
        chkLan.AutoSize = true;
        chkLan.Location = new Point(146, 62);
        chkLan.Name = "chkLan";
        chkLan.TabIndex = 1;
        chkLan.Text = "Yerel ağdan bağlantıya izin ver";
        chkLan.UseVisualStyleBackColor = true;
        //
        // btnStart
        //
        btnStart.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnStart.Location = new Point(556, 56);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(104, 32);
        btnStart.TabIndex = 2;
        btnStart.Text = "Başlat";
        btnStart.UseVisualStyleBackColor = true;
        btnStart.Click += BtnStart_Click;
        //
        // btnStop
        //
        btnStop.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnStop.Location = new Point(668, 56);
        btnStop.Name = "btnStop";
        btnStop.Size = new Size(104, 32);
        btnStop.TabIndex = 3;
        btnStop.Text = "Durdur";
        btnStop.UseVisualStyleBackColor = true;
        btnStop.Click += BtnStop_Click;
        //
        // grpClients
        //
        grpClients.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        grpClients.Controls.Add(lvClients);
        grpClients.Location = new Point(12, 98);
        grpClients.Name = "grpClients";
        grpClients.Size = new Size(320, 400);
        grpClients.TabIndex = 4;
        grpClients.TabStop = false;
        grpClients.Text = "Bağlı İstemciler (0)";
        //
        // lvClients
        //
        lvClients.Columns.AddRange(new ColumnHeader[] { colUser, colAddress, colTime });
        lvClients.Dock = DockStyle.Fill;
        lvClients.FullRowSelect = true;
        lvClients.GridLines = true;
        lvClients.Name = "lvClients";
        lvClients.TabIndex = 0;
        lvClients.UseCompatibleStateImageBehavior = false;
        lvClients.View = View.Details;
        //
        // colUser
        //
        colUser.Text = "Kullanıcı";
        colUser.Width = 110;
        //
        // colAddress
        //
        colAddress.Text = "Adres";
        colAddress.Width = 120;
        //
        // colTime
        //
        colTime.Text = "Bağlanma";
        colTime.Width = 70;
        //
        // grpLog
        //
        grpLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grpLog.Controls.Add(lstLog);
        grpLog.Location = new Point(342, 98);
        grpLog.Name = "grpLog";
        grpLog.Size = new Size(430, 400);
        grpLog.TabIndex = 5;
        grpLog.TabStop = false;
        grpLog.Text = "Sunucu Günlüğü";
        //
        // lstLog
        //
        lstLog.Dock = DockStyle.Fill;
        lstLog.FormattingEnabled = true;
        lstLog.HorizontalScrollbar = true;
        lstLog.IntegralHeight = false;
        lstLog.Name = "lstLog";
        lstLog.TabIndex = 0;
        //
        // ServerForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(784, 510);
        Controls.Add(lblStatus);
        Controls.Add(lblPort);
        Controls.Add(numPort);
        Controls.Add(chkLan);
        Controls.Add(btnStart);
        Controls.Add(btnStop);
        Controls.Add(grpClients);
        Controls.Add(grpLog);
        MinimumSize = new Size(800, 400);
        Name = "ServerForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Chat Sunucusu - YMH219";
        FormClosing += ServerForm_FormClosing;
        ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
        grpClients.ResumeLayout(false);
        grpLog.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    private Label lblStatus;
    private Label lblPort;
    private NumericUpDown numPort;
    private CheckBox chkLan;
    private Button btnStart;
    private Button btnStop;
    private GroupBox grpClients;
    private ListView lvClients;
    private ColumnHeader colUser;
    private ColumnHeader colAddress;
    private ColumnHeader colTime;
    private GroupBox grpLog;
    private ListBox lstLog;
}
