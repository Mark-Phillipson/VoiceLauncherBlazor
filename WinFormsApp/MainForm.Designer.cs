namespace WinFormsApp
{
	partial class MainForm
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			blazorWebView1 = new Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView();
			toolStripMain = new ToolStrip();
			folderWalkerToolStripButton = new ToolStripButton();
			speechFileSearchToolStripButton = new ToolStripButton();
			SuspendLayout();
			// 
			// blazorWebView1
			// 
			blazorWebView1.Dock = DockStyle.Fill;
			blazorWebView1.Location = new Point(0, 0);
			blazorWebView1.Name = "blazorWebView1";
			blazorWebView1.Size = new Size(855, 670);
			blazorWebView1.StartPath = "/";
			blazorWebView1.TabIndex = 0;
			blazorWebView1.Text = "blazorWebView1";
			//
			// toolStripMain
			//
			toolStripMain.Dock = DockStyle.Top;
			toolStripMain.GripStyle = ToolStripGripStyle.Hidden;
			toolStripMain.ImageScalingSize = new Size(20, 20);
			toolStripMain.Items.AddRange(new ToolStripItem[] { folderWalkerToolStripButton, speechFileSearchToolStripButton });
			toolStripMain.Location = new Point(0, 0);
			toolStripMain.Name = "toolStripMain";
			toolStripMain.Padding = new Padding(8, 4, 8, 4);
			toolStripMain.Size = new Size(855, 37);
			toolStripMain.TabIndex = 1;
			toolStripMain.Text = "Main toolbar";
			//
			// folderWalkerToolStripButton
			//
			folderWalkerToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
			folderWalkerToolStripButton.Name = "folderWalkerToolStripButton";
			folderWalkerToolStripButton.Size = new Size(110, 26);
			folderWalkerToolStripButton.Text = "Folder Walker";
			folderWalkerToolStripButton.ToolTipText = "Search for and select a folder";
			folderWalkerToolStripButton.Click += (sender, e) => ShowFolderWalkerWindow();
			//
			// speechFileSearchToolStripButton
			//
			speechFileSearchToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
			speechFileSearchToolStripButton.Name = "speechFileSearchToolStripButton";
			speechFileSearchToolStripButton.Size = new Size(150, 26);
			speechFileSearchToolStripButton.Text = "Speech File Search";
			speechFileSearchToolStripButton.ToolTipText = "Search files by voice or text";
			speechFileSearchToolStripButton.Click += (sender, e) => ShowSpeechFileSearchWindow();
			//			// MainForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(855, 670);
			Controls.Add(blazorWebView1);
			Controls.Add(toolStripMain);
			FormBorderStyle = FormBorderStyle.Sizable;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = true;
			MinimizeBox = true;
			Name = "MainForm";
			Text = "Voice Admin";
			ResumeLayout(false);
		}

		#endregion

		private Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView blazorWebView1;
		private ToolStrip toolStripMain;
		private ToolStripButton folderWalkerToolStripButton;
		private ToolStripButton speechFileSearchToolStripButton;
	}
}
