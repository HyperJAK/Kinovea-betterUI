#region License
/*
Copyright © Joan Charmant 2011.
jcharmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Kinovea.Services;

namespace Kinovea.Root
{
    /// <summary>
    /// PreferencePanelButtton.
    /// A simple "image + label" control to be used for preferences pages.
    /// </summary>
    public partial class PreferencePanelButtton : UserControl
    {
        #region Properties
        public IPreferencePanel PreferencePanel
        {
            get { return preferencePanel; }
        }
        #endregion
        
        #region Members
        private bool isSelected;
        private IPreferencePanel preferencePanel;
        private static readonly Font fontLabel = new Font("Segoe UI", 10, FontStyle.Regular);
        #endregion
        
        #region Construction
        public PreferencePanelButtton(IPreferencePanel preferencePanel)
        {
            InitializeComponent();
            this.preferencePanel = preferencePanel;
            Size = new Size(184, 44);
            TabStop = true;
            AccessibleRole = AccessibleRole.PageTab;
            AccessibleName = preferencePanel.Description;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            KeyDown += (sender, args) => { if (args.KeyCode == Keys.Enter || args.KeyCode == Keys.Space) { OnClick(EventArgs.Empty); args.Handled = true; } };
        }
        #endregion
        
        #region Public Methods
        public void SetSelected(bool isSelected)
        {
            this.isSelected = isSelected;
            this.BackColor = isSelected ? DarkTheme.Selection : DarkTheme.Surface;
            Invalidate();
        }
        #endregion
        
        #region Private Methods
        private void preferencePanelButtton_Paint(object sender, PaintEventArgs e)
        {
            FluentGeometry.FillBackground(e.Graphics, ClientRectangle, Parent == null ? DarkTheme.Background : Parent.BackColor);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var path = FluentGeometry.Round(new RectangleF(0, 0, Width - 1, Height - 1), 4))
            using (var brush = new SolidBrush(isSelected ? DarkTheme.Selection : BackColor))
                e.Graphics.FillPath(brush, path);
            if (isSelected)
                using (var brush = new SolidBrush(DarkTheme.Accent))
                    e.Graphics.FillRectangle(brush, 0, 12, 3, Height - 24);
            if (preferencePanel.Icon != null)
                e.Graphics.DrawImage(preferencePanel.Icon, new Rectangle(14, (Height - 20) / 2, 20, 20));
            TextRenderer.DrawText(e.Graphics, preferencePanel.Description, fontLabel,
                new Rectangle(preferencePanel.Icon == null ? 16 : 44, 0, Width - (preferencePanel.Icon == null ? 24 : 52), Height), DarkTheme.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            if (Focused && ShowFocusCues)
                ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -4, -4));
        }
        private void PreferencePanelButttonMouseEnter(object sender, EventArgs e)
        {
            if(!isSelected)
                this.BackColor = DarkTheme.Hover;
            Invalidate();
        }
        void PreferencePanelButttonMouseLeave(object sender, EventArgs e)
        {
            if(!isSelected)
                this.BackColor = DarkTheme.Surface;
            Invalidate();
        }
        #endregion
    }
}
