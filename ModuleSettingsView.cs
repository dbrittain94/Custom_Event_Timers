using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using System;

namespace roguishpanda.AB_Bauble_Farm
{
    public class ModuleSettingsView : View
    {
        protected override void Build(Container buildPanel)
        {
            StandardButton btnSettings = new StandardButton
            {
                Parent = buildPanel,
                Text = "Open Settings",
                Size = new Point(100, 30)
            };
            btnSettings.Location = new Point(((buildPanel.Size.X - btnSettings.Size.X) / 2), ((buildPanel.Size.Y - btnSettings.Size.Y) / 2));

            btnSettings.Click += OpenSettings_Click;
        }
        private void OpenSettings_Click(object sender, MouseEventArgs e)
        {
            BaubleFarmModule module = BaubleFarmModule.ModuleInstance;
            if (module._SettingsWindow.Visible == false)
            {
                module._SettingsWindow.Show();
            }
            else
            {
                module._SettingsWindow.Hide();
            }
        }
    }
}