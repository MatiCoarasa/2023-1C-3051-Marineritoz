using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace TGC.MonoGame.TP.Entities.Islands;

public class Island
{ 
    private TGCGame Game { get; set; }
    private Model Model { get; set; }
    private Effect Effect { get; set; }
    private Matrix World { get; set; }
    private IList<Texture2D> Textures { get; set; }
    public BoundingBox BoundingBox;

    public Island(TGCGame game, Model model, Effect effect, IList<Texture2D> textures, float scale, Vector3 translation)
    {
        Game = game; 
        Model = model;
        Effect = effect;
        Textures = textures;
        
        var tempBoundingBox = BoundingVolumesExtensions.CreateAABBFrom(Model);

        World = Matrix.CreateScale(scale) * Matrix.CreateTranslation(translation);

        BoundingBox = BoundingVolumesExtensions.ScaleCentered(tempBoundingBox, scale - scale/3);
        BoundingBox.Min += translation;
        BoundingBox.Max += translation;

        Debug.WriteLine("Created island bounding box: " + BoundingBox.Min + " - " + BoundingBox.Max);
        Debug.WriteLine("Scale: " + scale + " - Translation: " + translation);
    }
    
    public void Draw(Matrix view, Matrix projection)
    {
        Effect.Parameters["View"].SetValue(view);
        Effect.Parameters["Projection"].SetValue(projection);
        
        var index = 0;
        foreach (var mesh in Model.Meshes)
        {
            Effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * World);
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                meshPart.Effect.GraphicsDevice.Indices = meshPart.IndexBuffer;
                var meshPartColorTexture = Textures[index];
                Effect.Parameters["ModelTexture"].SetValue(meshPartColorTexture);
                foreach (var pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    meshPart.Effect.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshPart.VertexOffset, meshPart.StartIndex,
                        meshPart.PrimitiveCount);
                }
        
                index++;
            }
        }

        var bbCenter = BoundingVolumesExtensions.GetCenter(BoundingBox);
        var bbExtents = BoundingVolumesExtensions.GetExtents(BoundingBox);
        Game.Gizmos.DrawCube(bbCenter, bbExtents * 2f, Color.Red);
    }
}