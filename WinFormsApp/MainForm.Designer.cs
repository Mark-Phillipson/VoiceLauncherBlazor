namespace WinFormsApp
{
	partial class MainForm
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			blazorWebView1 = new Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView();
			launchToolbar = new FlowLayoutPanel();

			Button CreateToolButton(string text, string description, Action openWindow)
			{
				var button = new Button
				{
					AccessibleName = text,
					AccessibleDescription = description,
					AutoSize = false,
					Font = new Font("Segoe UI", 9F, FontStyle.Regular),
					Margin = new Padding(4),
					Size = new Size(148, 48),
					Text = text,
					UseVisualStyleBackColor = true
				};
				button.Click += (_, _) => openWindow();
				return button;
			}

			SuspendLayout();
			launchToolbar.SuspendLayout();

			// Main hybrid WebView
			blazorWebView1.Dock = DockStyle.Fill;
			blazorWebView1.Location = new Point(0, 0);
			blazorWebView1.Name = "blazorWebView1";
			blazorWebView1.Size = new Size(855, 630);
			blazorWebView1.StartPath = "/";
			blazorWebView1.TabIndex = 0;
			blazorWebView1.Text = "blazorWebView1";

			// Eye-tracker friendly tool launch buttons. The flow layout wraps them
			// into additional rows when the main window is narrower.
			launchToolbar.AutoSize = true;
			launchToolbar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			launchToolbar.BackColor = SystemColors.Control;
			launchToolbar.Dock = DockStyle.Top;
			launchToolbar.FlowDirection = FlowDirection.LeftToRight;
			launchToolbar.Location = new Point(0, 0);
			launchToolbar.Name = "launchToolbar";
			launchToolbar.Padding = new Padding(8);
			launchToolbar.TabIndex = 1;
			launchToolbar.WrapContents = true;
			launchToolbar.Controls.AddRange(new Control[]
			{
				CreateToolButton("Folder Walker", "Search and select folders on a drive", ShowFolderWalkerWindow),
				CreateToolButton("Speech File Search", "Search files by voice or text", ShowSpeechFileSearchWindow),
				CreateToolButton("Global Search", "Open Global Search", ShowGlobalWindow),
				CreateToolButton("Display Search", "Open Display Value Search", () => ShowDisplayWindow()),
				CreateToolButton("Talon Search", "Search Talon voice commands", () => ShowTalonWindow()),
				CreateToolButton("Snippets", "Open Snippets", () => ShowSnippetsWindow()),
				CreateToolButton("Launcher", "Open the application launcher", () => ShowLauncherWindow()),
				CreateToolButton("Clipboard History", "Open Clipboard History", ShowClipboardWindow),
				CreateToolButton("Calculator", "Open Calculator", ShowCalculatorWindow),
				CreateToolButton("AI Chat", "Open AI Chat", ShowAIChatWindow)
			});

			// MainForm
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(855, 670);
			Controls.Add(blazorWebView1);
			Controls.Add(launchToolbar);
			FormBorderStyle = FormBorderStyle.Sizable;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = true;
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
