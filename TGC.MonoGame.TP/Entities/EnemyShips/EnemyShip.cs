using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
    private float RotationAngle { get; set; } = 0f;

    public Obus[] _bullets;

    private Model ObusModel;
    private Effect ObusEffect;

    private float shootCooldown = 2f;

    private float timerCooldownShoot = 0;

    private float angle = 0f;
    private int currentBullet = 0;

    public EnemyShip(TGCGame game)
    {
        Game = game;
        _bullets = new Obus[10];

        for (int i = 0; i < 10; i++) _bullets[i] = new Obus(game, World.Translation);
    }

    public void LoadContent(Model model, Effect effect, Model obusModel, Effect obusEffect, Vector3 safeSpawn)
    {
        ObusModel = obusModel;
        ObusEffect = obusEffect;

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

        foreach (var bullet in _bullets)
        {
            bullet.LoadContent(Game.Content, Effect, ObusModel);
        }

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
    
    private float Lerp(float firstFloat, float secondFloat, float by)
    {
        return firstFloat * (1 - by) + secondFloat * by;
    }

    public void Update(float totalTime, float deltaTime, Camera followCamera, Vector3 shipPosition, List<BoundingSphere> islandsBoxes)
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
        if (IARayCollided)
        {
            if (IARaysCollisionsDistance[0] > IARaysCollisionsDistance[1])
            {
                translation = World.Translation + IARays[0].Direction *
                    GlobalConfig.EnemyVelocity * deltaTime;
                translation.Y = 0;

                float anguloNuevo = MathF.Atan2(IARays[0].Direction.X, IARays[0].Direction.Z);
                RotationAngle = Lerp(RotationAngle, anguloNuevo, 0.6f);
            }
            else
            {
                translation = World.Translation + IARays[2].Direction *
                    GlobalConfig.EnemyVelocity * deltaTime;
                translation.Y = 0;
                
                float anguloNuevo = MathF.Atan2(IARays[2].Direction.X, IARays[2].Direction.Z);
                RotationAngle = Lerp(RotationAngle, anguloNuevo, 0.6f);
            }
        }
        else
        {
            translation = World.Translation + Vector3.Normalize(vectorDistanceToShip) * GlobalConfig.EnemyVelocity * deltaTime;
            translation.Y = 0;
            
            float anguloNuevo = MathF.Atan2(vectorDistanceToShip.X, vectorDistanceToShip.Z);
            RotationAngle = Lerp(RotationAngle, anguloNuevo, 0.6f);
        }
        var waterPosition = translation.GetPositionInWave(totalTime);
        var previousTranslation = World.Translation;
        World = Matrix.CreateScale(0.0025f)
                * Matrix.CreateRotationY(RotationAngle)
                * Matrix.CreateWorld(Vector3.Zero, - waterPosition.binormal, waterPosition.normal)
                * Matrix.CreateTranslation(new Vector3(translation.X, waterPosition.position.Y, translation.Z));
        var movement = World.Translation - previousTranslation;
        BoundingBox = new BoundingBox(BoundingBox.Min + movement, BoundingBox.Max + movement);

        timerCooldownShoot += deltaTime;
        var distanciaBetweenEnemyAndPlay = Vector3.Distance(vectorDistanceToShip, World.Translation);

        if (distanciaBetweenEnemyAndPlay < 300f && timerCooldownShoot > shootCooldown)
        {
            Fire();
            timerCooldownShoot = 0f;
        }

        foreach (Obus oneBullet in _bullets)
        {
            oneBullet.Update(deltaTime, World.Translation, vectorDistanceToShip, MathHelper.ToRadians(Math.Abs(MathF.Sin(distanciaBetweenEnemyAndPlay))));
        }
    }

    public void RestartPosition(Vector3 safeSpawn)
    {
        var previousTranslation = World.Translation;
        World = Matrix.CreateScale(0.0025f) * Matrix.CreateTranslation(safeSpawn);
        var movement = World.Translation - previousTranslation;
        BoundingBox = new BoundingBox(BoundingBox.Min + movement, BoundingBox.Max + movement);
    }

    public void Fire()
    {

        currentBullet++;

        if (currentBullet >= _bullets.Length)
            currentBullet = 0;
        _bullets[currentBullet].Fire();

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

        foreach (Obus bullet in _bullets)
        {
            bullet.Draw(camera);
        }

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