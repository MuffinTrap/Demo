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
	class MeshDataGenerator
	{
		// Reads on .obj file
		static public MeshData CreateFromFile(string filename, MaterialManager materialManager)
		{
			MeshData newData = new MeshData();

			List<Vector3> positions = new List<Vector3>();
			List<Vector3> normals = new List<Vector3>();
			List<Vector2> texCoords = new List<Vector2>();

			List<OBJFileReader.OBJFace> faces = new List<OBJFileReader.OBJFace>();

			MaterialManager.Material meshMaterial = new MaterialManager.Material();

			OBJFileReader.readOBJ(filename, materialManager, ref faces, ref positions, ref normals, ref texCoords, ref meshMaterial);

			// Create positions 
			newData.VertexAmount = faces.Count;

			newData.positions = new List<Vector3>();
			newData.texCoords = new List<Vector2>();
			newData.normals = new List<Vector3>();

			newData.hasPositionData = true;
			newData.hasTexCoordData = true;
			newData.hasNormalData = true;


			foreach (OBJFileReader.OBJFace face in faces)
			{
				newData.positions.Add(positions[(int)face.positionIndex - 1]);
				newData.texCoords.Add(texCoords[(int)face.texCoordIndex - 1]);
				newData.normals.Add(normals[(int)face.normalIndex - 1]);
			}

			Logger.LogInfo("Mesh Data read from " + filename);

			newData.drawType = MeshData.DataDrawType.Triangles;

			newData.GenerateBufferHandles();

			Error.checkGLError("Mesh Data created from file: " + filename);

			return newData;
		}

		static public MeshData CreateTriangleMesh(AssetManager assetManager)
		{
			MeshData triMesh = new MeshData();
			triMesh.sourceFileName = "triangle";
			// positions

			triMesh.positions = new List<Vector3>();
			triMesh.positions.Add(new Vector3(-1f, 1f, 0.0f));
			triMesh.positions.Add(new Vector3(1f, 1f, 0.0f));
			triMesh.positions.Add(new Vector3(0.0f, 0.0f, 0.0f));

			triMesh.hasPositionData = true;

			triMesh.indices = new List<int> { 0, 1, 2 };
			triMesh.hasIndexData = true;

			triMesh.VertexAmount = 3;
			triMesh.drawType = MeshData.DataDrawType.Triangles;

			triMesh.GenerateBufferHandles();
			
			Error.checkGLError("Triangle Mesh Data creation");

			return triMesh;
		}

		static public MeshData CreateTexturedQuadMesh(AssetManager assetManager)
		{
			MeshData quadMesh = new MeshData();
			quadMesh.sourceFileName = "quad";

			List<Vector3> positions = new List<Vector3>();

			positions.Add(new Vector3(-1.0f, -1.0f, 0.0f));	// 0
			positions.Add(new Vector3(-1.0f,  1.0f, 0.0f));	// 1
			positions.Add(new Vector3( 1.0f,  1.0f, 0.0f));	// 2
			positions.Add(new Vector3( 1.0f, -1.0f, 0.0f));	// 3

			List<Vector2> texCoords = new List<Vector2>();

			texCoords.Add(new Vector2(0.0f, 0.0f));
			texCoords.Add(new Vector2(0.0f, 1.0f));
			texCoords.Add(new Vector2(1.0f, 1.0f));
			texCoords.Add(new Vector2(1.0f, 0.0f));

			quadMesh.positions = positions;
			quadMesh.texCoords = texCoords;

			quadMesh.hasPositionData = true;
			quadMesh.hasTexCoordData = true;

			quadMesh.indices = new List<int> { 0, 1, 2, 2, 3, 0};
			quadMesh.hasIndexData = true;

			quadMesh.VertexAmount = positions.Count;

			quadMesh.drawType = MeshData.DataDrawType.Triangles;

			quadMesh.GenerateBufferHandles();

			Error.checkGLError("Quad Mesh Data creation");

			return quadMesh;
		}

		static public MeshData CreateXZGrid(float width, float depth, float linesPerWidth, float linesPerDepth)
		{
			MeshData grid = new MeshData();
			grid.sourceFileName = "Grid" + width + "x" + depth;

			grid.positions = new List<Vector3>();

			for (float w = -(width / 2); w <= (width / 2); w += (1 / linesPerWidth))
			{
				for (float d = -(depth / 2); d <= (depth / 2); d += (1 / linesPerDepth))
				{
					grid.positions.Add(new Vector3(w, 0.0f, d));
				}
			}

			grid.hasPositionData = true;
			grid.VertexAmount = grid.positions.Count;
			grid.drawType = MeshData.DataDrawType.Points;

			grid.GenerateBufferHandles();

			Error.checkGLError("Grid Mesh Data creation");

			return grid;
		}
	}
}