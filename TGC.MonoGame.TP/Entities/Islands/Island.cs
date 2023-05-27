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

        World = Matrix.CreateScale(scale) * Matrix.CreateTranslation(translation);

        var tempBoundingBox = BoundingVolumesExtensions.CreateAABBFrom(Model);
        tempBoundingBox = BoundingVolumesExtensions.Scale(tempBoundingBox, scale);
        var diff = (tempBoundingBox.Max - tempBoundingBox.Min)/1.5f;
        
        BoundingBox = BoundingVolumesExtensions.FromMatrix(Matrix.CreateScale(diff.Length()) * Matrix.CreateTranslation(translation));
        
        // BoundingBox = new BoundingBox(BoundingBox.Min + translation, BoundingBox.Max + translation);
        Debug.WriteLine("Created island bounding box: " + BoundingBox.Min + " - " + BoundingBox.Max);
        Debug.WriteLine("Scale: " + scale + " - Translation: " + translation);
    }
    
    public void Draw(Matrix view, Matrix projection)
    {
        Effect.Parameters["View"].SetValue(view);
        Effect.Parameters["Projection"].SetValue(projection);
        foreach (var mesh in Model.Meshes)
        {
            Effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * World);
            for (var index = 0; index < mesh.MeshParts.Count; index++)
            {
                var meshPartColorTexture = Textures[index];
                Effect.Parameters["ModelTexture"].SetValue(meshPartColorTexture);
                mesh.Draw();
            }
        }

        Game.Gizmos.DrawCube(BoundingVolumesExtensions.GetCenter(BoundingBox), BoundingVolumesExtensions.GetExtents(BoundingBox) * 2f, Color.Red);
    }
}