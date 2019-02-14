using System;

using OpenTK;

namespace MuffinSpace
{
	public class TransformComponent
	{
		public Matrix4Uniform worldMatrix;
		public Matrix4 rotationTransformMatrix;
		public Vector4 Translation { get; set; }
		public Vector4 Direction { get; set; }
		public float Scale { get; set; }

		public TransformComponent Parent { get; set; }

		private float rotationAngle;
		private Vector3 rotationAxis;

		private Matrix4 rotationMatrix;

		public TransformComponent()
		{
			worldMatrix = new Matrix4Uniform(ShaderUniformName.WorldMatrix);
			worldMatrix.Matrix = Matrix4.Identity;
			rotationMatrix = Matrix4.Identity;

			Scale = 1.0f;
			rotationAxis = new Vector3(0.0f, 1.0f, 0.0f);
			rotationAngle = 0.0f;

			Parent = null;
		}

		public TransformComponent(Vector3 translationParam) : this()
		{
			Translation = new Vector4(translationParam, 1);
		}

		public TransformComponent(Vector3 translationParam, float scale) : this()
		{
			Translation = new Vector4(translationParam, 1);
			Scale = scale;
		}

		public Matrix4 CreateRotationMatrixFromAxisAngle()
		{
			return Matrix4.CreateFromAxisAngle(rotationAxis, rotationAngle);
		}

		public Vector3 GetWorldPosition()
		{
			UpdateWorldMatrix();
			return new Vector3(rotationTransformMatrix * Translation);
		}

		public void UpdateWorldMatrix()
		{
			Matrix4 T = Matrix4.CreateTranslation(Translation.Xyz);
			Matrix4 R = CreateRotationMatrixFromAxisAngle();
			Matrix4 S = Matrix4.CreateScale(Scale);
			rotationTransformMatrix = R * T;
			worldMatrix.Matrix = S * rotationTransformMatrix;

			if (Parent != null)
			{
				Parent.UpdateWorldMatrix();
				worldMatrix.Matrix = worldMatrix.Matrix * Parent.rotationTransformMatrix;
			}
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

		public void SetRotationMatrix(Matrix4 rotation)
		{
			rotationMatrix = rotation;
		}
	}
}