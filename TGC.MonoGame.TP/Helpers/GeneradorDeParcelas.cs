using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Camera;

namespace TGC.MonoGame.TP.Helpers
{
    public class GeneradorDeParcelas
    {

        private int _cantidadDebarcos;
        private Matrix _world;

        private Model modelIsland;
        private Effect effectIsland;
        private Model modelShip;
        private Effect effectShip;
        public GeneradorDeParcelas() { }

        public GeneradorDeParcelas setCantidadDeBarcos(int cantidad)
        {
            this._cantidadDebarcos = cantidad;
            return this;
        }

        public GeneradorDeParcelas setPosicion(Matrix world)
        {
            this._world = world;
            return this;
        }

        public GeneradorDeParcelas setModelIsland(Model modelIsland)
        {
            this.modelIsland = modelIsland;
            return this;
        }

        public GeneradorDeParcelas setModelShip(Model modeShip)
        {
            this.modelShip = modeShip;
            return this;
        }

        public GeneradorDeParcelas setDefaultEffect(Effect effect) { 
            this.effectShip = effect;
            this.effectIsland = effect;
            return this;
        }

        public Parcela generar()
        {
            //Matrix world, int shipQuantity, Model modelIsland, Effect effectIsland, Model modelShip, Effect effectShip

            if (effectShip == null) Debug.WriteLine("NO TENGO NADA");

            return new Parcela(this._world,this._cantidadDebarcos, this.modelIsland, this.effectIsland, this.modelShip, this.effectShip);

        }
    }
}
