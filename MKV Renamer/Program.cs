#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
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
            public static readonly Drawing.Color Accent = Drawing.Color.FromArgb(16, 122, 72);
            public static readonly Drawing.Color AccentHover = Drawing.Color.FromArgb(12, 98, 58);
            public static readonly Drawing.Color AccentPressed = Drawing.Color.FromArgb(8, 78, 46);
            public static readonly Drawing.Color Surface = Drawing.Color.FromArgb(232, 242, 238);
            public static readonly Drawing.Color WindowBg = Drawing.Color.FromArgb(242, 247, 245);
            public static readonly Drawing.Color InputBg = Drawing.Color.FromArgb(252, 253, 252);
            public static readonly Drawing.Color Text = Drawing.Color.FromArgb(22, 28, 25);
            public static readonly Drawing.Color TextOnAccent = Drawing.Color.White;
            public static readonly Drawing.Color Border = Drawing.Color.FromArgb(208, 223, 219);
        }

        private sealed class ModernButton : WinForms.Button
        {
            public int CornerRadius { get; set; } = 14;
            private bool _hover, _pressed;
            public bool UseDefaultMargin { get; set; } = true;
            public ModernButton()
            {
                SetStyle(WinForms.ControlStyles.AllPaintingInWmPaint | WinForms.ControlStyles.UserPaint | WinForms.ControlStyles.OptimizedDoubleBuffer, true);
                FlatStyle = WinForms.FlatStyle.Flat;
                FlatAppearance.BorderSize = 0;
                BackColor = Theme.Accent;
                ForeColor = Theme.TextOnAccent;
                Padding = new WinForms.Padding(16, 10, 16, 10);
                Margin = new WinForms.Padding(6);
                Font = new Drawing.Font("Segoe UI Semibold", 10.5F);
                AutoSize = true;
                AutoSizeMode = WinForms.AutoSizeMode.GrowAndShrink;
                MinimumSize = new Drawing.Size(140, 44);
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

        // ===== GLOBAL THEME / LAYOUT NORMALIZER =====
        private void ApplyThemeRecursive(WinForms.Control root)
        {
            root.BackColor = root is WinForms.TextBox or WinForms.ListBox or WinForms.CheckedListBox or WinForms.ListView ? Theme.InputBg : Theme.WindowBg;
            root.ForeColor = Theme.Text;
            if (root is WinForms.TabPage tp) tp.Padding = new WinForms.Padding(14);

            foreach (WinForms.Control c in root.Controls)
            {
                switch (c)
                {
                    case ModernButton mb:
                        mb.FlatStyle = WinForms.FlatStyle.Flat;
                        mb.FlatAppearance.BorderSize = 0;
                        mb.BackColor = Theme.Accent;
                        mb.ForeColor = Theme.TextOnAccent;
                        mb.Font = new Drawing.Font("Segoe UI Semibold", 10.5F);
                        mb.Padding = new WinForms.Padding(16, 10, 16, 10);
                        if (mb.UseDefaultMargin) mb.Margin = new WinForms.Padding(6);
                        break;

                    case WinForms.Button b:
                        b.FlatStyle = WinForms.FlatStyle.Flat;
                        b.FlatAppearance.BorderSize = 0;
                        b.BackColor = Theme.Accent;
                        b.ForeColor = Theme.TextOnAccent;
                        b.Font = new Drawing.Font("Segoe UI Semibold", 10.5F);
                        b.Padding = new WinForms.Padding(14, 10, 14, 10);
                        b.Margin = new WinForms.Padding(6);
                        break;

                    case WinForms.Label:
                        c.BackColor = Theme.WindowBg;
                        c.ForeColor = Theme.Text;
                        c.Margin = new WinForms.Padding(4, 6, 4, 6);
                        break;

                    case WinForms.TextBox or WinForms.ListBox or WinForms.CheckedListBox:
                        c.BackColor = Theme.InputBg;
                        c.ForeColor = Theme.Text;
                        c.Margin = new WinForms.Padding(4, 6, 4, 6);
                        break;

                    case WinForms.NumericUpDown nud:
                        nud.BackColor = Theme.InputBg;
                        nud.ForeColor = Theme.Text;
                        nud.Margin = new WinForms.Padding(4, 6, 4, 6);
                        break;

                    case WinForms.ListView lv:
                        lv.BackColor = Theme.InputBg;
                        lv.ForeColor = Theme.Text;
                        lv.GridLines = false;
                        lv.BorderStyle = WinForms.BorderStyle.FixedSingle;
                        lv.Margin = WinForms.Padding.Empty; // critical: no outer margin
                        break;

                    case WinForms.Panel or WinForms.TableLayoutPanel or WinForms.FlowLayoutPanel:
                        c.BackColor = Theme.Surface;
                        c.Margin = WinForms.Padding.Empty; // containers don't push out
                        break;
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

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var ch in invalid) name = name.Replace(ch, ' ');
            while (name.Contains("  ")) name = name.Replace("  ", " ");
            return name.Trim();
        }

        private static string GuessEpisodeTitle(string fileNameNoExt, string seriesTitle)
        {
            var title = fileNameNoExt;
            if (!string.IsNullOrWhiteSpace(seriesTitle))
            {
                if (title.StartsWith(seriesTitle, StringComparison.OrdinalIgnoreCase))
                    title = title.Substring(seriesTitle.Length).TrimStart('-', '_', '.', ' ', ':');
            }
            title = System.Text.RegularExpressions.Regex.Replace(title, @"(?i)\bS?\d{1,2}\s*[xXeE]\s*\d{1,2}\b", "");
            title = title.Replace('_', ' ').Replace('.', ' ').Replace('-', ' ').Trim();
            if (title.Length > 0) title = char.ToUpper(title[0]) + (title.Length > 1 ? title.Substring(1) : "");
            return string.IsNullOrWhiteSpace(title) ? fileNameNoExt : title;
        }

        // ===== FIELDS =====
        private WinForms.TabControl tabs = null!;
        private WinForms.TabPage tabMain = null!;
        private WinForms.TabPage tabRenamer = null!;
        private WinForms.TabPage tabNewFolder = null!;
        private WinForms.TabPage tabTV = null!;

        // Tab1 (Movies)
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

        // Tab2 (Batch rename)
        private WinForms.TextBox txtFolder2 = null!;
        private ModernButton btnBrowse2 = null!;
        private WinForms.ListBox lstFiles2 = null!;
        private WinForms.Label lblHint2 = null!;
        private WinForms.TextBox txtPrefix2 = null!;
        private WinForms.NumericUpDown numStart2 = null!;
        private ModernButton btnRenameSeq2 = null!;
        private WinForms.TextBox txtLog2 = null!;
        private string selectedFolder2 = string.Empty;

        // Tab3 (New movie folder)
        private WinForms.TextBox txtParent3 = null!;
        private ModernButton btnBrowse3 = null!;
        private WinForms.TextBox txtName3 = null!;
        private WinForms.CheckBox chkExtras3 = null!;
        private ModernButton btnCreate3 = null!;
        private WinForms.TextBox txtLog3 = null!;
        private string selectedParent3 = string.Empty;

        // Tab4 (TV)
        private WinForms.TextBox txtShowFolderTV = null!;
        private ModernButton btnBrowseTV = null!;
        private WinForms.TextBox txtSeriesTitleTV = null!;
        private WinForms.NumericUpDown numSeasonsTV = null!;
        private ModernButton btnCreateSeasonsTV = null!;
        private WinForms.ListView lvTV = null!;
        private WinForms.ComboBox cboSeasonTV = null!;
        private ModernButton btnAssignSeasonTV = null!;
        private ModernButton btnEditTitleTV = null!;
        private ModernButton btnMoveRenameTV = null!;
        private WinForms.TextBox txtLogTV = null!;
        private string selectedShowFolderTV = string.Empty;

        private sealed class EpisodeInfo
        {
            public string FullPath = "";
            public string FileName = "";
            public int? Season = null;
            public int? Episode = null;
            public string EpisodeTitle = "";
            public string Duration = "";
        }

        public MainForm()
        {
            Text = "MKV Organizer";
            StartPosition = WinForms.FormStartPosition.CenterScreen;
            MinimumSize = new Drawing.Size(960, 600);
            ClientSize = new Drawing.Size(1140, 760);
            AutoScaleMode = WinForms.AutoScaleMode.Font;
            Font = new Drawing.Font("Segoe UI", 10.5F);
            BackColor = Theme.WindowBg; ForeColor = Theme.Text; DoubleBuffered = true;

            try { this.Icon = Drawing.Icon.ExtractAssociatedIcon(WinForms.Application.ExecutablePath); } catch { }

            tabs = new WinForms.TabControl { Dock = WinForms.DockStyle.Fill, DrawMode = WinForms.TabDrawMode.OwnerDrawFixed, Padding = new Drawing.Point(22, 8) };
            tabs.DrawItem += Tabs_DrawItem;

            tabMain = new WinForms.TabPage("Main & Extras");
            tabRenamer = new WinForms.TabPage("Rename Extras");
            tabNewFolder = new WinForms.TabPage("New Movie Folder");
            tabTV = new WinForms.TabPage("TV Shows");

            tabs.TabPages.Add(tabMain);
            tabs.TabPages.Add(tabRenamer);
            tabs.TabPages.Add(tabNewFolder);
            tabs.TabPages.Add(tabTV);
            Controls.Add(tabs);

            BuildTab1();
            BuildTab2();
            BuildTab3();
            BuildTabTV();

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

        // ===== TAB 1 ===== (Movies)
        private void BuildTab1()
        {
            var table = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new WinForms.Padding(14), Margin = WinForms.Padding.Empty };
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.AutoSize));
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Percent, 100));
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Absolute, 40));
            tabMain.Controls.Clear(); tabMain.Controls.Add(table);

            var folderRow1 = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Top, ColumnCount = 2, AutoSize = true, Padding = new WinForms.Padding(2), Margin = WinForms.Padding.Empty };
            folderRow1.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 100F));
            folderRow1.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            txtFolder1 = new WinForms.TextBox { Dock = WinForms.DockStyle.Fill, ReadOnly = true, PlaceholderText = "Select a movie folder (e.g., Austin Powers Goldmember (2002))" };
            btnBrowse1 = new ModernButton { Text = "Browse…" }; btnBrowse1.Click += BtnBrowse1_Click;
            folderRow1.Controls.Add(txtFolder1, 0, 0); folderRow1.Controls.Add(btnBrowse1, 1, 0);
            table.Controls.Add(folderRow1, 0, 0);

            split1 = new WinForms.SplitContainer { Dock = WinForms.DockStyle.Fill, Orientation = WinForms.Orientation.Vertical, SplitterWidth = 6 };
            split1.HandleCreated += (s, e) => SafeInitSplit1();
            table.Controls.Add(split1, 0, 1);

            lvFiles1 = new WinForms.ListView { Dock = WinForms.DockStyle.Fill, View = WinForms.View.Details, FullRowSelect = true, HideSelection = false, MultiSelect = false, BorderStyle = WinForms.BorderStyle.FixedSingle, Margin = WinForms.Padding.Empty };
            lvFiles1.Columns.Add("File", 460);
            lvFiles1.Columns.Add("Duration", 120);
            var pnlLeftBottom = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Bottom, ColumnCount = 1, RowCount = 2, AutoSize = true, Padding = new WinForms.Padding(6), Margin = WinForms.Padding.Empty };
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

            var rightTop = new WinForms.FlowLayoutPanel { Dock = WinForms.DockStyle.Top, Height = 46, FlowDirection = WinForms.FlowDirection.LeftToRight, WrapContents = false, Padding = new WinForms.Padding(6, 10, 6, 0), Margin = WinForms.Padding.Empty };
            chkHasExtras1 = new WinForms.CheckBox { Text = "I have extras to add", AutoSize = true };
            chkHasExtras1.CheckedChanged += (s, e) => clbExtras1.Enabled = chkHasExtras1.Checked;
            rightTop.Controls.Add(chkHasExtras1);
            clbExtras1 = new WinForms.CheckedListBox { Dock = WinForms.DockStyle.Fill, Enabled = false, CheckOnClick = true, BorderStyle = WinForms.BorderStyle.FixedSingle, Margin = WinForms.Padding.Empty };
            btnExecute1 = new ModernButton { Text = "Rename / Move" }; btnExecute1.AutoSize = false; btnExecute1.Height = 44; btnExecute1.MinimumSize = new Drawing.Size(0, 44); btnExecute1.Dock = WinForms.DockStyle.Bottom; btnExecute1.Click += BtnExecute1_Click;
            split1.Panel2.Controls.Add(clbExtras1); split1.Panel2.Controls.Add(btnExecute1); split1.Panel2.Controls.Add(rightTop);

            txtLog1 = new WinForms.TextBox { Dock = WinForms.DockStyle.Fill, Multiline = false, ReadOnly = true, BorderStyle = WinForms.BorderStyle.FixedSingle, Margin = WinForms.Padding.Empty };
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

        // ===== TAB 2 ===== (Batch rename extras)
        private void BuildTab2()
        {
            var table = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new WinForms.Padding(14), Margin = WinForms.Padding.Empty };
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.AutoSize));
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Percent, 100));
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.AutoSize));
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Absolute, 40));
            tabRenamer.Controls.Clear(); tabRenamer.Controls.Add(table);

            var folderRow2 = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Top, ColumnCount = 2, AutoSize = true, Padding = new WinForms.Padding(2), Margin = WinForms.Padding.Empty };
            folderRow2.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 100F));
            folderRow2.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            txtFolder2 = new WinForms.TextBox { Dock = WinForms.DockStyle.Fill, ReadOnly = true, PlaceholderText = "Select folder with extras" };
            btnBrowse2 = new ModernButton { Text = "Browse…" }; btnBrowse2.Click += BtnBrowse2_Click;
            folderRow2.Controls.Add(txtFolder2, 0, 0); folderRow2.Controls.Add(btnBrowse2, 1, 0);
            table.Controls.Add(folderRow2, 0, 0);

            lstFiles2 = new WinForms.ListBox { Dock = WinForms.DockStyle.Fill, SelectionMode = WinForms.SelectionMode.MultiExtended, BorderStyle = WinForms.BorderStyle.FixedSingle, Margin = WinForms.Padding.Empty };
            table.Controls.Add(lstFiles2, 0, 1);

            var controlsRow = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Top, ColumnCount = 2, AutoSize = true, Padding = new WinForms.Padding(0, 10, 0, 10), Margin = WinForms.Padding.Empty };
            controlsRow.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 100F));
            controlsRow.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));

            var leftBlock = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Fill, ColumnCount = 1, AutoSize = true, Margin = WinForms.Padding.Empty };
            leftBlock.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 100F));
            lblHint2 = new WinForms.Label { AutoSize = true, Margin = new WinForms.Padding(6, 2, 6, 6), Text = "Tip: Select files to rename only those; none selected = all (.mkv only)." };
            var inputs = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Top, ColumnCount = 4, AutoSize = true, Margin = new WinForms.Padding(6, 2, 6, 6), Padding = WinForms.Padding.Empty };
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

            txtLog2 = new WinForms.TextBox { Dock = WinForms.DockStyle.Fill, Multiline = false, ReadOnly = true, BorderStyle = WinForms.BorderStyle.FixedSingle, Margin = WinForms.Padding.Empty };
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

        // ===== TAB 3 ===== (New movie folder)
        private void BuildTab3()
        {
            var table = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new WinForms.Padding(14), Margin = WinForms.Padding.Empty };
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.AutoSize));
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.AutoSize));
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.AutoSize));
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Absolute, 40));
            tabNewFolder.Controls.Clear(); tabNewFolder.Controls.Add(table);

            var row1 = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Top, ColumnCount = 2, AutoSize = true, Padding = new WinForms.Padding(2), Margin = WinForms.Padding.Empty };
            row1.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 100F));
            row1.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            txtParent3 = new WinForms.TextBox { Dock = WinForms.DockStyle.Fill, ReadOnly = true, PlaceholderText = "Choose where to create the new movie folder" };
            btnBrowse3 = new ModernButton { Text = "Browse…" }; btnBrowse3.Click += BtnBrowse3_Click;
            row1.Controls.Add(txtParent3, 0, 0); row1.Controls.Add(btnBrowse3, 1, 0); table.Controls.Add(row1, 0, 0);

            var row2 = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Top, ColumnCount = 2, AutoSize = true, Padding = new WinForms.Padding(0, 8, 0, 0), Margin = WinForms.Padding.Empty };
            row2.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            row2.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 100F));
            var lblName = new WinForms.Label { AutoSize = true, Text = "Folder name:", Margin = new WinForms.Padding(6, 6, 6, 6) };
            txtName3 = new WinForms.TextBox { Dock = WinForms.DockStyle.Fill, PlaceholderText = "e.g., Austin Powers Goldmember (2002)" };
            row2.Controls.Add(lblName, 0, 0); row2.Controls.Add(txtName3, 1, 0); table.Controls.Add(row2, 0, 1);

            var row3 = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Top, ColumnCount = 2, AutoSize = true, Padding = new WinForms.Padding(0, 8, 0, 8), Margin = WinForms.Padding.Empty };
            row3.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 100F));
            row3.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            chkExtras3 = new WinForms.CheckBox { Text = "Also create Extras subfolder", AutoSize = true, Margin = new WinForms.Padding(6, 6, 6, 6) };
            btnCreate3 = new ModernButton { Text = "Create Folder" }; btnCreate3.MinimumSize = new Drawing.Size(160, 44); btnCreate3.Click += BtnCreate3_Click;
            row3.Controls.Add(chkExtras3, 0, 0); row3.Controls.Add(btnCreate3, 1, 0); table.Controls.Add(row3, 0, 2);

            txtLog3 = new WinForms.TextBox { Dock = WinForms.DockStyle.Fill, Multiline = false, ReadOnly = true, BorderStyle = WinForms.BorderStyle.FixedSingle, Margin = WinForms.Padding.Empty };
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

        // ===== TAB 4 ===== (TV Shows)
        private void BuildTabTV()
        {
            var table = new WinForms.TableLayoutPanel
            {
                Dock = WinForms.DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new WinForms.Padding(14),
                Margin = WinForms.Padding.Empty
            };
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.AutoSize));           // top controls
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Percent, 100F));      // list
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.AutoSize));           // assign row
            table.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.AutoSize));           // bottom (button + log)
            tabTV.Controls.Clear();
            tabTV.Controls.Add(table);

            // ---------- TOP BAR ----------
            var topBarOuter = new WinForms.Panel
            {
                Dock = WinForms.DockStyle.Fill,
                Padding = new WinForms.Padding(12, 12, 12, 8), // a little more breathing room
                Margin = WinForms.Padding.Empty
            };
            table.Controls.Add(topBarOuter, 0, 0);

            var topBar = new WinForms.TableLayoutPanel
            {
                Dock = WinForms.DockStyle.Fill,
                ColumnCount = 3,
                AutoSize = false,
                Padding = new WinForms.Padding(6),
                Margin = WinForms.Padding.Empty
            };
            topBar.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 45F));
            topBar.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 35F));
            topBar.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            topBarOuter.Controls.Add(topBar);

            WinForms.Panel Card() => new WinForms.Panel
            {
                BackColor = Theme.Surface,
                Dock = WinForms.DockStyle.Fill,
                Padding = new WinForms.Padding(12),
                Margin = new WinForms.Padding(6, 0, 6, 0)
            };

            // Folder card
            var cardFolder = Card();
            var folderRow = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Fill, ColumnCount = 2, AutoSize = true, Padding = new WinForms.Padding(0), Margin = WinForms.Padding.Empty };
            folderRow.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 100F));
            folderRow.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            txtShowFolderTV = new WinForms.TextBox { Dock = WinForms.DockStyle.Fill, ReadOnly = true, PlaceholderText = "Select the TV show folder (top-level)" };
            btnBrowseTV = new ModernButton { Text = "Browse…" };
            btnBrowseTV.Click += BtnBrowseTV_Click;
            folderRow.Controls.Add(txtShowFolderTV, 0, 0);
            folderRow.Controls.Add(btnBrowseTV, 1, 0);
            cardFolder.Controls.Add(folderRow);
            topBar.Controls.Add(cardFolder, 0, 0);

            // Series Title card
            var cardSeries = Card();
            var seriesRow = new WinForms.TableLayoutPanel { Dock = WinForms.DockStyle.Fill, ColumnCount = 2, AutoSize = true, Padding = new WinForms.Padding(0), Margin = WinForms.Padding.Empty };
            seriesRow.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            seriesRow.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 100F));
            var lblSeries = new WinForms.Label { AutoSize = true, Text = "Series Title:", Margin = new WinForms.Padding(0, 6, 10, 6) };
            txtSeriesTitleTV = new WinForms.TextBox { Dock = WinForms.DockStyle.Fill, PlaceholderText = "e.g., The Office (US)" };
            seriesRow.Controls.Add(lblSeries, 0, 0);
            seriesRow.Controls.Add(txtSeriesTitleTV, 1, 0);
            cardSeries.Controls.Add(seriesRow);
            topBar.Controls.Add(cardSeries, 1, 0);

            // Seasons card (FIXED LAYOUT)
            var cardSeasons = Card();
            var seasonsRow = new WinForms.TableLayoutPanel
            {
                Dock = WinForms.DockStyle.Fill,
                ColumnCount = 3,
                AutoSize = false,
                Padding = new WinForms.Padding(0),
                Margin = WinForms.Padding.Empty
            };
            seasonsRow.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));          // label
            seasonsRow.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));          // numeric up/down
            seasonsRow.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));          // button keeps natural width
            var lblSeasons = new WinForms.Label { AutoSize = true, Text = "Seasons:", Margin = new WinForms.Padding(0, 6, 8, 6) };
            numSeasonsTV = new WinForms.NumericUpDown { Minimum = 1, Maximum = 99, Value = 1, Width = 72, Margin = new WinForms.Padding(0, 2, 10, 2) };
            btnCreateSeasonsTV = new ModernButton { Text = "Create Season Folders" };
            btnCreateSeasonsTV.UseDefaultMargin = false;
            // important: keep the button inside its cell
            btnCreateSeasonsTV.AutoSize = false;
            btnCreateSeasonsTV.MinimumSize = new Drawing.Size(220, 44);
            btnCreateSeasonsTV.Height = 44;
            btnCreateSeasonsTV.Dock = WinForms.DockStyle.Fill;
            btnCreateSeasonsTV.Margin = new WinForms.Padding(0, 0, 0, 0);
            btnCreateSeasonsTV.Click += BtnCreateSeasonsTV_Click;

            seasonsRow.Controls.Add(lblSeasons, 0, 0);
            seasonsRow.Controls.Add(numSeasonsTV, 1, 0);
            seasonsRow.Controls.Add(btnCreateSeasonsTV, 2, 0);
            cardSeasons.Controls.Add(seasonsRow);
            topBar.Controls.Add(cardSeasons, 2, 0);

            // ---------- LIST (episodes) ----------
            lvTV = new WinForms.ListView
            {
                Dock = WinForms.DockStyle.Fill,
                View = WinForms.View.Details,
                FullRowSelect = true,
                HideSelection = false,
                MultiSelect = true,
                BorderStyle = WinForms.BorderStyle.FixedSingle,
                Margin = new WinForms.Padding(12, 0, 12, 0)
            };
            lvTV.Columns.Add("File", 360);
            lvTV.Columns.Add("Season", 80);
            lvTV.Columns.Add("Ep #", 60);
            lvTV.Columns.Add("Episode Title", 360);
            lvTV.Columns.Add("Duration", 110);
            lvTV.Resize += (s, e) => AutoSizeColumnsTV();
            table.Controls.Add(lvTV, 0, 1);

            // ---------- ASSIGN ROW ----------
            var assignRow = new WinForms.TableLayoutPanel
            {
                Dock = WinForms.DockStyle.Fill,
                ColumnCount = 4,
                AutoSize = false,
                Padding = new WinForms.Padding(12, 8, 12, 8),
                Margin = WinForms.Padding.Empty
            };
            assignRow.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            assignRow.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            assignRow.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            assignRow.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 100F));

            var lblAssign = new WinForms.Label { AutoSize = true, Text = "Assign to season:", Margin = new WinForms.Padding(0, 10, 10, 6) };
            cboSeasonTV = new WinForms.ComboBox { DropDownStyle = WinForms.ComboBoxStyle.DropDownList, Width = 140, Margin = new WinForms.Padding(0, 6, 10, 6) };
            cboSeasonTV.Items.AddRange(Enumerable.Range(1, 99).Select(i => (object)$"Season {i}").ToArray());
            cboSeasonTV.SelectedIndex = 0;

            btnAssignSeasonTV = new ModernButton { Text = "Set Season for Selected" };
            btnAssignSeasonTV.UseDefaultMargin = false;
            btnAssignSeasonTV.Margin = new WinForms.Padding(6, 2, 6, 2);
            btnAssignSeasonTV.Click += BtnAssignSeasonTV_Click;

            btnEditTitleTV = new ModernButton { Text = "Edit Episode Title…" };
            btnEditTitleTV.UseDefaultMargin = false;
            btnEditTitleTV.Margin = new WinForms.Padding(6, 2, 0, 2);
            btnEditTitleTV.Click += BtnEditTitleTV_Click;

            assignRow.Controls.Add(lblAssign, 0, 0);
            assignRow.Controls.Add(cboSeasonTV, 1, 0);
            assignRow.Controls.Add(btnAssignSeasonTV, 2, 0);
            assignRow.Controls.Add(btnEditTitleTV, 3, 0);
            table.Controls.Add(assignRow, 0, 2);

            // ---------- BOTTOM ROW (button + slim log) ----------
            var bottomRow = new WinForms.TableLayoutPanel
            {
                Dock = WinForms.DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new WinForms.Padding(12, 0, 12, 8),
                Margin = WinForms.Padding.Empty
            };
            bottomRow.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.AutoSize));
            bottomRow.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 100F));

            btnMoveRenameTV = new ModernButton { Text = "Move & Rename (Seasons)" };
            btnMoveRenameTV.UseDefaultMargin = false;
            btnMoveRenameTV.Margin = new WinForms.Padding(0, 0, 10, 0);
            btnMoveRenameTV.AutoSize = false;
            btnMoveRenameTV.MinimumSize = new Drawing.Size(220, 44);
            btnMoveRenameTV.Height = 44;
            btnMoveRenameTV.Click += BtnMoveRenameTV_Click;

            txtLogTV = new WinForms.TextBox
            {
                Dock = WinForms.DockStyle.Fill,
                Multiline = false,
                ReadOnly = true,
                BorderStyle = WinForms.BorderStyle.FixedSingle,
                Margin = WinForms.Padding.Empty,
                MinimumSize = new Drawing.Size(0, 28),
                MaximumSize = new Drawing.Size(int.MaxValue, 28)
            };

            bottomRow.Controls.Add(btnMoveRenameTV, 0, 0);
            bottomRow.Controls.Add(txtLogTV, 1, 0);
            table.Controls.Add(bottomRow, 0, 3);

            AutoSizeColumnsTV();
        }

        // ===== TV HELPERS / HANDLERS =====
        private void AutoSizeColumnsTV()
        {
            if (lvTV == null || lvTV.Columns.Count < 5) return;
            int w = lvTV.ClientSize.Width - WinForms.SystemInformation.VerticalScrollBarWidth;
            if (w <= 0) return;

            // File 40%, Season 8%, Ep# 8%, Title 34%, Duration 10%
            int fileW = (int)(w * 0.40);
            int seasonW = (int)(w * 0.08);
            int epW = (int)(w * 0.08);
            int titleW = (int)(w * 0.34);
            int durW = (int)(w * 0.10);

            int sum = fileW + seasonW + epW + titleW + durW;
            titleW += (w - sum); // fix rounding
            lvTV.Columns[0].Width = Math.Max(140, fileW);
            lvTV.Columns[1].Width = Math.Max(70, seasonW);
            lvTV.Columns[2].Width = Math.Max(60, epW);
            lvTV.Columns[3].Width = Math.Max(160, titleW);
            lvTV.Columns[4].Width = Math.Max(90, durW);
        }

        private void BtnBrowseTV_Click(object? sender, EventArgs e)
        {
            using var fbd = new WinForms.FolderBrowserDialog { Description = "Select the TV show folder (top-level)" };
            if (fbd.ShowDialog(this) == WinForms.DialogResult.OK)
            {
                selectedShowFolderTV = fbd.SelectedPath ?? string.Empty;
                txtShowFolderTV.Text = selectedShowFolderTV;
                LoadTVFiles();
            }
        }

        private void LoadTVFiles()
        {
            lvTV.BeginUpdate();
            try
            {
                lvTV.Items.Clear();
                if (string.IsNullOrWhiteSpace(selectedShowFolderTV) || !Directory.Exists(selectedShowFolderTV))
                {
                    txtLogTV.Text = "Pick a show folder.";
                    return;
                }

                // Only top-level MKVs (move into Season folders via the UI)
                var files = Directory.GetFiles(selectedShowFolderTV, "*.mkv", SearchOption.TopDirectoryOnly)
                                     .OrderBy(Path.GetFileName)
                                     .ToArray();

                foreach (var f in files)
                {
                    var epi = new EpisodeInfo
                    {
                        FullPath = f,
                        FileName = Path.GetFileName(f),
                        Season = null,
                        Episode = null,
                        EpisodeTitle = GuessEpisodeTitle(Path.GetFileNameWithoutExtension(f), txtSeriesTitleTV.Text ?? string.Empty),
                        Duration = GetDurationStringSafe(f)
                    };
                    AddEpisodeToList(epi);
                }
                txtLogTV.Text = files.Length == 0 ? "No .mkv files at the top level." : $"Loaded {files.Length} file(s).";
                AutoSizeColumnsTV();
            }
            finally { lvTV.EndUpdate(); }
        }

        private void AddEpisodeToList(EpisodeInfo epi)
        {
            var li = new WinForms.ListViewItem(epi.FileName);
            li.SubItems.Add(epi.Season?.ToString() ?? "");
            li.SubItems.Add(epi.Episode?.ToString() ?? "");
            li.SubItems.Add(epi.EpisodeTitle);
            li.SubItems.Add(epi.Duration);
            li.Tag = epi;
            lvTV.Items.Add(li);
        }

        private int ParseSeasonFromCombo()
        {
            if (cboSeasonTV.SelectedItem is string s)
            {
                var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && int.TryParse(parts[1], out int n)) return n;
            }
            return 1;
        }

        private void BtnCreateSeasonsTV_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedShowFolderTV) || !Directory.Exists(selectedShowFolderTV))
            {
                WinForms.MessageBox.Show(this, "Choose a TV show folder first.", "Folder required",
                    WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Warning);
                return;
            }
            int count = (int)numSeasonsTV.Value;
            int made = 0;
            try
            {
                for (int i = 1; i <= count; i++)
                {
                    var p = Path.Combine(selectedShowFolderTV, $"Season {i}");
                    if (!Directory.Exists(p)) { Directory.CreateDirectory(p); made++; }
                }
                txtLogTV.Text = made == 0 ? "All season folders already exist." : $"Created {made} season folder(s).";
            }
            catch (Exception ex)
            {
                WinForms.MessageBox.Show(this, ex.Message, "Error", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error);
                txtLogTV.Text = "ERROR: " + ex.Message;
            }
        }

        private void BtnAssignSeasonTV_Click(object? sender, EventArgs e)
        {
            if (lvTV.SelectedItems.Count == 0)
            {
                WinForms.MessageBox.Show(this, "Select episode(s) in the list first.", "Nothing selected",
                    WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Information);
                return;
            }
            int season = ParseSeasonFromCombo();

            // Find current max episode number for that season to continue numbering
            int currentMax = 0;
            foreach (WinForms.ListViewItem it in lvTV.Items)
            {
                if (it.Tag is EpisodeInfo ep && ep.Season == season && ep.Episode.HasValue)
                    currentMax = Math.Max(currentMax, ep.Episode.Value);
            }

            int assigned = 0;
            foreach (WinForms.ListViewItem it in lvTV.SelectedItems)
            {
                if (it.Tag is not EpisodeInfo epi) continue;
                epi.Season = season;
                epi.Episode ??= ++currentMax; // preserve if already set; otherwise assign next
                it.SubItems[1].Text = season.ToString();
                it.SubItems[2].Text = epi.Episode?.ToString() ?? "";
                assigned++;
            }
            txtLogTV.Text = assigned == 0 ? "No episodes updated." : $"Assigned Season {season} to {assigned} episode(s).";
        }

        private void BtnEditTitleTV_Click(object? sender, EventArgs e)
        {
            if (lvTV.SelectedItems.Count != 1)
            {
                WinForms.MessageBox.Show(this, "Select a single episode to edit its title.", "Select one",
                    WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Information);
                return;
            }
            var li = lvTV.SelectedItems[0];
            if (li.Tag is not EpisodeInfo epi) return;

            var dlg = new InputDialog("Episode Title", "Enter a title for this episode:", epi.EpisodeTitle);
            if (dlg.ShowDialog(this) == WinForms.DialogResult.OK)
            {
                epi.EpisodeTitle = SanitizeFileName(dlg.Value ?? "");
                li.SubItems[3].Text = epi.EpisodeTitle;
                txtLogTV.Text = $"Updated title: {epi.EpisodeTitle}";
            }
        }

        private void BtnMoveRenameTV_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedShowFolderTV) || !Directory.Exists(selectedShowFolderTV))
            {
                WinForms.MessageBox.Show(this, "Choose a TV show folder first.", "Folder required",
                    WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Warning);
                return;
            }
            var seriesTitle = (txtSeriesTitleTV.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(seriesTitle))
            {
                WinForms.MessageBox.Show(this, "Enter a Series Title (used for renaming).", "Series title required",
                    WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Warning);
                return;
            }

            // Build next-episode counters per season  
            var nextForSeason = new Dictionary<int, int>();
            foreach (WinForms.ListViewItem item in lvTV.Items)
            {
                if (item.Tag is EpisodeInfo episode && episode.Season.HasValue && episode.Episode.HasValue)
                {
                    int season = episode.Season.Value;
                    if (!nextForSeason.ContainsKey(season)) nextForSeason[season] = 0;
                    nextForSeason[season] = Math.Max(nextForSeason[season], episode.Episode.Value);
                }
            }

            int moved = 0;
            try
            {
                foreach (WinForms.ListViewItem item in lvTV.Items)
                {
                    if (item.Tag is not EpisodeInfo episode) continue;
                    if (!episode.Season.HasValue) continue; // only move items assigned to a season  

                    int season = episode.Season.Value;
                    int epNum = episode.Episode ?? (nextForSeason.TryGetValue(season, out var cur) ? cur + 1 : 1);
                    nextForSeason[season] = epNum;

                    var fileExt = Path.GetExtension(episode.FullPath);
                    var title = string.IsNullOrWhiteSpace(episode.EpisodeTitle)
                                ? GuessEpisodeTitle(Path.GetFileNameWithoutExtension(episode.FileName), seriesTitle)
                                : episode.EpisodeTitle;

                    string seasonFolder = Path.Combine(selectedShowFolderTV, $"Season {season}");
                    Directory.CreateDirectory(seasonFolder);

                    string newName = $"{seriesTitle} - {season:00}X{epNum:00} - {title}{fileExt}";
                    newName = SanitizeFileName(newName);
                    string destPath = GetUniquePath(Path.Combine(seasonFolder, newName));

                    if (!PathsEqual(episode.FullPath, destPath))
                    {
                        File.Move(episode.FullPath, destPath);
                        moved++;

                        // Update model + row  
                        episode.FullPath = destPath;
                        episode.FileName = Path.GetFileName(destPath);
                        episode.Episode = epNum;
                        episode.EpisodeTitle = title;

                        item.Text = episode.FileName;
                        item.SubItems[1].Text = season.ToString();
                        item.SubItems[2].Text = epNum.ToString();
                        item.SubItems[3].Text = episode.EpisodeTitle;
                    }
                }
                txtLogTV.Text = moved == 0 ? "Nothing moved/renamed." : $"Done. {moved} file(s) moved and renamed.";
                WinForms.MessageBox.Show(this, "Season move + rename complete.", "Success",
                    WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Information);

                // Refresh top-level list (likely empty now since moved into season folders)  
                LoadTVFiles();
            }
            catch (Exception ex)
            {
                WinForms.MessageBox.Show(this, ex.Message, "Error", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error);
                txtLogTV.Text = "ERROR: " + ex.Message;
            }
        }

        // --- Tiny input dialog for episode title edits ---
        private sealed class InputDialog : WinForms.Form
        {
            private readonly WinForms.TextBox _txt = new WinForms.TextBox { Dock = WinForms.DockStyle.Top };
            private readonly ModernButton _ok = new ModernButton { Text = "OK" };
            private readonly ModernButton _cancel = new ModernButton { Text = "Cancel" };
            public string? Value => _txt.Text;

            public InputDialog(string title, string prompt, string initial)
            {
                Text = title;
                StartPosition = WinForms.FormStartPosition.CenterParent;
                FormBorderStyle = WinForms.FormBorderStyle.FixedDialog;
                MinimizeBox = MaximizeBox = false;
                ClientSize = new Drawing.Size(480, 160);

                var lbl = new WinForms.Label { Text = prompt, Dock = WinForms.DockStyle.Top, AutoSize = false, Height = 40, Padding = new WinForms.Padding(10, 12, 10, 4) };
                _txt.Text = initial;
                _txt.Margin = new WinForms.Padding(10);
                _txt.Width = 440;

                var buttons = new WinForms.FlowLayoutPanel { Dock = WinForms.DockStyle.Bottom, Height = 60, FlowDirection = WinForms.FlowDirection.RightToLeft, Padding = new WinForms.Padding(10), Margin = WinForms.Padding.Empty };
                _ok.Click += (s, e) => { DialogResult = WinForms.DialogResult.OK; Close(); };
                _cancel.Click += (s, e) => { DialogResult = WinForms.DialogResult.Cancel; Close(); };
                buttons.Controls.Add(_ok);
                buttons.Controls.Add(_cancel);

                Controls.Add(buttons);
                Controls.Add(_txt);
                Controls.Add(lbl);

                BackColor = Theme.WindowBg;
                ForeColor = Theme.Text;
            }
        }
    }
}
