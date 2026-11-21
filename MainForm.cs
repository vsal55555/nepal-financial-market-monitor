using System.Data;

namespace MarketMonitor;

/// <summary>
/// UI only. The layout is built in code so you can paste this into a new
/// WinForms project (.NET 6+) and run it. No scraping, SQL or threading yet:
/// those come in your Day 2-8 steps and plug into the public controls below.
/// </summary>
public class MainForm : Form
{
    // ---- Colours ----
    static readonly Color Ink = Color.FromArgb(17, 24, 39);
    static readonly Color Muted = Color.FromArgb(75, 85, 99);
    static readonly Color Border = Color.FromArgb(213, 218, 225);
    static readonly Color Ground = Color.FromArgb(238, 240, 243);
    static readonly Color Accent = Color.FromArgb(31, 95, 191);
    static readonly Color Up = Color.FromArgb(21, 128, 61);
    static readonly Color Down = Color.FromArgb(180, 35, 24);

    // ---- Controls you will wire up later ----
    public readonly Button BtnStart = new() { Text = "Start Collection" };
    public readonly Button BtnStop = new() { Text = "Stop Collection", Enabled = false };
    public readonly Button BtnRefresh = new() { Text = "Refresh" };
    public readonly Button BtnExport = new() { Text = "Export Excel" };
    public readonly ComboBox CmbInterval = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    public readonly Label LblState = new();

    public readonly Label GoldValue = new(), SilverValue = new(), NepseValue = new(), NepseChange = new(),
        TurnoverValue = new(), GainerValue = new(), GainerSub = new(), LoserValue = new(), LoserSub = new();

    public readonly DataGridView GridStocks = new(), GridGold = new(), GridNepse = new();
    public readonly Label[] ThreadState = { new(), new(), new() };
    public readonly Label[] ThreadDetail = { new(), new(), new() };
    public readonly ListBox LogList = new();
    public readonly ToolStripStatusLabel StatusLeft = new(), StatusRight = new();

    public MainForm()
    {
        Text = "Financial Market Monitor — Nepal Stock & Gold/Silver";
        ClientSize = new Size(1280, 780);
        MinimumSize = new Size(1100, 700);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9.5f);
        BackColor = Ground;

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(0) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(BuildToolbar(), 0, 0);
        root.Controls.Add(BuildKpiRow(), 0, 1);
        root.Controls.Add(BuildMainArea(), 0, 2);

        var status = new StatusStrip { BackColor = Accent, SizingGrip = false };
        StatusLeft.Text = "Not connected to SQL Server";
        StatusLeft.ForeColor = Color.White;
        StatusLeft.Spring = true;
        StatusLeft.TextAlign = ContentAlignment.MiddleLeft;
        StatusRight.Text = "Sample data shown · Collection stopped";
        StatusRight.ForeColor = Color.White;
        status.Items.AddRange(new ToolStripItem[] { StatusLeft, StatusRight });

        Controls.Add(root);    // added first -> laid out last, so it fills the space left over
        Controls.Add(status);

        LoadSampleData();
    }

    // ================= Toolbar =================
    Control BuildToolbar()
    {
        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 249, 251),
            Padding = new Padding(16, 12, 16, 0),
            WrapContents = false
        };

        StyleButton(BtnStart, primary: true);
        StyleButton(BtnStop, danger: true);
        StyleButton(BtnRefresh);
        StyleButton(BtnExport);

        var intervalLabel = new Label { Text = "Collect every", AutoSize = true, ForeColor = Muted, Margin = new Padding(24, 9, 6, 0) };
        CmbInterval.Items.AddRange(new object[] { "1 minute", "5 minutes", "15 minutes" });
        CmbInterval.SelectedIndex = 1;
        CmbInterval.Width = 110;
        CmbInterval.Margin = new Padding(0, 6, 0, 0);

        LblState.Text = "● Stopped";
        LblState.AutoSize = false;
        LblState.Size = new Size(100, 32);
        LblState.TextAlign = ContentAlignment.MiddleCenter;
        LblState.ForeColor = Muted;
        LblState.BackColor = Color.FromArgb(229, 232, 237);
        LblState.Font = new Font(Font, FontStyle.Bold);
        LblState.Margin = new Padding(16, 3, 0, 0);

        bar.Controls.AddRange(new Control[] { BtnStart, BtnStop, BtnRefresh, BtnExport, intervalLabel, CmbInterval, LblState });
        return bar;
    }

    static void StyleButton(Button b, bool primary = false, bool danger = false)
    {
        b.Size = new Size(b.Text.Length > 8 ? 140 : 100, 36);
        b.FlatStyle = FlatStyle.Flat;
        b.Cursor = Cursors.Hand;
        b.Margin = new Padding(0, 0, 8, 0);
        b.FlatAppearance.BorderColor = primary ? Accent : Color.FromArgb(201, 207, 216);
        b.BackColor = primary ? Accent : Color.White;
        b.ForeColor = primary ? Color.White : danger ? Down : Ink;
        b.Font = new Font("Segoe UI", 9.5f, primary || danger ? FontStyle.Bold : FontStyle.Regular);
    }

    // ================= KPI cards =================
    Control BuildKpiRow()
    {
        var row = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 1, Padding = new Padding(16, 14, 16, 0) };
        for (int i = 0; i < 6; i++) row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 6));

        var goldSub = new Label(); var silverSub = new Label(); var turnSub = new Label();
        row.Controls.Add(Card("Gold today", GoldValue, goldSub), 0, 0);
        row.Controls.Add(Card("Silver today", SilverValue, silverSub), 1, 0);
        row.Controls.Add(Card("NEPSE index", NepseValue, NepseChange), 2, 0);
        row.Controls.Add(Card("Market turnover", TurnoverValue, turnSub), 3, 0);
        row.Controls.Add(Card("Top gainer", GainerValue, GainerSub), 4, 0);
        row.Controls.Add(Card("Top loser", LoserValue, LoserSub), 5, 0);

        goldSub.Text = "Rs per tola"; silverSub.Text = "Rs per tola"; turnSub.Text = "Today";
        foreach (var l in new[] { goldSub, silverSub, turnSub }) l.ForeColor = Muted;
        return row;
    }

    static Panel Card(string title, Label value, Label sub)
    {
        var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, 12, 0), Padding = new Padding(12, 8, 8, 8) };
        p.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(Border), 0, 0, p.Width - 1, p.Height - 1);

        var t = new Label { Text = title, Dock = DockStyle.Top, Height = 20, ForeColor = Muted, Font = new Font("Segoe UI", 8.5f) };
        value.Dock = DockStyle.Top; value.Height = 34; value.ForeColor = Ink;
        value.Font = new Font("Segoe UI Semibold", 16f);
        sub.Dock = DockStyle.Top; sub.Height = 20; sub.Font = new Font("Segoe UI Semibold", 8.5f);

        p.Controls.Add(sub); p.Controls.Add(value); p.Controls.Add(t);   // Top docks stack in reverse add order
        return p;
    }

    // ================= Main area =================
    Control BuildMainArea()
    {
        var area = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(16, 14, 16, 14) };
        area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        area.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 340));

        var tabs = new TabControl { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0), Padding = new Point(18, 6) };
        tabs.TabPages.Add(GridTab("Stock Prices", GridStocks));
        tabs.TabPages.Add(GridTab("Gold / Silver", GridGold));
        tabs.TabPages.Add(GridTab("NEPSE Summary", GridNepse));
        area.Controls.Add(tabs, 0, 0);
        area.Controls.Add(BuildSidePanel(), 1, 0);
        return area;
    }

    static TabPage GridTab(string title, DataGridView grid)
    {
        var page = new TabPage(title) { BackColor = Color.White, Padding = new Padding(0) };
        grid.Dock = DockStyle.Fill;
        StyleGrid(grid);
        page.Controls.Add(grid);
        return page;
    }

    static void StyleGrid(DataGridView g)
    {
        g.ReadOnly = true;
        g.AllowUserToAddRows = false;
        g.AllowUserToDeleteRows = false;
        g.AllowUserToResizeRows = false;
        g.RowHeadersVisible = false;
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        g.BackgroundColor = Color.White;
        g.BorderStyle = BorderStyle.None;
        g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        g.GridColor = Color.FromArgb(229, 232, 237);
        g.EnableHeadersVisualStyles = false;
        g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 251);
        g.ColumnHeadersDefaultCellStyle.ForeColor = Ink;
        g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5f);
        g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
        g.ColumnHeadersHeight = 36;
        g.RowTemplate.Height = 34;
        g.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(222, 232, 248);
        g.DefaultCellStyle.SelectionForeColor = Ink;
        g.DataBindingComplete += (_, _) =>
        {
            foreach (DataGridViewColumn c in g.Columns)
            {
                bool numeric = c.ValueType == typeof(decimal) || c.ValueType == typeof(long) || c.ValueType == typeof(double);
                c.DefaultCellStyle.Alignment = numeric ? DataGridViewContentAlignment.MiddleRight : DataGridViewContentAlignment.MiddleLeft;
                c.HeaderCell.Style.Alignment = c.DefaultCellStyle.Alignment;
            }
        };
    }

    // ================= Side panel =================
    Control BuildSidePanel()
    {
        var side = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        side.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));
        side.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Collector threads
        var threads = new GroupBox { Text = "Collector threads", Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(10, 6, 10, 6) };
        var tl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3 };
        tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        string[] names = { "Thread 1 · Gold / Silver", "Thread 2 · NEPSE Summary", "Thread 3 · Stock Prices" };
        for (int i = 0; i < 3; i++)
        {
            tl.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3f));
            var box = new Panel { Dock = DockStyle.Fill };
            box.Controls.Add(new Label { Text = "Waiting for first run", ForeColor = Muted, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8.5f) });
            ThreadDetail[i].Text = "Waiting for first run";
            ThreadDetail[i].ForeColor = Muted; ThreadDetail[i].Dock = DockStyle.Bottom; ThreadDetail[i].Height = 18;
            ThreadDetail[i].Font = new Font("Segoe UI", 8.5f);
            var name = new Label { Text = names[i], Dock = DockStyle.Top, Height = 20, ForeColor = Ink };
            var left = new Panel { Dock = DockStyle.Fill };
            left.Controls.Add(ThreadDetail[i]); left.Controls.Add(name);

            ThreadState[i].Text = "Stopped"; ThreadState[i].AutoSize = false; ThreadState[i].Size = new Size(70, 24);
            ThreadState[i].TextAlign = ContentAlignment.MiddleCenter; ThreadState[i].BackColor = Color.FromArgb(238, 240, 243);
            ThreadState[i].ForeColor = Color.FromArgb(55, 65, 81); ThreadState[i].Font = new Font("Segoe UI Semibold", 8.5f);
            ThreadState[i].Anchor = AnchorStyles.Right;

            tl.Controls.Add(left, 0, i);
            tl.Controls.Add(ThreadState[i], 1, i);
        }
        threads.Controls.Add(tl);

        // Activity log
        var log = new GroupBox { Text = "Activity log", Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(8, 6, 8, 8), Margin = new Padding(0, 12, 0, 0) };
        LogList.Dock = DockStyle.Fill;
        LogList.BorderStyle = BorderStyle.None;
        LogList.BackColor = Color.FromArgb(248, 249, 251);
        LogList.Font = new Font("Consolas", 9f);
        LogList.IntegralHeight = false;
        log.Controls.Add(LogList);

        side.Controls.Add(threads, 0, 0);
        side.Controls.Add(log, 0, 1);
        return side;
    }

    // ================= Sample data (replace in Day 3-4) =================
    void LoadSampleData()
    {
        var stocks = new DataTable();
        stocks.Columns.Add("Symbol", typeof(string));
        stocks.Columns.Add("Company", typeof(string));
        stocks.Columns.Add("LTP (Rs)", typeof(decimal));
        stocks.Columns.Add("Change", typeof(decimal));
        stocks.Columns.Add("Change %", typeof(decimal));
        stocks.Columns.Add("Volume", typeof(long));
        stocks.Columns.Add("Updated", typeof(string));
        stocks.Rows.Add("NABIL", "Nabil Bank", 1284.00m, 14.00m, 1.10m, 52340L, "15:10:02");
        stocks.Rows.Add("SBI", "Nepal SBI Bank", 502.50m, -3.20m, -0.63m, 31870L, "15:10:02");
        stocks.Rows.Add("NICA", "NIC Asia Bank", 818.00m, 9.50m, 1.17m, 24615L, "15:10:02");
        stocks.Rows.Add("GBIME", "Global IME Bank", 241.30m, 2.10m, 0.88m, 118200L, "15:10:02");
        stocks.Rows.Add("HBL", "Himalayan Bank", 296.00m, -1.80m, -0.60m, 40120L, "15:10:02");
        stocks.Rows.Add("NBL", "Nepal Bank", 189.70m, 0.00m, 0.00m, 9860L, "15:10:02");
        GridStocks.DataSource = stocks;
        GridStocks.CellFormatting += ColourChangeColumns;

        var gold = new DataTable();
        gold.Columns.Add("Date", typeof(string));
        gold.Columns.Add("Gold (Rs / tola)", typeof(decimal));
        gold.Columns.Add("Silver (Rs / tola)", typeof(decimal));
        gold.Columns.Add("Collected at", typeof(string));
        gold.Rows.Add("2026-11-19", 198500m, 2250m, "15:10:02");
        gold.Rows.Add("2026-11-19", 198300m, 2245m, "15:00:01");
        gold.Rows.Add("2026-11-19", 198100m, 2240m, "14:50:01");
        GridGold.DataSource = gold;

        var nepse = new DataTable();
        nepse.Columns.Add("Date", typeof(string));
        nepse.Columns.Add("NEPSE Index", typeof(decimal));
        nepse.Columns.Add("Change", typeof(decimal));
        nepse.Columns.Add("Volume", typeof(long));
        nepse.Columns.Add("Turnover (Rs)", typeof(long));
        nepse.Rows.Add("2026-11-19", 2850.00m, 12.55m, 9420000L, 8500000000L);
        nepse.Rows.Add("2026-11-18", 2837.45m, -6.10m, 8110000L, 7300000000L);
        GridNepse.DataSource = nepse;
        GridNepse.CellFormatting += ColourChangeColumns;

        foreach (var g in new[] { GridStocks, GridGold, GridNepse })
            foreach (DataGridViewColumn c in g.Columns)
                if (c.ValueType == typeof(decimal)) c.DefaultCellStyle.Format = "N2";
                else if (c.ValueType == typeof(long)) c.DefaultCellStyle.Format = "N0";
        GridGold.Columns[1].DefaultCellStyle.Format = "N0";
        GridGold.Columns[2].DefaultCellStyle.Format = "N0";

        GoldValue.Text = "198,500"; SilverValue.Text = "2,250";
        NepseValue.Text = "2,850.00"; NepseChange.Text = "▲ +12.55 (+0.44%)"; NepseChange.ForeColor = Up;
        TurnoverValue.Text = "Rs 8.5 B";
        GainerValue.Text = "NICA"; GainerSub.Text = "▲ +1.17%"; GainerSub.ForeColor = Up;
        LoserValue.Text = "SBI"; LoserSub.Text = "▼ -0.63%"; LoserSub.ForeColor = Down;

        LogList.Items.AddRange(new object[]
        {
            "15:10:02  Cycle complete in 2.0 s",
            "15:10:02  Stock Prices: 6 rows saved",
            "15:10:02  NEPSE Summary: 1 row saved",
            "15:10:01  Gold/Silver: 1 row saved",
            "15:10:00  Cycle started (3 tasks)"
        });
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        // 
        // MainForm
        // 
        ClientSize = new Size(836, 486);
        Name = "MainForm";
        ResumeLayout(false);

    }

    static void ColourChangeColumns(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        var g = (DataGridView)sender!;
        string col = g.Columns[e.ColumnIndex].HeaderText;
        if ((col == "Change" || col == "Change %") && e.Value is decimal d)
        {
            e.CellStyle.ForeColor = d > 0 ? Up : d < 0 ? Down : Muted;
            e.CellStyle.Font = new Font("Segoe UI Semibold", 9.5f);
            e.CellStyle.Format = col == "Change %" ? "+0.00;-0.00;0.00" : "+0.00;-0.00;0.00";
        }
    }
}
