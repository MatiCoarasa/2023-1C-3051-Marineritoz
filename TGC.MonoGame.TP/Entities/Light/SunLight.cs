using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.TP.Cameras;

namespace TGC.MonoGame.TP.Entities.Light;

public class SunLight
{
    public Light Light { get; set; }
    public CubePrimitive LightBox { get; set; }

    public SunLight(GraphicsDevice graphicsDevice)
    {
        var light = new Light();
        light.Color = new Vector3(255, 210, 0);
        Light = light.GenerateShowColor();
        
        LightBox = new CubePrimitive(graphicsDevice, 1, Color.White);
    }

    public void LoadContent(Effect effect)
    {
        LightBox.LoadEffect(effect);
    }

    public void Update(float time)
    {
        var light = Light;
        var reducedTime = time * 0.2f;
        light.Position = new Vector3(MathF.Cos(reducedTime) * 200f, 200f, MathF.Sin(reducedTime) * 200f);
        // light.Position = new Vector3(0, 100f, 0);
        Light = light;
    }

    public void Draw(Camera camera, Effect effect)
    {
        effect.Parameters["DiffuseColor"].SetValue(Light.ShowColor);
        LightBox.Draw(Matrix.CreateTranslation(Light.Position), camera.View, camera.Projection);
    }
}