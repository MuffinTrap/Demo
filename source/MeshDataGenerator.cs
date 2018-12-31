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

			triMesh.indices = new List<int> { 0, 2, 1 };
			triMesh.hasIndexData = true;

			triMesh.VertexAmount = 3;
			triMesh.drawType = MeshData.DataDrawType.Triangles;

			triMesh.GenerateBufferHandles();
			
			Error.checkGLError("Triangle Mesh Data creation");

			return triMesh;
		}
		static public MeshData CreateTextMesh()
		{
			MeshData textMesh = new MeshData();
			textMesh.sourceFileName = "text_mesh";
			textMesh.positions = new List<Vector3>();
			textMesh.texCoords = new List<Vector2>();
			textMesh.indices = new List<int>();

			int letters = 100 * 4;
			float xstart = 0.0f;
			float step = 0.01f;
			for (int i = 0; i < letters; i++)
			{
				textMesh.positions.Add(new Vector3(xstart + 0.0f, 0.0f, 0.0f)); // 0
				textMesh.positions.Add(new Vector3(.0f, 1.0f, 0.0f));  // 1
				textMesh.positions.Add(new Vector3(1.0f, 1.0f, 0.0f));   // 2
				textMesh.positions.Add(new Vector3(1.0f, 0.0f, 0.0f));  // 3
				textMesh.texCoords.Add(new Vector2(0, 0));
			}

			textMesh.hasPositionData = true;
			textMesh.hasTexCoordData = true;

			textMesh.GenerateBufferHandles();

			return textMesh();
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

			quadMesh.VertexAmount = positions.Count;

			quadMesh.drawType = MeshData.DataDrawType.Triangles;

			quadMesh.GenerateBufferHandles();

			Error.checkGLError("Quad Mesh Data creation");

			return quadMesh;
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

			pyraMesh.VertexAmount = positions.Count;
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
			grid.VertexAmount = grid.positions.Count;
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
			grid.VertexAmount = grid.positions.Count;
			grid.drawType = MeshData.DataDrawType.Points;

			grid.GenerateBufferHandles();

			Error.checkGLError("Grid Mesh Data creation");

			return grid;
		}

		static public MeshData CreateStarSphere(float radius, int starsAmount, float sizeDegrees)
		{
			Random randomizer = new Random(0);
			MeshData stars = new MeshData();
			stars.sourceFileName = "Stars_r:_" + radius + "_amount:_" + stars;
			stars.hasPositionData = true;
			stars.hasTexCoordData = true;
			// stars.hasIndexData = true;
			stars.positions = new List<Vector3>();
			stars.texCoords = new List<Vector2>();
			// stars.indices = new List<int>();

			float circle = MathHelper.DegreesToRadians(360);
			float size = MathHelper.DegreesToRadians(sizeDegrees);
			Vector3 right = new Vector3(1.0f, 0.0f, 0.0f);
			for (int i = 0; i < starsAmount; i++)
			{
				float yAngle = (float)randomizer.NextDouble() * circle;
				float zAngle = (float)randomizer.NextDouble() * circle;

				Matrix3 rot = Matrix3.CreateRotationZ(zAngle) * Matrix3.CreateRotationY(yAngle);
				Vector3 center = rot * right;


				center.Normalize();
				center *= radius;
				stars.positions.Add(center);
				Vector2 tex = new Vector2((float)randomizer.NextDouble(), 0.5f);
				stars.texCoords.Add(tex);
				// Logger.LogInfo("Star at: " + pos.X + ", " + pos.Y + ", " + pos.Z + ".U: " + tex.X + " V: " + tex.Y );
			}

			stars.VertexAmount = starsAmount;
			stars.hasNormalData = false;
			stars.hasIndexData = false;

			stars.drawType = MeshData.DataDrawType.Points;
			stars.GenerateBufferHandles();
			return stars;
		}

		static public MeshData CreateMountains(float sideLength, float trianglesPerUnit
			, bool createNormals, float UVrepeatX, float UVrepeatZ, int iterations, float variation)
		{
			MeshData mountains = new MeshData();
			mountains.sourceFileName = "Mountains_" + sideLength + "_x_" + sideLength;

			Random randomizer = new Random(1);

			// Code from the article
			float maxLevel = 0.0f;
			int dim = (int)Math.Pow(2, iterations);
			float[,] data = new float[dim + 1, dim + 1];
			for (int iteration = iterations; iteration > 0; iteration--)
			{
				int skip = (int)Math.Pow(2, iteration);
				int half = (skip / 2);
				float squareSide = ((float)(skip) / (float)dim) * sideLength;
				Logger.LogInfo("Iteration " + iteration + " skip: " + skip);

				// Logger.LogInfo("Tops and bottoms");
				for (int y = 0; y <= dim; y += skip)
				{
					for (int x = half; x <= dim; x += skip)
					{
						float rand = ((float)randomizer.NextDouble() - 0.5f) * variation * squareSide;
						float change = (data[x - half, y] + data[x + half, y]) / 2.0f;
						data[x, y] = change + rand;
					}
				}

				// Logger.LogInfo("Sides");
				for (int x = 0; x <= dim; x += skip)
				{
					for (int y = half; y <= dim; y += skip)
					{
						float rand = ((float)randomizer.NextDouble() - 0.5f) * variation * squareSide;
						float change = (data[x, y - half] + data[x, y + half]) / 2.0f;
						data[x, y] = change + rand;
					}
				}

				// Logger.LogInfo("Centers");
				for (int x = half; x <= dim; x += skip)
				{
					for (int y = half; y <= dim; y += skip)
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

			////////////////

			// 4 Points on sides of square
			float s = sideLength;
			float sh = sideLength / 2.0f;
			float xc = 0.0f - sh;
			float zc = 0.0f + sh;

			mountains.hasPositionData = true;
			mountains.positions = new List<Vector3>();

			mountains.hasIndexData = true;
			mountains.indices = new List<int>();


			// Indices
			/*  (0,0) (1,0) (1,1)
			 *	(0,0) (1,1) (0,1)
			 * 
			 *  x,y  x+1, y  x+1, y+1
			 *  x,y  
			 *  
			 *  x
			 */
			float waterLevel = 0.0f;
			float sideStep = sideLength / (float)dim;
			for (int posZ = 0; posZ <= dim; posZ++)
			{
				for (int posX = 0; posX <= dim; posX++)
				{
					mountains.positions.Add(new Vector3(xc + sideStep * posX
					, MathHelper.Clamp(data[posX, posZ], waterLevel, maxLevel)
					, zc - sideStep * posZ));
				}
			}

			// Create normals
			if (createNormals)
			{
				Logger.LogInfo("Creating normals for mountains");
				mountains.hasNormalData = true;
				mountains.normals = new List<Vector3>();
				Vector3 right = new Vector3(1.0f, 0.0f, 0.0f);
				Vector3 backward = new Vector3(0.0f, 0.0f, -1.0f);
				Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
				Vector3 unset = new Vector3(-1.0f, -1.0f, -1.0f);
				for (int ni = 0; ni < mountains.positions.Count; ni++)
				{
					mountains.normals.Add(unset);
				}
				// Along x axis
				for (int posZ = 0; posZ <= dim; posZ++)
				{
					for (int posX = 0; posX <= dim; posX++)
					{
						if ((posX == 0 || posX == dim) && (posZ == 0 || posZ == dim))
						{
							continue;
						}
						// Check previous and next to determine normal
						int positionIndex = posX + (dim + 1) * posZ;
						int prevI = positionIndex - 1;
						int currentI = positionIndex + 0;
						int nextI = positionIndex + 1;
						int max = mountains.positions.Count;
						if (prevI >= max || nextI > max || currentI > max)
						{
							Logger.LogError(Logger.ErrorState.Critical, "Out of array, index " + prevI + ", " + currentI + ", " + nextI + " > " + max);
							break;
						}
						// Logger.LogInfo("Height indices X: " + prevI + ", " + currentI + ", " + nextI);
						Vector3 prev = mountains.positions[prevI];
						Vector3 current = mountains.positions[currentI];
						Vector3 next = mountains.positions[nextI];

						Vector3 normal = CalculateMountainNormal(prev, next, current, right);
						mountains.normals[currentI] = normal;
					}
				}

				// Along Z axis
				for (int posX = 0; posX <= dim; posX++)
				{
					for (int posZ = 1; posZ < dim; posZ++)
					{
						if ((posX == 0 || posX == dim) && (posZ == 0 || posZ == dim))
						{
							continue;
						}
						// Check previous and next to determine normal
						int perRow = (dim + 1);
						int positionIndex = posX + perRow * posZ;
						int prevI = positionIndex - perRow;
						int currentI = positionIndex + 0;
						int nextI = positionIndex + perRow;
						// Logger.LogInfo("Height indices Z: " + prevI + ", " + currentI + ", " + nextI);
						Vector3 prev = mountains.positions[prevI];
						Vector3 current = mountains.positions[currentI];
						Vector3 next = mountains.positions[nextI];

						Vector3 normal = CalculateMountainNormal(prev, next, current, backward);
						if (mountains.normals[currentI] == unset)
						{
							mountains.normals[currentI] = normal;
						}
						else
						{
							mountains.normals[currentI] += normal;
						}
					}
				}

				for(int normalI = 0; normalI < mountains.normals.Count; normalI++)
				{
					if (mountains.normals[normalI] == unset)
					{
						mountains.normals[normalI] = up;
					}
					else
					{
						mountains.normals[normalI].Normalize();
					}
				}
			}

			// Not until the end, because the triangle includes the next row/column
			Logger.LogInfo("Creating indices for mountains");
			for (int indiceY = 0; indiceY < dim; indiceY++)
			{
				for (int indiceX = 0; indiceX < dim; indiceX++)
				{
					int corner = indiceY * (dim + 1) + indiceX;
					int nextX = indiceY * (dim + 1) + indiceX + 1;
					int below = (indiceY + 1) * (dim + 1) + indiceX;
					int across = (indiceY + 1) * (dim + 1) + indiceX + 1;

					mountains.indices.Add(corner);
					mountains.indices.Add(nextX);
					mountains.indices.Add(across);

					mountains.indices.Add(corner);
					mountains.indices.Add(across);
					mountains.indices.Add(below);
				}
			}
			
			mountains.VertexAmount = mountains.positions.Count;

			mountains.hasTexCoordData = true;
			mountains.texCoords = new List<Vector2>();

			for (int i = 0; i < mountains.VertexAmount; i++)
			{
				float h = mountains.positions[i].Y;
				float tec = h / maxLevel;
            
				Vector2 tex = new Vector2(tec, 0.5f);
				mountains.texCoords.Add(tex);
			}

			mountains.drawType = MeshData.DataDrawType.Triangles;

			mountains.GenerateBufferHandles();

			return mountains;
		}

		static private Vector3 CalculateMountainNormal(Vector3 prev, Vector3 next, Vector3 current, Vector3 slopeSideFacing)
		{
			Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
			Vector3 sideFacing = new Vector3(slopeSideFacing);
			Vector3 slope = (prev - next).Normalized();
			bool adjust = false;
			if (prev.Y < current.Y && current.Y < next.Y) 
			{
				sideFacing *= -1.0f;
				slope = (next - prev).Normalized();
				adjust = true;
			}
			if (prev.Y > current.Y && current.Y > next.Y)
			{
				adjust = true;
			}
			if (adjust)
			{
				float outerAngle = (float)Math.Cos(Vector3.Dot(up, slope));
				Matrix3 rot = Matrix3.CreateFromAxisAngle(Vector3.Cross(sideFacing, up), outerAngle);
				return new Vector3(sideFacing * rot);
			}
			return up;
		}

		static public MeshData CreateTerrain(float width, float depth, float trianglesPerUnit
			, bool createNormals, float UVrepeatX, float UVrepeatZ)
		{
			MeshData terrain = new MeshData();
			terrain.sourceFileName = "Terrain" + width + "x" + depth;

			int quadsWidth = (int)Math.Floor(trianglesPerUnit * width);
			int quadsDepth = (int)Math.Floor(trianglesPerUnit * depth);
			int widthVertices = quadsWidth * 2;
			int depthVertices = quadsDepth * 2;
			float triangleSideWidth = width / (quadsWidth);
			float triangleSideDepth = depth / (quadsDepth);

			int vertexAmount = widthVertices * depthVertices;
			terrain.positions = new List<Vector3>(vertexAmount);
			terrain.texCoords = new List<Vector2>(vertexAmount);
			if (createNormals)
			{
				terrain.normals = new List<Vector3>(vertexAmount);
				Vector3 normal = new Vector3(0, 1, 0);
				for (int i = 0; i< vertexAmount; i++)
				{
					terrain.normals.Add(normal);
				}
			}
			terrain.indices = new List<int>();
			
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

					float divWidth = quadsWidth / UVrepeatX;
					float divDepth = quadsDepth / UVrepeatZ;
					float texX0 = ((float)qw / divWidth);
					float texX1 = ((float)(qw + 1)/ divWidth);
					float texY0 = ((float)qd / divDepth);
					float texY1 = ((float)(qd + 1) / divDepth);
					Vector2 tex00 = new Vector2(texX0, texY0);
					Vector2 tex10 = new Vector2(texX1, texY0);
					Vector2 tex01 = new Vector2(texX0, texY1);
					Vector2 tex11 = new Vector2(texX1, texY1);

					/*
					Logger.LogInfo("Terrain piece (" + qw + "," + qd 
					+ " at: " + xCoord + ", " + zCoord 
					+ " TxC: " + tex00.X + ", " + tex00.Y + " - " + tex11.X + ", " + tex11.Y
					+ " Size: " + triangleSideWidth + ", " + triangleSideDepth);
					*/


					// 0
					terrain.positions.Add(new Vector3(xCoord
											, 0.0f
											, zCoord));
					terrain.texCoords.Add(tex00);

					// 1
					terrain.positions.Add(new Vector3(xCoord + triangleSideWidth
										, 0.0f
										, zCoord));
					terrain.texCoords.Add(tex10);

					// 2
					terrain.positions.Add(new Vector3(xCoord
											, 0.0f
											, zCoord + triangleSideDepth));
					terrain.texCoords.Add(tex01);

					// 3
					terrain.positions.Add(new Vector3(xCoord + triangleSideWidth
										, 0.0f
										, zCoord + triangleSideDepth));
					terrain.texCoords.Add(tex11);

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
			if (createNormals)
			{
				terrain.hasNormalData = true;
			}
			terrain.VertexAmount = vertexAmount;
			terrain.drawType = MeshData.DataDrawType.Triangles;

			terrain.GenerateBufferHandles();

			Logger.LogInfo("Generated terrain: Vertices: " + terrain.VertexAmount + " Indices: " + terrain.indices.Count);
			Error.checkGLError("Terrain Mesh Data creation");
			return terrain;
		}
	}
}