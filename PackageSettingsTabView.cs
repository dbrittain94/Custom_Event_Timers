using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Runtime;

namespace roguishpanda.AB_Bauble_Farm
{
    public class PackageSettingsTabView : View
    {
        private BaubleFarmModule _BaubleFarmModule;
        private Panel _timerEventsTitlePanel;
        private Label _timerEventsTitleLabel;
        private Label _PackageLabelDisplay;
        private TextBox _PackageTextBox;
        private SettingEntry<string> _PackageSettingEntry;
        private SettingCollection _Settings;

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

            _PackageLabelDisplay = new Blish_HUD.Controls.Label
            {
                Text =  "Current Package",
                Size = new Point(120, 40),
                Location = new Point(110, 120),
                Font = GameService.Content.DefaultFont16,
                Visible = true,
                Parent = listSettingsPanel
            };
            _PackageTextBox = new Blish_HUD.Controls.TextBox
            {
                Size = new Point(120, 40),
                Location = new Point(230, 120),
                Font = GameService.Content.DefaultFont16,
                Visible = true,
                Parent = listSettingsPanel
            };

            SettingCollection SettingsCollection = _BaubleFarmModule._PackageSettingsCollection;

            _Settings = _BaubleFarmModule._settings;
            SettingCollection PackageSettings = _Settings.AddSubCollection("PackageSettings");
            if (PackageSettings != null)
            {
                _PackageSettingEntry = null;
                PackageSettings.TryGetSetting("CurrentPackageSelection", out _PackageSettingEntry);
                if (_PackageSettingEntry != null)
                {
                    _PackageSettingEntry.SettingChanged += PackageSettingEntry_SettingChanged;
                    _PackageTextBox.Text = _PackageSettingEntry.Value.ToString();
                }
            }

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
                Text = "Package Settings",
                Size = new Point(300, 40),
                Location = new Point(10, 0),
                Font = GameService.Content.DefaultFont16,
                TextColor = Color.White,
                Parent = _timerEventsTitlePanel
            };
        }
        private void PackageSettingEntry_SettingChanged(object sender, ValueChangedEventArgs<string> e)
        {
            _PackageTextBox.Text = _PackageSettingEntry.Value.ToString();
        }
    }
}