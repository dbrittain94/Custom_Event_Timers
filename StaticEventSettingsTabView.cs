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
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace roguishpanda.AB_Bauble_Farm
{
    public class StaticEventSettingsTabView : View
    {
        private static readonly Logger Logger = Logger.GetLogger<MainWindowModule>();
        private MainWindowModule _BaubleFarmModule;
        private AsyncTexture2D _NoTexture;
        private Panel[] _staticEventsPanels;
        private TextBox[] _staticEventTextbox;
        private Panel _staticPackagePanel;
        private int _CurrentEventSelected;
        private Panel _SettingsControlPanel;
        private TextBox _textNewEvent;
        private Label _CreateEventAlert;
        private StandardButton _buttonRestartModule;
        private Label _CurrentEventLabel;
        private Panel _staticEventsTitlePanel;
        private Label _staticEventsTitleLabel;
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
        private Panel _staticSettingsPanel;
        private SettingCollection _MainSettings;
        private List<StaticDetailData> _eventNotes;
        private List<StaticDetailData> _eventNotesReload;
        private List<PackageData> _PackageData;
        private int StaticRowNum;
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
            _eventNotes = new List<StaticDetailData>(_BaubleFarmModule._staticEvents);
            _eventNotesReload = new List<StaticDetailData>(_BaubleFarmModule._staticEvents);
            _PackageData = new List<PackageData>(_BaubleFarmModule._PackageData);
            StaticRowNum = _BaubleFarmModule.StaticRowNum;
            _CurrentPackage = _BaubleFarmModule._CurrentPackage;
            _NoTexture = new AsyncTexture2D();
            _cancelTexture = AsyncTexture2D.FromAssetId(2175782);
            _addTexture = AsyncTexture2D.FromAssetId(155911);
            _broadcastTexture = AsyncTexture2D.FromAssetId(1234950);
            _upArrowTexture = _BaubleFarmModule.ContentsManager.GetTexture(@"png\517181.png");
            _downArrowTexture = _BaubleFarmModule.ContentsManager.GetTexture(@"png\517181-180.png");
            _staticSettingsPanel = new Blish_HUD.Controls.Panel
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Size.X + 500, buildPanel.ContentRegion.Size.Y + 400), // Match the panel to the content region
                Location = new Point(buildPanel.ContentRegion.Location.X, buildPanel.ContentRegion.Location.Y - 35), // Align with content region
                CanScroll = true,
                BackgroundTexture = MainWindowModule.ModuleInstance._asyncTimertexture
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
                Location = new Point(530, 550),
                Visible = false,
                Parent = _staticSettingsPanel
            };
            _buttonSaveEvents.Click += (s, e) => CreateEventJson();
            _buttonReloadEvents = new StandardButton
            {
                Text = "Reload",
                Size = new Point(140, 40),
                Location = new Point(680, 550),
                Visible = false,
                Parent = _staticSettingsPanel
            };
            _buttonReloadEvents.Click += (s, e) => ReloadEvents();
            _CreateEventAlert = new Label
            {
                Size = new Point(400, 40),
                Location = new Point(530, 590),
                Font = GameService.Content.DefaultFont16,
                TextColor = Color.Red,
                Visible = false,
                Parent = _staticSettingsPanel
            };
            _buttonRestartModule = new StandardButton
            {
                Text = "Restart Module",
                Size = new Point(200, 40),
                Location = new Point(530, 550),
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
                for (int i = 0; i < StaticRowNum; i++)
                {
                    _staticEventsPanels[i].Dispose();
                    _staticEventTextbox[i].Dispose();
                    _cancelButton[i].Dispose();
                    _upArrowButton[i].Dispose();
                    _downArrowButton[i].Dispose();
                }

                // Initialize new UI Info
                StaticRowNum = _eventNotes.Count();
                _staticEventsPanels = new Panel[StaticRowNum];
                _staticEventTextbox = new TextBox[StaticRowNum];
                _cancelButton = new Image[StaticRowNum];
                _upArrowButton = new Image[StaticRowNum];
                _downArrowButton = new Image[StaticRowNum];
                LoadEventTable(StaticRowNum);
                StaticSettings_Click(_staticEventsPanels[StaticRowNum - 1], null);
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
                _eventNotes = _eventNotes.Select((note, index) => new StaticDetailData
                {
                    ID = index + 1,
                    Description = note.Description,
                    WaypointData = note.WaypointData,
                    NotesData = note.NotesData
                }).ToList();
                if (_eventNotes.Count <= 0)
                {
                    _staticEventsPanels[0].Dispose();
                    _staticEventTextbox[0].Dispose();
                    _cancelButton[0].Dispose();
                    _upArrowButton[0].Dispose();
                    _downArrowButton[0].Dispose();
                    _SettingsControlPanel.Dispose();
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

                // Initialize new UI Info
                StaticRowNum = _eventNotes.Count();
                _staticEventsPanels = new Panel[StaticRowNum];
                _staticEventTextbox = new TextBox[StaticRowNum];
                _cancelButton = new Image[StaticRowNum];
                _upArrowButton = new Image[StaticRowNum];
                _downArrowButton = new Image[StaticRowNum];
                LoadEventTable(StaticRowNum);
                StaticSettings_Click(_staticEventsPanels[StaticRowNum - 1], null);
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

                // Initialize new UI Info
                StaticRowNum = _eventNotes.Count();
                _staticEventsPanels = new Panel[StaticRowNum];
                _staticEventTextbox = new TextBox[StaticRowNum];
                _cancelButton = new Image[StaticRowNum];
                _upArrowButton = new Image[StaticRowNum];
                _downArrowButton = new Image[StaticRowNum];
                LoadEventTable(StaticRowNum);
                StaticSettings_Click(_staticEventsPanels[Index + Direction], null);
                _buttonSaveEvents.Visible = true;
                _buttonReloadEvents.Visible = true;
                _buttonRestartModule.Visible = false;
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
                _eventNotes = new List<StaticDetailData>(_eventNotesReload);

                // Clear old UI info
                for (int i = 0; i < StaticRowNum; i++)
                {
                    _staticEventsPanels[i].Dispose();
                    _staticEventTextbox[i].Dispose();
                    _cancelButton[i].Dispose();
                    _upArrowButton[i].Dispose();
                    _downArrowButton[i].Dispose();
                }

                // Initialize new UI Info
                StaticRowNum = _eventNotes.Count();
                _staticEventsPanels = new Panel[StaticRowNum];
                _staticEventTextbox = new TextBox[StaticRowNum];
                _cancelButton = new Image[StaticRowNum];
                _upArrowButton = new Image[StaticRowNum];
                _downArrowButton = new Image[StaticRowNum];
                LoadEventTable(StaticRowNum);
                StaticSettings_Click(_staticEventsPanels[StaticRowNum - 1], null);
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
                if (_SettingsControlPanel != null)
                {
                    _SettingsControlPanel.Dispose();
                }
                _CurrentEventLabel.Text = _eventNotes[senderIndex].Description;

                // Control Panel
                _CurrentEventSelected = senderIndex;
                _SettingsControlPanel = new Blish_HUD.Controls.Panel
                {
                    Parent = _staticSettingsPanel,
                    Location = new Point(410, 110),
                    Size = new Point(600, 400),
                    CanScroll = true,
                };
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
                        Location = new Point(0, 0),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Font = GameService.Content.DefaultFont16,
                        Parent = _SettingsControlPanel
                    };
                    _WaypointsTextbox[y] = new Blish_HUD.Controls.TextBox
                    {
                        Size = new Point(350, 40),
                        Location = new Point(110, 0),
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
                        Location = new Point(0, 50 + (currentControlCount * 90)),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Font = GameService.Content.DefaultFont16,
                        Parent = _SettingsControlPanel
                    };
                    _NotesTextbox[z] = new Blish_HUD.Controls.MultilineTextBox
                    {
                        Size = new Point(450, 80),
                        Location = new Point(110, 50 + (currentControlCount * 90)),
                        Font = GameService.Content.DefaultFont16,
                        Parent = _SettingsControlPanel
                    };
                    _broadcastImage[z] = new Image
                    {
                        Texture = _broadcastTexture,
                        Size = new Point(32, 32),
                        Location = new Point(50, 80 + (currentControlCount * 90)),
                        Parent = _SettingsControlPanel
                    };
                    _broadcastCheckbox[z] = new Checkbox
                    {
                        Size = new Point(32, 32),
                        Location = new Point(80, 80 + (currentControlCount * 90)),
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
    }
}