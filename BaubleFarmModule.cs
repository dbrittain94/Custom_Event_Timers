using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace roguishpanda.AB_Bauble_Farm
{
    public class EventData
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public DateTime? StartTime { get; set; }
        public bool IsActive { get; set; }
    }

    [Export(typeof(Blish_HUD.Modules.Module))]
    public class BaubleFarmModule : Blish_HUD.Modules.Module
    {
        private static readonly Logger Logger = Logger.GetLogger<BaubleFarmModule>();
        internal static BaubleFarmModule ModuleInstance;

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;

        #endregion

        private Label[] _timerLabelDescriptions;
        private Label[] _timerLabels;
        private Label _statusValue;
        private Label _startTimeValue;
        private Label _endTimeValue;
        private Checkbox _InOrdercheckbox;
        private DateTime elapsedDateTime;
        private DateTime initialDateTime;
        private int TimerRowNum = 12;
        private StandardButton _stopButton;
        private StandardButton[] _stopButtons;
        private StandardButton[] _resetButtons;
        private Dropdown[] _customDropdownTimers;
        private DateTime?[] _timerStartTimes; // Nullable to track if timer is started
        private bool[] _timerRunning; // Track running state
        private TimeSpan[] _timerDurationDefaults;
        private TimeSpan[] _timerDurationOverride;
        private Panel[] _TimerWindowsOrdered;
        private Panel _infoPanel;
        private Panel _timerPanel;
        private StandardWindow _TimerWindow;
        private StandardWindow _InfoWindow;
        private CornerIcon _cornerIcon;
        private SettingEntry<KeyBinding> _toggleTimerWindowKeybind;
        private SettingEntry<KeyBinding> _toggleInfoWindowKeybind;
        private SettingEntry<KeyBinding> _svetKeybind;
        private SettingEntry<KeyBinding> _evetKeybind;
        private SettingEntry<KeyBinding> _nvetKeybind;
        private SettingEntry<KeyBinding> _wvetKeybind;
        private SettingEntry<KeyBinding> _sapKeybind;
        private SettingEntry<KeyBinding> _balthKeybind;
        private SettingEntry<KeyBinding> _wyvernKeybind;
        private SettingEntry<KeyBinding> _brambleKeybind;
        private SettingEntry<KeyBinding> _oozeKeybind;
        private SettingEntry<KeyBinding> _guzzlerKeybind;
        private SettingEntry<KeyBinding> _tmKeybind;
        private SettingEntry<KeyBinding> _stoneheadKeybind;
        private SettingEntry<bool> _InOrdercheckboxDefault;
        private SettingEntry<float> _OpacityDefault;
        private SettingEntry<int> _timerLowDefault;
        private SettingEntry<int> _timerSVETdefault;
        private SettingEntry<int> _timerEVETdefault;
        private SettingEntry<int> _timerNVETdefault;
        private SettingEntry<int> _timerWVETdefault;
        private SettingEntry<int> _timerSAPdefault;
        private SettingEntry<int> _timerBALTHdefault;
        private SettingEntry<int> _timerWYVERNdefault;
        private SettingEntry<int> _timerBRAMBLEdefault;
        private SettingEntry<int> _timerOOZEdefault;
        private SettingEntry<int> _timerGUZZLERdefault;
        private SettingEntry<int> _timerTMdefault;
        private SettingEntry<int> _timerSTONEHEADSdefault;
        private AsyncTexture2D _asyncTimertexture;
        private string _jsonFilePath;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true // Makes JSON human-readable
        };

        [ImportingConstructor]
        public BaubleFarmModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ModuleInstance = this;
        }

        protected override void DefineSettings(SettingCollection settings)
        {

            _InOrdercheckboxDefault = settings.DefineSetting("InOrdercheckboxDefault", false, () => "Order by Timer", () => "Check this box if you want to order your timers by time.");

            _OpacityDefault = settings.DefineSetting("OpacityDefault", 1.0f, () => "Window Opacity", () => "Changing the opacity will adjust how translucent the windows are.");
            _OpacityDefault.SetRange(0.1f, 1.0f);
            _OpacityDefault.SettingChanged += ChangeOpacity_Activated;

            _toggleTimerWindowKeybind = settings.DefineSetting("TimerKeybinding",new KeyBinding(ModifierKeys.Shift, Keys.L),() => "Timer Window",() => "Keybind to show or hide the Timer window.");
            _toggleTimerWindowKeybind.Value.BlockSequenceFromGw2 = true;
            _toggleTimerWindowKeybind.Value.Enabled = true;
            _toggleTimerWindowKeybind.Value.Activated += ToggleTimerWindowKeybind_Activated;

            _toggleInfoWindowKeybind = settings.DefineSetting("InfoKeybinding",new KeyBinding(ModifierKeys.Shift, Keys.OemSemicolon),() => "Info Window",() => "Keybind to show or hide the Information window.");
            _toggleInfoWindowKeybind.Value.BlockSequenceFromGw2 = true;
            _toggleInfoWindowKeybind.Value.Enabled = true;
            _toggleInfoWindowKeybind.Value.Activated += ToggleInfoWindowKeybind_Activated;

            _svetKeybind = settings.DefineSetting("svetKeybinding", new KeyBinding(Keys.None), () => "SVET Keybinding", () => "Keybind to start/stop svet timer.");
            _svetKeybind.Value.BlockSequenceFromGw2 = true;
            _svetKeybind.Value.Enabled = true;
            _svetKeybind.Value.Activated += (sender, e) => timerKeybinds(0);

            _evetKeybind = settings.DefineSetting("evetKeybinding", new KeyBinding(Keys.None), () => "EVET Keybinding", () => "Keybind to start/stop evet timer.");
            _evetKeybind.Value.BlockSequenceFromGw2 = true;
            _evetKeybind.Value.Enabled = true;
            _evetKeybind.Value.Activated += (sender, e) => timerKeybinds(1);

            _nvetKeybind = settings.DefineSetting("nvetKeybinding", new KeyBinding(Keys.None), () => "NVET Keybinding", () => "Keybind to start/stop nvet timer.");
            _nvetKeybind.Value.BlockSequenceFromGw2 = true;
            _nvetKeybind.Value.Enabled = true;
            _nvetKeybind.Value.Activated += (sender, e) => timerKeybinds(2);

            _wvetKeybind = settings.DefineSetting("wvetKeybinding", new KeyBinding(Keys.None), () => "WVET Keybinding", () => "Keybind to start/stop wvet timer.");
            _wvetKeybind.Value.BlockSequenceFromGw2 = true;
            _wvetKeybind.Value.Enabled = true;
            _wvetKeybind.Value.Activated += (sender, e) => timerKeybinds(3);

            _sapKeybind = settings.DefineSetting("sapKeybinding", new KeyBinding(Keys.None), () => "SAP Keybinding", () => "Keybind to start/stop sap timer.");
            _sapKeybind.Value.BlockSequenceFromGw2 = true;
            _sapKeybind.Value.Enabled = true;
            _sapKeybind.Value.Activated += (sender, e) => timerKeybinds(4);

            _balthKeybind = settings.DefineSetting("balthKeybinding", new KeyBinding(Keys.None), () => "BALTH Keybinding", () => "Keybind to start/stop balth timer.");
            _balthKeybind.Value.BlockSequenceFromGw2 = true;
            _balthKeybind.Value.Enabled = true;
            _balthKeybind.Value.Activated += (sender, e) => timerKeybinds(5);

            _wyvernKeybind = settings.DefineSetting("wyvernKeybinding", new KeyBinding(Keys.None), () => "WYVERN Keybinding", () => "Keybind to start/stop wyvern timer.");
            _wyvernKeybind.Value.BlockSequenceFromGw2 = true;
            _wyvernKeybind.Value.Enabled = true;
            _wyvernKeybind.Value.Activated += (sender, e) => timerKeybinds(6);

            _brambleKeybind = settings.DefineSetting("brambleKeybinding", new KeyBinding(Keys.None), () => "BRAMBLE Keybinding", () => "Keybind to start/stop bramble timer.");
            _brambleKeybind.Value.BlockSequenceFromGw2 = true;
            _brambleKeybind.Value.Enabled = true;
            _brambleKeybind.Value.Activated += (sender, e) => timerKeybinds(7);

            _oozeKeybind = settings.DefineSetting("oozeKeybinding", new KeyBinding(Keys.None), () => "OOZE Keybinding", () => "Keybind to start/stop ooze timer.");
            _oozeKeybind.Value.BlockSequenceFromGw2 = true;
            _oozeKeybind.Value.Enabled = true;
            _oozeKeybind.Value.Activated += (sender, e) => timerKeybinds(8);

            _guzzlerKeybind = settings.DefineSetting("guzzlerKeybinding", new KeyBinding(Keys.None), () => "GUZZLER Keybinding", () => "Keybind to start/stop guzzler timer.");
            _guzzlerKeybind.Value.BlockSequenceFromGw2 = true;
            _guzzlerKeybind.Value.Enabled = true;
            _guzzlerKeybind.Value.Activated += (sender, e) => timerKeybinds(9);

            _tmKeybind = settings.DefineSetting("tmKeybinding", new KeyBinding(Keys.None), () => "TM Keybinding", () => "Keybind to start/stop TM timer.");
            _tmKeybind.Value.BlockSequenceFromGw2 = true;
            _tmKeybind.Value.Enabled = true;
            _tmKeybind.Value.Activated += (sender, e) => timerKeybinds(10);

            _stoneheadKeybind = settings.DefineSetting("stoneheadKeybinding", new KeyBinding(Keys.None), () => "STONEHEAD Keybinding", () => "Keybind to start/stop stonehead timer.");
            _stoneheadKeybind.Value.BlockSequenceFromGw2 = true;
            _stoneheadKeybind.Value.Enabled = true;
            _stoneheadKeybind.Value.Activated += (sender, e) => timerKeybinds(11);

            _timerLowDefault = settings.DefineSetting("LowTimerDefaultTimer",30,() => "Low Timer",() => "Set timer for when timer gets below certain threshold in seconds.");
            _timerLowDefault.SetRange(1, 120);

            _timerSVETdefault = settings.DefineSetting("SVETDefaultTimer",10,() => "SVET Timer",() => "Set timer for SVET in minutes.");
            _timerSVETdefault.SetRange(1, 10);
            _timerSVETdefault.SettingChanged += (sender, e) => timerDefaults(0);

            _timerEVETdefault = settings.DefineSetting("EVETDefaultTimer",10,() => "EVET Timer", () => "Set timer for EVET in minutes.");
            _timerEVETdefault.SetRange(1, 10);
            _timerEVETdefault.SettingChanged += (sender, e) => timerDefaults(1);

            _timerNVETdefault = settings.DefineSetting("NVETDefaultTimer",10,() => "NVET Timer", () => "Set timer for NVET in minutes.");
            _timerNVETdefault.SetRange(1, 10);
            _timerNVETdefault.SettingChanged += (sender, e) => timerDefaults(2);

            _timerWVETdefault = settings.DefineSetting("WVETDefaultTimer",10,() => "WVET Timer", () => "Set timer for WVET in minutes.");
            _timerWVETdefault.SetRange(1, 10);
            _timerWVETdefault.SettingChanged += (sender, e) => timerDefaults(3);

            _timerSAPdefault = settings.DefineSetting("SAPDefaultTimer",8,() => "SAP Timer", () => "Set timer for SAP in minutes.");
            _timerSAPdefault.SetRange(1, 10);
            _timerSAPdefault.SettingChanged += (sender, e) => timerDefaults(4);

            _timerBALTHdefault = settings.DefineSetting("BALTHDefaultTimer",8,() => "BALTH Timer", () => "Set timer for BALTH in minutes.");
            _timerBALTHdefault.SetRange(1, 10);
            _timerBALTHdefault.SettingChanged += (sender, e) => timerDefaults(5);

            _timerWYVERNdefault = settings.DefineSetting("WYVERNDefaultTimer",13,() => "WYVERN Timer", () => "Set timer for WYVERN in minutes.");
            _timerWYVERNdefault.SetRange(1, 15);
            _timerWYVERNdefault.SettingChanged += (sender, e) => timerDefaults(6);

            _timerBRAMBLEdefault = settings.DefineSetting("BRAMBLEDefaultTimer",13,() => "BRAMBLE Timer", () => "Set timer for BRAMBLE in minutes.");
            _timerBRAMBLEdefault.SetRange(1, 15);
            _timerBRAMBLEdefault.SettingChanged += (sender, e) => timerDefaults(7);

            _timerOOZEdefault = settings.DefineSetting("OOZEDefaultTimer",14,() => "OOZE Timer", () => "Set timer for OOZE in minutes.");
            _timerOOZEdefault.SetRange(1, 15);
            _timerOOZEdefault.SettingChanged += (sender, e) => timerDefaults(8);

            _timerGUZZLERdefault = settings.DefineSetting("GUZZLERDefaultTimer",13,() => "GUZZLER Timer", () => "Set timer for GUZZLER in minutes.");
            _timerGUZZLERdefault.SetRange(1, 15);
            _timerGUZZLERdefault.SettingChanged += (sender, e) => timerDefaults(9);

            _timerTMdefault = settings.DefineSetting("TMDefaultTimer",10,() => "TM Timer", () => "Set timer for TM in minutes.");
            _timerTMdefault.SetRange(1, 10);
            _timerTMdefault.SettingChanged += (sender, e) => timerDefaults(10);

            _timerSTONEHEADSdefault = settings.DefineSetting("STONEHEADSDefaultTimer",12,() => "STONEHEADS Timer", () => "Set timer for STONEHEADS in minutes.");
            _timerSTONEHEADSdefault.SetRange(1, 15);
            _timerSTONEHEADSdefault.SettingChanged += (sender, e) => timerDefaults(11);
        }
        private void ToggleTimerWindowKeybind_Activated(object sender, EventArgs e)
        {
            if (_TimerWindow.Visible)
            {
                _TimerWindow.Hide();
            }
            else
            {
                _TimerWindow.Show();
            }
        }
        private void ToggleInfoWindowKeybind_Activated(object sender, EventArgs e)
        {
            if (_InfoWindow.Visible)
            {
                _InfoWindow.Hide();
            }
            else
            {
                _InfoWindow.Show();
            }
        }
        private void ChangeOpacity_Activated(object sender, EventArgs e)
        {
            _infoPanel.Opacity = _OpacityDefault.Value;
            _timerPanel.Opacity = _OpacityDefault.Value;
        }
        private void timerKeybinds(int timerIndex)
        {
            if (_resetButtons[timerIndex].Enabled == true)
            {
                ResetButton_Click(timerIndex);
            }
            else
            {
                stopButtons_Click(timerIndex);
            }
        }
        private void timerDefaults(int timerIndex)
        {
            int[] timerDefaultValues = { _timerSVETdefault.Value, _timerEVETdefault.Value, _timerNVETdefault.Value, _timerWVETdefault.Value, _timerSAPdefault.Value,
            _timerBALTHdefault.Value, _timerWYVERNdefault.Value, _timerBRAMBLEdefault.Value, _timerOOZEdefault.Value, _timerGUZZLERdefault.Value,
            _timerTMdefault.Value, _timerSTONEHEADSdefault.Value
            };

            _timerDurationDefaults[timerIndex] = TimeSpan.FromMinutes(timerDefaultValues[timerIndex]);
            _timerLabels[timerIndex].Text = $"{_timerDurationDefaults[timerIndex]:mm\\:ss}";
            if (_timerRunning[timerIndex] == false)
            {
            }
        }
        protected override void Initialize()
        {
            #region Initialize Defaults
            _timerStartTimes = new DateTime?[TimerRowNum];
            _timerRunning = new bool[TimerRowNum];
            _timerLabelDescriptions = new Label[TimerRowNum];
            _timerLabels = new Label[TimerRowNum];
            _resetButtons = new StandardButton[TimerRowNum];
            _stopButtons = new StandardButton[TimerRowNum];
            _customDropdownTimers = new Dropdown[TimerRowNum];
            _TimerWindowsOrdered = new Panel[TimerRowNum];
            _timerDurationOverride = new TimeSpan[TimerRowNum];
            _timerDurationDefaults = new TimeSpan[TimerRowNum];

            // Initialize Timer Defaults
            int[] timerDefaultValues = { _timerSVETdefault.Value, _timerEVETdefault.Value, _timerNVETdefault.Value, _timerWVETdefault.Value, _timerSAPdefault.Value,
            _timerBALTHdefault.Value, _timerWYVERNdefault.Value, _timerBRAMBLEdefault.Value, _timerOOZEdefault.Value, _timerGUZZLERdefault.Value,
            _timerTMdefault.Value, _timerSTONEHEADSdefault.Value
            };

            // Initialize all timers as not started
            for (int i = 0; i < TimerRowNum; i++)
            {
                _timerDurationDefaults[i] = TimeSpan.FromMinutes(timerDefaultValues[i]);
                _timerStartTimes[i] = null; // Not started
                _timerRunning[i] = false;
            }
            #endregion

            try
            {
                #region Timer Window Window

                //// Assign all textures and parameters for timer window
                //_asyncTimertexture = AsyncTexture2D.FromAssetId(155985); //GameService.Content.DatAssetCache.GetTextureFromAssetId(155985)
                AsyncTexture2D NoTexture = new AsyncTexture2D();
                _TimerWindow = new StandardWindow(
                    NoTexture,
                    new Rectangle(0, 0, 390, 470), // The windowRegion
                    new Rectangle(0, -10, 390, 470)) // The contentRegion
                {
                    Parent = GameService.Graphics.SpriteScreen,
                    Title = "", //Timers
                    SavesPosition = true,
                    SavesSize = true,
                    CanResize = true,
                    Id = $"{nameof(BaubleFarmModule)}_BaubleFarmTimerWindow_38d37290-b5f9-447d-97ea-45b0b50e5f56",
                };
                /// Create texture panel for timer window
                _timerPanel = new Panel
                {
                    Parent = _TimerWindow, // Set the panel's parent to the StandardWindow
                    Size = new Point(_TimerWindow.ContentRegion.Size.X + 500, _TimerWindow.ContentRegion.Size.X + 500), // Match the panel to the content region
                    Location = _TimerWindow.ContentRegion.Location, // Align with content region
                    BackgroundColor = Color.Black,
                    Opacity = _OpacityDefault.Value
                };

                #endregion

                #region Bauble Information Window
                //// Display information about next Bauble run here
                _InfoWindow = new StandardWindow(
                    NoTexture,
                    new Rectangle(0, 0, 320, 130), // The windowRegion
                    new Rectangle(0, -10, 320, 130)) // The contentRegion
                {
                    Parent = GameService.Graphics.SpriteScreen,
                    Title = "Information",
                    SavesPosition = true,
                    Id = $"{nameof(BaubleFarmModule)}_BaubleFarmInfoWindow_38d37290-b5f9-447d-97ea-45b0b50e5f56",
                };

                _infoPanel = new Panel
                {
                    Parent = _InfoWindow, // Set the panel's parent to the StandardWindow
                    Size = new Point(_InfoWindow.ContentRegion.Size.X + 500, _InfoWindow.ContentRegion.Size.X + 500), // Match the panel to the content region
                    Location = _InfoWindow.ContentRegion.Location, // Align with content region
                    BackgroundColor = Color.Black,
                    Opacity = _OpacityDefault.Value
                };

                #endregion

                #region Corner Icon

                // Update the corner icon
                AsyncTexture2D cornertexture = AsyncTexture2D.FromAssetId(1010539); //156022
                _cornerIcon = new CornerIcon
                {
                    Icon = cornertexture, // Use a game-sourced texture
                    Size = new Point(32, 32),
                    //Location = new Point(0, 0), // Adjust to position as corner icon
                    BasicTooltipText = "Bauble Farm",
                    Parent = GameService.Graphics.SpriteScreen
                };

                // Handle click event to toggle window visibility
                _cornerIcon.Click += CornerIcon_Click;

                #endregion

                #region Bauble Information Timestamps

                var BaubleInformation = GetBaubleInformation();
                DateTime NextBaubleStartDate = BaubleInformation.NextBaubleStartDate;
                DateTime EndofBaubleWeek = BaubleInformation.EndofBaubleWeek;
                string FarmStatus = BaubleInformation.FarmStatus;
                Color Statuscolor = BaubleInformation.Statuscolor;
                initialDateTime = DateTime.Now;

                #endregion

                #region Bauble Information Labels
                Label statusLabel = new Label
                {
                    Text = "Bauble Farm Status :",
                    Size = new Point(180, 30),
                    Location = new Point(30, 30),
                    Font = GameService.Content.DefaultFont16,
                    Parent = _InfoWindow
                };
                _statusValue = new Label
                {
                    Text = FarmStatus,
                    Size = new Point(230, 30),
                    Location = new Point(190, 30),
                    Font = GameService.Content.DefaultFont16,
                    TextColor = Statuscolor,
                    Parent = _InfoWindow
                };
                Label startTimeLabel = new Label
                {
                    Text = "Start ->",
                    Size = new Point(100, 30),
                    Location = new Point(30, 60),
                    Font = GameService.Content.DefaultFont16,
                    Parent = _InfoWindow
                };
                _startTimeValue = new Label
                {
                    Text = NextBaubleStartDate.ToString("hh:mm tt (MMMM dd, yyyy)"),
                    Size = new Point(230, 30),
                    Location = new Point(90, 60),
                    Font = GameService.Content.DefaultFont16,
                    StrokeText = true,
                    TextColor = Color.DodgerBlue,
                    Parent = _InfoWindow
                };
                Label endTimeLabel = new Label
                {
                    Text = "End ->",
                    Size = new Point(100, 30),
                    Location = new Point(30, 90),
                    Font = GameService.Content.DefaultFont16,
                    Parent = _InfoWindow
                };
                _endTimeValue = new Label
                {
                    Text = EndofBaubleWeek.ToString("hh:mm tt (MMMM dd, yyyy)"),
                    Size = new Point(230, 30),
                    Location = new Point(80, 90),
                    Font = GameService.Content.DefaultFont16,
                    StrokeText = true,
                    TextColor = Color.DodgerBlue,
                    Parent = _InfoWindow
                };
                #endregion

                #region Timer Controls

                _stopButton = new StandardButton
                {
                    Text = "Stop All Timers",
                    Size = new Point(120, 30),
                    Location = new Point(20, 30),
                    Parent = _TimerWindow
                };
                _stopButton.Click += (s, e) => StopButton_Click();

                _InOrdercheckbox = new Checkbox
                {
                    Text = "Order by Timer",
                    Size = new Point(120, 30),
                    Location = new Point(150, 30),
                    Parent = _TimerWindow
                };
                _InOrdercheckbox.Checked = _InOrdercheckboxDefault.Value;
                _InOrdercheckbox.Click += (s, e) => InOrdercheckbox_Click();

                Label eventsLabel = new Label
                {
                    Text = "Events",
                    Size = new Point(120, 30),
                    Location = new Point(40, 65),
                    Font = GameService.Content.DefaultFont16,
                    StrokeText = true,
                    TextColor = Color.DodgerBlue,
                    Parent = _TimerWindow
                };
                Label timerLabel = new Label
                {
                    Text = "Timer",
                    Size = new Point(120, 30),
                    Location = new Point(140, 65),
                    Font = GameService.Content.DefaultFont16,
                    StrokeText = true,
                    TextColor = Color.DodgerBlue,
                    Parent = _TimerWindow
                };
                Label overridesLabel = new Label
                {
                    Text = "Override (min)",
                    Size = new Point(120, 30),
                    Location = new Point(280, 65),
                    Font = GameService.Content.DefaultFont16,
                    StrokeText = true,
                    TextColor = Color.DodgerBlue,
                    Parent = _TimerWindow
                };

                // Create UI elements for each timer
                string[] Descriptions = { "SVET", "EVET", "NVET", "WVET", "SAP", "BALTH", "WYVERN", "BRAMBLE", "OOZE", "GUZZLER", "TM", "STONEHEADS" };

                for (int i = 0; i < TimerRowNum; i++)
                {
                    int index = i; // Capture index for event handlers

                    // Timer Panels
                    _TimerWindowsOrdered[i] = new Panel
                    {
                        Parent = _TimerWindow,
                        Size = new Point(390, 30),
                        Location = new Point(0, 95 + (i * 30)),
                    };

                    _timerLabelDescriptions[i] = new Label
                    {
                        Text = Descriptions[i],
                        Size = new Point(100, 30),
                        Location = new Point(40, 0),
                        Parent = _TimerWindowsOrdered[i]
                    };

                    // Timer label
                    _timerLabels[i] = new Label
                    {
                        Text = _timerDurationDefaults[i].ToString(@"mm\:ss"),
                        Size = new Point(100, 30),
                        Location = new Point(110, 0),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Font = GameService.Content.DefaultFont16,
                        TextColor = Color.GreenYellow,
                        Parent = _TimerWindowsOrdered[i]
                    };

                    // Reset button
                    _resetButtons[i] = new StandardButton
                    {
                        Text = "Start",
                        Size = new Point(50, 30),
                        Location = new Point(190, 0),
                        Parent = _TimerWindowsOrdered[i]
                    };
                    _resetButtons[i].Click += (s, e) => ResetButton_Click(index);

                    // Reset button
                    _stopButtons[i] = new StandardButton
                    {
                        Text = "Stop",
                        Size = new Point(50, 30),
                        Location = new Point(240, 0),
                        Parent = _TimerWindowsOrdered[i]
                    };
                    _stopButtons[i].Click += (s, e) => stopButtons_Click(index);

                    // Override Timer dropdown
                    _customDropdownTimers[i] = new Dropdown
                    {
                        Items = { "Default", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" },
                        Size = new Point(80, 30),
                        Location = new Point(290, 0),
                        Parent = _TimerWindowsOrdered[i]
                    };
                    _customDropdownTimers[i].ValueChanged += (s, e) => dropdownChanged_Click(index);

                    #endregion
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in LoadAsync: {ex.Message}");
            }
        }

        private void CornerIcon_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            // Toggle window visibility
            if (_TimerWindow.Visible)
            {
                _TimerWindow.Hide();
                _InfoWindow.Hide();
            }
            else
            {
                _TimerWindow.Show();
                _InfoWindow.Show();
            }
        }
        private (DateTime NextBaubleStartDate, DateTime EndofBaubleWeek, string FarmStatus, Color Statuscolor) GetBaubleInformation()
        {
            /// Shiny Bauble Time Rotation
            TimeZoneInfo localTimeZone = TimeZoneInfo.Local; // Get the user's local time zone information.
            DateTime currentTime = DateTime.Now;
            DateTime rawBaubleStartTime = DateTime.Parse("2025-08-28 20:00:00"); // UTC time zone reset
            DateTime originBaubleStartTime = TimeZoneInfo.ConvertTimeFromUtc(rawBaubleStartTime, localTimeZone);
            int weekInterval = 3; // Number of weeks between bauble starts
            TimeSpan differenceOriginCurrent = currentTime - originBaubleStartTime;
            int weeksElapsed = (int)Math.Floor(differenceOriginCurrent.TotalDays / 7);
            int currentIntervalNumber = (int)Math.Floor((double)weeksElapsed / weekInterval);
            DateTime currentIntervalStartDate = originBaubleStartTime.AddDays(currentIntervalNumber * weekInterval * 7);
            DateTime nextThirdWeekIntervalStartDate = currentIntervalStartDate.AddDays(weekInterval * 7);
            DateTime NextBaubleStartDate = new DateTime();
            DateTime EndofBaubleWeek = new DateTime();
            DateTime oneWeekAheadcurrent = currentIntervalStartDate.AddDays(7);
            DateTime oneWeekAheadnext = nextThirdWeekIntervalStartDate.AddDays(7);
            string FarmStatus = "";
            Color Statuscolor = Color.Red;
            if (currentIntervalStartDate >= currentTime && currentTime <= oneWeekAheadcurrent)
            {
                NextBaubleStartDate = currentIntervalStartDate;
                EndofBaubleWeek = oneWeekAheadcurrent;
                FarmStatus = "ON";
                Statuscolor = Color.Green;
            }
            else
            {
                NextBaubleStartDate = nextThirdWeekIntervalStartDate;
                EndofBaubleWeek = oneWeekAheadnext;
                FarmStatus = "OFF";
                Statuscolor = Color.Red;
            }

            return (NextBaubleStartDate, EndofBaubleWeek, FarmStatus, Statuscolor);
        }
        private void ResetButton_Click(int timerIndex)
        {
            string DropdownValue = _customDropdownTimers[timerIndex].SelectedItem;
            _timerStartTimes[timerIndex] = DateTime.Now;
            _timerRunning[timerIndex] = true;
            _resetButtons[timerIndex].Enabled = false;
            _customDropdownTimers[timerIndex].Enabled = false;
            if (DropdownValue != "Default")
            {
                if (int.TryParse(DropdownValue, out int totalMinutes))
                {
                    _timerDurationOverride[timerIndex] = TimeSpan.FromMinutes(totalMinutes);
                }
            }

            UpdateJsonEvents();
        }
        private void stopButtons_Click(int timerIndex)
        {
            string DropdownValue = _customDropdownTimers[timerIndex].SelectedItem;
            if (_timerStartTimes[timerIndex].HasValue)
            {
                if (DropdownValue == "Default")
                {
                    _timerLabels[timerIndex].Text = $"{_timerDurationDefaults[timerIndex]:mm\\:ss}";
                }
                else
                {
                    _timerLabels[timerIndex].Text = $"{_timerDurationOverride[timerIndex]:mm\\:ss}";
                }
                _timerRunning[timerIndex] = false;
                _timerLabels[timerIndex].TextColor = Color.GreenYellow;
                _resetButtons[timerIndex].Enabled = true;
                _customDropdownTimers[timerIndex].Enabled = true;
            }

            UpdateJsonEvents();
        }
        private void StopButton_Click()
        {
            for (int timerIndex = 0; timerIndex < TimerRowNum; timerIndex++)
            {
                string DropdownValue = _customDropdownTimers[timerIndex].SelectedItem;
                if (_timerStartTimes[timerIndex].HasValue)
                {
                    if (DropdownValue == "Default")
                    {
                        _timerLabels[timerIndex].Text = $"{_timerDurationDefaults[timerIndex]:mm\\:ss}";
                    }
                    else
                    {
                        _timerLabels[timerIndex].Text = $"{_timerDurationOverride[timerIndex]:mm\\:ss}";
                    }
                    _timerRunning[timerIndex] = false;
                    _timerLabels[timerIndex].TextColor = Color.GreenYellow;
                    _resetButtons[timerIndex].Enabled = true;
                    _customDropdownTimers[timerIndex].Enabled = true;
                }
            }

            UpdateJsonEvents();
        }
        private void dropdownChanged_Click(int timerIndex)
        {
            string DropdownValue = _customDropdownTimers[timerIndex].SelectedItem;
            if (DropdownValue == "Default")
            {
                _timerLabels[timerIndex].Text = $"{_timerDurationDefaults[timerIndex]:mm\\:ss}";
            }
            else
            {
                if (int.TryParse(DropdownValue, out int totalMinutes))
                {
                    _timerDurationOverride[timerIndex] = TimeSpan.FromMinutes(totalMinutes);
                }
                _timerLabels[timerIndex].Text = $"{_timerDurationOverride[timerIndex]:mm\\:ss}";
            }
        }
        private void UpdateJsonEvents()
        {
            /// Backup timers in case of DC, disconnect, or crash
            List<EventData> eventDataList = new List<EventData>();
            for (int i = 0; i < TimerRowNum; i++)
            {
                DateTime? startTime = null;
                if (_timerRunning[i] == true)
                {
                    startTime = _timerStartTimes[i];
                }
                eventDataList.Add(new EventData
                {
                    ID = i,
                    Description = $"{_timerLabelDescriptions[i].Text}",
                    StartTime = startTime,
                    IsActive = _timerRunning[i]
                });
            }
            try
            {
                string jsonContent = JsonSerializer.Serialize(eventDataList, _jsonOptions);
                File.WriteAllText(_jsonFilePath, jsonContent);
                //Logger.Info($"Saved {_eventDataList.Count} events to {_jsonFilePath}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to save JSON file: {ex.Message}");
            }
            //eventDataList = new List<EventData>();
        }
        protected override async Task LoadAsync()
        {
            // Load backup JSON file for timers in case of DC, crashes, or disconnects
            //_eventDataList = new List<EventData>();
            List<EventData> eventDataList = new List<EventData>();

            // Get module-specific directory
            string moduleDir = DirectoriesManager.GetFullDirectoryPath("Shiny_Baubles");
            _jsonFilePath = Path.Combine(moduleDir, "Event_Timers.json");

            // Load JSON file if it exists
            if (File.Exists(_jsonFilePath))
            {
                try
                {
                    // Use StreamReader for async file reading in .NET Framework 4.8
                    using (StreamReader reader = new StreamReader(_jsonFilePath))
                    {
                        string jsonContent = await reader.ReadToEndAsync();
                        eventDataList = JsonSerializer.Deserialize<List<EventData>>(jsonContent, _jsonOptions);
                        //Logger.Info($"Loaded {_eventDataList.Count} events from {_jsonFilePath}");
                    }

                    var eventData = eventDataList;
                    for (int i = 0; i < TimerRowNum; i++)
                    {
                        DateTime? startTime = eventData[i].StartTime;
                        if (eventData[i].IsActive = true && startTime != null && eventData[i].Description == _timerLabelDescriptions[i].Text)
                        {
                            DateTime now = DateTime.Now;
                            TimeSpan difference = now - startTime.Value;

                            if (difference.TotalSeconds < 3600)
                            {
                                _timerStartTimes[i] = eventData[i].StartTime;
                                _timerRunning[i] = eventData[i].IsActive;
                                _resetButtons[i].Enabled = false;
                            }
                            else
                            {
                                _timerStartTimes[i] = null;
                                _timerRunning[i] = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Logger.Warn($"Failed to load JSON file: {ex.Message}");
                    //_eventDataList = new List<EventData>(); // Fallback to empty list
                }
            }
            else
            {
                Logger.Info("No JSON file found. Starting with an empty event list.");
            }
        }
        protected override void Update(GameTime gameTime)
        {
            #region Bauble Information Updates

            // Update Bauble Information Labels
            elapsedDateTime = DateTime.Now;
            TimeSpan difference = elapsedDateTime - initialDateTime;

            if (difference >= TimeSpan.FromMinutes(1))
            {
                var BaubleInformation = GetBaubleInformation();
                DateTime NextBaubleStartDate = BaubleInformation.NextBaubleStartDate;
                DateTime EndofBaubleWeek = BaubleInformation.EndofBaubleWeek;
                string FarmStatus = BaubleInformation.FarmStatus;
                Color Statuscolor = BaubleInformation.Statuscolor;
                _statusValue.Text = FarmStatus;
                _statusValue.TextColor = Statuscolor;
                _startTimeValue.Text = NextBaubleStartDate.ToString("hh:mm tt (MMMM dd, yyyy)");
                _endTimeValue.Text = EndofBaubleWeek.ToString("hh:mm tt (MMMM dd, yyyy)");
                initialDateTime = DateTime.Now;
            }

            #endregion

            #region Timer Information Updates

            // Update Timer Information
            TimeSpan[] CurrentElapsedTime = new TimeSpan[TimerRowNum];
            for (int i = 0; i < TimerRowNum; i++)
            {
                string DropdownValue = _customDropdownTimers[i].SelectedItem;
                TimeSpan remaining = TimeSpan.FromMinutes(0);
                if (_timerRunning[i] && _timerStartTimes[i].HasValue)
                {
                    var elapsed = DateTime.Now - _timerStartTimes[i].Value;
                    if (DropdownValue == "Default")
                    {
                        remaining = _timerDurationDefaults[i] - elapsed;
                    }
                    else
                    {
                        remaining = _timerDurationOverride[i] - elapsed;
                    }

                    _timerLabels[i].Text = $"{remaining:mm\\:ss}";
                    if (remaining.TotalSeconds <= -3600)
                    {
                        if (DropdownValue == "Default")
                        {
                            _timerLabels[i].Text = $"{_timerDurationDefaults[i]:mm\\:ss}";
                        }
                        else
                        {
                            _timerLabels[i].Text = $"{_timerDurationOverride[i]:mm\\:ss}";
                        }
                        _timerRunning[i] = false;
                        _timerLabels[i].TextColor = Color.GreenYellow;
                        _resetButtons[i].Enabled = true;
                    }
                    else if (remaining.TotalSeconds <= 0)
                    {
                        _timerLabels[i].Text = "-" + _timerLabels[i].Text;
                    }
                }
                if (_timerRunning[i] == false)
                {
                    if (DropdownValue == "Default")
                    {
                        CurrentElapsedTime[i] = _timerDurationDefaults[i];
                    }
                    else
                    {
                        CurrentElapsedTime[i] = _timerDurationOverride[i];
                    }
                }
                else
                {
                    CurrentElapsedTime[i] = remaining;
                    if (remaining.TotalSeconds < _timerLowDefault.Value)
                    {
                        _timerLabels[i].TextColor = Color.Red;
                    }
                    else
                    {
                        _timerLabels[i].TextColor = Color.GreenYellow;
                    }
                }
            }

            if (_InOrdercheckbox.Checked == true)
            {
                OrderPanelsByTime(CurrentElapsedTime);
            }

            #endregion
        }
        private void OrderPanelsByTime(TimeSpan[] CurrentElapsedTime)
        {
            var sortedWithIndices = CurrentElapsedTime
                .Select((value, index) => (Value: value, OriginalIndex: index))
                .OrderBy(item => item.Value)
                .ToList();

            for (int i = 0; i < TimerRowNum; i++)
            {
                _TimerWindowsOrdered[sortedWithIndices[i].OriginalIndex].Location = new Point(0, 95 + (i * 30));
            }
        }
        private void InOrdercheckbox_Click()
        {
            if (_InOrdercheckbox.Checked == false)
            {
                for (int i = 0; i < TimerRowNum; i++)
                {
                    _TimerWindowsOrdered[i].Location = new Point(0, 95 + (i * 30));
                }
            }
        }

        protected override void Unload()
        {
            // Clean up
            ModuleInstance = null;
            for (int i = 0; i < TimerRowNum; i++)
            {
                _resetButtons[i]?.Dispose();
                _timerLabels[i]?.Dispose();
            }

            _cornerIcon.Click -= CornerIcon_Click;
            _cornerIcon?.Dispose();

            if (_toggleTimerWindowKeybind != null)
            {
                _toggleTimerWindowKeybind.Value.Activated -= ToggleTimerWindowKeybind_Activated;
            }
            _TimerWindow?.Dispose();
            _TimerWindow = null;
            if (_toggleInfoWindowKeybind != null)
            {
                _toggleInfoWindowKeybind.Value.Activated -= ToggleInfoWindowKeybind_Activated;
            }
            _InfoWindow?.Dispose();
            _InfoWindow = null;
        }
    }
}
