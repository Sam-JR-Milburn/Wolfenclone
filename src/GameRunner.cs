// Filename: GameRunner.cs
using System;
using System.IO; // DEBUG: Logger

using RenderEngine;
// Key-input check.
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GameEngine {

  /// <summary>
  /// GameRunner takes key/mouse input (controller) to the core game engine (model),
  /// and passes the results to the window (view).
  /// </summary>
  class GameRunner : IWindowObserver {
    // Keep RenderWindow reference to call Close(), when quit is detected.
    // Care should be taken if the Window reference is changed. Consider: resizing window.
    private RenderWindow? TargetWindow;

    /// Observe messages.
    public void Notify(String message){
      if(message.Equals("WINDOWCLOSE")){
        this.Running = false;
        File.AppendAllText("logfile", "close signal: "+DateTime.Now+"\n"); // DEBUG: Logger
      }
    }

    private void ProcessInput(){
      // SHIFT-ESC: Quit, if necessary.
      if(
        this.TargetWindow.KeyboardState.IsKeyDown(Keys.LeftShift) &&
        this.TargetWindow.KeyboardState.IsKeyDown(Keys.Escape)){
        this.TargetWindow.Close();
      }
      // --
    }

    /// <summary> Continuous loop is closed when close signal received. </summary>
    private bool Running = true;
    public void Run(){
      while(this.Running){
        // Process keyboard input.
        this.ProcessInput();
        // Pass game state back to the Window?
      }
    }

    /// <remarks>
    /// We should probably call *some* initialisation here.
    /// </remarks>
    public GameRunner(){
      this.TargetWindow = RenderWindow.GetInstance();
    }
  }
}
