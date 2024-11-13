// Filename: Camera.cs
namespace GameEngine {
  
  class Camera {
    private double LocationX;
    public double GetLocationX(){ return this.LocationX; }
    public void AddLocationX(double deltaX){
      this.LocationX += deltaX;
    }
    private double LocationY;
    public double GetLocationY(){ return this.LocationY; }
    public void AddLocationY(double deltaY){
      this.LocationY += deltaY;
    }

    /// <remarks> If a camera must be 'teleported'. </remarks>
    public void MoveTo(double locationX, double locationY){
      this.MoveTo(locationX, locationY, 0.0);
    }
    public void MoveTo(double locationX, double locationY, double angle){
      this.LocationX = locationX; this.LocationY = locationY; this.HorizontalAngle = angle;
    }

    /// <remarks> Measured in Radians, normalises angle to [0 pi,2 pi] space. </remarks>
    private double HorizontalAngle; // In Radians.
    public double GetAngle(){
      return this.HorizontalAngle % System.Math.PI;
    }
    public void AddAngle(double deltaAngle){
      this.HorizontalAngle += deltaAngle;
    }

    /// <summary> Insantiate a camera to some X,Y coordinates and a radians angle. </summary>
    public Camera() : this(0.0, 0.0, 0.0){}
    public Camera(double locationX, double locationY, double angle){
      this.LocationX = locationX;
      this.LocationY = locationY;
      this.HorizontalAngle = angle;
    }
  }
}
