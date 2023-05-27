﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP.Entities
{
    /// <summary>
    /// Esta es la primera version de 0.1 del agua.
    /// </summary>
    public class Water
    {
        private const string ContentFolderEffects = "Effects/";
        private const string ContentFolderTextures = "Textures/";
        private const int RowsOfQuads = 100;
        private GraphicsDevice GraphicsDevice { get; }
        private Quad Quad { get; }

        public Water(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            Quad = new Quad(graphicsDevice, RowsOfQuads);
        }

        public void LoadContent(ContentManager contentManager)
        {
            var waterTexture = contentManager.Load<Texture2D>(ContentFolderTextures + "water");
            var textureEffect = contentManager.Load<Effect>(ContentFolderEffects + "OceanShader");
            Quad.LoadContent(textureEffect, waterTexture);
        }

        /// <summary>
        /// Recibe la posicion donde se va a empezar a generar el mar. 
        /// Empieza a dibujar de izquierda a derecha. Por lo tanto si seteamos la posicion en 0. Va a empezar a dibujar hacia la izquierda
        /// (Todo esto basado en la camara que tenemos ahora, obviamente)
        /// </summary>
        /// <param name="view"></param>
        /// <param name="projection"></param>
        /// <param name="time"></param>
        public void Draw(Matrix view, Matrix projection, float time)
        {
            const float escala = 1000f;
            var world = Matrix.CreateScale(escala) * Matrix.CreateTranslation(0,0.0005f, 0);
            
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            
            Quad.Draw(world, view, projection, time);
        }
    }
}
