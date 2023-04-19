using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Camera;

namespace TGC.MonoGame.TP.Entities;

public class Island
{
    private Matrix World { get; set; }

    public Island(Matrix world)
    {
        World = world;
    }
    
    public void Draw(Model ModelIsland, Effect EffectIsland)
    {
        EffectIsland.Parameters["DiffuseColor"].SetValue(Color.Yellow.ToVector3());
        var modelMeshesBaseTransforms = new Matrix[ModelIsland.Bones.Count];
        ModelIsland.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
        foreach (var mesh in ModelIsland.Meshes)
        {
            var relativeTransform = modelMeshesBaseTransforms[mesh.ParentBone.Index];
            EffectIsland.Parameters["World"].SetValue(relativeTransform * World);
            mesh.Draw();
        }
    }
}