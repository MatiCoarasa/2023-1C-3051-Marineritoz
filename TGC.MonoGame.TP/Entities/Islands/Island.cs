using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace TGC.MonoGame.TP.Entities.Islands;

public class Island
{ 
    private Model Model { get; set; }
    private Effect Effect { get; set; }
    private Matrix World { get; set; }
    private IList<Texture2D> Textures { get; set; }
    public BoundingBox BoundingBox;

    public Island(Model model, Effect effect, IList<Texture2D> textures, float scale, Vector3 translation)
    {
        Model = model;
        Effect = effect;
        Textures = textures;

        World = Matrix.CreateScale(scale) * Matrix.CreateTranslation(translation);

        BoundingBox = BoundingVolumesExtensions.CreateAABBFrom(Model);
        Debug.WriteLine("Bounding box center pre scale: " + BoundingVolumesExtensions.GetCenter(BoundingBox));
        BoundingBox = BoundingVolumesExtensions.Scale(BoundingBox, scale);

        var blabla = new Vector3(translation.X, -BoundingVolumesExtensions.GetCenter(BoundingBox).Y, translation.Z);
        BoundingBox = new BoundingBox(BoundingBox.Min + blabla, BoundingBox.Max + blabla);
        Debug.WriteLine("Created island bounding box: " + BoundingBox.Min + " - " + BoundingBox.Max);
        Debug.WriteLine("Scale: " + scale + " - Translation: " + translation);
    }
    
    public void Draw(Matrix view, Matrix projection)
    {
        Effect.Parameters["View"].SetValue(view);
        Effect.Parameters["Projection"].SetValue(projection);
        int index = 0;
        foreach (var mesh in Model.Meshes)
        {
            Effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * World);
            foreach (var meshPart in mesh.MeshParts)
            {
                var meshPartColorTexture = Textures[index];
                Effect.Parameters["ModelTexture"].SetValue(meshPartColorTexture);
                mesh.Draw();
                index++;
            }
        }
    }
}