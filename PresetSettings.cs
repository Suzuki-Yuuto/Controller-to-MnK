using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ControllerToMnK;

public class PresetSettings
{
    public string LeftUp { get; set; } = "w";
    public string LeftLeft { get; set; } = "a";
    public string LeftDown { get; set; } = "s";
    public string LeftRight { get; set; } = "d";

    public string RightUp { get; set; } = "i";
    public string RightDown { get; set; } = "k";
    public string RightLeft { get; set; } = "j";
    public string RightRight { get; set; } = "l";

    public string DPadUp { get; set; } = "up";
    public string DPadDown { get; set; } = "down";
    public string DPadLeft { get; set; } = "left";
    public string DPadRight { get; set; } = "right";

    public string BtnA { get; set; } = "space";
    public string BtnB { get; set; } = "e";
    public string BtnX { get; set; } = "r";
    public string BtnY { get; set; } = "q";

    public string L3 { get; set; } = "f";
    public string R3 { get; set; } = "m";

    public string LB { get; set; } = "lshift";
    public string RB { get; set; } = "ctrl";
    public string LT { get; set; } = "lbutton";
    public string RT { get; set; } = "rbutton";

    public string MouseThreshold { get; set; } = "7";
    public string MouseMultiplier { get; set; } = "0.4";
    public string MouseSource { get; set; } = "RightStick";
    public bool DarkMode { get; set; } = true;
}

public class PresetsFile
{
    public string LastPreset { get; set; } = "Default";
    public Dictionary<string, PresetSettings> Presets { get; set; } = new();
}
