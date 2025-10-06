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
    public class StaticEventSettingsTabView : View
    {
        private static readonly Logger Logger = Logger.GetLogger<BaubleFarmModule>();
        private BaubleFarmModule _BaubleFarmModule;
        private AsyncTexture2D _NoTexture;
        private Panel[] _staticEventsPanels;
        private TextBox[] _staticEventTextbox;
        private Panel _staticPackagePanel;
        private TextBox _textNewEvent;
        private Label _CreateEventAlert;
        private StandardButton _buttonRestartModule;
        private Label _CurrentEventLabel;
        private Panel _staticEventsTitlePanel;
        private Label _staticEventsTitleLabel;
        private Image[] _cancelButton;
        private Image[] _upArrowButton;
        private Image[] _downArrowButton;
        private AsyncTexture2D _cancelTexture;
        private AsyncTexture2D _addTexture;
        private Texture2D _upArrowTexture;
        private Texture2D _downArrowTexture;
        private Panel _staticSettingsPanel;
        private SettingEntry<string> _staticWaypoint;
        private SettingEntry<bool> _staticBroadcastNoteOneDefault;
        private SettingEntry<string> _staticNoteOneDefault;
        private SettingEntry<string> _staticNoteTwoDefault;
        private SettingEntry<string> _staticNoteThreeDefault;
        private SettingEntry<string> _staticNoteFourDefault;
        private SettingEntry<bool> _staticBroadcastNoteFourDefault;
        private ViewContainer _settingsViewContainer;
        private SettingCollection _MainSettings;
        private List<StaticDetailData> _eventNotes;
        private List<PackageData> _PackageData;
        private int StaticRowNum;
        private StandardButton _buttonSaveEvents;
        private StandardButton _buttonReloadEvents;
        private SettingEntry<bool> _staticBroadcastNoteTwoDefault;
        private SettingEntry<bool> _staticBroadcastNoteThreeDefault;
        private string _CurrentPackage;
        public readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true // Makes JSON human-readable
        };

        protected override void Build(Container buildPanel)
        {
            _BaubleFarmModule = BaubleFarmModule.ModuleInstance;
            _MainSettings = _BaubleFarmModule._settings;
            _eventNotes = new List<StaticDetailData>(_BaubleFarmModule._staticEvents);
            _PackageData = new List<PackageData>(_BaubleFarmModule._PackageData);
            StaticRowNum = _BaubleFarmModule.StaticRowNum;
            _CurrentPackage = _BaubleFarmModule._CurrentPackage;
            _NoTexture = new AsyncTexture2D();
            _cancelTexture = AsyncTexture2D.FromAssetId(2175782);
            _addTexture = AsyncTexture2D.FromAssetId(155911);
            _upArrowTexture = _BaubleFarmModule.ContentsManager.GetTexture(@"png\517181.png");
            _downArrowTexture = _BaubleFarmModule.ContentsManager.GetTexture(@"png\517181-180.png");
            _staticSettingsPanel = new Blish_HUD.Controls.Panel
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Size.X + 500, buildPanel.ContentRegion.Size.Y + 400), // Match the panel to the content region
                Location = new Point(buildPanel.ContentRegion.Location.X, buildPanel.ContentRegion.Location.Y - 35), // Align with content region
                CanScroll = true,
                BackgroundTexture = BaubleFarmModule.ModuleInstance._asyncTimertexture
            };
            _staticPackagePanel = new Blish_HUD.Controls.Panel
            {
                Parent = _staticSettingsPanel,
                Size = new Point(300, 400), // Match the panel to the content region
                Location = new Point(100, 100), // Align with content region
                CanScroll = true,
                ShowBorder = true,
            };

            Label CreateEventDesc = new Label
            {
                Text = "Add Event:",
                Size = new Point(200, 30),
                Location = new Point(100, 510),
                Font = GameService.Content.DefaultFont16,
                Parent = _staticSettingsPanel
            };
            _textNewEvent = new TextBox
            {
                Size = new Point(300, 40),
                Location = new Point(100, 540),
                //Visible = false,
                Parent = _staticSettingsPanel
            };
            Image buttonCreateEvent = new Image
            {
                Texture = _addTexture,
                Size = new Point(32, 32),
                Location = new Point(405, 540),
                //Visible = false,
                Parent = _staticSettingsPanel
            };
            buttonCreateEvent.Click += CreateEvent_Click;
            _buttonSaveEvents = new StandardButton
            {
                Text = "Save",
                Size = new Point(140, 40),
                Location = new Point(530, 450),
                Visible = false,
                Parent = _staticSettingsPanel
            };
            _buttonSaveEvents.Click += (s, e) => CreateEventJson();
            _buttonReloadEvents = new StandardButton
            {
                Text = "Reload",
                Size = new Point(140, 40),
                Location = new Point(680, 450),
                Visible = false,
                Parent = _staticSettingsPanel
            };
            _buttonReloadEvents.Click += (s, e) => ReloadEvents();
            _CreateEventAlert = new Label
            {
                Size = new Point(400, 40),
                Location = new Point(530, 490),
                Font = GameService.Content.DefaultFont16,
                TextColor = Color.Red,
                Visible = false,
                Parent = _staticSettingsPanel
            };
            _buttonRestartModule = new StandardButton
            {
                Text = "Restart Module",
                Size = new Point(200, 40),
                Location = new Point(530, 450),
                Visible = false,
                Parent = _staticSettingsPanel
            };
            _buttonRestartModule.Click += RestartModule_Click;

            _CurrentEventLabel = new Blish_HUD.Controls.Label
            {
                Size = new Point(300, 40),
                Location = new Point(420, 60),
                Font = GameService.Content.DefaultFont32,
                TextColor = Color.LimeGreen,
                Parent = _staticSettingsPanel
            };

            AsyncTexture2D TitleTexture = AsyncTexture2D.FromAssetId(1234872);
            _staticEventsTitlePanel = new Blish_HUD.Controls.Panel
            {
                Parent = _staticSettingsPanel,
                Size = new Point(290, 40),
                Location = new Point(102, 60),
                BackgroundTexture = TitleTexture,
            };
            _staticEventsTitleLabel = new Blish_HUD.Controls.Label
            {
                Text = "Static Events",
                Size = new Point(300, 40),
                Location = new Point(10, 0),
                Font = GameService.Content.DefaultFont16,
                TextColor = Color.White,
                Parent = _staticEventsTitlePanel
            };

            _staticEventsPanels = new Panel[StaticRowNum];
            _staticEventTextbox = new TextBox[StaticRowNum];
            _cancelButton = new Image[StaticRowNum];
            _upArrowButton = new Image[StaticRowNum];
            _downArrowButton = new Image[StaticRowNum];
            LoadEventTable(StaticRowNum);
            LoadDefaults(StaticRowNum);
            if (StaticRowNum != 0)
            {
                StaticSettings_Click(_staticEventsPanels[0], null);
            }
        }

        private void CurrentEvent_TextChanged(int Index)
        {
            try
            {
                string NewDescription = _staticEventTextbox[Index].Text;
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

                StaticDetailData notesData = new StaticDetailData()
                {
                    ID = NewID,
                    Description = _textNewEvent.Text,
                    Notes = new List<string>() { "" },
                    Waypoints = new List<string>() { "" },
                    Broadcast = new List<bool>() { false, false, false, false }
                };
                _eventNotes.Add(notesData);

                // Clear old UI info
                for (int i = 0; i < StaticRowNum; i++)
                {
                    _staticEventsPanels[i].Dispose();
                    _staticEventTextbox[i].Dispose();
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
                StaticRowNum = _eventNotes.Count();
                _staticEventsPanels = new Panel[StaticRowNum];
                _staticEventTextbox = new TextBox[StaticRowNum];
                _cancelButton = new Image[StaticRowNum];
                _upArrowButton = new Image[StaticRowNum];
                _downArrowButton = new Image[StaticRowNum];
                LoadEventTable(StaticRowNum);
                LoadDefaults(StaticRowNum);
                StaticSettings_Click(_staticEventsPanels[StaticRowNum - 1], null);
                //CreateEventJson();
                _textNewEvent.Text = "";
                _buttonSaveEvents.Visible = true;
                _buttonReloadEvents.Visible = true;
                _buttonRestartModule.Visible = false;
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
                package.StaticDetailData = _eventNotes;
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
                _CreateEventAlert.Text = "Events have been saved! Restart Module to reset static UI";
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
                _eventNotes = _eventNotes.Select((note, index) => new StaticDetailData
                {
                    ID = index + 1,
                    Description = note.Description,
                    Notes = note.Notes,
                    Waypoints = note.Waypoints,
                    Broadcast = note.Broadcast
                }).ToList();
                if (_eventNotes.Count <= 0)
                {
                    _staticEventsPanels[0].Dispose();
                    _staticEventTextbox[0].Dispose();
                    _cancelButton[0].Dispose();
                    _upArrowButton[0].Dispose();
                    _downArrowButton[0].Dispose();
                    _settingsViewContainer.Clear();
                    _settingsViewContainer.Dispose();
                    _CurrentEventLabel.Visible = false;
                    _buttonSaveEvents.Visible = true;
                    _buttonReloadEvents.Visible = true;
                    _buttonRestartModule.Visible = false;
                    return;
                }
                int NewTotal = _eventNotes.Max(note => note.ID);

                // Clear old UI info
                for (int i = 0; i < StaticRowNum; i++)
                {
                    _staticEventsPanels[i].Dispose();
                    _staticEventTextbox[i].Dispose();
                    _cancelButton[i].Dispose();
                    _upArrowButton[i].Dispose();
                    _downArrowButton[i].Dispose();
                }
                _settingsViewContainer.Clear();
                _settingsViewContainer.Dispose();

                // Initialize new UI Info
                StaticRowNum = _eventNotes.Count();
                _staticEventsPanels = new Panel[StaticRowNum];
                _staticEventTextbox = new TextBox[StaticRowNum];
                _cancelButton = new Image[StaticRowNum];
                _upArrowButton = new Image[StaticRowNum];
                _downArrowButton = new Image[StaticRowNum];
                LoadEventTable(StaticRowNum);
                LoadDefaults(StaticRowNum);
                StaticSettings_Click(_staticEventsPanels[StaticRowNum - 1], null);
                //CreateEventJson();
                SettingCollection PackageInfo = _MainSettings.AddSubCollection(_CurrentPackage + "_PackageInfo");
                SettingCollection staticCollector = PackageInfo.AddSubCollection(Description + "_StaticInfo");
                staticCollector.UndefineSetting(Description + "_StaticInfo");
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
                StaticDetailData temp = _eventNotes[Index];
                _eventNotes[Index] = _eventNotes[Index + Direction];
                _eventNotes[Index + Direction] = temp;

                // Clear old UI info
                for (int i = 0; i < StaticRowNum; i++)
                {
                    _staticEventsPanels[i].Dispose();
                    _staticEventTextbox[i].Dispose();
                    _cancelButton[i].Dispose();
                    _upArrowButton[i].Dispose();
                    _downArrowButton[i].Dispose();
                }
                _settingsViewContainer.Clear();
                _settingsViewContainer.Dispose();

                // Initialize new UI Info
                StaticRowNum = _eventNotes.Count();
                _staticEventsPanels = new Panel[StaticRowNum];
                _staticEventTextbox = new TextBox[StaticRowNum];
                _cancelButton = new Image[StaticRowNum];
                _upArrowButton = new Image[StaticRowNum];
                _downArrowButton = new Image[StaticRowNum];
                LoadEventTable(StaticRowNum);
                LoadDefaults(StaticRowNum);
                StaticSettings_Click(_staticEventsPanels[Index + Direction], null);
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
                _eventNotes = new List<StaticDetailData>(_BaubleFarmModule._staticEvents);

                // Clear old UI info
                for (int i = 0; i < StaticRowNum; i++)
                {
                    _staticEventsPanels[i].Dispose();
                    _staticEventTextbox[i].Dispose();
                    _cancelButton[i].Dispose();
                    _upArrowButton[i].Dispose();
                    _downArrowButton[i].Dispose();
                }
                _settingsViewContainer.Clear();
                _settingsViewContainer.Dispose();

                // Initialize new UI Info
                StaticRowNum = _eventNotes.Count();
                _staticEventsPanels = new Panel[StaticRowNum];
                _staticEventTextbox = new TextBox[StaticRowNum];
                _cancelButton = new Image[StaticRowNum];
                _upArrowButton = new Image[StaticRowNum];
                _downArrowButton = new Image[StaticRowNum];
                LoadEventTable(StaticRowNum);
                LoadDefaults(StaticRowNum);
                StaticSettings_Click(_staticEventsPanels[StaticRowNum - 1], null);
                _buttonSaveEvents.Visible = false;
                _buttonReloadEvents.Visible = false;
                _CreateEventAlert.Visible = true;
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
                    _staticEventsPanels[i] = new Blish_HUD.Controls.Panel
                    {
                        Parent = _staticPackagePanel,
                        Size = new Point(300, 40),
                        Location = new Point(0, (i * 40)),
                    };
                    _staticEventsPanels[i].Click += StaticSettings_Click;

                    if (i % 2 == 0)
                    {
                        _staticEventsPanels[i].BackgroundColor = new Color(0, 0, 0, 0.5f);
                    }
                    else
                    {
                        _staticEventsPanels[i].BackgroundColor = new Color(0, 0, 0, 0.2f);
                    }

                    _staticEventTextbox[i] = new Blish_HUD.Controls.TextBox
                    {
                        Text = eventNotes[i].Description,
                        Size = new Point(200, 30),
                        Location = new Point(30, 5),
                        HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Left,
                        Font = GameService.Content.DefaultFont16,
                        HideBackground = true,
                        ForeColor = Color.LimeGreen,
                        Parent = _staticEventsPanels[i]
                    };
                    _staticEventTextbox[i].TextChanged += (s, e) => CurrentEvent_TextChanged(Index);

                    _cancelButton[i] = new Image
                    {
                        Texture = _cancelTexture,
                        Size = new Point(16, 16),
                        Location = new Point(10, 10),
                        Visible = false,
                        Parent = _staticEventsPanels[i]
                    };
                    _cancelButton[i].Click += (s, e) => CancelEvent_Click(Index);
                    _upArrowButton[i] = new Image
                    {
                        Texture = _upArrowTexture,
                        Size = new Point(20, 20),
                        Location = new Point(240, 4),
                        Visible = false,
                        Parent = _staticEventsPanels[i]
                    };
                    _upArrowButton[i].Click += (s, e) => MoveEvent_Click(Index, -1);
                    _downArrowButton[i] = new Image
                    {
                        Texture = _downArrowTexture,
                        Size = new Point(20, 20),
                        Location = new Point(240, 20),
                        Visible = false,
                        Parent = _staticEventsPanels[i]
                    };
                    _downArrowButton[i].Click += (s, e) => MoveEvent_Click(Index, 1);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load events: {ex.Message}");
            }
        }

        private void StaticSettings_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            try
            {
                int senderIndex = Array.IndexOf(_staticEventsPanels, sender);
                var eventNotes = _eventNotes;
                if (_settingsViewContainer != null)
                {
                    _settingsViewContainer.Clear();
                    _settingsViewContainer.Dispose();
                }

                SettingCollection PackageInfo = _MainSettings.AddSubCollection(_CurrentPackage + "_PackageInfo");
                SettingCollection staticCollector = PackageInfo.AddSubCollection(_eventNotes[senderIndex].Description + "_StaticInfo");
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
                _staticWaypoint = staticCollector.DefineSetting(_eventNotes[senderIndex].Description + "_Waypoint", Waypoint, () => "Waypoint", () => "Use to control the note #1");
                _staticWaypoint.SettingChanged += (s2, e2) => LoadWaypointCustomized(senderIndex);

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
                _staticNoteOneDefault = staticCollector.DefineSetting(_eventNotes[senderIndex].Description + "_NoteOne", NoteOne, () => "Note #1", () => "Use to control the note #1");
                _staticNoteOneDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);

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
                _staticNoteTwoDefault = staticCollector.DefineSetting(_eventNotes[senderIndex].Description + "_NoteTwo", NoteTwo, () => "Note #2", () => "Use to control the note #2");
                _staticNoteTwoDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);

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
                _staticNoteThreeDefault = staticCollector.DefineSetting(_eventNotes[senderIndex].Description + "_NoteThree", NoteThree, () => "Note #3", () => "Use to control the note #3");
                _staticNoteThreeDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);

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
                _staticNoteFourDefault = staticCollector.DefineSetting(_eventNotes[senderIndex].Description + "_NoteFour", NoteFour, () => "Note #4", () => "Use to control the note #4");
                _staticNoteFourDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);

                bool BroadcastNoteOne = false;
                if (_eventNotes[senderIndex].Broadcast != null)
                {
                    if (_eventNotes[senderIndex].Broadcast.Count > 0)
                    {
                        BroadcastNoteOne = _eventNotes[senderIndex].Broadcast[0];
                    }
                }
                _staticBroadcastNoteOneDefault = staticCollector.DefineSetting(_eventNotes[senderIndex].Description + "_BroadcastNoteOne", BroadcastNoteOne, () => "Broadcast Note #1", () => "Use to broadcast note #1");
                _staticBroadcastNoteOneDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);

                bool BroadcastNoteTwo = false;
                if (_eventNotes[senderIndex].Broadcast != null)
                {
                    if (_eventNotes[senderIndex].Broadcast.Count > 1)
                    {
                        BroadcastNoteTwo = _eventNotes[senderIndex].Broadcast[1];
                    }
                }
                _staticBroadcastNoteTwoDefault = staticCollector.DefineSetting(_eventNotes[senderIndex].Description + "_BroadcastNoteTwo", BroadcastNoteTwo, () => "Broadcast Note #2", () => "Use to broadcast note #2");
                _staticBroadcastNoteTwoDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);

                bool BroadcastNoteThree = false;
                if (_eventNotes[senderIndex].Broadcast != null)
                {
                    if (_eventNotes[senderIndex].Broadcast.Count > 2)
                    {
                        BroadcastNoteThree = _eventNotes[senderIndex].Broadcast[2];
                    }
                }
                _staticBroadcastNoteThreeDefault = staticCollector.DefineSetting(_eventNotes[senderIndex].Description + "_BroadcastNoteThree", BroadcastNoteThree, () => "Broadcast Note #3", () => "Use to broadcast note #3");
                _staticBroadcastNoteThreeDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);

                bool BroadcastNoteFour = false;
                if (_eventNotes[senderIndex].Broadcast != null)
                {
                    if (_eventNotes[senderIndex].Broadcast.Count > 3)
                    {
                        BroadcastNoteFour = _eventNotes[senderIndex].Broadcast[3];
                    }
                }
                _staticBroadcastNoteFourDefault = staticCollector.DefineSetting(_eventNotes[senderIndex].Description + "_BroadcastNoteFour", BroadcastNoteFour, () => "Broadcast Note #4", () => "Use to broadcast note #4");
                _staticBroadcastNoteFourDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);

                _settingsViewContainer = new ViewContainer
                {
                    Parent = _staticSettingsPanel,
                    Location = new Point(410, 110),
                    Size = new Point(500, 350)
                };
                var settingsView = new SettingsView(staticCollector);
                _settingsViewContainer.Show(settingsView);
                _CurrentEventLabel.Text = _eventNotes[senderIndex].Description;

                //// Re-color panels
                for (int i = 0; i < StaticRowNum; i++)
                {
                    if (i % 2 == 0)
                    {
                        _staticEventsPanels[i].BackgroundColor = new Color(0, 0, 0, 0.5f);
                    }
                    else
                    {
                        _staticEventsPanels[i].BackgroundColor = new Color(0, 0, 0, 0.2f);
                    }
                }
                //// Change color of selected
                _staticEventsPanels[senderIndex].BackgroundColor = new Color(0, 0, 0, 1.0f);

                for (int i = 0; i < StaticRowNum; i++)
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
                if ((senderIndex + 1) != StaticRowNum)
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
                    SettingCollection PackageInfo = _MainSettings.AddSubCollection(_CurrentPackage + "_PackageInfo");
                    SettingCollection staticCollector = PackageInfo.AddSubCollection(eventNotes[i].Description + "_StaticInfo");
                    if (staticCollector != null && staticCollector.Count() > 0)
                    {
                        SettingEntry<string> WaypointSettingEntry = null;
                        staticCollector.TryGetSetting(eventNotes[i].Description + "_Waypoint", out WaypointSettingEntry);
                        SettingEntry<string> NotesOneSettingEntry = null;
                        staticCollector.TryGetSetting(eventNotes[i].Description + "_NoteOne", out NotesOneSettingEntry);
                        SettingEntry<string> NotesTwoSettingEntry = null;
                        staticCollector.TryGetSetting(eventNotes[i].Description + "_NoteTwo", out NotesTwoSettingEntry);
                        SettingEntry<string> NotesThreeSettingEntry = null;
                        staticCollector.TryGetSetting(eventNotes[i].Description + "_NoteThree", out NotesThreeSettingEntry);
                        SettingEntry<string> NotesFourSettingEntry = null;
                        staticCollector.TryGetSetting(eventNotes[i].Description + "_NoteFour", out NotesFourSettingEntry);
                        SettingEntry<bool> BroadcastNotesOneSettingEntry = null;
                        staticCollector.TryGetSetting(eventNotes[i].Description + "_BroadcastNoteOne", out NotesOneSettingEntry);
                        SettingEntry<bool> BroadcastNotesTwoSettingEntry = null;
                        staticCollector.TryGetSetting(eventNotes[i].Description + "_BroadcastNoteTwo", out NotesTwoSettingEntry);
                        SettingEntry<bool> BroadcastNotesThreeSettingEntry = null;
                        staticCollector.TryGetSetting(eventNotes[i].Description + "_BroadcastNoteThree", out NotesThreeSettingEntry);
                        SettingEntry<bool> BroadcastNotesFourSettingEntry = null;
                        staticCollector.TryGetSetting(eventNotes[i].Description + "_BroadcastNoteFour", out NotesFourSettingEntry);

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
        private void LoadWaypointCustomized(int Index)
        {
            try
            {
                List<string> WaypointList = new List<string>();
                if (_staticWaypoint != null)
                {
                    string Waypoint = _staticWaypoint.Value;
                    if (Waypoint != "")
                    {
                        WaypointList.Add(Waypoint);
                    }
                }
                if (WaypointList.Count > 0)
                {
                    _BaubleFarmModule._staticWaypoints[Index].Clear();
                    _BaubleFarmModule._staticWaypoints[Index].AddRange(WaypointList);
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
                if (_staticNoteOneDefault != null)
                {
                    string Notes = _staticNoteOneDefault.Value;
                    if (Notes != "")
                    {
                        NotesList.Add(Notes);
                    }
                }
                if (_staticNoteTwoDefault != null)
                {
                    string Notes = _staticNoteTwoDefault.Value;
                    if (Notes != "")
                    {
                        NotesList.Add(Notes);
                    }
                }
                if (_staticNoteThreeDefault != null)
                {
                    string Notes = _staticNoteThreeDefault.Value;
                    if (Notes != "")
                    {
                        NotesList.Add(Notes);
                    }
                }
                if (_staticNoteFourDefault != null)
                {
                    string Notes = _staticNoteFourDefault.Value;
                    if (Notes != "")
                    {
                        NotesList.Add(Notes);
                    }
                }

                if (_staticBroadcastNoteOneDefault != null)
                {
                    bool BroadcastNotes = _staticBroadcastNoteOneDefault.Value;
                    BroadcastNotesList.Add(BroadcastNotes);
                }
                else
                {
                    BroadcastNotesList.Add(false); // Add false for broadcast if note not null
                }
                if (_staticBroadcastNoteTwoDefault != null)
                {
                    bool BroadcastNotes = _staticBroadcastNoteTwoDefault.Value;
                    BroadcastNotesList.Add(BroadcastNotes);
                }
                else
                {
                    BroadcastNotesList.Add(false); // Add false for broadcast if note not null
                }
                if (_staticBroadcastNoteThreeDefault != null)
                {
                    bool BroadcastNotes = _staticBroadcastNoteThreeDefault.Value;
                    BroadcastNotesList.Add(BroadcastNotes);
                }
                else
                {
                    BroadcastNotesList.Add(false); // Add false for broadcast if note not null
                }
                if (_staticBroadcastNoteFourDefault != null)
                {
                    bool BroadcastNotes = _staticBroadcastNoteFourDefault.Value;
                    BroadcastNotesList.Add(BroadcastNotes);
                }
                else
                {
                    BroadcastNotesList.Add(false); // Add false for broadcast if note not null
                }

                if (NotesList.Count > 0)
                {
                    _BaubleFarmModule._staticNotes[Index].Clear();
                    _BaubleFarmModule._staticNotes[Index].AddRange(NotesList);
                    _eventNotes[Index].Notes.Clear();
                    _eventNotes[Index].Notes.AddRange(NotesList);

                    if (BroadcastNotesList.Count > 0)
                    {
                        _BaubleFarmModule._staticBroadcast[Index].Clear();
                        _BaubleFarmModule._staticBroadcast[Index].AddRange(BroadcastNotesList);
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