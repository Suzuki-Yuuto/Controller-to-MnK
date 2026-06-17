using System;
using System.Drawing;
using System.Windows.Forms;
using GregsStack.InputSimulatorStandard;
using GregsStack.InputSimulatorStandard.Native;
using Vortice.XInput;

namespace ControllerRemapper;

public enum MouseSource { None, LeftStick, RightStick, DPad }

public class MainForm : Form
{
    private TextBox edit_W, edit_A, edit_S, edit_D;
    private TextBox edit_RUp, edit_RDown, edit_RLeft, edit_RRight;
    private TextBox edit_Up, edit_Down, edit_Left, edit_Right;
    private TextBox edit_A_Btn, edit_B_Btn, edit_X_Btn, edit_Y_Btn;
    private TextBox edit_L3, edit_R3;
    private TextBox edit_LB, edit_RB, edit_LT, edit_RT;
    private TextBox edit_Thresh, edit_Mult;
    
    private RadioButton radMouseNone, radMouseLeft, radMouseRight, radMouseDPad;
    private CheckBox chk_Enable;
    private Button btn_Apply, btn_Exit, btn_Help;

    private System.Windows.Forms.Timer loopTimer;
    private InputSimulator inputSim;

    // State Variables
    private bool isEnabled = false;
    private MouseSource currentMouseSource = MouseSource.RightStick;
    
    private VirtualKeyCode key_W, key_A, key_S, key_D;
    private VirtualKeyCode key_RUp, key_RDown, key_RLeft, key_RRight;
    private VirtualKeyCode key_Up, key_Down, key_Left, key_Right;
    private VirtualKeyCode key_A_Btn, key_B_Btn, key_X_Btn, key_Y_Btn;
    private VirtualKeyCode key_LB, key_RB, key_LT, key_RT;
    private VirtualKeyCode key_L3, key_R3;

    // Mouse settings
    private int mouseThreshold = 7;
    private double mouseMultiplier = 0.4;

    // Key tracking states
    private bool w_down, a_down, s_down, d_down;
    private bool r_up_down, r_down_down, r_left_down, r_right_down;
    private bool up_down, down_down, left_down, right_down;
    private bool btnA_down, btnB_down, btnX_down, btnY_down;
    private bool lb_down, rb_down, lt_down, rt_down;
    private bool l3_down, r3_down;

    public MainForm()
    {
        inputSim = new InputSimulator();
        InitializeUI();
        ApplySettings();

        loopTimer = new System.Windows.Forms.Timer();
        loopTimer.Interval = 10;
        loopTimer.Tick += LoopTimer_Tick;
        loopTimer.Start();
    }

    private void InitializeUI()
    {
        this.Text = "Controller Remapper Dashboard";
        this.Size = new Size(560, 520);
        this.MaximizeBox = false;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        this.FormClosed += (s, e) => ExitApp();

        // --- COLUMN 1: Left Stick & D-Pad ---
        GroupBox gbMove = new GroupBox { Text = "Left Stick (Keyboard)", Bounds = new Rectangle(15, 10, 150, 140), Parent = this };
        CreateLabelTextBox(gbMove, "Up:", "w", 25, out edit_W);
        CreateLabelTextBox(gbMove, "Left:", "a", 55, out edit_A);
        CreateLabelTextBox(gbMove, "Down:", "s", 85, out edit_S);
        CreateLabelTextBox(gbMove, "Right:", "d", 115, out edit_D);

        GroupBox gbDPad = new GroupBox { Text = "D-Pad (Keyboard)", Bounds = new Rectangle(15, 160, 150, 140), Parent = this };
        CreateLabelTextBox(gbDPad, "Up:", "up", 25, out edit_Up);
        CreateLabelTextBox(gbDPad, "Down:", "down", 55, out edit_Down);
        CreateLabelTextBox(gbDPad, "Left:", "left", 85, out edit_Left);
        CreateLabelTextBox(gbDPad, "Right:", "right", 115, out edit_Right);

        // --- COLUMN 2: Face Buttons & Stick Clicks ---
        GroupBox gbFace = new GroupBox { Text = "Face Buttons", Bounds = new Rectangle(180, 10, 150, 140), Parent = this };
        CreateLabelTextBox(gbFace, "A:", "space", 25, out edit_A_Btn);
        CreateLabelTextBox(gbFace, "B:", "e", 55, out edit_B_Btn);
        CreateLabelTextBox(gbFace, "X:", "r", 85, out edit_X_Btn);
        CreateLabelTextBox(gbFace, "Y:", "q", 115, out edit_Y_Btn);

        GroupBox gbClicks = new GroupBox { Text = "Stick Clicks", Bounds = new Rectangle(180, 160, 150, 90), Parent = this };
        CreateLabelTextBox(gbClicks, "L3:", "f", 25, out edit_L3);
        CreateLabelTextBox(gbClicks, "R3:", "m", 55, out edit_R3);

        // --- COLUMN 3: Shoulders, Triggers & Mouse Config ---
        GroupBox gbShoulders = new GroupBox { Text = "Shoulders & Triggers", Bounds = new Rectangle(345, 10, 180, 140), Parent = this };
        CreateLabelTextBox(gbShoulders, "LB:", "lshift", 25, out edit_LB, 30, 100);
        CreateLabelTextBox(gbShoulders, "RB:", "ctrl", 55, out edit_RB, 30, 100);
        CreateLabelTextBox(gbShoulders, "LT:", "lbutton", 85, out edit_LT, 30, 100);
        CreateLabelTextBox(gbShoulders, "RT:", "rbutton", 115, out edit_RT, 30, 100);

        GroupBox gbMouseOpts = new GroupBox { Text = "Mouse Settings", Bounds = new Rectangle(345, 160, 180, 90), Parent = this };
        CreateLabelTextBox(gbMouseOpts, "Dead:", "7", 25, out edit_Thresh, 40, 100);
        CreateLabelTextBox(gbMouseOpts, "Sens:", "0.4", 55, out edit_Mult, 40, 100);

        // --- NEW ROW: Right Stick & Mouse Source ---
        GroupBox gbRightStick = new GroupBox { Text = "Right Stick (Keyboard)", Bounds = new Rectangle(15, 310, 150, 140), Parent = this };
        CreateLabelTextBox(gbRightStick, "Up:", "i", 25, out edit_RUp);
        CreateLabelTextBox(gbRightStick, "Down:", "k", 55, out edit_RDown);
        CreateLabelTextBox(gbRightStick, "Left:", "j", 85, out edit_RLeft);
        CreateLabelTextBox(gbRightStick, "Right:", "l", 115, out edit_RRight);

        GroupBox gbMouseSource = new GroupBox { Text = "Mouse Controller", Bounds = new Rectangle(180, 310, 150, 140), Parent = this };
        radMouseNone = new RadioButton { Text = "None", Bounds = new Rectangle(15, 25, 120, 20), Parent = gbMouseSource };
        radMouseLeft = new RadioButton { Text = "Left Stick", Bounds = new Rectangle(15, 50, 120, 20), Parent = gbMouseSource };
        radMouseRight = new RadioButton { Text = "Right Stick", Bounds = new Rectangle(15, 75, 120, 20), Checked = true, Parent = gbMouseSource };
        radMouseDPad = new RadioButton { Text = "D-Pad", Bounds = new Rectangle(15, 100, 120, 20), Parent = gbMouseSource };

        radMouseNone.CheckedChanged += UpdateTextBoxStates;
        radMouseLeft.CheckedChanged += UpdateTextBoxStates;
        radMouseRight.CheckedChanged += UpdateTextBoxStates;
        radMouseDPad.CheckedChanged += UpdateTextBoxStates;

        // --- LOWER CONTROL PANEL ---
        btn_Apply = new Button { Text = "Apply Settings", Bounds = new Rectangle(345, 310, 180, 30), Parent = this };
        btn_Apply.Click += (s, e) => ApplySettings();

        chk_Enable = new CheckBox { Text = "Enable Remapper", Bounds = new Rectangle(345, 350, 180, 20), Checked = true, Parent = this };

        btn_Help = new Button { Text = "Help (Key Names)", Bounds = new Rectangle(345, 385, 180, 30), Parent = this };
        btn_Help.Click += (s, e) => ShowHelp();

        btn_Exit = new Button { Text = "Exit Script", Bounds = new Rectangle(345, 420, 180, 30), Parent = this };
        btn_Exit.Click += (s, e) => ExitApp();

        UpdateTextBoxStates(null, null);
    }

    private void UpdateTextBoxStates(object? sender, EventArgs? e)
    {
        bool isLeftMouse = radMouseLeft.Checked;
        edit_W.Enabled = !isLeftMouse;
        edit_A.Enabled = !isLeftMouse;
        edit_S.Enabled = !isLeftMouse;
        edit_D.Enabled = !isLeftMouse;

        bool isRightMouse = radMouseRight.Checked;
        edit_RUp.Enabled = !isRightMouse;
        edit_RDown.Enabled = !isRightMouse;
        edit_RLeft.Enabled = !isRightMouse;
        edit_RRight.Enabled = !isRightMouse;

        bool isDPadMouse = radMouseDPad.Checked;
        edit_Up.Enabled = !isDPadMouse;
        edit_Down.Enabled = !isDPadMouse;
        edit_Left.Enabled = !isDPadMouse;
        edit_Right.Enabled = !isDPadMouse;
    }

    private void CreateLabelTextBox(Control parent, string labelText, string defaultText, int yPos, out TextBox tb, int labelWidth = 35, int tbWidth = 80)
    {
        new Label { Text = labelText, Bounds = new Rectangle(10, yPos + 3, labelWidth, 20), Parent = parent };
        tb = new TextBox { Text = defaultText, Bounds = new Rectangle(10 + labelWidth + 5, yPos, tbWidth, 20), Parent = parent };
    }

    private void ShowHelp()
    {
        Form helpForm = new Form
        {
            Text = "Help - Valid Keys",
            Size = new Size(450, 600),
            StartPosition = FormStartPosition.CenterParent,
            ShowIcon = false,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        ListView lv = new ListView
        {
            View = View.Details,
            Dock = DockStyle.Fill,
            FullRowSelect = true,
            HeaderStyle = ColumnHeaderStyle.None,
            Parent = helpForm,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point)
        };

        lv.Columns.Add("Action", 200);
        lv.Columns.Add("Key Code", 200);

        void AddGroup(string groupName, (string, string)[] items)
        {
            ListViewGroup group = new ListViewGroup(groupName);
            lv.Groups.Add(group);
            foreach (var item in items)
            {
                ListViewItem lvi = new ListViewItem(item.Item1) { Group = group };
                lvi.SubItems.Add(item.Item2);
                lv.Items.Add(lvi);
            }
        }

        AddGroup("Standard Letters & Numbers", new[] {
            ("Letters (Lowercase)", "a through z"),
            ("Numbers", "0 through 9")
        });

        AddGroup("Modifier & System Keys", new[] {
            ("Spacebar", "space"),
            ("Left / Right Shift", "lshift / rshift"),
            ("Left / Right Control", "lctrl / rctrl"),
            ("Left / Right Alt", "lalt / ralt"),
            ("Left / Right Windows", "lwin / rwin"),
            ("Enter / Return", "enter"),
            ("Escape", "esc"),
            ("Tab", "tab"),
            ("Backspace", "backspace"),
            ("Caps Lock", "capslock"),
            ("Print Screen", "printscreen"),
            ("Scroll Lock", "scrolllock"),
            ("Pause / Break", "pause")
        });

        AddGroup("Navigation & Editing", new[] {
            ("Arrows", "up, down, left, right"),
            ("Insert / Delete", "insert / delete"),
            ("Home / End", "home / end"),
            ("Page Up / Down", "pgup / pgdn")
        });

        AddGroup("Function Keys", new[] {
            ("F1 to F24", "f1, f2, f3 ... f24")
        });

        AddGroup("Numeric Keypad (Numpad)", new[] {
            ("Numbers", "numpad0 through numpad9"),
            ("Period / Dot", "numpaddot"),
            ("Enter", "numpadenter"),
            ("Plus / Minus", "numpadadd / numpadsub"),
            ("Multiply / Divide", "numpadmult / numpaddiv"),
            ("Num Lock", "numlock"),
            ("Clear", "numpadclear")
        });

        AddGroup("Mouse Controls", new[] {
            ("Left / Right Click", "lbutton / rbutton"),
            ("Middle Click / Wheel", "mbutton"),
            ("Side Buttons", "xbutton1 / xbutton2"),
            ("Scroll Wheel", "wheelup, wheeldown, wheelleft, wheelright")
        });

        AddGroup("Punctuation & Symbols", new[] {
            ("Semicolon (;)", ";"),
            ("Comma (,)", ","),
            ("Period (.)", "."),
            ("Forward Slash (/)", "/"),
            ("Backslash (\\)", "\\"),
            ("Minus (-)", "-"),
            ("Equal (=)", "="),
            ("Open Bracket ([)", "["),
            ("Close Bracket (])", "]"),
            ("Apostrophe (')", "'"),
            ("Backtick / Tilde (`)", "`")
        });

        helpForm.ShowDialog();
    }

    private void ApplySettings()
    {
        VirtualKeyMapper.TryMapKey(edit_W.Text, out key_W);
        VirtualKeyMapper.TryMapKey(edit_A.Text, out key_A);
        VirtualKeyMapper.TryMapKey(edit_S.Text, out key_S);
        VirtualKeyMapper.TryMapKey(edit_D.Text, out key_D);
        
        VirtualKeyMapper.TryMapKey(edit_RUp.Text, out key_RUp);
        VirtualKeyMapper.TryMapKey(edit_RDown.Text, out key_RDown);
        VirtualKeyMapper.TryMapKey(edit_RLeft.Text, out key_RLeft);
        VirtualKeyMapper.TryMapKey(edit_RRight.Text, out key_RRight);
        
        VirtualKeyMapper.TryMapKey(edit_Up.Text, out key_Up);
        VirtualKeyMapper.TryMapKey(edit_Down.Text, out key_Down);
        VirtualKeyMapper.TryMapKey(edit_Left.Text, out key_Left);
        VirtualKeyMapper.TryMapKey(edit_Right.Text, out key_Right);

        VirtualKeyMapper.TryMapKey(edit_A_Btn.Text, out key_A_Btn);
        VirtualKeyMapper.TryMapKey(edit_B_Btn.Text, out key_B_Btn);
        VirtualKeyMapper.TryMapKey(edit_X_Btn.Text, out key_X_Btn);
        VirtualKeyMapper.TryMapKey(edit_Y_Btn.Text, out key_Y_Btn);

        VirtualKeyMapper.TryMapKey(edit_LB.Text, out key_LB);
        VirtualKeyMapper.TryMapKey(edit_RB.Text, out key_RB);
        VirtualKeyMapper.TryMapKey(edit_L3.Text, out key_L3);
        VirtualKeyMapper.TryMapKey(edit_R3.Text, out key_R3);

        VirtualKeyMapper.TryMapKey(edit_LT.Text, out key_LT);
        VirtualKeyMapper.TryMapKey(edit_RT.Text, out key_RT);

        if (!int.TryParse(edit_Thresh.Text, out mouseThreshold)) mouseThreshold = 7;
        if (!double.TryParse(edit_Mult.Text, out mouseMultiplier)) mouseMultiplier = 0.4;

        if (radMouseNone.Checked) currentMouseSource = MouseSource.None;
        else if (radMouseLeft.Checked) currentMouseSource = MouseSource.LeftStick;
        else if (radMouseRight.Checked) currentMouseSource = MouseSource.RightStick;
        else if (radMouseDPad.Checked) currentMouseSource = MouseSource.DPad;

        isEnabled = true;
    }

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
        else if (k == VirtualKeyMapper.WHEEL_UP || k == VirtualKeyMapper.WHEEL_DOWN || k == VirtualKeyMapper.WHEEL_LEFT || k == VirtualKeyMapper.WHEEL_RIGHT) { /* No release needed for scroll */ }
        else inputSim.Keyboard.KeyUp(k);
    }

    private void ReleaseAllKeys()
    {
        if (w_down) { ReleaseKey(key_W); w_down = false; }
        if (a_down) { ReleaseKey(key_A); a_down = false; }
        if (s_down) { ReleaseKey(key_S); s_down = false; }
        if (d_down) { ReleaseKey(key_D); d_down = false; }
        
        if (r_up_down) { ReleaseKey(key_RUp); r_up_down = false; }
        if (r_down_down) { ReleaseKey(key_RDown); r_down_down = false; }
        if (r_left_down) { ReleaseKey(key_RLeft); r_left_down = false; }
        if (r_right_down) { ReleaseKey(key_RRight); r_right_down = false; }
        
        if (up_down) { ReleaseKey(key_Up); up_down = false; }
        if (down_down) { ReleaseKey(key_Down); down_down = false; }
        if (left_down) { ReleaseKey(key_Left); left_down = false; }
        if (right_down) { ReleaseKey(key_Right); right_down = false; }

        if (btnA_down) { ReleaseKey(key_A_Btn); btnA_down = false; }
        if (btnB_down) { ReleaseKey(key_B_Btn); btnB_down = false; }
        if (btnX_down) { ReleaseKey(key_X_Btn); btnX_down = false; }
        if (btnY_down) { ReleaseKey(key_Y_Btn); btnY_down = false; }

        if (lb_down) { ReleaseKey(key_LB); lb_down = false; }
        if (rb_down) { ReleaseKey(key_RB); rb_down = false; }
        if (l3_down) { ReleaseKey(key_L3); l3_down = false; }
        if (r3_down) { ReleaseKey(key_R3); r3_down = false; }

        if (lt_down) { ReleaseKey(key_LT); lt_down = false; }
        if (rt_down) { ReleaseKey(key_RT); rt_down = false; }
    }

    private void ExitApp()
    {
        ReleaseAllKeys();
        Application.Exit();
    }

    private void LoopTimer_Tick(object sender, EventArgs e)
    {
        if (!XInput.GetState(0, out State state))
            return;

        bool currentEnableState = chk_Enable.Checked;

        if (!currentEnableState)
        {
            if (isEnabled)
            {
                ReleaseAllKeys();
                isEnabled = false;
            }
            return;
        }

        if (!isEnabled && currentEnableState)
        {
            ApplySettings();
        }

        Gamepad gamepad = state.Gamepad;
        GamepadButtons btns = gamepad.Buttons;

        // --- MOUSE CALCULATIONS ---
        double moveX = 0, moveY = 0;
        
        if (currentMouseSource == MouseSource.RightStick)
        {
            double deltaX = (gamepad.RightThumbX / 32768.0) * 50.0;
            double deltaY = -(gamepad.RightThumbY / 32768.0) * 50.0; 
            if (Math.Abs(deltaX) > mouseThreshold) moveX = deltaX * mouseMultiplier;
            if (Math.Abs(deltaY) > mouseThreshold) moveY = deltaY * mouseMultiplier;
        }
        else if (currentMouseSource == MouseSource.LeftStick)
        {
            double deltaX = (gamepad.LeftThumbX / 32768.0) * 50.0;
            double deltaY = -(gamepad.LeftThumbY / 32768.0) * 50.0; 
            if (Math.Abs(deltaX) > mouseThreshold) moveX = deltaX * mouseMultiplier;
            if (Math.Abs(deltaY) > mouseThreshold) moveY = deltaY * mouseMultiplier;
        }
        else if (currentMouseSource == MouseSource.DPad)
        {
            bool mUp = (btns & GamepadButtons.DPadUp) != 0;
            bool mDown = (btns & GamepadButtons.DPadDown) != 0;
            bool mLeft = (btns & GamepadButtons.DPadLeft) != 0;
            bool mRight = (btns & GamepadButtons.DPadRight) != 0;
            
            // Fixed speed for DPad mouse
            if (mUp) moveY = -50.0 * mouseMultiplier;
            if (mDown) moveY = 50.0 * mouseMultiplier;
            if (mLeft) moveX = -50.0 * mouseMultiplier;
            if (mRight) moveX = 50.0 * mouseMultiplier;
        }

        if (moveX != 0 || moveY != 0)
        {
            inputSim.Mouse.MoveMouseBy((int)Math.Round(moveX), (int)Math.Round(moveY));
        }

        // --- KEYBOARD CALCULATIONS ---
        double deadzoneLow = 35;
        double deadzoneHigh = 65;

        // Left Stick
        if (currentMouseSource != MouseSource.LeftStick)
        {
            double joyX = (gamepad.LeftThumbX / 32768.0) * 50.0 + 50.0;
            double joyY = -(gamepad.LeftThumbY / 32768.0) * 50.0 + 50.0;

            if (joyY < deadzoneLow) { if (!w_down) { PressKey(key_W); w_down = true; } }
            else if (w_down) { ReleaseKey(key_W); w_down = false; }

            if (joyY > deadzoneHigh) { if (!s_down) { PressKey(key_S); s_down = true; } }
            else if (s_down) { ReleaseKey(key_S); s_down = false; }

            if (joyX < deadzoneLow) { if (!a_down) { PressKey(key_A); a_down = true; } }
            else if (a_down) { ReleaseKey(key_A); a_down = false; }

            if (joyX > deadzoneHigh) { if (!d_down) { PressKey(key_D); d_down = true; } }
            else if (d_down) { ReleaseKey(key_D); d_down = false; }
        }

        // Right Stick
        if (currentMouseSource != MouseSource.RightStick)
        {
            double rjoyX = (gamepad.RightThumbX / 32768.0) * 50.0 + 50.0;
            double rjoyY = -(gamepad.RightThumbY / 32768.0) * 50.0 + 50.0;

            if (rjoyY < deadzoneLow) { if (!r_up_down) { PressKey(key_RUp); r_up_down = true; } }
            else if (r_up_down) { ReleaseKey(key_RUp); r_up_down = false; }

            if (rjoyY > deadzoneHigh) { if (!r_down_down) { PressKey(key_RDown); r_down_down = true; } }
            else if (r_down_down) { ReleaseKey(key_RDown); r_down_down = false; }

            if (rjoyX < deadzoneLow) { if (!r_left_down) { PressKey(key_RLeft); r_left_down = true; } }
            else if (r_left_down) { ReleaseKey(key_RLeft); r_left_down = false; }

            if (rjoyX > deadzoneHigh) { if (!r_right_down) { PressKey(key_RRight); r_right_down = true; } }
            else if (r_right_down) { ReleaseKey(key_RRight); r_right_down = false; }
        }

        // D-Pad
        if (currentMouseSource != MouseSource.DPad)
        {
            bool isUp = (btns & GamepadButtons.DPadUp) != 0;
            if (isUp && !up_down) { PressKey(key_Up); up_down = true; }
            else if (!isUp && up_down) { ReleaseKey(key_Up); up_down = false; }

            bool isRight = (btns & GamepadButtons.DPadRight) != 0;
            if (isRight && !right_down) { PressKey(key_Right); right_down = true; }
            else if (!isRight && right_down) { ReleaseKey(key_Right); right_down = false; }

            bool isDown = (btns & GamepadButtons.DPadDown) != 0;
            if (isDown && !down_down) { PressKey(key_Down); down_down = true; }
            else if (!isDown && down_down) { ReleaseKey(key_Down); down_down = false; }

            bool isLeft = (btns & GamepadButtons.DPadLeft) != 0;
            if (isLeft && !left_down) { PressKey(key_Left); left_down = true; }
            else if (!isLeft && left_down) { ReleaseKey(key_Left); left_down = false; }
        }

        // --- FACE BUTTONS ---
        bool aBtn = (btns & GamepadButtons.A) != 0;
        if (aBtn && !btnA_down) { PressKey(key_A_Btn); btnA_down = true; }
        else if (!aBtn && btnA_down) { ReleaseKey(key_A_Btn); btnA_down = false; }

        bool bBtn = (btns & GamepadButtons.B) != 0;
        if (bBtn && !btnB_down) { PressKey(key_B_Btn); btnB_down = true; }
        else if (!bBtn && btnB_down) { ReleaseKey(key_B_Btn); btnB_down = false; }

        bool xBtn = (btns & GamepadButtons.X) != 0;
        if (xBtn && !btnX_down) { PressKey(key_X_Btn); btnX_down = true; }
        else if (!xBtn && btnX_down) { ReleaseKey(key_X_Btn); btnX_down = false; }

        bool yBtn = (btns & GamepadButtons.Y) != 0;
        if (yBtn && !btnY_down) { PressKey(key_Y_Btn); btnY_down = true; }
        else if (!yBtn && btnY_down) { ReleaseKey(key_Y_Btn); btnY_down = false; }

        // --- SHOULDERS ---
        bool lbBtn = (btns & GamepadButtons.LeftShoulder) != 0;
        if (lbBtn && !lb_down) { PressKey(key_LB); lb_down = true; }
        else if (!lbBtn && lb_down) { ReleaseKey(key_LB); lb_down = false; }

        bool rbBtn = (btns & GamepadButtons.RightShoulder) != 0;
        if (rbBtn && !rb_down) { PressKey(key_RB); rb_down = true; }
        else if (!rbBtn && rb_down) { ReleaseKey(key_RB); rb_down = false; }

        // --- STICK CLICKS ---
        bool l3Btn = (btns & GamepadButtons.LeftThumb) != 0;
        if (l3Btn && !l3_down) { PressKey(key_L3); l3_down = true; }
        else if (!l3Btn && l3_down) { ReleaseKey(key_L3); l3_down = false; }

        bool r3Btn = (btns & GamepadButtons.RightThumb) != 0;
        if (r3Btn && !r3_down) { PressKey(key_R3); r3_down = true; }
        else if (!r3Btn && r3_down) { ReleaseKey(key_R3); r3_down = false; }

        // --- ANALOG TRIGGERS (LT / RT) ---
        if (gamepad.LeftTrigger > 180)
        {
            if (!lt_down) { PressKey(key_LT); lt_down = true; }
        }
        else if (lt_down) { ReleaseKey(key_LT); lt_down = false; }

        if (gamepad.RightTrigger > 180)
        {
            if (!rt_down) { PressKey(key_RT); rt_down = true; }
        }
        else if (rt_down) { ReleaseKey(key_RT); rt_down = false; }
    }
}
