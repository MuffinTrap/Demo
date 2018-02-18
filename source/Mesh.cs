using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTkConsole
{
	public class DrawableMesh
	{
		public string Name { get; set; }

		public MeshData Data { get; set; }

		public TransformComponent Transform { get; set; }

		public ShaderProgram ShaderProgram { get; set; }

		public MaterialManager.Material BoundMaterial { get; set; }
		
		public DrawableMesh(string name, MeshData data, List<ShaderAttribute> attributes, TransformComponent transform, MaterialManager.Material material, ShaderProgram shader)
		{
			Name = name;
			Data = data;
			Transform = transform;
			BoundMaterial = material;
			ShaderProgram = shader;

			Data.loadToVAO(attributes);
			
			Error.checkGLError("Mesh constructor of " + name);
		}


		public void draw()
		{
			if (Transform != null)
			{
				Transform.UpdateWorldMatrix();
				Transform.worldMatrix.Set(ShaderProgram);
			}

			if (BoundMaterial != null)
			{
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, BoundMaterial.textureGLIndex);
			}

			GL.BindVertexArray(Data.VAOHandle);

			PrimitiveType beginType = PrimitiveType.Triangles;

			switch (Data.drawType)
			{
				case MeshData.DataDrawType.Triangles:
					beginType = PrimitiveType.Triangles;
					break;
				case MeshData.DataDrawType.Lines:
					beginType = PrimitiveType.Lines;
					break;
				case MeshData.DataDrawType.Points:
					beginType = PrimitiveType.Points;
					GL.PointSize(5.0f);
					break;
			}

			if (Data.hasIndexData)
			{
				int bufferOffset = 0;
				GL.DrawElements(beginType, Data.indices.Count, DrawElementsType.UnsignedInt, bufferOffset);
			}
			else
			{
				GL.DrawArrays(beginType, 0, Data.VertexAmount);
			}
			

			GL.BindVertexArray(0);
			GL.BindTexture(TextureTarget.Texture2D, 0);

			Error.checkGLError("DrawableMesh.draw " + Name);
		}
	}
}