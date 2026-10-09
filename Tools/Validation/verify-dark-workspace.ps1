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
    public static class ScreenManagerLang {
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
class Check {
    static int checks;
    static void Assert(bool value, string message) { checks++; if (!value) throw new Exception(message); }
    [STAThread] static void Main() {
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
        Console.WriteLine("Passed " + checks + " UI and extraction checks.");
    }
}
'@
    $stubPath = Join-Path $output 'Check.cs'
    [IO.File]::WriteAllText($stubPath, $stub)
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
    $executable = Join-Path $output 'Check.exe'
    & $compiler /nologo /target:exe "/out:$executable" /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Core.dll /r:System.Data.dll $stubPath (Join-Path $workspace 'Kinovea.Services/WinForms/DarkTheme.cs') (Join-Path $workspace 'Kinovea.ScreenManager/Exporters/Images/FormConfigureExportImageSequence.cs') (Join-Path $workspace 'Kinovea.ScreenManager/Exporters/Images/FormConfigureExportImageSequence.Designer.cs')
    if ($LASTEXITCODE -ne 0) { throw 'UI compilation failed' }
    & $executable
    if ($LASTEXITCODE -ne 0) { throw 'UI validation failed' }
} finally {
    # Exact, task-owned temporary files only.
    if (Test-Path (Join-Path $output 'Check.cs')) { Remove-Item -LiteralPath (Join-Path $output 'Check.cs') }
    if (Test-Path (Join-Path $output 'Check.exe')) { Remove-Item -LiteralPath (Join-Path $output 'Check.exe') }
    Remove-Item -LiteralPath $output
}
