using System;
using System.Collections.Generic;

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
		
		private int vertexSize;
		private int elementsInPosition;
		
		private List<Vector3> rawPositions;
		
		public Matrix4Uniform worldMatrix;
		
		private struct OBJFace
		{
			public int positionIndex;
			public int normalIndex;
			public int materialIndex;
		}
		
		static public Mesh CreateTriangleMesh()
		{
			// positions
			List<Vector3> positions = new List<Vector3>(3);
			positions.Add(new Vector3(-0.5f, 0.5f, 0.0f));
			positions.Add(new Vector3(0.5f, 0.5f, 0.0f));
			positions.Add(new Vector3(0.0f, 0.0f, 0.0f));
			
			return new Mesh(positions);
		}
		
		
		
		// Reads on .obs file
		static public Mesh CreateFromFile(string filename)
		{
			List<OBJFace> faces;
			
			List<Vector3> positions;
			List<Vector3> normals;
			List<int> materials;

            faces = new List<OBJFace>();
            positions = new List<Vector3>();
            normals = new List<Vector3>();
            materials = new List<int>();

            readOBJ(filename, ref faces, ref positions, ref normals, ref materials);

			// Create positions 
			List<Vector3> trianglePositions = new List<Vector3>(positions.Count);
			
			foreach(OBJFace face in faces)
			{
				trianglePositions.Add( positions[ face.positionIndex - 1 ]);
			}
			
            return new Mesh(trianglePositions);
		}
		
		public Mesh(List<Vector3> positions)
		{
			BufferHandle = GL.GenBuffer();
			VAOHandle = GL.GenVertexArray();
			
			VertexAmount = positions.Count;
			
			int bytesPerFloat = 4;
            elementsInPosition = 3;
			int positionSize = elementsInPosition * bytesPerFloat;
			vertexSize = positionSize;
			
			Error.checkGLError("Mesh constructor");

			 rawPositions = positions;
			 
			 worldMatrix = new Matrix4Uniform("modelMatrix");
			 worldMatrix.Matrix = Matrix4.Identity;
		}
		
		public void bufferData(int positionIndex)
		{
			GL.BindVertexArray(VAOHandle);
			
			GL.BindBuffer(BufferTarget.ArrayBuffer, BufferHandle);
			
			GL.BufferData(BufferTarget.ArrayBuffer, VertexAmount * vertexSize, rawPositions.ToArray(), BufferUsageHint.StaticDraw);

            //  Vertex attributes

            GL.VertexAttribPointer(index: positionIndex, size: elementsInPosition
                , type: VertexAttribPointerType.Float
                , normalized: false, stride: vertexSize, offset: 0);

            GL.EnableVertexAttribArray(positionIndex);
			// GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			
			Error.checkGLError("Mesh.bufferData");
		}
		
		private static void readOBJ(string filename, ref List<OBJFace> faces, ref List<Vector3> positions, ref List<Vector3> normals, ref List<int> materials)
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
				else if (line.Contains("vn"))
				{
					normals.Add(readNormal(line));
				}
				else if (line.Contains("vt"))
				{
					// skip for now
				}
				else if (line.Contains("v"))
				{
					positions.Add(readPos(line));
				}
				else if (line.Contains("f"))
				{
					readFaces(line, ref faces);
				}
				
			} while( line != null);
			

            sourceFile.Close();
		}
		
		static Vector3 readPos(string posLine)
		{
			char [] split = {' '};
			string[] positions = posLine.Split(split);

            Vector3 pos = new Vector3();
			if (positions.Length == 4)  // v # # #
			{
				pos.X = Convert.ToSingle(positions[1].Trim());
				pos.Y = Convert.ToSingle(positions[2].Trim());
				pos.Z = Convert.ToSingle(positions[3].Trim());
			}

            return pos;
		}

        static Vector3 readNormal(string normalLine)
		{
			char [] split = {' '};
			string[] normals = normalLine.Split(split);

            Vector3 norm = new Vector3();

            if (normals.Length == 4)  // vn # # #
			{
				norm.X = Convert.ToSingle(normals[1].Trim());
				norm.Y = Convert.ToSingle(normals[2].Trim());
				norm.Z = Convert.ToSingle(normals[3].Trim());
			}

            return norm;
		}

        static void readFaces(string faceLine, ref List<OBJFace> faces)
		{
			char [] split = {' '};
			string[] faceStrings = faceLine.Split(split);
			if (faceStrings.Length == 4)  // f #/#/# #/#/# #/#/#
			{
				faces.Add(readFace(faceStrings[1]));
				faces.Add(readFace(faceStrings[2]));
				faces.Add(readFace(faceStrings[2]));
			}
		}

        static OBJFace readFace(string faceLine)
		{
			char [] split = {'/'};
			string[] faceStrings = faceLine.Split(split);
				
			OBJFace face = new OBJFace();
			if (faceStrings.Length == 3)  // #/#/#
			{
				face.positionIndex = Convert.ToInt32(faceStrings[0].Trim());
				face.normalIndex = Convert.ToInt32(faceStrings[1].Trim());
				face.materialIndex = Convert.ToInt32(faceStrings[2].Trim());
			}
			
			return face;
		}
	}
}