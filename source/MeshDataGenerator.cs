using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Linq;

namespace MuffinSpace
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

			Material meshMaterial = new Material("");

			OBJFileReader.readOBJ(filename, materialManager, ref faces, ref positions, ref normals, ref texCoords, ref meshMaterial);

			// Create positions 

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

			triMesh.indices = new List<int> { 0, 2, 1 };
			triMesh.hasIndexData = true;

			triMesh.drawType = MeshData.DataDrawType.Triangles;

			triMesh.GenerateBufferHandles();
			
			Error.checkGLError("Triangle Mesh Data creation");

			return triMesh;
		}
		static public MeshData CreateTextMesh(string message, PixelFont font)
		{
			MeshData textMesh = new MeshData();
			textMesh.sourceFileName = "text_mesh: " + message;

			textMesh.positions = new List<Vector3>();
			textMesh.texCoords = new List<Vector2>();
			textMesh.indices = new List<int>();

			textMesh.hasPositionData = true;
			textMesh.hasTexCoordData = true;
			textMesh.hasIndexData = true;

			List<Vector2> uvs = font.GetUVsOfString(message);
			Vector2 uvStep = font.GetLetterUVSize();
			int letters = message.Length;

			float xstart = 0.0f;
			float step = 1.0f;
			int ind = 0;
			for (int i = 0; i < letters; i++)
			{
				textMesh.positions.Add(new Vector3(xstart + 0.0f, 0.0f, 0.0f)); // 0  L B
				textMesh.positions.Add(new Vector3(xstart + 0.0f, step, 0.0f));  // 1 L T
				textMesh.positions.Add(new Vector3(xstart + step, step, 0.0f));   // 2  R T
				textMesh.positions.Add(new Vector3(xstart + step, 0.0f, 0.0f));  // 3	R B

				// We are given the upper left corner in uvs[]
				// but the quads start from lower left
				// Texture coordinates Y decreases downwards, top is 1
				float uX = uvs[i].X;		// L
				float uY = uvs[i].Y;		// T
				float uW = uX + uvStep.X;	// R
				float uH = uY - uvStep.Y;	// B

				textMesh.texCoords.Add(new Vector2(uX, uH));	// L B
				textMesh.texCoords.Add(new Vector2(uX, uY));	// L T
				textMesh.texCoords.Add(new Vector2(uW, uY));	// R T
				textMesh.texCoords.Add(new Vector2(uW, uH));	// R B

				textMesh.indices.Add(ind);
				textMesh.indices.Add(ind + 3);
				textMesh.indices.Add(ind + 2);

				textMesh.indices.Add(ind);
				textMesh.indices.Add(ind + 2);
				textMesh.indices.Add(ind + 1);

				ind += 4;

				xstart += step;
			}

			textMesh.drawType = MeshData.DataDrawType.Triangles;
			textMesh.GenerateBufferHandles();

			Error.checkGLError("Text Mesh Data creation");

			return textMesh;
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

			positions.Add(new Vector3(0.0f, 0.0f, 0.0f)); // 0
			positions.Add(new Vector3(0.0f, 1.0f, 0.0f));  // 1
			positions.Add(new Vector3(1.0f, 1.0f, 0.0f));   // 2
			positions.Add(new Vector3(1.0f, 0.0f, 0.0f));  // 3

			quadMesh.positions = positions;
			quadMesh.hasPositionData = true;

			quadMesh.indices = new List<int> { 0, 3, 2, 0, 2, 1 };
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


			quadMesh.drawType = MeshData.DataDrawType.Triangles;

			quadMesh.GenerateBufferHandles();

			Error.checkGLError("Quad Mesh Data creation");

			return quadMesh;
		}

		static private void CreateCubeSide(bool counterClockTriangles, Vector3 sideCenter, Vector3 up, Vector3 right, ref List<Vector3> positions
		, ref List<Vector3> normals, ref List<Vector2> texCoords, ref List<int> indices)
		{
			int ind = positions.Count;
			positions.Add(sideCenter - up - right);
			positions.Add(sideCenter - up + right);
			positions.Add(sideCenter + up + right);
			positions.Add(sideCenter + up - right);

			Vector3 sideNormal = sideCenter;
			if (!counterClockTriangles)
			{
				sideNormal *= -1.0f;
			}
			normals.Add(sideNormal);
			normals.Add(sideNormal);
			normals.Add(sideNormal);
			normals.Add(sideNormal);

			texCoords.Add(new Vector2(0.0f, 0.0f));
			texCoords.Add(new Vector2(1.0f, 0.0f));
			texCoords.Add(new Vector2(1.0f, 1.0f));
			texCoords.Add(new Vector2(0.0f, 1.0f));

			if (counterClockTriangles)
			{
				indices.Add(ind + 0);
				indices.Add(ind + 1);
				indices.Add(ind + 2);

				indices.Add(ind + 0);
				indices.Add(ind + 2);
				indices.Add(ind + 3);
			}
			else
			{
				indices.Add(ind + 0);
				indices.Add(ind + 2);
				indices.Add(ind + 1);

				indices.Add(ind + 0);
				indices.Add(ind + 3);
				indices.Add(ind + 2);
			}
		}

		static public MeshData CreateCubeMesh(Vector3 size, bool createNormals, bool createTexCoords, bool triangleWindingCCW = true)
		{
			MeshData cube = new MeshData();

			List<Vector3> positions = new List<Vector3>();
			List<Vector3> normals = new List<Vector3>();
			List<Vector2> texCoords = new List<Vector2>();
			List<int> indices = new List<int>();

			Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
			Vector3 right = new Vector3(1.0f, 0.0f, 0.0f);
			Vector3 forwards = new Vector3(0.0f, 0.0f, 1.0f);

			// Forwards
			bool tw = triangleWindingCCW;
			
			CreateCubeSide(tw, forwards, up, right, ref positions, ref normals, ref texCoords, ref indices);
			CreateCubeSide(tw, -forwards, up, -right, ref positions, ref normals, ref texCoords, ref indices);

			CreateCubeSide(tw, up, forwards, -right, ref positions, ref normals, ref texCoords, ref indices);
			CreateCubeSide(tw, -up, forwards, right, ref positions, ref normals, ref texCoords, ref indices);

			CreateCubeSide(tw, right, up, -forwards, ref positions, ref normals, ref texCoords, ref indices);
			CreateCubeSide(tw, -right, up, forwards, ref positions, ref normals, ref texCoords, ref indices);

			Matrix3 scale = Matrix3.CreateScale(size);

			for (int pi = 0; pi < positions.Count; pi++)
			{
				positions[pi] = scale * positions[pi];
			}

			cube.positions = positions;
			cube.hasPositionData = true;

			cube.indices = indices;
			cube.hasIndexData = true;

			if (createNormals)
			{
				cube.normals = normals;
				foreach(Vector3 n in cube.normals)
				{
					n.Normalize();
				}
				cube.hasNormalData = true;
			}
			if (createTexCoords)
			{
				cube.texCoords = texCoords;
				cube.hasTexCoordData = true;
			}
			
			cube.drawType = MeshData.DataDrawType.Triangles;
			cube.GenerateBufferHandles();

			return cube;
		}

		static public MeshData CreateSkybox()
		{
			bool counterClockTriangles = false;
			return CreateCubeMesh(new Vector3(2.0f, 2.0f, 2.0f), false, false, counterClockTriangles);
		}
		
		static private void CreatePyramidSideNormals(Vector3 sideFacing, float normalAngle, List<Vector3> normals)
		{
			Vector3 up = new Vector3(0, 1, 0);
			Matrix3 rot = Matrix3.CreateFromAxisAngle(Vector3.Cross(sideFacing, up), normalAngle);
			Vector3 normal = sideFacing * rot;

			normals.Add(normal);
			normals.Add(normal);
			normals.Add(normal);
		}

		static public MeshData CreatePyramidMesh(float baseWidth, float height, bool createNormals, bool createTexCoords)
		{
			MeshData pyraMesh = new MeshData();
			pyraMesh.sourceFileName = "pyramid";
			if (createNormals)
			{
				pyraMesh.sourceFileName += "_normals";
				pyraMesh.normals = new List<Vector3>();
				pyraMesh.hasNormalData = true;
			}
			if (createTexCoords)
			{
				pyraMesh.sourceFileName += "_texCoords";
				pyraMesh.texCoords = new List<Vector2>();
				pyraMesh.hasTexCoordData = true;
			}

			List<Vector3> positions = new List<Vector3>();

			float hw = baseWidth / 2.0f;
			Vector3 pXpZ = new Vector3(hw, 0.0f, hw);
			Vector3 pXmZ = new Vector3(hw, 0.0f, -hw);
			Vector3 mXmZ = new Vector3(-hw, 0.0f, -hw);
			Vector3 mXpZ = new Vector3(-hw, 0.0f, hw);
			Vector3 peak = new Vector3(0.0f, height, 0.0f);

			Vector2 top = new Vector2(0.5f, 1.0f);
			Vector2 left = new Vector2(0.0f, 0.0f);
			Vector2 right = new Vector2(1.0f, 0.0f);

			if (createNormals)
			{
				pyraMesh.indices = new List<int>();
				pyraMesh.hasIndexData = true;

				// When normals are created, 4 distinct sides are needed
				float sideLength = (float)Math.Sqrt((hw * hw) + (height * height));
				float angle = (float)Math.Sin(height / sideLength);
				float normalAngle = MathHelper.PiOver2 - angle;

				// Side to positive Z

				Vector3 toZ = new Vector3(0, 0, 1);
				CreatePyramidSideNormals(toZ, normalAngle, pyraMesh.normals);


				positions.Add(peak);  // 0
				positions.Add(mXpZ);  // 1
				positions.Add(pXpZ);  // 2

				int ibase = 0;
				pyraMesh.indices.Add(ibase + 0);
				pyraMesh.indices.Add(ibase + 2);
				pyraMesh.indices.Add(ibase + 1);
				ibase += 3;
				

				// Side to negative Z
				Vector3 fromZ = toZ * -1.0f;
				CreatePyramidSideNormals(fromZ, normalAngle, pyraMesh.normals);

				positions.Add(peak);
				positions.Add(pXmZ);
				positions.Add(mXmZ);

				pyraMesh.indices.Add(ibase + 0);
				pyraMesh.indices.Add(ibase + 2);
				pyraMesh.indices.Add(ibase + 1);
				ibase += 3;

				// Side to positive X
				Vector3 toX = new Vector3(1.0f, 0.0f, 0.0f);
				CreatePyramidSideNormals(toX, normalAngle, pyraMesh.normals);

				positions.Add(peak);
				positions.Add(pXpZ);
				positions.Add(pXmZ);

				pyraMesh.indices.Add(ibase + 0);
				pyraMesh.indices.Add(ibase + 2);
				pyraMesh.indices.Add(ibase + 1);
				ibase += 3;

				// Side to negative X
				Vector3 fromX = toX * -1.0f;
				CreatePyramidSideNormals(fromX, normalAngle, pyraMesh.normals);

				positions.Add(peak);
				positions.Add(mXmZ);
				positions.Add(mXpZ);

				pyraMesh.indices.Add(ibase + 0);
				pyraMesh.indices.Add(ibase + 2);
				pyraMesh.indices.Add(ibase + 1);
				ibase += 3;

				// Bottom
				positions.Add(pXmZ);
				positions.Add(pXpZ);
				positions.Add(mXmZ);
				positions.Add(mXpZ);

				pyraMesh.indices.Add(ibase + 0);
				pyraMesh.indices.Add(ibase + 2);
				pyraMesh.indices.Add(ibase + 1);

				pyraMesh.indices.Add(ibase + 1);
				pyraMesh.indices.Add(ibase + 2);
				pyraMesh.indices.Add(ibase + 3);

				Vector3 normalDown = new Vector3(0.0f, -1.0f, 0.0f);
				pyraMesh.normals.Add(normalDown);
				pyraMesh.normals.Add(normalDown);
				pyraMesh.normals.Add(normalDown);
				pyraMesh.normals.Add(normalDown);


				if (createTexCoords)
				{
					pyraMesh.texCoords.Add(top);
					pyraMesh.texCoords.Add(right);
					pyraMesh.texCoords.Add(left);

					pyraMesh.texCoords.Add(top);
					pyraMesh.texCoords.Add(right);
					pyraMesh.texCoords.Add(left);

					pyraMesh.texCoords.Add(top);
					pyraMesh.texCoords.Add(right);
					pyraMesh.texCoords.Add(left);

					pyraMesh.texCoords.Add(top);
					pyraMesh.texCoords.Add(right);
					pyraMesh.texCoords.Add(left);

					pyraMesh.texCoords.Add(new Vector2(0.0f, 1.0f));
					pyraMesh.texCoords.Add(new Vector2(1.0f, 1.0f));
					pyraMesh.texCoords.Add(left);
					pyraMesh.texCoords.Add(right);
				}
			}
			else
			{
				pyraMesh.hasNormalData = false;
				// Without normals only 5 distinct points are needed
				// Base
				positions.Add(pXpZ);    // 0
				positions.Add(pXmZ);    // 1
				positions.Add(mXmZ);    // 2
				positions.Add(mXpZ);    // 3
				positions.Add(peak); // 4 peak


				pyraMesh.indices = new List<int> {
					0, 3, 2		// base 
					, 1, 0, 2	// base 
					, 0, 1, 4	// to X
					, 3, 0, 4   // to Z
					, 2, 3, 4	// to -X
					, 1, 2, 4 }; // To -Z

				pyraMesh.hasIndexData = true;

				if (createTexCoords)
				{
					pyraMesh.texCoords.Add(new Vector2(0.0f, 0.0f));
					pyraMesh.texCoords.Add(new Vector2(0.0f, 1.0f));
					pyraMesh.texCoords.Add(new Vector2(1.0f, 0.0f));
					pyraMesh.texCoords.Add(new Vector2(1.0f, 1.0f));
					pyraMesh.texCoords.Add(new Vector2(0.5f, 1.0f));
				}
			}

			pyraMesh.positions = positions;
			pyraMesh.hasPositionData = true;

			pyraMesh.drawType = MeshData.DataDrawType.Triangles;

			pyraMesh.GenerateBufferHandles();

			Error.checkGLError("Quad Mesh Data creation");

			return pyraMesh;
		}

		static public MeshData CreateNormalDebug(List<Vector3>positions, List<Vector3> normals)
		{
			MeshData grid = new MeshData();
			grid.sourceFileName = "NormalDebug";
			grid.positions = new List<Vector3>();

			for(int i = 0; i < positions.Count; i++)
			{
				grid.positions.Add(positions[i]);
				grid.positions.Add(positions[i] + normals[i]);
			}

			grid.hasPositionData = true;
			grid.drawType = MeshData.DataDrawType.Lines;

			grid.GenerateBufferHandles();

			Error.checkGLError("Normals debug Data creation");

			return grid;

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
			grid.drawType = MeshData.DataDrawType.Points;

			grid.GenerateBufferHandles();

			Error.checkGLError("Grid Mesh Data creation");

			return grid;
		}

		static public MeshData CreateStarSphere(float radius, int starsAmount, float sizeMin, float sizeMax)
		{
			Random randomizer = new Random(0);
			MeshData stars = new MeshData();
			stars.sourceFileName = "Stars_r:_" + radius + "_amount:_" + stars;
			stars.hasPositionData = true;
			stars.hasTexCoordData = true;
			stars.hasIndexData = true;
			stars.positions = new List<Vector3>();
			stars.texCoords = new List<Vector2>();
			stars.indices = new List<int>();

			Vector3 worldRight = new Vector3(1.0f, 0.0f, 0.0f);
			Vector3 worldUp = new Vector3(0.0f, 1.0f, 0.0f);
			float circle = MathHelper.DegreesToRadians(360);
			float hCircle = circle * 0.5f;
			int ind = 0;
			for (int i = 0; i < starsAmount; i++)
			{
				float yAngle = (float)randomizer.NextDouble() * circle;
				float starAngle = ((float)i / starsAmount) * circle;
				float elevationAngle = ((float)randomizer.NextDouble() - 0.5f) * hCircle;

				Matrix3 headingRot = Matrix3.CreateRotationY(yAngle);
				Vector3 heading = headingRot * worldRight;
				Vector3 headingRigth = Vector3.Cross(heading, worldUp);

				Vector3 right = headingRigth;

				Matrix3 elevationRot = Matrix3.CreateFromAxisAngle(headingRigth, elevationAngle);
				Vector3 direction = elevationRot * heading;

				direction.Normalize();
				Vector3 up = Vector3.Cross(right, direction);

				// Logger.LogInfo("Star angle: " + MathHelper.RadiansToDegrees(elevationAngle) + " dir.length " + direction.Length + " r.length " + right.Length + " up.length: " + up.Length);

				direction *= radius;
				float size = (float)randomizer.NextDouble() * (sizeMax - sizeMin) + sizeMin;
				right *= size;
				up *= size;

				stars.positions.Add(direction - right - up);
				stars.positions.Add(direction + right - up);
				stars.positions.Add(direction + right + up);
				stars.positions.Add(direction - right + up);

				stars.indices.Add(ind);
				stars.indices.Add(ind + 1);
				stars.indices.Add(ind + 2);
				stars.indices.Add(ind);
				stars.indices.Add(ind + 2);
				stars.indices.Add(ind + 3);

				ind += 4;

				stars.texCoords.Add(new Vector2(0.0f, 0.0f));
				stars.texCoords.Add(new Vector2(1.0f, 0.0f));
				stars.texCoords.Add(new Vector2(1.0f, 1.0f));
				stars.texCoords.Add(new Vector2(0.0f, 1.0f));
			}

			stars.drawType = MeshData.DataDrawType.Triangles;
			stars.GenerateBufferHandles();

			return stars;
		}

		static public MeshData CreateMountains(float sideLength
			, bool createNormals, int iterations, float variation, int randomSeed)
		{

			Random randomizer = new Random(randomSeed);

			// Code from the article
			float maxLevel = 0.0f;
			int dim = (int)Math.Pow(2, iterations);
			int arrayWidth = dim + 1;
			int arrayHeight = dim + 1;

			float[,] data = new float[arrayWidth, arrayHeight];

			for (int iteration = iterations; iteration > 0; iteration--)
			{
				int skip = (int)Math.Pow(2, iteration);
				int half = (skip / 2);
				float squareSide = ((float)(skip) / (float)dim) * sideLength;
				Logger.LogInfo("Iteration " + iteration + " skip: " + skip);

				// Logger.LogInfo("Tops and bottoms");
				for (int y = 0; y < arrayHeight; y += skip)
				{
					for (int x = half; x < arrayWidth; x += skip)
					{
						float rand = ((float)randomizer.NextDouble() - 0.5f) * variation * squareSide;
						float change = (data[x - half, y] + data[x + half, y]) / 2.0f;
						data[x, y] = change + rand;
					}
				}

				// Logger.LogInfo("Sides");
				for (int x = 0; x < arrayWidth; x += skip)
				{
					for (int y = half; y < arrayHeight; y += skip)
					{
						float rand = ((float)randomizer.NextDouble() - 0.5f) * variation * squareSide;
						float change = (data[x, y - half] + data[x, y + half]) / 2.0f;
						data[x, y] = change + rand;
					}
				}

				// Logger.LogInfo("Centers");
				for (int x = half; x < arrayWidth; x += skip)
				{
					for (int y = half; y < arrayHeight; y += skip)
					{
						float rand = ((float)randomizer.NextDouble() - 0.5f) * variation * squareSide;
						float change1 = (data[x + half, y - half] + data[x - half, y + half]) / 2.0f;
						float change2 = (data[x - half, y - half] + data[x + half, y + half]) / 2.0f;
						data[x, y] = (change1 + change2) / 2.0f + rand;
						if (data[x, y] > maxLevel)
						{
							maxLevel = data[x, y];
						}
					}
				}
			}

			MeshData mountains = new MeshData();
			mountains.sourceFileName = "Mountains_" + sideLength + "_x_" + sideLength;

			CreateTerrainFromHeightArray(data, arrayWidth, arrayHeight, sideLength, sideLength, ref mountains, createNormals);
			CreateHeightScaleTexCoords(arrayWidth, arrayHeight, ref mountains, maxLevel);

			mountains.drawType = MeshData.DataDrawType.Triangles;
			mountains.GenerateBufferHandles();

			Error.checkGLError("Mountain Mesh Data creation");
			return mountains;
		}

		static public MeshData CreateTerrain(float width, float depth, float trianglesPerUnit
			, bool createNormals, bool createTexCoords
			, float UVrepeatX, float UVrepeatZ)
		{
			// Array of heights
			int arrayWidth = (int)Math.Floor(trianglesPerUnit * width);
			int arrayHeight = (int)Math.Floor(trianglesPerUnit * depth);

			int vertexAmount = arrayWidth * arrayHeight;

			float[,] heights = new float[arrayWidth, arrayHeight];

			for (int x = 0; x < arrayWidth; x++)
			{
				for (int z = 0; z < arrayHeight; z++)
				{
					heights[x, z] = 0.0f;
				}
			}

			MeshData terrain = new MeshData();
			terrain.sourceFileName = "Terrain " + width + "x" + depth;

			CreateTerrainFromHeightArray(heights, arrayWidth, arrayHeight, width, depth, ref terrain, createNormals);
			CreateRepeatingTexCoords(arrayWidth, arrayHeight, ref terrain, UVrepeatX, UVrepeatZ);
			
			terrain.drawType = MeshData.DataDrawType.Triangles;
			terrain.GenerateBufferHandles();

			Error.checkGLError("Terrain Mesh Data creation");
			return terrain;
		}

		static private void CreateTerrainFromHeightArray(float[,] heights, int width, int height
		, float terrainWidth, float terrainDepth
		, ref MeshData mesh , bool createNormals)
		{
			// Positions

			/*  X ->
			 *  Z [0, 1, 2, 3, ..., W - 1
			 *  |  W, W+1, ... , W * 2 - 1
			 *	|  2W, 
				|	...				W * H
			 *  V

			 Array = [x0,z0, x1,z0, x2, z0 ...
			 */
			mesh.hasPositionData = true;
			mesh.positions = new List<Vector3>();

			float s = terrainWidth;
			float hw = terrainWidth / 2.0f;
			float hd = terrainDepth / 2.0f;
			float xc = 0.0f - hw;
			float zc = 0.0f - hd;

			float widthStep = terrainWidth / (float)width;
			float depthStep = terrainDepth / (float)height;
			for (int posZ = 0; posZ < height; posZ++)
			{
				for (int posX = 0; posX < width; posX++)
				{
						float Y = heights[posX, posZ];
						mesh.positions.Add(new Vector3(xc + widthStep * posX
						, Y
						, zc + depthStep * posZ));
				}
			}


			// Create normals
			if (createNormals)
			{
				Logger.LogInfo("Creating normals for terrain, position count : " + mesh.positions.Count);

				mesh.hasNormalData = true;
				mesh.normals = new List<Vector3>();

				Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);

				for (int posZ = 0; posZ < height; posZ++)
				{
					for (int posX = 0; posX < width; posX++)
					{
						int positionIndex = posX + (width) * posZ;
						int currentI = positionIndex + 0;

						if ((posX == 0 || posX == width - 1) 
							|| (posZ == 0 || posZ == height - 1))
						{
							// Borders and corners
							mesh.normals.Add(up);
							continue;
						}
						// Check previous and next to determine normal
						int prevXI = currentI - 1;
						int nextXI = currentI + 1;
						int prevZI = currentI - width;
						int nextZI = currentI + width;
						Vector3 current = mesh.positions[currentI];
						Vector3 prevX = mesh.positions[prevXI];
						Vector3 nextX = mesh.positions[nextXI];
						Vector3 prevZ = mesh.positions[prevZI];
						Vector3 nextZ = mesh.positions[nextZI];

						Vector3 normal = CalculateTerrainNormal(prevX, nextX, prevZ, nextZ, current);
						mesh.normals.Add(normal);
					}
				}
			}

			// Indices

			mesh.hasIndexData = true;
			mesh.indices = new List<int>();

			// Not until the end, because the triangle includes the next row/column
			Logger.LogInfo("Creating indices for mountains");
			for (int indiceZ = 0; indiceZ < height - 1; indiceZ++)
			{
				for (int indiceX = 0; indiceX < width - 1; indiceX++)
				{
					int corner = indiceX * width + indiceZ;
					int nextX = corner + 1;
					int below = corner + width;
					int across = below + 1;

					mesh.indices.Add(corner);
					mesh.indices.Add(below);
					mesh.indices.Add(across);

					mesh.indices.Add(corner);
					mesh.indices.Add(across);
					mesh.indices.Add(nextX);
				}
			}
		}

		static private Vector3 CalculateTerrainNormal(Vector3 prevX, Vector3 nextX
			, Vector3 prevZ, Vector3 nextZ
			, Vector3 current)
		{
			Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
			Vector3 right = new Vector3(1.0f, 0.0f, 0.0f);
			Vector3 forward = new Vector3(0.0f, 0.0f, 1.0f);

			Vector3 sideFacingX = new Vector3(right);
			Vector3 sideFacingZ = new Vector3(forward);
			Vector3 slopeX = (nextX - prevX).Normalized();
			Vector3 slopeZ = (nextZ - prevZ).Normalized();
			bool adjustX = false;
			bool adjustZ = false;

			Vector3 normalX = up;
			Vector3 normalZ = up;;

			Error.Assume(Vector3.Dot(slopeX, forward) == 0.0f, "Slope X is not x-wise");
			Error.Assume(Vector3.Dot(slopeZ, right) == 0.0f, "Slope Z is not z-wise");

			if (prevX.Y > current.Y && current.Y > nextX.Y) 
			{
				sideFacingX *= -1.0f;
				slopeX *= -1.0f;
				adjustX = true;
			}
			if (prevX.Y < current.Y && current.Y < nextX.Y)
			{
				adjustX = true;
			}
			if (prevZ.Y > current.Y && current.Y > nextZ.Y) 
			{
				sideFacingZ *= -1.0f;
				slopeZ *= -1.0f;
				adjustZ = true;
			}
			if (prevZ.Y < current.Y && current.Y < nextZ.Y)
			{
				adjustZ = true;
			}

			if (adjustX)
			{
				Error.Assume(slopeX.Y > 0.0f, "Slope X is not going up");
				normalX = CalculateSlopeNormal(slopeX, sideFacingX);
			}
			if (adjustZ)
			{
				Error.Assume(slopeZ.Y > 0.0f, "Slope Z is not going up");
				normalZ = CalculateSlopeNormal(slopeZ, sideFacingZ);
			}
			return (normalX + normalZ).Normalized();
		}

		static private Vector3 CalculateSlopeNormal(Vector3 slope, Vector3 sideFacing)
		{
			Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
			float dotProd = Vector3.Dot(up, slope);
			Error.Assume(dotProd >= 0.0f, "up dot slope is negative");
			float outerAngle = (float)Math.Cos(dotProd);
			Error.Assume(outerAngle > 0.0f && MathHelper.RadiansToDegrees(outerAngle) < 90, "Outer angle is negative or over 90 degrees");
			Matrix3 rot = Matrix3.CreateFromAxisAngle(Vector3.Cross(sideFacing, up), outerAngle);
				return new Vector3(up * rot);
		}

		static private void CreateRepeatingTexCoords(int width, int height, ref MeshData mesh, float uvRepeatX, float uvRepeatZ)
		{
			mesh.hasTexCoordData = true;
			mesh.texCoords = new List<Vector2>();

			float uStep = uvRepeatX / width;
			float vStep = uvRepeatZ / height;

			for (int x = 0; x < width; x++)
			{
				for (int z = 0; z < height; z++)
				{
					Vector2 tex = new Vector2(x * uStep, z * vStep);
					mesh.texCoords.Add(tex);
				}
			}
		}

		static private void CreateHeightScaleTexCoords(int width, int height, ref MeshData mesh, float maxHeight)
		{
			mesh.hasTexCoordData = true;
			mesh.texCoords = new List<Vector2>();

			for (int i = 0; i < mesh.positions.Count; i++)
			{
				float h = mesh.positions[i].Y;
				float tec = h / maxHeight;
            
				Vector2 tex = new Vector2(tec, 0.5f);
				mesh.texCoords.Add(tex);
			}
		}
	}
}