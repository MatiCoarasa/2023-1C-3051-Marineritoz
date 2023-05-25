using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.DataClass;
using TGC.MonoGame.TP.Utils;

namespace TGC.MonoGame.TP.Entities;

public class ShipPlayer
{
    public const string ContentFolder3D = "Models/";
    private Model Model { get; set; }
    private WaterPosition WaterPosition { get; set; }
    private Effect Effect { get; set; }
    public Matrix World { get; set; }
    private float Rotation { get; set; }
    private Vector3 Position { get; set; }
    private float Velocity { get; set; } = 10f;
    private IList<Texture2D> ColorTextures { get; set; } = new List<Texture2D>();
    private float RotationVelocity { get; set; } = 1.5f;

    // Attrs para velocidad
    // reversa, neutro, 1/4 de marcha, 1/2 de marcha, full throttle 
    private float CurrentVelocity { get; set; }
    private float[] Velocities { get; } = {-20f, 0f, 5f, 10f, 15f, 20f};
    private int CurrentVelocityIndex { get; set; } = 1;
    private float LastVelocityChangeTimer { get; set; }
    private float MinimumSecsBetweenVelocityChanges { get; } = .5f;

    private GraphicsDevice GraphicsDevice;
    private float Acceleration { get; } = 1f;

    // Uso el constructor como el Initialize
    public ShipPlayer(GraphicsDevice graphicsDevice)
    {
        World = Matrix.Identity;
        Position = Vector3.Zero;
        Rotation = 0f;
        GraphicsDevice = graphicsDevice;
        WaterPosition = new WaterPosition(Vector3.Zero, Vector3.Zero, Vector3.Zero);
    }

    public void LoadContent(ContentManager content, Effect effect)
    {
        Effect = effect;
        Model = content.Load<Model>(ContentFolder3D + "ShipA/Ship");

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
        var totalTime = Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds);
        LastVelocityChangeTimer += deltaTime;


        // Capturar Input teclado
        var keyboardState = Keyboard.GetState();
        ResolveShipRotation(deltaTime, keyboardState);
        ResolveShipMovement(deltaTime, keyboardState);
        WaterPosition = Position.GetPositionInWave(totalTime);
        World = Matrix.CreateScale(0.00025f)
                * Matrix.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(WaterPosition.tangent.X, WaterPosition.tangent.Y / 4, WaterPosition.tangent.Z / 2))
                * Matrix.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(WaterPosition.binormal.X, WaterPosition.binormal.Y / 4, WaterPosition.binormal.Z / 2))
                * Matrix.CreateRotationY(Rotation)
                * Matrix.CreateTranslation(WaterPosition.position + new Vector3(0, 1, 0));
        followCamera.Update(gameTime, World);
    }

    private void ResolveShipMovement(float deltaTime, KeyboardState keyboardState)
    {
        float targetVelocity = Velocities[CurrentVelocityIndex];
        if (targetVelocity == 0f && Math.Abs(CurrentVelocity) < 0.01)
        {
            CurrentVelocity = 0;
        }
        else if (CurrentVelocity < targetVelocity)
        {
            CurrentVelocity += Acceleration * deltaTime;
        } else if (CurrentVelocity > targetVelocity)
        {
            CurrentVelocity -= Acceleration * deltaTime;
        }
        
        var waterVelocityDisplacement = Math.Min(WaterPosition.tangent.Z * 2 * CurrentVelocity, 0);
        Position += Matrix.CreateRotationY(Rotation).Right * deltaTime * (CurrentVelocity - waterVelocityDisplacement);

        if (LastVelocityChangeTimer < MinimumSecsBetweenVelocityChanges) return;
        
        if (keyboardState.IsKeyDown(Keys.W))
        {
            LastVelocityChangeTimer = 0f;
            
            // No permito que 'CurrentVelocityIndex' supere el indice de velocidad maxima (Velocities.Length - 1)
            CurrentVelocityIndex = Math.Min(CurrentVelocityIndex + 1, Velocities.Length - 1);
        } else if (keyboardState.IsKeyDown(Keys.S))
        {
            LastVelocityChangeTimer = 0f;
            
            // No permito que el Index se vaya por abajo de 0
            CurrentVelocityIndex = Math.Max(CurrentVelocityIndex - 1, 0);
        }
        
    }
        
    private void ResolveShipRotation(float deltaTime, KeyboardState keyboardState)
    {
        // Si el barco no esta en movimiento, no rota
        if (CurrentVelocity == 0f) return;

        // Si se mueve para adelante rota en un sentido. Si esta yendo para atras, rota en sentido contrario.
        if (keyboardState.IsKeyDown(Keys.A))
        {
            Rotation += deltaTime * RotationVelocity * Math.Clamp(CurrentVelocity/3, -1f, 1f);
        }
        if (keyboardState.IsKeyDown(Keys.D))
        {
            Rotation -= deltaTime * RotationVelocity * Math.Clamp(CurrentVelocity/3, -1f, 1f);
        }
    }
    public void Draw(Camera followCamera, SpriteBatch spriteBatch, SpriteFont spriteFont)
    {
        Effect.Parameters["View"].SetValue(followCamera.View);
        Effect.Parameters["Projection"].SetValue(followCamera.Projection);
        
        int index = 0;
        GraphicsDevice.BlendState = BlendState.Opaque;
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

        spriteBatch.Begin();
        spriteBatch.DrawString(spriteFont, "Speed: " + CurrentVelocity.ToString("0.00"), new Vector2(0, 20), Color.Black);
        spriteBatch.DrawString(spriteFont, "Shift: " + (CurrentVelocityIndex - 1).ToString("D") + "/" + (Velocities.Length - 2),
            new Vector2(0, 0), Color.Black);
        spriteBatch.End();
    }
}