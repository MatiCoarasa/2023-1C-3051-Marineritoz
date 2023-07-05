using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using TGC.MonoGame.TP.Cameras;

namespace TGC.MonoGame.TP.Entities.Islands;

public class Island
{ 
    private TGCGame Game { get; set; }
    private Model Model { get; set; }
    private Effect Effect { get; set; }
    private Matrix World { get; set; }
    private IList<Texture2D> Textures { get; set; }
    public BoundingBox BoundingBox;
    private GlobalConfigurationSingleton GlobalConfig => GlobalConfigurationSingleton.GetInstance();

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
    
    public void Draw( Camera camera, Vector3 lightPosition)
    {
        Effect.Parameters["View"].SetValue(camera.View);
        Effect.Parameters["Projection"].SetValue(camera.Projection);
        Effect.Parameters["lightPosition"].SetValue(lightPosition);
        Effect.Parameters["eyePosition"]?.SetValue(camera.Position);
        Effect.Parameters["ambientColor"].SetValue(GlobalConfig.IslandAmbientColor.ToVector3());
        Effect.Parameters["diffuseColor"].SetValue(GlobalConfig.IslandDiffuseColor.ToVector3());
        Effect.Parameters["specularColor"].SetValue(GlobalConfig.IslandSpecularColor.ToVector3());
        Effect.Parameters["KAmbient"].SetValue(GlobalConfig.IslandKAmbient);
        Effect.Parameters["KDiffuse"].SetValue(GlobalConfig.IslandKDiffuse);
        Effect.Parameters["KSpecular"].SetValue(GlobalConfig.IslandKSpecular);
        Effect.Parameters["shininess"].SetValue(GlobalConfig.IslandShininess);
        var index = 0;
        foreach (var mesh in Model.Meshes)
        {
            Effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * World);
            Effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Invert(Matrix.Transpose(mesh.ParentBone.Transform * World)));
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