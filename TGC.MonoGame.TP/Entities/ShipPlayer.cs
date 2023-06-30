using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.DataClass;
using TGC.MonoGame.TP.Utils;

namespace TGC.MonoGame.TP.Entities;

public class ShipPlayer
{
    private const string ContentFolder3D = "Models/";
    private TGCGame Game { get; }
    private Model Model { get; set; }
    private WaterPosition WaterPosition { get; set; }
    private Effect Effect { get; set; }
    public Matrix World { get; set; }

    private IList<Texture2D> ColorTextures { get; } = new List<Texture2D>();

    private GlobalConfigurationSingleton GlobalConfig => GlobalConfigurationSingleton.GetInstance();

    private Vector3 Position { get; set; }

    private float CurrentVelocity { get; set; }
    private int CurrentVelocityIndex { get; set; } = 1;
    private float LastVelocityChangeTimer { get; set; }
    
    private float Rotation { get; set; }

    private Matrix OBBWorld { get; set; }
    private OrientedBoundingBox ShipBoundingBox { get; set; }
    private bool HasCollisioned { get; set; }
    private bool IsReactingToCollision { get; set; }
    private float LastCollisionTimer { get; set; } = 0;

    private bool _reactToKeyboard;
    
    private GearBox GearBox { get; set; }

    Arsenal Arsenal { get; set; }

    // Uso el constructor como el Initialize
    public ShipPlayer(TGCGame game, bool reactToKeyboard)
    {
        World = Matrix.Identity;
        Position = Vector3.Zero;
        Rotation = 0f;
        Game = game;
        _reactToKeyboard = reactToKeyboard; 
        WaterPosition = new WaterPosition();
        GearBox = new GearBox();
        Arsenal = new Arsenal(game, 15, World.Translation);
    }

    public void LoadContent(Effect effect)
    {
        Effect = effect;
        Model = Game.Content.Load<Model>(ContentFolder3D + "ShipA/Ship");
        Arsenal.LoadContent();
        // Set Ship oriented bounding box
        var tempAABB = BoundingVolumesExtensions.CreateAABBFrom(Model);
        tempAABB = BoundingVolumesExtensions.Scale(tempAABB, GlobalConfig.PlayerScale);
        ShipBoundingBox = OrientedBoundingBox.FromAABB(tempAABB);
        ShipBoundingBox.Center = Position;
        ShipBoundingBox.Orientation = Matrix.CreateRotationY(Rotation);
        
        GearBox.LoadContent(Game.GraphicsDevice, Game.Content);
        Arsenal.LoadContent(Game.Content, Effect);

        foreach (var mesh in Model.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                
                ColorTextures.Add(((BasicEffect)meshPart.Effect).Texture);
                meshPart.Effect = Effect;
            }
        }
    }
    
    public Vector3 Update(float totalTime, float deltaTime, Camera followCamera)
    {
        LastVelocityChangeTimer += deltaTime;
        LastCollisionTimer += deltaTime;

        // Capturar Input teclado
        if (_reactToKeyboard)
        {
            var keyboardState = Keyboard.GetState();

            var deltaRotation = ResolveShipRotation(deltaTime, keyboardState);
            ShipBoundingBox.Rotate(Matrix.CreateRotationY(deltaRotation));

            var deltaPosition = ResolveShipMovement(deltaTime, keyboardState);
            ShipBoundingBox.Center += deltaPosition;
        }

        WaterPosition = Position.GetPositionInWave(totalTime);
        World = OBBWorld = Matrix.CreateScale(GlobalConfig.PlayerScale)
                           * Matrix.CreateRotationY(Rotation)
                           * Matrix.CreateWorld(Vector3.Zero, - WaterPosition.binormal, WaterPosition.normal)
                           * Matrix.CreateTranslation(WaterPosition.position);

            
        Arsenal.Update(deltaTime, Position, followCamera);

        followCamera.Update(deltaTime, World, Game.IsActive);
        return World.Translation;
    }
    
    private Vector3 ResolveShipMovement(float deltaTime, KeyboardState keyboardState)
    {
        // Logica de rebote si hay colision
        if (HasCollisioned)
        {
            if (IsReactingToCollision)
            {
                CurrentVelocity += -Math.Abs(CurrentVelocity)/CurrentVelocity * GlobalConfig.PlayerAcceleration * deltaTime;
            }
            else
            {
                IsReactingToCollision = true;
                CurrentVelocity = -CurrentVelocity / 3f;
            }

            if (Math.Abs(CurrentVelocity) < 0.1)
            {
                IsReactingToCollision = false;
                HasCollisioned = false;
            }
        }
        
        var targetVelocity = GlobalConfig.PlayerVelocities[CurrentVelocityIndex];
        if (Math.Abs(CurrentVelocity - targetVelocity) < .1f)
        {
            CurrentVelocity = targetVelocity;
        }
        else
        {
            CurrentVelocity += Math.Sign(targetVelocity - CurrentVelocity) * GlobalConfig.PlayerAcceleration * deltaTime;
        }


        var prePositionChange = Position;
        var waterVelocityDisplacement = Math.Min(WaterPosition.tangent.Z * 2 * CurrentVelocity, 0);
        Position += Matrix.CreateRotationY(Rotation).Right * deltaTime * (CurrentVelocity - waterVelocityDisplacement);
        var deltaPosition = Position - prePositionChange;

        if (LastVelocityChangeTimer < GlobalConfig.PlayerSecsBetweenChanges) return deltaPosition;
        
        if (keyboardState.IsKeyDown(Keys.W))
        {
            LastVelocityChangeTimer = 0f;
            
            // No permito que 'CurrentVelocityIndex' supere el indice de velocidad maxima (Velocities.Length - 1)
            CurrentVelocityIndex = Math.Min(CurrentVelocityIndex + 1, GlobalConfig.PlayerVelocities.Length - 1);
            GearBox.currentGearBoxOption = CurrentVelocityIndex;
        } else if (keyboardState.IsKeyDown(Keys.S))
        {
            LastVelocityChangeTimer = 0f;
            
            // No permito que el Index se vaya por abajo de 0
            CurrentVelocityIndex = Math.Max(CurrentVelocityIndex - 1, 0);
            GearBox.currentGearBoxOption = CurrentVelocityIndex;
        }

        GearBox.currentVelocity = CurrentVelocity;
        return deltaPosition;
    }
        
    private float ResolveShipRotation(float deltaTime, KeyboardState keyboardState)
    {
        // Si el barco no esta en movimiento, no rota
        if (CurrentVelocity == 0f) return 0f;
        
        // Si se mueve para adelante rota en un sentido. Si esta yendo para atras, rota en sentido contrario.
        var preRotation = Rotation;

        if (keyboardState.IsKeyDown(Keys.A))
        {
            Rotation += deltaTime * GlobalConfig.PlayerMaxRotationVelocity * Math.Clamp(CurrentVelocity/3, -1f, 1f);
        }
        if (keyboardState.IsKeyDown(Keys.D))
        {
            Rotation -= deltaTime * GlobalConfig.PlayerMaxRotationVelocity * Math.Clamp(CurrentVelocity/3, -1f, 1f);
        }

        return Rotation - preRotation;
    }

    public void Draw(Camera followCamera, SpriteBatch spriteBatch, float height, bool isNormalCamera)
    {
        Effect.Parameters["View"].SetValue(followCamera.View);
        Effect.Parameters["Projection"].SetValue(followCamera.Projection);

        var index = 0;
        Game.GraphicsDevice.BlendState = BlendState.Opaque;

        foreach (var mesh in Model.Meshes)
        {
            var world = isNormalCamera
                ? World
                : Matrix.CreateScale(GlobalConfig.PlayerScaleInEnvironment) * Matrix.CreateRotationX((float)Math.PI) * World * Matrix.CreateTranslation(GlobalConfig.PlayerTranslationInEnvironment);
            Effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * world);
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                meshPart.Effect.GraphicsDevice.Indices = meshPart.IndexBuffer;
                Effect.Parameters["ModelTexture"].SetValue(ColorTextures[index]);
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
        if (isNormalCamera) GearBox.Draw(spriteBatch, height);

        Arsenal.Draw(followCamera);

        Game.Gizmos.DrawCube(OBBWorld * 2, Color.Red);
        
        Game.Gizmos.DrawLine(World.Translation, WaterPosition.normal + World.Translation, Color.Green);
        Game.Gizmos.DrawLine(World.Translation, WaterPosition.binormal + World.Translation, Color.Red);
        Game.Gizmos.DrawLine(World.Translation, WaterPosition.tangent + World.Translation, Color.Violet);
    }

    public void CheckCollision(BoundingBox boundingBox, HealthBar healthBar)
    {
        if (CurrentVelocity == 0f) return;
        
        if (!IsReactingToCollision && ShipBoundingBox.Intersects(boundingBox))
        {
            if (LastCollisionTimer > 1f)
            {
                healthBar.Life -= 15;
                LastCollisionTimer = 0f;
            }
            HasCollisioned = true;
        }

        Arsenal.CheckCollision(boundingBox);
    }
    
}