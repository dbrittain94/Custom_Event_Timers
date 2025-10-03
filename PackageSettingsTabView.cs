using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.ServiceModel.Description;
using System.Text.Json;
using System.Threading.Tasks;

namespace roguishpanda.AB_Bauble_Farm
{
    public class PackageSettingsTabView : View
    {
        private static readonly Logger Logger = Logger.GetLogger<BaubleFarmModule>();
        private BaubleFarmModule _BaubleFarmModule;
        private List<PackageData> _PackageData;
        private Texture2D _penUpdateTexture;
        private Panel _timerEventsTitlePanel;
        private Label _timerEventsTitleLabel;
        private Label _PackageLabelDisplay;
        private Label _PackageRenameLabel;
        private TextBox _PackageTextBox;
        private Label _DefaultPackageLabel;
        private Image _renamePenImage;
        private SettingEntry<string> _PackageSettingEntry;
        private SettingCollection _Settings;
        private StandardButton _ButtonRename;
        private Label _PackageLoadLabel;
        private Dropdown _PackageLoadDropdown;
        private StandardButton _ButtonLoad;
        private Label _PackageRenameAlert;
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
            _penUpdateTexture = _BaubleFarmModule.ContentsManager.GetTexture(@"png\pen.png");
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
                Text =  "Current Package:",
                Size = new Point(120, 40),
                Location = new Point(110, 110),
                Font = GameService.Content.DefaultFont16,
                Visible = true,
                Parent = listSettingsPanel
            };
            _DefaultPackageLabel = new Blish_HUD.Controls.Label
            {
                Size = new Point(120, 40),
                Location = new Point(270, 110),
                Font = GameService.Content.DefaultFont16,
                TextColor = Color.Gold,
                Visible = true,
                Parent = listSettingsPanel
            };
            _renamePenImage = new Blish_HUD.Controls.Image
            {
                Size = new Point(20, 20),
                Location = new Point(240, 120),
                Texture = _penUpdateTexture,
                Visible = true,
                Parent = listSettingsPanel
            };
            _PackageRenameLabel = new Blish_HUD.Controls.Label
            {
                Text = "Rename Package:",
                Size = new Point(120, 40),
                Location = new Point(110, 150),
                Font = GameService.Content.DefaultFont16,
                Visible = true,
                Parent = listSettingsPanel
            };
            _PackageTextBox = new Blish_HUD.Controls.TextBox
            {
                Size = new Point(120, 30),
                Location = new Point(260, 150),
                Font = GameService.Content.DefaultFont16,
                Visible = true,
                Parent = listSettingsPanel
            };
            _PackageTextBox.TextChanged += _PackageTextBox_TextChanged;
            _ButtonRename = new StandardButton()
            {
                Text = "Rename",
                Size = new Point(80, 30),
                Location = new Point(400, 150),
                Visible = false,
                Parent = listSettingsPanel
            };
            _ButtonRename.Click += _ButtonRename_Click;
            _PackageRenameAlert = new Label
            {
                Size = new Point(400, 40),
                Location = new Point(400, 150),
                Font = GameService.Content.DefaultFont16,
                Visible = false,
                Parent = listSettingsPanel
            };

            _PackageLoadLabel = new Blish_HUD.Controls.Label
            {
                Text = "Package Selection:",
                Size = new Point(140, 40),
                Location = new Point(110, 200),
                Font = GameService.Content.DefaultFont16,
                Visible = false,
                Parent = listSettingsPanel
            };
            _PackageLoadDropdown = new Blish_HUD.Controls.Dropdown
            {
                Size = new Point(160, 40),
                Location = new Point(260, 200),
                Visible = false,
                Parent = listSettingsPanel
            };
            Task task = LoadPackageDropdownOptions();
            _PackageLoadDropdown.ValueChanged += _PackageLoadDropdown_ValueChanged;
            _ButtonLoad = new StandardButton()
            {
                Text = "Load",
                Size = new Point(80, 30),
                Location = new Point(440, 200),
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
                    _DefaultPackageLabel.Text = _PackageSettingEntry.Value.ToString();
                }
            }
            _ButtonRename.Visible = false;
        }
        private async Task LoadPackageDropdownOptions()
        {
            try
            {
                _PackageData = new List<PackageData>();
                string moduleDir = _BaubleFarmModule.DirectoriesManager.GetFullDirectoryPath("Shiny_Baubles");
                string jsonFilePath = Path.Combine(moduleDir, "Package_Defaults.json");
                using (StreamReader reader = new StreamReader(jsonFilePath))
                {
                    string jsonContent = await reader.ReadToEndAsync();
                    _PackageData = JsonSerializer.Deserialize<List<PackageData>>(jsonContent, _jsonOptions);
                }

                for (int i = 0; i < _PackageData.Count; i++)
                {
                    string PackageName = _PackageData[i].PackageName;
                    _PackageLoadDropdown.Items.Add(PackageName);
                    if (PackageName != _DefaultPackageLabel.Text)
                    {
                        _PackageLoadDropdown.Items.Add(PackageName);
                    }
                }

                if (_PackageLoadDropdown.Items.Count > 0)
                {
                    _PackageLoadLabel.Visible = true;
                    _PackageLoadDropdown.Visible = true;
                    _ButtonLoad.Visible = true;
                }
                else
                {
                    _PackageLoadLabel.Visible = false;
                    _PackageLoadDropdown.Visible = false;
                    _ButtonLoad.Visible = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load packagedropdowns: {ex.Message}");
            }
        }

        private void _PackageLoadDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {

        }

        private void _ButtonRename_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _ButtonRename.Visible = false;
            var package = _PackageData.FirstOrDefault(p => p.PackageName == _DefaultPackageLabel.Text);
            if (package != null)
            {
                package.PackageName = _PackageTextBox.Text;
                _PackageSettingEntry.Value = _PackageTextBox.Text;
                _DefaultPackageLabel.Text = _PackageTextBox.Text;
            }
            RenamePackageJsonUpdate();
        }

        private void _PackageTextBox_TextChanged(object sender, EventArgs e)
        {
            _ButtonRename.Visible = true;
            _PackageRenameAlert.Visible = false;
        }
        private void RenamePackageJsonUpdate()
        {
            string moduleDir = _BaubleFarmModule.DirectoriesManager.GetFullDirectoryPath("Shiny_Baubles");
            string jsonFilePath = Path.Combine(moduleDir, "Package_Defaults.json");
            try
            {
                string jsonContent = JsonSerializer.Serialize(_PackageData, _jsonOptions);
                File.WriteAllText(jsonFilePath, jsonContent);
                _PackageRenameAlert.Text = "Events have been saved! Restart Module to reset timer UI!";
                _PackageRenameAlert.Visible = true;
                _PackageRenameAlert.TextColor = Color.LimeGreen;
                //Logger.Info($"Saved {_eventDataList.Count} events to {_jsonFilePath}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to save JSON file: {ex.Message}");
            }
        }
    }
}