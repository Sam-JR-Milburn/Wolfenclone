// Filename: Camera.cs
namespace GameEngine {

  class Camera {
    private double _locationX;
    public double GetLocationX(){ return this._locationX; }
    protected void AddLocationX(double deltaX){
      this._locationX += deltaX;
    }
    private double _locationY;
    public double GetLocationY(){ return this._locationY; }
    protected void AddLocationY(double deltaY){
      this._locationY += deltaY;
    }

    /// <remarks> If a camera must be 'teleported'. </remarks>
    public void MoveTo(double locationX, double locationY){
      this.MoveTo(locationX, locationY, 0.0);
    }
    public void MoveTo(double locationX, double locationY, double angle){
      this._locationX = locationX; this._locationY = locationY; this._angle = angle;
    }

    /// <remarks> Measured in Radians, normalises angle to [0 pi,2 pi] space. </remarks>
    private double _angle; // Horizontal, in Radians.
    public double GetAngle(){
      return this._angle % System.Math.PI;
    }
    public void AddAngle(double deltaAngle){
      this._angle += deltaAngle;
    }

    /// <summary> Insantiate a camera to some X,Y coordinates and a radians angle. </summary>
    public Camera() : this(0.0, 0.0, 0.0){}
    public Camera(double locationX, double locationY, double angle){
      this._locationX = locationX;
      this._locationY = locationY;
      this._angle = angle;
    }
  }
}
