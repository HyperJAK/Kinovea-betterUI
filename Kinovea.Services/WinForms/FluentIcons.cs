using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Kinovea.Services
{
    public enum FluentIcon { Open, Save, Frames, Library, Video, Compare, Camera, DualCamera, Mixed, Play, Pause, Previous, Next, First, Last, Image, VideoSave, VideoPause, Slideshow, FolderAdd, Refresh, Expand, Collapse }

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
                        case FluentIcon.Play:
                            g.DrawPolygon(pen, new [] { new Point(4, 2), new Point(13, 8), new Point(4, 14) }); break;
                        case FluentIcon.Pause:
                            g.DrawRectangle(pen, 3, 2, 3, 12); g.DrawRectangle(pen, 10, 2, 3, 12); break;
                        case FluentIcon.Previous:
                        case FluentIcon.First:
                            g.DrawLine(pen, 2, 2, 2, 14);
                            g.DrawPolygon(pen, new [] { new Point(12, 3), new Point(5, 8), new Point(12, 13) });
                            if (icon == FluentIcon.First) g.DrawLine(pen, 15, 3, 15, 13);
                            break;
                        case FluentIcon.Next:
                        case FluentIcon.Last:
                            g.DrawPolygon(pen, new [] { new Point(3, 3), new Point(10, 8), new Point(3, 13) });
                            g.DrawLine(pen, 13, 2, 13, 14);
                            if (icon == FluentIcon.Last) g.DrawLine(pen, 0, 3, 0, 13);
                            break;
                        case FluentIcon.Image:
                            g.DrawRectangle(pen, 1, 2, 14, 12); g.DrawEllipse(pen, 4, 4, 2, 2);
                            g.DrawLines(pen, new [] { new Point(2, 12), new Point(6, 8), new Point(9, 11), new Point(12, 7), new Point(14, 10) }); break;
                        case FluentIcon.VideoSave:
                            Video(g, pen, 1, 1, 12, 9); g.DrawLine(pen, 10, 10, 10, 15);
                            g.DrawLines(pen, new [] { new Point(7, 12), new Point(10, 15), new Point(13, 12) }); break;
                        case FluentIcon.VideoPause:
                            Video(g, pen, 1, 1, 12, 9); g.DrawLine(pen, 9, 11, 9, 15); g.DrawLine(pen, 13, 11, 13, 15); break;
                        case FluentIcon.Slideshow:
                            g.DrawRectangle(pen, 1, 1, 14, 10); g.DrawLine(pen, 8, 11, 8, 15);
                            g.DrawLine(pen, 4, 15, 12, 15); g.DrawLines(pen, new [] { new Point(3, 8), new Point(6, 5), new Point(10, 8) }); break;
                        case FluentIcon.FolderAdd:
                            g.DrawLines(pen, new [] { new Point(7, 13), new Point(1, 13), new Point(1, 3), new Point(6, 3), new Point(8, 5), new Point(14, 5), new Point(14, 8) });
                            g.DrawLine(pen, 12, 9, 12, 15); g.DrawLine(pen, 9, 12, 15, 12); break;
                        case FluentIcon.Refresh:
                            g.DrawArc(pen, 2, 2, 12, 12, 50, 290);
                            g.DrawLines(pen, new [] { new Point(14, 2), new Point(14, 7), new Point(9, 7) }); break;
                        case FluentIcon.Expand:
                            g.DrawLines(pen, new [] { new Point(1, 6), new Point(1, 1), new Point(6, 1) });
                            g.DrawLines(pen, new [] { new Point(10, 1), new Point(15, 1), new Point(15, 6) });
                            g.DrawLines(pen, new [] { new Point(1, 10), new Point(1, 15), new Point(6, 15) });
                            g.DrawLines(pen, new [] { new Point(10, 15), new Point(15, 15), new Point(15, 10) }); break;
                        case FluentIcon.Collapse:
                            g.DrawLines(pen, new [] { new Point(1, 6), new Point(6, 6), new Point(6, 1) });
                            g.DrawLines(pen, new [] { new Point(10, 1), new Point(10, 6), new Point(15, 6) });
                            g.DrawLines(pen, new [] { new Point(1, 10), new Point(6, 10), new Point(6, 15) });
                            g.DrawLines(pen, new [] { new Point(10, 15), new Point(10, 10), new Point(15, 10) }); break;
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
