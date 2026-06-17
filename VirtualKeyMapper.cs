using System;
using System.Collections.Generic;
using GregsStack.InputSimulatorStandard.Native;

namespace ControllerRemapper;

public static class VirtualKeyMapper
{
    // Custom codes for Wheel
    public const VirtualKeyCode WHEEL_UP = (VirtualKeyCode)0xFF;
    public const VirtualKeyCode WHEEL_DOWN = (VirtualKeyCode)0xFE;
    public const VirtualKeyCode WHEEL_LEFT = (VirtualKeyCode)0xFD;
    public const VirtualKeyCode WHEEL_RIGHT = (VirtualKeyCode)0xFC;

    private static readonly Dictionary<string, VirtualKeyCode> keyMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // STANDARD LETTERS & NUMBERS
        { "a", VirtualKeyCode.VK_A }, { "b", VirtualKeyCode.VK_B }, { "c", VirtualKeyCode.VK_C },
        { "d", VirtualKeyCode.VK_D }, { "e", VirtualKeyCode.VK_E }, { "f", VirtualKeyCode.VK_F },
        { "g", VirtualKeyCode.VK_G }, { "h", VirtualKeyCode.VK_H }, { "i", VirtualKeyCode.VK_I },
        { "j", VirtualKeyCode.VK_J }, { "k", VirtualKeyCode.VK_K }, { "l", VirtualKeyCode.VK_L },
        { "m", VirtualKeyCode.VK_M }, { "n", VirtualKeyCode.VK_N }, { "o", VirtualKeyCode.VK_O },
        { "p", VirtualKeyCode.VK_P }, { "q", VirtualKeyCode.VK_Q }, { "r", VirtualKeyCode.VK_R },
        { "s", VirtualKeyCode.VK_S }, { "t", VirtualKeyCode.VK_T }, { "u", VirtualKeyCode.VK_U },
        { "v", VirtualKeyCode.VK_V }, { "w", VirtualKeyCode.VK_W }, { "x", VirtualKeyCode.VK_X },
        { "y", VirtualKeyCode.VK_Y }, { "z", VirtualKeyCode.VK_Z },

        { "0", VirtualKeyCode.VK_0 }, { "1", VirtualKeyCode.VK_1 }, { "2", VirtualKeyCode.VK_2 },
        { "3", VirtualKeyCode.VK_3 }, { "4", VirtualKeyCode.VK_4 }, { "5", VirtualKeyCode.VK_5 },
        { "6", VirtualKeyCode.VK_6 }, { "7", VirtualKeyCode.VK_7 }, { "8", VirtualKeyCode.VK_8 },
        { "9", VirtualKeyCode.VK_9 },

        // MODIFIER & SYSTEM KEYS
        { "space", VirtualKeyCode.SPACE },
        { "lshift", VirtualKeyCode.LSHIFT }, { "rshift", VirtualKeyCode.RSHIFT },
        { "lctrl", VirtualKeyCode.LCONTROL }, { "rctrl", VirtualKeyCode.RCONTROL },
        { "ctrl", VirtualKeyCode.CONTROL }, { "alt", VirtualKeyCode.MENU },
        { "lalt", VirtualKeyCode.LMENU }, { "ralt", VirtualKeyCode.RMENU },
        { "lwin", VirtualKeyCode.LWIN }, { "rwin", VirtualKeyCode.RWIN },
        { "enter", VirtualKeyCode.RETURN },
        { "esc", VirtualKeyCode.ESCAPE }, { "escape", VirtualKeyCode.ESCAPE },
        { "tab", VirtualKeyCode.TAB },
        { "backspace", VirtualKeyCode.BACK },
        { "capslock", VirtualKeyCode.CAPITAL },
        { "printscreen", VirtualKeyCode.SNAPSHOT },
        { "scrolllock", VirtualKeyCode.SCROLL },
        { "pause", VirtualKeyCode.PAUSE },

        // NAVIGATION & EDITING
        { "up", VirtualKeyCode.UP }, { "down", VirtualKeyCode.DOWN },
        { "left", VirtualKeyCode.LEFT }, { "right", VirtualKeyCode.RIGHT },
        { "insert", VirtualKeyCode.INSERT }, { "delete", VirtualKeyCode.DELETE },
        { "home", VirtualKeyCode.HOME }, { "end", VirtualKeyCode.END },
        { "pgup", VirtualKeyCode.PRIOR }, { "pgdn", VirtualKeyCode.NEXT },

        // FUNCTION KEYS
        { "f1", VirtualKeyCode.F1 }, { "f2", VirtualKeyCode.F2 }, { "f3", VirtualKeyCode.F3 },
        { "f4", VirtualKeyCode.F4 }, { "f5", VirtualKeyCode.F5 }, { "f6", VirtualKeyCode.F6 },
        { "f7", VirtualKeyCode.F7 }, { "f8", VirtualKeyCode.F8 }, { "f9", VirtualKeyCode.F9 },
        { "f10", VirtualKeyCode.F10 }, { "f11", VirtualKeyCode.F11 }, { "f12", VirtualKeyCode.F12 },
        { "f13", VirtualKeyCode.F13 }, { "f14", VirtualKeyCode.F14 }, { "f15", VirtualKeyCode.F15 },
        { "f16", VirtualKeyCode.F16 }, { "f17", VirtualKeyCode.F17 }, { "f18", VirtualKeyCode.F18 },
        { "f19", VirtualKeyCode.F19 }, { "f20", VirtualKeyCode.F20 }, { "f21", VirtualKeyCode.F21 },
        { "f22", VirtualKeyCode.F22 }, { "f23", VirtualKeyCode.F23 }, { "f24", VirtualKeyCode.F24 },

        // NUMERIC KEYPAD
        { "numpad0", VirtualKeyCode.NUMPAD0 }, { "numpad1", VirtualKeyCode.NUMPAD1 },
        { "numpad2", VirtualKeyCode.NUMPAD2 }, { "numpad3", VirtualKeyCode.NUMPAD3 },
        { "numpad4", VirtualKeyCode.NUMPAD4 }, { "numpad5", VirtualKeyCode.NUMPAD5 },
        { "numpad6", VirtualKeyCode.NUMPAD6 }, { "numpad7", VirtualKeyCode.NUMPAD7 },
        { "numpad8", VirtualKeyCode.NUMPAD8 }, { "numpad9", VirtualKeyCode.NUMPAD9 },
        { "numpaddot", VirtualKeyCode.DECIMAL },
        { "numpadenter", VirtualKeyCode.RETURN }, // Note: Standard Return usually triggers NumpadEnter too in games
        { "numpadadd", VirtualKeyCode.ADD },
        { "numpadsub", VirtualKeyCode.SUBTRACT },
        { "numpadmult", VirtualKeyCode.MULTIPLY },
        { "numpaddiv", VirtualKeyCode.DIVIDE },
        { "numlock", VirtualKeyCode.NUMLOCK },
        { "numpadclear", VirtualKeyCode.CLEAR },

        // MOUSE CONTROLS
        { "lbutton", VirtualKeyCode.LBUTTON },
        { "rbutton", VirtualKeyCode.RBUTTON },
        { "mbutton", VirtualKeyCode.MBUTTON },
        { "xbutton1", VirtualKeyCode.XBUTTON1 },
        { "xbutton2", VirtualKeyCode.XBUTTON2 },
        { "wheelup", WHEEL_UP },
        { "wheeldown", WHEEL_DOWN },
        { "wheelleft", WHEEL_LEFT },
        { "wheelright", WHEEL_RIGHT },

        // PUNCTUATION & SYMBOLS
        { ";", VirtualKeyCode.OEM_1 },
        { ",", VirtualKeyCode.OEM_COMMA },
        { ".", VirtualKeyCode.OEM_PERIOD },
        { "/", VirtualKeyCode.OEM_2 },
        { "\\", VirtualKeyCode.OEM_5 },
        { "-", VirtualKeyCode.OEM_MINUS },
        { "=", VirtualKeyCode.OEM_PLUS },
        { "[", VirtualKeyCode.OEM_4 },
        { "]", VirtualKeyCode.OEM_6 },
        { "'", VirtualKeyCode.OEM_7 },
        { "`", VirtualKeyCode.OEM_3 }
    };

    public static bool TryMapKey(string input, out VirtualKeyCode keyCode)
    {
        return keyMap.TryGetValue(input.Trim(), out keyCode);
    }
}
