using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using TGC.MonoGame.TP.Cameras;

namespace TGC.MonoGame.TP
{
    class Rain
    {
        public const string ContentFolderEffects = "Effects/";
        private Effect Effect { get; set; }

        private ContentManager Content { get; set; }

        private GraphicsDevice GraphicsDevice { get; set; }

        private VertexBuffer _vertexBuffer;

        private IndexBuffer _indexBuffer;

        private VertexBuffer _instanceBuffer;

        private float _size;

        private int _dropCount;

        private float _maxHeight;

        private float _minHeight;

        private float _speedBaseDrop;

        private DropDataInstance[] _dropDataInstance;

        private VertexBufferBinding[] Binding;

        private VertexDeclaration _instanceDeclaration;

        public Rain(ContentManager content, GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
            Content = content;
        }

        struct DropDataInstance
        {
            public Vector3 Offset;
        }

        public void Initialize(float size, float maxHeight, float minHeight, int dropCount, float speedDrop)
        {
            this._size = size;
            this._maxHeight = maxHeight;
            this._minHeight = minHeight;
            this._speedBaseDrop = speedDrop;
            this._dropCount = dropCount;

            var rand = new Random();

            var middleNumber = _size / 2;

            this._dropDataInstance = new DropDataInstance[_dropCount];

            for ( int i = 0; i < _dropCount; i++)
            {
                _dropDataInstance[i].Offset = new Vector3(rand.NextSingle() * -_size + middleNumber, rand.NextSingle() * maxHeight + 0.001f, rand.NextSingle() * -_size + middleNumber);
            }
        }

        public void Load()
        {
            _vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColorNormal), 4, BufferUsage.None);

            // Pasa la ram a GPU
            _vertexBuffer.SetData(new VertexPositionColorNormal[]
            {
                new VertexPositionColorNormal(new Vector3(0f,-1f,-0.03f),Color.Blue,Vector3.UnitX),
                new VertexPositionColorNormal(new Vector3(0f,-1f,0.03f),Color.White,Vector3.UnitX),
                new VertexPositionColorNormal(new Vector3(0f,1f,0.03f),Color.White,Vector3.UnitX),
                new VertexPositionColorNormal(new Vector3(0f,1f,-0.03f),Color.SkyBlue,Vector3.UnitX),
            });


            _instanceDeclaration = new VertexDeclaration(new VertexElement[]
            {
                new VertexElement(0,VertexElementFormat.Vector3,VertexElementUsage.Position,1)
            });

            _instanceBuffer = new VertexBuffer(GraphicsDevice, _instanceDeclaration, _dropCount, BufferUsage.None);

            _instanceBuffer.SetData(_dropDataInstance);




            _indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, 6, BufferUsage.None);

            _indexBuffer.SetData(new ushort[]{
                0, 3, 1,
                1, 3, 2
            });



            Binding = new VertexBufferBinding[2];
            Binding[0] = new VertexBufferBinding(_vertexBuffer);
            Binding[1] = new VertexBufferBinding(_instanceBuffer, 0, 1);



            Effect = Content.Load<Effect>(ContentFolderEffects + "RainShader");
        }
        
        public void Draw(GameTime gameTime, Camera Camera)
        {
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            Effect.Parameters["View"].SetValue(Camera.View);
            Effect.Parameters["Projection"].SetValue(Camera.Projection);
            Effect.Parameters["DiffuseColor"].SetValue(Color.Blue.ToVector3());
            Effect.Parameters["Time"]?.SetValue(Convert.ToSingle(gameTime.TotalGameTime.TotalSeconds));
            Effect.Parameters["MaxHeight"]?.SetValue(_maxHeight);
            Effect.Parameters["MinHeight"]?.SetValue(_minHeight);
            Effect.Parameters["Speed"]?.SetValue(_speedBaseDrop);
            Effect.Parameters["CameraPosition"].SetValue(Camera.Position);
            GraphicsDevice.Indices = _indexBuffer;
            GraphicsDevice.SetVertexBuffers(Binding);
            Effect.CurrentTechnique.Passes[0].Apply();


            GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 2,_dropCount);
        }
    }
}
