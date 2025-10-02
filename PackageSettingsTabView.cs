using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text.Json;

namespace roguishpanda.AB_Bauble_Farm
{
    public class PackageSettingsTabView : View
    {
        private static readonly Logger Logger = Logger.GetLogger<BaubleFarmModule>();
        private BaubleFarmModule _BaubleFarmModule;
        private List<PackageData> _PackageData;
        private Panel _timerEventsTitlePanel;
        private Label _timerEventsTitleLabel;
        private Label _PackageLabelDisplay;
        private TextBox _PackageTextBox;
        private SettingEntry<string> _PackageSettingEntry;
        private SettingCollection _Settings;
        private StandardButton _ButtonRename;
        private Label _PackageAlert;
        public readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true // Makes JSON human-readable
        };

        protected override void Build(Container buildPanel)
        {
            _BaubleFarmModule = BaubleFarmModule.ModuleInstance;
            _PackageData = new List<PackageData>(_BaubleFarmModule._PackageData);

            Blish_HUD.Controls.Panel listSettingsPanel = new Blish_HUD.Controls.Panel
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Size.X + 200, buildPanel.ContentRegion.Size.Y + 300), // Match the panel to the content region
                Location = new Point(buildPanel.ContentRegion.Location.X, buildPanel.ContentRegion.Location.Y - 35), // Align with content region
                BackgroundTexture = BaubleFarmModule.ModuleInstance._asyncTimertexture
            };

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
            _PackageTextBox.TextChanged += _PackageTextBox_TextChanged;
            _ButtonRename = new StandardButton()
            {
                Text = "Rename",
                Size = new Point(140, 40),
                Location = new Point(360, 120),
                Visible = false,
                Parent = listSettingsPanel
            };
            _ButtonRename.Click += _ButtonRename_Click;
            _PackageAlert = new Label
            {
                Size = new Point(400, 40),
                Location = new Point(250, 60),
                Font = GameService.Content.DefaultFont16,
                Visible = false,
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
                    _PackageTextBox.Text = _PackageSettingEntry.Value.ToString();
                }
            }
            _ButtonRename.Visible = false;
        }

        private void _ButtonRename_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _ButtonRename.Visible = false;
            var package = _PackageData.FirstOrDefault(p => p.PackageName == _PackageSettingEntry.Value);
            if (package != null)
            {
                package.PackageName = _PackageTextBox.Text;
                _PackageSettingEntry.Value = _PackageTextBox.Text;
            }
            RenamePackageJsonUpdate();
        }

        private void _PackageTextBox_TextChanged(object sender, EventArgs e)
        {
            _ButtonRename.Visible = true;
        }
        private void RenamePackageJsonUpdate()
        {
            string moduleDir = _BaubleFarmModule.DirectoriesManager.GetFullDirectoryPath("Shiny_Baubles");
            string jsonFilePath = Path.Combine(moduleDir, "Package_Defaults.json");
            try
            {
                string jsonContent = JsonSerializer.Serialize(_PackageData, _jsonOptions);
                File.WriteAllText(jsonFilePath, jsonContent);
                _PackageAlert.Text = "Events have been saved! Restart Module to reset timer UI";
                _PackageAlert.Visible = true;
                _PackageAlert.TextColor = Color.LimeGreen;
                //Logger.Info($"Saved {_eventDataList.Count} events to {_jsonFilePath}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to save JSON file: {ex.Message}");
            }
        }
    }
}