#nullable enable
using System;
using System.IO;
using System.Linq;
using WinForms = System.Windows.Forms;
using Drawing = System.Drawing;
using System.Drawing.Drawing2D;

namespace MKVRenamer
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            WinForms.Application.EnableVisualStyles();
            WinForms.Application.SetCompatibleTextRenderingDefault(false);
            WinForms.Application.SetUnhandledExceptionMode(WinForms.UnhandledExceptionMode.CatchException);
            WinForms.Application.ThreadException += (s, e) => WinForms.MessageBox.Show(e.Exception.ToString(), "UI Thread Exception");
            AppDomain.CurrentDomain.UnhandledException += (s, e) => WinForms.MessageBox.Show(e.ExceptionObject?.ToString() ?? "Unknown", "Unhandled Exception");
            try { WinForms.Application.Run(new MainForm()); }
            catch (Exception ex) { WinForms.MessageBox.Show(ex.ToString(), "Startup Error"); }
        }
    }

    public class MainForm : WinForms.Form
    {
        // ===== THEME =====
        private static class Theme
        {
            // Modern colorful palette (emerald accent)
            public static readonly Drawing.Color Accent = Drawing.Color.FromArgb(16, 122, 72);       // emerald
            public static readonly Drawing.Color AccentHover = Drawing.Color.FromArgb(12, 98, 58);
            public static readonly Drawing.Color AccentPressed = Drawing.Color.FromArgb(8, 78, 46);
            public static readonly Drawing.Color Surface = Drawing.Color.FromArgb(232, 242, 238);    // soft tinted panel
            public static readonly Drawing.Color WindowBg = Drawing.Color.FromArgb(242, 247, 245);   // near-white neutral
            public static readonly Drawing.Color InputBg = Drawing.Color.FromArgb(252, 253, 252);
            public static readonly Drawing.Color Text = Drawing.Color.FromArgb(22, 28, 25);
            public static readonly Drawing.Color TextOnAccent = Drawing.Color.White;
            public static readonly Drawing.Color Border = Drawing.Color.FromArgb(208, 223, 219);
        }

        private sealed class ModernButton : WinForms.Button
        {
            public int CornerRadius { get; set; } = 14;
            private bool _hover, _pressed;
            public ModernButton()
            {
                SetStyle(WinForms.ControlStyles.AllPaintingInWmPaint | WinForms.ControlStyles.UserPaint | WinForms.ControlStyles.OptimizedDoubleBuffer, true);
                FlatStyle = WinForms.FlatStyle.Flat;
                FlatAppearance.BorderSize = 0;
                BackColor = Theme.Accent;
                ForeColor = Theme.TextOnAccent;
                Padding = new WinForms.Padding(16, 10, 16, 10);
                Margin = new WinForms.Padding(10);
                Font = new Drawing.Font("Segoe UI Semibold", 10.5F); AutoSize = true; AutoSizeMode = WinForms.AutoSizeMode.GrowAndShrink; MinimumSize = new Drawing.Size(140, 44);
                Cursor = WinForms.Cursors.Hand;
            }
            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                using var path = RoundedPath(new Drawing.Rectangle(0, 0, Width, Height), CornerRadius);
                Region = new Drawing.Region(path);
                Invalidate();
            }
            protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
            protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
            protected override void OnMouseDown(WinForms.MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
            protected override void OnMouseUp(WinForms.MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }
            protected override void OnPaint(WinForms.PaintEventArgs e)
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Drawing.RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
                using var path = RoundedPath(Drawing.Rectangle.Round(rect), CornerRadius);
                var bg = !Enabled ? Drawing.Color.FromArgb(180, Theme.Border) : (_pressed ? Theme.AccentPressed : (_hover ? Theme.AccentHover : Theme.Accent));
                using var b = new Drawing.SolidBrush(bg);
                using var pen = new Drawing.Pen(bg, 1f);
                g.FillPath(b, path);
                g.DrawPath(pen, path);

                // text
                var sf = new Drawing.StringFormat { Alignment = Drawing.StringAlignment.Center, LineAlignment = Drawing.StringAlignment.Center };
                using var txt = new Drawing.SolidBrush(Theme.TextOnAccent);
                g.DrawString(Text, Font, txt, rect, sf);
            }
            private static GraphicsPath RoundedPath(Drawing.Rectangle r, int radius)
            {
                int d = radius * 2; var gp = new GraphicsPath();
                gp.AddArc(r.X, r.Y, d, d, 180, 90);
                gp.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                gp.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                gp.CloseFigure();
                return gp;
            }
        }

        private void ApplyThemeRecursive(WinForms.Control root)
        {
            // Base colors
            root.BackColor = root is WinForms.TextBox or WinForms.ListBox or WinForms.CheckedListBox or WinForms.ListView ? Theme.InputBg : Theme.WindowBg;
            root.ForeColor = Theme.Text;
            if (root is WinForms.TabPage tp) tp.Padding = new WinForms.Padding(12);

            foreach (WinForms.Control c in root.Controls)
            {
                switch (c)
                {
                    case WinForms.Button b:
                        // fallback for non-ModernButton
                        b.FlatStyle = WinForms.FlatStyle.Flat; b.FlatAppearance.BorderSize = 0;
                        b.BackColor = Theme.Accent; b.ForeColor = Theme.TextOnAccent;
                        b.Font = new Drawing.Font("Segoe UI Semibold", 10.5F); b.Padding = new WinForms.Padding(14, 10, 14, 10); b.Margin = new WinForms.Padding(10);
                        break;
                    case WinForms.Label:
                        c.BackColor = Theme.WindowBg; c.ForeColor = Theme.Text; c.Margin = new WinForms.Padding(6);
                        break;
                    case WinForms.TextBox or WinForms.ListBox or WinForms.CheckedListBox:
                        c.BackColor = Theme.InputBg; c.ForeColor = Theme.Text; c.Margin = new WinForms.Padding(6);
                        break;
                    case WinForms.NumericUpDown nud:
                        nud.BackColor = Theme.InputBg; nud.ForeColor = Theme.Text; nud.Margin = new WinForms.Padding(6); break;
                    case WinForms.Panel or WinForms.TableLayoutPanel or WinForms.FlowLayoutPanel:
                        c.BackColor = Theme.Surface; c.Margin = new WinForms.Padding(6); break;
                    case WinForms.ListView lv:
                        lv.BackColor = Theme.InputBg; lv.ForeColor = Theme.Text; lv.GridLines = false;
                        lv.BorderStyle = WinForms.BorderStyle.FixedSingle; lv.Margin = new WinForms.Padding(6); break;
                }
                ApplyThemeRecursive(c);
            }
        }

        // ===== SHARED HELPERS =====
        private static string GetUniquePath(string targetPath)
        {
            if (!File.Exists(targetPath)) return targetPath;
            var dir = Path.GetDirectoryName(targetPath) ?? string.Empty;
            var name = Path.GetFileNameWithoutExtension(targetPath);
            var ext = Path.GetExtension(targetPath);
            int i = 1; string candidate;
            do { candidate = Path.Combine(dir, $"{name} ({i++}){ext}"); } while (File.Exists(candidate));
            return candidate;
        }
        private static bool PathsEqual(string a, string b) =>
            string.Equals(Path.GetFullPath(a).TrimEnd(Path.DirectorySeparatorChar),
                          Path.GetFullPath(b).TrimEnd(Path.DirectorySeparatorChar),
                          StringComparison.OrdinalIgnoreCase);

        // ===== FIELDS =====
        private WinForms.TabControl tabs = null!;
        private WinForms.TabPage tabMain = null!;
        private WinForms.TabPage tabRenamer = null!;
        private WinForms.TabPage tabNewFolder = null!;

        // Tab1
        private WinForms.TextBox txtFolder1 = null!;
        private ModernButton btnBrowse1 = null!;
        private WinForms.ListView lvFiles1 = null!;
        private ModernButton btnSetMain1 = null!;
        private WinForms.Label lblMain1 = null!;
        private WinForms.CheckBox chkHasExtras1 = null!;
        private WinForms.CheckedListBox clbExtras1 = null!;
        private ModernButton btnExecute1 = null!;
        private WinForms.TextBox txtLog1 = null!;
        private string selectedFolder1 = string.Empty;
        private string mainFileFullPath1 = string.Empty;
        private WinForms.SplitContainer split1 = null!;

        // Tab2
        private WinForms.TextBox txtFolder2 = null!;
        private ModernButton btnBrowse2 = null!;
        private WinForms.ListBox lstFiles2 = null!;
        private WinForms.Label lblHint2 = null!;
        private WinForms.TextBox txtPrefix2 = null!;
        private WinForms.NumericUpDown numStart2 = null!;
        private ModernButton btnRenameSeq2 = null!;
        private WinForms.TextBox txtLog2 = null!;
        private string selectedFolder2 = string.Empty;

        // Tab3
        private WinForms.TextBox txtParent3 = null!;
        private ModernButton btnBrowse3 = null!;
        private WinForms.TextBox txtName3 = null!;
        private WinForms.CheckBox chkExtras3 = null!;
        private ModernButton btnCreate3 = null!;
        private WinForms.TextBox txtLog3 = null!;
        private string selectedParent3 = string.Empty;

        public MainForm()
        {
            Text = "MKV Organizer";
            StartPosition = WinForms.FormStartPosition.CenterScreen;
            MinimumSize = new Drawing.Size(960, 600);
            ClientSize = new Drawing.Size(1140, 760);
            AutoScaleMode = WinForms.AutoScaleMode.Font;
            Font = new Drawing.Font("Segoe UI", 10.5F);
            BackColor = Theme.WindowBg; ForeColor = Theme.Text; DoubleBuffered = true;

            tabs = new WinForms.TabControl { Dock = WinForms.DockStyle.Fill, DrawMode = WinForms.TabDrawMode.OwnerDrawFixed, Padding = new Drawing.Point(22, 8) };
            tabs.DrawItem += Tabs_DrawItem;

            tabMain = new WinForms.TabPage("Main & Extras");
            tabRenamer = new WinForms.TabPage("Rename Extras");
            tabNewFolder = new WinForms.TabPage("New Movie Folder");
            tabs.TabPages.Add(tabMain);
            tabs.TabPages.Add(tabRenamer);
            tabs.TabPages.Add(tabNewFolder);
            Controls.Add(tabs);

            BuildTab1();
            BuildTab2();
            BuildTab3();

            ApplyThemeRecursive(this);
        }

        private void Tabs_DrawItem(object? sender, WinForms.DrawItemEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var tab = tabs.TabPages[e.Index];
            var selected = (e.State & WinForms.DrawItemState.Selected) == WinForms.DrawItemState.Selected;
            var bounds = new Drawing.Rectangle(e.Bounds.X + 4, e.Bounds.Y + 6, e.Bounds.Width - 8, e.Bounds.Height - 12);
            using var bg = new Drawing.SolidBrush(selected ? Theme.Accent : Theme.Surface);
            using var pen = new Drawing.Pen(selected ? Theme.Accent : Theme.Border, 1f);
            using var path = new GraphicsPath(); int r = 10;
            path.AddArc(bounds.X, bounds.Y, r, r, 180, 90);
            path.AddArc(bounds.Right - r, bounds.Y, r, r, 270, 90);
            path.AddArc(bounds.Right - r, bounds.Bottom - r, r, r, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - r, r, r, 90, 90); path.CloseFigure();
            g.FillPath(bg, path); g.DrawPath(pen, path);
            using var txt = new Drawing.SolidBrush(selected ? Theme.TextOnAccent : Theme.Text);
            var sf = new Drawing.StringFormat { Alignment = Drawing.StringAlignment.Center, LineAlignment = Drawing.StringAlignment.Center };
            g.DrawString(tab.Text, Font, txt, bounds, sf);
        }

        // ===== TAB 1 =====
        private void BuildTab1()
        {
            var table = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new WinForms.Padding(14) };
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.AutoSize));
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Percent, 100));
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Absolute, 40));
            tabMain.Controls.Clear(); tabMain.Controls.Add(table);

            // Folder row
            var folderRow1 = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Top, ColumnCount = 2, AutoSize = true, Padding = new WinForms.Padding(2) };
            folderRow1.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 100F));
            folderRow1.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            txtFolder1 = new WinForms.TextBox { Dock = WinForms.DockStyle.Fill, ReadOnly = true, PlaceholderText = "Select a movie folder (e.g., Austin Powers Goldmember (2002))" };
            btnBrowse1 = new ModernButton { Text = "Browse…" }; btnBrowse1.Click += BtnBrowse1_Click;
            folderRow1.Controls.Add(txtFolder1, 0, 0); folderRow1.Controls.Add(btnBrowse1, 1, 0);
            table.Controls.Add(folderRow1, 0, 0);

            // Split main area
            split1 = new WinForms.SplitContainer { Dock = WinForms.DockStyle.Fill, Orientation = WinForms.Orientation.Vertical, SplitterWidth = 6 };
            split1.HandleCreated += (s, e) => SafeInitSplit1();
            table.Controls.Add(split1, 0, 1);

            // Left: list
            lvFiles1 = new WinForms.ListView { Dock = WinForms.DockStyle.Fill, View = WinForms.View.Details, FullRowSelect = true, HideSelection = false, MultiSelect = false, BorderStyle = WinForms.BorderStyle.FixedSingle };
            lvFiles1.Columns.Add("File", 460);
            lvFiles1.Columns.Add("Duration", 120);
            var pnlLeftBottom = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Bottom, ColumnCount = 1, RowCount = 2, AutoSize = true, Padding = new WinForms.Padding(6) };
            pnlLeftBottom.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.AutoSize));
            pnlLeftBottom.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.AutoSize));
            btnSetMain1 = new ModernButton { Text = "Set Selected as Main" };
            btnSetMain1.AutoSize = false; btnSetMain1.Dock = WinForms.DockStyle.Top; btnSetMain1.Height = 44; btnSetMain1.MinimumSize = new Drawing.Size(0, 44);
            btnSetMain1.Click += BtnSetMain1_Click;
            lblMain1 = new WinForms.Label { Text = "Main movie: (none)", Dock = WinForms.DockStyle.Top, AutoSize = false, Height = 48, Padding = new WinForms.Padding(10), BorderStyle = WinForms.BorderStyle.FixedSingle, TextAlign = Drawing.ContentAlignment.MiddleLeft, Margin = new WinForms.Padding(0, 6, 0, 0) };
            pnlLeftBottom.Controls.Add(btnSetMain1, 0, 0);
            pnlLeftBottom.Controls.Add(lblMain1, 0, 1);
            split1.Panel1.Controls.Add(lvFiles1);
            split1.Panel1.Controls.Add(pnlLeftBottom);

            // Right: extras
            var rightTop = new WinForms.FlowLayoutPanel { Dock = WinForms.DockStyle.Top, Height = 46, FlowDirection = WinForms.FlowDirection.LeftToRight, WrapContents = false, Padding = new WinForms.Padding(6, 10, 6, 0) };
            chkHasExtras1 = new WinForms.CheckBox { Text = "I have extras to add", AutoSize = true };
            chkHasExtras1.CheckedChanged += (s, e) => clbExtras1.Enabled = chkHasExtras1.Checked;
            rightTop.Controls.Add(chkHasExtras1);
            clbExtras1 = new WinForms.CheckedListBox { Dock = WinForms.DockStyle.Fill, Enabled = false, CheckOnClick = true, BorderStyle = WinForms.BorderStyle.FixedSingle };
            btnExecute1 = new ModernButton { Text = "Rename / Move" }; btnExecute1.AutoSize = false; btnExecute1.Height = 44; btnExecute1.MinimumSize = new Drawing.Size(0, 44); btnExecute1.Dock = WinForms.DockStyle.Bottom; btnExecute1.Click += BtnExecute1_Click;
            split1.Panel2.Controls.Add(clbExtras1); split1.Panel2.Controls.Add(btnExecute1); split1.Panel2.Controls.Add(rightTop);

            // Log
            txtLog1 = new WinForms.TextBox { Dock = WinForms.DockStyle.Fill, Multiline = false, ReadOnly = true, BorderStyle = WinForms.BorderStyle.FixedSingle };
            table.Controls.Add(txtLog1, 0, 2);
        }

        private void SafeInitSplit1()
        {
            try
            {
                if (split1 == null || !split1.IsHandleCreated || split1.Width <= 0) return;
                int min1 = Math.Min(300, Math.Max(0, split1.Width / 3));
                int min2 = Math.Min(300, Math.Max(0, split1.Width / 3));
                split1.Panel1MinSize = min1; split1.Panel2MinSize = min2;
                int maxAllowed = split1.Width - split1.Panel2MinSize;
                int desired = Math.Clamp(split1.Width / 2, split1.Panel1MinSize, Math.Max(split1.Panel1MinSize, maxAllowed - 1));
                split1.SplitterDistance = desired;
            }
            catch { }
        }

        private void BtnBrowse1_Click(object? sender, EventArgs e)
        {
            using var fbd = new WinForms.FolderBrowserDialog() { Description = "Select a movie folder (e.g., 'Austin Powers Goldmember (2002)')" };
            if (fbd.ShowDialog(this) == WinForms.DialogResult.OK)
            { selectedFolder1 = fbd.SelectedPath ?? string.Empty; txtFolder1.Text = selectedFolder1; LoadFiles1(); }
        }
        private void LoadFiles1()
        {
            lvFiles1.Items.Clear(); clbExtras1.Items.Clear(); mainFileFullPath1 = string.Empty; lblMain1.Text = "Main movie: (none)";
            if (string.IsNullOrWhiteSpace(selectedFolder1) || !Directory.Exists(selectedFolder1)) return;
            var files = Directory.GetFiles(selectedFolder1, "*.mkv", SearchOption.TopDirectoryOnly).OrderBy(Path.GetFileName).ToArray();
            foreach (var f in files)
            { var it = new WinForms.ListViewItem(Path.GetFileName(f)); it.SubItems.Add(GetDurationStringSafe(f)); lvFiles1.Items.Add(it); }
            txtLog1.Text = files.Length == 0 ? "No .mkv files found in this folder." : $"Found {files.Length} .mkv file(s).";
        }
        private void BtnSetMain1_Click(object? sender, EventArgs e)
        {
            if (lvFiles1.SelectedItems.Count == 0)
            { WinForms.MessageBox.Show(this, "Select a file and click Set Selected as Main.", "No selection", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Information); return; }
            var name = lvFiles1.SelectedItems[0].Text; mainFileFullPath1 = Path.Combine(selectedFolder1, name); lblMain1.Text = "Main movie: " + name;
            clbExtras1.Items.Clear(); foreach (WinForms.ListViewItem x in lvFiles1.Items) if (!string.Equals(x.Text, name, StringComparison.OrdinalIgnoreCase)) clbExtras1.Items.Add(x.Text, false);
        }
        private void BtnExecute1_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedFolder1) || !Directory.Exists(selectedFolder1))
            { WinForms.MessageBox.Show(this, "Choose a valid folder first.", "Folder required", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Warning); return; }
            if (string.IsNullOrEmpty(mainFileFullPath1) || !File.Exists(mainFileFullPath1))
            { WinForms.MessageBox.Show(this, "Set the main movie file.", "Main file required", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Warning); return; }
            try
            {
                var folderName = new DirectoryInfo(selectedFolder1).Name; var mainTarget = Path.Combine(selectedFolder1, folderName + ".mkv");
                if (!PathsEqual(mainFileFullPath1, mainTarget)) { var final = GetUniquePath(mainTarget); File.Move(mainFileFullPath1, final); Log1($"Renamed main: '{Path.GetFileName(mainFileFullPath1)}' -> '{Path.GetFileName(final)}'"); mainFileFullPath1 = final; }
                else Log1("Main file already has the desired name.");

                if (chkHasExtras1.Checked && clbExtras1.Items.Count > 0)
                {
                    var extrasDir = Path.Combine(selectedFolder1, "Extras"); Directory.CreateDirectory(extrasDir);
                    var chosen = clbExtras1.CheckedItems.Cast<object>().Select(o => o!.ToString()).Where(s => !string.IsNullOrEmpty(s)).Cast<string>().ToArray();
                    foreach (var n in chosen)
                    {
                        var src = Path.Combine(selectedFolder1, n); if (!File.Exists(src)) continue;
                        var dest = GetUniquePath(Path.Combine(extrasDir, n));
                        File.Move(src, dest); Log1($"Moved extra: '{n}' -> {Path.Combine("Extras", Path.GetFileName(dest))}");
                    }
                }
                Log1("Done."); WinForms.MessageBox.Show(this, "All operations completed.", "Success", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Information); LoadFiles1();
            }
            catch (Exception ex) { WinForms.MessageBox.Show(this, ex.Message, "Error", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error); Log1("ERROR: " + ex); }
        }
        private void Log1(string msg) => txtLog1.Text = msg;

        private string GetDurationStringSafe(string path)
        {
            try
            {
                var dir = Path.GetDirectoryName(path); var name = Path.GetFileName(path);
                if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(name)) return string.Empty;
                var shellType = Type.GetTypeFromProgID("Shell.Application"); if (shellType == null) return string.Empty;
                dynamic shell = Activator.CreateInstance(shellType)!; dynamic folder = shell.NameSpace(dir); if (folder == null) return string.Empty; dynamic item = folder.ParseName(name); if (item == null) return string.Empty;
                int lenIdx = -1; for (int i = 0; i < 320; i++) { string header = folder.GetDetailsOf(null, i); if (string.Equals(header, "Length", StringComparison.OrdinalIgnoreCase) || string.Equals(header, "Duration", StringComparison.OrdinalIgnoreCase)) { lenIdx = i; break; } }
                if (lenIdx < 0) return string.Empty; string val = folder.GetDetailsOf(item, lenIdx); return val ?? string.Empty;
            }
            catch { return string.Empty; }
        }

        // ===== TAB 2 =====
        private void BuildTab2()
        {
            var table = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new WinForms.Padding(14) };
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.AutoSize));
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Percent, 100));
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.AutoSize));
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Absolute, 40));
            tabRenamer.Controls.Clear(); tabRenamer.Controls.Add(table);

            var folderRow2 = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Top, ColumnCount = 2, AutoSize = true, Padding = new WinForms.Padding(2) };
            folderRow2.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 100F));
            folderRow2.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            txtFolder2 = new WinForms.TextBox { Dock = WinForms.DockStyle.Fill, ReadOnly = true, PlaceholderText = "Select folder with extras" };
            btnBrowse2 = new ModernButton { Text = "Browse…" }; btnBrowse2.Click += BtnBrowse2_Click;
            folderRow2.Controls.Add(txtFolder2, 0, 0); folderRow2.Controls.Add(btnBrowse2, 1, 0);
            table.Controls.Add(folderRow2, 0, 0);

            lstFiles2 = new WinForms.ListBox { Dock = WinForms.DockStyle.Fill, SelectionMode = WinForms.SelectionMode.MultiExtended, BorderStyle = WinForms.BorderStyle.FixedSingle };
            table.Controls.Add(lstFiles2, 0, 1);

            var controlsRow = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Top, ColumnCount = 2, AutoSize = true, Padding = new WinForms.Padding(0, 10, 0, 10) };
            controlsRow.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 100F));
            controlsRow.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));

            var leftBlock = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Fill, ColumnCount = 1, AutoSize = true };
            leftBlock.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 100F));
            lblHint2 = new WinForms.Label { AutoSize = true, Margin = new WinForms.Padding(6, 2, 6, 6), Text = "Tip: Select files to rename only those; none selected = all (.mkv only)." };
            var inputs = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Top, ColumnCount = 4, AutoSize = true, Margin = new WinForms.Padding(6, 2, 6, 6) };
            inputs.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            inputs.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            inputs.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            inputs.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            var lblPrefix = new WinForms.Label { AutoSize = true, Text = "Prefix:", Anchor = WinForms.AnchorStyles.Left, Margin = new WinForms.Padding(0, 6, 6, 6) };
            txtPrefix2 = new WinForms.TextBox { Width = 280, Anchor = WinForms.AnchorStyles.Left, Margin = new WinForms.Padding(0, 3, 12, 3), Text = "Extras - " };
            var lblStart = new WinForms.Label { AutoSize = true, Text = "Start:", Anchor = WinForms.AnchorStyles.Left, Margin = new WinForms.Padding(0, 6, 6, 6) };
            numStart2 = new WinForms.NumericUpDown { Width = 90, Minimum = 1, Maximum = 9999, Value = 1, Anchor = WinForms.AnchorStyles.Left, Margin = new WinForms.Padding(0, 3, 0, 3) };
            inputs.Controls.Add(lblPrefix, 0, 0); inputs.Controls.Add(txtPrefix2, 1, 0); inputs.Controls.Add(lblStart, 2, 0); inputs.Controls.Add(numStart2, 3, 0);
            leftBlock.Controls.Add(lblHint2, 0, 0); leftBlock.Controls.Add(inputs, 0, 1);

            btnRenameSeq2 = new ModernButton { Text = "Rename" }; btnRenameSeq2.MinimumSize = new Drawing.Size(140, 44); btnRenameSeq2.Click += BtnRenameSeq2_Click;
            controlsRow.Controls.Add(leftBlock, 0, 0); controlsRow.Controls.Add(btnRenameSeq2, 1, 0);
            table.Controls.Add(controlsRow, 0, 2);

            txtLog2 = new WinForms.TextBox { Dock = WinForms.DockStyle.Fill, Multiline = false, ReadOnly = true, BorderStyle = WinForms.BorderStyle.FixedSingle };
            table.Controls.Add(txtLog2, 0, 3);
        }

        private void BtnBrowse2_Click(object? sender, EventArgs e)
        {
            using var fbd = new WinForms.FolderBrowserDialog() { Description = "Select a folder with extras (.mkv files)" };
            if (fbd.ShowDialog(this) == WinForms.DialogResult.OK)
            { selectedFolder2 = fbd.SelectedPath ?? string.Empty; txtFolder2.Text = selectedFolder2; LoadFiles2(); }
        }
        private void LoadFiles2()
        {
            lstFiles2.Items.Clear(); if (string.IsNullOrWhiteSpace(selectedFolder2) || !Directory.Exists(selectedFolder2)) return;
            var files = Directory.GetFiles(selectedFolder2, "*.mkv", SearchOption.TopDirectoryOnly).OrderBy(Path.GetFileName).ToArray();
            foreach (var f in files) lstFiles2.Items.Add(Path.GetFileName(f));
            txtLog2.Text = files.Length == 0 ? "No .mkv files found in this folder." : $"Loaded {files.Length} .mkv file(s).";
        }
        private void BtnRenameSeq2_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedFolder2) || !Directory.Exists(selectedFolder2))
            { WinForms.MessageBox.Show(this, "Please select a valid folder first.", "Folder required", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Warning); return; }
            var items = lstFiles2.SelectedItems.Count > 0 ? lstFiles2.SelectedItems.Cast<object>().Select(o => o!.ToString()).Where(s => !string.IsNullOrEmpty(s)).Cast<string>().ToList() : lstFiles2.Items.Cast<object>().Select(o => o!.ToString()).Where(s => !string.IsNullOrEmpty(s)).Cast<string>().ToList();
            if (items.Count == 0) { WinForms.MessageBox.Show(this, "There are no files to rename.", "Nothing to do", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Information); return; }
            var prefix = txtPrefix2.Text ?? string.Empty; int start = (int)numStart2.Value;
            // Auto pad width: at least 2, or enough digits to cover the last index
            int maxIndex = start + items.Count - 1; int padWidth = Math.Max(2, maxIndex.ToString().Length);
            int idx = start; int renamed = 0;
            try
            {
                foreach (var name in items)
                {
                    var src = Path.Combine(selectedFolder2, name); if (!File.Exists(src)) continue;
                    var ext = Path.GetExtension(src); var number = idx.ToString(new string('0', padWidth));
                    var dest = GetUniquePath(Path.Combine(selectedFolder2, $"{prefix}{number}{ext}"));
                    if (!PathsEqual(src, dest)) { File.Move(src, dest); Log2($"Renamed: '{name}' -> '{Path.GetFileName(dest)}'"); renamed++; }
                    idx++;
                }
                Log2($"Done. {renamed} file(s) renamed."); LoadFiles2();
            }
            catch (Exception ex) { WinForms.MessageBox.Show(this, ex.Message, "Error", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error); Log2("ERROR: " + ex); }
        }
        private void Log2(string msg) => txtLog2.Text = msg;

        // ===== TAB 3 =====
        private void BuildTab3()
        {
            var table = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new WinForms.Padding(14) };
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.AutoSize));
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.AutoSize));
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.AutoSize));
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Absolute, 40));
            tabNewFolder.Controls.Clear(); tabNewFolder.Controls.Add(table);

            var row1 = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Top, ColumnCount = 2, AutoSize = true, Padding = new WinForms.Padding(2) };
            row1.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 100F));
            row1.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            txtParent3 = new WinForms.TextBox { Dock = WinForms.DockStyle.Fill, ReadOnly = true, PlaceholderText = "Choose where to create the new movie folder" };
            btnBrowse3 = new ModernButton { Text = "Browse…" }; btnBrowse3.Click += BtnBrowse3_Click;
            row1.Controls.Add(txtParent3, 0, 0); row1.Controls.Add(btnBrowse3, 1, 0); table.Controls.Add(row1, 0, 0);

            var row2 = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Top, ColumnCount = 2, AutoSize = true, Padding = new WinForms.Padding(0, 8, 0, 0) };
            row2.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            row2.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 100F));
            var lblName = new WinForms.Label { AutoSize = true, Text = "Folder name:", Margin = new WinForms.Padding(6, 6, 6, 6) };
            txtName3 = new WinForms.TextBox { Dock = WinForms.DockStyle.Fill, PlaceholderText = "e.g., Austin Powers Goldmember (2002)" };
            row2.Controls.Add(lblName, 0, 0); row2.Controls.Add(txtName3, 1, 0); table.Controls.Add(row2, 0, 1);

            var row3 = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Top, ColumnCount = 2, AutoSize = true, Padding = new WinForms.Padding(0, 8, 0, 8) };
            row3.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 100F));
            row3.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            chkExtras3 = new WinForms.CheckBox { Text = "Also create Extras subfolder", AutoSize = true, Margin = new WinForms.Padding(6, 6, 6, 6) };
            btnCreate3 = new ModernButton { Text = "Create Folder" }; btnCreate3.MinimumSize = new Drawing.Size(160, 44); btnCreate3.Click += BtnCreate3_Click;
            row3.Controls.Add(chkExtras3, 0, 0); row3.Controls.Add(btnCreate3, 1, 0); table.Controls.Add(row3, 0, 2);

            txtLog3 = new WinForms.TextBox { Dock = WinForms.DockStyle.Fill, Multiline = false, ReadOnly = true, BorderStyle = WinForms.BorderStyle.FixedSingle };
            table.Controls.Add(txtLog3, 0, 3);
        }

        private void BtnBrowse3_Click(object? sender, EventArgs e)
        {
            using var fbd = new WinForms.FolderBrowserDialog() { Description = "Select the parent folder where the new movie folder will be created" };
            if (fbd.ShowDialog(this) == WinForms.DialogResult.OK)
            { selectedParent3 = fbd.SelectedPath ?? string.Empty; txtParent3.Text = selectedParent3; }
        }
        private void BtnCreate3_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedParent3) || !Directory.Exists(selectedParent3))
            { WinForms.MessageBox.Show(this, "Please choose a valid parent folder.", "Parent folder required", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Warning); return; }
            var name = (txtName3.Text ?? string.Empty).Trim(); if (string.IsNullOrWhiteSpace(name)) { WinForms.MessageBox.Show(this, "Please enter a folder name.", "Name required", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Warning); return; }
            try
            {
                var newPath = Path.Combine(selectedParent3, name); if (!Directory.Exists(newPath)) Directory.CreateDirectory(newPath); Log3($"Created: {newPath}");
                if (chkExtras3.Checked) { var extras = Path.Combine(newPath, "Extras"); if (!Directory.Exists(extras)) Directory.CreateDirectory(extras); Log3($"Created: {extras}"); }
                WinForms.MessageBox.Show(this, "Folder created successfully.", "Success", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Information);
            }
            catch (Exception ex) { WinForms.MessageBox.Show(this, ex.Message, "Error", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error); Log3("ERROR: " + ex); }
        }
        private void Log3(string msg) => txtLog3.Text = msg;
    }
}
