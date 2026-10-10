using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Kinovea.Services
{
    public enum FluentIcon { Open, Save, Frames, Library, Video, Compare, Camera, DualCamera, Mixed }

    /// <summary>Small vector line icons rasterized once, independent of icon fonts.</summary>
    public static class FluentIcons
    {
        private static readonly Dictionary<FluentIcon, Bitmap> cache = new Dictionary<FluentIcon, Bitmap>();
        public static Bitmap Get(FluentIcon icon)
        {
            lock (cache)
            {
                Bitmap image;
                if (cache.TryGetValue(icon, out image)) return image;
                image = new Bitmap(20, 20);
                using (Graphics g = Graphics.FromImage(image))
                using (Pen pen = new Pen(DarkTheme.Text, 1.25F))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TranslateTransform(2, 2);
                    switch (icon)
                    {
                        case FluentIcon.Open:
                            g.DrawLines(pen, new [] { new Point(1, 13), new Point(1, 3), new Point(6, 3), new Point(8, 5), new Point(14, 5), new Point(14, 7) });
                            g.DrawPolygon(pen, new [] { new Point(1, 13), new Point(4, 7), new Point(15, 7), new Point(12, 13) });
                            break;
                        case FluentIcon.Save:
                            g.DrawPolygon(pen, new [] { new Point(2, 1), new Point(12, 1), new Point(14, 3), new Point(14, 14), new Point(2, 14) });
                            g.DrawRectangle(pen, 5, 1, 6, 4); g.DrawRectangle(pen, 5, 9, 6, 5);
                            break;
                        case FluentIcon.Frames:
                            g.DrawRectangle(pen, 1, 5, 11, 9); g.DrawLines(pen, new [] { new Point(4, 2), new Point(15, 2), new Point(15, 11) });
                            g.DrawLines(pen, new [] { new Point(2, 12), new Point(5, 9), new Point(7, 11), new Point(9, 8), new Point(12, 11) });
                            break;
                        case FluentIcon.Library:
                            g.DrawRectangle(pen, 1, 2, 14, 12); g.DrawLine(pen, 5, 2, 5, 14);
                            break;
                        case FluentIcon.Video:
                            Video(g, pen, 1, 3, 14, 10); break;
                        case FluentIcon.Compare:
                            Video(g, pen, 0, 4, 7, 9); Video(g, pen, 9, 4, 7, 9); break;
                        case FluentIcon.Camera:
                            Camera(g, pen, 1, 4); break;
                        case FluentIcon.DualCamera:
                            Camera(g, pen, 0, 3); Camera(g, pen, 3, 6); break;
                        case FluentIcon.Mixed:
                            Video(g, pen, 0, 2, 10, 7); Camera(g, pen, 3, 7); break;
                    }
                }
                cache.Add(icon, image);
                return image;
            }
        }

        private static void Video(Graphics g, Pen pen, int x, int y, int width, int height)
        {
            g.DrawRectangle(pen, x, y, width, height);
            using (var brush = new SolidBrush(DarkTheme.Text))
                g.FillPolygon(brush, new [] { new PointF(x + width * .4F, y + height * .25F), new PointF(x + width * .4F, y + height * .75F), new PointF(x + width * .7F, y + height * .5F) });
        }
        private static void Camera(Graphics g, Pen pen, int x, int y)
        {
            g.DrawRectangle(pen, x, y, 9, 7);
            g.DrawPolygon(pen, new [] { new Point(x + 9, y + 2), new Point(x + 13, y), new Point(x + 13, y + 7), new Point(x + 9, y + 5) });
        }
    }
}
