using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
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
        private Panel _timerEventsTitlePanel;
        private Label _timerEventsTitleLabel;
        private Label _PackageLabelDisplay;
        private Label _PackageCreateLabel;
        private TextBox _PackageCreateTextBox;
        private StandardButton _ButtonCreate;
        private TextBox _PackageRenameTextBox;
        private Label _DefaultPackageLabel;
        private SettingEntry<string> _PackageSettingEntry;
        private SettingCollection _Settings;
        private StandardButton _ButtonSaveRename;
        private StandardButton _ButtonLoadRename;
        private Label _PackageLoadLabel;
        private Dropdown _PackageLoadDropdown;
        private StandardButton _ButtonLoad;
        private Label _PackageCreateAlert;
        private StandardButton _buttonRestartModule;
        private StandardButton _ButtonDelete;
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
            _PackageRenameTextBox = new Blish_HUD.Controls.TextBox
            {
                Size = new Point(120, 30),
                Location = new Point(260, 110),
                Font = GameService.Content.DefaultFont16,
                Visible = false,
                Parent = listSettingsPanel
            };
            _ButtonLoadRename = new Blish_HUD.Controls.StandardButton
            {
                Text = "Rename",
                Size = new Point(80, 30),
                Location = new Point(440, 110),
                Visible = true,
                Parent = listSettingsPanel
            };
            _ButtonLoadRename.Click += _ButtonLoadRename_Click; ;
            _ButtonSaveRename = new StandardButton()
            {
                Text = "Save",
                Size = new Point(80, 30),
                Location = new Point(440, 110),
                Visible = false,
                Parent = listSettingsPanel
            };
            _ButtonSaveRename.Click += _ButtonSaveRename_Click;
            _PackageCreateLabel = new Blish_HUD.Controls.Label
            {
                Text = "Create Package:",
                Size = new Point(120, 40),
                Location = new Point(110, 150),
                Font = GameService.Content.DefaultFont16,
                Visible = true,
                Parent = listSettingsPanel
            };
            _PackageCreateTextBox = new Blish_HUD.Controls.TextBox
            {
                Size = new Point(120, 30),
                Location = new Point(260, 150),
                Font = GameService.Content.DefaultFont16,
                Visible = true,
                Parent = listSettingsPanel
            };
            _ButtonCreate = new StandardButton()
            {
                Text = "Create",
                Size = new Point(80, 30),
                Location = new Point(440, 150),
                Visible = true,
                Parent = listSettingsPanel
            };
            _ButtonCreate.Click += _ButtonCreate_Click;
            _PackageCreateAlert = new Label
            {
                Size = new Point(400, 40),
                Location = new Point(530, 150),
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
            _ButtonLoad = new StandardButton()
            {
                Text = "Load",
                Size = new Point(80, 30),
                Location = new Point(440, 200),
                Visible = false,
                Parent = listSettingsPanel
            };
            _ButtonLoad.Click += _ButtonLoad_Click;
            _ButtonDelete = new StandardButton()
            {
                Text = "Delete",
                Size = new Point(80, 30),
                Location = new Point(530, 200),
                Visible = false,
                Parent = listSettingsPanel
            };
            _ButtonDelete.Click += _ButtonDelete_Click;

            _buttonRestartModule = new StandardButton
            {
                Text = "Restart Module",
                Size = new Point(200, 40),
                Location = new Point(110, 350),
                Visible = false,
                Parent = listSettingsPanel
            };
            _buttonRestartModule.Click += RestartModule_Click;

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
            _ButtonSaveRename.Visible = false;
        }

        private void _ButtonDelete_Click(object sender, MouseEventArgs e)
        {
            _PackageData = _PackageData.Where(pd => pd.PackageName != _PackageLoadDropdown.SelectedItem).ToList();
            SavePackageJsonUpdate();
            Task task = LoadPackageDropdownOptions();
        }
        private void _ButtonLoad_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _PackageSettingEntry.Value = _PackageLoadDropdown.SelectedItem.ToString();
            _DefaultPackageLabel.Text = _PackageLoadDropdown.SelectedItem.ToString();
            Task task = LoadPackageDropdownOptions();
            _BaubleFarmModule.Restart();
        }

        private void _ButtonCreate_Click(object sender, MouseEventArgs e)
        {
            // Check parameters
            if (_PackageCreateTextBox.Text.Length < 4)
            {
                _PackageCreateAlert.Text = "* 4 characters mininimum required to create new package";
                _PackageCreateAlert.Visible = true;
                _PackageCreateAlert.TextColor = Color.Red;
                return;
            }
            foreach (var Packages in _PackageData)
            {
                if (Packages.PackageName == _PackageCreateTextBox.Text)
                {
                    _PackageCreateAlert.Text = "* This package name already exists";
                    _PackageCreateAlert.Visible = true;
                    _PackageCreateAlert.TextColor = Color.Red;
                    return;
                }
            }
            _PackageCreateAlert.Text = "Package has been created!";
            _PackageCreateAlert.Visible = true;
            _PackageCreateAlert.TextColor = Color.LimeGreen;

            // Create a new PackageData instance
            PackageData newPackage = new PackageData
            {
                PackageName = _PackageCreateTextBox.Text,
                StaticDetailData = new List<StaticDetailData>(), // Initialize empty list or add items
                TimerDetailData = new List<TimerDetailData>()    // Initialize empty list or add items
            };

            _PackageData.Add(newPackage);
            SavePackageJsonUpdate();
            Task task = LoadPackageDropdownOptions();
            //_buttonRestartModule.Visible = true;
        }

        private void _ButtonLoadRename_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _DefaultPackageLabel.Visible = false;
            _PackageRenameTextBox.Visible = true;
            _ButtonLoadRename.Visible = false;
            _ButtonSaveRename.Visible = true;
            _PackageCreateAlert.Visible = false;
            _PackageRenameTextBox.Text = _DefaultPackageLabel.Text;
        }

        private async Task LoadPackageDropdownOptions()
        {
            try
            {
                _PackageLoadDropdown.Items.Clear();
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
                    //_PackageLoadDropdown.Items.Add(PackageName);
                    if (PackageName != _DefaultPackageLabel.Text)
                    {
                        _PackageLoadDropdown.Items.Add(PackageName);
                    }
                }

                if (_PackageLoadDropdown.Items.Count > 0)
                {
                    _PackageLoadDropdown.SelectedItem = _PackageLoadDropdown.Items[0];
                    _PackageLoadLabel.Visible = true;
                    _PackageLoadDropdown.Visible = true;
                    _ButtonLoad.Visible = true;
                    _ButtonDelete.Visible = true;
                }
                else
                {
                    _PackageLoadLabel.Visible = false;
                    _PackageLoadDropdown.Visible = false;
                    _ButtonLoad.Visible = false;
                    _ButtonDelete.Visible = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load packagedropdowns: {ex.Message}");
            }
        }

        private void _ButtonSaveRename_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _ButtonSaveRename.Visible = false;
            _DefaultPackageLabel.Visible = true;
            _PackageCreateAlert.Visible = false;
            _PackageRenameTextBox.Visible = false;
            _ButtonLoadRename.Visible = true;
            _ButtonSaveRename.Visible = false;
            var package = _PackageData.FirstOrDefault(p => p.PackageName == _DefaultPackageLabel.Text);
            if (package != null)
            {
                package.PackageName = _PackageRenameTextBox.Text;
                _PackageSettingEntry.Value = _PackageRenameTextBox.Text;
                _DefaultPackageLabel.Text = _PackageRenameTextBox.Text;
            }
            SavePackageJsonUpdate();
            Task task = LoadPackageDropdownOptions();
            _buttonRestartModule.Visible = true;
        }
        private void SavePackageJsonUpdate()
        {
            string moduleDir = _BaubleFarmModule.DirectoriesManager.GetFullDirectoryPath("Shiny_Baubles");
            string jsonFilePath = Path.Combine(moduleDir, "Package_Defaults.json");
            try
            {
                string jsonContent = JsonSerializer.Serialize(_PackageData, _jsonOptions);
                File.WriteAllText(jsonFilePath, jsonContent);
                //Logger.Info($"Saved {_eventDataList.Count} events to {_jsonFilePath}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to save JSON file: {ex.Message}");
            }
        }
        private void RestartModule_Click(object sender, MouseEventArgs e)
        {
            _BaubleFarmModule.Restart();
            _buttonRestartModule.Visible = false;
            _PackageCreateAlert.Visible = false;
        }
    }
}