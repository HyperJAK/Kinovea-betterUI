using System.Drawing;
using System.Windows.Forms;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    partial class FormConfigureExportImageSequence
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            Font = new Font("Segoe UI", 9F);
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(540, 390);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;

            lblHeading = new Label
            {
                AutoSize = false, Location = new Point(24, 22), Size = new Size(492, 42),
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            grpboxConfig = new FluentGroupBox
            {
                Location = new Point(24, 88), Size = new Size(492, 208),
                TabIndex = 0, TabStop = false
            };
            lblFrameDecimation = new Label
            {
                AutoSize = false, Location = new Point(24, 38), Size = new Size(444, 26),
                TextAlign = ContentAlignment.MiddleLeft
            };
            lblTotalFrames = new Label
            {
                AutoSize = false, Location = new Point(24, 130), Size = new Size(444, 26),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            lblTimeDecimation = new Label
            {
                AutoSize = false, Location = new Point(24, 162), Size = new Size(444, 24),
                TextAlign = ContentAlignment.MiddleLeft
            };
            lblQualityHint = new Label
            {
                AutoSize = false, Location = new Point(24, 298), Size = new Size(492, 26),
                TextAlign = ContentAlignment.MiddleLeft
            };
            btnOK = new FluentButton
            {
                DialogResult = DialogResult.OK, Location = new Point(280, 334), Size = new Size(112, 34),
                TabIndex = 1, Accent = true
            };
            btnCancel = new FluentButton
            {
                DialogResult = DialogResult.Cancel, Location = new Point(404, 334), Size = new Size(112, 34),
                TabIndex = 2
            };
            grpboxConfig.Controls.AddRange(new Control[] { lblFrameDecimation, lblTotalFrames, lblTimeDecimation });
            Controls.AddRange(new Control[] { lblHeading, grpboxConfig, lblQualityHint, btnOK, btnCancel });
            AcceptButton = btnOK;
            CancelButton = btnCancel;
            Name = "FormConfigureExportImageSequence";
            ResumeLayout(false);
        }

        private GroupBox grpboxConfig;
        private Label lblHeading;
        private Label lblQualityHint;
        private Label lblTotalFrames;
        private Label lblTimeDecimation;
        private Label lblFrameDecimation;
        private Button btnOK;
        private Button btnCancel;
    }
}