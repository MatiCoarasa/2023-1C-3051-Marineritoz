using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.MonoGame.TP.Cameras;

namespace TGC.MonoGame.TP.Entities.Islands
{
    public class Map
    {
        private GlobalConfigurationSingleton GlobalConfig => GlobalConfigurationSingleton.GetInstance();
        int _blockSize;
        private IslandGenerator _generator;
        float _squarePerSide;  // Deben ser par
        private List<Block> Blocks = new List<Block>();
        private Random Rnd = new Random();
        private GraphicsDevice GraphicsDevice;

        public Map(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            _blockSize = GlobalConfig.BlockSize;
            _squarePerSide = GlobalConfig.SquarePerSize; 

        }
        public void Load(TGCGame game, ContentManager content, Effect effect)
        {
            _generator = new IslandGenerator(game);
            _generator.LoadContent(content, effect);


            float PosicionX = _blockSize * _squarePerSide / 2 - _blockSize;
            float PosicionZ = -_blockSize * _squarePerSide / 2;
            // Columna
            for (int i = 0; i < _squarePerSide; i++)
            {
                //Fila
                for (int j = 0; j < _squarePerSide; j++) {

                    Block _block = new Block(_blockSize, new Vector3(PosicionX, 0f, PosicionZ));
                    int modelNumer = Rnd.Next(2);

                    if (modelNumer != 1)
                    {
                        loadIslandsInBlock(_block, modelNumer);
                    } else
                    {
                        _block.LoadIsland(_generator.CreateIslandWithRandomScale(modelNumer, _block.getCenter()));
                    }
                    Blocks.Add(_block);

                    PosicionZ += _blockSize;
                }

                PosicionX -= _blockSize;
                PosicionZ = -_blockSize * _squarePerSide / 2;
            }
        }

        private void loadIslandsInBlock(Block block, int modelNumber)
        {
            int maxLoads = Rnd.Next(4);
            int index = 0;

            List<Vector3> posiciones = block.getPositions();


            for (int i = 0; i < maxLoads; i++)
            {
                index = Rnd.Next(posiciones.Count - 1);

                block.LoadIsland(_generator.CreateIslandWithRandomScale(modelNumber, posiciones[index]));

                posiciones.RemoveAt(index);
            }

        }

        public List<BoundingBox> IslandColliders()
        {
            List<BoundingBox> boxes = new List<BoundingBox>();

            foreach(Block block in Blocks)
            {
               foreach(Island island in block.Islands)
                {
                    boxes.Add(island.BoundingBox);
                }
            }

            return boxes;
        }

        public void Draw(TGCGame game, Camera camera,Vector3 lightPosition, BoundingFrustum bounding)
        {
            GraphicsDevice.BlendState = BlendState.Opaque;

            foreach (Block block in Blocks)
            {
                block.Draw(game, camera, lightPosition, bounding);
            }
        }

        public Vector3 getSafeSpawnPosition(Vector3 shipPosition)
        {
            List<Vector3> posicionesSeguras = new List<Vector3> { };
            foreach(Block block in Blocks)
            {
                if (block.VertexPosition != Vector3.Zero && Vector3.Distance(block.VertexPosition, shipPosition) < 850)
                {
                    posicionesSeguras.Add(block.VertexPosition);
                }
            }
            int index = Rnd.Next(posicionesSeguras.Count - 1);
            return posicionesSeguras[index];
        }
    }
}
