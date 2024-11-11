// Filename: InitMain.cs
using System;
using System.Json;
using System.IO;
using System.Threading; // Calls the GameEngine loop.

// For the MonitorInfo functionality.
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;

using GameEngine;
using RenderEngine;

namespace Initialisation {

  public class InitMain {
    /// <summary> Keep track of the monitor we're on, reassign if necessary. </summary>
    /// <remarks> Needs to be called before other initialisation functions </remarks>
    private static MonitorInfo CurrentMonitorInfo = Monitors.GetPrimaryMonitor();
    private static bool SetMonitorInfo(NativeWindow nw){
      if(nw == null){ return false; }
      InitMain.CurrentMonitorInfo = Monitors.GetMonitorFromWindow(nw);
      return true;
    }

    /// <summary> Our default options from the settings.json file. </summary>
    private static NativeWindowSettings WindowSettingsDefault;
    public static NativeWindowSettings GetSettingsDefault(){
      return InitMain.WindowSettingsDefault;
    }

    /// <summary>
    /// Custom, overriding options from the settings.json file: Nullable.
    /// </summary>
    private static NativeWindowSettings? WindowSettingsCustom = null;
    public static NativeWindowSettings? GetSettingsCustom(){
      return InitMain.WindowSettingsCustom;
    }

    /// <summary> Parse a block of JSON settings. </summary>
    private static NativeWindowSettings ParseSettingsTree(JsonValue settingsTree){
      // Check for invalid input.
      if(settingsTree == null){
        throw new ArgumentException("Invalid settings argument encountered while parsing.");
      }

      // Dynamic Settings: Window Size from MonitorInfo.
      int width   = InitMain.CurrentMonitorInfo.HorizontalResolution;
      int height  = InitMain.CurrentMonitorInfo.VerticalResolution;
      // Process the enum. I know this is finicky with JSON, but it works for now.
      String wstateRaw = settingsTree["WindowState"].ToString().Replace("\"", String.Empty);
      WindowState wstate; // Non-nullable.
      if(!Enum.TryParse<WindowState>(wstateRaw, out wstate)){
        throw new FormatException("Couldn't parse WindowState enum.");
      }
      // Instantiate, return.
      NativeWindowSettings nws = new NativeWindowSettings(){
        ClientSize = (width, height),
        Title = (String)settingsTree["Title"],
        WindowState = wstate
      };
      return nws;
    }

    /// <summary> Load settings from settings.json. </summary>
    protected static void LoadOptionsFromFile(String filename){
      // Check we have a valid json filename, that can be accessed.
      if(String.IsNullOrEmpty(filename) || !filename.ToLower().EndsWith(".json")){
        throw new ArgumentException("Invalid filename argument.");
      }
      if(!File.Exists(filename)){
        throw new FileNotFoundException("Filename provided is not a file.");
      }

      // Parse.
      String jsonRaw;
      JsonValue settingsJson;
      try {
        jsonRaw = File.ReadAllText(filename);
        settingsJson = JsonValue.Parse(jsonRaw);
      } catch(Exception) {
        throw; // Pass the *specific* exception to an external catcher.
      }

      // Assign the NativeWindowSettings.
      JsonValue settingsBlock;
      if(!settingsJson.ContainsKey("default")){
        throw new ArgumentException("settings.json needs a default settings block.");
      }
      settingsBlock = settingsJson["default"];
      InitMain.WindowSettingsDefault = InitMain.ParseSettingsTree(settingsBlock);

      // Not-necessary.
      if(settingsJson.ContainsKey("custom")){
        settingsBlock = settingsJson["custom"];
        InitMain.WindowSettingsCustom = InitMain.ParseSettingsTree(settingsBlock);
      }
    }

		/// <summary>
    /// Process initial arguments, collect system information.
    /// </summary>
		public static void Main(String[] args){
      // Load settings file.
      try {
        InitMain.LoadOptionsFromFile("settings.json");
      } catch(Exception e){
        Console.WriteLine("Couldn't parse settings!");
        Console.WriteLine(e.ToString());
        return;
      }

      // OpenGL (GLFW) must be called from the Main thread.
      Window window = new Window(InitMain.GetSettingsDefault(), 60.0);
      // Pass the window reference to the GameEngine first.
      GameRunner runner = new GameRunner(window);
      window.AddObserver(runner);
      new Thread(() => {
        runner.Run();
      }).Start();
      // Run Window after setting up multithreading.
      window.Run();
		}
    // EOF InitMain.
	}
}
