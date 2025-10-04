using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.GameServices.ArcDps.V2.Models;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace roguishpanda.AB_Bauble_Farm
{
    public class PackageData
    {
        public string PackageName { get; set; }
        public List<StaticDetailData> StaticDetailData { get; set; }
        public List<TimerDetailData> TimerDetailData { get; set; }
    }
    public class TimerLogData
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public DateTime? StartTime { get; set; }
        public bool IsActive { get; set; }
    }
    public class StaticLogData
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
    public class TimerDetailData
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public double Minutes { get; set; }
        public double Seconds { get; set; }
        public List<string> Notes { get; set; }
        public List<bool> Broadcast { get; set; }
        public List<string> Waypoints { get; set; }
    }
    public class StaticDetailData
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public List<string> Notes { get; set; }
        public List<bool> Broadcast { get; set; }
        public List<string> Waypoints { get; set; }
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

        public Blish_HUD.Controls.Label[] _timerLabelDescriptions;
        public Blish_HUD.Controls.Label[] _timerLabels;
        public Blish_HUD.Controls.Label _statusValue;
        public Blish_HUD.Controls.Label _startTimeValue;
        public Blish_HUD.Controls.Label _endTimeValue;
        public List<List<string>> _Notes;
        public List<List<bool>> _Broadcast;
        public List<List<string>> _Waypoints;
        public Checkbox _InOrdercheckbox;
        public DateTime elapsedDateTime;
        public DateTime initialDateTime;
        public int TimerRowNum = 12;
        public StandardButton _stopButton;
        public StandardButton[] _stopButtons;
        public StandardButton[] _resetButtons;
        public Dropdown[] _customDropdownTimers;
        public DateTime?[] _timerStartTimes; // Nullable to track if timer is started
        public bool[] _timerRunning; // Track running state
        public TimeSpan[] _timerDurationDefaults;
        public TimeSpan[] _timerDurationOverride;
        public Blish_HUD.Controls.Panel[] _TimerWindowsOrdered;
        public Blish_HUD.Controls.Panel _infoPanel;
        public Blish_HUD.Controls.Panel _timerPanel;
        public Blish_HUD.Controls.Panel _SettingsPanel;
        public StandardWindow _TimerWindow;
        public StandardWindow _InfoWindow;
        public TabbedWindow2 _SettingsWindow;
        public Blish_HUD.Controls.Panel _timerSettingsPanel;
        public CornerIcon _cornerIcon;
        public SettingEntry<KeyBinding> _toggleTimerWindowKeybind;
        public SettingEntry<KeyBinding> _toggleInfoWindowKeybind;
        public SettingEntry<KeyBinding> _stoneheadKeybind;
        public SettingEntry<KeyBinding> _postNotesKeybind;
        public SettingEntry<KeyBinding> _cancelNotesKeybind;
        public SettingCollection _MainSettingsCollection;
        public SettingCollection _PackageSettingsCollection;
        public SettingEntry<bool> _InOrdercheckboxDefault;
        public SettingEntry<float> _OpacityDefault;
        public SettingEntry<int> _timerLowDefault;
        public AsyncTexture2D _asyncTimertexture;
        public AsyncTexture2D _asyncGeneralSettingstexture;
        public AsyncTexture2D _asyncNotesSettingstexture;
        public Blish_HUD.Controls.Panel _inputPanel;
        public Blish_HUD.Controls.Label _instructionLabel;
        public Image[] _notesIcon;
        public Image[] _waypointIcon;
        public int[] _ModifierKeys;
        public int[] _PrimaryKey;
        public double[] _TimerMinutes;
        public double[] _TimerSeconds;
        public int[] _TimerID;
        public SettingEntry<string> _CurrentPackageSelection;
        public SettingCollection _settings;
        public List<PackageData> _PackageData;
        public List<TimerDetailData> _eventNotes;
        public readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
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
            _MainSettingsCollection = settings.AddSubCollection("MainSettings");
            _PackageSettingsCollection = settings.AddSubCollection("PackageSettings");

            _InOrdercheckboxDefault = _MainSettingsCollection.DefineSetting("InOrdercheckboxDefault", false, () => "Order by Timer", () => "Check this box if you want to order your timers by time.");

            _timerLowDefault = _MainSettingsCollection.DefineSetting("LowTimerDefaultTimer", 30, () => "Low Timer", () => "Set timer for when timer gets below certain threshold in seconds.");
            _timerLowDefault.SetRange(1, 120);

            _OpacityDefault = _MainSettingsCollection.DefineSetting("OpacityDefault", 1.0f, () => "Window Opacity", () => "Changing the opacity will adjust how translucent the windows are.");
            _OpacityDefault.SetRange(0.1f, 1.0f);
            _OpacityDefault.SettingChanged += ChangeOpacity_Activated;

            _toggleTimerWindowKeybind = _MainSettingsCollection.DefineSetting("TimerKeybinding",new KeyBinding(ModifierKeys.Shift, Microsoft.Xna.Framework.Input.Keys.L),() => "Timer Window",() => "Keybind to show or hide the Timer window.");
            _toggleTimerWindowKeybind.Value.BlockSequenceFromGw2 = true;
            _toggleTimerWindowKeybind.Value.Enabled = true;
            _toggleTimerWindowKeybind.Value.Activated += ToggleTimerWindowKeybind_Activated;

            _toggleInfoWindowKeybind = _MainSettingsCollection.DefineSetting("InfoKeybinding",new KeyBinding(ModifierKeys.Shift, Microsoft.Xna.Framework.Input.Keys.OemSemicolon),() => "Info Window",() => "Keybind to show or hide the Information window.");
            _toggleInfoWindowKeybind.Value.BlockSequenceFromGw2 = true;
            _toggleInfoWindowKeybind.Value.Enabled = true;
            _toggleInfoWindowKeybind.Value.Activated += ToggleInfoWindowKeybind_Activated;

            _postNotesKeybind = _MainSettingsCollection.DefineSetting("PostNotesKeybinding", new KeyBinding(ModifierKeys.Shift, Microsoft.Xna.Framework.Input.Keys.B), () => "Post Notes", () => "Keybind to confirm posting notes in chat.");
            _postNotesKeybind.Value.BlockSequenceFromGw2 = true;
            _postNotesKeybind.Value.Enabled = true;
            _postNotesKeybind.Value.BindingChanged += PostNotes_BindingChanged;

            _cancelNotesKeybind = _MainSettingsCollection.DefineSetting("CancelNotesKeybinding", new KeyBinding(ModifierKeys.Shift, Microsoft.Xna.Framework.Input.Keys.N), () => "Cancel Notes", () => "Keybind to cancel posting notes in chat.");
            _cancelNotesKeybind.Value.BlockSequenceFromGw2 = true;
            _cancelNotesKeybind.Value.Enabled = true;
            _cancelNotesKeybind.Value.BindingChanged += CancelNotes_BindingChanged;

            _CurrentPackageSelection = _PackageSettingsCollection.DefineSetting("CurrentPackageSelection", "Default", () => "Current Package", () => "This is the current package selection.");

            _settings = settings;
        }
        public override IView GetSettingsView() => new ModuleSettingsView();

        private void CancelNotes_BindingChanged(object sender, EventArgs e)
        {
            if (_cancelNotesKeybind.Value.PrimaryKey == Microsoft.Xna.Framework.Input.Keys.None)
            {
                for (int i = 0; i < _notesIcon.Count(); i++)
                {
                    _notesIcon[i].Hide();
                    _waypointIcon[i].Hide();
                }
            }
            else
            {
                for (int i = 0; i < _notesIcon.Count(); i++)
                {
                    _notesIcon[i].Show();
                    _waypointIcon[i].Show();
                }
            }
        }

        private void PostNotes_BindingChanged(object sender, EventArgs e)
        {
            if ( _postNotesKeybind.Value.PrimaryKey == Microsoft.Xna.Framework.Input.Keys.None)
            {
                for (int i = 0; i < _notesIcon.Count(); i++)
                {
                    _notesIcon[i].Hide();
                    _waypointIcon[i].Hide();
                }
            }
            else
            {
                for (int i = 0; i < _notesIcon.Count(); i++)
                {
                    _notesIcon[i].Show();
                    _waypointIcon[i].Show();
                }
            }
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
        public void LoadTimerDefaults(int TotalEvents)
        {
            for (int i = 0; i < TotalEvents; i++)
            {
                int count = i;
                SettingCollection TimerCollector = _settings.AddSubCollection(_timerLabelDescriptions[i].Text + "TimerInfo");
                SettingEntry<KeyBinding> KeybindSettingEntry = null;
                TimerCollector.TryGetSetting(_timerLabelDescriptions[i].Text + "Keybind", out KeybindSettingEntry);
                SettingEntry<int> MintuesSettingEntry = null;
                TimerCollector.TryGetSetting(_timerLabelDescriptions[i].Text + "TimerMinutes", out MintuesSettingEntry);
                SettingEntry<int> SecondsSettingEntry = null;
                TimerCollector.TryGetSetting(_timerLabelDescriptions[i].Text + "TimerSeconds", out SecondsSettingEntry);
                SettingEntry<string> WaypointSettingEntry = null;
                TimerCollector.TryGetSetting(_timerLabelDescriptions[i].Text + "Waypoint", out WaypointSettingEntry);
                SettingEntry<string> NotesOneSettingEntry = null;
                TimerCollector.TryGetSetting(_timerLabelDescriptions[i].Text + "NoteOne", out NotesOneSettingEntry);
                SettingEntry<string> NotesTwoSettingEntry = null;
                TimerCollector.TryGetSetting(_timerLabelDescriptions[i].Text + "NoteTwo", out NotesTwoSettingEntry);
                SettingEntry<string> NotesThreeSettingEntry = null;
                TimerCollector.TryGetSetting(_timerLabelDescriptions[i].Text + "NoteThree", out NotesThreeSettingEntry);
                SettingEntry<string> NotesFourSettingEntry = null;
                TimerCollector.TryGetSetting(_timerLabelDescriptions[i].Text + "NoteFour", out NotesFourSettingEntry);
                SettingEntry<bool> BroadcastNotesOneSettingEntry = null;
                TimerCollector.TryGetSetting(_timerLabelDescriptions[i].Text + "BroadcastNoteOne", out BroadcastNotesOneSettingEntry);
                SettingEntry<bool> BroadcastNotesTwoSettingEntry = null;
                TimerCollector.TryGetSetting(_timerLabelDescriptions[i].Text + "BroadcastNoteTwo", out BroadcastNotesTwoSettingEntry);
                SettingEntry<bool> BroadcastNotesThreeSettingEntry = null;
                TimerCollector.TryGetSetting(_timerLabelDescriptions[i].Text + "BroadcastNoteThree", out BroadcastNotesThreeSettingEntry);
                SettingEntry<bool> BroadcastNotesFourSettingEntry = null;
                TimerCollector.TryGetSetting(_timerLabelDescriptions[i].Text + "BroadcastNoteFour", out BroadcastNotesFourSettingEntry);

                if (KeybindSettingEntry != null)
                {
                    KeybindSettingEntry.Value.BlockSequenceFromGw2 = true;
                    KeybindSettingEntry.Value.Enabled = true;
                    KeybindSettingEntry.Value.Activated += (s, e) => timerKeybinds(count);
                }

                TimeSpan Minutes = TimeSpan.FromMinutes(_TimerMinutes[i]);
                TimeSpan Seconds = TimeSpan.FromSeconds(_TimerSeconds[i]);
                if (MintuesSettingEntry != null)
                {
                    TimeSpan TempMinutes = TimeSpan.FromMinutes(MintuesSettingEntry.Value);
                    if (TempMinutes != TimeSpan.FromMinutes(0))
                    {
                        Minutes = TempMinutes;
                    }
                }
                if (SecondsSettingEntry != null)
                {
                    TimeSpan TempSeconds = TimeSpan.FromSeconds(SecondsSettingEntry.Value);
                    if (TempSeconds != TimeSpan.FromSeconds(0))
                    {
                        Seconds = TempSeconds;
                    }
                }
                _timerDurationDefaults[i] = Minutes + Seconds;
                _timerLabels[i].Text = _timerDurationDefaults[i].ToString(@"mm\:ss");

                List<string> WaypointList = new List<string>();
                if (WaypointSettingEntry != null)
                {
                    string Waypoints = WaypointSettingEntry.Value;
                    if (Waypoints != "")
                    {
                        WaypointList.Add(Waypoints);
                    }
                }
                if (WaypointList.Count > 0)
                {
                    _Waypoints[i].Clear();
                    _Waypoints[i].AddRange(WaypointList);
                }

                List<string> NotesList = new List<string>();
                List<bool> BroadcastNotesList = new List<bool>();
                if (NotesOneSettingEntry != null)
                {
                    string Notes = NotesOneSettingEntry.Value;
                    if (Notes != "")
                    {
                        NotesList.Add(Notes);
                    }
                }
                if (NotesTwoSettingEntry != null)
                {
                    string Notes = NotesTwoSettingEntry.Value;
                    if (Notes != "")
                    {
                        NotesList.Add(Notes);
                    }
                }
                if (NotesThreeSettingEntry != null)
                {
                    string Notes = NotesThreeSettingEntry.Value;
                    if (Notes != "")
                    {
                        NotesList.Add(Notes);
                    }
                }
                if (NotesFourSettingEntry != null)
                {
                    string Notes = NotesFourSettingEntry.Value;
                    if (Notes != "")
                    {
                        NotesList.Add(Notes);
                    }
                }
                if (BroadcastNotesOneSettingEntry != null)
                {
                    bool BroadcastNotes = BroadcastNotesOneSettingEntry.Value;
                    BroadcastNotesList.Add(BroadcastNotes);
                }
                else
                {
                    BroadcastNotesList.Add(false); // Add false for broadcast if note not null
                }
                if (BroadcastNotesTwoSettingEntry != null)
                {
                    bool BroadcastNotes = BroadcastNotesTwoSettingEntry.Value;
                    BroadcastNotesList.Add(BroadcastNotes);
                }
                else
                {
                    BroadcastNotesList.Add(false); // Add false for broadcast if note not null
                }
                if (BroadcastNotesThreeSettingEntry != null)
                {
                    bool BroadcastNotes = BroadcastNotesThreeSettingEntry.Value;
                    BroadcastNotesList.Add(BroadcastNotes);
                }
                else
                {
                    BroadcastNotesList.Add(false); // Add false for broadcast if note not null
                }
                if (BroadcastNotesFourSettingEntry != null)
                {
                    bool BroadcastNotes = BroadcastNotesFourSettingEntry.Value;
                    BroadcastNotesList.Add(BroadcastNotes);
                }
                else
                {
                    BroadcastNotesList.Add(false); // Add false for broadcast if note not null
                }

                if (NotesList.Count > 0)
                {
                    _Notes[i].Clear();
                    _Notes[i].AddRange(NotesList);
                }
                if (BroadcastNotesList.Count > 0)
                {
                    _Broadcast[i].Clear();
                    _Broadcast[i].AddRange(BroadcastNotesList);
                }
            }
        }
        protected override void Initialize()
        {
        }
        // Constants for SendInput
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint VK_SHIFT = 0x10; // Virtual key code for Shift
        private const uint VK_RETURN = 0x0D; // Virtual key code for Enter
        private const uint VK_CONTROL = 0x11; // Virtual key code for Ctrl

        // Structs for SendInput
        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public INPUTUNION u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUTUNION
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        // Import SendInput from user32.dll
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        // Import MapVirtualKey to convert virtual key to scan code
        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        // Simulate a single key press (down and up)
        private static void SendKey(uint virtualKey)
        {
            INPUT[] inputs = new INPUT[2];

            // Key down
            inputs[0] = new INPUT
            {
                type = 1, // INPUT_KEYBOARD
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (ushort)virtualKey,
                        wScan = (ushort)MapVirtualKey(virtualKey, 0),
                        dwFlags = 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            // Key up
            inputs[1] = new INPUT
            {
                type = 1, // INPUT_KEYBOARD
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (ushort)virtualKey,
                        wScan = (ushort)MapVirtualKey(virtualKey, 0),
                        dwFlags = KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(10); // Small delay to ensure input is processed
        }

        // Send multiple keys
        private static void SendTwoKeys(uint keyone, uint keytwo)
        {
            INPUT[] inputs = new INPUT[4];

            // Keyone down
            inputs[0] = new INPUT
            {
                type = 1, // INPUT_KEYBOARD
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (ushort)keyone,
                        wScan = (ushort)MapVirtualKey(keyone, 0),
                        dwFlags = 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            SendInput(1, new[] { inputs[0] }, Marshal.SizeOf(typeof(INPUT))); // Ctrl down
            Thread.Sleep(50);

            // Keytwo down
            inputs[1] = new INPUT
            {
                type = 1, // INPUT_KEYBOARD
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (ushort)keytwo,
                        wScan = (ushort)MapVirtualKey(keytwo, 0),
                        dwFlags = 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            SendInput(1, new[] { inputs[1] }, Marshal.SizeOf(typeof(INPUT))); // V down
            Thread.Sleep(50);

            // Keytwo up
            inputs[2] = new INPUT
            {
                type = 1, // INPUT_KEYBOARD
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (ushort)keytwo,
                        wScan = (ushort)MapVirtualKey(keytwo, 0),
                        dwFlags = KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            SendInput(1, new[] { inputs[2] }, Marshal.SizeOf(typeof(INPUT))); // V up
            Thread.Sleep(50);

            // Keyone up
            inputs[3] = new INPUT
            {
                type = 1, // INPUT_KEYBOARD
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (ushort)keyone,
                        wScan = (ushort)MapVirtualKey(keyone, 0),
                        dwFlags = KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            SendInput(1, new[] { inputs[3] }, Marshal.SizeOf(typeof(INPUT))); // Ctrl up
            Thread.Sleep(50);
        }

        // Simulate Ctrl+V (paste)
        private static void SendCtrlV()
        {
            INPUT[] inputs = new INPUT[4];

            // Ctrl down
            inputs[0] = new INPUT
            {
                type = 1, // INPUT_KEYBOARD
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (ushort)VK_CONTROL,
                        wScan = (ushort)MapVirtualKey(VK_CONTROL, 0),
                        dwFlags = 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            SendInput(1, new[] { inputs[0] }, Marshal.SizeOf(typeof(INPUT))); // Ctrl down
            Thread.Sleep(50);

            // V down
            inputs[1] = new INPUT
            {
                type = 1, // INPUT_KEYBOARD
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0x56, // Virtual key code for 'V'
                        wScan = (ushort)MapVirtualKey(0x56, 0),
                        dwFlags = 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            }; 
            SendInput(1, new[] { inputs[1] }, Marshal.SizeOf(typeof(INPUT))); // V down
            Thread.Sleep(50);

            // V up
            inputs[2] = new INPUT
            {
                type = 1, // INPUT_KEYBOARD
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0x56, // Virtual key code for 'V'
                        wScan = (ushort)MapVirtualKey(0x56, 0),
                        dwFlags = KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            SendInput(1, new[] { inputs[2] }, Marshal.SizeOf(typeof(INPUT))); // V up
            Thread.Sleep(50);

            // Ctrl up
            inputs[3] = new INPUT
            {
                type = 1, // INPUT_KEYBOARD
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (ushort)VK_CONTROL,
                        wScan = (ushort)MapVirtualKey(VK_CONTROL, 0),
                        dwFlags = KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            SendInput(1, new[] { inputs[3] }, Marshal.SizeOf(typeof(INPUT))); // Ctrl up
            Thread.Sleep(50);
        }

        // Copy text to clipboard
        private static void CopyToClipboard(string text)
        {
            // Ensure clipboard access is thread-safe
            Thread thread = new Thread(() => Clipboard.SetText(text));
            thread.SetApartmentState(ApartmentState.STA); // Clipboard requires STA
            thread.Start();
            thread.Join(); // Wait for clipboard operation to complete
        }
        private void ClipboardPaste(string Message, bool Broadcast)
        {
            if (Broadcast == true)
            {
                // Press Shift + Return to broadcast
                SendTwoKeys(VK_SHIFT, VK_RETURN);
                Thread.Sleep(100);
            }
            else
            {
                // Press Enter
                SendKey(VK_RETURN);
            }
            Thread.Sleep(100);
            // Copy "hello" to clipboard
            CopyToClipboard(Message);
            Thread.Sleep(100);
            // Simulate Ctrl+V to paste
            SendCtrlV();
            Thread.Sleep(100);
            // Press Enter again
            SendKey(VK_RETURN);
            Thread.Sleep(100);
        }
        private async Task NotesIcon_Click(int index)
        {
            List<string> notes = _Notes[index];

            for (int i = 0; i < _notesIcon.Count(); i++)
            {
                _notesIcon[i].Enabled = false;
                _waypointIcon[i].Enabled = false;
            }

            ShowInputPanel("Notes");
            bool wasKeybindPressed = await WaitForKeybindAsync();
            await WaitForShiftKeyUpAsync();
            _inputPanel?.Hide();
            _inputPanel = null;
            Thread.Sleep(1000);

            if (wasKeybindPressed)
            {
                for (int i = 0; i < notes.Count; i++)
                {
                    string message = notes[i];
                    if (message != null && message.Length > 0)
                    {
                        ClipboardPaste(notes[i], _eventNotes[index].Broadcast[i]);
                    }
                }
            }

            for (int i = 0; i < _notesIcon.Count(); i++)
            {
                _notesIcon[i].Enabled = true;
                _waypointIcon[i].Enabled = true;
            }
        }
        private void WaypointIcon_Click(int index)
        {
            List<string> waypoints = _Waypoints[index];

            for (int i = 0; i < _waypointIcon.Count(); i++)
            {
                _notesIcon[i].Enabled = false;
                _waypointIcon[i].Enabled = false;
            }

            //ShowInputPanel("Waypoint");
            //bool wasKeybindPressed = await WaitForKeybindAsync();
            //await WaitForShiftKeyUpAsync();
            //_inputPanel?.Hide();
            //_inputPanel = null;
            //Thread.Sleep(1000);

            //if (wasKeybindPressed)
            //{
                for (int i = 0; i < waypoints.Count; i++)
                {
                    string message = waypoints[i];
                    if (message != null && message.Length > 0)
                    {
                        ClipboardPaste(waypoints[i], false);
                    }
                }
            //}

            for (int i = 0; i < _waypointIcon.Count(); i++)
            {
                _notesIcon[i].Enabled = true;
                _waypointIcon[i].Enabled = true;
            }
        }
        private void ShowInputPanel(string Title)
        {
            _inputPanel = new Blish_HUD.Controls.Panel()
            {
                Parent = GameService.Graphics.SpriteScreen,
                Width = 300,
                Height = 40,
                BackgroundColor = Color.Black,
                Opacity = 0.9f
            };
            _inputPanel.Location = new Point((GameService.Graphics.SpriteScreen.Size.X - _inputPanel.Size.X) / 2, 30);

            _instructionLabel = new Blish_HUD.Controls.Label()
            {
                Text = "------" + Title + "------\n Press (" + _postNotesKeybind.Value.GetBindingDisplayText() + ") to continue... OR (" + _cancelNotesKeybind.Value.GetBindingDisplayText() + ") to cancel",
                Size = new Point(500, 40),
                HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Center,
                Parent = _inputPanel,
                Font = GameService.Content.DefaultFont12
            };
            _instructionLabel.Location = new Point((_inputPanel.Size.X - _instructionLabel.Size.X) / 2, ((_inputPanel.Size.Y/2) - _instructionLabel.Size.Y) / 2);
        }
        private Task<bool> WaitForKeybindAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            KeyboardState currentKeyboardState = Keyboard.GetState();

            // Register the keybind event
            EventHandler<EventArgs> handler = null;
            EventHandler<EventArgs> handler2 = null;
            handler = (s, e) =>
            {
                tcs.TrySetResult(true);
                _postNotesKeybind.Value.Activated -= handler;
                _cancelNotesKeybind.Value.Activated -= handler2;
            };
            _postNotesKeybind.Value.Activated += handler;

            // Handler for Escape key
            handler2 = (s, e) =>
            {
                tcs.TrySetResult(false);
                _postNotesKeybind.Value.Activated -= handler;
                _cancelNotesKeybind.Value.Activated -= handler2;
            };
            _cancelNotesKeybind.Value.Activated += handler2;

            return tcs.Task;
        }
        public async Task WaitForShiftKeyUpAsync()
        {
            // Get the Input service from Blish HUD
            var input = GameService.Input;

            // Check if either Shift key is currently pressed
            if (input.Keyboard.ActiveModifiers.HasFlag(ModifierKeys.Shift))
            {
                // Create a TaskCompletionSource to handle async waiting
                var tcs = new TaskCompletionSource<bool>();

                // Define a handler for key state changes
                void KeyStateChanged(object sender, KeyboardEventArgs e)
                {
                    // Check if Shift is no longer pressed
                    if (!input.Keyboard.ActiveModifiers.HasFlag(ModifierKeys.Shift))
                    {
                        // Unsubscribe from the event to avoid memory leaks
                        input.Keyboard.KeyStateChanged -= KeyStateChanged;
                        // Signal task completion
                        tcs.SetResult(true);
                    }
                }

                // Subscribe to the KeyStateChanged event
                input.Keyboard.KeyStateChanged += KeyStateChanged;

                // Wait for the task to complete (Shift key released)
                await tcs.Task;
            }
        }
        private void InfoIcon_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            if (_InfoWindow.Visible == true)
            {
                _InfoWindow.Hide();
            }
            else
            {
                _InfoWindow.Show();
            }
        }
        private void SettingsIcon_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            if (_SettingsWindow.Visible == true)
            {
                _SettingsWindow.Hide();
            }
            else
            {
                _SettingsWindow.Show();
            }
        }

        private void CornerIcon_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            // Toggle window visibility
            if (_TimerWindow.Visible)
            {
                _TimerWindow.Hide();
                //_InfoWindow.Hide();
            }
            else
            {
                _TimerWindow.Show();
                //_InfoWindow.Show();
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
            if (currentIntervalStartDate >= currentTime || currentTime <= oneWeekAheadcurrent)
            {
                NextBaubleStartDate = currentIntervalStartDate;
                EndofBaubleWeek = oneWeekAheadcurrent;
                FarmStatus = "ON";
                Statuscolor = Color.LimeGreen;
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
            List<TimerLogData> eventDataList = new List<TimerLogData>();
            string moduleDir = DirectoriesManager.GetFullDirectoryPath("Shiny_Baubles");
            string jsonFilePath = Path.Combine(moduleDir, "Event_Timers.json");
            for (int i = 0; i < TimerRowNum; i++)
            {
                DateTime? startTime = null;
                if (_timerRunning[i] == true)
                {
                    startTime = _timerStartTimes[i];
                }
                eventDataList.Add(new TimerLogData
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
                File.WriteAllText(jsonFilePath, jsonContent);
                //Logger.Info($"Saved {_eventDataList.Count} events to {_jsonFilePath}");
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to save JSON file: {ex.Message}");
            }
            //eventDataList = new List<EventData>();
        }
        private void CreateJsonEventsDefaults()
        {
            try
            {
                string jsonFilePath = @"Defaults\Package_Defaults.json";
                Stream json = ContentsManager.GetFileStream(jsonFilePath);

                string moduleDir = DirectoriesManager.GetFullDirectoryPath("Shiny_Baubles");
                string jsonFilePath2 = Path.Combine(moduleDir, "Package_Defaults.json");
                using (var fileStream = File.Create(jsonFilePath2))
                {
                    json.CopyTo(fileStream);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to copy JSON event default file: {ex.Message}");
            }
            //eventDataList = new List<EventData>();
        }
        protected override async Task LoadAsync()
        {
            #region Initialize Default Data

            try
            {
                _PackageData = new List<PackageData>();
                _eventNotes = new List<TimerDetailData>();
                string moduleDir2 = DirectoriesManager.GetFullDirectoryPath("Shiny_Baubles");
                string jsonFilePath2 = Path.Combine(moduleDir2, "Package_Defaults.json");
                if (!File.Exists(jsonFilePath2))
                {
                    CreateJsonEventsDefaults();
                }
                using (StreamReader reader = new StreamReader(jsonFilePath2))
                {
                    string jsonContent = await reader.ReadToEndAsync();
                    _PackageData = JsonSerializer.Deserialize<List<PackageData>>(jsonContent, _jsonOptions);
                    //Logger.Info($"Loaded {_eventDataList.Count} events from {jsonFilePath}");
                }

                int index = 0;
                int Defaultindex = _PackageData.FindIndex(p => p.PackageName == _CurrentPackageSelection.Value);
                if (Defaultindex >= 0)
                {
                    index = Defaultindex;
                }
                _eventNotes = _PackageData[index].TimerDetailData;
                var timerNotesData = _PackageData[index].TimerDetailData;
                int Count = _eventNotes.Count();
                TimerRowNum = Count;
                _timerStartTimes = new DateTime?[TimerRowNum];
                _Notes = new List<List<string>>();
                _Broadcast = new List<List<bool>>();
                _Waypoints = new List<List<string>>();
                _timerRunning = new bool[TimerRowNum];
                _timerLabelDescriptions = new Blish_HUD.Controls.Label[TimerRowNum];
                _notesIcon = new Image[TimerRowNum];
                _waypointIcon = new Image[TimerRowNum];
                _timerLabels = new Blish_HUD.Controls.Label[TimerRowNum];
                _resetButtons = new StandardButton[TimerRowNum];
                _stopButtons = new StandardButton[TimerRowNum];
                _customDropdownTimers = new Dropdown[TimerRowNum];
                _TimerWindowsOrdered = new Blish_HUD.Controls.Panel[TimerRowNum];
                _timerDurationOverride = new TimeSpan[TimerRowNum];
                _timerDurationDefaults = new TimeSpan[TimerRowNum];
                _ModifierKeys = new int[TimerRowNum];
                _PrimaryKey = new int[TimerRowNum];
                _TimerMinutes = new double[TimerRowNum];
                _TimerSeconds = new double[TimerRowNum];
                _TimerID = new int[TimerRowNum];

                for (int i = 0; i < TimerRowNum; i++)
                {
                    _Notes.Add(timerNotesData[i].Notes);
                    _Broadcast.Add(timerNotesData[i].Broadcast);
                    _Waypoints.Add(timerNotesData[i].Waypoints);
                    _timerLabelDescriptions[i] = new Blish_HUD.Controls.Label();
                    _timerLabelDescriptions[i].Text = timerNotesData[i].Description;
                    _TimerMinutes[i] = timerNotesData[i].Minutes;
                    _TimerSeconds[i] = timerNotesData[i].Seconds;
                    _TimerID[i] = timerNotesData[i].ID;

                    //Logger.Info($"Waypoint: {timerNotesData[i].Waypoint} Notes: {timerNotesData[i].Notes}");
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load Package_Defaults JSON file: {ex.Message}");
            }

            // Initialize all timers as not started
            for (int i = 0; i < TimerRowNum; i++)
            {
                _timerDurationDefaults[i] = TimeSpan.FromMinutes(0);
                _timerLabels[i] = new Blish_HUD.Controls.Label();
                _timerStartTimes[i] = null; // Not started
                _timerRunning[i] = false;
            }

            LoadTimerDefaults(TimerRowNum);

            #endregion


            try
            {
                #region Timer Window Window

                //// Assign all textures and parameters for timer window
                _asyncTimertexture = AsyncTexture2D.FromAssetId(155985); //GameService.Content.DatAssetCache.GetTextureFromAssetId(155985)
                _asyncGeneralSettingstexture = AsyncTexture2D.FromAssetId(156701);
                _asyncNotesSettingstexture = AsyncTexture2D.FromAssetId(1654244);
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
                _timerPanel = new Blish_HUD.Controls.Panel
                {
                    Parent = _TimerWindow, // Set the panel's parent to the StandardWindow
                    Size = new Point(_TimerWindow.ContentRegion.Size.X + 500, _TimerWindow.ContentRegion.Size.Y + 500), // Match the panel to the content region
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

                _infoPanel = new Blish_HUD.Controls.Panel
                {
                    Parent = _InfoWindow, // Set the panel's parent to the StandardWindow
                    Size = new Point(_InfoWindow.ContentRegion.Size.X + 500, _InfoWindow.ContentRegion.Size.Y + 500), // Match the panel to the content region
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
                Blish_HUD.Controls.Label statusLabel = new Blish_HUD.Controls.Label
                {
                    Text = "Bauble Farm Status :",
                    Size = new Point(180, 30),
                    Location = new Point(30, 30),
                    Font = GameService.Content.DefaultFont16,
                    Parent = _InfoWindow
                };
                _statusValue = new Blish_HUD.Controls.Label
                {
                    Text = FarmStatus,
                    Size = new Point(230, 30),
                    Location = new Point(190, 30),
                    Font = GameService.Content.DefaultFont16,
                    TextColor = Statuscolor,
                    Parent = _InfoWindow
                };
                Blish_HUD.Controls.Label startTimeLabel = new Blish_HUD.Controls.Label
                {
                    Text = "Start ->",
                    Size = new Point(100, 30),
                    Location = new Point(30, 60),
                    Font = GameService.Content.DefaultFont16,
                    Parent = _InfoWindow
                };
                _startTimeValue = new Blish_HUD.Controls.Label
                {
                    Text = NextBaubleStartDate.ToString("hh:mm tt (MMMM dd, yyyy)"),
                    Size = new Point(230, 30),
                    Location = new Point(90, 60),
                    Font = GameService.Content.DefaultFont16,
                    StrokeText = true,
                    TextColor = Color.DodgerBlue,
                    Parent = _InfoWindow
                };
                Blish_HUD.Controls.Label endTimeLabel = new Blish_HUD.Controls.Label
                {
                    Text = "End ->",
                    Size = new Point(100, 30),
                    Location = new Point(30, 90),
                    Font = GameService.Content.DefaultFont16,
                    Parent = _InfoWindow
                };
                _endTimeValue = new Blish_HUD.Controls.Label
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
                    Location = new Point(0, 30),
                    Parent = _TimerWindow
                };
                _stopButton.Click += (s, e) => StopButton_Click();

                _InOrdercheckbox = new Checkbox
                {
                    Text = "Order by Timer",
                    Size = new Point(120, 30),
                    Location = new Point(130, 30),
                    Parent = _TimerWindow
                };
                _InOrdercheckbox.Checked = _InOrdercheckboxDefault.Value;
                _InOrdercheckbox.Click += (s, e) => InOrdercheckbox_Click();

                Blish_HUD.Controls.Label eventsLabel = new Blish_HUD.Controls.Label
                {
                    Text = "Events",
                    Size = new Point(120, 30),
                    Location = new Point(60, 65),
                    Font = GameService.Content.DefaultFont16,
                    StrokeText = true,
                    TextColor = Color.DodgerBlue,
                    Parent = _TimerWindow
                };
                Blish_HUD.Controls.Label timerLabel = new Blish_HUD.Controls.Label
                {
                    Text = "Timer",
                    Size = new Point(120, 30),
                    Location = new Point(160, 65),
                    Font = GameService.Content.DefaultFont16,
                    StrokeText = true,
                    TextColor = Color.DodgerBlue,
                    Parent = _TimerWindow
                };
                Blish_HUD.Controls.Label overridesLabel = new Blish_HUD.Controls.Label
                {
                    Text = "Override (min)",
                    Size = new Point(120, 30),
                    Location = new Point(300, 65),
                    Font = GameService.Content.DefaultFont16,
                    StrokeText = true,
                    Visible = false,
                    TextColor = Color.DodgerBlue,
                    Parent = _TimerWindow
                };

                // Create UI elements for each timer
                //string[] Descriptions = { "SVET", "EVET", "NVET", "WVET", "SAP", "BALTH", "WYVERN", "BRAMBLE", "OOZE", "GUZZLER", "TM", "STONEHEADS" };

                for (int i = 0; i < TimerRowNum; i++)
                {
                    int index = i; // Capture index for event handlers

                    // Timer Panels
                    _TimerWindowsOrdered[i] = new Blish_HUD.Controls.Panel
                    {
                        Parent = _TimerWindow,
                        Size = new Point(390, 30),
                        Location = new Point(0, 95 + (i * 30)),
                    };

                    // WAypoint Icon
                    AsyncTexture2D waypointTexture = AsyncTexture2D.FromAssetId(102348);
                    _waypointIcon[i] = new Image
                    {
                        Texture = waypointTexture,
                        Location = new Point(0, 0),
                        Size = new Point(32, 32),
                        Opacity = 0.7f,
                        //Visible = false,
                        Parent = _TimerWindowsOrdered[i]
                    };
                    _waypointIcon[i].MouseEntered += (sender, e) => {
                        Image noteIcon = sender as Image;
                        noteIcon.Location = new Point(0 - 2, 0 - 2);
                        noteIcon.Size = new Point(36, 36);
                        noteIcon.Opacity = 1f;
                    };
                    _waypointIcon[i].MouseLeft += (sender, e) => {
                        Image noteIcon = sender as Image;
                        noteIcon.Location = new Point(0, 0);
                        noteIcon.Size = new Point(32, 32);
                        noteIcon.Opacity = 0.7f;
                    };
                    _waypointIcon[i].Click += (s, e) => WaypointIcon_Click(index);

                    // Notes Icon
                    AsyncTexture2D notesTexture = AsyncTexture2D.FromAssetId(2604584);
                    _notesIcon[i] = new Image
                    {
                        Texture = notesTexture,
                        Location = new Point(30, 0),
                        Size = new Point(32, 32),
                        Opacity = 0.7f,
                        //Visible = false,
                        Parent = _TimerWindowsOrdered[i]
                    };
                    _notesIcon[i].MouseEntered += (sender, e) => {
                        Image noteIcon = sender as Image;
                        noteIcon.Location = new Point(30 - 2, 0 - 2);
                        noteIcon.Size = new Point(36, 36);
                        noteIcon.Opacity = 1f;
                    };
                    _notesIcon[i].MouseLeft += (sender, e) => {
                        Image noteIcon = sender as Image;
                        noteIcon.Location = new Point(30, 0);
                        noteIcon.Size = new Point(32, 32);
                        noteIcon.Opacity = 0.7f;
                    };
                    _notesIcon[i].Click += async (s, e) => await NotesIcon_Click(index);

                    // Timer Event Description
                    _timerLabelDescriptions[i].Size = new Point(100, 30);
                    _timerLabelDescriptions[i].Location = new Point(60, 0);
                    _timerLabelDescriptions[i].Parent = _TimerWindowsOrdered[i];

                    // Timer label
                    _timerLabels[i].Text = _timerDurationDefaults[i].ToString(@"mm\:ss");
                    _timerLabels[i].Size = new Point(100, 30);
                    _timerLabels[i].Location = new Point(130, 0);
                    _timerLabels[i].HorizontalAlignment = Blish_HUD.Controls.HorizontalAlignment.Center;
                    _timerLabels[i].Font = GameService.Content.DefaultFont16;
                    _timerLabels[i].TextColor = Color.GreenYellow;
                    _timerLabels[i].Parent = _TimerWindowsOrdered[i];

                    // Reset button
                    _resetButtons[i] = new StandardButton
                    {
                        Text = "Start",
                        Size = new Point(50, 30),
                        Location = new Point(210, 0),
                        Parent = _TimerWindowsOrdered[i]
                    };
                    _resetButtons[i].Click += (s, e) => ResetButton_Click(index);

                    // Reset button
                    _stopButtons[i] = new StandardButton
                    {
                        Text = "Stop",
                        Size = new Point(50, 30),
                        Location = new Point(260, 0),
                        Parent = _TimerWindowsOrdered[i]
                    };
                    _stopButtons[i].Click += (s, e) => stopButtons_Click(index);

                    // Override Timer dropdown
                    _customDropdownTimers[i] = new Dropdown
                    {
                        Items = { "Default", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" },
                        Size = new Point(80, 30),
                        Location = new Point(310, 0),
                        Visible = false,
                        Parent = _TimerWindowsOrdered[i]
                    };
                    _customDropdownTimers[i].ValueChanged += (s, e) => dropdownChanged_Click(index);


                    if (_postNotesKeybind.Value.PrimaryKey == Microsoft.Xna.Framework.Input.Keys.None || _cancelNotesKeybind.Value.PrimaryKey == Microsoft.Xna.Framework.Input.Keys.None)
                    {
                        _notesIcon[i].Hide();
                        _waypointIcon[i].Hide();
                    }
                    else
                    {
                        _notesIcon[i].Show();
                        _waypointIcon[i].Show();
                    }
                }
                #endregion

                #region Timer Settings Window

                _SettingsWindow = new TabbedWindow2(
                    NoTexture,
                    new Rectangle(0, 0, 1050, 650), // The windowRegion
                    new Rectangle(0, 0, 1050, 650)) // The contentRegion
                {
                    Parent = GameService.Graphics.SpriteScreen,
                    Title = "Settings",
                    Location = new Point(300, 300),
                    SavesPosition = true,
                    Visible = false,
                    Id = $"{nameof(BaubleFarmModule)}_BaubleFarmTimerSettingsWindow_38d37290-b5f9-447d-97ea-45b0b50e5f56"
                };

                AsyncTexture2D clockTexture = AsyncTexture2D.FromAssetId(155156);
                _SettingsWindow.Tabs.Add(new Tab(
                    clockTexture,
                    () => new TimerSettingsTabView(),
                    "Timer Events"
                ));
                AsyncTexture2D staticTexture = AsyncTexture2D.FromAssetId(156909);
                _SettingsWindow.Tabs.Add(new Tab(
                    staticTexture,
                    () => new StaticEventSettingsTabView(),
                    "Static Events"
                ));
                AsyncTexture2D packageTexture = AsyncTexture2D.FromAssetId(156701);
                _SettingsWindow.Tabs.Add(new Tab(
                    packageTexture,
                    () => new PackageSettingsTabView(),
                    "Packages"
                ));
                AsyncTexture2D listTexture = AsyncTexture2D.FromAssetId(157109);
                _SettingsWindow.Tabs.Add(new Tab(
                    listTexture,
                    () => new ListSettingsTabView(),
                    "General Settings"
                ));

                AsyncTexture2D infoTexture = AsyncTexture2D.FromAssetId(440023);
                Image infoIcon = new Image
                {
                    Texture = infoTexture,
                    Location = new Point(250, 30),
                    Size = new Point(32, 32),
                    Opacity = 0.7f,
                    //Visible = false,
                    Parent = _TimerWindow
                };
                infoIcon.MouseEntered += (sender, e) => {
                    infoIcon.Location = new Point(250 - 4, 30 - 4);
                    infoIcon.Size = new Point(40, 40);
                    infoIcon.Opacity = 1f;
                };
                infoIcon.MouseLeft += (s, e) => {
                    infoIcon.Location = new Point(250, 30);
                    infoIcon.Size = new Point(32, 32);
                    infoIcon.Opacity = 0.7f;
                };
                infoIcon.Click += InfoIcon_Click;

                AsyncTexture2D geartexture = AsyncTexture2D.FromAssetId(155052);
                Image settingsIcon = new Image
                {
                    Texture = geartexture,
                    Location = new Point(280, 30),
                    Size = new Point(32, 32),
                    Opacity = 0.7f,
                    //Visible = false,
                    Parent = _TimerWindow
                };
                settingsIcon.MouseEntered += (sender, e) => {
                    settingsIcon.Location = new Point(280 - 4, 30 - 4);
                    settingsIcon.Size = new Point(40, 40);
                    settingsIcon.Opacity = 1f;
                };
                settingsIcon.MouseLeft += (s, e) => {
                    settingsIcon.Location = new Point(280, 30);
                    settingsIcon.Size = new Point(32, 32);
                    settingsIcon.Opacity = 0.7f;
                };
                settingsIcon.Click += SettingsIcon_Click;

                #endregion
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load Time UI: {ex.Message}");
            }

            #region Load Backup Timer JSON

            List<TimerLogData> eventDataList = new List<TimerLogData>();
            string moduleDir = DirectoriesManager.GetFullDirectoryPath("Shiny_Baubles");
            string jsonFilePath = Path.Combine(moduleDir, "Event_Timers.json");
            if (File.Exists(jsonFilePath))
            {
                try
                {
                    using (StreamReader reader = new StreamReader(jsonFilePath))
                    {
                        string jsonContent = await reader.ReadToEndAsync();
                        eventDataList = JsonSerializer.Deserialize<List<TimerLogData>>(jsonContent, _jsonOptions);
                        //Logger.Info($"Loaded {_eventDataList.Count} events from {jsonFilePath}");
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
                                _customDropdownTimers[i].Enabled = false;
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
                    Logger.Info($"Failed to load Event_Timers JSON file: {ex.Message}");
                }
            }
            else
            {
                Logger.Info("No Timers Event_Timers JSON file found.");
            }

            #endregion
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

        public void Restart()
        {
            // First, unload the module. This runs your clean-up code.
            Unload();
            ModuleInstance = this;
            // Then, re-initialize it. This should be a full re-initialization.
            Task task = LoadAsync();
        }
    }
}
