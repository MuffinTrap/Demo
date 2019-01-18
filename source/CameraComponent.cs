using OpenTK;
using OpenTK.Input;
using System.Collections.Generic;
using System;

namespace MuffinSpace
{
	public struct PosAndDir
	{
		public Vector3 position;
		public Vector3 direction;

		public PosAndDir(Vector3 posParam, Vector3 dirParam)
		{
			position = posParam;
			direction = dirParam;
		}
	}

	public class CameraComponent : IShaderDataOwner
	{
		// Camera
		public Vector3 Position
		{
			get;
			set;
		}

		// Direction is the opposite of where the camera is actually pointing to
		// because movement is done so that to move forward, 
		// we translate the scene in opposite direction 
		public Vector3 Direction
		{
			get;
			set;
		}

		private Vector3 CameraFront
		{
			get;
			set;
		}

		public Vector3 Up
		{
			get;
			set;
		}

		private int lastMouseX = 0;
		private int lastMouseY = 0;

		private int cameraDebug = 0;

		public float Speed { get; set; }
		private float speedStep;
		public float SpeedStep { get
			{
				return speedStep;
			}
			set
			{
				speedStep = Math.Max(0.0f, value);
			}
		}

		public float FOV = 90.0f;
		public float AspectRatio = 16.0f / 9.0f;
		public float Near = 0.1f;
		public float Far = 100.0f;

		public bool FreeMode { get; set; }

		private float yaw = 0.0f;
		private float pitch = 0.0f;

		private Matrix4Uniform viewMatrix;
		private Matrix4Uniform projectionMatrix;
		private Matrix4Uniform perspectiveMatrix;
		private Matrix4Uniform orthogonalMatrix;

		public CameraComponent()
		{
			// OPENGL IS RIGHT HANDED 

			// IN OPENGL THE POSITIVE Z IS TOWARDS YOU
			Position = new Vector3(0, 0, 0.0f);

			// THE NEGATIVE IS TO THE DEPTH
			Direction = CalculateEulerDirection(pitch, yaw);
			Up = new Vector3(0.0f, 1.0f, 0.0f);

			Speed = 0.15f;
			SpeedStep = 0.05f;

			viewMatrix = new Matrix4Uniform(ShaderUniformName.ViewMatrix);
			projectionMatrix = new Matrix4Uniform(ShaderUniformName.ProjectionMatrix);
			orthogonalMatrix = new Matrix4Uniform(ShaderUniformName.ProjectionMatrix);
			perspectiveMatrix = new Matrix4Uniform(ShaderUniformName.ProjectionMatrix);

			FOV = 90.0f;
			CreateMatrices();

			EnablePerspective();

			viewMatrix.Matrix = GetViewMatrix();

			Logger.LogInfo("Camera start. Dir: (" + Direction.X + ", " + Direction.Y + ", " + Direction.Z + ")");
		}

		public void CreateMatrices()
		{
			perspectiveMatrix.Matrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV), AspectRatio, Near, Far);
			orthogonalMatrix.Matrix = Matrix4.CreateOrthographic(AspectRatio, 1.0f, Near, Far);

		}


		public bool SetUniform(ShaderProgram program, int location, ShaderUniformName name)
		{
			switch(name)
			{
				case ShaderUniformName.ViewMatrix:
					viewMatrix.Matrix = GetViewMatrix();
					viewMatrix.SetToShader(program, location);
					break;
				case ShaderUniformName.ProjectionMatrix:
					projectionMatrix.SetToShader(program, location);
					break;
				default:
					return false;
			}
			return true;
		}

		public void EnablePerspective()
		{
			projectionMatrix = perspectiveMatrix;
		}

		public void EnableOrthogonal()
		{
			projectionMatrix = orthogonalMatrix;
		}

		private Vector3 GetCameraDirectionTo(Vector3 targetPosition)
		{
			// This is intentionally wrong way around
			return Vector3.Normalize(Position - targetPosition);
		}

		public Matrix4 GetViewMatrix()
		{
			Vector3 target = Position + CameraFront;
			Direction = GetCameraDirectionTo(target);
			Matrix4 lookAtRotation = LookAtRotation(Position, Direction);


			lookAtRotation.Transpose();

			// Matrix4 translationInverse = Matrix4.CreateTranslation(Position).Inverted();
			Matrix4 lookAtTranslation = Matrix4.CreateTranslation(-Position);
			//Matrix4 lookAtMatrix = lookAtRotation * lookAtTranslation;
			Matrix4 lookAtMatrix = lookAtTranslation * lookAtRotation;
			return lookAtMatrix;
			
		}


		public Matrix4 LookAtRotation(Vector3 cameraPosition, Vector3 cameraDirection)
		{
			// From gluLookAt
			Vector3 worldUp = new Vector3(0.0f, 1.0f, 0.0f);
			Vector3 cameraRight = Vector3.Normalize(Vector3.Cross(worldUp, cameraDirection));
			Vector3 cameraUp = Vector3.Cross(cameraDirection, cameraRight);
			Matrix4 lookAtRotation = new Matrix4(
				  new Vector4(cameraRight, 0.0f)
				, new Vector4(cameraUp, 0.0f)
				, new Vector4(cameraDirection, 0.0f)
				, new Vector4(0, 0, 0, 1));

			return lookAtRotation;
		}



		public void UpdateInput(KeyboardState keyState, MouseState mouseState)
		{
			Vector3 cameraRight = Vector3.Normalize(Vector3.Cross(CameraFront, Up));

			// Position 
			if (keyState.IsKeyDown(key: Key.Up) || keyState.IsKeyDown(Key.W))
			{
				Position += CameraFront * Speed;
			}
			else if (keyState.IsKeyDown(Key.Down) || keyState.IsKeyDown(Key.S))
			{
				Position -= CameraFront * Speed;
			}

			if (keyState.IsKeyDown(key: Key.Left) || keyState.IsKeyDown(Key.A))
			{
				Position -= cameraRight * Speed;
			}
			else if (keyState.IsKeyDown(Key.Right) || keyState.IsKeyDown(Key.D))
			{
				Position += cameraRight * Speed;
			}

			if (keyState.IsKeyDown(key: Key.R))
			{
				Position += Up * Speed;
			}
			else if (keyState.IsKeyDown(Key.F))
			{
				Position -= Up * Speed;
			}
			

			// Direction

			// Rotation around Y is 0 when direction is x, z = (1,0)
			// It increases counter-clockwise

			float yawSpeedDegrees = 1.0f;

			if (keyState.IsKeyDown(key: Key.Q))
			{
				yaw += yawSpeedDegrees;
			}
			else if (keyState.IsKeyDown(Key.E))
			{
				yaw -= yawSpeedDegrees;
			}


			// Rotation around X is 0 when direction is y, z = (0, -1)
			// It increases when camera is turned up

			float pitchSpeedDegrees = 1.0f;

			if (keyState.IsKeyDown(key: Key.T))
			{
				pitch += pitchSpeedDegrees;
			}
			else if (keyState.IsKeyDown(Key.G))
			{
				pitch -= pitchSpeedDegrees;
			}

			// Mouse state

			int mouseMoveX = mouseState.X - lastMouseX;
			int mouseMoveY = lastMouseY - mouseState.Y;

			lastMouseX = mouseState.X;
			lastMouseY = mouseState.Y;

			float mouseSensitivity = 0.1f;
			float mouseMoveFX = mouseMoveX * mouseSensitivity;
			float mouseMoveFY = mouseMoveY * mouseSensitivity;

			yaw += mouseMoveFX;
			pitch += mouseMoveFY;

			pitch = MathHelper.Clamp(pitch, -89, 89);

			CameraFront = CalculateEulerDirection(pitch, yaw);

			cameraDebug++;
			if (cameraDebug > 120)
			{
				// Logger.LogInfo("Camera Dir: (" + Direction.X + ", " + Direction.Y + ", " + Direction.Z + ")");
				cameraDebug = 0;
			}
		}

		// Looking around
		public Vector3 CalculateEulerDirection(float pitch, float yaw)
		{
			double pitchAngle = Convert.ToDouble(pitch);
			double pitchRad = MathHelper.DegreesToRadians(pitchAngle);
			double cosPitch = Math.Cos(pitchRad); 
			double sinPitch = Math.Sin(pitchRad); 

			double yawAngle = Convert.ToDouble(yaw);
			double yawRad = MathHelper.DegreesToRadians(yawAngle);
			double cosYaw = Math.Cos(yawRad);
			double sinYaw = Math.Sin(yawRad);

			double directionX = cosPitch * cosYaw;
			double directionY = sinPitch;
			double directionZ = cosPitch * sinYaw;

			Vector3 direction =  new Vector3(Convert.ToSingle(directionX), Convert.ToSingle(directionY), Convert.ToSingle(directionZ));
			return Vector3.Normalize(direction);
		}

		// Changing frame
		public void SetFrame(int frame, float progress, List<PosAndDir> cameraFrames)
		{
			int firstFrame = frame;
			int secondFrame = firstFrame + 1;

			bool firstInFrames = firstFrame < cameraFrames.Count;
			bool secondInFrames = secondFrame < cameraFrames.Count;
			if (firstInFrames && secondInFrames)
			{
				Vector3 startPos = cameraFrames[firstFrame].position;
				Vector3 startDir = cameraFrames[firstFrame].direction;
				Vector3 targetPos = cameraFrames[secondFrame].position;
				Vector3 targetDir = cameraFrames[secondFrame].direction;

				Position = startPos * (1.0f - progress) + targetPos * (progress);
				Direction = startDir * (1.0f - progress) + targetDir * (progress);
			}
			else if (firstInFrames && !secondInFrames)
			{
				Position = cameraFrames[firstFrame].position;
				Direction = cameraFrames[firstFrame].direction;
			}
			else
			{
				// nop
			}
		}

		// Used for billboarding
		public Matrix4 GetRotationMatrix()
		{
			return LookAtRotation(Position, Direction);
		}
	}
}