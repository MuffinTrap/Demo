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


		private float rotationX;
		private float rotationY;
		private float rotationZ;

		private Matrix4 rotationMatrix;

		public float Scale { get; set; }

		public TransformComponent()
		{
			worldMatrix = new Matrix4Uniform(ShaderAttribute.getUniformName(ShaderUniformName.WorldMatrix));
			worldMatrix.Matrix = Matrix4.Identity;
			rotationMatrix = Matrix4.Identity;

			Scale = 1.0f;
			rotationX = 0.0f;
			rotationY = 0.0f;
			rotationZ = 0.0f;
		}

		public TransformComponent(Vector3 position) : this()
		{
			WorldPosition = position;
		}

		public TransformComponent(Vector3 position, float scale) : this()
		{
			WorldPosition = position;
			Scale = scale;
		}

		public Vector3 WorldPosition
		{
			get; set;
		}

		private Matrix4 CreateRotationMatrixFromAxisAngle()
		{
			return Matrix4.CreateRotationX(rotationX) * Matrix4.CreateRotationY(rotationY) * Matrix4.CreateRotationZ(rotationZ);
		}


		public void UpdateWorldMatrix()
		{
			Matrix4 T = Matrix4.CreateTranslation(WorldPosition);
			Matrix4 R = rotationMatrix;
			Matrix4 S = Matrix4.CreateScale(Scale);
			worldMatrix.Matrix = S * R * T;
		}

		public void rotateAroundY(float speed)
		{
			rotationY += speed;
			if (rotationY > MathHelper.TwoPi)
			{
				rotationY = 0.0f;
			}
		}

		public void setLocationAndScale(Vector3 position, float scale)
		{
			WorldPosition = position;
			Scale = scale;
		}

		public void SetRotationMatrix(Matrix4 rotation)
		{
			rotationMatrix = rotation;
		}

	}
}