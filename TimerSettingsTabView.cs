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
using System.Text.Json;
using System.Threading.Tasks;

namespace roguishpanda.AB_Bauble_Farm
{
    public class TimerSettingsTabView : View
    {
        private static readonly Logger Logger = Logger.GetLogger<BaubleFarmModule>();
        private BaubleFarmModule _BaubleFarmModule;
        private AsyncTexture2D _NoTexture;
        private Panel[] _timerEventsPanels;
        private TextBox[] _timerEventTextbox;
        private Panel _timerPackagePanel;
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
        private AsyncTexture2D _cancelTexture;
        private AsyncTexture2D _addTexture;
        private Texture2D _upArrowTexture;
        private Texture2D _downArrowTexture;
        private Panel _timerSettingsPanel;
        private SettingEntry<KeyBinding> _timerKeybind;
        private SettingEntry<int> _timerMinutesDefault;
        private SettingEntry<int> _timerSecondsDefault;
        private SettingEntry<string> _timerWaypoint;
        private SettingEntry<bool> _timerBroadcastNoteOneDefault;
        private SettingEntry<string> _timerNoteOneDefault;
        private SettingEntry<string> _timerNoteTwoDefault;
        private SettingEntry<string> _timerNoteThreeDefault;
        private SettingEntry<string> _timerNoteFourDefault;
        private SettingEntry<bool> _timerBroadcastNoteFourDefault;
        private ViewContainer _settingsViewContainer;
        private SettingCollection _MainSettings;
        private List<TimerDetailData> _eventNotes;
        private List<PackageData> _PackageData;
        private int TimerRowNum;
        private StandardButton _buttonSaveEvents;
        private StandardButton _buttonReloadEvents;
        private SettingEntry<string> _PackageSettingEntry;
        private SettingEntry<bool> _timerBroadcastNoteTwoDefault;
        private SettingEntry<bool> _timerBroadcastNoteThreeDefault;
        public readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true // Makes JSON human-readable
        };

        protected override void Build(Container buildPanel)
        {
            _BaubleFarmModule = BaubleFarmModule.ModuleInstance;
            _MainSettings = _BaubleFarmModule._settings;
            _eventNotes = new List<TimerDetailData>(_BaubleFarmModule._eventNotes);
            _PackageData = new List<PackageData>(_BaubleFarmModule._PackageData);
            TimerRowNum = _BaubleFarmModule.TimerRowNum;
            _NoTexture = new AsyncTexture2D();
            _cancelTexture = AsyncTexture2D.FromAssetId(2175782);
            _addTexture = AsyncTexture2D.FromAssetId(155911);
            _upArrowTexture = _BaubleFarmModule.ContentsManager.GetTexture(@"png\517181.png");
            _downArrowTexture = _BaubleFarmModule.ContentsManager.GetTexture(@"png\517181-180.png");
            _timerSettingsPanel = new Blish_HUD.Controls.Panel
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Size.X + 500, buildPanel.ContentRegion.Size.Y + 400), // Match the panel to the content region
                Location = new Point(buildPanel.ContentRegion.Location.X, buildPanel.ContentRegion.Location.Y - 35), // Align with content region
                CanScroll = true,
                BackgroundTexture = BaubleFarmModule.ModuleInstance._asyncTimertexture
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
                Location = new Point(530, 450),
                Visible = false,
                Parent = _timerSettingsPanel
            };
            _buttonSaveEvents.Click += (s, e) => CreateEventJson();
            _buttonReloadEvents = new StandardButton
            {
                Text = "Reload",
                Size = new Point(140, 40),
                Location = new Point(680, 450),
                Visible = false,
                Parent = _timerSettingsPanel
            };
            _buttonReloadEvents.Click += (s, e) => ReloadEvents();
            _CreateEventAlert = new Label
            {
                Size = new Point(400, 40),
                Location = new Point(530, 490),
                Font = GameService.Content.DefaultFont16,
                TextColor = Color.Red,
                Visible = false,
                Parent = _timerSettingsPanel
            };
            _buttonRestartModule = new StandardButton
            {
                Text = "Restart Module",
                Size = new Point(200, 40),
                Location = new Point(530, 450),
                Visible = false,
                Parent = _timerSettingsPanel
            };
            _buttonRestartModule.Click += RestartModule_Click;

            _MinutesLabelDisplay = new Blish_HUD.Controls.Label
            {
                Size = new Point(100, 40),
                Location = new Point(890, 130),
                Font = GameService.Content.DefaultFont16,
                Parent = _timerSettingsPanel
            };
            _SecondsLabelDisplay = new Blish_HUD.Controls.Label
            {
                Size = new Point(100, 40),
                Location = new Point(890, 150),
                Font = GameService.Content.DefaultFont16,
                Parent = _timerSettingsPanel
            };
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
            LoadDefaults(TimerRowNum);
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
                    Notes = new List<string>() { "" },
                    Waypoints = new List<string>() { "" },
                    Broadcast = new List<bool>() { false, false, false, false }
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
                LoadDefaults(TimerRowNum);
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
            string CurrentPackage = "";
            SettingCollection PackageSettings = _MainSettings.AddSubCollection("PackageSettings");
            if (PackageSettings != null)
            {
                _PackageSettingEntry = null;
                PackageSettings.TryGetSetting("CurrentPackageSelection", out _PackageSettingEntry);
                if (_PackageSettingEntry != null)
                {
                    CurrentPackage = _PackageSettingEntry.Value.ToString();
                }
            }
            var package = _PackageData.FirstOrDefault(p => p.PackageName == CurrentPackage); 
            if (package != null)
            {
                package.TimerDetailData = _eventNotes;
            }
            else
            {
                throw new ArgumentException($"No PackageData found with PackageName: {CurrentPackage}");
            }
            ReplacePackage(_PackageData, package);

            string moduleDir = _BaubleFarmModule.DirectoriesManager.GetFullDirectoryPath("Shiny_Baubles");
            string jsonFilePath = Path.Combine(moduleDir, "Package_Defaults.json");
            try
            {
                string jsonContent = JsonSerializer.Serialize(_PackageData, _jsonOptions);
                File.WriteAllText(jsonFilePath, jsonContent);
                _CreateEventAlert.Text = "Events have been saved! Restart Module to reset timer UI";
                _CreateEventAlert.Visible = true;
                _CreateEventAlert.TextColor = Color.LimeGreen;
                //Logger.Info($"Saved {_eventDataList.Count} events to {_jsonFilePath}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to save JSON file: {ex.Message}");
            }
            _buttonSaveEvents.Visible = false;
            _buttonReloadEvents.Visible = false;
            _buttonRestartModule.Visible = true;
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
                    Notes = note.Notes,
                    Waypoints = note.Waypoints,
                    Broadcast = note.Broadcast
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
                    _MinutesLabelDisplay.Visible = false;
                    _SecondsLabelDisplay.Visible = false;
                    _CurrentEventLabel.Visible = false;
                    _buttonSaveEvents.Visible = true;
                    _buttonReloadEvents.Visible = true;
                    _buttonRestartModule.Visible = false;
                    return;
                }
                int NewTotal = _eventNotes.Max(note => note.ID);

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
                LoadDefaults(TimerRowNum);
                TimerSettings_Click(_timerEventsPanels[TimerRowNum - 1], null);
                //CreateEventJson();
                SettingCollection TimerCollector = _MainSettings.AddSubCollection(Description + "TimerInfo");
                TimerCollector.UndefineSetting(Description + "TimerInfo");
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
                LoadDefaults(TimerRowNum);
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
                _eventNotes = new List<TimerDetailData>(_BaubleFarmModule._eventNotes);

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
                LoadDefaults(TimerRowNum);
                TimerSettings_Click(_timerEventsPanels[TimerRowNum - 1], null);
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
                        Size = new Point(150, 30),
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

                SettingCollection TimerCollector = _MainSettings.AddSubCollection(_eventNotes[senderIndex].Description + "TimerInfo");
                _timerKeybind = new SettingEntry<KeyBinding>();
                _timerKeybind = TimerCollector.DefineSetting(_eventNotes[senderIndex].Description + "Keybind", new KeyBinding(Keys.None), () => "Keybind", () => "Keybind is used to control start/stop for timer");
                _timerMinutesDefault = TimerCollector.DefineSetting(_eventNotes[senderIndex].Description + "TimerMinutes", Convert.ToInt32(_eventNotes[senderIndex].Minutes), () => "Timer (minutes)", () => "Use to control minutes on the timer");
                _timerMinutesDefault.SetRange(0, 59);
                _MinutesLabelDisplay.Text = _timerMinutesDefault.Value.ToString() + " Minutes";
                _timerMinutesDefault.SettingChanged += (s2, e2) => LoadTimeCustomized(senderIndex);
                _timerSecondsDefault = TimerCollector.DefineSetting(_eventNotes[senderIndex].Description + "TimerSeconds", Convert.ToInt32(_eventNotes[senderIndex].Seconds), () => "Timer (seconds)", () => "Use to control seconds on the timer");
                _timerSecondsDefault.SetRange(0, 59);
                _SecondsLabelDisplay.Text = _timerSecondsDefault.Value.ToString() + " Seconds";
                _timerSecondsDefault.SettingChanged += (s2, e2) => LoadTimeCustomized(senderIndex);

                string Waypoint = "";
                if (_eventNotes[senderIndex].Waypoints != null)
                {
                    if (_eventNotes[senderIndex].Waypoints.Count > 0)
                    {
                        if (_eventNotes[senderIndex].Waypoints[0] != null)
                        {
                            Waypoint = _eventNotes[senderIndex].Waypoints[0];
                        }
                    }
                }
                _timerWaypoint = TimerCollector.DefineSetting(_eventNotes[senderIndex].Description + "Waypoint", Waypoint, () => "Waypoint", () => "Use to control the note #1");
                _timerWaypoint.SettingChanged += (s2, e2) => LoadWaypointCustomized(senderIndex);

                string NoteOne = "";
                if (_eventNotes[senderIndex].Notes != null)
                {
                    if (_eventNotes[senderIndex].Notes.Count > 0)
                    {
                        if (_eventNotes[senderIndex].Notes[0] != null)
                        {
                            NoteOne = _eventNotes[senderIndex].Notes[0];
                        }
                    }
                }
                _timerNoteOneDefault = TimerCollector.DefineSetting(_eventNotes[senderIndex].Description + "NoteOne", NoteOne, () => "Note #1", () => "Use to control the note #1");
                _timerNoteOneDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);

                string NoteTwo = "";
                if (_eventNotes[senderIndex].Notes != null)
                {
                    if (_eventNotes[senderIndex].Notes.Count > 1)
                    {
                        if (_eventNotes[senderIndex].Notes[1] != null)
                        {
                            NoteTwo = _eventNotes[senderIndex].Notes[1];
                        }
                    }
                }
                _timerNoteTwoDefault = TimerCollector.DefineSetting(_eventNotes[senderIndex].Description + "NoteTwo", NoteTwo, () => "Note #2", () => "Use to control the note #2");
                _timerNoteTwoDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);

                string NoteThree = "";
                if (_eventNotes[senderIndex].Notes != null)
                {
                    if (_eventNotes[senderIndex].Notes.Count > 2)
                    {
                        if (_eventNotes[senderIndex].Notes[2] != null)
                        {
                            NoteThree = _eventNotes[senderIndex].Notes[2];
                        }
                    }
                }
                _timerNoteThreeDefault = TimerCollector.DefineSetting(_eventNotes[senderIndex].Description + "NoteThree", NoteThree, () => "Note #3", () => "Use to control the note #3");
                _timerNoteThreeDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);

                string NoteFour = "";
                if (_eventNotes[senderIndex].Notes != null)
                {
                    if (_eventNotes[senderIndex].Notes.Count > 3)
                    {
                        if (_eventNotes[senderIndex].Notes[3] != null)
                        {
                            NoteFour = _eventNotes[senderIndex].Notes[3];
                        }
                    }
                }
                _timerNoteFourDefault = TimerCollector.DefineSetting(_eventNotes[senderIndex].Description + "NoteFour", NoteFour, () => "Note #4", () => "Use to control the note #4");
                _timerNoteFourDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);

                bool BroadcastNoteOne = false;
                if (_eventNotes[senderIndex].Broadcast != null)
                {
                    if (_eventNotes[senderIndex].Broadcast.Count > 0)
                    {
                        BroadcastNoteOne = _eventNotes[senderIndex].Broadcast[0];
                    }
                }
                _timerBroadcastNoteOneDefault = TimerCollector.DefineSetting(_eventNotes[senderIndex].Description + "BroadcastNoteOne", BroadcastNoteOne, () => "Broadcast Note #1", () => "Use to broadcast note #1");
                _timerBroadcastNoteOneDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);

                bool BroadcastNoteTwo = false;
                if (_eventNotes[senderIndex].Broadcast != null)
                {
                    if (_eventNotes[senderIndex].Broadcast.Count > 1)
                    {
                        BroadcastNoteTwo = _eventNotes[senderIndex].Broadcast[1];
                    }
                }
                _timerBroadcastNoteTwoDefault = TimerCollector.DefineSetting(_eventNotes[senderIndex].Description + "BroadcastNoteTwo", BroadcastNoteTwo, () => "Broadcast Note #2", () => "Use to broadcast note #2");
                _timerBroadcastNoteTwoDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);

                bool BroadcastNoteThree = false;
                if (_eventNotes[senderIndex].Broadcast != null)
                {
                    if (_eventNotes[senderIndex].Broadcast.Count > 2)
                    {
                        BroadcastNoteThree = _eventNotes[senderIndex].Broadcast[2];
                    }
                }
                _timerBroadcastNoteThreeDefault = TimerCollector.DefineSetting(_eventNotes[senderIndex].Description + "BroadcastNoteThree", BroadcastNoteThree, () => "Broadcast Note #3", () => "Use to broadcast note #3");
                _timerBroadcastNoteThreeDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);

                bool BroadcastNoteFour = false;
                if (_eventNotes[senderIndex].Broadcast != null)
                {
                    if (_eventNotes[senderIndex].Broadcast.Count > 3)
                    {
                        BroadcastNoteFour = _eventNotes[senderIndex].Broadcast[3];
                    }
                }
                _timerBroadcastNoteFourDefault = TimerCollector.DefineSetting(_eventNotes[senderIndex].Description + "BroadcastNoteFour", BroadcastNoteFour, () => "Broadcast Note #4", () => "Use to broadcast note #4");
                _timerBroadcastNoteFourDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);

                _settingsViewContainer = new ViewContainer
                {
                    Parent = _timerSettingsPanel,
                    Location = new Point(410, 110),
                    Size = new Point(500, 350)
                };
                var settingsView = new SettingsView(TimerCollector);
                _settingsViewContainer.Show(settingsView);
                _CurrentEventLabel.Text = _eventNotes[senderIndex].Description;

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
        private void LoadDefaults(int TotalEvents)
        {
            try
            {
                var eventNotes = _eventNotes;
                for (int i = 0; i < TotalEvents; i++)
                {
                    SettingCollection TimerCollector = _MainSettings.AddSubCollection(eventNotes[i].Description + "TimerInfo");
                    if (TimerCollector != null && TimerCollector.Count() > 0)
                    {
                        SettingEntry<int> MintuesSettingEntry = null;
                        TimerCollector.TryGetSetting(eventNotes[i].Description + "TimerMinutes", out MintuesSettingEntry);
                        SettingEntry<int> SecondsSettingEntry = null;
                        TimerCollector.TryGetSetting(eventNotes[i].Description + "TimerSeconds", out SecondsSettingEntry);
                        SettingEntry<string> WaypointSettingEntry = null;
                        TimerCollector.TryGetSetting(eventNotes[i].Description + "Waypoint", out WaypointSettingEntry);
                        SettingEntry<string> NotesOneSettingEntry = null;
                        TimerCollector.TryGetSetting(eventNotes[i].Description + "NoteOne", out NotesOneSettingEntry);
                        SettingEntry<string> NotesTwoSettingEntry = null;
                        TimerCollector.TryGetSetting(eventNotes[i].Description + "NoteTwo", out NotesTwoSettingEntry);
                        SettingEntry<string> NotesThreeSettingEntry = null;
                        TimerCollector.TryGetSetting(eventNotes[i].Description + "NoteThree", out NotesThreeSettingEntry);
                        SettingEntry<string> NotesFourSettingEntry = null;
                        TimerCollector.TryGetSetting(eventNotes[i].Description + "NoteFour", out NotesFourSettingEntry);
                        SettingEntry<bool> BroadcastNotesOneSettingEntry = null;
                        TimerCollector.TryGetSetting(eventNotes[i].Description + "BroadcastNoteOne", out NotesOneSettingEntry);
                        SettingEntry<bool> BroadcastNotesTwoSettingEntry = null;
                        TimerCollector.TryGetSetting(eventNotes[i].Description + "BroadcastNoteTwo", out NotesTwoSettingEntry);
                        SettingEntry<bool> BroadcastNotesThreeSettingEntry = null;
                        TimerCollector.TryGetSetting(eventNotes[i].Description + "BroadcastNoteThree", out NotesThreeSettingEntry);
                        SettingEntry<bool> BroadcastNotesFourSettingEntry = null;
                        TimerCollector.TryGetSetting(eventNotes[i].Description + "BroadcastNoteFour", out NotesFourSettingEntry);

                        double Minutes = eventNotes[i].Minutes;
                        double Seconds = eventNotes[i].Seconds;
                        if (MintuesSettingEntry != null)
                        {
                            double TempMinutes = MintuesSettingEntry.Value;
                            if (TempMinutes != 0)
                            {
                                Minutes = TempMinutes;
                            }
                        }
                        if (SecondsSettingEntry != null)
                        {
                            double TempSeconds = SecondsSettingEntry.Value;
                            if (TempSeconds != 0)
                            {
                                Seconds = TempSeconds;
                            }
                        }
                        MintuesSettingEntry.Value = Convert.ToInt32(Minutes);
                        SecondsSettingEntry.Value = Convert.ToInt32(Seconds);

                        if (WaypointSettingEntry != null)
                        {
                            string Waypoint = WaypointSettingEntry.Value;
                            if (eventNotes[i].Waypoints != null)
                            {
                                if (eventNotes[i].Waypoints.Count >= 0)
                                {
                                    if (Waypoint == "" && eventNotes[i].Waypoints[0] != null)
                                    {
                                        WaypointSettingEntry.Value = eventNotes[i].Waypoints[0];
                                    }
                                }
                            }
                        }

                        if (NotesOneSettingEntry != null)
                        {
                            string Notes = NotesOneSettingEntry.Value;
                            if (eventNotes[i].Notes != null)
                            {
                                if (eventNotes[i].Notes.Count >= 0)
                                {
                                    if (Notes == "" && eventNotes[i].Notes[0] != null)
                                    {
                                        NotesOneSettingEntry.Value = eventNotes[i].Notes[0];
                                    }
                                }
                            }
                        }
                        if (NotesTwoSettingEntry != null)
                        {
                            if (eventNotes[i].Notes != null)
                            {
                                string Notes = NotesTwoSettingEntry.Value;
                                if (eventNotes[i].Notes.Count > 1)
                                {
                                    if (Notes == "" && eventNotes[i].Notes[1] != null)
                                    {
                                        NotesTwoSettingEntry.Value = eventNotes[i].Notes[1];
                                    }
                                }
                            }
                        }
                        if (NotesThreeSettingEntry != null)
                        {
                            if (eventNotes[i].Notes != null)
                            {
                                string Notes = NotesThreeSettingEntry.Value;
                                if (eventNotes[i].Notes.Count > 2)
                                {
                                    if (Notes == "" && eventNotes[i].Notes[2] != null)
                                    {
                                        NotesThreeSettingEntry.Value = eventNotes[i].Notes[2];
                                    }
                                }
                            }
                        }
                        if (NotesFourSettingEntry != null)
                        {
                            if (eventNotes[i].Notes != null)
                            {
                                string Notes = NotesFourSettingEntry.Value;
                                if (eventNotes[i].Notes.Count > 3)
                                {
                                    if (Notes == "" && eventNotes[i].Notes[3] != null)
                                    {
                                        NotesFourSettingEntry.Value = eventNotes[i].Notes[3];
                                    }
                                }
                            }
                        }

                        if (BroadcastNotesOneSettingEntry != null)
                        {
                            bool Broadcast = BroadcastNotesOneSettingEntry.Value;
                            if (eventNotes[i].Broadcast != null)
                            {
                                if (eventNotes[i].Broadcast.Count > 0)
                                {
                                    BroadcastNotesOneSettingEntry.Value = eventNotes[i].Broadcast[0];
                                }
                            }
                        }
                        if (BroadcastNotesTwoSettingEntry != null)
                        {
                            bool Broadcast = BroadcastNotesTwoSettingEntry.Value;
                            if (eventNotes[i].Broadcast != null)
                            {
                                if (eventNotes[i].Broadcast.Count > 1)
                                {
                                    BroadcastNotesTwoSettingEntry.Value = eventNotes[i].Broadcast[1];
                                }
                            }
                        }
                        if (BroadcastNotesThreeSettingEntry != null)
                        {
                            bool Broadcast = BroadcastNotesThreeSettingEntry.Value;
                            if (eventNotes[i].Broadcast != null)
                            {
                                if (eventNotes[i].Broadcast.Count > 2)
                                {
                                    BroadcastNotesThreeSettingEntry.Value = eventNotes[i].Broadcast[2];
                                }
                            }
                        }
                        if (BroadcastNotesFourSettingEntry != null)
                        {
                            bool Broadcast = BroadcastNotesFourSettingEntry.Value;
                            if (eventNotes[i].Broadcast != null)
                            {
                                if (eventNotes[i].Broadcast.Count > 3)
                                {
                                    BroadcastNotesFourSettingEntry.Value = eventNotes[i].Broadcast[3];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load event defaults: {ex.Message}");
            }
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
        private void LoadWaypointCustomized(int Index)
        {
            try
            {
                List<string> WaypointList = new List<string>();
                if (_timerWaypoint != null)
                {
                    string Waypoint = _timerWaypoint.Value;
                    if (Waypoint != "")
                    {
                        WaypointList.Add(Waypoint);
                    }
                }
                if (WaypointList.Count > 0)
                {
                    _BaubleFarmModule._Waypoints[Index].Clear();
                    _BaubleFarmModule._Waypoints[Index].AddRange(WaypointList);
                    _eventNotes[Index].Waypoints.Clear();
                    _eventNotes[Index].Waypoints.AddRange(WaypointList);
                }

                _textNewEvent.Text = "";
                _buttonSaveEvents.Visible = true;
                _buttonRestartModule.Visible = false;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load settings custom waypoint: {ex.Message}");
            }
        }
        private void LoadNotesCustomized(int Index)
        {
            try
            {
                List<string> NotesList = new List<string>();
                List<bool> BroadcastNotesList = new List<bool>();
                if (_timerNoteOneDefault != null)
                {
                    string Notes = _timerNoteOneDefault.Value;
                    if (Notes != "")
                    {
                        NotesList.Add(Notes);
                    }
                }
                if (_timerNoteTwoDefault != null)
                {
                    string Notes = _timerNoteTwoDefault.Value;
                    if (Notes != "")
                    {
                        NotesList.Add(Notes);
                    }
                }
                if (_timerNoteThreeDefault != null)
                {
                    string Notes = _timerNoteThreeDefault.Value;
                    if (Notes != "")
                    {
                        NotesList.Add(Notes);
                    }
                }
                if (_timerNoteFourDefault != null)
                {
                    string Notes = _timerNoteFourDefault.Value;
                    if (Notes != "")
                    {
                        NotesList.Add(Notes);
                    }
                }

                if (_timerBroadcastNoteOneDefault != null)
                {
                    bool BroadcastNotes = _timerBroadcastNoteOneDefault.Value;
                    BroadcastNotesList.Add(BroadcastNotes);
                }
                else
                {
                    BroadcastNotesList.Add(false); // Add false for broadcast if note not null
                }
                if (_timerBroadcastNoteTwoDefault != null)
                {
                    bool BroadcastNotes = _timerBroadcastNoteTwoDefault.Value;
                    BroadcastNotesList.Add(BroadcastNotes);
                }
                else
                {
                    BroadcastNotesList.Add(false); // Add false for broadcast if note not null
                }
                if (_timerBroadcastNoteThreeDefault != null)
                {
                    bool BroadcastNotes = _timerBroadcastNoteThreeDefault.Value;
                    BroadcastNotesList.Add(BroadcastNotes);
                }
                else
                {
                    BroadcastNotesList.Add(false); // Add false for broadcast if note not null
                }
                if (_timerBroadcastNoteFourDefault != null)
                {
                    bool BroadcastNotes = _timerBroadcastNoteFourDefault.Value;
                    BroadcastNotesList.Add(BroadcastNotes);
                }
                else
                {
                    BroadcastNotesList.Add(false); // Add false for broadcast if note not null
                }

                if (NotesList.Count > 0)
                {
                    _BaubleFarmModule._Notes[Index].Clear();
                    _BaubleFarmModule._Notes[Index].AddRange(NotesList);
                    _eventNotes[Index].Notes.Clear();
                    _eventNotes[Index].Notes.AddRange(NotesList);

                    if (BroadcastNotesList.Count > 0)
                    {
                        _BaubleFarmModule._Broadcast[Index].Clear();
                        _BaubleFarmModule._Broadcast[Index].AddRange(BroadcastNotesList);
                        _eventNotes[Index].Broadcast.Clear();
                        _eventNotes[Index].Broadcast.AddRange(BroadcastNotesList);
                    }
                }

                _textNewEvent.Text = "";
                _buttonSaveEvents.Visible = true;
                _buttonRestartModule.Visible = false;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load settings custom notes: {ex.Message}");
            }
        }

    }
}