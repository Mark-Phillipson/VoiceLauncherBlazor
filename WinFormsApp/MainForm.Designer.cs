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
		private void InitializeComponent()
		{
			blazorWebView1 = new Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView();
			button1 = new Button();
			SuspendLayout();
			// 
			// blazorWebView1
			// 
			blazorWebView1.Dock = DockStyle.Fill;
			blazorWebView1.Location = new Point(0, 0);
			blazorWebView1.Name = "blazorWebView1";
			blazorWebView1.Size = new Size(805, 670);
			blazorWebView1.StartPath = "/";
			blazorWebView1.TabIndex = 0;
			blazorWebView1.Text = "blazorWebView1";
			// 
			// button1
			// 
			button1.Dock = DockStyle.Bottom;
			button1.Location = new Point(0, 647);
			button1.Name = "button1";
			button1.Size = new Size(805, 23);
			button1.TabIndex = 1;
			button1.Text = "&Close Application";
			button1.UseVisualStyleBackColor = true;
			button1.Click += button1_Click;
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(805, 670);
			Controls.Add(button1);
			Controls.Add(blazorWebView1);
			Name = "MainForm";
			Text = "Voice Admin";
			ResumeLayout(false);
		}

		#endregion

		private Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView blazorWebView1;
		private Button button1;
	}
}
