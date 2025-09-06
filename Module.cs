using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Overlay.UI.Views;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.Direct2D1;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AB_Bauble_Farm
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
        private CornerIcon _cornerIcon;

        [ImportingConstructor]
        public BaubleFarmModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
        }
        private SettingEntry<KeyBinding> _toggleWindowKeybind;
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
        protected override void DefineSettings(SettingCollection settings)
        {
            _toggleWindowKeybind = settings.DefineSetting(
                "Timer Window",
                new KeyBinding(Keys.None),
                () => "Toggle Window Keybind",
                () => "Keybind to show or hide the custom window."
            );
            _toggleWindowKeybind.Value.Enabled = true;
            _toggleWindowKeybind.Value.Activated += ToggleWindowKeybind_Activated;

            _timerSVETdefault = settings.DefineSetting(
                "SVET Default Timer",
                10,
                () => "SVET (minutes)",
                () => "Set timer for SVET in minutes."
            );
            _timerSVETdefault.SetRange(1, 10);

            _timerEVETdefault = settings.DefineSetting(
                "EVET Default Timer",
                10,
                () => "EVET (minutes)",
                () => "Set timer for EVET in minutes."
            );
            _timerEVETdefault.SetRange(1, 10);

            _timerNVETdefault = settings.DefineSetting(
                "NVET Default Timer",
                10,
                () => "NVET (minutes)",
                () => "Set timer for NVET in minutes."
            );
            _timerNVETdefault.SetRange(1, 10);

            _timerWVETdefault = settings.DefineSetting(
                "WVET Default Timer",
                10,
                () => "WVET (minutes)",
                () => "Set timer for WVET in minutes."
            );
            _timerWVETdefault.SetRange(1, 10);

            _timerSAPdefault = settings.DefineSetting(
                "SAP Default Timer",
                8,
                () => "SAP (minutes)",
                () => "Set timer for SAP in minutes."
            );
            _timerSAPdefault.SetRange(1, 10);

            _timerBALTHdefault = settings.DefineSetting(
                "BALTH Default Timer",
                8,
                () => "BALTH (minutes)",
                () => "Set timer for BALTH in minutes."
            );
            _timerBALTHdefault.SetRange(1, 10);

            _timerWYVERNdefault = settings.DefineSetting(
                "WYVERN Default Timer",
                13,
                () => "WYVERN (minutes)",
                () => "Set timer for WYVERN in minutes."
            );
            _timerWYVERNdefault.SetRange(1, 15);

            _timerBRAMBLEdefault = settings.DefineSetting(
                "BRAMBLE Default Timer",
                13,
                () => "BRAMBLE (minutes)",
                () => "Set timer for BRAMBLE in minutes."
            );
            _timerBRAMBLEdefault.SetRange(1, 15);

            _timerOOZEdefault = settings.DefineSetting(
                "OOZE Default Timer",
                14,
                () => "OOZE (minutes)",
                () => "Set timer for OOZE in minutes."
            );
            _timerOOZEdefault.SetRange(1, 15);

            _timerGUZZLERdefault = settings.DefineSetting(
                "GUZZLER Default Timer",
                13,
                () => "GUZZLER (minutes)",
                () => "Set timer for GUZZLER in minutes."
            );
            _timerGUZZLERdefault.SetRange(1, 15);
        }
        private void ToggleWindowKeybind_Activated(object sender, EventArgs e)
        {
            //if (_TimerWindow != null)
            //{
            //    _TimerWindow.Visible = !_TimerWindow.Visible; // Toggle visibility
            //}
            if (_TimerWindow.Visible)
            {
                _TimerWindow.Hide();
            }
            else
            {
                _TimerWindow.Show();
            }
        }

        protected override void Initialize()
        {
            _timerStartTimes = new DateTime?[10];
            _timerRunning = new bool[10];
            _timerLabelDescriptions = new Label[10];
            _timerLabels = new Label[10];
            _resetButtons = new StandardButton[10];
            _customDropdownTimers = new Dropdown[10];
            _timerDurationOverride = new TimeSpan[10];
            _timerDurationDefaults = new TimeSpan[10];

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

            // Initialize all timers as not started
            for (int i = 0; i < 10; i++)
            {
                _timerStartTimes[i] = null; // Not started
                _timerRunning[i] = false;
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
        }

        protected override async Task LoadAsync()
        {
            try
            {
                AsyncTexture2D texture = AsyncTexture2D.FromAssetId(155985); //GameService.Content.DatAssetCache.GetTextureFromAssetId(155985)
                AsyncTexture2D _asyncTexture = new AsyncTexture2D();
                _TimerWindow = new StandardWindow(
                    _asyncTexture,
                    new Rectangle(0, 0, 420, 400), // The windowRegion 300, 250
                    new Rectangle(0, -20, 440, 550)) // The contentRegion 250, 200
                {
                    Parent = GameService.Graphics.SpriteScreen,
                    Title = "Timers",
                    SavesPosition = true,
                    Id = $"{nameof(BaubleFarmModule)}_BaubleFarm_38d37290-b5f9-447d-97ea-45b0b50e5f56",
                };

                int originalPointX = _TimerWindow.ContentRegion.Location.X;
                int originalPointY = _TimerWindow.ContentRegion.Location.Y;
                Point newLocation = new Point();
                newLocation.X = originalPointX;
                newLocation.Y = originalPointY + 20;
                var contentPanel = new Panel
                {
                    Parent = _TimerWindow, // Set the panel's parent to the StandardWindow
                    BackgroundTexture = texture,
                    Size = _TimerWindow.ContentRegion.Size, // Match the panel to the content region
                    Location = newLocation // Align with content region
                };

                _TimerWindow.Show();

                if (_toggleWindowKeybind.Value.PrimaryKey == Keys.None)
                {
                    _toggleWindowKeybind.Value.PrimaryKey = Keys.LeftShift | Keys.L;
                }

                // Initialize the corner icon
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

                _stopButton = new StandardButton
                {
                    Text = "Stop All Timers",
                    Size = new Point(120, 30),
                    Location = new Point(220, 0),
                    Parent = _TimerWindow
                };
                _stopButton.Click += (s, e) => StopButton_Click();

                Label overridesLabel = new Label
                {
                    Text = "Override (min)",
                    Size = new Point(100, 30),
                    Location = new Point(330, 40),
                    Parent = _TimerWindow
                };

                // Create UI elements for each timer
                string[] Descriptions = { "SVET", "EVET", "NVET", "WVET", "SAP", "BALTH", "WYVERN", "BRAMBLE", "OOZE", "GUZZLER" };

                for (int i = 0; i < 10; i++)
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
                        Location = new Point(120, 80 + (i * 30)),
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

                    //_customDropdownTimers[i].Click += (s, e) => ResetButton_Click(index);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in LoadAsync: {ex.Message}");
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
            for (int i = 0; i < 10; i++)
            {
                _timerStartTimes[i] = DateTime.Now;
                _timerRunning[i] = false;
            }
        }
        protected override void Update(GameTime gameTime)
        {
            for (int i = 0; i < 10; i++)
            {
                string DropdownValue = _customDropdownTimers[i].SelectedItem;
                if (_timerRunning[i] && _timerStartTimes[i].HasValue)
                {
                    if (DropdownValue == "Default")
                    {
                        var elapsed = DateTime.Now - _timerStartTimes[i].Value;
                        var remaining = _timerDurationDefaults[i] - elapsed;
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
                        var remaining = _timerDurationOverride[i] - elapsed;
                        if (remaining.TotalSeconds <= 0)
                        {
                            remaining = TimeSpan.Zero;
                            _timerRunning[i] = false;
                        }
                        _timerLabels[i].Text = $"{remaining:mm\\:ss}";
                    }
                }
            }
        }

        protected override void Unload()
        {
            // Clean up
            for (int i = 0; i < 10; i++)
            {
                _resetButtons[i]?.Dispose();
                _timerLabels[i]?.Dispose();
            }

            _cornerIcon.Click -= CornerIcon_Click;
            _cornerIcon?.Dispose();

            if (_toggleWindowKeybind != null)
            {
                _toggleWindowKeybind.Value.Activated -= ToggleWindowKeybind_Activated;
            }
            _TimerWindow?.Dispose();
            _TimerWindow = null;
        }

    }
}
