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
	public class Mesh
	{
		public int VAOHandle;
		public int BufferHandle;
        public int IndexBufferHandle;
		public int VertexAmount;

		public enum vertexType
		{
			posNorTex,
			posTex
		}

		public vertexType usedVertex;

		private float rotationY;

		public string MeshName { get; set; }

		public const int bytesPerFloat = 4;
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
		
		public static int getElementsInNormal() { return getElementsInPosition(); }

		public static int getPositionSizeBytes()
		{
			return getElementsInPosition() * bytesPerFloat;
		}

		public static int getTexCoordSizeBytes()
		{
			return getElementsInTexCoord() * bytesPerFloat;
		}

		public static int getDiffuseColorSizeBytes()
		{
			return getElementsInDiffuseColor() * bytesPerFloat;
		}

		public static int getNormalSizeBytes() { return getElementsInNormal() * bytesPerFloat; }

		public int getVertexSize()
		{
			int vertexSize = 0;
			switch (usedVertex)
			{
				case vertexType.posNorTex:
					vertexSize = getPositionSizeBytes() + getNormalSizeBytes() + getTexCoordSizeBytes();
					break;

				case vertexType.posTex:
					vertexSize = getPositionSizeBytes() + getTexCoordSizeBytes();
					break;
			}

			return vertexSize;
		}

		public struct PosNorTexVertex
		{
			Vector3 position;
			Vector3 normal;
			Vector2 texCoord;

			public PosNorTexVertex(Vector3 pos, Vector3 nor, Vector2 uv)
			{
				position = pos;
				normal = nor;
				texCoord = uv;
			}
		}

		public struct PosTexVertex
		{
			Vector3 position;
			Vector2 texCoord;

			public PosTexVertex(Vector3 pos, Vector2 uv)
			{
				position = pos;
				texCoord = uv;
			}
		}

		public struct AttributeLocations
		{
			public int positionLocation;
			public int normalLocation;
			public int texCoordLocation;
			public int diffuseColorLocation;
		}

		public Matrix4Uniform worldMatrix;

		// RenderingComponent

		public float Scale { get; set; }
		//
	
		public MaterialManager.Material MeshMaterial { get; set; }

		// TransformComponent	
		public Vector3 WorldPosition 
		{
			get; set;
		}

		private void UpdateWorldMatrix()
		{
			Matrix4 T = Matrix4.CreateTranslation(WorldPosition);
			Matrix4 R = Matrix4.CreateRotationY(rotationY);
			Matrix4 S = Matrix4.CreateScale(Scale);
			worldMatrix.Matrix = T * S * R;
		}

		//

		static public Mesh CreateTriangleMesh(AssetManager assetManager, AttributeLocations locations)
		{
			Mesh triMesh = new Mesh();
			// positions

			List<PosNorTexVertex> vertices = new List<PosNorTexVertex>();
			Vector3 normal = new Vector3(0.0f, 0.0f, 1.0f);
			vertices.Add(new PosNorTexVertex(new Vector3(-1f, 1f, 0.0f), normal, new Vector2(0.0f,1.0f)));
			vertices.Add(new PosNorTexVertex(new Vector3(1f, 1f, 0.0f), normal, new Vector2(1.0f, 1.0f)));
			vertices.Add(new PosNorTexVertex(new Vector3(0.0f, 0.0f, 0.0f), normal, new Vector2(0.5f, 0.0f)));

			triMesh.VertexAmount = vertices.Count;

			GL.BindVertexArray(triMesh.VAOHandle);
			GL.BindBuffer(BufferTarget.ArrayBuffer, triMesh.BufferHandle);
			GL.BufferData(BufferTarget.ArrayBuffer, triMesh.VertexAmount * triMesh.getVertexSize()
				, vertices.ToArray(), BufferUsageHint.StaticDraw);

			triMesh.enableAttributes(locations, triMesh.VertexAmount, triMesh.getVertexSize());

			triMesh.MeshMaterial = assetManager.GetMaterial("white");

			Error.checkGLError("Quad Mesh constructor");

			return triMesh;
		}

		static public Mesh CreateTexturedQuadMesh(AssetManager assetManager, Mesh.AttributeLocations locations)
		{
			Mesh quadMesh = new Mesh();

			List<PosTexVertex> vertices = new List<PosTexVertex>();

			vertices.Add(new PosTexVertex(new Vector3(-1f, -1f, 0.0f),		 new Vector2(0.0f, 0.0f)));
			vertices.Add(new PosTexVertex(new Vector3(-1f, 1f, 0.0f),		 new Vector2(1.0f, 0.0f)));
			vertices.Add(new PosTexVertex(new Vector3(1.0f, 1.0f, 0.0f),		 new Vector2(1.0f, 1.0f)));
																	
			vertices.Add(new PosTexVertex(new Vector3(1f, 1f, 0.0f),			 new Vector2(1.0f, 1.0f)));
			vertices.Add(new PosTexVertex(new Vector3(1f, -1f, 0.0f),		 new Vector2(1.0f, 0.0f)));
			vertices.Add(new PosTexVertex(new Vector3(-1.0f, -1.0f, 0.0f),	 new Vector2(0.0f, 0.0f)));

			quadMesh.VertexAmount = vertices.Count;
			
			GL.BindVertexArray(quadMesh.VAOHandle);
			GL.BindBuffer(BufferTarget.ArrayBuffer, quadMesh.BufferHandle);
			GL.BufferData(BufferTarget.ArrayBuffer, quadMesh.VertexAmount * quadMesh.getVertexSize()
				, vertices.ToArray(), BufferUsageHint.StaticDraw);

			quadMesh.enableAttributes(locations, quadMesh.VertexAmount, quadMesh.getVertexSize());

			Error.checkGLError("Quad Mesh constructor");

			return quadMesh;
		}
			
		public Mesh()
		{ 
			GenerateBufferHandles();
	
			Error.checkGLError("Mesh constructor");
			SetupDefaultTransform();
		}

		void GenerateBufferHandles()
		{
			BufferHandle = GL.GenBuffer();
			VAOHandle = GL.GenVertexArray();
		}
		
		void SetupDefaultTransform()
		{
			// Transformcomponent
			worldMatrix = new Matrix4Uniform("modelMatrix");
			worldMatrix.Matrix = Matrix4.Identity;

			// RenderingComponent
			Scale = 1.0f;
			rotationY = 0.0f;
		}

		public void enableAttributes(AttributeLocations locations, int vertexAmount, int vertexSizeBytes)
		{
            //  Vertex attributes

			// Check!
			if (locations.positionLocation == locations.normalLocation|| locations.normalLocation == locations.texCoordLocation)
			{
				Logger.LogError(Logger.ErrorState.Limited, "Data indices are same, buffering will fail.");
			}
			if (locations.positionLocation == -1)
			{
				Logger.LogError(Logger.ErrorState.Limited, "Position location is invalid.");
			}

			int runningOffset = 0;

            GL.VertexAttribPointer(index: locations.positionLocation, size: getElementsInPosition()
                , type: VertexAttribPointerType.Float
                , normalized: false, stride: vertexSizeBytes, offset: runningOffset);

			GL.EnableVertexAttribArray(locations.positionLocation);

			runningOffset += getPositionSizeBytes();


			if (locations.normalLocation != -1)
			{
				GL.VertexAttribPointer(index: locations.normalLocation, size: getElementsInNormal()
				, type: VertexAttribPointerType.Float
				, normalized: false, stride: vertexSizeBytes, offset: runningOffset );

				GL.EnableVertexAttribArray(locations.normalLocation);

				runningOffset += getNormalSizeBytes();
			}

			if (locations.texCoordLocation != -1)
			{
				GL.VertexAttribPointer(index: locations.texCoordLocation, size: getElementsInTexCoord()
			   , type: VertexAttribPointerType.Float
			   , normalized: false, stride: vertexSizeBytes, offset: runningOffset);

				GL.EnableVertexAttribArray(locations.texCoordLocation);

				runningOffset += getTexCoordSizeBytes();
			}
		   
			if (locations.diffuseColorLocation != -1)
			{
				GL.VertexAttribPointer(index: locations.diffuseColorLocation, size: getElementsInDiffuseColor()
				   , type: VertexAttribPointerType.Float
				   , normalized: false, stride: vertexSizeBytes, offset: runningOffset);

				GL.EnableVertexAttribArray(locations.diffuseColorLocation);

				runningOffset += getDiffuseColorSizeBytes();
			}
			
			Error.checkGLError("Mesh.enableAttributes");
		}

		public void updateUniforms(ShaderProgram shaderProgram)
		{
			UpdateWorldMatrix();
			worldMatrix.Set(shaderProgram);
			
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, MeshMaterial.textureGLIndex);

			Error.checkGLError("Mesh.updateUniforms");
		}

		public void rotate(float speed)
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

		// Reads on .obs file
		static public Mesh CreateFromFile(string filename, MaterialManager materialManager)
        {
            List<OBJFileReader.OBJFace> faces = new List<OBJFileReader.OBJFace>();

            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> texCoords = new List<Vector2>();
            MaterialManager.Material meshMaterial = null;

            OBJFileReader.readOBJ(filename, materialManager, ref faces, ref positions, ref normals, ref texCoords, ref meshMaterial);

            // Create positions 
            List<PosNorTexVertex> vertices = new List<PosNorTexVertex>(positions.Count);

            foreach (OBJFileReader.OBJFace face in faces)
            {
				vertices.Add( new PosNorTexVertex(positions[(int)face.positionIndex - 1],  normals[(int)face.normalIndex - 1], texCoords[(int)face.texCoordIndex - 1]));
            }

			Logger.LogInfo("Mesh read from " + filename);

			Mesh fileMesh = new Mesh();

			fileMesh.VertexAmount = vertices.Count;

			GL.BindVertexArray(fileMesh.VAOHandle);
			GL.BindBuffer(BufferTarget.ArrayBuffer, fileMesh.BufferHandle);
			GL.BufferData(BufferTarget.ArrayBuffer, fileMesh.VertexAmount * fileMesh.getVertexSize()
				, vertices.ToArray(), BufferUsageHint.StaticDraw);

			fileMesh.MeshMaterial = meshMaterial;

			Error.checkGLError("File Mesh constructor");

			return fileMesh;

		}
	}
}