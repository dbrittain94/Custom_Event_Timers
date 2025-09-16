using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace roguishpanda.AB_Bauble_Farm
{
    public class ListSettingsTabView : View
    {
        private BaubleFarmModule _BaubleFarmModule;
        private ViewContainer _settingsViewContainer;
        private Panel _timerEventsTitlePanel;
        private Label _timerEventsTitleLabel;
        private Label _LowTimerLabelDisplay;
        private Label _OpacityLabelDisplay;
        private SettingEntry<int> _LowTimerSettingEntry;
        private SettingEntry<float> _OpacityDefaultSettingEntry;
        private SettingCollection _MainSettings;

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
                //ShowBorder = true,
                Padding = new Thickness(50, 50, 50, 50),
                Location = new Point(100, 100),
                Size = new Point(500, 300)
            };
            var settingsView = new SettingsView(SettingsCollection);
            _settingsViewContainer.Show(settingsView);

            _LowTimerLabelDisplay = new Blish_HUD.Controls.Label
            {
                Size = new Point(100, 40),
                Location = new Point(580, 125),
                Font = GameService.Content.DefaultFont16,
                Parent = listSettingsPanel
            };
            _OpacityLabelDisplay = new Blish_HUD.Controls.Label
            {
                Size = new Point(100, 40),
                Location = new Point(580, 150),
                Font = GameService.Content.DefaultFont16,
                Parent = listSettingsPanel
            };

            _MainSettings = _BaubleFarmModule._settings;
            SettingCollection TimerCollector = _MainSettings.AddSubCollection("MainSettings");
            if (TimerCollector != null)
            {
                _LowTimerSettingEntry = null;
                TimerCollector.TryGetSetting("LowTimerDefaultTimer", out _LowTimerSettingEntry);
                if (_LowTimerSettingEntry != null)
                {
                    _LowTimerSettingEntry.SettingChanged += LowTimerSettingEntry_SettingChanged;
                    _LowTimerLabelDisplay.Text = _LowTimerSettingEntry.Value.ToString() + " seconds";
                }
                _OpacityDefaultSettingEntry = null;
                TimerCollector.TryGetSetting("OpacityDefault", out _OpacityDefaultSettingEntry);
                if (_OpacityDefaultSettingEntry != null)
                {
                    _OpacityDefaultSettingEntry.SettingChanged += OpacityDefaultSettingEntry_SettingChanged;
                    _OpacityLabelDisplay.Text = Math.Round(_OpacityDefaultSettingEntry.Value * 100, 0) + "%";
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
                Text = "General Settings",
                Size = new Point(300, 40),
                Location = new Point(10, 0),
                Font = GameService.Content.DefaultFont16,
                TextColor = Color.White,
                Parent = _timerEventsTitlePanel
            };
        }

        private void OpacityDefaultSettingEntry_SettingChanged(object sender, ValueChangedEventArgs<float> e)
        {
            _OpacityLabelDisplay.Text = Math.Round(_OpacityDefaultSettingEntry.Value * 100, 0) + "%";
        }
        private void LowTimerSettingEntry_SettingChanged(object sender, ValueChangedEventArgs<int> e)
        {
            _LowTimerLabelDisplay.Text = _LowTimerSettingEntry.Value.ToString() + " seconds";
        }
    }
}