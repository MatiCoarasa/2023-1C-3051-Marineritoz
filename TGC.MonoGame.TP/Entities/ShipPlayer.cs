using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private Matrix World { get; set; }

    private IList<Texture2D> ColorTextures { get; } = new List<Texture2D>();

    private const float Scale = 0.00025f;
    private Vector3 Position { get; set; }

    // Cambios del barco
    // -1, 0, 1, 2, 3, 4 
    private float CurrentVelocity { get; set; }
    private float[] Velocities { get; } = {-10f, 0f, 5f, 10f, 15f, 20f};
    private int CurrentVelocityIndex { get; set; } = 1;
    private float LastVelocityChangeTimer { get; set; }
    private const float MinimumSecsBetweenVelocityChanges = .5f;
    
    private float Rotation { get; set; }
    private const float RotationVelocity = 1f;

    private const float Acceleration = 3f;

    private Matrix OBBWorld { get; set; }
    private OrientedBoundingBox ShipBoundingBox { get; set; }
    private bool HasCollisioned { get; set; }
    private bool IsReactingToCollision { get; set; }
    private float LastCollisionTimer { get; set; } = 0;
    
    private GearBox GearBox { get; set; }
    // Uso el constructor como el Initialize
    public ShipPlayer(TGCGame game)
    {
        World = Matrix.Identity;
        Position = Vector3.Zero;
        Rotation = 0f;
        Game = game;
        WaterPosition = new WaterPosition();
        GearBox = new GearBox();
    }

    public void LoadContent(GraphicsDevice graphicsDevice, ContentManager content, Effect effect)
    {
        Effect = effect;
        Model = content.Load<Model>(ContentFolder3D + "ShipA/Ship");
        
        // Set Ship oriented bounding box
        var tempAABB = BoundingVolumesExtensions.CreateAABBFrom(Model);
        tempAABB = BoundingVolumesExtensions.Scale(tempAABB, Scale);
        ShipBoundingBox = OrientedBoundingBox.FromAABB(tempAABB);
        ShipBoundingBox.Center = Position;
        ShipBoundingBox.Orientation = Matrix.CreateRotationY(Rotation);
        
        GearBox.LoadContent(graphicsDevice, content);
        foreach (var mesh in Model.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                ColorTextures.Add(((BasicEffect)meshPart.Effect).Texture);
                meshPart.Effect = Effect;
            }
        }
    }
    
    public Vector3 Update(GameTime gameTime, Camera followCamera)
    {
        var deltaTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
        var totalTime = Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds);
        LastVelocityChangeTimer += deltaTime;
        LastCollisionTimer += deltaTime;

        // Capturar Input teclado
        var keyboardState = Keyboard.GetState();
        
        var deltaRotation = ResolveShipRotation(deltaTime, keyboardState);
        ShipBoundingBox.Rotate(Matrix.CreateRotationY(deltaRotation));

        var deltaPosition = ResolveShipMovement(deltaTime, keyboardState);
        ShipBoundingBox.Center += deltaPosition;
        WaterPosition = Position.GetPositionInWaveNvidia(totalTime);
        World = OBBWorld = Matrix.CreateScale(Scale)
                           * Matrix.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(WaterPosition.tangent.X, WaterPosition.tangent.Y / 4, WaterPosition.tangent.Z / 2))
                           * Matrix.CreateRotationY(Rotation)
                           * Matrix.CreateTranslation(WaterPosition.position);

        followCamera.Update(gameTime, World);
        return World.Translation;
    }
    
    private Vector3 ResolveShipMovement(float deltaTime, KeyboardState keyboardState)
    {
        // Logica de rebote si hay colision
        if (HasCollisioned)
        {
            if (IsReactingToCollision)
            {
                CurrentVelocity += -Math.Abs(CurrentVelocity)/CurrentVelocity * Acceleration * deltaTime;
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
        
        var targetVelocity = Velocities[CurrentVelocityIndex];
        if (Math.Abs(CurrentVelocity - targetVelocity) < .1f)
        {
            CurrentVelocity = targetVelocity;
        }
        else if (CurrentVelocity < targetVelocity)
        {
            CurrentVelocity += Acceleration * deltaTime;
        } else if (CurrentVelocity > targetVelocity)
        {
            CurrentVelocity -= Acceleration * deltaTime;
        }

        var prePositionChange = Position;
        var waterVelocityDisplacement = Math.Min(WaterPosition.tangent.Z * 2 * CurrentVelocity, 0);
        Position += Matrix.CreateRotationY(Rotation).Right * deltaTime * (CurrentVelocity - waterVelocityDisplacement);
        var deltaPosition = Position - prePositionChange;

        if (LastVelocityChangeTimer < MinimumSecsBetweenVelocityChanges) return deltaPosition;
        
        if (keyboardState.IsKeyDown(Keys.W))
        {
            LastVelocityChangeTimer = 0f;
            
            // No permito que 'CurrentVelocityIndex' supere el indice de velocidad maxima (Velocities.Length - 1)
            CurrentVelocityIndex = Math.Min(CurrentVelocityIndex + 1, Velocities.Length - 1);
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
            Rotation += deltaTime * RotationVelocity * Math.Clamp(CurrentVelocity/3, -1f, 1f);
        }
        if (keyboardState.IsKeyDown(Keys.D))
        {
            Rotation -= deltaTime * RotationVelocity * Math.Clamp(CurrentVelocity/3, -1f, 1f);
        }

        return Rotation - preRotation;
    }

    public void Draw(Camera followCamera, SpriteBatch spriteBatch, float height)
    {
        Effect.Parameters["View"].SetValue(followCamera.View);
        Effect.Parameters["Projection"].SetValue(followCamera.Projection);

        GearBox.Draw(spriteBatch, height);
        var index = 0;
        Game.GraphicsDevice.BlendState = BlendState.Opaque;
        foreach (var mesh in Model.Meshes)
        {
            Effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * World);
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

        Game.Gizmos.DrawCube(OBBWorld * 2, Color.Red);
        
        Game.Gizmos.DrawLine(WaterPosition.position, WaterPosition.normal, Color.Green);
        Game.Gizmos.DrawLine(WaterPosition.position, WaterPosition.binormal, Color.Red);
        Game.Gizmos.DrawLine(WaterPosition.position, WaterPosition.tangent, Color.Violet);
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
    }
    
}