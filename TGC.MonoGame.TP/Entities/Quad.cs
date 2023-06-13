using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Cameras;

namespace TGC.MonoGame.TP.Entities
{
    public class Quad
    {
        private VertexBuffer Vertices { get; set; }
        private Texture2D Texture { get; set; }
        private IndexBuffer Indices { get; set; }
        private Effect Effect { get; set; }

        public Quad(GraphicsDevice graphicsDevice, int rows)
        {
            SetVertexBuffer(graphicsDevice, rows);
            CreateIndexBuffer(graphicsDevice, rows);
        }

        public void LoadContent(Effect effect, Texture2D texture)
        {
            Effect = effect;
            Texture = texture;
        }

        public void SetVertexBuffer(GraphicsDevice graphicsDevice, int rows)
        {
            float subdivisionPosition = 2f / rows;
            // Si queremos que todo el quad tenga la misma textura
            // float subdivisionTexture = 1f / rows;
            List<VertexPositionColorNormal> vertices = new List<VertexPositionColorNormal>();

            /*
             * IMPORTANTE:
             * La textura se mueve de 0 a 1
             * La posición se mueve de -1 a 1
            */
            for (float i = 0; i <= rows; i++)
            {
                for (float j = 0; j <= rows; j++)
                {
                    vertices.Add(new VertexPositionColorNormal(
                        new Vector3(Convert.ToSingle(i * subdivisionPosition - 1), 0, Convert.ToSingle(j * subdivisionPosition - 1)),
                        Color.Aqua,
                        Vector3.UnitY)
                    );
                }
            }
            Vertices = new VertexBuffer(graphicsDevice, VertexPositionColorNormal.VertexDeclaration, vertices.Count,
                BufferUsage.None);
            Vertices.SetData(vertices.ToArray());
        }

        private void CreateIndexBuffer(GraphicsDevice graphicsDevice, int rows)
        {
            List<int> indices = new List<int>();

            /*
             * 0 ---- 1  0 = left upper vertex
             * |   /  |  1 = right upper vertex
             * | /    |  2 = left bottom vertex
             * 2 ---- 3  3 = right bottom vertex
            */
            for (int i = 0; i <= rows - 1; i++)
            {
                for (int j = 0; j <= rows - 1; j++)
                {
                    // Es el salto en cantidad que hace el primer vertice de una row a otra row
                    var jump = rows + 1;
                    var leftUpperVertex = (int) (j + jump * i);
                    var rightUpperVertex = (int) (j + jump * i + 1);
                    var leftBottomVertex = (int) (jump * (i + 1) + j);
                    var rightBottomVertex = (int) (jump * (i + 1) + j + 1);
                    // Triangulo superior
                    indices.Add(leftUpperVertex);
                    indices.Add(rightUpperVertex);
                    indices.Add(leftBottomVertex);
                    // Triangulo inferior
                    indices.Add(rightUpperVertex);
                    indices.Add(rightBottomVertex);
                    indices.Add(leftBottomVertex);
                }
            }
            Indices = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count,
                BufferUsage.None);
            Indices.SetData(indices.ToArray());
        }

        /// <summary>
        ///     Draw the Quad.
        /// </summary>
        /// <param name="world">The world matrix for this box.</param>
        /// <param name="view">The view matrix, normally from the camera.</param>
        /// <param name="projection">The projection matrix, normally from the application.</param>
        /// <param name="time">Time in second since the game started</param>
        public void Draw(Vector3 lightPosition, Camera camera, Matrix world, float time)
        {
            Effect.Parameters["lightPosition"].SetValue(lightPosition);
            Effect.Parameters["eyePosition"].SetValue(camera.Position);
            Effect.Parameters["ambientColor"].SetValue(new Color(0, 115, 153).ToVector3());
            Effect.Parameters["diffuseColor"].SetValue(new Color(51, 153, 255).ToVector3());
            Effect.Parameters["specularColor"].SetValue(new Color(179, 236, 255).ToVector3());
            Effect.Parameters["KAmbient"].SetValue(0.8f);
            Effect.Parameters["KDiffuse"].SetValue(0.5f);
            Effect.Parameters["KSpecular"].SetValue(0.2f);
            Effect.Parameters["shininess"].SetValue(2.0f);
            Effect.Parameters["DiffuseColor"].SetValue(new Color(0, 204, 255).ToVector3());
            Effect.Parameters["World"].SetValue(world);
            Effect.Parameters["View"].SetValue(camera.View);
            Effect.Parameters["Projection"].SetValue(camera.Projection);
            Effect.Parameters["Time"].SetValue(time);
            Draw(Effect);
        }

        /// <summary>
        ///     Draws the primitive model, using the specified effect. Unlike the other Draw overload where you just specify the
        ///     world/view/projection matrices and color, this method does not set any render states, so you must make sure all
        ///     states are set to sensible values before you call it.
        /// </summary>
        /// <param name="effect">Used to set and query effects, and to choose techniques.</param>
        public void Draw(Effect effect)
        {
            var graphicsDevice = effect.GraphicsDevice;

            // Set our vertex declaration, vertex buffer, and index buffer.
            graphicsDevice.SetVertexBuffer(Vertices);
            graphicsDevice.Indices = Indices;
            foreach (var effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Indices.IndexCount / 3);
            }
        }
    }
}
