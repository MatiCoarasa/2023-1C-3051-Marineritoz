﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP.Cameras
{
    /// <summary>
    /// Una camara que sigue objetos
    /// </summary>
    public class ShipCamera : FollowCamera
    {
        private const float AxisDistanceToTarget = 7.5f;

        private const float AngleFollowSpeed = 0.015f;

        private const float AngleThreshold = 0.85f;

        private Vector3 CurrentRightVector { get; set; } = Vector3.Right;

        private float RightVectorInterpolator { get; set; } = 0f;

        private Microsoft.Xna.Framework.Vector3 PastRightVector { get; set; } = Vector3.Right;

        private int lastWheelValue;
        private float radius;
        private float yaw;
        private float factor;
        private float sens;
        private float pitch;

        /// <summary>
        /// Crea una FollowCamera que sigue a una matriz de mundo
        /// </summary>
        /// <param name="aspectRatio"></param>
        public ShipCamera(float aspectRatio) : base(aspectRatio)
        {
            // Orthographic camera
            // Projection = Matrix.CreateOrthographic(screenWidth, screenHeight, 0.01f, 10000f);

            // Perspective camera
            // Uso 60° como FOV, aspect ratio, pongo las distancias a near plane y far plane en 0.1 y 100000 (mucho) respectivamente
            Projection = Matrix.CreatePerspectiveFieldOfView(MathF.PI / 1.8f, aspectRatio, 0.1f, 1000f);

            radius = 7f;
            pitch = 10.0f;
            yaw = 180.0f;
            factor = 5f;
            sens = 25f;
        }

        /// <summary>
        /// Actualiza la Camara usando una matriz de mundo actualizada para seguirla
        /// </summary>
        /// <param name="gameTime">The Game Time to calculate framerate-independent movement</param>
        /// <param name="followedWorld">The World matrix to follow</param>
        public override void Update(GameTime gameTime, Matrix followedWorld)
        {
            // Obtengo el tiempo
            var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            // Obtengo la posicion de la matriz de mundo que estoy siguiendo
            var followedPosition = followedWorld.Translation;

            // Obtengo el vector Derecha de la matriz de mundo que estoy siguiendo
            var followedRight = followedWorld.Right;

            // Si el producto escalar entre el vector Derecha anterior
            // y el actual es mas grande que un limite,
            // muevo el Interpolator (desde 0 a 1) mas cerca de 1
            if (Vector3.Dot(followedRight, PastRightVector) > AngleThreshold)
            {
                // Incremento el Interpolator
                RightVectorInterpolator += elapsedTime * AngleFollowSpeed;

                // No permito que Interpolator pase de 1
                RightVectorInterpolator = MathF.Min(RightVectorInterpolator, 1f);

                // Calculo el vector Derecha a partir de la interpolacion
                // Esto mueve el vector Derecha para igualar al vector Derecha que sigo
                // En este caso uso la curva x^2 para hacerlo mas suave
                // Interpolator se convertira en 1 eventualmente
                CurrentRightVector = Vector3.Lerp(CurrentRightVector, followedRight, RightVectorInterpolator * RightVectorInterpolator);
            }
            else
                // Si el angulo no pasa de cierto limite, lo pongo de nuevo en cero
                RightVectorInterpolator = 0f;

            // Guardo el vector Derecha para usar en la siguiente iteracion
            PastRightVector = followedRight;



            detectarMovimiento(elapsedTime);


            var offsetedPosition = followedPosition + new Vector3(radius * MathF.Cos(MathHelper.ToRadians(yaw) * MathF.Cos(MathHelper.ToRadians(pitch)))
                    , radius * MathF.Sin(MathHelper.ToRadians(pitch))
                    , radius * MathF.Sin(MathHelper.ToRadians(yaw) * MathF.Cos(MathHelper.ToRadians(pitch))));

            // Calculo el vector Arriba actualizado
            // Nota: No se puede usar el vector Arriba por defecto (0, 1, 0)
            // Como no es correcto, se calcula con este truco de producto vectorial

            // Calcular el vector Adelante haciendo la resta entre el destino y el origen
            // y luego normalizandolo (Esta operacion es cara!)
            // (La siguiente operacion necesita vectores normalizados)
            var forward = (followedPosition - offsetedPosition);
            forward.Normalize();

            // Obtengo el vector Derecha asumiendo que la camara tiene el vector Arriba apuntando hacia arriba
            // y no esta rotada en el eje X (Roll)
            var right = Vector3.Cross(forward, Vector3.Up);

            // Una vez que tengo la correcta direccion Derecha, obtengo la correcta direccion Arriba usando
            // otro producto vectorial
            var cameraCorrectUp = Vector3.Cross(right, forward);

            // Calculo la matriz de Vista de la camara usando la Posicion, La Posicion a donde esta mirando,
            // y su vector Arriba
            View = Matrix.CreateLookAt(offsetedPosition, followedPosition, cameraCorrectUp);
        }


        private void detectarMovimiento(float elapsedTime)
        {


            var currentMouseState = Mouse.GetState();



            if (currentMouseState.ScrollWheelValue > lastWheelValue && radius > 4)
            {
                radius -= factor * sens * elapsedTime;
                lastWheelValue = currentMouseState.ScrollWheelValue;
            }

            if (currentMouseState.ScrollWheelValue < lastWheelValue && radius < 6)
            {
                radius += factor * sens * elapsedTime;
                lastWheelValue = currentMouseState.ScrollWheelValue;
            }

            if (currentMouseState.X > -1)
            {
                yaw -= factor * sens * elapsedTime;
                Mouse.SetPosition(0, 0);
            }

            if (currentMouseState.X < 1)
            {
                yaw += factor * sens * elapsedTime;
                Mouse.SetPosition(0, 0);
            }


            if (currentMouseState.Y > 4 && pitch > 0f)
            {

                pitch -= factor * sens / 2 * elapsedTime;
                Mouse.SetPosition(0, 0);

            }

            if (currentMouseState.Y < -4 && pitch < 30f)
            {
                pitch += factor * sens / 2 * elapsedTime;
                Mouse.SetPosition(0, 0);
            }
        }
    }
}
