# Controller to MnK

A standalone C# WinForms application that remaps an XInput controller's buttons and joysticks to keyboard strokes and mouse movements. It is a modern, standalone native Windows application designed as a simple Controller to MnK converter with an easy-to-use dashboard UI.

## Features
- **Dynamic Mouse Routing**: Easily assign the Left Stick, Right Stick, or D-Pad to exclusively control the mouse cursor.
- **Full Keyboard & Mouse Mapping**: Map controller face buttons, shoulders, triggers, and D-Pad to practically any keyboard key, mouse click, or scroll wheel action.
- **Preset System & Auto-Save**: Save multiple named presets for different games. Your last-used settings are automatically saved and restored when the app is reopened!
- **Real-time Configuration**: Apply your new bindings seamlessly without having to restart the application.
- **Dark Mode**: Comes with a sleek Dark Mode enabled by default.
- **Built-in Key Reference**: Includes a comprehensive built-in dictionary for valid key mapping strings.

## Packages Used
- **[Vortice.XInput](https://www.nuget.org/packages/Vortice.XInput/)**: Handles reading the gamepad state with zero-latency from any XInput-compatible controller.
- **[GregsStack.InputSimulatorStandard](https://www.nuget.org/packages/GregsStack.InputSimulatorStandard/)**: Provides low-level OS injection for accurately simulating keyboard and mouse events natively in Windows.

## How to Use
1. Go to the **[Releases](../../releases)** page of this GitHub repository.
2. Download the latest `Controller to MnK.exe` file.
   - *Note: This `.exe` is completely self-contained! You do **not** need to download or install the .NET runtime to use it. Just double-click and run!*
3. Configure your desired bindings in the UI text boxes (e.g., `space`, `lbutton`, `enter`, `f1`).
   - *Tip: Click the **Help (Key Names)** button to view all valid key string names.*
4. Use the **Mouse Controller** radio buttons to select which analog stick (or D-Pad) should control your mouse. 
5. Adjust your **Dead** (Deadzone Threshold) and **Sens** (Sensitivity Multiplier) to tune the cursor speed to your liking.
6. Make sure **Enable Remapper** is checked and click **Apply Settings**!

## Limitations & Compatibility
- **XInput Controllers Only**: Because this application relies directly on the Windows XInput API, it natively works *only* with Xbox controllers (Xbox 360, Xbox One, Xbox Series X|S) and compatible third-party PC controllers.
- **PlayStation / Switch Controllers**: Sony DualShock/DualSense and Nintendo Switch Pro controllers are DirectInput devices. To use them with this remapper, you must run a wrapper application in the background (such as [DS4Windows](https://ds4-windows.com/), DualSenseX, or Steam Input) to translate their inputs into an emulated Xbox controller.
- **Windows Only**: This application utilizes WinForms and native Windows input simulation hooks (`user32.dll`). It is not compatible with macOS or Linux.
- **Permissions**: Depending on the target game or application you are trying to control, you may need to run this remapper as an Administrator for the simulated inputs to be registered by the game engine.

## Credits & Disclaimers
- The icon used for this application is not my original artwork. It belongs to and depicts the VTuber **Ninomae Ina'nis** (Hololive English) — [YouTube](https://www.youtube.com/@NinomaeInanis) | [X](https://x.com/ninomaeinanis).
