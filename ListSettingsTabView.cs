using Blish_HUD;
using Blish_HUD.Content;
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
        private Panel _timerEventsTitlePanel;
        private Label _timerEventsTitleLabel;

        protected override void Build(Container buildPanel)
        {
            _BaubleFarmModule = BaubleFarmModule.ModuleInstance;

            Blish_HUD.Controls.Panel listSettingsPanel = new Blish_HUD.Controls.Panel
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Size.X + 200, buildPanel.ContentRegion.Size.Y + 300), // Match the panel to the content region
                Location = new Point(buildPanel.ContentRegion.Location.X, buildPanel.ContentRegion.Location.Y - 35), // Align with content region
                BackgroundTexture = BaubleFarmModule.ModuleInstance._asyncTimertexture
            };

            SettingCollection SettingsCollection = _BaubleFarmModule._SettingsCollection;
            _settingsViewContainer = new ViewContainer
            {
                Parent = listSettingsPanel,
                ShowBorder = true,
                Padding = new Thickness(50, 50, 50, 50),
                Location = new Point(100, 100),
                Size = new Point(700, 300)
            };
            var settingsView = new SettingsView(SettingsCollection);
            _settingsViewContainer.Show(settingsView);

            AsyncTexture2D TitleTexture = AsyncTexture2D.FromAssetId(1234872);
            _timerEventsTitlePanel = new Blish_HUD.Controls.Panel
            {
                Parent = listSettingsPanel,
                Size = new Point(700, 40),
                Location = new Point(102, 60),
                BackgroundTexture = TitleTexture,
            };
            _timerEventsTitleLabel = new Blish_HUD.Controls.Label
            {
                Text = "General Settings",
                Size = new Point(300, 40),
                Location = new Point(10, 0),
                Font = GameService.Content.DefaultFont16,
                TextColor = Color.White,
                Parent = _timerEventsTitlePanel
            };
        }
    }
}