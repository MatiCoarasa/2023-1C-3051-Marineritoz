﻿using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Utils.GUI.ImGuiNET
{
    /// <summary>
    ///     ImGui class to use with XNA-likes (FNA & MonoGame).
    /// </summary>
    public static class DrawVertDeclaration
    {
        public static readonly VertexDeclaration Declaration;

        public static readonly int Size;

        static DrawVertDeclaration()
        {
            unsafe
            {
                Size = sizeof(ImDrawVert);
            }

            Declaration = new VertexDeclaration(
                Size,

                // Position
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),

                // UV
                new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),

                // Color
                new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            );
        }
    }
}