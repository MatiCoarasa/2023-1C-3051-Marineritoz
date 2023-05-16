using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Linq;
using TGC.MonoGame.TP.Camera;

namespace TGC.MonoGame.TP.Entities;

public class ShipPlayer
{
    private const string ContentFolder3D = "Models/";
    private Model Model { get; set; }
    private Effect Effect { get; set; }
    private Matrix World { get; set; }

    private IList<Texture2D> ColorTextures { get; } = new List<Texture2D>();

    private float Scale { get; } = 0.00025f;
    public Vector3 Position { get; set; }

    // Cambios del barco
    // -1, 0, 1, 2, 3, 4 
    private float CurrentVelocity { get; set; }
    private float[] Velocities { get; } = {-20f, 0f, 5f, 10f, 15f, 20f};
    private int CurrentVelocityIndex { get; set; } = 1;
    private float LastVelocityChangeTimer { get; set; }
    private float MinimumSecsBetweenVelocityChanges { get; } = .5f;
    
    private float Rotation { get; set; }
    private float RotationVelocity { get; } = 1f;

    private float Acceleration { get; } = 1f;

    public OrientedBoundingBox BoundingBox;

    // Uso el constructor como el Initialize
    public ShipPlayer()
    {
        World = Matrix.Identity;
        Position = Vector3.Zero;
        Rotation = 0f;
        
    }

    public void LoadContent(ContentManager content, Effect effect)
    {
        Effect = effect;
        Model = content.Load<Model>(ContentFolder3D + "ShipA/Ship");
        
        // Set Ship oriented bounding box
        var tempAABB = BoundingVolumesExtensions.CreateAABBFrom(Model);
        tempAABB = BoundingVolumesExtensions.Scale(tempAABB, Scale);
        BoundingBox = OrientedBoundingBox.FromAABB(tempAABB);
        BoundingBox.Center = Position;
        BoundingBox.Orientation = Matrix.CreateRotationY(Rotation);
        
        foreach (var mesh in Model.Meshes)
        {
            ColorTextures.Add(((BasicEffect)mesh.MeshParts.FirstOrDefault().Effect).Texture);
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = Effect;
            }
        }
    }
    
    public void Update(GameTime gameTime, FollowCamera followCamera)
    {
        var deltaTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
        LastVelocityChangeTimer += deltaTime;


        // Capturar Input teclado
        var keyboardState = Keyboard.GetState();
        
        var deltaRotation = ResolveShipRotation(deltaTime, keyboardState);
        BoundingBox.Rotate(Matrix.CreateRotationY(deltaRotation));

        var deltaPosition = ResolveShipMovement(deltaTime, keyboardState);
        BoundingBox.Center += deltaPosition;
            
        World = Matrix.CreateScale(Scale) * Matrix.CreateRotationY(Rotation) * Matrix.CreateTranslation(Position);
        
        followCamera.Update(gameTime, World);
    }

    private Vector3 ResolveShipMovement(float deltaTime, KeyboardState keyboardState)
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

        var prePositionChange = Position;
        Position += Matrix.CreateRotationY(Rotation).Right * deltaTime * CurrentVelocity;
        var deltaPosition = Position - prePositionChange;

        if (LastVelocityChangeTimer < MinimumSecsBetweenVelocityChanges) return deltaPosition;
        
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
    public void Draw(FollowCamera followCamera, SpriteBatch spriteBatch, SpriteFont spriteFont)
    {
        Effect.Parameters["View"].SetValue(followCamera.View);
        Effect.Parameters["Projection"].SetValue(followCamera.Projection);
        var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
        Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
        
        spriteBatch.Begin();
        spriteBatch.DrawString(spriteFont, "Speed: " + CurrentVelocity.ToString("0.00"), new Vector2(0, 20), Color.Black);
        spriteBatch.DrawString(spriteFont, "Shift: " + (CurrentVelocityIndex - 1).ToString("D") + "/" + (Velocities.Length - 2), 
            new Vector2(0, 0), Color.Black);
        spriteBatch.End();
        
        int index = 0;
        
        foreach (var mesh in Model.Meshes)
        {
            var meshPartColorTexture = ColorTextures[index];
            Effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * World);
            Effect.Parameters["ModelTexture"].SetValue(meshPartColorTexture);
            mesh.Draw();
            index++;
        }
    }
}