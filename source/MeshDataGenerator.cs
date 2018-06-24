using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Linq;

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

			Material meshMaterial = new Material();

			OBJFileReader.readOBJ(filename, materialManager, ref faces, ref positions, ref normals, ref texCoords, ref meshMaterial);

			// Create positions 
			newData.VertexAmount = faces.Count;

			newData.hasPositionData = true;
			newData.hasTexCoordData = true;
			newData.hasNormalData = true;

			bool useIndices = true;

			List<OBJFileReader.OBJFace> uniqueFaces = null;
			Dictionary<OBJFileReader.OBJFace, int> uniqueFacesDict = null;
			

			if (useIndices)
			{
				uniqueFaces = new List<OBJFileReader.OBJFace>();
				uniqueFacesDict = new Dictionary<OBJFileReader.OBJFace, int>();
				newData.indices = new List<int>();
				newData.hasIndexData = true;
			}
			
			newData.positions = new List<Vector3>();
			newData.texCoords = new List<Vector2>();
			newData.normals = new List<Vector3>();

			bool addFace = true;

			foreach (OBJFileReader.OBJFace face in faces)
			{
				if (useIndices)
				{
					//OBJFileReader.OBJFinder finder = new OBJFileReader.OBJFinder(face);
					bool alreadyFound = uniqueFacesDict.ContainsKey(face);
					int faceIndex = -1;
					if (alreadyFound)
					{
						uniqueFacesDict.TryGetValue(face, out faceIndex);
						addFace = false;
					}
					else
					{
						faceIndex = uniqueFacesDict.Count;
						uniqueFacesDict.Add(face, faceIndex);

						// Add face info to arrays
						addFace = true;
					}
					newData.indices.Add(faceIndex);
				}
				
				// This is always true when not using indices
				if (addFace)
				{
					newData.positions.Add(positions[(int)face.positionIndex - 1]);
					newData.texCoords.Add(texCoords[(int)face.texCoordIndex - 1]);
					newData.normals.Add(normals[(int)face.normalIndex - 1]);
				}
			}

			Logger.LogInfo("Mesh Data read from " + filename);

			newData.drawType = MeshData.DataDrawType.Triangles;

			newData.GenerateBufferHandles();

			Error.checkGLError("Mesh Data created from file: " + filename);

			return newData;
		}

		static public MeshData CreateTriangleMesh()
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

		static public MeshData CreateQuadMesh(bool createNormals, bool createTexCoords)
		{
			MeshData quadMesh = new MeshData();
			quadMesh.sourceFileName = "quad";
			if (createNormals)
			{
				quadMesh.sourceFileName += "_normals";
			}
			if (createTexCoords)
			{
				quadMesh.sourceFileName += "_texCoords";
			}

			List<Vector3> positions = new List<Vector3>();

			positions.Add(new Vector3(-1.0f, -1.0f, 0.0f)); // 0
			positions.Add(new Vector3(-1.0f, 1.0f, 0.0f));  // 1
			positions.Add(new Vector3(1.0f, 1.0f, 0.0f));   // 2
			positions.Add(new Vector3(1.0f, -1.0f, 0.0f));  // 3

			quadMesh.positions = positions;
			quadMesh.hasPositionData = true;

			quadMesh.indices = new List<int> { 0, 1, 2, 2, 3, 0 };
			quadMesh.hasIndexData = true;

			if (createTexCoords)
			{
				List<Vector2> texCoords = new List<Vector2>();

				texCoords.Add(new Vector2(0.0f, 0.0f));
				texCoords.Add(new Vector2(0.0f, 1.0f));
				texCoords.Add(new Vector2(1.0f, 1.0f));
				texCoords.Add(new Vector2(1.0f, 0.0f));
				quadMesh.texCoords = texCoords;
				quadMesh.hasTexCoordData = true;
			}

			if (createNormals)
			{
				List<Vector3> normals = new List<Vector3>();
				normals.Add(new Vector3(0.0f, 0.0f, 1.0f));
				normals.Add(new Vector3(0.0f, 0.0f, 1.0f));
				normals.Add(new Vector3(0.0f, 0.0f, 1.0f));
				normals.Add(new Vector3(0.0f, 0.0f, 1.0f));
				quadMesh.normals = normals;
				quadMesh.hasNormalData = true;
			}

			quadMesh.VertexAmount = positions.Count;

			quadMesh.drawType = MeshData.DataDrawType.Triangles;

			quadMesh.GenerateBufferHandles();

			Error.checkGLError("Quad Mesh Data creation");

			return quadMesh;
		}
		static public MeshData CreatePyramidMesh(float baseWidth, float height, bool createNormals, bool createTexCoords)
		{
			MeshData pyraMesh = new MeshData();
			pyraMesh.sourceFileName = "pyramid";
			if (createNormals)
			{
				pyraMesh.sourceFileName += "_normals";
			}
			if (createTexCoords)
			{
				pyraMesh.sourceFileName += "_texCoords";
			}

			List<Vector3> positions = new List<Vector3>();

			// Base
			float hw = baseWidth / 2.0f;
			positions.Add(new Vector3(hw, 0.0f, -hw));	// 0
			positions.Add(new Vector3(hw,  0.0f, hw));	// 1
			positions.Add(new Vector3(-hw,  0.0f, -hw));	// 2
			positions.Add(new Vector3(-hw, 0.0f, hw));	// 3
			positions.Add(new Vector3(0.0f, height, 0.0f)); // 4 peak

			pyraMesh.positions = positions;
			pyraMesh.VertexAmount = positions.Count;
			pyraMesh.hasPositionData = true;

			pyraMesh.indices = new List<int> { 
			0, 1, 2		// base 
			, 2, 1, 3	// base 
			, 0, 1, 4
			, 1, 3, 4
			, 2, 3, 4
			, 2, 0, 4 };

			pyraMesh.hasIndexData = true;

			if (createTexCoords)
			{
				List<Vector2> texCoords = new List<Vector2>();
				texCoords.Add(new Vector2(0.0f, 0.0f));
				texCoords.Add(new Vector2(0.0f, 1.0f));
				texCoords.Add(new Vector2(1.0f, 0.0f));
				texCoords.Add(new Vector2(1.0f, 1.0f));
				texCoords.Add(new Vector2(0.5f, 1.0f));
				pyraMesh.texCoords = texCoords;
				pyraMesh.hasTexCoordData = true;
			}

			if (false && createNormals)
			{
				List<Vector3> normals = new List<Vector3>();
				// no idea?? 
				normals.Add(new Vector3());
				normals.Add(new Vector3());
				normals.Add(new Vector3());
				pyraMesh.hasNormalData = false;
			}

			pyraMesh.drawType = MeshData.DataDrawType.Triangles;

			pyraMesh.GenerateBufferHandles();

			Error.checkGLError("Quad Mesh Data creation");

			return pyraMesh;
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

		static public MeshData CreateTerrain(float width, float depth, float trianglesPerUnit)
		{
			MeshData terrain = new MeshData();
			terrain.sourceFileName = "Terrain" + width + "x" + depth;
			terrain.positions = new List<Vector3>();
			terrain.texCoords = new List<Vector2>();
			terrain.indices = new List<int>();

			int quadsWidth = (int)Math.Floor(trianglesPerUnit * width);
			int quadsDepth = (int)Math.Floor(trianglesPerUnit * depth);
			int widthVertices = quadsWidth * 2;
			int depthVertices = quadsDepth * 2;
			float triangleSideWidth = width / (quadsWidth);
			float triangleSideDepth = depth / (quadsDepth);
			
			/* Quad rows  -> width   V depth
			 *		0  (1 4) 5
			 *		2  (3 6) 7
			 *      8  (9
			 *      10 (11
			 */

			float widthStartPos = -(width / 2);
			float depthStartPos = -(depth / 2);
			int indice = 0;
	
			for (int qd = 0; qd < quadsDepth; qd++)
			{
				for (int qw = 0; qw < quadsWidth; qw++)
				{
					// 0
					float xCoord = widthStartPos + qw * triangleSideWidth;
					float zCoord = depthStartPos + qd * triangleSideDepth;

					//Logger.LogInfo("Terrain piece (" + qw + "," + qd + " at: " + xCoord + ", " + zCoord + " Size: " + triangleSideWidth + ", " + triangleSideDepth);

					terrain.positions.Add(new Vector3(xCoord
											, 0.0f
											, zCoord));
					terrain.texCoords.Add(new Vector2(0.0f, 0.0f));

					// 1
					terrain.positions.Add(new Vector3(xCoord + triangleSideWidth
										, 0.0f
										, zCoord));
					terrain.texCoords.Add(new Vector2(1.0f, 0.0f));

					// 2
					terrain.positions.Add(new Vector3(xCoord
											, 0.0f
											, zCoord + triangleSideDepth));
					terrain.texCoords.Add(new Vector2(0.0f, 1.0f));

					// 3
					terrain.positions.Add(new Vector3(xCoord + triangleSideWidth
										, 0.0f
										, zCoord + triangleSideDepth));
					terrain.texCoords.Add(new Vector2(1.0f, 1.0f));

					terrain.indices.Add(indice);
					terrain.indices.Add(indice + 1);
					terrain.indices.Add(indice + 2);
					terrain.indices.Add(indice + 3);
					terrain.indices.Add(indice + 2);
					terrain.indices.Add(indice + 1);
					indice += 4;
				}
			}

			terrain.hasPositionData = true;
			terrain.hasIndexData = true;
			terrain.hasTexCoordData = true;
			terrain.VertexAmount = widthVertices * depthVertices;
			terrain.drawType = MeshData.DataDrawType.Triangles;

			terrain.GenerateBufferHandles();
			Error.checkGLError("Terrain Mesh Data creation");
			return terrain;
		}
	}
}