using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Entities.Islands;

public class Island
{ 
    private Model Model { get; set; }
    private Effect Effect { get; set; }
    private Matrix World { get; set; }
    private IList<Texture2D> Textures { get; set; }

    public Island(Model model, Matrix matrix, Effect effect, IList<Texture2D> textures)
    {
        Model = model;
        Effect = effect;
        World = matrix;
        Textures = textures;
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