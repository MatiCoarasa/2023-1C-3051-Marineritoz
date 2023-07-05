using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Numerics;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Content.Gizmos;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace TGC.MonoGame.TP.Entities
{
    public class Obus
    {
        private Effect Effect;
        private Model Model;
        private OrientedBoundingBox OBBObus;
        private TGCGame Game;

        private Matrix World;
        public Vector3 ObusPosition;

        // 0.0018f
        private float _standarScale = 0.0018f;
        private float actualObusRotation = 0f;
        private float time;
        public bool Firing = false;

        private float actualAngle;
        public Vector3 actualCannonDirection;

        float baseSpeed = 100f;
        float gravity = 9.8f;
        float actualSpeed = 0f;
        public Obus(TGCGame game, Vector3 ShipPosition) {
            World = Matrix.CreateTranslation(ShipPosition);
            ObusPosition = ShipPosition;
            Game = game;
        }

        public void Update(float elapsedTime, Vector3 ShipPosition, Vector3 cannonDirection, float angle)
        {
            if (World.Translation.Y < -20f)
            {
                Firing = false;
            }

            if (!Firing)
            {
                actualAngle = angle;
                actualCannonDirection = Vector3.Normalize(cannonDirection);
                actualObusRotation = MathF.Atan(actualCannonDirection.X / actualCannonDirection.Z);
                ObusPosition = ShipPosition;
                World = Matrix.CreateScale(_standarScale) * Matrix.CreateTranslation(ObusPosition);
                actualSpeed = baseSpeed;
            } else
            {
                //Lo hago para poder ver la bala clavaba en el bounding box de las islas y en el suelo.

                    time += elapsedTime;
                    if (actualSpeed - time > 5f)
                    {
                        actualSpeed -= time;
                    }

                    actualCannonDirection.Y = MathF.Tan(actualAngle);
                    ObusPosition += (Vector3.Normalize(actualCannonDirection) * actualSpeed * time + 0.5f * gravity * Vector3.Down * time * time) * elapsedTime;

                    OBBObus.Orientation = Matrix.CreateRotationY(actualObusRotation);

                    World = Matrix.CreateScale(_standarScale) * Matrix.CreateRotationY(actualObusRotation) * Matrix.CreateTranslation(ObusPosition);
                    OBBObus.Center = World.Translation;

            }

        }


        public void Fire()
        {
            time = 0f;
            Firing = true;
        }

        public void LoadContent(ContentManager content, Effect effect, Model model)
        {
            Effect = effect;
            Model = model;

            var tempAABB = BoundingVolumesExtensions.CreateAABBFrom(Model);
            tempAABB = BoundingVolumesExtensions.Scale(tempAABB, _standarScale);
            OBBObus = OrientedBoundingBox.FromAABB(tempAABB);
        }


        public void Draw(Camera Camera)
        {
            if (!Firing) return;

            Effect.Parameters["View"].SetValue(Camera.View);
            Effect.Parameters["Projection"].SetValue(Camera.Projection);
            Effect.Parameters["DiffuseColor"].SetValue(Color.Yellow.ToVector3());

            foreach (var mesh in Model.Meshes)
            {
                Effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * Matrix.CreateScale(5) * World);

                foreach (var meshPart in mesh.MeshParts)
                {
                    foreach (var pass in Effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                    }
                    mesh.Draw();
                }   
            }


            var ObusOBBWorld = Matrix.CreateScale(OBBObus.Extents * 2f) *
                 OBBObus.Orientation * Matrix.CreateTranslation(World.Translation);

            Game.Gizmos.DrawCube(ObusOBBWorld, Color.White);
        }

        public bool CheckCollision(BoundingBox boundingBox)
        {
            return OBBObus.Intersects(boundingBox);
        }

    }
}
