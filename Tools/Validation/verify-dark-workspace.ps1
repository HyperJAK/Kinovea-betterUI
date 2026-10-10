param([string]$PreviewDirectory)
$ErrorActionPreference = 'Stop'
$workspace = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$output = Join-Path ([IO.Path]::GetTempPath()) ('Kinovea-UI-check-' + [Guid]::NewGuid())
New-Item -ItemType Directory -Path $output | Out-Null
try {
    # Compile the actual theme and extraction dialog against tiny media-model stubs.
    # This check needs only the Windows Framework runtime; it does not test decoding.
    $stub = @'
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Kinovea.Services;
using Kinovea.ScreenManager;
namespace Kinovea.Video { public class VideoInfo { public double AverageTimeStampsPerFrame = 40; } }
namespace Kinovea.ScreenManager {
    public class Metadata { public double BaselineFrameInterval = 40; public long SelectionStart; public long SelectionEnd; public double AverageTimeStampsPerFrame = 40; }
    public class Reader { public Kinovea.Video.VideoInfo Info = new Kinovea.Video.VideoInfo(); }
    public class Server { public Metadata Metadata = new Metadata(); public Reader VideoReader = new Reader(); }
    public class PlayerScreen { public Server FrameServer = new Server(); }
}
namespace Kinovea.ScreenManager.Languages {
    public class PreviewResources { public string GetString(string name) { return "Choose PNG for lossless images."; } }
    public static class ScreenManagerLang {
        public static readonly PreviewResources ResourceManager = new PreviewResources();
        public const string Generic_Close = "Close";
        public const string formConfigureExport_ImageSequence = "Image sequence";
        public const string Generic_Configuration = "Configuration";
        public const string Generic_Save = "Save";
        public const string Generic_Cancel = "Cancel";
        public const string dlgRafaleExport_ExportAll = "Export every frame";
        public const string dlgRafaleExport_ExportFrameDecimation = "Export every {0} frames";
        public const string dlgRafaleExport_ExportTimeDecimationSeconds = "Every {0} seconds";
        public const string dlgRafaleExport_ExportTimeDecimationMilliseconds = "Every {0} ms";
        public const string dlgRafaleExport_LabelTotalFrames = "{0} frames";
    }
}
namespace Kinovea.Services {
    public class PreviewWindow { public float ExplorerSplitterRatio = .25F; public bool ExplorerVisible = true; }
    public static class WindowManager { public static PreviewWindow ActiveWindow = new PreviewWindow(); }
    public static class LaunchSettingsManager { public static bool ExplorerVisible = true; }
    public static class NotificationCenter { public static event EventHandler ToggleNavigationPane; public static void RaisePreferencesOpened() { } }
    public class PreviewPreferences { public int PreferencePage = 1; }
    public static class PreferencesManager {
        public static PreviewPreferences GeneralPreferences = new PreviewPreferences();
        public static void BeforeRead() { } public static void SuspendSave() { } public static void ResumeSave() { }
    }
}
namespace Kinovea.Root {
    public class RootKernel { }
    public class PreviewPreferencePanel : UserControl, IPreferencePanel {
        private string description;
        public PreviewPreferencePanel(string name) {
            description = name; Size = new Size(490, 322);
            Controls.Add(new Label { Text = name, Location = new Point(16, 16), AutoSize = true, Font = new Font("Segoe UI", 14F, FontStyle.Bold) });
            Controls.Add(new Label { Text = "Sample page content for native layout validation", Location = new Point(16, 76), AutoSize = true });
        }
        public string Description { get { return description; } }
        public Bitmap Icon { get { return FluentIcons.Get(FluentIcon.Video); } }
        public System.Collections.Generic.List<PreferenceTab> Tabs { get { return new System.Collections.Generic.List<PreferenceTab>(); } }
        public void OpenTab(PreferenceTab tab) { } public void CommitChanges() { } public void Close() { }
    }
    public class PreferencePanelGeneral : PreviewPreferencePanel { public PreferencePanelGeneral() : base("General") { } }
    public class PreferencePanelPlayer : PreviewPreferencePanel { public PreferencePanelPlayer() : base("Playback") { } }
    public class PreferencePanelCapture : PreviewPreferencePanel { public PreferencePanelCapture() : base("Capture") { } }
    public class PreferencePanelDrawings : PreviewPreferencePanel { public PreferencePanelDrawings() : base("Drawings") { } }
    public class PreferencePanelKeyboard : PreviewPreferencePanel { public PreferencePanelKeyboard() : base("Keyboard") { } }
}
namespace Kinovea.Root.Languages { public static class RootLang { public const string NavigationPane = "Library"; public const string dlgPreferences_Title = "Preferences"; public const string Generic_Save = "Save"; public const string Generic_Cancel = "Cancel"; } }
namespace Kinovea.Root.Properties { public static class Resources { public static Image close2d_16 { get { return null; } } } }
class Check {
    static int checks;
    static void Assert(bool value, string message) { checks++; if (!value) throw new Exception(message); }
    [STAThread] static void Main(string[] args) {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.ThreadException += (sender, e) => {
            Console.Error.WriteLine(e.Exception);
            Environment.ExitCode = 1;
            Application.ExitThread();
        };
        // Shown windows and ToolStrip dropdowns need a real UI message loop,
        // including its normal shutdown. DoEvents after disposing them can
        // dispatch native callbacks to already-destroyed fixture handles.
        try {
            using (var context = new ApplicationContext()) {
                EventHandler run = null;
                run = (sender, e) => {
                    Application.Idle -= run;
                    try { Run(args); }
                    catch (Exception error) { Console.Error.WriteLine(error); Environment.ExitCode = 1; }
                    finally { context.ExitThread(); }
                };
                Application.Idle += run;
                try { Application.Run(context); }
                finally { Application.Idle -= run; }
            }
        }
        catch (Exception error) { Console.Error.WriteLine(error); Environment.ExitCode = 1; }
    }
    static void Run(string[] args) {
        Assert(Application.MessageLoop, "Native UI checks require an active message loop");
        foreach (int frames in new [] {1, 2, 15, 16, 30, 31, 1200}) {
            PlayerScreen player = new PlayerScreen();
            player.FrameServer.Metadata.SelectionEnd = (frames - 1) * 40;
            using (var dialog = new FormConfigureExportImageSequence(player)) {
                var input = dialog.Controls.OfType<GroupBox>().Single().Controls.OfType<NumericUpDown>().Single();
                Assert(dialog.RemainingFrames == frames, "Every frame must be available, including short selections");
                foreach (int interval in new [] {1, 2, 15, frames}) {
                    if (interval > frames) continue;
                    input.Value = interval;
                    int expected = Enumerable.Range(0, frames).Count(index => index % interval == 0);
                    Assert(dialog.RemainingFrames == expected, "Incorrect extraction count");
                    Assert(dialog.IntervalTimestamps == interval * 40, "Incorrect extraction interval");
                }
            }
        }
        using (var form = new ThemedForm()) {
            var button = new FluentButton { Size = new Size(120, 36), Accent = true, Text = "Save" };
            form.Controls.Add(button);
            DarkTheme.Apply(form);
            using (var bitmap = new Bitmap(button.Width, button.Height)) {
                button.DrawToBitmap(bitmap, button.ClientRectangle);
                Assert(bitmap.GetPixel(0, 0).ToArgb() == form.BackColor.ToArgb(), "Rounded button corner must expose its parent");
                Assert(bitmap.GetPixel(10, 18).ToArgb() == DarkTheme.Accent.ToArgb(), "Accent button fill missing");
                button.Enabled = false;
                button.DrawToBitmap(bitmap, button.ClientRectangle);
                Assert(bitmap.GetPixel(10, 18).ToArgb() == DarkTheme.Surface.ToArgb(), "Disabled button fill missing");
            }
            var tabs = new FluentTabControl { Size = new Size(400, 240) };
            tabs.TabPages.Add("Files"); tabs.TabPages.Add("Cameras");
            form.Controls.Add(tabs);
            tabs.SelectedIndex = 1;
            Assert(tabs.SelectedTab.Text == "Cameras", "Native tab selection failed");
        }
        foreach (FluentIcon icon in Enum.GetValues(typeof(FluentIcon)))
            Assert(object.ReferenceEquals(FluentIcons.Get(icon), FluentIcons.Get(icon)), "Toolbar icons must be cached");
        foreach (float scale in new [] {1F, 1.25F, 1.5F, 2F}) {
            using (var dialog = new FormConfigureExportImageSequence(new PlayerScreen())) {
                dialog.Scale(new SizeF(scale, scale));
                foreach (Control child in dialog.Controls)
                    Assert(dialog.ClientRectangle.Contains(child.Bounds), "Dialog control clipped after scaling");
                var group = dialog.Controls.OfType<GroupBox>().Single();
                foreach (Control child in group.Controls)
                    Assert(group.ClientRectangle.Contains(child.Bounds), "Extraction field clipped after scaling");
            }
        }
        FluentPreview.VerifyWorkspace();
        if (args.Length > 0) FluentPreview.Render(args[0]);
        using (var form = new ThemedForm()) {
            var strip = new ToolStrip(); form.Controls.Add(strip);
            DarkTheme.Apply(form);
            var button = new Button(); form.Controls.Add(button);
            Assert(button.BackColor == DarkTheme.Raised, "Dynamic button was not themed");
            var menu = new ToolStripDropDownButton("Menu"); strip.Items.Add(menu);
            var item = new ToolStripMenuItem("Export"); menu.DropDownItems.Add(item);
            menu.ShowDropDown(); menu.HideDropDown();
            Assert(item.ForeColor == DarkTheme.Text, "Dynamic menu was not themed");
            DarkTheme.Apply(form);
            Assert(form.BackColor == DarkTheme.Background, "Repeated theme application failed");
        }
        Assert(Application.OpenForms.Count == 0, "All fixture windows must close before UI shutdown");
        Console.WriteLine("Passed " + checks + " UI and extraction checks.");
    }
}
'@
    $stubPath = Join-Path $output 'Check.cs'
    [IO.File]::WriteAllText($stubPath, $stub)
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
    $executable = Join-Path $output 'Check.exe'
    $sources = @(
        $stubPath,
        (Join-Path $workspace 'Kinovea.Services/WinForms/DarkTheme.cs'),
        (Join-Path $workspace 'Kinovea.Services/WinForms/FluentControls.cs'),
        (Join-Path $workspace 'Kinovea.Services/WinForms/FluentIcons.cs'),
        (Join-Path $workspace 'Kinovea.ScreenManager/Exporters/Images/FormConfigureExportImageSequence.cs'),
        (Join-Path $workspace 'Kinovea.ScreenManager/Exporters/Images/FormConfigureExportImageSequence.Designer.cs'),
        (Join-Path $workspace 'Kinovea/UserInterface/SupervisorUserInterface.cs'),
        (Join-Path $workspace 'Kinovea/UserInterface/SupervisorUserInterface.Designer.cs'),
        (Join-Path $workspace 'Kinovea/UserInterface/PreferencePanelButtton.cs'),
        (Join-Path $workspace 'Kinovea/UserInterface/PreferencePanelButtton.Designer.cs'),
        (Join-Path $workspace 'Kinovea/UserInterface/PreferencePanels/IPreferencePanel.cs'),
        (Join-Path $workspace 'Kinovea.Services/Preferences/PreferenceTab.cs'),
        (Join-Path $workspace 'Kinovea/UserInterface/FormPreferences2.cs'),
        (Join-Path $workspace 'Kinovea/UserInterface/FormPreferences2.Designer.cs'),
        (Join-Path $workspace 'Tools/Validation/FluentPreview.cs')
    )
    & $compiler /nologo /target:exe "/out:$executable" /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Core.dll /r:System.Data.dll @sources
    if ($LASTEXITCODE -ne 0) { throw 'UI compilation failed' }
    if ($PreviewDirectory) { & $executable $PreviewDirectory } else { & $executable }
    if ($LASTEXITCODE -ne 0) {
        $exitCode = $LASTEXITCODE
        $hexCode = [BitConverter]::ToUInt32([BitConverter]::GetBytes([int]$exitCode), 0).ToString('X8')
        throw "UI validation failed (exit code $exitCode / 0x$hexCode)"
    }
} finally {
    # Exact, task-owned temporary files only.
    if (Test-Path (Join-Path $output 'Check.cs')) { Remove-Item -LiteralPath (Join-Path $output 'Check.cs') }
    if (Test-Path (Join-Path $output 'Check.exe')) { Remove-Item -LiteralPath (Join-Path $output 'Check.exe') }
    Remove-Item -LiteralPath $output
}
