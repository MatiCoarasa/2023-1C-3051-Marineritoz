using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Entities;

namespace TGC.MonoGame.TP.Helpers
{
    public class Parcela
    {
        private Matrix World { get; set; }

        private Island Island { get; set; }

        private List<SimpleShip> _ships = new List<SimpleShip> { };
        private Model ModelIsland { get; set; }
        private Effect EffectIsland { get; set; }
        private Model ModelShip { get; set; }
        private Effect EffectShip { get; set; }


        public Parcela (Matrix world, int shipQuantity, Effect commonEffect,  Model modelIsland, Model modelShip)
        {
            this.World = world;
            this.Island = new Island(World);
            this.EffectIsland = commonEffect;
            this.EffectShip = commonEffect;
            this.ModelIsland = modelIsland;
            this.ModelShip = modelShip;

             for (int i = 0; i < shipQuantity; i++) _ships.Add(new SimpleShip(matrizDeBarco(world, i + 1)));

        }
        public Parcela(Matrix world, int shipQuantity, Model modelIsland, Effect effectIsland, Model modelShip, Effect effectShip)
        {
            World = world;
            Island = new Island(Matrix.CreateScale(4f) * World);


            this.EffectIsland = effectIsland;
            this.EffectShip = effectShip;
            this.ModelIsland = modelIsland;
            this.ModelShip = modelShip;

            for (int i = 0; i < shipQuantity; i++) _ships.Add(new SimpleShip(matrizDeBarco(world, i + 1)));
        }

        private Matrix matrizDeBarco(Matrix baseMatrix, float desplazamiento)
        {
            var randomNumber = new Random();

            float randomYaw = Convert.ToSingle(randomNumber.NextDouble() * 100);
            float rotation = Convert.ToSingle(randomNumber.NextDouble() * 100);
            float displacement = Convert.ToSingle(randomNumber.NextDouble() * -4000 - 19000);

            return Matrix.CreateFromYawPitchRoll(randomYaw, 0f, 0f) * Matrix.CreateScale(0.3f) *Matrix.CreateTranslation(new Vector3(displacement, -500f, 0f)) * Matrix.CreateRotationY(rotation) * baseMatrix ;
        }

        public void Draw()
        {


            Island.Draw(this.ModelIsland, this.EffectIsland);
            foreach (var ship in _ships)
            {
                ship.Draw(this.ModelShip, this.EffectShip);
            }
        }
    }
}
