using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Globalization;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTkConsole
{
	static class OBJFileReader
	{
		public class FaceComparer : IComparer<OBJFace>
		{
			public int Compare(OBJFace a, OBJFace b)
			{
				if (
					a.normalIndex == b.normalIndex
					&& a.positionIndex == b.positionIndex
					&& a.texCoordIndex == b.texCoordIndex)
				{
					return 0;
				}
				else
				{
					if (a.positionIndex > b.positionIndex)
					{
						return 1;
					}
					else
					{
						return -1;
					}
				}
			}
		}

		public class OBJFinder
		{
			private OBJFace targetFace;
			public OBJFinder(OBJFace target)
			{
				targetFace = target;
			}

			public bool isSame(OBJFace test)
			{
				FaceComparer f = new FaceComparer();
				return (f.Compare(targetFace, test) == 0);
			}
		}

		public struct OBJFace
		{
			public uint positionIndex;
			public uint texCoordIndex;
			public uint normalIndex;
		}

		public static Vector3 readVector3(string vectorLine)
		{
			char[] split = { ' ' };
			string[] values = vectorLine.Split(split);

			Vector3 vec = new Vector3();

			if (values.Length == 4)  // X # # #
			{
				// use . as separator instead of system default
				System.Globalization.NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

				try
				{
					vec.X = Single.Parse(values[1], nfi);
					vec.Y = Single.Parse(values[2], nfi);
					vec.Z = Single.Parse(values[3], nfi);
				} catch (FormatException e)
				{
					Logger.LogError(Logger.ErrorState.Limited, string.Format("Value {0} Caught Formatexception:" + e.Message, values[1]));
				}
			}

			return vec;
		}

		public static Vector2 readVector2(string vectorLine)
		{
			char[] split = { ' ' };
			string[] values = vectorLine.Split(split);

			Vector2 vec = new Vector2();

			if (values.Length == 3)  // X # #
			{
				// use . as separator instead of system default
				System.Globalization.NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

				try
				{
					vec.X = Single.Parse(values[1], nfi);
					vec.Y = Single.Parse(values[2], nfi);
				}
				catch (FormatException e)
				{
					Logger.LogError(Logger.ErrorState.Limited, string.Format("Value {0} Caught Formatexception:" + e.Message, values[1]));
				}
			}

			return vec;
		}

		public static void readFaces(string faceLine, ref List<OBJFace> faces)
		{
			char[] split = { ' ' };
			string[] faceStrings = faceLine.Split(split);
			if (faceStrings.Length == 4)  // f #/#/# #/#/# #/#/#
			{
				faces.Add(readFace(faceStrings[1]));
				faces.Add(readFace(faceStrings[2]));
				faces.Add(readFace(faceStrings[3]));
			}
		}

		public static OBJFace readFace(string faceLine)
		{
			char[] split = { '/' };
			string[] faceStrings = faceLine.Split(split);

			OBJFace face = new OBJFace();
			if (faceStrings.Length == 3)  // #/#/#
			{
				face.positionIndex = Convert.ToUInt32(faceStrings[0].Trim());
				face.texCoordIndex = Convert.ToUInt32(faceStrings[1].Trim());
				face.normalIndex = Convert.ToUInt32(faceStrings[2].Trim());
			}

			return face;
		}

		public static void readMaterialFile(string mttlibLine, MaterialManager materialManager, ref MaterialManager.Material material)
		{
			// mtllib voxelColor.mtl
			char[] space = { ' ' };
			string[] matFileLines = mttlibLine.Split(space);
			string filename = matFileLines[1];

			string fullPath = Directory.GetCurrentDirectory() + "\\" + filename;

			material = materialManager.loadMaterial(fullPath);
		}

		public static void readOBJ(string filename, MaterialManager materialManager, ref List<OBJFileReader.OBJFace> faces, ref List<Vector3> positions, ref List<Vector3> normals, ref List<Vector2> texCoords, ref MaterialManager.Material material)
		{
			// Sections 
			// . normals 
			// . texcoords 
			// . verts 
			// . faces 


			StreamReader sourceFile = new StreamReader(filename);

			string line;
			do
			{
				line = sourceFile.ReadLine();
				if (line == null)
				{
					break;
				}

				if (line.Contains("#"))
				{
					// comment
				}
				else if (line.Contains("mtllib"))
				{
					// material
					// Read material properties from file 
					OBJFileReader.readMaterialFile(line, materialManager, ref material);
				}
				else if (line.Contains("usemtl"))
				{
					// material, what material to use.
				}
				else if (line.Contains("vn"))
				{
					normals.Add(readNormal(line));
				}
				else if (line.Contains("vt"))
				{
					texCoords.Add(readVector2(line));
				}
				else if (line.Contains("v"))
				{
					positions.Add(readPos(line));
				}
				else if (line.Contains("f"))
				{
					OBJFileReader.readFaces(line, ref faces);
				}

			} while (line != null);


			sourceFile.Close();
		}

		static Vector3 readPos(string posLine)
		{
			return OBJFileReader.readVector3(posLine);
		}

		static Vector3 readNormal(string normalLine)
		{
			// vn # # # 
			return OBJFileReader.readVector3(normalLine);
		}

	}
}