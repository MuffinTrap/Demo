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
	class Mesh
	{
		public int VAOHandle;
		public int BufferHandle;
		public int VertexAmount;

		private float rotationY;
		
		public struct PosTexVertex
		{
			Vector3 position;
			Vector2 texCoord;

			public PosTexVertex(Vector3 pos, Vector2 uv)
			{
				position = pos;
				texCoord = uv;
			}

			const int bytesPerFloat = 4;

			public static int getPositionSizeBytes()
			{
				return getElementsInPosition() * bytesPerFloat;
			}

			public static int getTexCoordSizeBytes()
			{
				return getElementsInTexCoord() * bytesPerFloat;
			}

			public static int getElementsInPosition()
			{
				return 3;
			}

			public static int getElementsInTexCoord()
			{
				return 2;
			}
		}

		private List<PosTexVertex> rawVertices;
		
		public Matrix4Uniform worldMatrix;

		public static int PositionDataIndex { get; set; }
		public static int TexCoordDataIndex { get; set; }
		public static int ColorDataIndex { get; set; }
		public static int ScaleDataIndex { get; set; }

		// RenderingComponent
		public Color4 DiffuseColor { get; set; }

		public float Scale { get; set; }
		//

	
		public MaterialManager.Material MeshMaterial { get; set; }

		// TransformComponent
		private Vector3 worldPosition;
		
		public Vector3 WorldPosition 
		{
			get
			{
				return worldPosition;
			}
			set
			{
				worldPosition = value;
				worldMatrix.Matrix = Matrix4.CreateTranslation(worldPosition);
			}	
		}

		//

		static public Mesh CreateTriangleMesh()
		{
			// positions

			List<PosTexVertex> vertices = new List<PosTexVertex>(3);
			vertices.Add(new PosTexVertex(new Vector3(-1f, 1f, 0.0f), new Vector2(0.0f,1.0f)));
			vertices.Add(new PosTexVertex(new Vector3(1f, 1f, 0.0f), new Vector2(1.0f, 1.0f)));
			vertices.Add(new PosTexVertex(new Vector3(0.0f, 0.0f, 0.0f), new Vector2(0.5f, 0.0f)));

			return new Mesh(vertices, MaterialManager.getMaterialByName("white"));
		}
			
		public Mesh(List<PosTexVertex> vertices, MaterialManager.Material meshMaterial)
		{
			BufferHandle = GL.GenBuffer();
			VAOHandle = GL.GenVertexArray();
			
			VertexAmount = vertices.Count;
			
			Error.checkGLError("Mesh constructor");

			 rawVertices = vertices;

			MeshMaterial = meshMaterial;
			 
			 // Transformcomponent
			 worldMatrix = new Matrix4Uniform("modelMatrix");
			 worldMatrix.Matrix = Matrix4.Identity;

			// RenderingComponent
			Scale = 1.0f;
			DiffuseColor = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
			rotationY = 0.0f;

		}
		
		public void bufferData()
		{
			int vertexSize = PosTexVertex.getPositionSizeBytes() + PosTexVertex.getTexCoordSizeBytes();


			GL.BindVertexArray(VAOHandle);
			
			GL.BindBuffer(BufferTarget.ArrayBuffer, BufferHandle);
			
			GL.BufferData(BufferTarget.ArrayBuffer, VertexAmount * vertexSize, rawVertices.ToArray(), BufferUsageHint.StaticDraw);

            //  Vertex attributes

            GL.VertexAttribPointer(index: PositionDataIndex, size: PosTexVertex.getElementsInPosition()
                , type: VertexAttribPointerType.Float
                , normalized: false, stride: vertexSize, offset: 0);

			GL.VertexAttribPointer(index: TexCoordDataIndex, size: PosTexVertex.getElementsInTexCoord()
		   , type: VertexAttribPointerType.Float
		   , normalized: false, stride: vertexSize, offset: PosTexVertex.getPositionSizeBytes());

			GL.EnableVertexAttribArray(PositionDataIndex);
			GL.EnableVertexAttribArray(TexCoordDataIndex);
			
			Error.checkGLError("Mesh.bufferData");
		}

		public void updateUniforms(ShaderProgram shaderProgram)
		{
			worldMatrix.Set(shaderProgram);
			
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, MeshMaterial.textureGLIndex);

			GL.Uniform4(ColorDataIndex, DiffuseColor);
			GL.Uniform1(ScaleDataIndex, Scale);

			Error.checkGLError("Mesh.updateUniforms");
		}

		public void rotate(float speed)
		{
			rotationY += speed;
			if (rotationY > MathHelper.TwoPi)
			{
				rotationY = 0.0f;
			}
			worldMatrix.Matrix = Matrix4.CreateRotationY(rotationY);
		}

		// Reads on .obs file
		static public Mesh CreateFromFile(string filename)
		{
			List<OBJFileReader.OBJFace> faces = new List<OBJFileReader.OBJFace>();

			List<Vector3> positions = new List<Vector3>();
			List<Vector3> normals = new List<Vector3>();
			List<Vector2> texCoords = new List<Vector2>();
			MaterialManager.Material meshMaterial = null;
			

			OBJFileReader.readOBJ(filename, ref faces, ref positions, ref normals, ref texCoords, ref meshMaterial);

			// Create positions 
			List<PosTexVertex> vertices = new List<PosTexVertex>(positions.Count);

			Console.WriteLine("Mesh read from " + filename);
			foreach (OBJFileReader.OBJFace face in faces)
			{
				vertices.Add( new PosTexVertex(positions[face.positionIndex - 1], texCoords[face.texCoordIndex -1]));
			}

			return new Mesh(vertices, meshMaterial);
		}
	}
}