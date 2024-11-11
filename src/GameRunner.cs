// Filename: GameRunner.cs
using System;

using System.IO; // DEBUG

using RenderEngine;

namespace GameEngine {

  /// <summary> ... </summary>
  class GameRunner : IWindowObserver {
    private Window TargetWindow; // NECESSARY? CHECK THIS.


    /// --
    public void Notify(String message){
      if(message.Equals("WINDOWCLOSE")){
        this.Running = false;
        File.AppendAllText("logfile", "close signal: "+DateTime.Now+"\n"); // DEBUG
      }


    }



    /// <summary> Continuous loop is closed when close signal received. </summary>
    private bool Running = true;
    public void Run(){
      Thread.Sleep(1000);
      this.TargetWindow.Close();
      /*
      while(this.Running){
        // DO STUFF HERE...
      }
      */
    }

    /// <summary> ... </summary>
    /// <remarks>
    /// We should probably call *some* initialisation here.
    /// --
    /// </remarks>
    public GameRunner(Window window){
      this.TargetWindow = window;
    }
  }
}
