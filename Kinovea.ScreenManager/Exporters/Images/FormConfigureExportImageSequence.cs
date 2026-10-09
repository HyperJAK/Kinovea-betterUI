using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    public partial class FormConfigureExportImageSequence : Kinovea.Services.ThemedForm
    {
        /// <summary>
        /// Frame interval to export at, in timestamps.
        /// </summary>
        public double IntervalTimestamps
        {
            get { return videoInfo.AverageTimeStampsPerFrame * decimationFrames; }
        }

        /// <summary>
        /// Number of frames we will export.
        /// </summary>
        public int RemainingFrames
        {
            get { return 1 + (totalFrames - 1) / decimationFrames; }
        }


        private VideoInfo videoInfo;
        private int totalFrames;
        private int decimationFrames = 1;
        private double frameInterval;
        private NumericUpDown intervalInput;

        public FormConfigureExportImageSequence(PlayerScreen player)
        {
            var metadata = player.FrameServer.Metadata;
            videoInfo = player.FrameServer.VideoReader.Info;
            frameInterval = metadata.BaselineFrameInterval;
            totalFrames = Math.Max(1, (int)((metadata.SelectionEnd - metadata.SelectionStart) / metadata.AverageTimeStampsPerFrame) + 1);

            InitializeComponent();
            InitializeCulture();
            trkDecimate.Visible = false;
            intervalInput = new NumericUpDown
            {
                Minimum = 1,
                Maximum = totalFrames,
                Value = 1,
                Location = new Point(25, 40),
                Size = new Size(140, 28),
                TabIndex = 0,
                ThousandsSeparator = true,
                AccessibleName = ScreenManagerLang.formConfigureExport_ImageSequence
            };
            intervalInput.ValueChanged += (sender, args) =>
            {
                decimationFrames = (int)intervalInput.Value;
                UpdateLabels();
            };
            grpboxConfig.Controls.Add(intervalInput);
            UpdateLabels();
        }
        private void InitializeCulture()
        {
            this.Text = ScreenManagerLang.formConfigureExport_ImageSequence;

            grpboxConfig.Text = ScreenManagerLang.Generic_Configuration;
            btnOK.Text = ScreenManagerLang.Generic_Save;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }

        private void UpdateLabels()
        {
            int remainingFrames = RemainingFrames;

            if (decimationFrames == 1)
                lblFrameDecimation.Text = ScreenManagerLang.dlgRafaleExport_ExportAll;
            else
                lblFrameDecimation.Text = string.Format(ScreenManagerLang.dlgRafaleExport_ExportFrameDecimation, decimationFrames);

            double decimationTime = decimationFrames * frameInterval;
            if (decimationTime >= 1000)
            {
                decimationTime /= 1000;
                lblTimeDecimation.Text = string.Format(ScreenManagerLang.dlgRafaleExport_ExportTimeDecimationSeconds, decimationTime);
            }
            else
            {
                lblTimeDecimation.Text = string.Format(ScreenManagerLang.dlgRafaleExport_ExportTimeDecimationMilliseconds, decimationTime);
            }

            lblTotalFrames.Text = string.Format(ScreenManagerLang.dlgRafaleExport_LabelTotalFrames, remainingFrames);
        }

        private void trkDecimate_ValueChanged(object sender, EventArgs e)
        {
            // Retained for the designer; exact intervals use intervalInput.
            UpdateLabels();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {

        }
    }
}
