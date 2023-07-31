using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using TGC.MonoGame.TP.Cameras;

namespace TGC.MonoGame.TP.Entities
{
    /// <summary>
    /// Esta es la primera version de 0.1 del agua.
    /// </summary>
    public class Water
    {
        private const string ContentFolderEffects = "Effects/";
        private const string ContentFolderTextures = "Textures/";
        private const int RowsOfQuads = 200;
        private GraphicsDevice GraphicsDevice { get; }
        private Quad Quad { get; }
        private Quad ShipQuad { get; }

        public Water(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            Quad = new Quad(graphicsDevice, RowsOfQuads);
            ShipQuad = new Quad(graphicsDevice, 1500);
        }

        public void LoadContent(ContentManager contentManager)
        {
            var waterTexture = contentManager.Load<Texture2D>(ContentFolderTextures + "water");
            var textureEffect = contentManager.Load<Effect>(ContentFolderEffects + "OceanShader");
            Quad.LoadContent(textureEffect, waterTexture);
            ShipQuad.LoadContent(textureEffect, waterTexture);
        }

        /// <summary>
        /// Recibe la posicion donde se va a empezar a generar el mar. 
        /// Empieza a dibujar de izquierda a derecha. Por lo tanto si seteamos la posicion en 0. Va a empezar a dibujar hacia la izquierda
        /// (Todo esto basado en la camara que tenemos ahora, obviamente)
        /// </summary>
        /// <param name="view"></param>
        /// <param name="projection"></param>
        /// <param name="time"></param>
        public void Draw(Vector3 ShipPosition, Vector3 lightPosition, Camera camera, float time, RenderTarget2D renderTargetCube)
        {
            var waterPosition = ShipPosition;
            waterPosition.Y = 0;
            var world = Matrix.CreateScale(500f) * Matrix.CreateTranslation(waterPosition);
            var worldAbove = Matrix.CreateScale(50f) * Matrix.CreateTranslation(waterPosition);

            var previousBlendState = GraphicsDevice.BlendState;
            var previousDepthStencilState = GraphicsDevice.DepthStencilState;
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Quad.SetOceanDrawing();
            Quad.Draw(lightPosition, camera, world * Matrix.CreateTranslation(Vector3.Down * 0.27f), time, renderTargetCube);
            Quad.SetEnvironmentMappingDrawing();
            Quad.Draw(lightPosition, camera, worldAbove, time, renderTargetCube);
            GraphicsDevice.BlendState = previousBlendState;
            GraphicsDevice.DepthStencilState = previousDepthStencilState;
        }
        
        public void SetOceanDrawing()
        {
            Quad.SetOceanDrawing();
        }

        public void SetEnvironmentMappingDrawing()
        {
            Quad.SetEnvironmentMappingDrawing();
        }
    }
}
