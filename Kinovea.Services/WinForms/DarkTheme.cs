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
        public static readonly Color Background = Color.FromArgb(18, 20, 25);
        public static readonly Color Surface = Color.FromArgb(28, 31, 38);
        public static readonly Color Raised = Color.FromArgb(40, 44, 54);
        public static readonly Color Text = Color.FromArgb(232, 234, 240);
        public static readonly Color Muted = Color.FromArgb(164, 173, 190);
        public static readonly Color Accent = Color.FromArgb(174, 196, 255);
        public static readonly Color Selection = Color.FromArgb(58, 73, 108);
        private static readonly ConditionalWeakTable<Control, object> styled = new ConditionalWeakTable<Control, object>();
        private static readonly ConditionalWeakTable<ToolStripItem, object> styledItems = new ConditionalWeakTable<ToolStripItem, object>();
        private static readonly ToolStripRenderer renderer = new ToolStripProfessionalRenderer(new DarkColors());

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
            control.ForeColor = Text;

            Button button = control as Button;
            if (button != null)
            {
                button.UseVisualStyleBackColor = false;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = Raised;
                button.FlatAppearance.MouseOverBackColor = Selection;
                button.FlatAppearance.MouseDownBackColor = Raised;
                button.BackColor = Raised;
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
            ToolStripDropDownItem dropdown = item as ToolStripDropDownItem;
            if (dropdown == null)
                return;
            dropdown.DropDown.Renderer = renderer;
            dropdown.DropDown.BackColor = Surface;
            ApplyItems(dropdown.DropDownItems);
            dropdown.DropDownOpening += (sender, args) => ApplyItems(dropdown.DropDownItems);
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
            }
        }
    }
}
