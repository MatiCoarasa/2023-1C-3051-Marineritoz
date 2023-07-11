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
using Microsoft.Xna.Framework.Audio;
using TGC.MonoGame.TP.Cameras;

namespace TGC.MonoGame.TP.Entities
{


    public class Arsenal
    {

        private TGCGame Game;
        public Effect Effect;
        public Model Model;
        private const string ContentFolder3D = "Models/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderEffects = "Effects/";

        private Obus[] _bullets;

        private float shootCooldown = 2f;

        private float timerCooldownShoot = 0;

        private float angle = 0f;
        private float sens = 10f;
        private Vector3 ShipPosition;
        private Vector3 targetDirectionXZ;

        private int _size;
        private int currentBullet = 0;
        private SoundEffect GunShotEffect { get; set; }

        public Arsenal(TGCGame game,int size, Vector3 shipPosition) {
            
            ShipPosition = shipPosition;
            _size = size;
            _bullets = new Obus[_size];

            for (int i = 0; i < _size; i++) _bullets[i] = new Obus(game, ShipPosition);

            Game = game;
        }

        public void LoadContent()
        {
            GunShotEffect = Game.Content.Load<SoundEffect>(ContentFolderSounds + "gunshot");
        }

        public void Update(float elapsedTime, Vector3 shipPosition, Camera Camera)
        {
            var keyboardState = Keyboard.GetState();
            ShipPosition = shipPosition;
            timerCooldownShoot += elapsedTime;

            // Obtengo el vector que mira hacia la camara y lo pongo en negativo para que sea el "donde mira la camara"
            targetDirectionXZ = new Vector3(-Camera.View.M13,0f,-Camera.View.M33);


            foreach( Obus oneBullet  in _bullets )
            {
                oneBullet.Update(elapsedTime, ShipPosition, targetDirectionXZ, MathHelper.ToRadians(angle));
            }

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
                Fire();
                var instance = GunShotEffect.CreateInstance();
                instance.Volume = 0.05f;
                instance.Play();
                timerCooldownShoot = 0f;
            }

        }

        public void Draw(Camera camera)
        {

            foreach (Obus bullet in _bullets)
            {
                bullet.Draw(camera);
            }

            // Hay un problema con el dibujo. Pareciera que estuviera apntando en el eje Y. Supongo que es una especie de ilusion.
            // ya que como la camara está alineado con la linea se ve de esa manera. (Supongo)
            Game.Gizmos.DrawLine(ShipPosition, ShipPosition + targetDirectionXZ * 10f, Color.White);
        }

        public void Fire()
        {

            currentBullet++;

            if (currentBullet >= _bullets.Length) 
                currentBullet = 0;
            _bullets[currentBullet].Fire();
            
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

            foreach (var bullet in _bullets)
            {
                bullet.LoadContent(content, Effect, Model);
            }
        }

        public bool CheckCollision(BoundingBox collider)
        {
            var hasCollisioned = false;
            foreach( var bullet in _bullets)
            {
                hasCollisioned = hasCollisioned || bullet.CheckCollision(collider);
            }
            return hasCollisioned;
        }

    }
}
