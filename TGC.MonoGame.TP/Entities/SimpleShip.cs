using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP.Entities
{
    public class SimpleShip
    {
        public Matrix World { get; set; }

        public SimpleShip(Matrix world)
        {
            World = world;
        }

        public void Draw(Model ModelShip, Effect EffectShip)
        {
            EffectShip.Parameters["DiffuseColor"].SetValue(Color.Red.ToVector3());
            var modelMeshesBaseTransforms = new Matrix[ModelShip.Bones.Count];
            ModelShip.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
            foreach (var mesh in ModelShip.Meshes)
            {
                var relativeTransform = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                EffectShip.Parameters["World"].SetValue(relativeTransform * World);
                mesh.Draw();
            }
        }
    }
}
