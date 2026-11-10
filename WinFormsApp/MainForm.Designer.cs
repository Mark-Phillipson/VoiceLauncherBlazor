namespace WinFormsApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                blazorWebView1?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            blazorWebView1 = new Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView();
            launchToolbar = new FlowLayoutPanel();

            Bitmap CreateToolIcon(string iconKind)
            {
                var icon = new Bitmap(28, 28);
                using var graphics = Graphics.FromImage(icon);
                using var pen = new Pen(Color.FromArgb(173, 181, 189), 1.8F)
                {
                    StartCap = System.Drawing.Drawing2D.LineCap.Round,
                    EndCap = System.Drawing.Drawing2D.LineCap.Round
                };
                using var fill = new SolidBrush(Color.FromArgb(173, 181, 189));
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                void DrawLines(params Point[] points) => graphics.DrawLines(pen, points);
                void DrawLine(int x1, int y1, int x2, int y2) => graphics.DrawLine(pen, x1, y1, x2, y2);

                switch (iconKind)
                {
                    case "folder":
                        DrawLines(new Point(3, 7), new Point(10, 7), new Point(12, 10), new Point(25, 10), new Point(23, 21), new Point(3, 21), new Point(3, 7));
                        DrawLine(3, 10, 25, 10);
                        break;
                    case "speech-search":
                        DrawLines(new Point(4, 3), new Point(17, 3), new Point(20, 6), new Point(20, 12));
                        DrawLines(new Point(4, 3), new Point(4, 24), new Point(12, 24));
                        DrawLine(17, 3, 17, 7);
                        DrawLine(17, 7, 20, 7);
                        graphics.DrawEllipse(pen, 12, 12, 10, 10);
                        DrawLine(20, 20, 25, 25);
                        break;
                    case "global-search":
                        graphics.DrawEllipse(pen, 2, 2, 18, 18);
                        graphics.DrawArc(pen, 7, 2, 8, 18, 90, 180);
                        graphics.DrawArc(pen, 7, 2, 8, 18, 270, 180);
                        DrawLine(3, 11, 19, 11);
                        graphics.DrawEllipse(pen, 17, 17, 7, 7);
                        DrawLine(22, 22, 26, 26);
                        break;
                    case "display":
                        graphics.DrawRectangle(pen, 2, 4, 20, 14);
                        DrawLine(12, 18, 12, 23);
                        DrawLine(7, 24, 17, 24);
                        DrawLine(6, 8, 12, 8);
                        DrawLine(6, 12, 18, 12);
                        break;
                    case "talon":
                        graphics.DrawEllipse(pen, 10, 2, 8, 15);
                        graphics.DrawArc(pen, 5, 6, 18, 16, 0, 180);
                        DrawLine(14, 22, 14, 25);
                        DrawLine(10, 25, 18, 25);
                        break;
                    case "snippets":
                        using (var iconFont = new Font("Segoe UI", 17F, FontStyle.Bold, GraphicsUnit.Pixel))
                            graphics.DrawString("{ }", iconFont, fill, new PointF(1, 2));
                        break;
                    case "launcher":
                        graphics.DrawRectangle(pen, 3, 3, 8, 8);
                        graphics.DrawRectangle(pen, 16, 3, 8, 8);
                        graphics.DrawRectangle(pen, 3, 16, 8, 8);
                        graphics.DrawRectangle(pen, 16, 16, 8, 8);
                        break;
                    case "clipboard":
                        graphics.DrawRectangle(pen, 5, 5, 18, 20);
                        graphics.DrawRectangle(pen, 10, 2, 8, 6);
                        DrawLine(9, 11, 19, 11);
                        DrawLine(9, 15, 19, 15);
                        DrawLine(9, 19, 16, 19);
                        break;
                    case "calculator":
                        graphics.DrawRectangle(pen, 5, 2, 18, 24);
                        graphics.DrawRectangle(pen, 8, 5, 12, 5);
                        for (var row = 0; row < 3; row++)
                        for (var column = 0; column < 2; column++)
                            graphics.FillRectangle(fill, 8 + column * 7, 13 + row * 4, 3, 2);
                        break;
                    case "ai-chat":
                        DrawLines(new Point(3, 4), new Point(24, 4), new Point(24, 19), new Point(12, 19), new Point(6, 25), new Point(6, 19), new Point(3, 19), new Point(3, 4));
                        DrawLine(8, 10, 19, 10);
                        DrawLine(8, 14, 16, 14);
                        break;
                }

                return icon;
            }

            Button CreateToolButton(string text, string description, string iconKind, string accelerator, Action openWindow)
            {
                var icon = CreateToolIcon(iconKind);
                var buttonFont = new Font("Segoe UI", 9F, FontStyle.Regular);
                var buttonText = text.Replace("&", "");
                var buttonWidth = Math.Max(112, TextRenderer.MeasureText(buttonText, buttonFont).Width + 16);
                var button = new Button
                {
                    AccessibleName = buttonText,
                    AccessibleDescription = description + ". Keyboard shortcut: Alt+" + accelerator + ".",
                    AutoSize = false,
                    BackColor = Color.FromArgb(33, 37, 41),
                    FlatStyle = FlatStyle.Flat,
                    Font = buttonFont,
                    ForeColor = Color.FromArgb(173, 181, 189),
                    Image = icon,
                    ImageAlign = ContentAlignment.TopCenter,
                    Margin = new Padding(3),
                    Padding = new Padding(4, 4, 4, 4),
                    Size = new Size(buttonWidth, 72),
                    Text = text,
                    TextAlign = ContentAlignment.BottomCenter,
                    TextImageRelation = TextImageRelation.ImageAboveText,
                    UseMnemonic = true,
                    UseVisualStyleBackColor = false
                };
                button.FlatAppearance.BorderColor = Color.FromArgb(108, 117, 125);
                button.FlatAppearance.BorderSize = 1;
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(52, 58, 64);
                button.FlatAppearance.MouseDownBackColor = Color.FromArgb(73, 80, 87);
                button.Click += (_, _) => openWindow();
                button.Disposed += (_, _) =>
                {
                    icon.Dispose();
                    buttonFont.Dispose();
                };
                return button;
            }

            SuspendLayout();
            launchToolbar.SuspendLayout();

            // Keep the Blazor host for the existing startup services, but do not
            // add it to the form. The main form is now only the tool launcher.
            blazorWebView1.Name = "blazorWebView1";
            blazorWebView1.StartPath = "/";
            blazorWebView1.TabIndex = 0;
            blazorWebView1.Text = "blazorWebView1";

            launchToolbar.AutoSize = true;
            launchToolbar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            launchToolbar.BackColor = Color.FromArgb(33, 37, 41);
            launchToolbar.Dock = DockStyle.Fill;
            launchToolbar.FlowDirection = FlowDirection.LeftToRight;
            launchToolbar.Location = new Point(0, 0);
            launchToolbar.Name = "launchToolbar";
            launchToolbar.Padding = new Padding(6);
            launchToolbar.TabIndex = 1;
            launchToolbar.WrapContents = true;
            launchToolbar.Controls.AddRange(new Control[]
            {
                CreateToolButton("&Folder Walker", "Search and select folders on a drive", "folder", "F", ShowFolderWalkerWindow),
                CreateToolButton("&Speech File Search", "Search files by voice or text", "speech-search", "S", ShowSpeechFileSearchWindow),
                CreateToolButton("&Global Search", "Open Global Search", "global-search", "G", ShowGlobalWindow),
                CreateToolButton("&Display Search", "Open Display Value Search", "display", "D", () => ShowDisplayWindow()),
                CreateToolButton("&Talon Search", "Search Talon voice commands", "talon", "T", () => ShowTalonWindow()),
                CreateToolButton("S&nippets", "Open Snippets", "snippets", "N", () => ShowSnippetsWindow()),
                CreateToolButton("&Launcher", "Open the application launcher", "launcher", "L", () => ShowLauncherWindow()),
                CreateToolButton("&Clipboard", "Open Clipboard History", "clipboard", "C", ShowClipboardWindow),
                CreateToolButton("C&alculator", "Open Calculator", "calculator", "A", ShowCalculatorWindow),
                CreateToolButton("AI C&hat", "Open AI Chat", "ai-chat", "H", ShowAIChatWindow)
            });