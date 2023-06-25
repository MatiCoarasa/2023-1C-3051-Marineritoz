using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.Cameras
{
    public abstract class Camera
    {
        public float AspectRatio { get; set; }
        public Matrix Projection { get; set; }
        public Matrix View { get; set; }

        public Vector3 RightDirection { get; set; }
        public Vector3 UpDirection { get; set; }
        public Vector3 FrontDirection { get; set; }
        public Vector3 Position { get; set; }
        public abstract void Update(float deltaTime, Matrix followedWorld);


    }
}
