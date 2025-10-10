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
using System.Reflection;
using System.Runtime;
using System.ServiceModel.Description;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace roguishpanda.AB_Bauble_Farm
{
    public class PackageSettingsTabView : Blish_HUD.Graphics.UI.View
    {
        private static readonly Logger Logger = Logger.GetLogger<BaubleFarmModule>();
        private BaubleFarmModule _BaubleFarmModule;
        private List<PackageData> _PackageData;
        private List<PackageData> _CommunityPackageData;
        private Blish_HUD.Controls.Panel _timerEventsTitlePanel;
        private Blish_HUD.Controls.Label _timerEventsTitleLabel;
        private Blish_HUD.Controls.Label _PackageLabelDisplay;
        private Blish_HUD.Controls.Label _PackageRenameAlert;
        private Blish_HUD.Controls.Label _PackageCreateLabel;
        private Blish_HUD.Controls.TextBox _PackageCreateTextBox;
        private StandardButton _ButtonCreate;
        private Blish_HUD.Controls.TextBox _PackageRenameTextBox;
        private Blish_HUD.Controls.Label _DefaultPackageLabel;
        private SettingEntry<string> _PackageSettingEntry;
        private SettingCollection _Settings;
        private StandardButton _ButtonSaveRename;
        private StandardButton _ButtonLoadRename;
        private Blish_HUD.Controls.Label _PackageLoadPersonalLabel;
        private Dropdown _PackageLoadPersonalDropdown;
        private StandardButton _ButtonLoadPersonal;
        private Blish_HUD.Controls.Label _PackageCreateAlert;
        private Blish_HUD.Controls.Label _PackageLoadCommunityLabel;
        private Dropdown _PackageLoadCommunityDropdown;
        private Blish_HUD.Controls.Label _PackageLoadPackageAlert;
        private StandardButton _ButtonDeletePersonal;
        private StandardButton _ButtonLoadCommunity;
        private Blish_HUD.Controls.Label _PackageLoadCommunityAlert;
        private StandardButton _ButtonCopyClipboard;
        private StandardButton _ButtonImportClipboard;
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

            _ButtonCopyClipboard = new StandardButton()
            {
                Text = "Copy to Clipboard",
                Size = new Point(140, 30),
                Location = new Point(110, 110),
                Parent = listSettingsPanel
            };
            _ButtonCopyClipboard.Click += _ButtonCopyClipboard_Click;
            _ButtonImportClipboard = new StandardButton()
            {
                Text = "Import from Clipboard",
                Size = new Point(160, 30),
                Location = new Point(270, 110),
                Parent = listSettingsPanel
            };
            _ButtonImportClipboard.Click += _ButtonImportClipboard_Click;
            _PackageLoadPackageAlert = new Blish_HUD.Controls.Label
            {
                Size = new Point(500, 40),
                Location = new Point(440, 105),
                Font = GameService.Content.DefaultFont16,
                Visible = false,
                Parent = listSettingsPanel
            };
            _PackageLabelDisplay = new Blish_HUD.Controls.Label
            {
                Text = "Current Package:",
                Size = new Point(160, 40),
                Location = new Point(110, 150),
                Font = GameService.Content.DefaultFont16,
                HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Right,
                Visible = true,
                Parent = listSettingsPanel
            };
            _DefaultPackageLabel = new Blish_HUD.Controls.Label
            {
                Size = new Point(120, 40),
                Location = new Point(290, 150),
                Font = GameService.Content.DefaultFont16,
                //TextColor = Color.Gold,
                Visible = true,
                Parent = listSettingsPanel
            };
            _PackageRenameTextBox = new Blish_HUD.Controls.TextBox
            {
                Size = new Point(150, 30),
                Location = new Point(280, 150),
                Font = GameService.Content.DefaultFont16,
                Visible = false,
                Parent = listSettingsPanel
            };
            _ButtonLoadRename = new Blish_HUD.Controls.StandardButton
            {
                Text = "Rename",
                Size = new Point(80, 30),
                Location = new Point(460, 150),
                Visible = true,
                Parent = listSettingsPanel
            };
            _ButtonLoadRename.Click += _ButtonLoadRename_Click; ;
            _ButtonSaveRename = new StandardButton()
            {
                Text = "Save",
                Size = new Point(80, 30),
                Location = new Point(460, 150),
                Visible = false,
                Parent = listSettingsPanel
            };
            _ButtonSaveRename.Click += _ButtonSaveRename_Click;
            _PackageRenameAlert = new Blish_HUD.Controls.Label
            {
                Size = new Point(400, 40),
                Location = new Point(550, 150),
                Font = GameService.Content.DefaultFont16,
                Visible = false,
                Parent = listSettingsPanel
            };
            _PackageCreateLabel = new Blish_HUD.Controls.Label
            {
                Text = "Create Package:",
                Size = new Point(160, 40),
                Location = new Point(110, 200),
                Font = GameService.Content.DefaultFont16,
                HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Right,
                Visible = true,
                Parent = listSettingsPanel
            };
            _PackageCreateTextBox = new Blish_HUD.Controls.TextBox
            {
                Size = new Point(150, 30),
                Location = new Point(280, 200),
                Font = GameService.Content.DefaultFont16,
                Visible = true,
                Parent = listSettingsPanel
            };
            _ButtonCreate = new StandardButton()
            {
                Text = "Create",
                Size = new Point(80, 30),
                Location = new Point(460, 200),
                Visible = true,
                Parent = listSettingsPanel
            };
            _ButtonCreate.Click += _ButtonCreate_Click;
            _PackageCreateAlert = new Blish_HUD.Controls.Label
            {
                Size = new Point(400, 40),
                Location = new Point(550, 200),
                Font = GameService.Content.DefaultFont16,
                Visible = false,
                Parent = listSettingsPanel
            };

            _PackageLoadCommunityLabel = new Blish_HUD.Controls.Label
            {
                Text = "Community Packages:",
                Size = new Point(160, 40),
                Location = new Point(110, 250),
                Font = GameService.Content.DefaultFont16,
                HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Right,
                Parent = listSettingsPanel
            };
            _PackageLoadCommunityDropdown = new Blish_HUD.Controls.Dropdown
            {
                Size = new Point(160, 40),
                Location = new Point(280, 250),
                Parent = listSettingsPanel
            };
            _ButtonLoadCommunity = new StandardButton()
            {
                Text = "Import",
                Size = new Point(80, 30),
                Location = new Point(460, 250),
                Parent = listSettingsPanel
            };
            _PackageLoadCommunityAlert = new Blish_HUD.Controls.Label
            {
                Size = new Point(500, 40),
                Location = new Point(550, 250),
                Font = GameService.Content.DefaultFont16,
                Visible = false,
                Parent = listSettingsPanel
            };
            _ButtonLoadCommunity.Click += _ButtonLoadCommunity_Click;
            Task task = LoadCommunityPackageDropdownOptions();

            _PackageLoadPersonalLabel = new Blish_HUD.Controls.Label
            {
                Text = "Personal Packages:",
                Size = new Point(160, 40),
                Location = new Point(110, 300),
                Font = GameService.Content.DefaultFont16,
                HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Right,
                Visible = false,
                Parent = listSettingsPanel
            };
            _PackageLoadPersonalDropdown = new Blish_HUD.Controls.Dropdown
            {
                Size = new Point(160, 40),
                Location = new Point(280, 300),
                Visible = false,
                Parent = listSettingsPanel
            };
            _ButtonLoadPersonal = new StandardButton()
            {
                Text = "Load",
                Size = new Point(80, 30),
                Location = new Point(460, 300),
                Visible = false,
                Parent = listSettingsPanel
            };
            _ButtonLoadPersonal.Click += _ButtonLoadPersonal_Click;
            _ButtonDeletePersonal = new StandardButton()
            {
                Text = "Delete",
                Size = new Point(80, 30),
                Location = new Point(550, 300),
                Visible = false,
                Parent = listSettingsPanel
            };
            _ButtonDeletePersonal.Click += _ButtonDeletePersonal_Click;
            Task task2 = LoadPersonalPackageDropdownOptions();

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
        public static void CopyToClipboard(string text)
        {
            // Ensure clipboard access is thread-safe
            Thread thread = new Thread(() => Clipboard.SetText(text));
            thread.SetApartmentState(ApartmentState.STA); // Clipboard requires STA
            thread.Start();
            thread.Join(); // Wait for clipboard operation to complete
        }
        private void _ButtonCopyClipboard_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _DefaultPackageLabel.Visible = true;
            _PackageRenameTextBox.Visible = false;
            _PackageRenameAlert.Visible = false;
            _PackageCreateAlert.Visible = false;
            _PackageLoadCommunityAlert.Visible = false;
            _PackageLoadPackageAlert.Visible = true;
            string PackageName = _DefaultPackageLabel.Text.ToString();
            int index = _PackageData.FindIndex(p => p.PackageName == PackageName);
            if (index >= 0)
            {
                PackageData CommunityPackageData = _PackageData[index];
                string PackageData = JsonSerializer.Serialize(CommunityPackageData, _jsonOptions);
                CopyToClipboard(PackageData.ToString());
                _PackageLoadPackageAlert.Text = "The package has been copied to your clipboard!";
                _PackageLoadPackageAlert.TextColor = Color.LimeGreen;
            }
            else
            {
                _PackageLoadPackageAlert.Text = "* The package could not be copied to your clipboard!";
                _PackageLoadPackageAlert.TextColor = Color.Red;
            }
        }
        public static string GetClipboardText()
        {
            if (Clipboard.ContainsText())
            {
                // Retrieve the text from the clipboard
                return Clipboard.GetText();
            }
            return string.Empty; // Return empty string if no text is found
        }
        private void _ButtonImportClipboard_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _DefaultPackageLabel.Visible = true;
            _PackageRenameTextBox.Visible = false;
            _PackageRenameAlert.Visible = false;
            _PackageCreateAlert.Visible = false;
            _PackageLoadCommunityAlert.Visible = false;
            _PackageLoadPackageAlert.Visible = true;

            string ClipboardText = GetClipboardText();
            try
            {
                PackageData PackageData = JsonSerializer.Deserialize<PackageData>(ClipboardText, _jsonOptions);

                if (PackageData != null)
                {
                    foreach (var Packages in _PackageData)
                    {
                        if (Packages.PackageName == PackageData.PackageName)
                        {
                            _PackageLoadPackageAlert.Text = "* This package name already exists within personal packages!";
                            _PackageLoadPackageAlert.TextColor = Color.Red;
                            return;
                        }
                    }
                    _PackageData.Add(PackageData);
                    SavePackageJsonUpdate();
                    Task task = LoadPersonalPackageDropdownOptions();

                    _PackageLoadPackageAlert.Text = "The package was imported from your clipboard to personal packages!";
                    _PackageLoadPackageAlert.TextColor = Color.LimeGreen;
                }
                else
                {
                    _PackageLoadPackageAlert.Text = "* The package could not be imported from your clipboard!";
                    _PackageLoadPackageAlert.TextColor = Color.Red;
                }
            }
            catch (JsonException ex)
            {
                _PackageLoadPackageAlert.Text = "* The package could not be imported from your clipboard!";
                _PackageLoadPackageAlert.TextColor = Color.Red;
                Logger.Warn($"Deserialization failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                _PackageLoadPackageAlert.Text = "* The package could not be imported from your clipboard!";
                _PackageLoadPackageAlert.TextColor = Color.Red;
                Logger.Warn($"Unexpected error: {ex.Message}");
            }
        }

        private void _ButtonDeletePersonal_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _DefaultPackageLabel.Visible = true;
            _PackageRenameTextBox.Visible = false;
            _PackageRenameAlert.Visible = false;
            _PackageCreateAlert.Visible = false;
            _PackageLoadCommunityAlert.Visible = false;
            _PackageLoadPackageAlert.Visible = false;
            _PackageData = _PackageData.Where(pd => pd.PackageName != _PackageLoadPersonalDropdown.SelectedItem).ToList();
            SavePackageJsonUpdate();
            Task task = LoadPersonalPackageDropdownOptions();
        }
        private void _ButtonLoadPersonal_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _DefaultPackageLabel.Visible = true;
            _PackageRenameTextBox.Visible = false;
            _PackageRenameAlert.Visible = false;
            _PackageCreateAlert.Visible = false;
            _PackageLoadCommunityAlert.Visible = false;
            _PackageLoadPackageAlert.Visible = true;
            _PackageSettingEntry.Value = _PackageLoadPersonalDropdown.SelectedItem.ToString();
            _DefaultPackageLabel.Text = _PackageLoadPersonalDropdown.SelectedItem.ToString();
            Task task = LoadPersonalPackageDropdownOptions();
            _BaubleFarmModule.Restart();
            _PackageLoadPackageAlert.Text = "The personal package has been loaded!";
            _PackageLoadPackageAlert.TextColor = Color.LimeGreen;
        }
        private void _ButtonLoadCommunity_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _DefaultPackageLabel.Visible = true;
            _PackageRenameTextBox.Visible = false;
            _PackageRenameAlert.Visible = false;
            _PackageCreateAlert.Visible = true;
            _PackageLoadCommunityAlert.Visible = true;
            _PackageLoadPackageAlert.Visible = false;
            string CommunityPackageName = _PackageLoadCommunityDropdown.SelectedItem.ToString();
            foreach (var Packages in _PackageData)
            {
                if (Packages.PackageName == CommunityPackageName)
                {
                    _PackageLoadCommunityAlert.Text = "* This package name already exists within personal packages!";
                    _PackageLoadCommunityAlert.TextColor = Color.Red;
                    return;
                }
            }

            int index = _CommunityPackageData.FindIndex(p => p.PackageName == CommunityPackageName);
            if (index >= 0)
            {
                PackageData CommunityPackageData = _CommunityPackageData[index];
                _PackageData.Add(CommunityPackageData);
                SavePackageJsonUpdate();
                _PackageSettingEntry.Value = _PackageLoadCommunityDropdown.SelectedItem.ToString();
                _DefaultPackageLabel.Text = _PackageLoadCommunityDropdown.SelectedItem.ToString();
                Task task = LoadPersonalPackageDropdownOptions();
                _BaubleFarmModule.Restart();

                _PackageLoadCommunityAlert.Text = "This community package has been loaded into personal packages!";
                _PackageLoadCommunityAlert.TextColor = Color.LimeGreen;
            }
            else
            {
                _PackageLoadCommunityAlert.Text = "* This community package could not be loaded into personal packages!";
                _PackageLoadCommunityAlert.TextColor = Color.Red;
            }
        }
        private void _ButtonCreate_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            // Check parameters
            _DefaultPackageLabel.Visible = true;
            _PackageRenameTextBox.Visible = false;
            _PackageRenameAlert.Visible = false;
            _PackageCreateAlert.Visible = true;
            _PackageLoadCommunityAlert.Visible = false;
            _PackageLoadPackageAlert.Visible = false;
            if (_PackageCreateTextBox.Text.Length < 4)
            {
                _PackageCreateAlert.Text = "* 4 characters mininimum required to create new package";
                _PackageCreateAlert.TextColor = Color.Red;
                return;
            }
            foreach (var Packages in _PackageData)
            {
                if (Packages.PackageName == _PackageCreateTextBox.Text)
                {
                    _PackageCreateAlert.Text = "* This package name already exists";
                    _PackageCreateAlert.TextColor = Color.Red;
                    return;
                }
            }
            _PackageRenameTextBox.Text = "";
            _PackageCreateAlert.Text = "Package has been created!";
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
            _PackageSettingEntry.Value = _PackageCreateTextBox.Text.ToString();
            _DefaultPackageLabel.Text = _PackageCreateTextBox.Text.ToString();
            Task task = LoadPersonalPackageDropdownOptions();
            _BaubleFarmModule.Restart();
        }

        private void _ButtonLoadRename_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _DefaultPackageLabel.Visible = false;
            _PackageRenameTextBox.Visible = true;
            _ButtonLoadRename.Visible = false;
            _ButtonSaveRename.Visible = true;
            _PackageCreateAlert.Visible = false;
            _PackageRenameAlert.Visible = false;
            _PackageLoadCommunityAlert.Visible = false;
            _PackageLoadPackageAlert.Visible = false;
            _PackageRenameTextBox.Text = _DefaultPackageLabel.Text;
        }
        private async Task LoadCommunityPackageDropdownOptions()
        {
            try
            {
                _PackageLoadCommunityDropdown.Items.Clear();
                _CommunityPackageData = new List<PackageData>();
                string jsonFilePath = @"Defaults\Community_Packages.json";
                Stream json = _BaubleFarmModule.ContentsManager.GetFileStream(jsonFilePath);
                using (StreamReader reader = new StreamReader(json))
                {
                    string jsonContent = await reader.ReadToEndAsync();
                    _CommunityPackageData = JsonSerializer.Deserialize<List<PackageData>>(jsonContent, _jsonOptions);
                }

                for (int i = 0; i < _CommunityPackageData.Count; i++)
                {
                    string PackageName = _CommunityPackageData[i].PackageName;
                    _PackageLoadCommunityDropdown.Items.Add(PackageName);
                }

                if (_PackageLoadCommunityDropdown.Items.Count > 0)
                {
                    _PackageLoadCommunityDropdown.SelectedItem = _PackageLoadCommunityDropdown.Items[0];
                    _PackageLoadCommunityLabel.Visible = true;
                    _PackageLoadCommunityDropdown.Visible = true;
                    _ButtonLoadCommunity.Visible = true;
                }
                else
                {
                    _PackageLoadPersonalLabel.Visible = false;
                    _PackageLoadPersonalDropdown.Visible = false;
                    _ButtonLoadPersonal.Visible = false;
                    _ButtonDeletePersonal.Visible = false;
                    _PackageRenameAlert.Visible = false;
                    _PackageCreateAlert.Visible = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load packagedropdowns: {ex.Message}");
            }
        }
        private async Task LoadPersonalPackageDropdownOptions()
        {
            try
            {
                _PackageLoadPersonalDropdown.Items.Clear();
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
                    if (PackageName != _DefaultPackageLabel.Text)
                    {
                        _PackageLoadPersonalDropdown.Items.Add(PackageName);
                    }
                }

                if (_PackageLoadPersonalDropdown.Items.Count > 0)
                {
                    _PackageLoadPersonalDropdown.SelectedItem = _PackageLoadPersonalDropdown.Items[0];
                    _PackageLoadPersonalLabel.Visible = true;
                    _PackageLoadPersonalDropdown.Visible = true;
                    _ButtonLoadPersonal.Visible = true;
                    _ButtonDeletePersonal.Visible = true;
                }
                else
                {
                    _PackageLoadPersonalLabel.Visible = false;
                    _PackageLoadPersonalDropdown.Visible = false;
                    _ButtonLoadPersonal.Visible = false;
                    _ButtonDeletePersonal.Visible = false;
                    _PackageRenameAlert.Visible = false;
                    _PackageCreateAlert.Visible = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load packagedropdowns: {ex.Message}");
            }
        }

        private void _ButtonSaveRename_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            // Check parameters
            if (_PackageRenameTextBox.Text.Length < 4)
            {
                _PackageRenameAlert.Text = "* 4 characters mininimum required to create new package";
                _PackageRenameAlert.Visible = true;
                _PackageRenameAlert.TextColor = Color.Red;
                return;
            }
            foreach (var Packages in _PackageData)
            {
                if (Packages.PackageName == _PackageRenameTextBox.Text && _DefaultPackageLabel.Text != _PackageRenameTextBox.Text)
                {
                    _PackageRenameAlert.Text = "* This package name already exists";
                    _PackageRenameAlert.Visible = true;
                    _PackageRenameAlert.TextColor = Color.Red;
                    return;
                }
            }
            _PackageRenameAlert.Text = "Package has been renamed!";
            _PackageRenameAlert.Visible = true;
            _PackageRenameAlert.TextColor = Color.LimeGreen;

            _ButtonSaveRename.Visible = false;
            _DefaultPackageLabel.Visible = true;
            _PackageCreateAlert.Visible = false;
            _PackageRenameTextBox.Visible = false;
            _ButtonLoadRename.Visible = true;
            _ButtonSaveRename.Visible = false;
            _PackageLoadCommunityAlert.Visible = false;
            _PackageLoadPackageAlert.Visible = false;
            var package = _PackageData.FirstOrDefault(p => p.PackageName == _DefaultPackageLabel.Text);
            if (package != null)
            {
                package.PackageName = _PackageRenameTextBox.Text;
                _PackageSettingEntry.Value = _PackageRenameTextBox.Text;
                _DefaultPackageLabel.Text = _PackageRenameTextBox.Text;
            }
            SavePackageJsonUpdate();
            Task task = LoadPersonalPackageDropdownOptions();
            _BaubleFarmModule.Restart();
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
    }
}