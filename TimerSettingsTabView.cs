using Blish_HUD;
using Blish_HUD.Common.Gw2;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime;

namespace roguishpanda.AB_Bauble_Farm
{
    public class TimerSettingsTabView : View
    {
        private BaubleFarmModule _BaubleFarmModule;
        private AsyncTexture2D _NoTexture;
        private Panel[] _timerEventsPanels;
        private Label[] _timerEventLabels;
        private FlowPanel _timerPackagePanel;
        private int TimerRowNum;
        private KeybindingAssignmentWindow _assignerWindow;
        private Label _MinutesLabelDisplay;
        private Label _SecondsLabelDisplay;
        private Label _EventLabelDisplay;
        private List<string> _Descriptions;
        private Image[] _cancelButton;
        private AsyncTexture2D _cancelTexture;
        private Panel _timerSettingsPanel;
        private SettingCollection _settings;
        private SettingEntry<KeyBinding> _timerKeybind;
        private SettingEntry<int> _timerMinutesDefault;
        private SettingEntry<int> _timerSecondsDefault;
        private SettingEntry<string> _timerNoteOneDefault;
        private SettingEntry<string> _timerNoteTwoDefault;
        private SettingEntry<string> _timerNoteThreeDefault;
        private SettingEntry<string> _timerNoteFourDefault;
        private ViewContainer _settingsViewContainer;
        private SettingCollection _MainSettings;

        protected override void Build(Container buildPanel)
        {
            _BaubleFarmModule = BaubleFarmModule.ModuleInstance;
            _MainSettings = _BaubleFarmModule._settings;
            TimerRowNum = _BaubleFarmModule.TimerRowNum;
            _NoTexture = new AsyncTexture2D();
            _cancelTexture = AsyncTexture2D.FromAssetId(2175782);
            _timerSettingsPanel = new Blish_HUD.Controls.Panel
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Size.X + 500, buildPanel.ContentRegion.Size.Y + 535), // Match the panel to the content region
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

            StandardButton buttonCreateEvent = new StandardButton
            {
                Text = "Create Event",
                Size = new Point(100, 40),
                Location = new Point(100, 520),
                Visible = false,
                Parent = _timerSettingsPanel
            };
            buttonCreateEvent.Click += CreateEvent_Click;

            _MinutesLabelDisplay = new Blish_HUD.Controls.Label
            {
                Size = new Point(100, 40),
                Location = new Point(880, 120),
                Font = GameService.Content.DefaultFont16,
                Parent = _timerSettingsPanel
            };
            _SecondsLabelDisplay = new Blish_HUD.Controls.Label
            {
                Size = new Point(100, 40),
                Location = new Point(880, 140),
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

            _Descriptions = new List<string>{ "SVET", "EVET", "NVET", "WVET", "SAP", "BALTH", "WYVERN", "BRAMBLE", "OOZE", "GUZZLER", "TM", "STONEHEADS" };
            _timerEventsPanels = new Panel[TimerRowNum];
            _timerEventLabels = new Label[TimerRowNum];
            _cancelButton = new Image[TimerRowNum];
            LoadEventTable(TimerRowNum);
            LoadDefaults(TimerRowNum);
        }

        private void Assigner_Click(object sender, MouseEventArgs e)
        {
            _assignerWindow.Show();
        }

        private void CreateEvent_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _Descriptions.Add("New Event");
            for (int i = 0; i < TimerRowNum; i++)
            {
                _timerEventsPanels[i].Dispose();
                _timerEventLabels[i].Dispose();
            }
            TimerRowNum = TimerRowNum + 1;
            _timerEventsPanels = new Panel[TimerRowNum];
            _timerEventLabels = new Label[TimerRowNum];
            _cancelButton = new Image[TimerRowNum];
            LoadEventTable(TimerRowNum);
        }
        private void CancelEvent_Click(int Index)
        {
            _Descriptions.RemoveAt(Index - 1);
            for (int i = 0; i < TimerRowNum; i++)
            {
                _timerEventsPanels[i].Dispose();
                _timerEventLabels[i].Dispose();
                _cancelButton[i].Dispose();
            }
            TimerRowNum = TimerRowNum - 1;
            _timerEventsPanels = new Panel[TimerRowNum];
            _timerEventLabels = new Label[TimerRowNum];
            _cancelButton = new Image[TimerRowNum];
            LoadEventTable(TimerRowNum);
        }
        public void LoadEventTable(int TotalEvents)
        {
            for (int i = 0; i < TotalEvents; i++)
            {
                // Package Panels
                _timerEventsPanels[i] = new Blish_HUD.Controls.Panel
                {
                    Parent = _timerPackagePanel,
                    Size = new Point(300, 40),
                    Location = new Point(0, (i * 40)),
                };
                _timerEventsPanels[i].Click += TimerSettingsTabView_Click;

                if (i % 2 == 0)
                {
                    _timerEventsPanels[i].BackgroundColor = new Color(0, 0, 0, 0.5f);
                }
                else
                {
                    _timerEventsPanels[i].BackgroundColor = new Color(0, 0, 0, 0.2f);
                }

                // Package label
                _timerEventLabels[i] = new Blish_HUD.Controls.Label
                {
                    Text = _BaubleFarmModule._timerLabelDescriptions[i].Text,
                    Size = new Point(100, 40),
                    Location = new Point(20, 0),
                    HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Left,
                    Font = GameService.Content.DefaultFont16,
                    TextColor = Color.GreenYellow,
                    Parent = _timerEventsPanels[i]
                };

                _cancelButton[i] = new Image
                {
                    Texture = _cancelTexture,
                    Size = new Point(26, 26),
                    Location = new Point(240, 0),
                    Visible = false,
                    Parent = _timerEventsPanels[i]
                };
                _cancelButton[i].Click += (s, e) => CancelEvent_Click(i);
            }
        }

        private void TimerSettingsTabView_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            int senderIndex = Array.IndexOf(_timerEventsPanels, sender);
            Label[] Description = _BaubleFarmModule._timerLabelDescriptions;
            if (_settingsViewContainer != null)
            {
                _settingsViewContainer.Clear();
                _settingsViewContainer.Dispose();
            }
            //_settings = new SettingCollection();
            SettingCollection TimerCollector = _MainSettings.AddSubCollection(Description[senderIndex].Text + "TimerInfo");
            _timerKeybind = new SettingEntry<KeyBinding>();
            _timerKeybind = TimerCollector.DefineSetting(Description[senderIndex].Text + "Keybind", new KeyBinding(Keys.None), () => "Keybind", () => "Keybind is used to control start/stop for timer");
            _timerMinutesDefault = TimerCollector.DefineSetting(Description[senderIndex].Text + "TimerMinutes", 10, () => "Timer (minutes)", () => "Use to control minutes on the timer");
            _timerMinutesDefault.SetRange(1, 59);
            _MinutesLabelDisplay.Text = _timerMinutesDefault.Value.ToString() + " Minutes";
            _timerMinutesDefault.SettingChanged += (s2, e2) => LoadTimeCustomized(senderIndex);
            _timerSecondsDefault = TimerCollector.DefineSetting(Description[senderIndex].Text + "TimerSeconds", 30, () => "Timer (seconds)", () => "Use to control seconds on the timer");
            _timerSecondsDefault.SetRange(1, 59);
            _SecondsLabelDisplay.Text = _timerSecondsDefault.Value.ToString() + " Seconds";
            _timerSecondsDefault.SettingChanged += (s2, e2) => LoadTimeCustomized(senderIndex);
            _timerNoteOneDefault = TimerCollector.DefineSetting(Description[senderIndex].Text + "NoteOne", "", () => "Note #1", () => "Use to control the note #1");
            _timerNoteOneDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);
            _timerNoteTwoDefault = TimerCollector.DefineSetting(Description[senderIndex].Text + "NoteTwo", "", () => "Note #2", () => "Use to control the note #2");
            _timerNoteTwoDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);
            _timerNoteThreeDefault = TimerCollector.DefineSetting(Description[senderIndex].Text + "NoteThree", "", () => "Note #3", () => "Use to control the note #3");
            _timerNoteThreeDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);
            _timerNoteFourDefault = TimerCollector.DefineSetting(Description[senderIndex].Text + "NoteFour", "", () => "Note #4", () => "Use to control the note #4");
            _timerNoteFourDefault.SettingChanged += (s2, e2) => LoadNotesCustomized(senderIndex);
            _settingsViewContainer = new ViewContainer
            {
                Parent = _timerSettingsPanel,
                Location = new Point(400, 100),
                Size = new Point(500, 400)
            };
            var settingsView = new SettingsView(TimerCollector);
            _settingsViewContainer.Show(settingsView);
            _EventLabelDisplay.Text = Description[senderIndex].Text;

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
            for (int i = 0; i < TotalEvents; i++)
            {
                SettingCollection TimerCollector = _MainSettings.AddSubCollection(_BaubleFarmModule._timerLabelDescriptions[i].Text + "TimerInfo");
                SettingEntry<int> MintuesSettingEntry = null;
                TimerCollector.TryGetSetting(_BaubleFarmModule._timerLabelDescriptions[i].Text + "TimerMinutes", out MintuesSettingEntry);
                SettingEntry<int> SecondsSettingEntry = null;
                TimerCollector.TryGetSetting(_BaubleFarmModule._timerLabelDescriptions[i].Text + "TimerSeconds", out SecondsSettingEntry);
                SettingEntry<string> NotesOneSettingEntry = null;
                TimerCollector.TryGetSetting(_BaubleFarmModule._timerLabelDescriptions[i].Text + "NoteOne", out NotesOneSettingEntry);
                SettingEntry<string> NotesTwoSettingEntry = null;
                TimerCollector.TryGetSetting(_BaubleFarmModule._timerLabelDescriptions[i].Text + "NoteTwo", out NotesTwoSettingEntry);
                SettingEntry<string> NotesThreeSettingEntry = null;
                TimerCollector.TryGetSetting(_BaubleFarmModule._timerLabelDescriptions[i].Text + "NoteThree", out NotesThreeSettingEntry);
                SettingEntry<string> NotesFourSettingEntry = null;
                TimerCollector.TryGetSetting(_BaubleFarmModule._timerLabelDescriptions[i].Text + "NoteFour", out NotesFourSettingEntry);

                double Minutes = _BaubleFarmModule._TimerMinutes[i];
                double Seconds = _BaubleFarmModule._TimerSeconds[i];
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
                    if (_BaubleFarmModule._Notes[i].Count >= 0)
                    {
                        if (Notes == "" && _BaubleFarmModule._Notes[i][0] != null)
                        {
                            NotesOneSettingEntry.Value = _BaubleFarmModule._Notes[i][0];
                        }
                    }
                }
                if (NotesTwoSettingEntry != null)
                {
                    string Notes = NotesTwoSettingEntry.Value;
                    if (_BaubleFarmModule._Notes[i].Count > 1)
                    {
                        if (Notes == "" && _BaubleFarmModule._Notes[i][1] != null)
                        {
                            NotesTwoSettingEntry.Value = _BaubleFarmModule._Notes[i][1];
                        }
                    }
                }
                if (NotesThreeSettingEntry != null)
                {
                    string Notes = NotesThreeSettingEntry.Value;
                    if (_BaubleFarmModule._Notes[i].Count > 2)
                    {
                        if (Notes == "" && _BaubleFarmModule._Notes[i][2] != null)
                        {
                            NotesThreeSettingEntry.Value = _BaubleFarmModule._Notes[i][2];
                        }
                    }
                }
                if (NotesFourSettingEntry != null)
                {
                    string Notes = NotesFourSettingEntry.Value;
                    if (_BaubleFarmModule._Notes[i].Count > 3)
                    {
                        if (Notes == "" && _BaubleFarmModule._Notes[i][3] != null)
                        {
                            NotesFourSettingEntry.Value = _BaubleFarmModule._Notes[i][3];
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