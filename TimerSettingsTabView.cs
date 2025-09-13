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
        private List<string> _Descriptions;
        private Image[] _cancelButton;
        private AsyncTexture2D _cancelTexture;
        private Panel _timerSettingsPanel;
        private SettingCollection _settings;
        private SettingEntry<KeyBinding> _timerKeybind;
        private SettingEntry<int> _timerMinutesDefault;
        private SettingEntry<int> _timerSecondsDefault;
        private ViewContainer _settingsViewContainer;

        protected override void Build(Container buildPanel)
        {
            _BaubleFarmModule = BaubleFarmModule.ModuleInstance;
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
                Parent = _timerSettingsPanel
            };
            buttonCreateEvent.Click += CreateEvent_Click;

            _Descriptions = new List<string>{ "SVET", "EVET", "NVET", "WVET", "SAP", "BALTH", "WYVERN", "BRAMBLE", "OOZE", "GUZZLER", "TM", "STONEHEADS" };
            TimerRowNum = 12;
            _timerEventsPanels = new Panel[TimerRowNum];
            _timerEventLabels = new Label[TimerRowNum];
            _cancelButton = new Image[TimerRowNum];
            LoadEventTable(TimerRowNum);
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
        public void LoadEventTable(int RowCount)
        {
            for (int i = 0; i < TimerRowNum; i++)
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
                    Text = _Descriptions[i],
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
                    Parent = _timerEventsPanels[i]
                };
                _cancelButton[i].Click += (s, e) => CancelEvent_Click(i);
            }
        }

        private void TimerSettingsTabView_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            int senderIndex = Array.IndexOf(_timerEventsPanels, sender);
            if (_settingsViewContainer != null)
            {
                _settingsViewContainer.Clear();
                _settingsViewContainer.Dispose();
            }
            _settings = new SettingCollection();
            _timerKeybind = new SettingEntry<KeyBinding>();
            _timerKeybind = _settings.DefineSetting(_Descriptions[senderIndex] + "Keybind", new KeyBinding(Keys.None), () => _Descriptions[senderIndex] + " Keybind", () => "Keybind is used to control " + _Descriptions[senderIndex] + " Timer");
            _timerMinutesDefault = _settings.DefineSetting(_Descriptions[senderIndex] + "TimerMinutes", 0, () => _Descriptions[senderIndex] + " Timer (minutes)", () => "Timer is used to control " + _Descriptions[senderIndex] + " Timer (Minutes)");
            _timerMinutesDefault.SetRange(1, 59);
            _timerSecondsDefault = _settings.DefineSetting(_Descriptions[senderIndex] + "TimerSeconds", 0, () => _Descriptions[senderIndex] + " Timer (seconds)", () => "Timer is used to control " + _Descriptions[senderIndex] + " Timer (Seconds)");
            _timerSecondsDefault.SetRange(1, 59);
            _settingsViewContainer = new ViewContainer
            {
                Parent = _timerSettingsPanel,
                Location = new Point(500, 200),
                Size = new Point(400, 200)
            };
            var settingsView = new SettingsView(_settings);
            _settingsViewContainer.Show(settingsView);
        }
    }
}