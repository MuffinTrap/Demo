using OpenTK;
using OpenTK.Input;

using System;

namespace OpenTkConsole
{
	public class CameraComponent
	{
		// Camera
		public Vector3 Position {
			get;
			set;
			}

		public Vector3 Direction{
			get;
			set;
		}
		public Vector3 Up
		{
		get;
		set;
		}

		public float speed;

		private float yaw = 180.0f;
		private float pitch = 0.0f;

		private Matrix4Uniform viewMatrix;
		private Matrix4Uniform projectionMatrix;

		public CameraComponent()
		{
			// IN OPENGL THE POSITIVE Z IS TOWARDS YOU
			Position = new Vector3(0, 0, 0.0f);

			// THE NEGATIVE IS TO THE DEPTH
			Direction = new Vector3(0, 0, 1.0f);
			Up = new Vector3(0.0f, 1.0f, 0.0f);



			speed = 0.05f;

			viewMatrix = new Matrix4Uniform(ShaderAttribute.getUniformName(ShaderUniformName.ViewMatrix));
			projectionMatrix = new Matrix4Uniform(ShaderAttribute.getUniformName(ShaderUniformName.ProjectionMatrix));

			projectionMatrix.Matrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 16.0f / 9.0f, 0.1f, 100f);
		}

		public void SetTarget(Vector3 targetPosition)
		{
			Direction = Vector3.Normalize(targetPosition - Position);
		}

		public void setMatrices(ShaderProgram program)
		{
			viewMatrix.Matrix = GetViewMatrix();
			viewMatrix.Set(program);
			projectionMatrix.Set(program);
		}

		public Matrix4 GetViewMatrix()
		{
			return LookAtDirection(Position, Direction, Up);
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

			cameraCoordinates.Transpose();
			Matrix4 translationInverse = Matrix4.CreateTranslation(position).Inverted();
			return translationInverse * cameraCoordinates;
		}

		public Matrix4 LookAtTarget(Vector3 position, Vector3 target, Vector3 up)
		{
			Vector3 direction = Vector3.Normalize(target - position);
			return LookAtDirection(position, direction, up);
		}

		public void Update(KeyboardState keyState)
		{
			Vector3 rightX = Vector3.Normalize(Vector3.Cross(Up, Direction));

			// Position 
			if (keyState.IsKeyDown(key: Key.Up) || keyState.IsKeyDown(Key.W))
			{
				Position -= Direction * speed;
			}
			else if (keyState.IsKeyDown(Key.Down) || keyState.IsKeyDown(Key.S))
			{
				Position += Direction * speed;
			}

			if (keyState.IsKeyDown(key: Key.Left) || keyState.IsKeyDown(Key.A))
			{
				Position -= rightX * speed;
			}
			else if (keyState.IsKeyDown(Key.Right) || keyState.IsKeyDown(Key.D))
			{
				Position += rightX * speed;
			}

			if (keyState.IsKeyDown(key: Key.R))
			{
				Position += Up * speed;
			}
			else if (keyState.IsKeyDown(Key.F))
			{
				Position -= Up * speed;
			}
			

			// Direction


			float yawSpeedDegrees = 1.0f;

			if (keyState.IsKeyDown(key: Key.Q))
			{
				yaw -= yawSpeedDegrees;
			}
			else if (keyState.IsKeyDown(Key.E))
			{
				yaw += yawSpeedDegrees;
			}



			float pitchSpeedDegrees = 1.0f;

			if (keyState.IsKeyDown(key: Key.T))
			{
				pitch -= pitchSpeedDegrees;
			}
			else if (keyState.IsKeyDown(Key.G))
			{
				pitch += pitchSpeedDegrees;
			}

			pitch = MathHelper.Clamp(pitch, -89, 89);

			Direction = CalculateEulerDirection(pitch, yaw);
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


			Vector3 lookDir = new Vector3(Convert.ToSingle(directionX), Convert.ToSingle(directionY), Convert.ToSingle(directionZ));
			return Vector3.Normalize(lookDir);
		}
	}

}