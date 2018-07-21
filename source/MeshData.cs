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
	public class MeshData
	{
		public MeshData()
		{
			VAOHandle = GL.GenVertexArray();
		}

		public void GenerateBufferHandles()
		{
			if (hasPositionData)
			{
				positionBufferHandle = GL.GenBuffer();
			}
			if (hasTexCoordData)
			{
				texCoordBufferHandle = GL.GenBuffer();
			}
			if (hasNormalData)
			{
				normalBufferHandle = GL.GenBuffer();
			}
			if (hasIndexData)
			{
				indexBufferHandle = GL.GenBuffer();
			}
		}

		// Data and validity
		public List<Vector3> positions = null;
		public List<Vector3> normals = null;
		public List<Vector2> texCoords = null;
		public List<int> indices = null;

		public bool hasPositionData = false;
		public bool hasTexCoordData = false;
		public bool hasNormalData = false;
		public bool hasIndexData = false;

		public enum DataDrawType
		{
			Triangles,
			Lines,
			Points
		}

		public DataDrawType drawType = DataDrawType.Points;

		// OpenGL handles
		public int positionBufferHandle = -1;
		public int texCoordBufferHandle = -1;
		public int normalBufferHandle = -1;

		public int indexBufferHandle = -1;

		public string sourceFileName = "NO SOURCE";

		public string getInfoString()
		{
			return "Mesh " + sourceFileName + " Vertices :" + VertexAmount;
		}

		// Vertices
		public int VertexAmount { get; set; }
		public int VAOHandle;

		private bool checkAttribute(ShaderAttribute attribute)
		{
			if (attribute.location == -1)
			{
				Logger.LogError(Logger.ErrorState.Limited, "Location of attribute " + attribute.name + " is invalid");
				return false;
			}
			return true;
		}

		private ShaderAttribute getAttribute(ShaderAttributeName name, List<ShaderAttribute> attributes)
		{
			ShaderAttribute result = new ShaderAttribute();
			result.location = -1;

			foreach (ShaderAttribute a in attributes)
			{
				if (a.name.Equals(name))
				{
					if (checkAttribute(a))
					{
						result = a;
					}
				}
			}

			if (result.location == -1)
			{
				Logger.LogError(Logger.ErrorState.Limited, "Did not get attribute ");
			}
			return result;
		}

		private void enableAttribute(ShaderAttribute attr)
		{
			GL.VertexAttribPointer(index: attr.location, size: attr.elementSize
						, type: VertexAttribPointerType.Float
						, normalized: false, stride: 0, offset: 0);

			GL.EnableVertexAttribArray(attr.location);

			Error.checkGLError("MeshData.enableAttribute " + attr.name + " location:" + attr.location + " size: " + attr.elementSize);
		}

		public bool loadToVAO(List<ShaderAttribute> attributes)
		{
			bool success = false;

			GL.BindVertexArray(VAOHandle);
			{
				success = bufferData(attributes);
			}
			GL.BindVertexArray(0);

			return success;
		}

		private bool bufferData(List<ShaderAttribute> attributes)
		{
			if (hasPositionData)
			{
				GL.BindBuffer(BufferTarget.ArrayBuffer, positionBufferHandle);

				GL.BufferData(BufferTarget.ArrayBuffer, VertexAmount * MeshData.getPositionSizeBytes()
					, positions.ToArray(), BufferUsageHint.StaticDraw);

				ShaderAttribute aPosition = getAttribute(ShaderAttributeName.Position, attributes);
				if (checkAttribute(aPosition))
				{
					enableAttribute(aPosition);
				}

				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			}

			if (hasTexCoordData)
			{
				GL.BindBuffer(BufferTarget.ArrayBuffer, texCoordBufferHandle);

				GL.BufferData(BufferTarget.ArrayBuffer, VertexAmount * MeshData.getTexCoordSizeBytes()
					, texCoords.ToArray(), BufferUsageHint.StaticDraw);

				ShaderAttribute aTexCoord = getAttribute(ShaderAttributeName.TexCoord, attributes);
				if (checkAttribute(aTexCoord))
				{
					enableAttribute(aTexCoord);
				}
			}

			if (hasNormalData)
			{
				GL.BindBuffer(BufferTarget.ArrayBuffer, normalBufferHandle);

				GL.BufferData(BufferTarget.ArrayBuffer, VertexAmount * MeshData.getNormalSizeBytes()
					, normals.ToArray(), BufferUsageHint.StaticDraw);

				ShaderAttribute aNormal = getAttribute(ShaderAttributeName.Normal, attributes);
				if (checkAttribute(aNormal))
				{
					enableAttribute(aNormal);
				}
			}

			if (hasIndexData)
			{
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferHandle);
				GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * BytesPerFloat, indices.ToArray(), BufferUsageHint.StaticDraw);
			}

			Error.checkGLError("MeshData.bufferData " + sourceFileName);

			return true;
		}

		public List<ShaderAttributeName> GetNeededAttributes()
		{
			List<ShaderAttributeName> list = new List<ShaderAttributeName>();
			if (hasPositionData)
			{
				list.Add(ShaderAttributeName.Position);
			}
			if (hasNormalData)
			{
				list.Add(ShaderAttributeName.Normal);
			}
			if (hasTexCoordData)
			{
				list.Add(ShaderAttributeName.TexCoord);
			}
			return list;
		}

		public static int BytesPerFloat
		{
			get
			{
				return 4;
			}
		}

		public static int getElementsInPosition()
		{
			return 3;
		}

		public static int getElementsInTexCoord()
		{
			return 2;
		}

		public static int getElementsInDiffuseColor()
		{
			return 4;
		}

		public static int getElementsInNormal() 
		{ 
			return getElementsInPosition(); 
		}

		public static int getPositionSizeBytes()
		{
			return getElementsInPosition() * BytesPerFloat;
		}

		public static int getTexCoordSizeBytes()
		{
			return getElementsInTexCoord() * BytesPerFloat;
		}

		public static int getDiffuseColorSizeBytes()
		{
			return getElementsInDiffuseColor() * BytesPerFloat;
		}

		public static int getNormalSizeBytes() 
		{ 
			return getElementsInNormal() * BytesPerFloat; 
		}
	}
}