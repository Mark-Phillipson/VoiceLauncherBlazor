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
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            blazorWebView1 = new Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView();
            launchToolbar = new FlowLayoutPanel();

            Button CreateToolButton(string text, string description, string glyph, string accelerator, Action openWindow)
            {
                var icon = new Bitmap(24, 24);
                using (var graphics = Graphics.FromImage(icon))
                using (var font = new Font("Segoe MDL2 Assets", 16F, FontStyle.Regular, GraphicsUnit.Point))
                using (var brush = new SolidBrush(Color.FromArgb(173, 181, 189)))
                {
                    graphics.Clear(Color.Transparent);
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    graphics.DrawString(glyph, font, brush, new PointF(1, 2));
                }

                var buttonFont = new Font("Segoe UI", 9F, FontStyle.Regular);
                var buttonText = text.Replace("&", "");
                var buttonWidth = Math.Max(112, TextRenderer.MeasureText(buttonText, buttonFont).Width + 42);
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
                    ImageAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(3),
                    Size = new Size(buttonWidth, 42),
                    Text = text,
                    TextAlign = ContentAlignment.MiddleCenter,
                    TextImageRelation = TextImageRelation.ImageBeforeText,
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
                CreateToolButton("&Folder Walker", "Search and select folders on a drive", "\uE8B7", "F", ShowFolderWalkerWindow),
                CreateToolButton("&Speech File Search", "Search files by voice or text", "\uE721", "S", ShowSpeechFileSearchWindow),
                CreateToolButton("&Global Search", "Open Global Search", "\uE721", "G", ShowGlobalWindow),
                CreateToolButton("&Display Search", "Open Display Value Search", "\uE721", "D", () => ShowDisplayWindow()),
                CreateToolButton("&Talon Search", "Search Talon voice commands", "\uE721", "T", () => ShowTalonWindow()),
                CreateToolButton("S&nippets", "Open Snippets", "\uE943", "N", () => ShowSnippetsWindow()),
                CreateToolButton("&Launcher", "Open the application launcher", "\uE80A", "L", () => ShowLauncherWindow()),
                CreateToolButton("&Clipboard", "Open Clipboard History", "\uE8C8", "C", ShowClipboardWindow),
                CreateToolButton("C&alculator", "Open Calculator", "\uE8EF", "A", ShowCalculatorWindow),
                CreateToolButton("AI C&hat", "Open AI Chat", "\uE8BD", "H", ShowAIChatWindow)
            });

            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(33, 37, 41);
            ClientSize = new Size(1200, 58);
            Controls.Add(launchToolbar);
            FormBorderStyle = FormBorderStyle.Sizable;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = true;
            Name = "MainForm";
            Text = "Voice Admin";
            launchToolbar.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView blazorWebView1;
        private FlowLayoutPanel launchToolbar;
    }
}
