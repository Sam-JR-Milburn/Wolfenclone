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
    private RenderWindow? _targetWindow;

    /// Observe messages.
    public void Notify(String message){
      if(message.Equals("WINDOWCLOSE")){
        this._running = false;
        File.AppendAllText("logfile", "close signal: "+DateTime.Now+"\n"); // DEBUG: Logger
      }
    }

    /// <remarks> Made this a little safer.</remarks>
    private void ProcessInput(){
      if(this._targetWindow == null){
        File.AppendAllText("logfile", "Lost reference to the RenderWindow. \n"); // DEBUG: Logger
        this.Quit(); return;
      }

      // SHIFT-ESC: Quit, if necessary.
      if(
        this._targetWindow.KeyboardState.IsKeyDown(Keys.LeftShift) &&
        this._targetWindow.KeyboardState.IsKeyDown(Keys.Escape)){
        this.Quit();
      }
      // --
    }
    /// <summary> Defines: a continuous loop that keeps the game logic thread running. </summary>
    private bool _running = true;
    public void Run(){
      while(this._running){
        // Process keyboard input.
        this.ProcessInput();
        // Pass game state back to the Window?
        // -- //
      }
    }

    /// <remarks> A little safer now. </remarks>
    protected void Quit(){
      if(this._targetWindow == null){
        this._running = false; return;
      }
      this._targetWindow.Close();
    }

    /// <remarks>
    /// We should probably call *some* initialisation here.
    /// </remarks>
    public GameRunner(){
      this._targetWindow = RenderWindow.GetInstance();
    }
  }
}
