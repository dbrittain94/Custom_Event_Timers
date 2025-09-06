using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Overlay.UI.Views;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.DirectWrite;
using System;
using System.ComponentModel.Composition;
using System.Runtime;

namespace roguishpanda.AB_Bauble_Farm
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class BaubleFarmModule : Blish_HUD.Modules.Module
    {
        private static readonly Logger Logger = Logger.GetLogger<BaubleFarmModule>();

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;

        [ImportingConstructor]
        public BaubleFarmModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
        }
        #endregion

        private Label[] _timerLabelDescriptions;
        private Label[] _timerLabels;
        private StandardButton _stopButton;
        private StandardButton[] _resetButtons;
        private Dropdown[] _customDropdownTimers;
        private DateTime?[] _timerStartTimes; // Nullable to track if timer is started
        private bool[] _timerRunning; // Track running state
        private readonly TimeSpan _timerDurationDefault = TimeSpan.FromMinutes(10);
        private TimeSpan[] _timerDurationDefaults;
        private TimeSpan[] _timerDurationOverride;
        private StandardWindow _TimerWindow;
        private StandardWindow _InfoWindow;
        private CornerIcon _cornerIcon;
        private SettingEntry<KeyBinding> _toggleTimerWindowKeybind;
        private SettingEntry<KeyBinding> _toggleInfoWindowKeybind;
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
        protected override void DefineSettings(SettingCollection settings)
        {
            _toggleTimerWindowKeybind = settings.DefineSetting("Timer Keybinding",new KeyBinding(ModifierKeys.Shift, Keys.L),() => "Timer Keybinding",() => "Keybind to show or hide the Timer window.");
            _toggleTimerWindowKeybind.Value.Enabled = true;
            _toggleTimerWindowKeybind.Value.Activated += ToggleTimerWindowKeybind_Activated;

            _toggleInfoWindowKeybind = settings.DefineSetting("Info Keybinding",new KeyBinding(ModifierKeys.Shift, Keys.OemSemicolon),() => "Info Keybinding",() => "Keybind to show or hide the Information window.");
            _toggleInfoWindowKeybind.Value.Enabled = true;
            _toggleInfoWindowKeybind.Value.Activated += ToggleInfoWindowKeybind_Activated;

            _timerLowDefault = settings.DefineSetting("Low Timer Default Timer",30,() => "Low Timer (seconds)",() => "Set timer for when timer gets below certain threshold in seconds.");
            _timerLowDefault.SetRange(1, 120);

            _timerSVETdefault = settings.DefineSetting("SVET Default Timer",10,() => "SVET (minutes)",() => "Set timer for SVET in minutes.");
            _timerSVETdefault.SetRange(1, 10);

            _timerEVETdefault = settings.DefineSetting("EVET Default Timer",10,() => "EVET (minutes)",() => "Set timer for EVET in minutes.");
            _timerEVETdefault.SetRange(1, 10);

            _timerNVETdefault = settings.DefineSetting("NVET Default Timer",10,() => "NVET (minutes)",() => "Set timer for NVET in minutes.");
            _timerNVETdefault.SetRange(1, 10);

            _timerWVETdefault = settings.DefineSetting("WVET Default Timer",10,() => "WVET (minutes)",() => "Set timer for WVET in minutes.");
            _timerWVETdefault.SetRange(1, 10);

            _timerSAPdefault = settings.DefineSetting("SAP Default Timer",8,() => "SAP (minutes)",() => "Set timer for SAP in minutes.");
            _timerSAPdefault.SetRange(1, 10);

            _timerBALTHdefault = settings.DefineSetting("BALTH Default Timer",8,() => "BALTH (minutes)",() => "Set timer for BALTH in minutes.");
            _timerBALTHdefault.SetRange(1, 10);

            _timerWYVERNdefault = settings.DefineSetting("WYVERN Default Timer",13,() => "WYVERN (minutes)",() => "Set timer for WYVERN in minutes.");
            _timerWYVERNdefault.SetRange(1, 15);

            _timerBRAMBLEdefault = settings.DefineSetting("BRAMBLE Default Timer",13,() => "BRAMBLE (minutes)",() => "Set timer for BRAMBLE in minutes.");
            _timerBRAMBLEdefault.SetRange(1, 15);

            _timerOOZEdefault = settings.DefineSetting("OOZE Default Timer",14,() => "OOZE (minutes)",() => "Set timer for OOZE in minutes.");
            _timerOOZEdefault.SetRange(1, 15);

            _timerGUZZLERdefault = settings.DefineSetting("GUZZLER Default Timer",13,() => "GUZZLER (minutes)",() => "Set timer for GUZZLER in minutes.");
            _timerGUZZLERdefault.SetRange(1, 15);

            _timerTMdefault = settings.DefineSetting("TM Default Timer",10,() => "TM (minutes)",() => "Set timer for TM in minutes.");
            _timerTMdefault.SetRange(1, 10);

            _timerSTONEHEADSdefault = settings.DefineSetting("STONEHEADS Default Timer",12,() => "STONEHEADS (minutes)",() => "Set timer for STONEHEADS in minutes.");
            _timerSTONEHEADSdefault.SetRange(1, 15);
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

        protected override void Initialize()
        {
            #region Initialize Defaults
            _timerStartTimes = new DateTime?[12];
            _timerRunning = new bool[12];
            _timerLabelDescriptions = new Label[12];
            _timerLabels = new Label[12];
            _resetButtons = new StandardButton[12];
            _customDropdownTimers = new Dropdown[12];
            _timerDurationOverride = new TimeSpan[12];
            _timerDurationDefaults = new TimeSpan[12];
            
            // Initialize Timer Defaults
            _timerDurationDefaults[0] = TimeSpan.FromMinutes(_timerSVETdefault.Value);
            _timerDurationDefaults[1] = TimeSpan.FromMinutes(_timerEVETdefault.Value);
            _timerDurationDefaults[2] = TimeSpan.FromMinutes(_timerNVETdefault.Value);
            _timerDurationDefaults[3] = TimeSpan.FromMinutes(_timerWVETdefault.Value);
            _timerDurationDefaults[4] = TimeSpan.FromMinutes(_timerSAPdefault.Value);
            _timerDurationDefaults[5] = TimeSpan.FromMinutes(_timerBALTHdefault.Value);
            _timerDurationDefaults[6] = TimeSpan.FromMinutes(_timerWYVERNdefault.Value);
            _timerDurationDefaults[7] = TimeSpan.FromMinutes(_timerBRAMBLEdefault.Value);
            _timerDurationDefaults[8] = TimeSpan.FromMinutes(_timerOOZEdefault.Value);
            _timerDurationDefaults[9] = TimeSpan.FromMinutes(_timerGUZZLERdefault.Value);
            _timerDurationDefaults[10] = TimeSpan.FromMinutes(_timerTMdefault.Value);
            _timerDurationDefaults[11] = TimeSpan.FromMinutes(_timerSTONEHEADSdefault.Value);

            // Initialize all timers as not started
            for (int i = 0; i < 10; i++)
            {
                _timerStartTimes[i] = null; // Not started
                _timerRunning[i] = false;
            }
            #endregion

            try
            {
                #region Timer Window Window
                //// Assign all textures and parameters for timer window
                AsyncTexture2D _Timertexture = AsyncTexture2D.FromAssetId(155985); //GameService.Content.DatAssetCache.GetTextureFromAssetId(155985)
                AsyncTexture2D _asyncTimerTexture = new AsyncTexture2D();
                _TimerWindow = new StandardWindow(
                    _asyncTimerTexture,
                    new Rectangle(0, 0, 420, 440), // The windowRegion
                    new Rectangle(0, -20, 440, 590)) // The contentRegion
                {
                    Parent = GameService.Graphics.SpriteScreen,
                    Title = "Timers",
                    SavesPosition = true,
                    Id = $"{nameof(BaubleFarmModule)}_BaubleFarmTimerWindow_38d37290-b5f9-447d-97ea-45b0b50e5f56",
                };

                int originalPointX = _TimerWindow.ContentRegion.Location.X;
                int originalPointY = _TimerWindow.ContentRegion.Location.Y;
                Point newLocation = new Point();
                newLocation.X = originalPointX;
                newLocation.Y = originalPointY + 20;
                var timerPanel = new Panel
                {
                    Parent = _TimerWindow, // Set the panel's parent to the StandardWindow
                    BackgroundTexture = _Timertexture,
                    Size = _TimerWindow.ContentRegion.Size, // Match the panel to the content region
                    Location = newLocation // Align with content region
                };

                #endregion

                #region Bauble Information Window
                //// Display information about next Bauble run here
                AsyncTexture2D _Infotexture = AsyncTexture2D.FromAssetId(155985); //GameService.Content.DatAssetCache.GetTextureFromAssetId(155985)
                AsyncTexture2D _asyncInfoTexture = new AsyncTexture2D();
                _InfoWindow = new StandardWindow(
                    _asyncInfoTexture,
                    new Rectangle(0, 0, 320, 130), // The windowRegion
                    new Rectangle(0, -20, 340, 180)) // The contentRegion
                {
                    Parent = GameService.Graphics.SpriteScreen,
                    Title = "Information",
                    SavesPosition = true,
                    Id = $"{nameof(BaubleFarmModule)}_BaubleFarmInfoWindow_38d37290-b5f9-447d-97ea-45b0b50e5f56",
                };

                originalPointX = _InfoWindow.ContentRegion.Location.X;
                originalPointY = _InfoWindow.ContentRegion.Location.Y;
                newLocation = new Point();
                newLocation.X = originalPointX;
                newLocation.Y = originalPointY + 20;
                var infoPanel = new Panel
                {
                    Parent = _InfoWindow, // Set the panel's parent to the StandardWindow
                    BackgroundTexture = _Infotexture,
                    Size = _InfoWindow.ContentRegion.Size, // Match the panel to the content region
                    Location = newLocation // Align with content region
                };
                #endregion

                #region Corner Icon
                // Update the corner icon
                AsyncTexture2D cornertexture = AsyncTexture2D.FromAssetId(1010539); //156022
                _cornerIcon = new CornerIcon
                {
                    Icon = cornertexture, // Use a game-sourced texture
                    Size = new Point(32, 32),
                    Location = new Point(0, 0), // Adjust to position as corner icon
                    BasicTooltipText = "Toggle Bauble Farm",
                    Parent = GameService.Graphics.SpriteScreen
                };

                // Handle click event to toggle window visibility
                _cornerIcon.Click += CornerIcon_Click;

                #endregion

                #region Bauble Information Timestamps
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
                #endregion

                #region Bauble Information Labels
                Label statusLabel = new Label
                {
                    Text = "Bauble Farm Status :",
                    Size = new Point(180, 30),
                    Location = new Point(30, 50),
                    Font = GameService.Content.DefaultFont16,
                    Parent = _InfoWindow
                };
                Label statusValue = new Label
                {
                    Text = FarmStatus,
                    Size = new Point(230, 30),
                    Location = new Point(190, 50),
                    Font = GameService.Content.DefaultFont16,
                    TextColor = Statuscolor,
                    Parent = _InfoWindow
                };
                Label startTimeLabel = new Label
                {
                    Text = "Start ->",
                    Size = new Point(100, 30),
                    Location = new Point(30, 80),
                    Font = GameService.Content.DefaultFont16,
                    Parent = _InfoWindow
                };
                Label startTimeValue = new Label
                {
                    Text = NextBaubleStartDate.ToString("hh:mm tt (MMMM dd, yyyy)"),
                    Size = new Point(230, 30),
                    Location = new Point(90, 80),
                    Font = GameService.Content.DefaultFont16,
                    StrokeText = true,
                    TextColor = Color.DodgerBlue,
                    Parent = _InfoWindow
                };
                Label endTimeLabel = new Label
                {
                    Text = "End ->",
                    Size = new Point(100, 30),
                    Location = new Point(30, 110),
                    Font = GameService.Content.DefaultFont16,
                    Parent = _InfoWindow
                };
                Label endTimeValue = new Label
                {
                    Text = EndofBaubleWeek.ToString("hh:mm tt (MMMM dd, yyyy)"),
                    Size = new Point(230, 30),
                    Location = new Point(80, 110),
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
                    Location = new Point(220, 0),
                    Parent = _TimerWindow
                };
                _stopButton.Click += (s, e) => StopButton_Click();

                Label eventsLabel = new Label
                {
                    Text = "Events",
                    Size = new Point(120, 30),
                    Location = new Point(30, 55),
                    Font = GameService.Content.DefaultFont16,
                    StrokeText = true,
                    TextColor = Color.DodgerBlue,
                    Parent = _TimerWindow
                };
                Label timerLabel = new Label
                {
                    Text = "Timer",
                    Size = new Point(120, 30),
                    Location = new Point(130, 55),
                    Font = GameService.Content.DefaultFont16,
                    StrokeText = true,
                    TextColor = Color.DodgerBlue,
                    Parent = _TimerWindow
                };
                Label overridesLabel = new Label
                {
                    Text = "Override (min)",
                    Size = new Point(120, 30),
                    Location = new Point(310, 55),
                    Font = GameService.Content.DefaultFont16,
                    StrokeText = true,
                    TextColor = Color.DodgerBlue,
                    Parent = _TimerWindow
                };

                // Create UI elements for each timer
                string[] Descriptions = { "SVET", "EVET", "NVET", "WVET", "SAP", "BALTH", "WYVERN", "BRAMBLE", "OOZE", "GUZZLER", "TM", "STONEHEADS" };

                for (int i = 0; i < 12; i++)
                {
                    int index = i; // Capture index for event handlers

                    // Timer label descriptions
                    _timerLabelDescriptions[0] = new Label
                    {
                        Text = Descriptions[i],
                        Size = new Point(100, 30),
                        Location = new Point(30, 80 + (i * 30)),
                        Parent = _TimerWindow
                    };

                    // Timer label
                    _timerLabels[i] = new Label
                    {
                        Text = _timerDurationDefaults[i].ToString(@"mm\:ss"),
                        Size = new Point(100, 30),
                        Location = new Point(130, 80 + (i * 30)),
                        Font = GameService.Content.DefaultFont16,
                        TextColor = Color.GreenYellow,
                        Parent = _TimerWindow
                    };

                    // Reset button
                    _resetButtons[i] = new StandardButton
                    {
                        Text = "Start/Reset",
                        Size = new Point(120, 30),
                        Location = new Point(180, 80 + (i * 30)),
                        Parent = _TimerWindow
                    };
                    _resetButtons[i].Click += (s, e) => ResetButton_Click(index);

                    // Override Timer dropdown
                    _customDropdownTimers[i] = new Dropdown
                    {
                        Items = { "Default", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" },
                        Size = new Point(80, 30),
                        Location = new Point(320, 80 + (i * 30)),
                        Parent = _TimerWindow
                    };

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
            }
            else
            {
                _TimerWindow.Show();
            }

            if (_InfoWindow.Visible)
            {
                _InfoWindow.Hide();
            }
            else
            {
                _InfoWindow.Show();
            }
        }
        private void ResetButton_Click(int timerIndex)
        {
            string DropdownValue = _customDropdownTimers[timerIndex].SelectedItem;
            if (DropdownValue == "Default")
            {
                _timerStartTimes[timerIndex] = DateTime.Now;
                _timerRunning[timerIndex] = true;
            }
            else
            {
                _timerStartTimes[timerIndex] = DateTime.Now;
                _timerRunning[timerIndex] = true;
                if (int.TryParse(DropdownValue, out int totalMinutes))
                {
                    _timerDurationOverride[timerIndex] = TimeSpan.FromMinutes(totalMinutes);
                }
            }
        }
        private void StopButton_Click()
        {
            for (int i = 0; i < 12; i++)
            {
                _timerStartTimes[i] = DateTime.Now;
                _timerRunning[i] = false;
            }
        }
        protected override void Update(GameTime gameTime)
        {
            // Update Timer Defaults
            _timerDurationDefaults[0] = TimeSpan.FromMinutes(_timerSVETdefault.Value);
            _timerDurationDefaults[1] = TimeSpan.FromMinutes(_timerEVETdefault.Value);
            _timerDurationDefaults[2] = TimeSpan.FromMinutes(_timerNVETdefault.Value);
            _timerDurationDefaults[3] = TimeSpan.FromMinutes(_timerWVETdefault.Value);
            _timerDurationDefaults[4] = TimeSpan.FromMinutes(_timerSAPdefault.Value);
            _timerDurationDefaults[5] = TimeSpan.FromMinutes(_timerBALTHdefault.Value);
            _timerDurationDefaults[6] = TimeSpan.FromMinutes(_timerWYVERNdefault.Value);
            _timerDurationDefaults[7] = TimeSpan.FromMinutes(_timerBRAMBLEdefault.Value);
            _timerDurationDefaults[8] = TimeSpan.FromMinutes(_timerOOZEdefault.Value);
            _timerDurationDefaults[9] = TimeSpan.FromMinutes(_timerGUZZLERdefault.Value);
            _timerDurationDefaults[10] = TimeSpan.FromMinutes(_timerTMdefault.Value);
            _timerDurationDefaults[11] = TimeSpan.FromMinutes(_timerSTONEHEADSdefault.Value);

            for (int i = 0; i < 12; i++)
            {
                string DropdownValue = _customDropdownTimers[i].SelectedItem;
                if (_timerRunning[i] && _timerStartTimes[i].HasValue)
                {
                    TimeSpan remaining = TimeSpan.FromMinutes(0);
                    if (DropdownValue == "Default")
                    {
                        var elapsed = DateTime.Now - _timerStartTimes[i].Value;
                        remaining = _timerDurationDefaults[i] - elapsed;
                        if (remaining.TotalSeconds <= 0)
                        {
                            remaining = TimeSpan.Zero;
                            _timerRunning[i] = false;
                        }
                        _timerLabels[i].Text = $"{remaining:mm\\:ss}";
                    }
                    else
                    {
                        var elapsed = DateTime.Now - _timerStartTimes[i].Value;
                        remaining = _timerDurationOverride[i] - elapsed;
                        if (remaining.TotalSeconds <= 0)
                        {
                            remaining = TimeSpan.Zero;
                            _timerRunning[i] = false;
                        }
                        _timerLabels[i].Text = $"{remaining:mm\\:ss}";
                    }

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
        }

        protected override void Unload()
        {
            // Clean up
            for (int i = 0; i < 12; i++)
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
