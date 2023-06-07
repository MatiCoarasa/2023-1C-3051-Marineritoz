using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Cameras;

namespace TGC.MonoGame.TP.Entities;

public class ShipPlayer
{
    private const string ContentFolder3D = "Models/";
    private TGCGame Game { get; }
    private Model Model { get; set; }
    private Effect Effect { get; set; }
    private Matrix World { get; set; }

    private IList<Texture2D> ColorTextures { get; } = new List<Texture2D>();

    private GlobalConfigurationSingleton GlobalConfig => GlobalConfigurationSingleton.GetInstance();

    private Vector3 Position { get; set; }

    // Cambios del barco
    // -1, 0, 1, 2, 3, 4 
    private float CurrentVelocity { get; set; }
    private int CurrentVelocityIndex { get; set; } = 1;
    private float LastVelocityChangeTimer { get; set; }
    
    private float Rotation { get; set; }

    private Matrix OBBWorld { get; set; }
    private OrientedBoundingBox ShipBoundingBox { get; set; }
    private bool HasCollisioned { get; set; }
    private bool IsReactingToCollision { get; set; }

    // Uso el constructor como el Initialize
    public ShipPlayer(TGCGame game)
    {
        World = Matrix.Identity;
        Position = Vector3.Zero;
        Rotation = 0f;
        Game = game;
    }

    public void LoadContent(ContentManager content, Effect effect)
    {
        Effect = effect;
        Model = content.Load<Model>(ContentFolder3D + "ShipA/Ship");
        
        // Set Ship oriented bounding box
        var tempAABB = BoundingVolumesExtensions.CreateAABBFrom(Model);
        tempAABB = BoundingVolumesExtensions.Scale(tempAABB, GlobalConfig.PlayerScale);
        ShipBoundingBox = OrientedBoundingBox.FromAABB(tempAABB);
        ShipBoundingBox.Center = Position;
        ShipBoundingBox.Orientation = Matrix.CreateRotationY(Rotation);
        
        foreach (var mesh in Model.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                ColorTextures.Add(((BasicEffect)meshPart.Effect).Texture);
                meshPart.Effect = Effect;
            }
        }
    }
    
    public void Update(GameTime gameTime, Camera followCamera)
    {
        var deltaTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
        LastVelocityChangeTimer += deltaTime;


        // Capturar Input teclado
        var keyboardState = Keyboard.GetState();
        
        var deltaRotation = ResolveShipRotation(deltaTime, keyboardState);
        ShipBoundingBox.Rotate(Matrix.CreateRotationY(deltaRotation));

        var deltaPosition = ResolveShipMovement(deltaTime, keyboardState);
        ShipBoundingBox.Center += deltaPosition;
            
        World = OBBWorld = Matrix.CreateScale(GlobalConfig.PlayerScale) * Matrix.CreateRotationY(Rotation) * Matrix.CreateTranslation(Position);

        followCamera.Update(gameTime, World);
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
        Position += Matrix.CreateRotationY(Rotation).Right * deltaTime * CurrentVelocity;
        var deltaPosition = Position - prePositionChange;

        if (LastVelocityChangeTimer < GlobalConfig.PlayerSecsBetweenChanges) return deltaPosition;
        
        if (keyboardState.IsKeyDown(Keys.W))
        {
            LastVelocityChangeTimer = 0f;
            
            // No permito que 'CurrentVelocityIndex' supere el indice de velocidad maxima (Velocities.Length - 1)
            CurrentVelocityIndex = Math.Min(CurrentVelocityIndex + 1, GlobalConfig.PlayerVelocities.Length - 1);
        } else if (keyboardState.IsKeyDown(Keys.S))
        {
            LastVelocityChangeTimer = 0f;
            
            // No permito que el Index se vaya por abajo de 0
            CurrentVelocityIndex = Math.Max(CurrentVelocityIndex - 1, 0);
        }

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

    public void Draw(Camera followCamera, SpriteBatch spriteBatch, SpriteFont spriteFont)
    {
        Effect.Parameters["View"].SetValue(followCamera.View);
        Effect.Parameters["Projection"].SetValue(followCamera.Projection);
        
        spriteBatch.Begin();
        spriteBatch.DrawString(spriteFont, "Speed: " + CurrentVelocity.ToString("0.0"), new Vector2(0, 20), Color.White);
        spriteBatch.DrawString(spriteFont, "Shift: " + (CurrentVelocityIndex - 1).ToString("D") + "/" + (GlobalConfig.PlayerVelocities.Length - 2),
            new Vector2(0, 0), Color.White);
        spriteBatch.End();

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
    }

    public void CheckCollision(BoundingBox boundingBox)
    {
        if (CurrentVelocity == 0f) return;
        
        if (ShipBoundingBox.Intersects(boundingBox))
        {
            HasCollisioned = true;
        }
    }
    
}