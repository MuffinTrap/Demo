using System;

using OpenTK;

namespace MuffinSpace
{
	public class TransformComponent
	{
		public Matrix4Uniform worldMatrix;
		public Vector3 Position { get; set; }
		public Vector3 Direction { get; set; }
		public float Scale { get; set; }

		private float rotationAngle;
		private Vector3 rotationAxis;

		private float orbitAngle = 0.0f;

		private Matrix4 rotationMatrix;

		public TransformComponent()
		{
			worldMatrix = new Matrix4Uniform(ShaderUniformName.WorldMatrix);
			worldMatrix.Matrix = Matrix4.Identity;
			rotationMatrix = Matrix4.Identity;

			Scale = 1.0f;
			rotationAxis = new Vector3(0.0f, 1.0f, 0.0f);
			rotationAngle = 0.0f;
		}

		public TransformComponent(Vector3 position) : this()
		{
			Position = position;
		}

		public TransformComponent(Vector3 position, float scale) : this()
		{
			Position = position;
			Scale = scale;
		}

		private Matrix4 CreateRotationMatrixFromAxisAngle()
		{
			return Matrix4.CreateFromAxisAngle(rotationAxis, rotationAngle);
		}

		public void UpdateWorldMatrix()
		{
			Matrix4 T = Matrix4.CreateTranslation(Position);
			Matrix4 R = CreateRotationMatrixFromAxisAngle();
			Matrix4 S = Matrix4.CreateScale(Scale);
			worldMatrix.Matrix = S * R * T;
		}

		public void SetRotationAxis(Vector3 axis)
		{
			rotationAxis = axis;
		}

		public void SetRotation(float angle)
		{
			rotationAngle = angle;
			if (rotationAngle > MathHelper.TwoPi)
			{
				rotationAngle -= MathHelper.TwoPi;
			}
			else if (rotationAngle < 0.0f)
			{
				rotationAngle += MathHelper.TwoPi;
			}
		}

		public void rotateAroundY(float speed)
		{
		}

		public void Orbit(float speed, float height, float distance, Vector3 targetPoint)
		{
			orbitAngle += speed;
			const float fullCircle = (float)(Math.PI * 2.0f);
			if (orbitAngle > fullCircle)
			{
				orbitAngle -= fullCircle;
			}
			if (orbitAngle < 0)
			{
				orbitAngle += fullCircle;
			}
			Matrix4 rot = Matrix4.CreateRotationY(orbitAngle);
			Position = targetPoint + Vector3.TransformVector(new Vector3(0, height, distance), rot);
		}

		public void setLocationAndScale(Vector3 position, float scale)
		{
			Position = position;
			Scale = scale;
		}

		public void SetRotationMatrix(Matrix4 rotation)
		{
			rotationMatrix = rotation;
		}
	}
}