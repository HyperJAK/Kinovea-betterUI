/*
Copyright © Joan Charmant 2008.
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

using System;
using System.Windows.Forms;
using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.Root
{
    /// <summary>
    /// This control contains the main splitter between the explorer panel and the general screen panel.
    /// </summary>
    public partial class SupervisorUserInterface : UserControl
    {
        #region Members
        private RootKernel rootKernel;
        private bool initializing = true;
        private readonly FluentPanel librarySurface = new FluentPanel { Dock = DockStyle.Fill };
        private readonly FluentPanel viewerSurface = new FluentPanel { Dock = DockStyle.Fill };
        #endregion

        #region Construction Destruction
        public SupervisorUserInterface(RootKernel _RootKernel)
        {
            rootKernel = _RootKernel;
            InitializeComponent();
            Padding = new Padding(12, 8, 12, 12);
            splitWorkSpace.BorderStyle = BorderStyle.None;
            splitWorkSpace.SplitterWidth = 12;
            splitWorkSpace.Panel1.Controls.Add(librarySurface);
            splitWorkSpace.Panel2.Controls.Add(viewerSurface);
            var libraryHeader = new Panel { Dock = DockStyle.Top, Height = 36 };
            var heading = new Label
            {
                Text = Kinovea.Root.Languages.RootLang.NavigationPane,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold)
            };
            libraryHeader.Controls.Add(heading);
            buttonCloseExplo.Image = null;
            buttonCloseExplo.Text = "\u00d7";
            buttonCloseExplo.Dock = DockStyle.Right;
            buttonCloseExplo.Width = 32;
            buttonCloseExplo.AccessibleName = Kinovea.ScreenManager.Languages.ScreenManagerLang.Generic_Close;
            libraryHeader.Controls.Add(buttonCloseExplo);
            librarySurface.Controls.Add(libraryHeader);
            
            splitWorkSpace.SplitterDistance = (int)(splitWorkSpace.Width * WindowManager.ActiveWindow.ExplorerSplitterRatio);
            splitWorkSpace.SplitterMoved += SplitWorkSpace_SplitterMoved;

            NotificationCenter.ToggleNavigationPane += NotificationCenter_ToggleNavigationPane;
        }
        #endregion

        #region Public methods
        public void PlugUI(UserControl navigationPanel, UserControl screenManager)
        {
            SuspendLayout();

            librarySurface.Controls.Add(navigationPanel);
            viewerSurface.Controls.Add(screenManager);
            navigationPanel.Dock = DockStyle.Fill;
            navigationPanel.BringToFront();
            screenManager.Dock = DockStyle.Fill;
            ResumeLayout();
            initializing = false;
        }

        /// <summary>
        /// Force collapse or uncollapse the navigation pane.
        /// This is used during initialization, when the user is manually toggling or when going into/out of full screen mode.
        /// savePreferences should only be true if this action is originating from the user.
        /// </summary>
        public void ToggleNavigationPanel(bool show, bool savePreferences)
        {
            splitWorkSpace.Panel1Collapsed = !show;

            if (savePreferences)
            {
                WindowManager.ActiveWindow.ExplorerVisible = show;
            }
        }
        #endregion

        private void SupervisorUserInterface_Load(object sender, EventArgs e)
        {
            bool show = LaunchSettingsManager.ExplorerVisible && WindowManager.ActiveWindow.ExplorerVisible;
            ToggleNavigationPanel(show, false);
        }

        private void NotificationCenter_ToggleNavigationPane(object sender, EventArgs e)
        {
            bool show = splitWorkSpace.Panel1Collapsed;
            ToggleNavigationPanel(show, true);
        }

        private void buttonCloseExplo_Click(object sender, EventArgs e)
        {
            ToggleNavigationPanel(false, true);
        }

        private void SplitWorkSpace_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (initializing)
                return;
            
            WindowManager.ActiveWindow.ExplorerSplitterRatio = (float)splitWorkSpace.SplitterDistance / splitWorkSpace.Width;
        }
    }
}
