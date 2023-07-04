using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Content.Gizmos;

namespace TGC.MonoGame.TP.Entities.Islands
{
    public class Block
    {
        private int BlockSize;
        private Vector3 VertexPosition = Vector3.Zero;
        private Vector3 vectorCentro;
        private BoundingBox BlockBoundinBox;

        public List<Island> Islands = new List<Island> { };
        public Block(int blockSize, Vector3 basePosition) 
        { 
            BlockSize = blockSize;
            VertexPosition = basePosition;
            vectorCentro = new Vector3(BlockSize / 2, 0f, BlockSize / 2) + VertexPosition;
            BlockBoundinBox = new BoundingBox(VertexPosition,new Vector3(VertexPosition.X + BlockSize,5f,VertexPosition.Z + BlockSize));
        }

        // Retorna el centro del blque
        public Vector3 getCenter()
        {
            return new Vector3(BlockSize/2,0f,BlockSize/2) + VertexPosition;
        }

        // Retorna las cuatro posiciones para poder colocar uan isla.
        // Que son el centro de los subcuadrados.
        /*  _____________
         *  |      |     |
         *  |  x   | x   |
         *  -------|-----
         *  | x    | x   |
         *  |_____ |_____|
         */
        public List<Vector3> getPositions()
        {
            
            List<Vector3> posiciones = new List<Vector3> { };

            Vector3 vectorDistancia = new Vector3(BlockSize / 4, 0f, BlockSize / 4);

            posiciones.Add(vectorCentro + vectorDistancia);
            posiciones.Add(vectorCentro - vectorDistancia);
            posiciones.Add(vectorCentro + new Vector3(-vectorDistancia.X, 0f ,vectorDistancia.Z));
            posiciones.Add(vectorCentro + new Vector3(vectorDistancia.X, 0f, -vectorDistancia.Z));

            return posiciones;
        }

        public void LoadIsland(Island island)
        {
            Islands.Add(island);
        }

        public void Draw(TGCGame game, Camera camera, Vector3 lightPosition, BoundingFrustum bouding)
        {


            if (bouding.Intersects(BlockBoundinBox))
            {
                game.Gizmos.DrawCube(vectorCentro, new Vector3(600f, 5f, 600f), Color.White);
                foreach (Island island in Islands)
                {
                    island.Draw(camera, lightPosition);
                }
            } else
            {
                game.Gizmos.DrawCube(vectorCentro, new Vector3(600f, 5f, 600f), Color.Yellow);
            }


        }
    }
}
