using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Kinovea.Services
{
    /// <summary>Shared, event-driven styling; never runs in the video paint loop.</summary>
    public static class DarkTheme
    {
        public static readonly Color Background = Color.FromArgb(32, 32, 32);
        public static readonly Color Surface = Color.FromArgb(39, 39, 39);
        public static readonly Color Raised = Color.FromArgb(49, 49, 49);
        public static readonly Color Hover = Color.FromArgb(60, 60, 60);
        public static readonly Color Border = Color.FromArgb(68, 68, 68);
        public static readonly Color Text = Color.FromArgb(245, 245, 245);
        public static readonly Color Muted = Color.FromArgb(175, 175, 175);
        public static readonly Color Accent = Color.FromArgb(153, 210, 249);
        public static readonly Color Selection = Color.FromArgb(52, 66, 76);
        private static readonly ConditionalWeakTable<Control, object> styled = new ConditionalWeakTable<Control, object>();
        private static readonly ConditionalWeakTable<ToolStripItem, object> styledItems = new ConditionalWeakTable<ToolStripItem, object>();
        private static readonly ToolStripRenderer renderer = new FluentRenderer();

        public static void Apply(Control control)
        {
            object marker;
            if (control.IsDisposed || styled.TryGetValue(control, out marker))
                return;
            styled.Add(control, new object());
            // Swatch backgrounds are data: the color picker reads them on click.
            if (control is Button && control.Parent != null && control.Parent.GetType().Name == "ColorPicker")
                return;
            control.BackColor = control is Form ? Background : Surface;
            if (control is Label)
                control.BackColor = Color.Transparent;
            control.ForeColor = Text;
            // Preserve sizes from legacy layouts, while replacing their old font family.
            if (control.Font.FontFamily.Name == "Microsoft Sans Serif" || control.Font.FontFamily.Name == "Tahoma")
                control.Font = new Font("Segoe UI", control.Font.SizeInPoints, control.Font.Style);

            Button button = control as Button;
            if (button != null)
            {
                button.UseVisualStyleBackColor = false;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = Raised;
                button.FlatAppearance.MouseOverBackColor = Selection;
                button.FlatAppearance.MouseDownBackColor = Raised;
                button.BackColor = Raised;
                FluentButton fluent = button as FluentButton;
                Form form = control.FindForm();
                if (fluent != null && form != null && form.AcceptButton == button)
                    fluent.Accent = true;
            }
            if (control is TextBoxBase || control is NumericUpDown || control is ComboBox)
                control.BackColor = Raised;
            LinkLabel link = control as LinkLabel;
            if (link != null)
            {
                link.LinkColor = Accent;
                link.ActiveLinkColor = Text;
                link.VisitedLinkColor = Accent;
            }
            ListView list = control as ListView;
            if (list != null)
                list.BorderStyle = BorderStyle.None;
            TreeView tree = control as TreeView;
            if (tree != null)
            {
                tree.BorderStyle = BorderStyle.None;
                tree.LineColor = Raised;
            }
            ToolStrip strip = control as ToolStrip;
            if (strip != null)
            {
                strip.Renderer = renderer;
                strip.GripStyle = ToolStripGripStyle.Hidden;
                ApplyItems(strip.Items);
                strip.ItemAdded += (sender, args) => ApplyItem(args.Item);
            }
            SplitContainer split = control as SplitContainer;
            if (split != null)
            {
                split.BorderStyle = BorderStyle.None;
                split.BackColor = Background;
            }
            if (control.ContextMenuStrip != null)
                Apply(control.ContextMenuStrip);
            control.ControlAdded += (sender, args) => Apply(args.Control);
            foreach (Control child in control.Controls)
                Apply(child);
        }

        private static void ApplyItems(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
                ApplyItem(item);
        }

        private static void ApplyItem(ToolStripItem item)
        {
            object marker;
            if (styledItems.TryGetValue(item, out marker))
                return;
            styledItems.Add(item, new object());
            item.ForeColor = Text;
            if ((item is ToolStripButton || item is ToolStripDropDownItem) && !(item.Owner is ToolStripDropDown) && !(item.Owner is MenuStrip))
            {
                item.Padding = new Padding(8, 6, 8, 6);
                item.Margin = new Padding(2, 0, 2, 0);
            }
            ToolStripDropDownItem dropdown = item as ToolStripDropDownItem;
            if (dropdown == null)
                return;
            dropdown.DropDown.Renderer = renderer;
            dropdown.DropDown.BackColor = Surface;
            ApplyItems(dropdown.DropDownItems);
            dropdown.DropDownOpening += (sender, args) => ApplyItems(dropdown.DropDownItems);
        }

        private sealed class FluentRenderer : ToolStripProfessionalRenderer
        {
            public FluentRenderer() : base(new DarkColors()) { RoundedEdges = false; }
            protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
            {
                ToolStripButton button = e.Item as ToolStripButton;
                bool selected = e.Item.Selected || e.Item.Pressed || (button != null && button.Checked);
                if (!selected) return;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = FluentGeometry.Round(new RectangleF(1, 1, e.Item.Width - 2, e.Item.Height - 2), 4))
                using (var brush = new SolidBrush(e.Item.Pressed ? Selection : Hover))
                    e.Graphics.FillPath(brush, path);
                if (button != null && button.Checked)
                    using (var brush = new SolidBrush(Accent))
                        e.Graphics.FillRectangle(brush, 8, e.Item.Height - 3, Math.Max(0, e.Item.Width - 16), 2);
            }
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (!e.Item.Selected && !e.Item.Pressed) return;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = FluentGeometry.Round(new RectangleF(2, 1, e.Item.Width - 4, e.Item.Height - 2), 4))
                using (var brush = new SolidBrush(Hover))
                    e.Graphics.FillPath(brush, path);
            }
            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = e.Item.Enabled ? Text : Muted;
                if (e.TextDirection == ToolStripTextDirection.Horizontal)
                    TextRenderer.DrawText(e.Graphics, e.Text, e.TextFont, e.TextRectangle, e.TextColor, e.TextFormat);
                else
                    base.OnRenderItemText(e);
            }
            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                e.ArrowColor = e.Item.Enabled ? Text : Muted;
                base.OnRenderArrow(e);
            }
            protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
            {
                using (Pen pen = new Pen(Border))
                    if (e.Vertical) e.Graphics.DrawLine(pen, e.Item.Width / 2, 8, e.Item.Width / 2, e.Item.Height - 8);
                    else e.Graphics.DrawLine(pen, 12, e.Item.Height / 2, e.Item.Width - 12, e.Item.Height / 2);
            }
            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                if (e.ToolStrip is ToolStripDropDown)
                    base.OnRenderToolStripBorder(e);
            }
        }

        private sealed class DarkColors : ProfessionalColorTable
        {
            public DarkColors() { UseSystemColors = false; }
            public override Color ToolStripGradientBegin { get { return Surface; } }
            public override Color ToolStripGradientMiddle { get { return Surface; } }
            public override Color ToolStripGradientEnd { get { return Surface; } }
            public override Color MenuStripGradientBegin { get { return Background; } }
            public override Color MenuStripGradientEnd { get { return Background; } }
            public override Color StatusStripGradientBegin { get { return Background; } }
            public override Color StatusStripGradientEnd { get { return Background; } }
            public override Color ToolStripDropDownBackground { get { return Surface; } }
            public override Color ImageMarginGradientBegin { get { return Surface; } }
            public override Color ImageMarginGradientMiddle { get { return Surface; } }
            public override Color ImageMarginGradientEnd { get { return Surface; } }
            public override Color MenuItemSelected { get { return Selection; } }
            public override Color MenuItemBorder { get { return Selection; } }
            public override Color MenuBorder { get { return Raised; } }
            public override Color MenuItemSelectedGradientBegin { get { return Selection; } }
            public override Color MenuItemSelectedGradientEnd { get { return Selection; } }
            public override Color MenuItemPressedGradientBegin { get { return Raised; } }
            public override Color MenuItemPressedGradientMiddle { get { return Raised; } }
            public override Color MenuItemPressedGradientEnd { get { return Raised; } }
            public override Color ButtonSelectedGradientBegin { get { return Selection; } }
            public override Color ButtonSelectedGradientMiddle { get { return Selection; } }
            public override Color ButtonSelectedGradientEnd { get { return Selection; } }
            public override Color ButtonSelectedBorder { get { return Accent; } }
            public override Color ButtonPressedGradientBegin { get { return Raised; } }
            public override Color ButtonPressedGradientMiddle { get { return Raised; } }
            public override Color ButtonPressedGradientEnd { get { return Raised; } }
            public override Color ButtonCheckedGradientBegin { get { return Selection; } }
            public override Color ButtonCheckedGradientMiddle { get { return Selection; } }
            public override Color ButtonCheckedGradientEnd { get { return Selection; } }
            public override Color SeparatorDark { get { return Raised; } }
            public override Color SeparatorLight { get { return Surface; } }
            public override Color ToolStripBorder { get { return Raised; } }
        }
    }

    public class ThemedForm : Form
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DarkTheme.Apply(this);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            // Older Windows versions ignore unsupported DWM attributes.
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 1;
                DwmSetWindowAttribute(Handle, 20, ref enabled, sizeof(int));
                int rounded = 2; // DWMWCP_ROUND; unsupported attributes are ignored.
                DwmSetWindowAttribute(Handle, 33, ref rounded, sizeof(int));
            }
        }
    }
}
