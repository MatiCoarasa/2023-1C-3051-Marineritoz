using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Cameras;

namespace TGC.MonoGame.Samples.Cameras
{
    /// <summary>
    ///     Static camera without restrictions, where each component is configured and nothing is inferred.
    /// </summary>
    public class StaticCamera : Camera
    {
        /// <summary>
        ///     Static camera looking at a particular direction, which has the up vector (0,1,0).
        /// </summary>
        /// <param name="position">The position of the camera.</param>
        /// <param name="frontDirection">The direction where the camera is pointing.</param>
        /// <param name="upDirection">The direction that is "up" from the camera's point of view.</param>
        public StaticCamera(Vector3 position, Vector3 frontDirection, Vector3 upDirection)
        {
            Position = position;
            FrontDirection = frontDirection;
            UpDirection = upDirection;
            BuildView();
            BuildProjection(1f, 1f, 1000f, MathHelper.PiOver2);
        }

        /// <summary>
        ///     Build the camera View matrix using its properties.
        /// </summary>
        public void BuildView()
        {
            View = Matrix.CreateLookAt(Position, Position + FrontDirection, UpDirection);
        }
        
        public void BuildProjection(float aspectRatio, float nearPlaneDistance, float farPlaneDistance,
            float fieldOfViewDegrees)
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(fieldOfViewDegrees, aspectRatio, nearPlaneDistance,
                farPlaneDistance);
        }

        public void SetCubemapCameraForOrientation(CubeMapFace face)
        {
            switch (face)
            {
                default:
                case CubeMapFace.PositiveX:
                    FrontDirection = -Vector3.UnitX;
                    UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.NegativeX:
                    FrontDirection = Vector3.UnitX;
                    UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.PositiveY:
                    FrontDirection = Vector3.Down;
                    UpDirection = Vector3.UnitZ;
                    break;

                case CubeMapFace.NegativeY:
                    FrontDirection = Vector3.Up;
                    UpDirection = -Vector3.UnitZ;
                    break;

                case CubeMapFace.PositiveZ:
                    FrontDirection = -Vector3.UnitZ;
                    UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.NegativeZ:
                    FrontDirection = Vector3.UnitZ;
                    UpDirection = Vector3.Down;
                    break;
            }
        }

        public override void Update(float elapsedTime, Matrix followedWorld)
        {
            Position = followedWorld.Translation;
        }
    }
}