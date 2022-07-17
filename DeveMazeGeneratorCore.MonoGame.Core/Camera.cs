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
        private readonly bool allowMouseResets;
        public Matrix viewMatrix;
        public Matrix projectionMatrix;

        private int screenWidth;
        private int screenHeight;

        private MouseState mState = default(MouseState);


        public Camera(TheGame game, bool allowMouseResets)
        {
            this.game = game;
            this.allowMouseResets = allowMouseResets;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, game.GraphicsDevice.Viewport.AspectRatio, 0.3f, 10000000.0f);

            screenWidth = game.Window.ClientBounds.Width;
            screenHeight = game.Window.ClientBounds.Height;

            ResetMouseToCenter();
        }

        public void ResetMouseToCenter()
        {
            if (allowMouseResets)
            {
                Mouse.SetPosition(screenWidth / 2, screenHeight / 2);
            }
        }

        public void Update(GameTime gameTime)
        {

            MouseState currentMouseState = Mouse.GetState();

            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;


            GraphicsDevice device = game.GraphicsDevice;
            if (currentMouseState != mState)
            {
                float xDifference = currentMouseState.X - (allowMouseResets ? (screenWidth / 2) : mState.X);
                float yDifference = currentMouseState.Y - (allowMouseResets ? (screenHeight / 2) : mState.Y);
                ResetMouseToCenter();
                leftrightRot -= rotationSpeed * xDifference * timeDifference;

                var newUpDownRot = updownRot - (rotationSpeed * yDifference * timeDifference);

                updownRot = MathHelper.Clamp(newUpDownRot, -CustomMathHelper.HalfPi, CustomMathHelper.HalfPi);

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

        private void UpdateViewMatrix()
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
