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
    public List<BoundingSphere> IslasColliders { get; set; } = new List<BoundingSphere>();
    private GlobalConfigurationSingleton GlobalConfig => GlobalConfigurationSingleton.GetInstance();
    private float _scale;
    private int modelNumber;
    public Island(TGCGame game, Model model, Effect effect, IList<Texture2D> textures, float scale, Vector3 translation, int numeroModelo)
    {
        Game = game; 
        Model = model;
        Effect = effect;
        _scale = scale;
        Textures = textures;
        modelNumber = numeroModelo;

        var tempBoundingBox = BoundingVolumesExtensions.CreateAABBFrom(Model);

        World = Matrix.CreateScale(scale) * Matrix.CreateTranslation(translation);

        BoundingBox = BoundingVolumesExtensions.ScaleCentered(tempBoundingBox, scale - scale/3);
        BoundingBox.Min += translation;
        BoundingBox.Max += translation;

        generarBoundinBoxesBasadoEnIsla(numeroModelo);

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

        if (modelNumber == 0)
        {
            Game.Gizmos.DrawSphere(bbCenter + new Vector3(100f, 0f, 0f) * _scale, new Vector3(2400, 200, 2400) * _scale, Color.Red);
            Game.Gizmos.DrawSphere(bbCenter + new Vector3(-1000f, 0f, -1300f) * _scale, new Vector3(1000, 150, 1000) * _scale, Color.Red);
            Game.Gizmos.DrawSphere(bbCenter + new Vector3(1100f, 0f, 1400f) * _scale, new Vector3(1000, 150, 1000) * _scale, Color.Red);

        } else
        {
            if ( modelNumber == 2)
            {
                Game.Gizmos.DrawSphere(bbCenter + new Vector3(100f, 0f, 0f) * _scale, new Vector3(2400, 200, 2400) * _scale, Color.Red);
                Game.Gizmos.DrawSphere(bbCenter + new Vector3(-1000f, 0f, -1300f) * _scale, new Vector3(1000, 150, 1000) * _scale, Color.Red);
                Game.Gizmos.DrawSphere(bbCenter + new Vector3(1100f, 0f, 1400f) * _scale, new Vector3(1000, 150, 1000) * _scale, Color.Red);
            } else
            {
                Game.Gizmos.DrawSphere(bbCenter + new Vector3(-3700f, 0f, 4000f) * _scale, new Vector3(3000, 450, 3000) * _scale, Color.Red);
                Game.Gizmos.DrawSphere(bbCenter + new Vector3(-3600f, 0f, 600f) * _scale, new Vector3(3600, 1100, 3600) * _scale, Color.Red);
                Game.Gizmos.DrawSphere(bbCenter + new Vector3(-3700f, 0f, -2000f) * _scale, new Vector3(3200, 800, 3200) * _scale, Color.Red);
                Game.Gizmos.DrawSphere(bbCenter + new Vector3(-3400f, 0f, -6000f) * _scale, new Vector3(1200, 200f, 1200) * _scale, Color.Red);
                Game.Gizmos.DrawSphere(bbCenter + new Vector3(-3700f, 0f, -5000f) * _scale, new Vector3(750, 200, 750) * _scale, Color.Red);
                Game.Gizmos.DrawSphere(bbCenter + new Vector3(-6500f, 0f, -3100f) * _scale, new Vector3(1600, 200, 1600) * _scale, Color.Red);
                Game.Gizmos.DrawSphere(bbCenter + new Vector3(-6300f, 0f, 5500f) * _scale, new Vector3(1300, 250, 1300) * _scale, Color.Red);
                Game.Gizmos.DrawSphere(bbCenter + new Vector3(-2000f, 0f, 6500f) * _scale, new Vector3(750, 250, 750) * _scale, Color.Red);
                Game.Gizmos.DrawSphere(bbCenter + new Vector3(-900f, 0f, 4100f) * _scale, new Vector3(1000, 400, 1000) * _scale, Color.Red);

                Game.Gizmos.DrawSphere(bbCenter + new Vector3(1320f, 0f, -3620f) * _scale, new Vector3(150, 50, 150) * _scale, Color.Yellow);
                Game.Gizmos.DrawSphere(bbCenter + new Vector3(650f, 0f, -5000f) * _scale, new Vector3(150, 50, 150) * _scale, Color.Blue);


                Game.Gizmos.DrawSphere(bbCenter + new Vector3(3000f, 0f, -5000f) * _scale, new Vector3(600, 150, 600) * _scale, Color.Cyan);
                Game.Gizmos.DrawSphere(bbCenter + new Vector3(2500f, 0f, -5500f) * _scale, new Vector3(500, 150, 500) * _scale, Color.Cyan);
                Game.Gizmos.DrawSphere(bbCenter + new Vector3(4800f, 0f, -4000f) * _scale, new Vector3(2500, 150, 2500) * _scale, Color.Cyan);
                Game.Gizmos.DrawSphere(bbCenter + new Vector3(6700, 0f, -5000f) * _scale, new Vector3(800, 100, 800) * _scale, Color.Cyan);
                Game.Gizmos.DrawSphere(bbCenter + new Vector3(2200f, 0f, -6000f) * _scale, new Vector3(600, 100, 600) * _scale, Color.Cyan);
            }
        }
        /*Isla 1

        Game.Gizmos.DrawSphere(bbCenter + new Vector3(100f, 0f, 0f) * _scale, new Vector3(2400, 200, 2400) * _scale, Color.Red);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(-1000f, 0f, -1300f) * _scale, new Vector3(1000, 150, 1000) * _scale, Color.Red);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(1100f, 0f, 1400f) * _scale, new Vector3(1000, 150, 1000) * _scale, Color.Red);
        */


        /*Isla 2 

        Game.Gizmos.DrawSphere(bbCenter + new Vector3(-3700f, 0f, 4000f) * _scale, new Vector3(3000, 450, 3000) * _scale, Color.Red);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(-3600f, 0f, 600f) * _scale, new Vector3(3600, 1100, 3600) * _scale, Color.Red);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(-3700f, 0f, -2000f) * _scale, new Vector3(3200, 800, 3200) * _scale, Color.Red);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(-3400f, 0f, -6000f) * _scale, new Vector3(1200, 200f, 1200) * _scale, Color.Red);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(-3700f, 0f, -5000f) * _scale, new Vector3(750, 200, 750) * _scale, Color.Red);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(-6500f, 0f, -3100f) * _scale, new Vector3(1600, 200, 1600) * _scale, Color.Red);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(-6300f, 0f, 5500f) * _scale, new Vector3(1300, 250, 1300) * _scale, Color.Red);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(-2000f, 0f, 6500f) * _scale, new Vector3(750, 250, 750) * _scale, Color.Red);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(-900f, 0f, 4100f) * _scale, new Vector3(1000, 400, 1000) * _scale, Color.Red);
        
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(1320f, 0f, -3620f) * _scale, new Vector3(150, 50, 150) * _scale, Color.Yellow);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(650f, 0f, -5000f) * _scale, new Vector3(150, 50, 150) * _scale, Color.Blue);


        Game.Gizmos.DrawSphere(bbCenter + new Vector3(3000f, 0f, -5000f) * _scale, new Vector3(600, 150, 600) * _scale, Color.Cyan);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(2500f, 0f, -5500f) * _scale, new Vector3(500, 150, 500) * _scale, Color.Cyan);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(4800f, 0f, -4000f) * _scale, new Vector3(2500, 150, 2500) * _scale, Color.Cyan);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(6700, 0f, -5000f) * _scale, new Vector3(800, 100, 800) * _scale, Color.Cyan);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(2200f, 0f, -6000f) * _scale, new Vector3(600, 100, 600) * _scale, Color.Cyan);
        */


        /* ISLA 3
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(-1100f,0f,1400f) * _scale , new Vector3(1100,550,1100) * _scale, Color.Blue);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(-1700f, 0f, 400f) * _scale, new Vector3(900, 450, 900) * _scale, Color.Blue);


        Game.Gizmos.DrawSphere(bbCenter + new Vector3(1700f, 0f, 300f) * _scale, new Vector3(800, 150, 800) * _scale, Color.Yellow);

        Game.Gizmos.DrawSphere(bbCenter + new Vector3(1300f, 0f, -1700f) * _scale, new Vector3(300, 100, 300) * _scale, Color.Red);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(600f, 0f, -1700f) * _scale, new Vector3(700, 300, 700) * _scale, Color.Red);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(0f, 0f, -1900f) * _scale, new Vector3(800, 400, 800) * _scale, Color.Red);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(-600f, 0f, -1900f) * _scale, new Vector3(600, 300, 600) * _scale, Color.Red);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(-1000f, 0f, -1900f) * _scale, new Vector3(500, 200, 500) * _scale, Color.Red);
        Game.Gizmos.DrawSphere(bbCenter + new Vector3(-1300f, 0f, -1600f) * _scale, new Vector3(300, 100, 300) * _scale, Color.Red);
        */

        Game.Gizmos.DrawCube(bbCenter, bbExtents * 2f, Color.Red);
    }

    private void generarBoundinBoxesBasadoEnIsla(int numeroDeModelo)
    {
        var bbCenter = BoundingVolumesExtensions.GetCenter(BoundingBox);
        switch (numeroDeModelo)
        {
            case 0:
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(100f, 0f, 0f) * _scale, 2400f * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(-1000f, 0f, -1300f) * _scale, 1000 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(1100f, 0f, 1400f) * _scale, 1000 * _scale));
                break;
            case 2:
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(-1100f, 0f, 1400f) * _scale, 1100 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(-1700f, 0f, 400f) * _scale, 900 * _scale));

                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(1700f, 0f, 300f) * _scale, 800 * _scale));


                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(1300f, 0f, -1700f) * _scale, 300 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(600f, 0f, -1700f) * _scale, 700 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(0f, 0f, -1900f) * _scale, 800 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(-600f, 0f, -1900f) * _scale, 600 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(-1000f, 0f, -1900f) * _scale, 500 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(-1300f, 0f, -1600f) * _scale, 300 * _scale));

                break;
            default :

                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(-3700f, 0f, 4000f) * _scale, 3000 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(-3600f, 0f, 600f) * _scale, 3600 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(-3700f, 0f, -2000f) * _scale, 3200 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(-3400f, 0f, -6000f) * _scale, 1200 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(-3700f, 0f, -5000f) * _scale, 750 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(-6500f, 0f, -3100f) * _scale, 1600 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(-6300f, 0f, 5500f) * _scale, 1300 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(-2000f, 0f, 6500f) * _scale, 750 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(-900f, 0f, 4100f) * _scale, 1000 * _scale));

                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(1320f, 0f, -3620f) * _scale, 150 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(650f, 0f, -5000f) * _scale, 150  * _scale));


                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(3000f, 0f, -5000f) * _scale, 600 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(2500f, 0f, -5500f) * _scale, 500 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(4800f, 0f, -4000f) * _scale, 2500 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(6700, 0f, -5000f) * _scale, 800 * _scale));
                IslasColliders.Add(new BoundingSphere(bbCenter + new Vector3(2200f, 0f, -6000f) * _scale, 600 * _scale));


                break;
        }
    }
}