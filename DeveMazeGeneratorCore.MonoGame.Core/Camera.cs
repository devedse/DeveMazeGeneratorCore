using DeveMazeGeneratorCore.MonoGame.Core.HelperObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeveMazeGeneratorMonoGame
{
    public class Camera
    {
        //public Vector3 cameraPosition = new Vector3(-100, 100, -100);
        public Vector3 cameraPosition = new Vector3(7.5f, 7.5f, 7.5f);

        public float leftrightRot = -MathHelper.Pi * 0.75f;

        //private float updownRot = -0.33f;
        public float updownRot = 0;

        private const float rotationSpeed = 0.6f;
        private float moveSpeed = 100.0f;
        private TheGame game;
        public Matrix viewMatrix;
        public Matrix projectionMatrix;

        private MouseState mState = default(MouseState);


        public Camera(TheGame game)
        {
            this.game = game;

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, game.GraphicsDevice.Viewport.AspectRatio, 0.3f, 10000000.0f);

            game.ResetMouseToCenter();
        }



        public void Update(GameTime gameTime)
        {

            MouseState currentMouseState = Mouse.GetState();

            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;


            GraphicsDevice device = game.GraphicsDevice;

            //Console.WriteLine($"Game active: {game.IsActive}, {currentMouseState.X}   {currentMouseState.Y}");
            if (game.IsActive)
            {
                float xDifference = currentMouseState.X - (game.AllowMouseResets ? (game.ScreenWidth / 2) : mState.X);
                float yDifference = currentMouseState.Y - (game.AllowMouseResets ? (game.ScreenHeight / 2) : mState.Y);
                game.ResetMouseToCenter();
                leftrightRot -= rotationSpeed * xDifference * timeDifference;

                var newUpDownRot = updownRot - (rotationSpeed * yDifference * timeDifference);

                updownRot = MathHelper.Clamp(newUpDownRot, -MathHelper.PiOver2, MathHelper.PiOver2);

                UpdateViewMatrix();
            }

            Vector3 moveVector = new Vector3(0, 0, 0);
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.W))
                moveVector += new Vector3(0, 0, -1);
            if (keyState.IsKeyDown(Keys.S))
                moveVector += new Vector3(0, 0, 1);
            if (keyState.IsKeyDown(Keys.D))
                moveVector += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.A))
                moveVector += new Vector3(-1, 0, 0);
            if (keyState.IsKeyDown(Keys.Space))
                moveVector += new Vector3(0, 1, 0);
            if (keyState.IsKeyDown(Keys.LeftShift))
                moveVector += new Vector3(0, -1, 0);
            AddToCameraPosition(moveVector * timeDifference);


            if (InputDing.CurMouse.LeftButton == ButtonState.Pressed)
            {
                moveSpeed = 1000.0f;
            }
            else if (InputDing.CurMouse.RightButton == ButtonState.Pressed)
            {
                moveSpeed = 30.0f;
            }
            else
            {
                moveSpeed = 100.0f;
            }


            mState = currentMouseState;
        }

        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            cameraPosition += moveSpeed * rotatedVector;
            UpdateViewMatrix();
        }

        public void UpdateViewMatrix()
        {
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);

            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = cameraPosition + cameraRotatedTarget;

            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraFinalTarget, cameraRotatedUpVector);
        }
    }
}
