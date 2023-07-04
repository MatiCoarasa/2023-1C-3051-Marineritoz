using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Entities.Islands;
using TGC.MonoGame.TP.Utils;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace TGC.MonoGame.TP.Entities;

public class EnemyShip
{
    private Model Model { get; set; }
    private Effect Effect { get; set; }
    private Matrix World { get; set; }
    public BoundingBox BoundingBox;
    private TGCGame Game { get; set; }
    private GlobalConfigurationSingleton GlobalConfig => GlobalConfigurationSingleton.GetInstance();

    public EnemyShip(TGCGame game)
    {
        Game = game;
    }

    public void LoadContent(Model model, Effect effect, Vector3 safeSpawn)
    {
        Model = model;
        Effect = effect;
        BoundingBox = BoundingVolumesExtensions.CreateAABBFrom(Model);
        BoundingBox = BoundingVolumesExtensions.ScaleCentered(BoundingBox, 0.0025f - 0.0025f/3);
        BoundingBox.Max += safeSpawn;
        BoundingBox.Min += safeSpawn;
        World = Matrix.CreateScale(0.0025f) * Matrix.CreateTranslation(safeSpawn);
    }

    public void Update(float totalTime, float deltaTime, Vector3 shipPosition)
    {
        shipPosition.Y = 0;
        var vectorDistanceToShip = shipPosition - World.Translation;
        var actualPosition = World.Translation + Vector3.Normalize(vectorDistanceToShip) * GlobalConfig.EnemyVelocity * deltaTime;
        actualPosition.Y = 0;
        var WaterPosition = actualPosition.GetPositionInWave(totalTime);
        var angle = Math.Acos(Vector3.Dot(vectorDistanceToShip, Vector3.Forward) / (vectorDistanceToShip.Length() * Vector3.Forward.Length()));
        var previousTranslation = World.Translation;
        World = Matrix.CreateScale(0.0025f) 
                * Matrix.CreateRotationY((float) angle)
                * Matrix.CreateWorld(Vector3.Zero, - WaterPosition.binormal, WaterPosition.normal)
                * Matrix.CreateTranslation(new Vector3(actualPosition.X, WaterPosition.position.Y, actualPosition.Z));
        var movement = World.Translation - previousTranslation;
        BoundingBox = new BoundingBox(BoundingBox.Min + movement, BoundingBox.Max + movement);
    }

    public void RestartPosition(Vector3 safeSpawn)
    {
        var previousTranslation = World.Translation;
        World = Matrix.CreateScale(0.0025f) * Matrix.CreateTranslation(safeSpawn);
        var movement = World.Translation - previousTranslation;
        BoundingBox = new BoundingBox(BoundingBox.Min + movement, BoundingBox.Max + movement);
    }

    public void Draw(Camera camera, Vector3 lightPosition, List<Texture2D> colorTextures)
    {
        Effect.Parameters["View"].SetValue(camera.View);
        Effect.Parameters["Projection"].SetValue(camera.Projection);
        Effect.Parameters["lightPosition"].SetValue(lightPosition);
        Effect.Parameters["eyePosition"]?.SetValue(camera.Position);
        Effect.Parameters["ambientColor"].SetValue(GlobalConfig.EnemyAmbientColor.ToVector3());
        Effect.Parameters["diffuseColor"].SetValue(GlobalConfig.EnemyDiffuseColor.ToVector3());
        Effect.Parameters["specularColor"].SetValue(GlobalConfig.EnemySpecularColor.ToVector3());
        Effect.Parameters["KAmbient"].SetValue(GlobalConfig.EnemyKAmbient);
        Effect.Parameters["KDiffuse"].SetValue(GlobalConfig.EnemyKDiffuse);
        Effect.Parameters["KSpecular"].SetValue(GlobalConfig.EnemyKSpecular);
        Effect.Parameters["shininess"].SetValue(GlobalConfig.EnemyShininess);
        Game.Gizmos.DrawCube(BoundingVolumesExtensions.GetCenter(BoundingBox), BoundingVolumesExtensions.GetExtents(BoundingBox) * 2f, Color.Yellow);
        var index = 0;
        foreach (var mesh in Model.Meshes)
        {
            Effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * World);
            Effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Invert(Matrix.Transpose(mesh.ParentBone.Transform * World)));
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                meshPart.Effect.GraphicsDevice.Indices = meshPart.IndexBuffer;
                Effect.Parameters["ModelTexture"].SetValue(colorTextures[index]);
                foreach (var pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    meshPart.Effect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshPart.VertexOffset, meshPart.StartIndex,
                        meshPart.PrimitiveCount);
                }
                mesh.Draw();
                index++;
            }
        }
    }
}