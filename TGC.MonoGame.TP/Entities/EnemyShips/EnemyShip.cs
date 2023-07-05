﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Content.Gizmos;
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
    private Ray[] IARays = new Ray[3];
    private float[] IARaysCollisionsDistance = new float[2] { float.MaxValue, float.MaxValue };

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
        IARays[0] = new Ray();
        IARays[1] = new Ray();
        IARays[2] = new Ray();
        UpdateRays(safeSpawn, -safeSpawn);
    }

    private void UpdateRays(Vector3 origen, Vector3 target)
    {
        // Izquierda
        IARays[0].Position = origen;
        IARays[0].Direction = Vector3.Cross(Vector3.Up, Vector3.Normalize(target));
        // Centro
        IARays[1].Position = origen;
        IARays[1].Direction = Vector3.Normalize(target);
        // Derecha
        IARays[2].Position = origen;
        IARays[2].Direction = Vector3.Cross(Vector3.Normalize(target), Vector3.Up);
    }
    public void Update(float totalTime, float deltaTime, Vector3 shipPosition, List<BoundingBox> islandsBoxes)
    {
        shipPosition.Y = 0;
        var vectorDistanceToShip = shipPosition - World.Translation;
        UpdateRays(World.Translation, vectorDistanceToShip);
        bool IARayCollided = false;
        foreach (var islandBox in islandsBoxes)
        {
            var centerIntersectionDistance = IARays[1].Intersects(islandBox);
            IARayCollided = IARayCollided || centerIntersectionDistance is < 15f and > 10f;
            var IARayLeftDistance = IARays[0].Intersects(islandBox) ?? float.MaxValue;
            var IARayRightDistance = IARays[2].Intersects(islandBox) ?? float.MaxValue;
            IARaysCollisionsDistance[0] = Math.Min(IARaysCollisionsDistance[0], IARayLeftDistance);
            IARaysCollisionsDistance[1] = Math.Min(IARaysCollisionsDistance[1], IARayRightDistance);
        }
        Vector3 translation;
        double angle;
        if (IARayCollided)
        {
            if (IARaysCollisionsDistance[0] > IARaysCollisionsDistance[1])
            {
                translation = World.Translation + IARays[0].Direction *
                    GlobalConfig.EnemyVelocity * deltaTime;
                translation.Y = 0;
                angle = Math.Acos(Vector3.Dot(vectorDistanceToShip, World.Left) /
                                  (vectorDistanceToShip.Length() * World.Left.Length()));
            }
            else
            {
                translation = World.Translation + IARays[2].Direction *
                    GlobalConfig.EnemyVelocity * deltaTime;
                translation.Y = 0;
                angle = Math.Acos(Vector3.Dot(vectorDistanceToShip, World.Right) /
                                  (vectorDistanceToShip.Length() * World.Right.Length()));
            }
        }
        else
        {
            translation = World.Translation + Vector3.Normalize(vectorDistanceToShip) * GlobalConfig.EnemyVelocity * deltaTime;
            translation.Y = 0;
            angle = Math.Acos(Vector3.Dot(vectorDistanceToShip, World.Forward) /
                              (vectorDistanceToShip.Length() * World.Forward.Length()));
        }
        var waterPosition = translation.GetPositionInWave(totalTime);
        var previousTranslation = World.Translation;
        World = Matrix.CreateScale(0.0025f)
                * Matrix.CreateRotationY((float) angle)
                * Matrix.CreateWorld(Vector3.Zero, - waterPosition.binormal, waterPosition.normal)
                * Matrix.CreateTranslation(new Vector3(translation.X, waterPosition.position.Y, translation.Z));
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
        // Game.Gizmos.DrawLine(IARays[0].Position,IARays[0].Position + IARays[0].Direction * 10, Color.Yellow);
        // Game.Gizmos.DrawLine(IARays[1].Position,IARays[1].Position + IARays[1].Direction * 15f, Color.Red);
        // Game.Gizmos.DrawLine(IARays[2].Position,IARays[2].Position + IARays[2].Direction * 10, Color.Green);
        Game.Gizmos.DrawLine(World.Translation, World.Translation + World.Forward * 100000, Color.Green);
    }
}