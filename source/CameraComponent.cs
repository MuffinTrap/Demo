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

		public Vector3 Direction
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
		
		public float FOV { get; set; }

		public bool FreeMode { get; set; }

		private float yaw = 0.0f;
		private float pitch = 0.0f;

		private Matrix4Uniform viewMatrix;
		private Matrix4Uniform projectionMatrix;
		private Matrix4Uniform perspectiveMatrix;
		private Matrix4Uniform orthogonalMatrix;

		public CameraComponent()
		{
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

			float aspectRatio = 16.0f / 9.0f;
			float near = 0.1f;
			float far = 100.0f;
			perspectiveMatrix.Matrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, aspectRatio, near, far);
			orthogonalMatrix.Matrix = Matrix4.CreateOrthographic(aspectRatio, 1.0f, near, far);

			EnablePerspective();

			viewMatrix.Matrix = GetViewMatrix();

			Logger.LogInfo("Camera start. Dir: (" + Direction.X + ", " + Direction.Y + ", " + Direction.Z + ")");
		}

		public void SetTarget(Vector3 targetPosition)
		{
			Direction = Vector3.Normalize(Position - targetPosition);
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

		public Matrix4 GetViewMatrix()
		{
			Matrix4 cameraCoordinates = LookAtDirection(Position, Direction, Up);

			cameraCoordinates.Transpose();
			Matrix4 translationInverse = Matrix4.CreateTranslation(Position).Inverted();
			return translationInverse * cameraCoordinates;
		}


		public Matrix4 LookAtDirection(Vector3 position, Vector3 direction, Vector3 up)
		{
			// From gluLookAt
			Vector3 rightX = Vector3.Normalize(Vector3.Cross(up, direction));
			Vector3 upY = Vector3.Cross(direction, rightX);
			Matrix4 cameraCoordinates = new Matrix4(
				  new Vector4(rightX, 0.0f)
				, new Vector4(upY, 0.0f)
				, new Vector4(direction, 0.0f)
				, new Vector4(0, 0, 0, 1));

			
			return cameraCoordinates;
		}

		public Matrix4 LookAtTarget(Vector3 position, Vector3 target, Vector3 up)
		{
			Vector3 direction = Vector3.Normalize(target - position);
			return LookAtDirection(position, direction, up);
		}

		public Matrix4 GetRotationMatrix()
		{
			return LookAtDirection(Position, Direction, Up);
		}

		public void UpdateInput(KeyboardState keyState, MouseState mouseState)
		{
			Vector3 rightX = Vector3.Normalize(Vector3.Cross(Up, Direction));

			// Position 
			if (keyState.IsKeyDown(key: Key.Up) || keyState.IsKeyDown(Key.W))
			{
				Position -= Direction * Speed;
			}
			else if (keyState.IsKeyDown(Key.Down) || keyState.IsKeyDown(Key.S))
			{
				Position += Direction * Speed;
			}

			if (keyState.IsKeyDown(key: Key.Left) || keyState.IsKeyDown(Key.A))
			{
				Position -= rightX * Speed;
			}
			else if (keyState.IsKeyDown(Key.Right) || keyState.IsKeyDown(Key.D))
			{
				Position += rightX * Speed;
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
			int mouseMoveY = mouseState.Y - lastMouseY;

			lastMouseX = mouseState.X;
			lastMouseY = mouseState.Y;

			float mouseSensitivity = 0.1f;
			float mouseMoveFX = mouseMoveX * mouseSensitivity;
			float mouseMoveFY = mouseMoveY * mouseSensitivity;

			yaw += mouseMoveFX;
			pitch += mouseMoveFY;

			pitch = MathHelper.Clamp(pitch, -89, 89);

			Direction = CalculateEulerDirection(pitch, yaw);

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
			double px = Math.Cos(pitchRad);
			double py = Math.Sin(pitchRad);
			double pz = px;

			double yawAngle = Convert.ToDouble(yaw);
			double yawRad = MathHelper.DegreesToRadians(yawAngle);
			double yx = Math.Cos(yawRad);
			double yz = Math.Sin(yawRad);

			double directionX = px * yx;
			double directionY = py;
			double directionZ = pz * yz;

			Vector3 lookDir =  new Vector3(Convert.ToSingle(directionX), Convert.ToSingle(directionY), Convert.ToSingle(directionZ));
			return Vector3.Normalize(lookDir);
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
	}
}