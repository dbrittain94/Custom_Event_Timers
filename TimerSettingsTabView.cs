using Blish_HUD;
using Blish_HUD.Common.Gw2;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace roguishpanda.AB_Bauble_Farm
{
    public class TimerSettingsTabView : View
    {
        private static readonly Logger Logger = Logger.GetLogger<MainWindowModule>();
        private MainWindowModule _BaubleFarmModule;
        private AsyncTexture2D _NoTexture;
        private Panel[] _timerEventsPanels;
        private TextBox[] _timerEventTextbox;
        private Panel _timerPackagePanel;
        private int _CurrentEventSelected;
        private Panel _SettingsControlPanel;
        private TextBox _textNewEvent;
        private Label _CreateEventAlert;
        private StandardButton _buttonRestartModule;
        private Label _MinutesLabelDisplay;
        private Label _SecondsLabelDisplay;
        private Label _CurrentEventLabel;
        private Panel _timerEventsTitlePanel;
        private Label _timerEventsTitleLabel;
        private Image[] _cancelButton;
        private Image[] _upArrowButton;
        private Image[] _downArrowButton;
        private Image[] _broadcastImage;
        private Checkbox[] _broadcastCheckbox;
        private AsyncTexture2D _cancelTexture;
        private AsyncTexture2D _addTexture;
        private AsyncTexture2D _broadcastTexture;
        private Texture2D _upArrowTexture;
        private Texture2D _downArrowTexture;
        private Panel _timerSettingsPanel;
        private SettingEntry<KeyBinding> _timerKeybind;
        private SettingEntry<int> _timerMinutesDefault;
        private SettingEntry<int> _timerSecondsDefault;
        private ViewContainer _settingsViewContainer;
        private SettingCollection _MainSettings;
        private List<TimerDetailData> _eventNotes;
        private List<PackageData> _PackageData;
        private int TimerRowNum;
        private StandardButton _buttonSaveEvents;
        private StandardButton _buttonReloadEvents;
        private string _CurrentPackage;
        private Label[] _WaypointsLabel;
        private TextBox[] _WaypointsTextbox;
        private Label[] _NotesLabel;
        private MultilineTextBox[] _NotesTextbox;
        public readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true // Makes JSON human-readable
        };

        protected override void Build(Container buildPanel)
        {
            _BaubleFarmModule = MainWindowModule.ModuleInstance;
            _MainSettings = _BaubleFarmModule._settings;
            _eventNotes = new List<TimerDetailData>(_BaubleFarmModule._timerEvents);
            _PackageData = new List<PackageData>(_BaubleFarmModule._PackageData);
            TimerRowNum = _BaubleFarmModule.TimerRowNum;
            _CurrentPackage = _BaubleFarmModule._CurrentPackage;
            _NoTexture = new AsyncTexture2D();
            _cancelTexture = AsyncTexture2D.FromAssetId(2175782);
            _addTexture = AsyncTexture2D.FromAssetId(155911);
            _broadcastTexture = AsyncTexture2D.FromAssetId(1234950);
            _upArrowTexture = _BaubleFarmModule.ContentsManager.GetTexture(@"png\517181.png");
            _downArrowTexture = _BaubleFarmModule.ContentsManager.GetTexture(@"png\517181-180.png");
            _timerSettingsPanel = new Blish_HUD.Controls.Panel
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Size.X + 500, buildPanel.ContentRegion.Size.Y + 400), // Match the panel to the content region
                Location = new Point(buildPanel.ContentRegion.Location.X, buildPanel.ContentRegion.Location.Y - 35), // Align with content region
                CanScroll = true,
                BackgroundTexture = MainWindowModule.ModuleInstance._asyncTimertexture
            };
            _timerPackagePanel = new Blish_HUD.Controls.Panel
            {
                Parent = _timerSettingsPanel,
                Size = new Point(300, 400), // Match the panel to the content region
                Location = new Point(100, 100), // Align with content region
                CanScroll = true,
                ShowBorder = true,
            };

            Label CreateEventDesc = new Label
            {
                Text = "Add Timer:",
                Size = new Point(200, 30),
                Location = new Point(100, 510),
                Font = GameService.Content.DefaultFont16,
                Parent = _timerSettingsPanel
            };
            _textNewEvent = new TextBox
            {
                Size = new Point(300, 40),
                Location = new Point(100, 540),
                //Visible = false,
                Parent = _timerSettingsPanel
            };
            Image buttonCreateEvent = new Image
            {
                Texture = _addTexture,
                Size = new Point(32, 32),
                Location = new Point(405, 540),
                //Visible = false,
                Parent = _timerSettingsPanel
            };
            buttonCreateEvent.Click += CreateEvent_Click;
            _buttonSaveEvents = new StandardButton
            {
                Text = "Save",
                Size = new Point(140, 40),
                Location = new Point(530, 550),
                Visible = false,
                Parent = _timerSettingsPanel
            };
            _buttonSaveEvents.Click += (s, e) => CreateEventJson();
            _buttonReloadEvents = new StandardButton
            {
                Text = "Reload",
                Size = new Point(140, 40),
                Location = new Point(680, 550),
                Visible = false,
                Parent = _timerSettingsPanel
            };
            _buttonReloadEvents.Click += (s, e) => ReloadEvents();
            _CreateEventAlert = new Label
            {
                Size = new Point(400, 40),
                Location = new Point(530, 590),
                Font = GameService.Content.DefaultFont16,
                TextColor = Color.Red,
                Visible = false,
                Parent = _timerSettingsPanel
            };
            _buttonRestartModule = new StandardButton
            {
                Text = "Restart Module",
                Size = new Point(200, 40),
                Location = new Point(530, 550),
                Visible = false,
                Parent = _timerSettingsPanel
            };
            _buttonRestartModule.Click += RestartModule_Click;

            _CurrentEventLabel = new Blish_HUD.Controls.Label
            {
                Size = new Point(300, 40),
                Location = new Point(420, 60),
                Font = GameService.Content.DefaultFont32,
                TextColor = Color.LimeGreen,
                Parent = _timerSettingsPanel
            };

            AsyncTexture2D TitleTexture = AsyncTexture2D.FromAssetId(1234872);
            _timerEventsTitlePanel = new Blish_HUD.Controls.Panel
            {
                Parent = _timerSettingsPanel,
                Size = new Point(290, 40),
                Location = new Point(102, 60),
                BackgroundTexture = TitleTexture,
            };
            _timerEventsTitleLabel = new Blish_HUD.Controls.Label
            {
                Text = "Timers",
                Size = new Point(300, 40),
                Location = new Point(10, 0),
                Font = GameService.Content.DefaultFont16,
                TextColor = Color.White,
                Parent = _timerEventsTitlePanel
            };

            _timerEventsPanels = new Panel[TimerRowNum];
            _timerEventTextbox = new TextBox[TimerRowNum];
            _cancelButton = new Image[TimerRowNum];
            _upArrowButton = new Image[TimerRowNum];
            _downArrowButton = new Image[TimerRowNum];
            LoadEventTable(TimerRowNum);
            if (TimerRowNum != 0)
            {
                TimerSettings_Click(_timerEventsPanels[0], null);
            }
        }

        private void CurrentEvent_TextChanged(int Index)
        {
            try
            {
                string NewDescription = _timerEventTextbox[Index].Text;
                _CurrentEventLabel.Text = NewDescription;
                _eventNotes[Index].Description = NewDescription;

                _buttonSaveEvents.Visible = true;
                _buttonReloadEvents.Visible = true;
                _buttonRestartModule.Visible = false;
                _CreateEventAlert.Visible = false;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to rename event: {ex.Message}");
            }
        }

        private void RestartModule_Click(object sender, MouseEventArgs e)
        {
            _BaubleFarmModule.Restart();
            _buttonRestartModule.Visible = false;
            _CreateEventAlert.Visible = false;
        }

        private void CreateEvent_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            try
            {
                var originalNotesData = _eventNotes;
                int maxId = 0;
                int NewID = 1;
                if (_eventNotes.Count > 0)
                {
                    maxId = originalNotesData.Max(note => note.ID);
                    NewID = maxId + 1;
                }
                if (_textNewEvent.Text.Length < 4)
                {
                    _CreateEventAlert.Text = "* 4 characters mininimum required to create new event";
                    _CreateEventAlert.Visible = true;
                    _CreateEventAlert.TextColor = Color.Red;
                    return;
                }
                foreach (var Events in originalNotesData)
                {
                    if (Events.Description == _textNewEvent.Text)
                    {
                        _CreateEventAlert.Text = "* This event already exists";
                        _CreateEventAlert.Visible = true;
                        _CreateEventAlert.TextColor = Color.Red;
                        return;
                    }
                }
                _CreateEventAlert.Text = "Event has been added! Click save to confirm changes!";
                _CreateEventAlert.Visible = true;
                _CreateEventAlert.TextColor = Color.LimeGreen;

                TimerDetailData notesData = new TimerDetailData()
                {
                    ID = NewID,
                    Description = _textNewEvent.Text,
                    Minutes = 8,
                    Seconds = 30,
                    WaypointData = new List<NotesData>
                    {
                        new NotesData { Type = "", Notes = "", Broadcast = false }
                    },
                    NotesData = new List<NotesData>
                    {
                        new NotesData { Type = "", Notes = "", Broadcast = false },
                        new NotesData { Type = "", Notes = "", Broadcast = false },
                        new NotesData { Type = "", Notes = "", Broadcast = false },
                        new NotesData { Type = "", Notes = "", Broadcast = false }
                    }
                };
                _eventNotes.Add(notesData);

                // Clear old UI info
                for (int i = 0; i < TimerRowNum; i++)
                {
                    _timerEventsPanels[i].Dispose();
                    _timerEventTextbox[i].Dispose();
                    _cancelButton[i].Dispose();
                    _upArrowButton[i].Dispose();
                    _downArrowButton[i].Dispose();
                }
                if (_settingsViewContainer != null)
                {
                    _settingsViewContainer.Clear();
                    _settingsViewContainer.Dispose();
                }

                // Initialize new UI Info
                TimerRowNum = _eventNotes.Count();
                _timerEventsPanels = new Panel[TimerRowNum];
                _timerEventTextbox = new TextBox[TimerRowNum];
                _cancelButton = new Image[TimerRowNum];
                _upArrowButton = new Image[TimerRowNum];
                _downArrowButton = new Image[TimerRowNum];
                LoadEventTable(TimerRowNum);
                TimerSettings_Click(_timerEventsPanels[TimerRowNum - 1], null);
                //CreateEventJson();
                _textNewEvent.Text = "";
                _buttonSaveEvents.Visible = true;
                _buttonReloadEvents.Visible = true;
                _buttonRestartModule.Visible = false;
                _MinutesLabelDisplay.Visible = true;
                _SecondsLabelDisplay.Visible = true;
                _CurrentEventLabel.Visible = true;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to created event: {ex.Message}");
            }
        }
        public void ReplacePackage(List<PackageData> packageList, PackageData newPackage)
        {
            for (int i = 0; i < packageList.Count; i++)
            {
                if (packageList[i].PackageName == newPackage.PackageName)
                {
                    packageList[i] = newPackage;
                    return;
                }
            }
            // Optional: Add the new package if it doesn't exist
            packageList.Add(newPackage);
        }
        private void CreateEventJson()
        {
            var package = _PackageData.FirstOrDefault(p => p.PackageName == _CurrentPackage); 
            if (package != null)
            {
                package.TimerDetailData = _eventNotes;
            }
            else
            {
                throw new ArgumentException($"No PackageData found with PackageName: {_CurrentPackage}");
            }
            ReplacePackage(_PackageData, package);

            string moduleDir = _BaubleFarmModule.DirectoriesManager.GetFullDirectoryPath("Shiny_Baubles");
            string jsonFilePath = Path.Combine(moduleDir, "Package_Defaults.json");
            try
            {
                string jsonContent = JsonSerializer.Serialize(_PackageData, _jsonOptions);
                File.WriteAllText(jsonFilePath, jsonContent);
                _CreateEventAlert.Text = "Events have been saved!";
                _CreateEventAlert.Visible = true;
                _buttonReloadEvents.Visible = false;
                _CreateEventAlert.TextColor = Color.LimeGreen;
                //Logger.Info($"Saved {_eventDataList.Count} events to {_jsonFilePath}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to save JSON file: {ex.Message}");
            }
            _BaubleFarmModule.Restart();
        }
        private void CancelEvent_Click(int Index)
        {
            try
            {
                string Description = _eventNotes[Index].Description;
                int ID = _eventNotes[Index].ID;
                _eventNotes.RemoveAll(note => note.Description == Description && note.ID == ID);
                _eventNotes = _eventNotes.Select((note, index) => new TimerDetailData
                {
                    ID = index + 1,
                    Description = note.Description,
                    Minutes = note.Minutes,
                    Seconds = note.Seconds,
                    WaypointData = note.WaypointData,
                    NotesData = note.NotesData
                }).ToList();
                if (_eventNotes.Count <= 0)
                {
                    _timerEventsPanels[0].Dispose();
                    _timerEventTextbox[0].Dispose();
                    _cancelButton[0].Dispose();
                    _upArrowButton[0].Dispose();
                    _downArrowButton[0].Dispose();
                    _settingsViewContainer.Clear();
                    _settingsViewContainer.Dispose();
                    _SettingsControlPanel.Dispose();
                    _MinutesLabelDisplay.Visible = false;
                    _SecondsLabelDisplay.Visible = false;
                    _CurrentEventLabel.Visible = false;
                    _buttonSaveEvents.Visible = true;
                    _buttonReloadEvents.Visible = true;
                    _buttonRestartModule.Visible = false;
                    return;
                }
                int NewTotal = _eventNotes.Max(note => note.ID);

                //Clear old setting
                SettingCollection PackageInfo = _MainSettings.AddSubCollection(_CurrentPackage + "_PackageInfo");
                SettingCollection staticCollector = PackageInfo.AddSubCollection("TimerInfo_" + ID);
                staticCollector.UndefineSetting("TimerInfo_" + ID);

                // Clear old UI info
                for (int i = 0; i < TimerRowNum; i++)
                {
                    _timerEventsPanels[i].Dispose();
                    _timerEventTextbox[i].Dispose();
                    _cancelButton[i].Dispose();
                    _upArrowButton[i].Dispose();
                    _downArrowButton[i].Dispose();
                }
                _settingsViewContainer.Clear();
                _settingsViewContainer.Dispose();

                // Initialize new UI Info
                TimerRowNum = _eventNotes.Count();
                _timerEventsPanels = new Panel[TimerRowNum];
                _timerEventTextbox = new TextBox[TimerRowNum];
                _cancelButton = new Image[TimerRowNum];
                _upArrowButton = new Image[TimerRowNum];
                _downArrowButton = new Image[TimerRowNum];
                LoadEventTable(TimerRowNum);
                TimerSettings_Click(_timerEventsPanels[0], null);
                _buttonSaveEvents.Visible = true;
                _buttonReloadEvents.Visible = true;
                _buttonRestartModule.Visible = false;
                _CreateEventAlert.Visible = true;
                _CreateEventAlert.Text = "Event was deleted!";
                _CreateEventAlert.TextColor = Color.LimeGreen;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to remove event: {ex.Message}");
            }
        }
        private void MoveEvent_Click(int Index, int Direction)
        {
            try
            {
                string Description = _eventNotes[Index].Description;
                int ID = _eventNotes[Index].ID;
                TimerDetailData temp = _eventNotes[Index];
                _eventNotes[Index] = _eventNotes[Index + Direction];
                _eventNotes[Index + Direction] = temp;

                //Clear old setting
                SettingCollection PackageInfo = _MainSettings.AddSubCollection(_CurrentPackage + "_PackageInfo");
                SettingCollection staticCollector = PackageInfo.AddSubCollection("TimerInfo_" + ID);
                staticCollector.UndefineSetting("TimerInfo_" + ID);

                // Clear old UI info
                for (int i = 0; i < TimerRowNum; i++)
                {
                    _timerEventsPanels[i].Dispose();
                    _timerEventTextbox[i].Dispose();
                    _cancelButton[i].Dispose();
                    _upArrowButton[i].Dispose();
                    _downArrowButton[i].Dispose();
                }
                _settingsViewContainer.Clear();
                _settingsViewContainer.Dispose();

                // Initialize new UI Info
                TimerRowNum = _eventNotes.Count();
                _timerEventsPanels = new Panel[TimerRowNum];
                _timerEventTextbox = new TextBox[TimerRowNum];
                _cancelButton = new Image[TimerRowNum];
                _upArrowButton = new Image[TimerRowNum];
                _downArrowButton = new Image[TimerRowNum];
                LoadEventTable(TimerRowNum);
                TimerSettings_Click(_timerEventsPanels[Index + Direction], null);
                _buttonSaveEvents.Visible = true;
                _buttonReloadEvents.Visible = true;
                _buttonRestartModule.Visible = false;
                //_CreateEventAlert.Visible = true;
                //_CreateEventAlert.Text = "Event was moved!";
                //_CreateEventAlert.TextColor = Color.LimeGreen;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to move event: {ex.Message}");
            }
        }
        private void ReloadEvents()
        {
            try
            {
                // Reload events from original UI
                _eventNotes = new List<TimerDetailData>(_BaubleFarmModule._timerEvents);

                // Clear old UI info
                for (int i = 0; i < TimerRowNum; i++)
                {
                    _timerEventsPanels[i].Dispose();
                    _timerEventTextbox[i].Dispose();
                    _cancelButton[i].Dispose();
                    _upArrowButton[i].Dispose();
                    _downArrowButton[i].Dispose();
                }
                _settingsViewContainer.Clear();
                _settingsViewContainer.Dispose();

                // Initialize new UI Info
                TimerRowNum = _eventNotes.Count();
                _timerEventsPanels = new Panel[TimerRowNum];
                _timerEventTextbox = new TextBox[TimerRowNum];
                _cancelButton = new Image[TimerRowNum];
                _upArrowButton = new Image[TimerRowNum];
                _downArrowButton = new Image[TimerRowNum];
                LoadEventTable(TimerRowNum);
                TimerSettings_Click(_timerEventsPanels[0], null);
                _buttonSaveEvents.Visible = false;
                _buttonReloadEvents.Visible = false;
                _CreateEventAlert.Visible = true;
                _MinutesLabelDisplay.Visible = true;
                _SecondsLabelDisplay.Visible = true;
                _CurrentEventLabel.Visible = true;
                _CreateEventAlert.Text = "Events have reloaded!";
                _CreateEventAlert.TextColor = Color.LimeGreen;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to reload events: {ex.Message}");
            }
        }
        public void LoadEventTable(int TotalEvents)
        {
            try
            {
                var eventNotes = _eventNotes;
                for (int i = 0; i < TotalEvents; i++)
                {
                    int Index = i;
                    _timerEventsPanels[i] = new Blish_HUD.Controls.Panel
                    {
                        Parent = _timerPackagePanel,
                        Size = new Point(300, 40),
                        Location = new Point(0, (i * 40)),
                    };
                    _timerEventsPanels[i].Click += TimerSettings_Click;

                    if (i % 2 == 0)
                    {
                        _timerEventsPanels[i].BackgroundColor = new Color(0, 0, 0, 0.5f);
                    }
                    else
                    {
                        _timerEventsPanels[i].BackgroundColor = new Color(0, 0, 0, 0.2f);
                    }

                    _timerEventTextbox[i] = new Blish_HUD.Controls.TextBox
                    {
                        Text = eventNotes[i].Description,
                        Size = new Point(200, 30),
                        Location = new Point(30, 5),
                        HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Left,
                        Font = GameService.Content.DefaultFont16,
                        HideBackground = true,
                        ForeColor = Color.LimeGreen,
                        Parent = _timerEventsPanels[i]
                    };
                    _timerEventTextbox[i].TextChanged += (s, e) => CurrentEvent_TextChanged(Index);

                    _cancelButton[i] = new Image
                    {
                        Texture = _cancelTexture,
                        Size = new Point(16, 16),
                        Location = new Point(10, 10),
                        Visible = false,
                        Parent = _timerEventsPanels[i]
                    };
                    _cancelButton[i].Click += (s, e) => CancelEvent_Click(Index);
                    _upArrowButton[i] = new Image
                    {
                        Texture = _upArrowTexture,
                        Size = new Point(20, 20),
                        Location = new Point(240, 4),
                        Visible = false,
                        Parent = _timerEventsPanels[i]
                    };
                    _upArrowButton[i].Click += (s, e) => MoveEvent_Click(Index, -1);
                    _downArrowButton[i] = new Image
                    {
                        Texture = _downArrowTexture,
                        Size = new Point(20, 20),
                        Location = new Point(240, 20),
                        Visible = false,
                        Parent = _timerEventsPanels[i]
                    };
                    _downArrowButton[i].Click += (s, e) => MoveEvent_Click(Index, 1);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load events: {ex.Message}");
            }
        }

        private void TimerSettings_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            try
            {
                int senderIndex = Array.IndexOf(_timerEventsPanels, sender);
                var eventNotes = _eventNotes;
                if (_settingsViewContainer != null)
                {
                    _settingsViewContainer.Clear();
                    _settingsViewContainer.Dispose();
                }
                if (_SettingsControlPanel != null)
                {
                    _SettingsControlPanel.Dispose();
                }

                SettingCollection PackageInfo = _MainSettings.AddSubCollection(_CurrentPackage + "_PackageInfo");
                SettingCollection TimerCollector = PackageInfo.AddSubCollection("TimerInfo_" + _eventNotes[senderIndex].ID);

                // Control Panel
                _CurrentEventSelected = senderIndex;
                _SettingsControlPanel = new Blish_HUD.Controls.Panel
                {
                    Parent = _timerSettingsPanel,
                    Location = new Point(410, 110),
                    Size = new Point(600, 400),
                    CanScroll = true,
                };

                _settingsViewContainer = new ViewContainer
                {
                    Parent = _SettingsControlPanel,
                    Location = new Point(0, 0),
                    Size = new Point(500, 100)
                };
                var settingsView = new SettingsView(TimerCollector);
                _settingsViewContainer.Show(settingsView);
                _CurrentEventLabel.Text = _eventNotes[senderIndex].Description;
                _MinutesLabelDisplay = new Blish_HUD.Controls.Label
                {
                    Size = new Point(100, 40),
                    Location = new Point(480, 20),
                    Font = GameService.Content.DefaultFont16,
                    Parent = _SettingsControlPanel
                };
                _SecondsLabelDisplay = new Blish_HUD.Controls.Label
                {
                    Size = new Point(100, 40),
                    Location = new Point(480, 40),
                    Font = GameService.Content.DefaultFont16,
                    Parent = _SettingsControlPanel
                };

                _timerKeybind = new SettingEntry<KeyBinding>();
                _timerKeybind = TimerCollector.DefineSetting("Keybind", new KeyBinding(Keys.None), () => "Keybind", () => "Keybind is used to control start/stop for timer");
                _timerMinutesDefault = TimerCollector.DefineSetting("TimerMinutes", Convert.ToInt32(_eventNotes[senderIndex].Minutes), () => "Timer (minutes)", () => "Use to control minutes on the timer");
                _timerMinutesDefault.SetRange(0, 59);
                _MinutesLabelDisplay.Text = _timerMinutesDefault.Value.ToString() + " Minutes";
                _timerMinutesDefault.SettingChanged += (s2, e2) => LoadTimeCustomized(senderIndex);
                _timerSecondsDefault = TimerCollector.DefineSetting("TimerSeconds", Convert.ToInt32(_eventNotes[senderIndex].Seconds), () => "Timer (seconds)", () => "Use to control seconds on the timer");
                _timerSecondsDefault.SetRange(0, 59);
                _SecondsLabelDisplay.Text = _timerSecondsDefault.Value.ToString() + " Seconds";
                _timerSecondsDefault.SettingChanged += (s2, e2) => LoadTimeCustomized(senderIndex);

                int waypointCount = _eventNotes[senderIndex].WaypointData.Count;
                int notesCount = _eventNotes[senderIndex].NotesData.Count;
                _WaypointsLabel = new Label[1];
                _WaypointsTextbox = new TextBox[1];
                _NotesLabel = new Label[4];
                _NotesTextbox = new MultilineTextBox[4];
                _broadcastImage = new Image[4];
                _broadcastCheckbox = new Checkbox[4];
                int currentControlCount = 0;
                for (int y = 0; y < 1; y++)
                {
                    _WaypointsLabel[y] = new Blish_HUD.Controls.Label
                    {
                        Text = "Waypoint:",
                        Size = new Point(100, 40),
                        Location = new Point(0, 90),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Font = GameService.Content.DefaultFont16,
                        Parent = _SettingsControlPanel
                    };
                    _WaypointsTextbox[y] = new Blish_HUD.Controls.TextBox
                    {
                        Size = new Point(350, 40),
                        Location = new Point(110, 90),
                        Font = GameService.Content.DefaultFont16,
                        Parent = _SettingsControlPanel
                    };

                    _WaypointsTextbox[y].TextChanged += _WaypointsTextbox_TextChanged;
                    if (eventNotes[senderIndex].WaypointData.Count > y)
                    {
                        _WaypointsTextbox[y].Text = eventNotes[senderIndex].WaypointData[y].Notes;
                    }
                }
                for (int z = 0; z < 4; z++)
                {
                    _NotesLabel[z] = new Blish_HUD.Controls.Label
                    {
                        Text = "Note #" + (z + 1).ToString() + ":",
                        Size = new Point(100, 40),
                        Location = new Point(0, 140 + (currentControlCount * 90)),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Font = GameService.Content.DefaultFont16,
                        Parent = _SettingsControlPanel
                    };
                    _NotesTextbox[z] = new Blish_HUD.Controls.MultilineTextBox
                    {
                        Size = new Point(450, 80),
                        Location = new Point(110, 140 + (currentControlCount * 90)),
                        Font = GameService.Content.DefaultFont16,
                        Parent = _SettingsControlPanel
                    };
                    _broadcastImage[z] = new Image
                    {
                        Texture = _broadcastTexture,
                        Size = new Point(32, 32),
                        Location = new Point(50, 170 + (currentControlCount * 90)),
                        Parent = _SettingsControlPanel
                    };
                    _broadcastCheckbox[z] = new Checkbox
                    {
                        Size = new Point(32, 32),
                        Location = new Point(80, 170 + (currentControlCount * 90)),
                        Parent = _SettingsControlPanel
                    };

                    _NotesTextbox[z].TextChanged += _NotesTextbox_TextChanged;
                    _broadcastCheckbox[z].CheckedChanged += _broadcastCheckbox_CheckedChanged;
                    if (eventNotes[senderIndex].NotesData.Count > z)
                    {
                        _NotesTextbox[z].Text = WrapText(eventNotes[senderIndex].NotesData[z].Notes, 60);
                        if (eventNotes[senderIndex].NotesData[z].Broadcast == true)
                        {
                            _broadcastCheckbox[z].Checked = true;
                        }
                        _broadcastCheckbox[z].CheckedChanged += (s2, e2) =>
                        {
                            _buttonSaveEvents.Visible = true;
                        };
                    }

                    currentControlCount++;
                }

                //// Re-color panels
                for (int i = 0; i < TimerRowNum; i++)
                {
                    if (i % 2 == 0)
                    {
                        _timerEventsPanels[i].BackgroundColor = new Color(0, 0, 0, 0.5f);
                    }
                    else
                    {
                        _timerEventsPanels[i].BackgroundColor = new Color(0, 0, 0, 0.2f);
                    }
                }
                //// Change color of selected
                _timerEventsPanels[senderIndex].BackgroundColor = new Color(0, 0, 0, 1.0f);

                for (int i = 0; i < TimerRowNum; i++)
                {
                    _cancelButton[i].Visible = false;
                    _upArrowButton[i].Visible = false;
                    _downArrowButton[i].Visible = false;
                }

                _cancelButton[senderIndex].Visible = true;
                if (senderIndex != 0)
                {
                    _upArrowButton[senderIndex].Visible = true;
                }
                if ((senderIndex + 1) != TimerRowNum)
                {
                    _downArrowButton[senderIndex].Visible = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load event details when clicking event panel: {ex.Message}");
            }
        }
        private void _WaypointsTextbox_TextChanged(object sender, EventArgs e)
        {
            int senderIndex = Array.IndexOf(_WaypointsTextbox, sender);
            _eventNotes[_CurrentEventSelected].WaypointData[senderIndex].Notes = _WaypointsTextbox[senderIndex].Text;
            _buttonSaveEvents.Visible = true;
        }
        private void _NotesTextbox_TextChanged(object sender, EventArgs e)
        {
            int senderIndex = Array.IndexOf(_NotesTextbox, sender);
            _eventNotes[_CurrentEventSelected].NotesData[senderIndex].Notes = _NotesTextbox[senderIndex].Text.Replace("\n", " ");
            _buttonSaveEvents.Visible = true;
        }
        private void _broadcastCheckbox_CheckedChanged(object sender, CheckChangedEvent e)
        {
            int senderIndex = Array.IndexOf(_broadcastCheckbox, sender);
            _eventNotes[_CurrentEventSelected].NotesData[senderIndex].Broadcast = _broadcastCheckbox[senderIndex].Checked;
            _buttonSaveEvents.Visible = true;
        }

        private string WrapText(string text, int maxWidth)
        {
            var words = text.Split(' ');
            var lines = new List<string>();
            var currentLine = new StringBuilder();

            foreach (var word in words)
            {
                if ((currentLine.Length + word.Length + 1) > maxWidth)
                {
                    lines.Add(currentLine.ToString().Trim());
                    currentLine.Clear();
                }
                currentLine.Append(word + " ");
            }

            if (currentLine.Length > 0)
                lines.Add(currentLine.ToString().Trim());

            return string.Join("\n", lines);
        }
        private void LoadTimeCustomized(int Index)
        {
            TimeSpan Minutes = TimeSpan.FromMinutes(_timerMinutesDefault.Value);
            TimeSpan Seconds = TimeSpan.FromSeconds(_timerSecondsDefault.Value);
            _MinutesLabelDisplay.Text = _timerMinutesDefault.Value.ToString() + " Minutes";
            _SecondsLabelDisplay.Text = _timerSecondsDefault.Value.ToString() + " Seconds";

            if (Index < _BaubleFarmModule._timerDurationDefaults.Count())
            {
                _BaubleFarmModule._timerDurationDefaults[Index] = Minutes + Seconds;
                _BaubleFarmModule._timerLabels[Index].Text = _BaubleFarmModule._timerDurationDefaults[Index].ToString(@"mm\:ss");
            }
            if (Index < _eventNotes.Count)
            {
                _eventNotes[Index].Minutes = Minutes.TotalMinutes;
                _eventNotes[Index].Seconds = Seconds.TotalSeconds;
            }

            _textNewEvent.Text = "";
            _buttonSaveEvents.Visible = true;
            _buttonRestartModule.Visible = false;
        }
    }
}