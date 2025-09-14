using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;

namespace roguishpanda.AB_Bauble_Farm
{
    public class ListSettingsTabView : View
    {
        private BaubleFarmModule _BaubleFarmModule;
        private ViewContainer _settingsViewContainer;

        protected override void Build(Container buildPanel)
        {
            _BaubleFarmModule = BaubleFarmModule.ModuleInstance;

            Blish_HUD.Controls.Panel listSettingsPanel = new Blish_HUD.Controls.Panel
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Size.X + 500, buildPanel.ContentRegion.Size.Y + 535), // Match the panel to the content region
                Location = new Point(buildPanel.ContentRegion.Location.X, buildPanel.ContentRegion.Location.Y - 35), // Align with content region
                BackgroundTexture = BaubleFarmModule.ModuleInstance._asyncTimertexture
            };

            SettingCollection SettingsCollection = _BaubleFarmModule._SettingsCollection;
            _settingsViewContainer = new ViewContainer
            {
                Parent = listSettingsPanel,
                Location = new Point(100, 100),
                Size = new Point(700, 700)
            };
            var settingsView = new SettingsView(SettingsCollection);
            _settingsViewContainer.Show(settingsView);
        }
    }
}