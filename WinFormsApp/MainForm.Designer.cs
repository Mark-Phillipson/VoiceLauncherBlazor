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
