using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.MonoGame.TP.Cameras;

namespace TGC.MonoGame.TP.Entities
{


    class Arsenal
    {

        private TGCGame Game;
        private Effect Effect;
        private Model Model;
        private const string ContentFolder3D = "Models/";

        private List<Obus> _bullets = new List<Obus> { };

        private float shootCooldown = 4f;
        private float reUseBulletCooldown = 4f;

        private float timerCooldownShoot = 0;
        private float timerReloadCooldown = 0;

        private float angle = 0f;
        private float sens = 10f;
        private Vector3 ShipPosition;
        private Vector3 targetDirectionXZ;
        public Arsenal(TGCGame game,int size, Vector3 shipPosition) {
            ShipPosition = shipPosition;
            for (int i = 0; i < size; i++) _bullets.Add(new Obus(game, ShipPosition));
            Game = game;
        }
        public void Update(GameTime gameTime, Vector3 shipPosition, Camera Camera)
        {
            var keyboardState = Keyboard.GetState();
            ShipPosition = shipPosition;

            var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            timerCooldownShoot += elapsedTime;


            this.Reload(gameTime);


            // Obtengo el vector que mira hacia la camara y lo pongo en negativo para que sea el "donde mira la camara"
            targetDirectionXZ = new Vector3(-Camera.View.M13,0f,-Camera.View.M33);

            _bullets.ForEach(obus => obus.Update(gameTime, ShipPosition, targetDirectionXZ, MathHelper.ToRadians(angle)));

            if (keyboardState.IsKeyDown(Keys.Up) && angle < 49.9f)
            {
                angle += 3f * sens * elapsedTime;
            }

            if (keyboardState.IsKeyDown(Keys.Down) && angle > -0.05f)
            {
                angle -= 3f * sens * elapsedTime;
            }

            if (keyboardState.IsKeyDown(Keys.F) && timerCooldownShoot > shootCooldown)
            {
                this.Fire();
                timerCooldownShoot = 0f;
            }

        }

        public void Reload(GameTime gameTime)
        {
            Debug.WriteLine("Tamaño de los que estan volando: " + _bullets.FindAll(b => b.Firing).Count);

            if (_bullets.Any(obus => obus.Firing))
            {
                timerReloadCooldown += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
                if (timerReloadCooldown > reUseBulletCooldown)
                {
                    _bullets.Find(Obus => Obus.Firing)?.Reload();
                    //if (bulletUsed != null) bulletUsed.Reload();

                    timerReloadCooldown = 0f;
                    Debug.WriteLine("Recargo");
                }
            }
        }

        public void Draw(Camera camera)
        {
            _bullets.ForEach(obus => obus.Draw(camera));
            // Hay un problema con el dibujo. Pareciera que estuviera apntando en el eje Y. Supongo que es una especie de ilusion.
            // ya que como la camara está alineado con la linea se ve de esa manera. (Supongo)
            Game.Gizmos.DrawLine(ShipPosition, ShipPosition + targetDirectionXZ * 10f, Color.White);
        }

        public void Fire()
        {

            var _bullet = _bullets.Find(obus => !obus.Firing);

            if ( _bullet != null)
            {
                _bullet.Fire();
                _bullets.RemoveAt(0);
                _bullets.Add(_bullet);
            } else return;

            
        }


        public void LoadContent(ContentManager content, Effect effect)
        {

            Effect = effect;
            Model = content.Load<Model>(ContentFolder3D + "Obus/Obus");

            foreach (var mesh in Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Effect;
                }
            }

            _bullets.ForEach(obus => obus.LoadContent(content, effect, Model));
        }

        public void CheckCollision(BoundingBox collider)
        {
            _bullets.ForEach(o => o.CheckCollision(collider));
        }

    }
}
