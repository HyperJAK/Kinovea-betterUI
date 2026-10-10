// UI fixture: uses the real shell, controls and extraction dialog with stub media data.
// Rendered sample content is not a full application or playback benchmark.
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Kinovea.Root;
using Kinovea.Services;
using Kinovea.ScreenManager;

static class FluentPreview
{
    public static void VerifyWorkspace()
    {
        using (var shell = new SupervisorUserInterface(new RootKernel()))
        using (var library = new UserControl())
        using (var viewer = new UserControl())
        {
            shell.Size = new Size(1100, 650);
            shell.PlugUI(library, viewer);
            shell.PerformLayout();
            if (library.Width < 100 || viewer.Width < 100 || library.Height < 100 || viewer.Height < 100)
                throw new Exception("Workspace pane layout is invalid");
            shell.ToggleNavigationPanel(false, false);
            if (viewer.Width < 900) throw new Exception("Collapsed library did not expand the viewer");
            shell.ToggleNavigationPanel(true, false);
            if (library.Width < 100) throw new Exception("Library did not restore");
        }
        Console.WriteLine("Passed workspace layout and collapse checks.");
        using (var settings = new FormPreferences2())
        {
            settings.StartPosition = FormStartPosition.Manual;
            settings.Location = new Point(-32000, -32000);
            settings.Show();
            var navigation = settings.Controls.OfType<Panel>().Single().Controls.OfType<PreferencePanelButtton>().ToArray();
            if (navigation.Length != 5) throw new Exception("Settings categories are missing");
            foreach (var item in navigation)
                if (!settings.ClientRectangle.Contains(new Rectangle(item.Parent.Left + item.Left, item.Parent.Top + item.Top, item.Width, item.Height)))
                    throw new Exception("Settings navigation is clipped");
            typeof(Control).GetMethod("OnKeyDown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(navigation[3], new object[] { new KeyEventArgs(Keys.Enter) });
            if (PreferencesManager.GeneralPreferences.PreferencePage != 3)
                throw new Exception("Keyboard navigation did not select the requested settings page");
            if (!navigation[3].PreferencePanel.Visible)
                throw new Exception("Selected settings page did not become visible");
            if (!(settings.AcceptButton is FluentButton) || !(settings.CancelButton is FluentButton))
                throw new Exception("Settings dialog default actions are missing");
            settings.Close();
        }
        PreferencesManager.GeneralPreferences.PreferencePage = 1;
        Console.WriteLine("Passed settings navigation, keyboard and default-action checks.");
    }

    public static void Render(string directory)
    {
        Directory.CreateDirectory(directory);
        var player = new PlayerScreen();
        player.FrameServer.Metadata.SelectionEnd = 1199 * 40;
        using (var dialog = new FormConfigureExportImageSequence(player))
        {
            dialog.Controls.OfType<GroupBox>().Single().Controls.OfType<NumericUpDown>().Single().Value = 15;
            DarkTheme.Apply(dialog);
            Save(dialog, Path.Combine(directory, "frame-extraction.png"));
        }
        using (var form = new ThemedForm { ClientSize = new Size(1180, 760), Text = "Kinovea - UI fixture" })
        {
            var shell = new SupervisorUserInterface(new RootKernel()) { Dock = DockStyle.Fill };
            var library = new UserControl();
            var tabs = new FluentTabControl { Dock = DockStyle.Fill };
            var files = new TabPage("Files");
            tabs.TabPages.Add(files); tabs.TabPages.Add("Cameras");
            var tree = new TreeView { Dock = DockStyle.Fill, Indent = 18 };
            tree.Nodes.Add("Videos").Nodes.Add("Motion analysis");
            tree.Nodes.Add("Pictures"); tree.Nodes.Add("Exports"); tree.ExpandAll();
            files.Controls.Add(tree); library.Controls.Add(tabs);
            var viewer = new UserControl();
            var title = new Label { Text = "Media workspace", Dock = DockStyle.Top, Height = 56, Padding = new Padding(16, 0, 0, 0), TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 18F, FontStyle.Bold) };
            var empty = new Label { Dock = DockStyle.Fill, Text = "Open an image or video\n\nSample content for the native UI preview", TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 13F) };
            viewer.Controls.Add(empty); viewer.Controls.Add(title);
            shell.PlugUI(library, viewer);
            var menu = new MenuStrip { Dock = DockStyle.Top, Padding = new Padding(12, 5, 12, 5), Font = new Font("Segoe UI", 9F) };
            foreach (string text in new [] {"File", "Edit", "View", "Image", "Video", "Tools", "Options", "Help"}) menu.Items.Add(text);
            var commands = new ToolStrip { Dock = DockStyle.Top, AutoSize = false, Height = 52, Padding = new Padding(12, 8, 12, 8), Font = new Font("Segoe UI", 9F) };
            commands.Items.Add(new ToolStripLabel("Kinovea") { Font = new Font("Segoe UI", 12F, FontStyle.Bold), Margin = new Padding(4, 0, 20, 0) });
            commands.Items.Add(new ToolStripButton("Open video", FluentIcons.Get(FluentIcon.Open)));
            commands.Items.Add(new ToolStripButton("Save", FluentIcons.Get(FluentIcon.Save)) { Enabled = false });
            commands.Items.Add(new ToolStripButton("Image sequence", FluentIcons.Get(FluentIcon.Frames)) { Enabled = false });
            commands.Items.Add(new ToolStripSeparator());
            commands.Items.Add(new ToolStripButton("Library", FluentIcons.Get(FluentIcon.Library)) { Checked = true });
            var status = new StatusStrip(); status.Items.Add("Native UI fixture - media engine is not loaded");
            form.Controls.Add(shell); form.Controls.Add(commands); form.Controls.Add(menu); form.Controls.Add(status);
            DarkTheme.Apply(form); form.PerformLayout();
            Save(form, Path.Combine(directory, "workspace.png"));
        }
        using (var form = new FormPreferences2())
        {
            DarkTheme.Apply(form);
            Save(form, Path.Combine(directory, "settings-navigation.png"));
        }
        Console.WriteLine("Rendered native UI fixtures to " + directory);
    }

    private static void Save(Form form, string path)
    {
        // Create and load child handles offscreen so DrawToBitmap includes them.
        form.StartPosition = FormStartPosition.Manual;
        form.Location = new Point(-32000, -32000);
        form.Show();
        form.PerformLayout();
        form.Update();
        using (Bitmap bitmap = new Bitmap(form.ClientSize.Width, form.ClientSize.Height))
        {
            // Render client controls, excluding the host's non-client caption style.
            using (var whole = new Bitmap(form.Width, form.Height))
            {
                form.DrawToBitmap(whole, new Rectangle(Point.Empty, whole.Size));
                Point origin = form.PointToScreen(Point.Empty);
                using (Graphics canvas = Graphics.FromImage(bitmap))
                    canvas.DrawImageUnscaled(whole, form.Left - origin.X, form.Top - origin.Y);
            }
            bitmap.Save(path, ImageFormat.Png);
        }
        form.Close();
    }
}
