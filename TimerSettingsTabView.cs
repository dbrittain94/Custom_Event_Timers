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
        private Label[] _timerEventLabels;
        private FlowPanel _timerPackagePanel;
        private TextBox _textNewEvent;
        private Label _CreateEventAlert;
        private Label _MinutesLabelDisplay;
        private Label _SecondsLabelDisplay;
        private Label _EventLabelDisplay;
        private Panel _timerEventsTitlePanel;
        private Label _timerEventsTitleLabel;
        private Image[] _cancelButton;
        private AsyncTexture2D _cancelTexture;
        private AsyncTexture2D _addTexture;
        private Panel _timerSettingsPanel;
        private SettingEntry<KeyBinding> _timerKeybind;
        private SettingEntry<int> _timerMinutesDefault;
        private SettingEntry<int> _timerSecondsDefault;
        private SettingEntry<string> _timerNoteOneDefault;
        private SettingEntry<string> _timerNoteTwoDefault;
        private SettingEntry<string> _timerNoteThreeDefault;
        private SettingEntry<string> _timerNoteFourDefault;
        private ViewContainer _settingsViewContainer;
        private SettingCollection _MainSettings;
        private List<NotesData> _eventNotes;
        private int TimerRowNum;
        public readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true // Makes JSON human-readable
        };

        protected override void Build(Container buildPanel)
        {
            _BaubleFarmModule = BaubleFarmModule.ModuleInstance;
            _MainSettings = _BaubleFarmModule._settings;
            _eventNotes = _BaubleFarmModule._eventNotes;
            TimerRowNum = _BaubleFarmModule.TimerRowNum;
            _NoTexture = new AsyncTexture2D();
            _cancelTexture = AsyncTexture2D.FromAssetId(2175782);
            _addTexture = AsyncTexture2D.FromAssetId(1444520);
            _timerSettingsPanel = new Blish_HUD.Controls.Panel
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Size.X + 500, buildPanel.ContentRegion.Size.Y + 400), // Match the panel to the content region
                Location = new Point(buildPanel.ContentRegion.Location.X, buildPanel.ContentRegion.Location.Y - 35), // Align with content region
                BackgroundTexture = BaubleFarmModule.ModuleInstance._asyncTimertexture
            };
            _timerPackagePanel = new Blish_HUD.Controls.FlowPanel
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
            _CreateEventAlert = new Label
            {
                Size = new Point(400, 40),
                Location = new Point(340, 540),
                Font = GameService.Content.DefaultFont16,
                TextColor = Color.Red,
                Visible = false,
                Parent = _timerSettingsPanel
            };

            _MinutesLabelDisplay = new Blish_HUD.Controls.Label
            {
                Size = new Point(100, 40),
                Location = new Point(890, 140),
                Font = GameService.Content.DefaultFont16,
                Parent = _timerSettingsPanel
            };
            _SecondsLabelDisplay = new Blish_HUD.Controls.Label
            {
                Size = new Point(100, 40),
                Location = new Point(890, 160),
                Font = GameService.Content.DefaultFont16,
                Parent = _timerSettingsPanel
            };
            _EventLabelDisplay = new Blish_HUD.Controls.Label
            {
                Size = new Point(180, 40),
                Location = new Point(420, 70),
                Font = GameService.Content.DefaultFont32,
                TextColor = Color.GreenYellow,
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
            _timerEventLabels = new Label[TimerRowNum];
            _cancelButton = new Image[TimerRowNum];
            LoadEventTable(TimerRowNum);
            LoadDefaults(TimerRowNum);
            TimerSettings_Click(_timerEventsPanels[0], null);
        }

        private void CreateEvent_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            var originalNotesData = _eventNotes;
            int maxId = originalNotesData.Max(note => note.ID);
            int NewID = maxId + 1;
            if (_textNewEvent.Text.Length < 3)
            {
                _CreateEventAlert.Text = "* 4 characters mininimum required to create new event";
                _CreateEventAlert.Visible = true;
                return;
            }
            foreach (var Events in originalNotesData)
            {
                if (Events.Description == _CreateEventAlert.Text)
                {
                    _CreateEventAlert.Text = "* This event already exist";
                    _CreateEventAlert.Visible = true;
                    return;
                }
            }
            _CreateEventAlert.Visible = false;

            NotesData notesData = new NotesData()
            {
                ID = NewID,
                Description = _textNewEvent.Text,
                Minutes = 8,
                Seconds = 30,
                Notes = new List<string>() { "" },
            };
            _eventNotes.Add(notesData);

            // Clear old UI info
            for (int i = 0; i < TimerRowNum; i++)
            {
                _timerEventsPanels[i].Dispose();
                _timerEventLabels[i].Dispose();
                _cancelButton[i].Dispose();
            }
            _settingsViewContainer.Clear();
            _settingsViewContainer.Dispose();

            // Initialize new UI Info
            TimerRowNum = _eventNotes.Count();
            _timerEventsPanels = new Panel[TimerRowNum];
            _timerEventLabels = new Label[TimerRowNum];
            _cancelButton = new Image[TimerRowNum];
            LoadEventTable(TimerRowNum);
            LoadDefaults(TimerRowNum);
            TimerSettings_Click(_timerEventsPanels[0], null);
            CreateEventJson();
            _textNewEvent.Text = "";
        }
        private void CreateEventJson()
        {
            List<EventData> eventDataList = new List<EventData>();
            string moduleDir = _BaubleFarmModule.DirectoriesManager.GetFullDirectoryPath("Shiny_Baubles");
            string jsonFilePath = Path.Combine(moduleDir, "Event_Defaults.json");
            try
            {
                string jsonContent = JsonSerializer.Serialize(_eventNotes, _jsonOptions);
                File.WriteAllText(jsonFilePath, jsonContent);
                //Logger.Info($"Saved {_eventDataList.Count} events to {_jsonFilePath}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to save JSON file: {ex.Message}");
            }
        }

        private void CancelEvent_Click(int Index)
        {
            string Description = _eventNotes[Index].Description;
            int ID = _eventNotes[Index].ID;
            _eventNotes.RemoveAll(note => note.Description == Description && note.ID == ID);
            _eventNotes = _eventNotes.Select((note, index) => new NotesData
            {
                ID = index + 1,
                Description = note.Description,
                Minutes = note.Minutes,
                Seconds = note.Seconds,
                Notes = note.Notes
            }).ToList();
            int NewTotal = _eventNotes.Max(note => note.ID);

            // Clear old UI info
            for (int i = 0; i < TimerRowNum; i++)
            {
                _timerEventsPanels[i].Dispose();
                _timerEventLabels[i].Dispose();
                _cancelButton[i].Dispose();
            }
            _settingsViewContainer.Clear();
            _settingsViewContainer.Dispose();

            // Initialize new UI Info
            TimerRowNum = _eventNotes.Count();
            _timerEventsPanels = new Panel[TimerRowNum];
            _timerEventLabels = new Label[TimerRowNum];
            _cancelButton = new Image[TimerRowNum];
            LoadEventTable(TimerRowNum);
            LoadDefaults(TimerRowNum);
            TimerSettings_Click(_timerEventsPanels[0], null);
            CreateEventJson();
            SettingCollection TimerCollector = _MainSettings.AddSubCollection(Description + "TimerInfo");
            TimerCollector.UndefineSetting(Description + "TimerInfo");
        }
        public void LoadEventTable(int TotalEvents)
        {
            var eventNotes = _eventNotes;
            for (int i = 0; i < TotalEvents; i++)
            {
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

                _timerEventLabels[i] = new Blish_HUD.Controls.Label
                {
                    Text = eventNotes[i].Description,
                    Size = new Point(100, 40),
                    Location = new Point(40, 0),
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Left,
                    Font = GameService.Content.DefaultFont16,
                    TextColor = Color.GreenYellow,
                    Parent = _timerEventsPanels[i]
                };

                _cancelButton[i] = new Image
                {
                    Texture = _cancelTexture,
                    Size = new Point(16, 16),
                    Location = new Point(10, 10),
                    //Visible = false,
                    Parent = _timerEventsPanels[i]
                };
                int Index = i;
                _cancelButton[i].Click += (s, e) => CancelEvent_Click(Index);
            }
        }

        private void TimerSettings_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            int senderIndex = Array.IndexOf(_timerEventsPanels, sender);
            var eventNotes = _eventNotes;
            if (_settingsViewContainer != null)
            {
                _settingsViewContainer.Clear();
                _settingsViewContainer.Dispose();
            }
            //_settings = new SettingCollection();
            SettingCollection TimerCollector = _MainSettings.AddSubCollection(_eventNotes[senderIndex].Description + "TimerInfo");
            _timerKeybind = new SettingEntry<KeyBinding>();
            _timerKeybind = TimerCollector.DefineSetting(_eventNotes[senderIndex].Description + "Keybind", new KeyBinding(Keys.None), () => "Keybind", () => "Keybind is used to control start/stop for timer");
            _timerMinutesDefault = TimerCollector.DefineSetting(_eventNotes[senderIndex].Description + "TimerMinutes", 10, () => "Timer (minutes)", () => "Use to control minutes on the timer");
            _timerMinutesDefault.SetRange(1, 59);
            _MinutesLabelDisplay.Text = _timerMinutesDefault.Value.ToString() + " Minutes";
            _timerMinutesDefault.SettingChanged += (s2, e2) => LoadTimeCustomized(senderIndex);
            _timerSecondsDefault = TimerCollector.DefineSetting(_eventNotes[senderIndex].Description + "TimerSeconds", 30, () => "Timer (seconds)", () => "Use to control seconds on the timer");
            _timerSecondsDefault.SetRange(1, 59);
            _SecondsLabelDisplay.Text = _timerSecondsDefault.Value.ToString() + " Seconds";
            _timerSecondsDefault.SettingChanged += (s2, e2) => LoadTimeCustomized(senderIndex);
            _timerNoteOneDefault = TimerCollector.DefineSetting(_eventNotes[senderIndex].Description + "NoteOne", "", () => "Note #1", () => "Use to control the note #1");
            _timerNoteOneDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);
            _timerNoteTwoDefault = TimerCollector.DefineSetting(_eventNotes[senderIndex].Description + "NoteTwo", "", () => "Note #2", () => "Use to control the note #2");
            _timerNoteTwoDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);
            _timerNoteThreeDefault = TimerCollector.DefineSetting(_eventNotes[senderIndex].Description + "NoteThree", "", () => "Note #3", () => "Use to control the note #3");
            _timerNoteThreeDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);
            _timerNoteFourDefault = TimerCollector.DefineSetting(_eventNotes[senderIndex].Description + "NoteFour", "", () => "Note #4", () => "Use to control the note #4");
            _timerNoteFourDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);
            _settingsViewContainer = new ViewContainer
            {
                Parent = _timerSettingsPanel,
                Location = new Point(410, 120),
                Size = new Point(500, 400)
            };
            var settingsView = new SettingsView(TimerCollector);
            _settingsViewContainer.Show(settingsView);
            _EventLabelDisplay.Text = _eventNotes[senderIndex].Description;
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
        }
        private void LoadDefaults(int TotalEvents)
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
                    SettingEntry<string> NotesOneSettingEntry = null;
                    TimerCollector.TryGetSetting(eventNotes[i].Description + "NoteOne", out NotesOneSettingEntry);
                    SettingEntry<string> NotesTwoSettingEntry = null;
                    TimerCollector.TryGetSetting(eventNotes[i].Description + "NoteTwo", out NotesTwoSettingEntry);
                    SettingEntry<string> NotesThreeSettingEntry = null;
                    TimerCollector.TryGetSetting(eventNotes[i].Description + "NoteThree", out NotesThreeSettingEntry);
                    SettingEntry<string> NotesFourSettingEntry = null;
                    TimerCollector.TryGetSetting(eventNotes[i].Description + "NoteFour", out NotesFourSettingEntry);

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

                    List<string> NotesList = new List<string>();
                    if (NotesOneSettingEntry != null)
                    {
                        string Notes = NotesOneSettingEntry.Value;
                        if (eventNotes[i].Notes.Count >= 0)
                        {
                            if (Notes == "" && eventNotes[i].Notes[0] != null)
                            {
                                NotesOneSettingEntry.Value = eventNotes[i].Notes[0];
                            }
                        }
                    }
                    if (NotesTwoSettingEntry != null)
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
                    if (NotesThreeSettingEntry != null)
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
                    if (NotesFourSettingEntry != null)
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
            }
        }
        private void LoadTimeCustomized(int Index)
        {
            TimeSpan Minutes = TimeSpan.FromMinutes(_timerMinutesDefault.Value);
            TimeSpan Seconds = TimeSpan.FromSeconds(_timerSecondsDefault.Value);
            _MinutesLabelDisplay.Text = _timerMinutesDefault.Value.ToString() + " Minutes";
            _SecondsLabelDisplay.Text = _timerSecondsDefault.Value.ToString() + " Seconds";
            _BaubleFarmModule._timerDurationDefaults[Index] = Minutes + Seconds;
            _BaubleFarmModule._timerLabels[Index].Text = _BaubleFarmModule._timerDurationDefaults[Index].ToString(@"mm\:ss");
        }
        private void LoadNotesCustomized(int Index)
        {
            List<string> NotesList = new List<string>();
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
            if (NotesList.Count > 0)
            {
                _BaubleFarmModule._Notes[Index].Clear();
                _BaubleFarmModule._Notes[Index].AddRange(NotesList);
            }
        }
    }
}