using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using GregsStack.InputSimulatorStandard;
using GregsStack.InputSimulatorStandard.Native;
using Vortice.XInput;

namespace ControllerToMnK;

public enum MouseSource { None, LeftStick, RightStick, DPad }

public class MainForm : Form
{
    // ── TextBoxes ────────────────────────────────────────────────────────────
    private TextBox edit_W = null!, edit_A = null!, edit_S = null!, edit_D = null!;
    private TextBox edit_RUp = null!, edit_RDown = null!, edit_RLeft = null!, edit_RRight = null!;
    private TextBox edit_Up = null!, edit_Down = null!, edit_Left = null!, edit_Right = null!;
    private TextBox edit_A_Btn = null!, edit_B_Btn = null!, edit_X_Btn = null!, edit_Y_Btn = null!;
    private TextBox edit_L3 = null!, edit_R3 = null!;
    private TextBox edit_LB = null!, edit_RB = null!, edit_LT = null!, edit_RT = null!;
    private TextBox edit_Thresh = null!, edit_Mult = null!;

    // ── Controls ─────────────────────────────────────────────────────────────
    private RadioButton radMouseNone = null!, radMouseLeft = null!, radMouseRight = null!, radMouseDPad = null!;
    private CheckBox chk_Enable = null!, chk_DarkMode = null!;
    private Button btn_Apply = null!, btn_Exit = null!, btn_Help = null!;
    private ComboBox cbo_Presets = null!;
    private Button btn_SavePreset = null!, btn_DeletePreset = null!;

    // ── Input ─────────────────────────────────────────────────────────────────
    private System.Windows.Forms.Timer loopTimer = null!;
    private InputSimulator inputSim = null!;

    // ── State ─────────────────────────────────────────────────────────────────
    private bool isEnabled = false;
    private MouseSource currentMouseSource = MouseSource.RightStick;

    private VirtualKeyCode key_W, key_A, key_S, key_D;
    private VirtualKeyCode key_RUp, key_RDown, key_RLeft, key_RRight;
    private VirtualKeyCode key_Up, key_Down, key_Left, key_Right;
    private VirtualKeyCode key_A_Btn, key_B_Btn, key_X_Btn, key_Y_Btn;
    private VirtualKeyCode key_LB, key_RB, key_LT, key_RT;
    private VirtualKeyCode key_L3, key_R3;

    private int mouseThreshold = 7;
    private double mouseMultiplier = 0.4;

    private bool w_down, a_down, s_down, d_down;
    private bool r_up_down, r_down_down, r_left_down, r_right_down;
    private bool up_down, down_down, left_down, right_down;
    private bool btnA_down, btnB_down, btnX_down, btnY_down;
    private bool lb_down, rb_down, lt_down, rt_down;
    private bool l3_down, r3_down;

    // ── Presets ───────────────────────────────────────────────────────────────
    private static readonly string SaveDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Controller to MnK");
    private static readonly string SavePath = Path.Combine(SaveDir, "presets.json");
    private PresetsFile presetsData = new();

    // ─────────────────────────────────────────────────────────────────────────
    public MainForm()
    {
        inputSim = new InputSimulator();
        InitializeUI();
        LoadPresetsFile();
        ApplySettings();

        loopTimer = new System.Windows.Forms.Timer { Interval = 10 };
        loopTimer.Tick += LoopTimer_Tick;
        loopTimer.Start();
    }

    // ── UI Setup ──────────────────────────────────────────────────────────────
    private void InitializeUI()
    {
        this.Text = "Controller to MnK Dashboard";
        this.Size = new Size(560, 590);
        this.MaximizeBox = false;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        this.FormClosed += (s, e) => ExitApp();

        try { this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        // ── COLUMN 1: Left Stick & D-Pad ──────────────────────────────────────
        var gbMove = new GroupBox { Text = "Left Stick (Keyboard)", Bounds = new Rectangle(15, 10, 150, 140), Parent = this };
        CreateLabelTextBox(gbMove, "Up:", "w", 25, out edit_W);
        CreateLabelTextBox(gbMove, "Left:", "a", 55, out edit_A);
        CreateLabelTextBox(gbMove, "Down:", "s", 85, out edit_S);
        CreateLabelTextBox(gbMove, "Right:", "d", 115, out edit_D);

        var gbDPad = new GroupBox { Text = "D-Pad (Keyboard)", Bounds = new Rectangle(15, 160, 150, 140), Parent = this };
        CreateLabelTextBox(gbDPad, "Up:", "up", 25, out edit_Up);
        CreateLabelTextBox(gbDPad, "Down:", "down", 55, out edit_Down);
        CreateLabelTextBox(gbDPad, "Left:", "left", 85, out edit_Left);
        CreateLabelTextBox(gbDPad, "Right:", "right", 115, out edit_Right);

        // ── COLUMN 2: Face Buttons & Stick Clicks ─────────────────────────────
        var gbFace = new GroupBox { Text = "Face Buttons", Bounds = new Rectangle(180, 10, 150, 140), Parent = this };
        CreateLabelTextBox(gbFace, "A:", "space", 25, out edit_A_Btn);
        CreateLabelTextBox(gbFace, "B:", "e", 55, out edit_B_Btn);
        CreateLabelTextBox(gbFace, "X:", "r", 85, out edit_X_Btn);
        CreateLabelTextBox(gbFace, "Y:", "q", 115, out edit_Y_Btn);

        var gbClicks = new GroupBox { Text = "Stick Clicks", Bounds = new Rectangle(180, 160, 150, 90), Parent = this };
        CreateLabelTextBox(gbClicks, "L3:", "f", 25, out edit_L3);
        CreateLabelTextBox(gbClicks, "R3:", "m", 55, out edit_R3);

        // ── COLUMN 3: Shoulders, Triggers & Mouse Settings ────────────────────
        var gbShoulders = new GroupBox { Text = "Shoulders & Triggers", Bounds = new Rectangle(345, 10, 180, 140), Parent = this };
        CreateLabelTextBox(gbShoulders, "LB:", "lshift", 25, out edit_LB, 30, 100);
        CreateLabelTextBox(gbShoulders, "RB:", "ctrl", 55, out edit_RB, 30, 100);
        CreateLabelTextBox(gbShoulders, "LT:", "lbutton", 85, out edit_LT, 30, 100);
        CreateLabelTextBox(gbShoulders, "RT:", "rbutton", 115, out edit_RT, 30, 100);

        var gbMouseOpts = new GroupBox { Text = "Mouse Settings", Bounds = new Rectangle(345, 160, 180, 90), Parent = this };
        CreateLabelTextBox(gbMouseOpts, "Dead:", "7", 25, out edit_Thresh, 40, 100);
        CreateLabelTextBox(gbMouseOpts, "Sens:", "0.4", 55, out edit_Mult, 40, 100);

        // ── ROW 3: Right Stick & Mouse Source ─────────────────────────────────
        var gbRightStick = new GroupBox { Text = "Right Stick (Keyboard)", Bounds = new Rectangle(15, 310, 150, 140), Parent = this };
        CreateLabelTextBox(gbRightStick, "Up:", "i", 25, out edit_RUp);
        CreateLabelTextBox(gbRightStick, "Down:", "k", 55, out edit_RDown);
        CreateLabelTextBox(gbRightStick, "Left:", "j", 85, out edit_RLeft);
        CreateLabelTextBox(gbRightStick, "Right:", "l", 115, out edit_RRight);

        var gbMouseSource = new GroupBox { Text = "Mouse Controller", Bounds = new Rectangle(180, 310, 150, 140), Parent = this };
        radMouseNone = new RadioButton { Text = "None", Bounds = new Rectangle(15, 25, 120, 20), Parent = gbMouseSource };
        radMouseLeft = new RadioButton { Text = "Left Stick", Bounds = new Rectangle(15, 50, 120, 20), Parent = gbMouseSource };
        radMouseRight = new RadioButton { Text = "Right Stick", Bounds = new Rectangle(15, 75, 120, 20), Checked = true, Parent = gbMouseSource };
        radMouseDPad = new RadioButton { Text = "D-Pad", Bounds = new Rectangle(15, 100, 120, 20), Parent = gbMouseSource };

        radMouseNone.CheckedChanged += UpdateTextBoxStates;
        radMouseLeft.CheckedChanged += UpdateTextBoxStates;
        radMouseRight.CheckedChanged += UpdateTextBoxStates;
        radMouseDPad.CheckedChanged += UpdateTextBoxStates;

        // ── Control Panel ─────────────────────────────────────────────────────
        btn_Apply = new Button { Text = "Apply Settings", Bounds = new Rectangle(345, 310, 180, 30), Parent = this };
        btn_Apply.Click += (s, e) => ApplySettings();

        chk_Enable = new CheckBox { Text = "Enable Remapper", Bounds = new Rectangle(345, 350, 110, 20), Checked = true, Parent = this };
        chk_DarkMode = new CheckBox { Text = "Dark Mode", Bounds = new Rectangle(455, 350, 80, 20), Checked = true, Parent = this };
        chk_DarkMode.CheckedChanged += ToggleDarkMode;

        btn_Help = new Button { Text = "Help (Key Names)", Bounds = new Rectangle(345, 380, 180, 30), Parent = this };
        btn_Help.Click += (s, e) => ShowHelp();

        btn_Exit = new Button { Text = "Exit Script", Bounds = new Rectangle(345, 420, 180, 30), Parent = this };
        btn_Exit.Click += (s, e) => ExitApp();

        // ── Presets Bar ───────────────────────────────────────────────────────
        var gbPresets = new GroupBox { Text = "Presets", Bounds = new Rectangle(15, 462, 510, 80), Parent = this };

        new Label { Text = "Preset:", Bounds = new Rectangle(10, 28, 48, 20), Parent = gbPresets };
        cbo_Presets = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Bounds = new Rectangle(62, 25, 190, 24),
            Parent = gbPresets
        };

        btn_SavePreset = new Button { Text = "Save As...", Bounds = new Rectangle(262, 24, 75, 26), Parent = gbPresets };
        btn_SavePreset.Click += SavePreset_Click;

        btn_DeletePreset = new Button { Text = "Delete", Bounds = new Rectangle(345, 24, 65, 26), Parent = gbPresets };
        btn_DeletePreset.Click += DeletePreset_Click;

        var btn_ResetPreset = new Button { Text = "Reset", Bounds = new Rectangle(418, 24, 80, 26), Parent = gbPresets };
        btn_ResetPreset.Click += ResetPreset_Click;

        cbo_Presets.SelectedIndexChanged += (s, e) => LoadSelectedPreset();

        UpdateTextBoxStates(null, null);
        // Apply dark mode on startup
        ToggleDarkMode(null, null);
    }

    // ── Preset Logic ──────────────────────────────────────────────────────────
    private void LoadPresetsFile()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                var json = File.ReadAllText(SavePath);
                presetsData = JsonSerializer.Deserialize<PresetsFile>(json) ?? new PresetsFile();
            }
        }
        catch { presetsData = new PresetsFile(); }

        // Ensure at least a Default preset exists
        if (presetsData.Presets.Count == 0)
            presetsData.Presets["Default"] = new PresetSettings();

        RefreshPresetComboBox(presetsData.LastPreset);

        // Apply the last used preset to the UI
        if (presetsData.Presets.TryGetValue(presetsData.LastPreset, out var preset))
            ApplyPresetToUI(preset);
    }

    private void SavePresetsFile()
    {
        try
        {
            Directory.CreateDirectory(SaveDir);
            var json = JsonSerializer.Serialize(presetsData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SavePath, json);
        }
        catch { /* silently fail */ }
    }

    private void RefreshPresetComboBox(string selectName)
    {
        cbo_Presets.Items.Clear();
        foreach (var name in presetsData.Presets.Keys)
            cbo_Presets.Items.Add(name);

        int idx = cbo_Presets.Items.IndexOf(selectName);
        cbo_Presets.SelectedIndex = idx >= 0 ? idx : 0;
    }

    private void LoadSelectedPreset()
    {
        if (cbo_Presets.SelectedItem is string name && presetsData.Presets.TryGetValue(name, out var preset))
        {
            ApplyPresetToUI(preset);
            presetsData.LastPreset = name;
            SavePresetsFile();
        }
    }

    private void SavePreset_Click(object? sender, EventArgs e)
    {
        string suggested = cbo_Presets.SelectedItem?.ToString() ?? "My Preset";
        string? name = PromptInput("Save Preset", "Enter a name for this preset:", suggested);
        if (string.IsNullOrWhiteSpace(name)) return;

        presetsData.Presets[name] = GetPresetFromUI();
        presetsData.LastPreset = name;
        SavePresetsFile();
        RefreshPresetComboBox(name);
    }

    private void DeletePreset_Click(object? sender, EventArgs e)
    {
        if (cbo_Presets.SelectedItem is not string name) return;
        if (presetsData.Presets.Count <= 1)
        {
            MessageBox.Show("You must have at least one preset.", "Cannot Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var result = MessageBox.Show($"Delete preset '{name}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result != DialogResult.Yes) return;

        presetsData.Presets.Remove(name);
        presetsData.LastPreset = presetsData.Presets.Keys.First();
        SavePresetsFile();
        RefreshPresetComboBox(presetsData.LastPreset);
    }

    private void ResetPreset_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show("Reset current settings to default?", "Confirm Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            ApplyPresetToUI(new PresetSettings());
        }
    }

    private void ApplyPresetToUI(PresetSettings p)
    {
        edit_W.Text = p.LeftUp;      edit_A.Text = p.LeftLeft;
        edit_S.Text = p.LeftDown;    edit_D.Text = p.LeftRight;
        edit_RUp.Text = p.RightUp;   edit_RDown.Text = p.RightDown;
        edit_RLeft.Text = p.RightLeft; edit_RRight.Text = p.RightRight;
        edit_Up.Text = p.DPadUp;     edit_Down.Text = p.DPadDown;
        edit_Left.Text = p.DPadLeft; edit_Right.Text = p.DPadRight;
        edit_A_Btn.Text = p.BtnA;    edit_B_Btn.Text = p.BtnB;
        edit_X_Btn.Text = p.BtnX;    edit_Y_Btn.Text = p.BtnY;
        edit_L3.Text = p.L3;         edit_R3.Text = p.R3;
        edit_LB.Text = p.LB;         edit_RB.Text = p.RB;
        edit_LT.Text = p.LT;         edit_RT.Text = p.RT;
        edit_Thresh.Text = p.MouseThreshold;
        edit_Mult.Text = p.MouseMultiplier;

        radMouseNone.Checked = p.MouseSource == "None";
        radMouseLeft.Checked = p.MouseSource == "LeftStick";
        radMouseRight.Checked = p.MouseSource == "RightStick";
        radMouseDPad.Checked = p.MouseSource == "DPad";
        if (!radMouseNone.Checked && !radMouseLeft.Checked && !radMouseRight.Checked && !radMouseDPad.Checked)
            radMouseRight.Checked = true;

        chk_DarkMode.Checked = p.DarkMode;
    }

    private PresetSettings GetPresetFromUI() => new PresetSettings
    {
        LeftUp = edit_W.Text,      LeftLeft = edit_A.Text,
        LeftDown = edit_S.Text,    LeftRight = edit_D.Text,
        RightUp = edit_RUp.Text,   RightDown = edit_RDown.Text,
        RightLeft = edit_RLeft.Text, RightRight = edit_RRight.Text,
        DPadUp = edit_Up.Text,     DPadDown = edit_Down.Text,
        DPadLeft = edit_Left.Text, DPadRight = edit_Right.Text,
        BtnA = edit_A_Btn.Text,    BtnB = edit_B_Btn.Text,
        BtnX = edit_X_Btn.Text,    BtnY = edit_Y_Btn.Text,
        L3 = edit_L3.Text,         R3 = edit_R3.Text,
        LB = edit_LB.Text,         RB = edit_RB.Text,
        LT = edit_LT.Text,         RT = edit_RT.Text,
        MouseThreshold = edit_Thresh.Text,
        MouseMultiplier = edit_Mult.Text,
        MouseSource = radMouseLeft.Checked ? "LeftStick"
                    : radMouseDPad.Checked ? "DPad"
                    : radMouseNone.Checked ? "None"
                    : "RightStick",
        DarkMode = chk_DarkMode.Checked
    };

    private static string? PromptInput(string title, string prompt, string defaultValue = "")
    {
        Form dlg = new Form
        {
            Text = title, Size = new Size(340, 145),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false, MinimizeBox = false, ShowIcon = false
        };
        new Label { Text = prompt, Bounds = new Rectangle(12, 12, 300, 20), Parent = dlg };
        var tb = new TextBox { Text = defaultValue, Bounds = new Rectangle(12, 36, 300, 24), Parent = dlg };
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Bounds = new Rectangle(145, 75, 80, 26), Parent = dlg };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Bounds = new Rectangle(232, 75, 80, 26), Parent = dlg };
        dlg.AcceptButton = ok; dlg.CancelButton = cancel;
        return dlg.ShowDialog() == DialogResult.OK ? tb.Text.Trim() : null;
    }

    // ── UI Helpers ────────────────────────────────────────────────────────────
    private void UpdateTextBoxStates(object? sender, EventArgs? e)
    {
        bool isLeftMouse = radMouseLeft.Checked;
        edit_W.Enabled = !isLeftMouse; edit_A.Enabled = !isLeftMouse;
        edit_S.Enabled = !isLeftMouse; edit_D.Enabled = !isLeftMouse;

        bool isRightMouse = radMouseRight.Checked;
        edit_RUp.Enabled = !isRightMouse; edit_RDown.Enabled = !isRightMouse;
        edit_RLeft.Enabled = !isRightMouse; edit_RRight.Enabled = !isRightMouse;

        bool isDPadMouse = radMouseDPad.Checked;
        edit_Up.Enabled = !isDPadMouse; edit_Down.Enabled = !isDPadMouse;
        edit_Left.Enabled = !isDPadMouse; edit_Right.Enabled = !isDPadMouse;
    }

    private void CreateLabelTextBox(Control parent, string labelText, string defaultText, int yPos, out TextBox tb, int labelWidth = 35, int tbWidth = 80)
    {
        new Label { Text = labelText, Bounds = new Rectangle(10, yPos + 3, labelWidth, 20), Parent = parent };
        tb = new TextBox { Text = defaultText, Bounds = new Rectangle(10 + labelWidth + 5, yPos, tbWidth, 20), Parent = parent };
    }

    // ── Dark Mode ─────────────────────────────────────────────────────────────
    private void ToggleDarkMode(object? sender, EventArgs? e)
    {
        bool isDark = chk_DarkMode.Checked;
        Color bg    = isDark ? Color.FromArgb(45, 45, 48) : SystemColors.Control;
        Color fg    = isDark ? Color.White : SystemColors.ControlText;
        Color boxBg = isDark ? Color.FromArgb(30, 30, 30) : SystemColors.Window;
        this.BackColor = bg; this.ForeColor = fg;
        foreach (Control c in this.Controls)
            UpdateControlColors(c, bg, fg, boxBg, isDark);
    }

    private void UpdateControlColors(Control c, Color bg, Color fg, Color boxBg, bool isDark)
    {
        if (c is GroupBox || c is CheckBox || c is RadioButton || c is Label)
            c.ForeColor = fg;
        else if (c is TextBox tb) { tb.BackColor = boxBg; tb.ForeColor = fg; tb.BorderStyle = isDark ? BorderStyle.FixedSingle : BorderStyle.Fixed3D; }
        else if (c is Button btn) { btn.BackColor = isDark ? Color.FromArgb(60, 60, 60) : SystemColors.Control; btn.ForeColor = fg; btn.FlatStyle = isDark ? FlatStyle.Flat : FlatStyle.Standard; if (isDark) btn.FlatAppearance.BorderColor = Color.Gray; }
        else if (c is ComboBox cbo) { cbo.BackColor = boxBg; cbo.ForeColor = fg; }
        foreach (Control child in c.Controls)
            UpdateControlColors(child, bg, fg, boxBg, isDark);
    }

    // ── Help Dialog ───────────────────────────────────────────────────────────
    private void ShowHelp()
    {
        Form helpForm = new Form
        {
            Text = "Help - Valid Keys", Size = new Size(450, 600),
            StartPosition = FormStartPosition.CenterParent, ShowIcon = false,
            FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false
        };
        TreeView tv = new TreeView
        {
            Dock = DockStyle.Fill, Parent = helpForm,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point),
            ShowLines = false, FullRowSelect = true, ItemHeight = 22
        };

        void AddCategory(string name, (string, string)[] items)
        {
            var node = tv.Nodes.Add(name);
            node.NodeFont = new Font(tv.Font, FontStyle.Bold);
            foreach (var (label, code) in items) node.Nodes.Add($"{label}  \u2794  {code}");
        }

        AddCategory("Standard Letters & Numbers", new[] { ("Letters (Lowercase)", "a through z"), ("Numbers", "0 through 9") });
        AddCategory("Modifier & System Keys", new[] { ("Spacebar","space"),("Left/Right Shift","lshift / rshift"),("Left/Right Control","lctrl / rctrl"),("Left/Right Alt","lalt / ralt"),("Left/Right Windows","lwin / rwin"),("Enter","enter"),("Escape","esc"),("Tab","tab"),("Backspace","backspace"),("Caps Lock","capslock"),("Print Screen","printscreen"),("Scroll Lock","scrolllock"),("Pause/Break","pause") });
        AddCategory("Navigation & Editing", new[] { ("Arrows","up, down, left, right"),("Insert / Delete","insert / delete"),("Home / End","home / end"),("Page Up / Down","pgup / pgdn") });
        AddCategory("Function Keys", new[] { ("F1 to F24","f1, f2, f3 ... f24") });
        AddCategory("Numeric Keypad (Numpad)", new[] { ("Numbers","numpad0 through numpad9"),("Period","numpaddot"),("Enter","numpadenter"),("Plus / Minus","numpadadd / numpadsub"),("Multiply / Divide","numpadmult / numpaddiv"),("Num Lock","numlock"),("Clear","numpadclear") });
        AddCategory("Mouse Controls", new[] { ("Left / Right Click","lbutton / rbutton"),("Middle Click","mbutton"),("Side Buttons","xbutton1 / xbutton2"),("Scroll Wheel","wheelup, wheeldown"),("Scroll Tilt","wheelleft, wheelright") });
        AddCategory("Punctuation & Symbols", new (string, string)[] { ("Semicolon (;)",";"),("Comma (,)",","),("Period (.)","."),("Forward Slash (/)","/"),("Backslash (\\)","\\"),("Minus (-)","-"),("Equal (=)","="),("Open Bracket ([)","["),("Close Bracket (])","]"),("Apostrophe (')","'"),("Backtick / Tilde (`)","` (next to 1)") });

        helpForm.ShowDialog();
    }

    // ── Apply Settings ────────────────────────────────────────────────────────
    private void ApplySettings()
    {
        var namedBoxes = new Dictionary<string, TextBox>
        {
            {"Left Stick Up",edit_W},{"Left Stick Left",edit_A},{"Left Stick Down",edit_S},{"Left Stick Right",edit_D},
            {"Right Stick Up",edit_RUp},{"Right Stick Down",edit_RDown},{"Right Stick Left",edit_RLeft},{"Right Stick Right",edit_RRight},
            {"D-Pad Up",edit_Up},{"D-Pad Down",edit_Down},{"D-Pad Left",edit_Left},{"D-Pad Right",edit_Right},
            {"Face A",edit_A_Btn},{"Face B",edit_B_Btn},{"Face X",edit_X_Btn},{"Face Y",edit_Y_Btn},
            {"LB",edit_LB},{"RB",edit_RB},{"L3",edit_L3},{"R3",edit_R3},{"LT",edit_LT},{"RT",edit_RT}
        };

        var keyToControls = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in namedBoxes)
        {
            if (!kvp.Value.Enabled || string.IsNullOrWhiteSpace(kvp.Value.Text)) continue;
            string key = kvp.Value.Text.Trim();
            if (!keyToControls.ContainsKey(key)) keyToControls[key] = new List<string>();
            keyToControls[key].Add(kvp.Key);
        }

        var conflicts = new List<string>();
        foreach (var kvp in keyToControls)
            if (kvp.Value.Count > 1)
                conflicts.Add($"  \"{kvp.Key}\"  \u2192  {string.Join(", ", kvp.Value)}");

        if (conflicts.Count > 0)
        {
            MessageBox.Show("Conflicting controls detected:\n\n" + string.Join("\n", conflicts) +
                "\n\nPlease ensure all active mappings are unique.", "Conflicts Detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            chk_Enable.Checked = false;
            return;
        }

        VirtualKeyMapper.TryMapKey(edit_W.Text, out key_W);   VirtualKeyMapper.TryMapKey(edit_A.Text, out key_A);
        VirtualKeyMapper.TryMapKey(edit_S.Text, out key_S);   VirtualKeyMapper.TryMapKey(edit_D.Text, out key_D);
        VirtualKeyMapper.TryMapKey(edit_RUp.Text, out key_RUp); VirtualKeyMapper.TryMapKey(edit_RDown.Text, out key_RDown);
        VirtualKeyMapper.TryMapKey(edit_RLeft.Text, out key_RLeft); VirtualKeyMapper.TryMapKey(edit_RRight.Text, out key_RRight);
        VirtualKeyMapper.TryMapKey(edit_Up.Text, out key_Up); VirtualKeyMapper.TryMapKey(edit_Down.Text, out key_Down);
        VirtualKeyMapper.TryMapKey(edit_Left.Text, out key_Left); VirtualKeyMapper.TryMapKey(edit_Right.Text, out key_Right);
        VirtualKeyMapper.TryMapKey(edit_A_Btn.Text, out key_A_Btn); VirtualKeyMapper.TryMapKey(edit_B_Btn.Text, out key_B_Btn);
        VirtualKeyMapper.TryMapKey(edit_X_Btn.Text, out key_X_Btn); VirtualKeyMapper.TryMapKey(edit_Y_Btn.Text, out key_Y_Btn);
        VirtualKeyMapper.TryMapKey(edit_LB.Text, out key_LB); VirtualKeyMapper.TryMapKey(edit_RB.Text, out key_RB);
        VirtualKeyMapper.TryMapKey(edit_L3.Text, out key_L3); VirtualKeyMapper.TryMapKey(edit_R3.Text, out key_R3);
        VirtualKeyMapper.TryMapKey(edit_LT.Text, out key_LT); VirtualKeyMapper.TryMapKey(edit_RT.Text, out key_RT);

        if (!int.TryParse(edit_Thresh.Text, out mouseThreshold)) mouseThreshold = 7;
        if (!double.TryParse(edit_Mult.Text, out mouseMultiplier)) mouseMultiplier = 0.4;

        currentMouseSource = radMouseLeft.Checked ? MouseSource.LeftStick
                           : radMouseDPad.Checked ? MouseSource.DPad
                           : radMouseNone.Checked ? MouseSource.None
                           : MouseSource.RightStick;

        // Auto-save current settings into the active preset
        if (cbo_Presets.SelectedItem is string presetName)
        {
            presetsData.Presets[presetName] = GetPresetFromUI();
            presetsData.LastPreset = presetName;
            SavePresetsFile();
        }

        isEnabled = true;
    }

    // ── Input Helpers ─────────────────────────────────────────────────────────
    private void PressKey(VirtualKeyCode k)
    {
        if (k == VirtualKeyCode.LBUTTON) inputSim.Mouse.LeftButtonDown();
        else if (k == VirtualKeyCode.RBUTTON) inputSim.Mouse.RightButtonDown();
        else if (k == VirtualKeyCode.MBUTTON) inputSim.Mouse.MiddleButtonDown();
        else if (k == VirtualKeyCode.XBUTTON1) inputSim.Mouse.XButtonDown(1);
        else if (k == VirtualKeyCode.XBUTTON2) inputSim.Mouse.XButtonDown(2);
        else if (k == VirtualKeyMapper.WHEEL_UP) inputSim.Mouse.VerticalScroll(1);
        else if (k == VirtualKeyMapper.WHEEL_DOWN) inputSim.Mouse.VerticalScroll(-1);
        else if (k == VirtualKeyMapper.WHEEL_LEFT) inputSim.Mouse.HorizontalScroll(-1);
        else if (k == VirtualKeyMapper.WHEEL_RIGHT) inputSim.Mouse.HorizontalScroll(1);
        else inputSim.Keyboard.KeyDown(k);
    }

    private void ReleaseKey(VirtualKeyCode k)
    {
        if (k == VirtualKeyCode.LBUTTON) inputSim.Mouse.LeftButtonUp();
        else if (k == VirtualKeyCode.RBUTTON) inputSim.Mouse.RightButtonUp();
        else if (k == VirtualKeyCode.MBUTTON) inputSim.Mouse.MiddleButtonUp();
        else if (k == VirtualKeyCode.XBUTTON1) inputSim.Mouse.XButtonUp(1);
        else if (k == VirtualKeyCode.XBUTTON2) inputSim.Mouse.XButtonUp(2);
        else if (k == VirtualKeyMapper.WHEEL_UP || k == VirtualKeyMapper.WHEEL_DOWN ||
                 k == VirtualKeyMapper.WHEEL_LEFT || k == VirtualKeyMapper.WHEEL_RIGHT) { }
        else inputSim.Keyboard.KeyUp(k);
    }

    private void ReleaseAllKeys()
    {
        void Rel(ref bool flag, VirtualKeyCode k) { if (flag) { ReleaseKey(k); flag = false; } }
        Rel(ref w_down, key_W); Rel(ref a_down, key_A); Rel(ref s_down, key_S); Rel(ref d_down, key_D);
        Rel(ref r_up_down, key_RUp); Rel(ref r_down_down, key_RDown); Rel(ref r_left_down, key_RLeft); Rel(ref r_right_down, key_RRight);
        Rel(ref up_down, key_Up); Rel(ref down_down, key_Down); Rel(ref left_down, key_Left); Rel(ref right_down, key_Right);
        Rel(ref btnA_down, key_A_Btn); Rel(ref btnB_down, key_B_Btn); Rel(ref btnX_down, key_X_Btn); Rel(ref btnY_down, key_Y_Btn);
        Rel(ref lb_down, key_LB); Rel(ref rb_down, key_RB); Rel(ref l3_down, key_L3); Rel(ref r3_down, key_R3);
        Rel(ref lt_down, key_LT); Rel(ref rt_down, key_RT);
    }

    private void ExitApp() { ReleaseAllKeys(); Application.Exit(); }

    // ── Poll Loop ─────────────────────────────────────────────────────────────
    private void LoopTimer_Tick(object? sender, EventArgs e)
    {
        if (!XInput.GetState(0, out State state)) return;

        bool currentEnableState = chk_Enable.Checked;
        if (!currentEnableState) { if (isEnabled) { ReleaseAllKeys(); isEnabled = false; } return; }
        if (!isEnabled && currentEnableState) ApplySettings();

        var gamepad = state.Gamepad;
        var btns = gamepad.Buttons;

        // ── Mouse Movement ────────────────────────────────────────────────────
        double moveX = 0, moveY = 0;
        if (currentMouseSource == MouseSource.RightStick)
        {
            double dx = (gamepad.RightThumbX / 32768.0) * 50.0;
            double dy = -(gamepad.RightThumbY / 32768.0) * 50.0;
            if (Math.Abs(dx) > mouseThreshold) moveX = dx * mouseMultiplier;
            if (Math.Abs(dy) > mouseThreshold) moveY = dy * mouseMultiplier;
        }
        else if (currentMouseSource == MouseSource.LeftStick)
        {
            double dx = (gamepad.LeftThumbX / 32768.0) * 50.0;
            double dy = -(gamepad.LeftThumbY / 32768.0) * 50.0;
            if (Math.Abs(dx) > mouseThreshold) moveX = dx * mouseMultiplier;
            if (Math.Abs(dy) > mouseThreshold) moveY = dy * mouseMultiplier;
        }
        else if (currentMouseSource == MouseSource.DPad)
        {
            if ((btns & GamepadButtons.DPadUp) != 0) moveY = -50.0 * mouseMultiplier;
            if ((btns & GamepadButtons.DPadDown) != 0) moveY = 50.0 * mouseMultiplier;
            if ((btns & GamepadButtons.DPadLeft) != 0) moveX = -50.0 * mouseMultiplier;
            if ((btns & GamepadButtons.DPadRight) != 0) moveX = 50.0 * mouseMultiplier;
        }
        if (moveX != 0 || moveY != 0)
            inputSim.Mouse.MoveMouseBy((int)Math.Round(moveX), (int)Math.Round(moveY));

        // ── Keyboard Helpers ──────────────────────────────────────────────────
        const double lo = 35, hi = 65;
        void HandleStick(double raw, ref bool neg, ref bool pos, VirtualKeyCode kNeg, VirtualKeyCode kPos)
        {
            double v = raw / 32768.0 * 50.0 + 50.0;
            if (v < lo) { if (!neg) { PressKey(kNeg); neg = true; } } else if (neg) { ReleaseKey(kNeg); neg = false; }
            if (v > hi) { if (!pos) { PressKey(kPos); pos = true; } } else if (pos) { ReleaseKey(kPos); pos = false; }
        }
        void HandleBtn(bool pressed, ref bool state2, VirtualKeyCode k)
        {
            if (pressed && !state2) { PressKey(k); state2 = true; }
            else if (!pressed && state2) { ReleaseKey(k); state2 = false; }
        }

        // ── Left Stick ────────────────────────────────────────────────────────
        if (currentMouseSource != MouseSource.LeftStick)
        {
            HandleStick(gamepad.LeftThumbY, ref w_down, ref s_down, key_W, key_S);
            HandleStick(gamepad.LeftThumbX, ref a_down, ref d_down, key_A, key_D);
        }

        // ── Right Stick ───────────────────────────────────────────────────────
        if (currentMouseSource != MouseSource.RightStick)
        {
            HandleStick(gamepad.RightThumbY, ref r_up_down, ref r_down_down, key_RUp, key_RDown);
            HandleStick(gamepad.RightThumbX, ref r_left_down, ref r_right_down, key_RLeft, key_RRight);
        }

        // ── D-Pad ─────────────────────────────────────────────────────────────
        if (currentMouseSource != MouseSource.DPad)
        {
            HandleBtn((btns & GamepadButtons.DPadUp) != 0, ref up_down, key_Up);
            HandleBtn((btns & GamepadButtons.DPadDown) != 0, ref down_down, key_Down);
            HandleBtn((btns & GamepadButtons.DPadLeft) != 0, ref left_down, key_Left);
            HandleBtn((btns & GamepadButtons.DPadRight) != 0, ref right_down, key_Right);
        }

        // ── Face Buttons ──────────────────────────────────────────────────────
        HandleBtn((btns & GamepadButtons.A) != 0, ref btnA_down, key_A_Btn);
        HandleBtn((btns & GamepadButtons.B) != 0, ref btnB_down, key_B_Btn);
        HandleBtn((btns & GamepadButtons.X) != 0, ref btnX_down, key_X_Btn);
        HandleBtn((btns & GamepadButtons.Y) != 0, ref btnY_down, key_Y_Btn);

        // ── Shoulders ─────────────────────────────────────────────────────────
        HandleBtn((btns & GamepadButtons.LeftShoulder) != 0, ref lb_down, key_LB);
        HandleBtn((btns & GamepadButtons.RightShoulder) != 0, ref rb_down, key_RB);
        HandleBtn((btns & GamepadButtons.LeftThumb) != 0, ref l3_down, key_L3);
        HandleBtn((btns & GamepadButtons.RightThumb) != 0, ref r3_down, key_R3);

        // ── Triggers ──────────────────────────────────────────────────────────
        HandleBtn(gamepad.LeftTrigger > 180, ref lt_down, key_LT);
        HandleBtn(gamepad.RightTrigger > 180, ref rt_down, key_RT);
    }
}
