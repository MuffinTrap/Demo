using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OpenTkConsole
{
	public class TransformComponent
	{
		public Matrix4Uniform worldMatrix;
		public Vector3 Position { get; set; }
		private Vector3 direction;
		public Vector3 Direction
		{
			get
			{
				Vector4 defaultRot = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
				Vector4 applied = rotationMatrix * defaultRot;
				return new Vector3(applied.X, applied.Y, applied.Z);
			}
			set
			{
				direction = value.Normalized(); 
			}
		}
		public float Scale { get; set; }

		private float rotationX;
		private float rotationY;
		private float rotationZ;

		private float orbitAngle = 0.0f;

		private Matrix4 rotationMatrix;


		public TransformComponent()
		{
			worldMatrix = new Matrix4Uniform(ShaderUniformName.WorldMatrix);
			worldMatrix.Matrix = Matrix4.Identity;
			rotationMatrix = Matrix4.Identity;

			Scale = 1.0f;
			rotationX = 0.0f;
			rotationY = 0.0f;
			rotationZ = 0.0f;
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
			return Matrix4.CreateRotationX(rotationX) * Matrix4.CreateRotationY(rotationY) * Matrix4.CreateRotationZ(rotationZ);
		}


		public void UpdateWorldMatrix()
		{
			Matrix4 T = Matrix4.CreateTranslation(Position);
			Matrix4 R = CreateRotationMatrixFromAxisAngle();
			Matrix4 S = Matrix4.CreateScale(Scale);
			worldMatrix.Matrix = S * R * T;
		}

		public void setRotationX(float rotation)
		{
			rotationX = rotation;
			rotationMatrix = CreateRotationMatrixFromAxisAngle();
		}

		public void rotateAroundY(float speed)
		{
			rotationY += speed;
			if (rotationY > MathHelper.TwoPi)
			{
				rotationY = 0.0f;
			}
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