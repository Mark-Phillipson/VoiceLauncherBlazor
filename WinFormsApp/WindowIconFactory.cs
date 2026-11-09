using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace WinFormsApp;

/// <summary>
/// Creates distinct small icons for the independent tool windows.
/// </summary>
[SupportedOSPlatform("windows")]
internal static class WindowIconFactory
{
    public static Icon CreateChat() => Create(Color.FromArgb(45, 130, 210), (g, p) =>
    {
        g.DrawRoundedRectangle(p, new Rectangle(4, 5, 24, 18), 4);
        g.DrawLines(p, new[] { new Point(10, 23), new Point(8, 28), new Point(16, 23) });
        g.DrawLine(p, 9, 11, 23, 11);
        g.DrawLine(p, 9, 16, 20, 16);
    });

    public static Icon CreateCalculator() => Create(Color.FromArgb(45, 155, 105), (g, p) =>
    {
        g.DrawRoundedRectangle(p, new Rectangle(6, 3, 20, 26), 3);
        g.DrawRectangle(p, 10, 7, 12, 5);
        foreach (var point in new[] { new Point(11, 16), new Point(17, 16), new Point(11, 22), new Point(17, 22) })
            g.DrawRectangle(p, point.X, point.Y, 3, 3);
        g.DrawLine(p, 22, 16, 22, 25);
    });

    public static Icon CreateClipboard() => Create(Color.FromArgb(210, 135, 40), (g, p) =>
    {
        g.DrawRectangle(p, 6, 6, 20, 23);
        g.DrawRoundedRectangle(p, new Rectangle(11, 3, 10, 6), 2);
        g.DrawLine(p, 10, 14, 22, 14);
        g.DrawLine(p, 10, 19, 22, 19);
        g.DrawLine(p, 10, 24, 19, 24);
    });

    public static Icon CreateSnippets() => Create(Color.FromArgb(135, 95, 190), (g, p) =>
    {
        g.DrawPolygon(p, new[] { new Point(8, 3), new Point(20, 3), new Point(25, 8), new Point(25, 29), new Point(8, 29) });
        g.DrawLine(p, 20, 3, 20, 8);
        g.DrawLine(p, 20, 8, 25, 8);
        g.DrawLine(p, 12, 14, 21, 14);
        g.DrawLine(p, 12, 19, 21, 19);
        g.DrawLine(p, 12, 24, 19, 24);
    });

    public static Icon CreateDisplay() => Create(Color.FromArgb(35, 155, 165), (g, p) =>
    {
        g.DrawEllipse(p, 4, 4, 18, 18);
        g.DrawLine(p, 19, 19, 29, 29);
        g.DrawLine(p, 9, 13, 17, 13);
        g.DrawLine(p, 13, 9, 13, 17);
    });

    private static Icon Create(Color accent, Action<Graphics, Pen> draw)
    {
        using var bitmap = new Bitmap(32, 32);
        using (var graphics = Graphics.FromImage(bitmap))
        using (var pen = new Pen(accent, 2.2f))
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);
            draw(graphics, pen);
        }

        var handle = bitmap.GetHicon();
        try
        {
            using var temporaryIcon = Icon.FromHandle(handle);
            return (Icon)temporaryIcon.Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr handle);
}

[SupportedOSPlatform("windows")]
internal static class GraphicsExtensions
{
    public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int radius)
    {
        var diameter = radius * 2;
        using var path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        graphics.DrawPath(pen, path);
    }
}
