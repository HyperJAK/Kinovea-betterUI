using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Kinovea.Services
{
    public class FluentPanel : Panel
    {
        public FluentPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Padding = new Padding(8);
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            FluentGeometry.FillBackground(e.Graphics, ClientRectangle, Parent == null ? DarkTheme.Background : Parent.BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = FluentGeometry.Round(new RectangleF(.5F, .5F, Width - 1, Height - 1), 8))
            using (var brush = new SolidBrush(DarkTheme.Surface))
            using (var border = new Pen(DarkTheme.Border))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(border, path);
            }
        }
    }
    public static class FluentGeometry
    {
        public static void FillBackground(Graphics canvas, Rectangle bounds, Color color)
        {
            using (var brush = new SolidBrush(color)) canvas.FillRectangle(brush, bounds);
        }
        public static GraphicsPath Round(RectangleF bounds, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
            if (diameter <= 0) { path.AddRectangle(bounds); return path; }
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    /// <summary>Native button semantics with Fluent painting and no animation timers.</summary>
    public class FluentButton : Button
    {
        private bool hover;
        private bool pressed;
        public bool Accent { get; set; }

        public FluentButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { pressed = e.Button == MouseButtons.Left; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { pressed = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnKeyDown(KeyEventArgs e) { if (e.KeyCode == Keys.Space) { pressed = true; Invalidate(); } base.OnKeyDown(e); }
        protected override void OnKeyUp(KeyEventArgs e) { pressed = false; Invalidate(); base.OnKeyUp(e); }
        protected override void OnLostFocus(EventArgs e) { pressed = false; Invalidate(); base.OnLostFocus(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            FluentGeometry.FillBackground(e.Graphics, ClientRectangle, Parent == null ? DarkTheme.Surface : Parent.BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color fill = !Enabled ? DarkTheme.Surface : Accent ? DarkTheme.Accent : BackColor.A == 0 ? DarkTheme.Raised : BackColor;
            if (Enabled && hover) fill = Accent ? Color.FromArgb(175, 218, 250) : DarkTheme.Hover;
            if (Enabled && pressed) fill = Accent ? Color.FromArgb(120, 181, 225) : DarkTheme.Selection;
            float scale = e.Graphics.DpiX / 96F;
            using (GraphicsPath outline = FluentGeometry.Round(new RectangleF(.5F, .5F, Width - 1, Height - 1), 4 * scale))
            using (SolidBrush brush = new SolidBrush(fill))
            using (Pen border = new Pen(Accent ? fill : DarkTheme.Border))
            {
                e.Graphics.FillPath(brush, outline);
                e.Graphics.DrawPath(border, outline);
            }
            Rectangle content = Rectangle.Inflate(ClientRectangle, -4, -2);
            Image image = Image ?? BackgroundImage;
            if (image != null)
            {
                int imageLeft = content.Left + (content.Width - image.Width) / 2;
                int imageTop = content.Top + (content.Height - image.Height) / 2;
                if (!string.IsNullOrEmpty(Text) && TextImageRelation == TextImageRelation.ImageBeforeText)
                {
                    imageLeft = content.Left + 4;
                    content.X += image.Width + 8;
                    content.Width -= image.Width + 8;
                }
                if (Enabled) e.Graphics.DrawImageUnscaled(image, imageLeft, imageTop);
                else ControlPaint.DrawImageDisabled(e.Graphics, image, imageLeft, imageTop, fill);
            }
            TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
            if (!UseMnemonic) flags |= TextFormatFlags.NoPrefix;
            if (!ShowKeyboardCues) flags |= TextFormatFlags.HidePrefix;
            if (RightToLeft == RightToLeft.Yes) flags |= TextFormatFlags.RightToLeft;
            TextRenderer.DrawText(e.Graphics, Text, Font, content,
                !Enabled ? DarkTheme.Muted : Accent ? Color.FromArgb(20, 30, 38) : DarkTheme.Text, flags);
            if (Focused && ShowFocusCues)
            {
                using (GraphicsPath focus = FluentGeometry.Round(new RectangleF(3, 3, Width - 6, Height - 6), 2 * scale))
                using (Pen pen = new Pen(Accent ? Color.Black : DarkTheme.Accent))
                    e.Graphics.DrawPath(pen, focus);
            }
        }

    }

    /// <summary>Keeps native tab selection, keyboard navigation and page layout.</summary>
    public class FluentTabControl : TabControl
    {
        public FluentTabControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Padding = new Point(16, 8);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            FluentGeometry.FillBackground(e.Graphics, ClientRectangle, DarkTheme.Surface);
            for (int i = 0; i < TabCount; i++)
            {
                Rectangle bounds = GetTabRect(i);
                if (i == SelectedIndex)
                {
                    using (GraphicsPath path = FluentGeometry.Round(Rectangle.Inflate(bounds, -2, -2), 4))
                    using (SolidBrush brush = new SolidBrush(DarkTheme.Raised))
                        e.Graphics.FillPath(brush, path);
                    using (SolidBrush brush = new SolidBrush(DarkTheme.Accent))
                        e.Graphics.FillRectangle(brush, bounds.Left + 14, bounds.Bottom - 3, Math.Max(0, bounds.Width - 28), 2);
                }
                Rectangle text = Rectangle.Inflate(bounds, -8, -4);
                if (ImageList != null)
                {
                    Image icon = null;
                    if (TabPages[i].ImageIndex >= 0 && TabPages[i].ImageIndex < ImageList.Images.Count)
                        icon = ImageList.Images[TabPages[i].ImageIndex];
                    else if (!string.IsNullOrEmpty(TabPages[i].ImageKey) && ImageList.Images.ContainsKey(TabPages[i].ImageKey))
                        icon = ImageList.Images[TabPages[i].ImageKey];
                    if (icon != null)
                    {
                        e.Graphics.DrawImageUnscaled(icon, text.Left, text.Top + (text.Height - icon.Height) / 2);
                        text.X += icon.Width + 6;
                        text.Width -= icon.Width + 6;
                    }
                }
                TextRenderer.DrawText(e.Graphics, TabPages[i].Text, Font, text,
                    i == SelectedIndex ? DarkTheme.Text : DarkTheme.Muted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                if (Focused && i == SelectedIndex && ShowFocusCues)
                    ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(bounds, -4, -4), DarkTheme.Text, DarkTheme.Surface);
            }
        }
        protected override void OnSelectedIndexChanged(EventArgs e) { Invalidate(); base.OnSelectedIndexChanged(e); }
    }

    public class FluentGroupBox : GroupBox
    {
        public FluentGroupBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            FluentGeometry.FillBackground(e.Graphics, ClientRectangle, Parent == null ? DarkTheme.Background : Parent.BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = FluentGeometry.Round(new RectangleF(.5F, .5F, Width - 1, Height - 1), 8))
            using (var brush = new SolidBrush(DarkTheme.Surface))
            using (var border = new Pen(DarkTheme.Border))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(border, path);
            }
            TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(12, 0, Width - 24, Font.Height + 6),
                Enabled ? DarkTheme.Text : DarkTheme.Muted, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }
    }
}
