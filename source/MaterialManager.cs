
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

	static class MaterialManager
	{
		// Material
		public class Material
		{
			public string materialName;
			public string textureName;
			public int textureGLIndex;
			public float illumination;
			public Vector3 alpha;
			public Vector3 diffuse;
			public Vector3 specular;
		}

		public static List<Material> materials;
		private static string dataDir;

		public static void init(string dataDirectory)
		{
			materials = new List<Material>();
			dataDir = dataDirectory;
			//createMaterial("white", Color.White);
		}

		public static Material getMaterialByName(string materialName)
		{
			foreach (Material m in materials)
			{
				if (m.materialName == materialName)
				{
					return m;
				}
			}

			return null;
		}
		

		public static Material loadMaterial(string materialFileName)
		{
			string fullPath = dataDir + materialFileName;
			StreamReader sourceFile = new StreamReader(fullPath);

			string line;
			string materialName = null;
			bool existingFound = false;
			Material newMaterial = null;

			char[] space = { ' ' };

			// First check if we already have this material
			do
			{
				line = sourceFile.ReadLine();
				if (line == null)
				{
					break;
				}
				else if (line.Contains("newmtl"))
				{
					materialName = line.Split(space)[1];
					break;
				}
			}
			while (line != null);


			newMaterial = getMaterialByName(materialName);
			
			if (newMaterial != null)
			{
				sourceFile.Close();
				return newMaterial;
			}
			else
			{
				newMaterial = new Material();
				newMaterial.materialName = materialName;
			}

			// Read rest of the file

			// use . as separator instead of system default
			System.Globalization.NumberFormatInfo nfi = new System.Globalization.CultureInfo("en-US", false).NumberFormat;

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
				else if (line.Contains("map_Kd"))
				{
					// map_Kd filename.png
					newMaterial.textureName = line.Split(space)[1];
					newMaterial.textureGLIndex = loadTexture(newMaterial.textureName);
				}
				else if (line.Contains("illum"))
				{
					newMaterial.illumination = Single.Parse(line.Split(space)[1], nfi);
				}
				else if (line.Contains("Ka"))
				{
					newMaterial.alpha = OBJFileReader.readVector3(line);
				}
				else if (line.Contains("Kd"))
				{
					newMaterial.diffuse = OBJFileReader.readVector3(line);
				}
				else if (line.Contains("Ks"))
				{
					newMaterial.specular = OBJFileReader.readVector3(line);
				}
				

			} while (line != null);


			sourceFile.Close();

			materials.Add(newMaterial);

			return newMaterial;
		}

		static void createMaterial(string materialName, Color materialColor)
		{
			Material newMaterial = getMaterialByName(materialName);
			
			if (newMaterial != null)
			{
				return;
			}
			
			newMaterial = new Material();
			newMaterial.materialName = materialName;
			newMaterial.diffuse = new Vector3(1,1,1);
			newMaterial.alpha = new Vector3(0,0,0);
			newMaterial.specular = new Vector3(0,0,0);
			newMaterial.textureName = "";
			
			newMaterial.textureGLIndex = createTexture(materialColor);
			
			materials.Add(newMaterial);
		}
		
		static int createTexture(System.Drawing.Color textureColor)
		{
			Bitmap map = new Bitmap(4, 4);
				for (int x = 0; x < 4; x++)
				{
					for (int y = 0; y < 4; y++)
					{
						map.SetPixel(x, y, textureColor);
					}
				}
			int textureId = loadTextureFromBitmap(map);
			return textureId;

		}

		static int loadTexture(string textureFileName)
		{
            Console.WriteLine("loadTexture " + textureFileName);
			string fullPath = dataDir + textureFileName;

			int textureId = -1;
			
			try
			{
				Bitmap map = new Bitmap(fullPath);
				textureId = loadTextureFromBitmap(map); 
			}
			catch (FileNotFoundException e)
			{
				Console.WriteLine("Load texture did not find file:" + e.Message);
			}
			catch (ArgumentException e)
			{
				Console.WriteLine("Load texture did not find file:" + e.Message);
			}

			return textureId;

		}

		static int loadTextureFromBitmap(Bitmap map)
		{
			int texID = GL.GenTexture();
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, texID);
			BitmapData data = map.LockBits(new System.Drawing.Rectangle(0, 0, map.Width, map.Height),
		ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			GL.TexImage2D(target: TextureTarget.Texture2D
				, level: 0
				, internalformat: PixelInternalFormat.Rgba	// Storage format.
				, width: data.Width
				, height: data.Height
				, border: 0
				, format: OpenTK.Graphics.OpenGL.PixelFormat.Bgra // Source format
				, type: PixelType.UnsignedByte	// Source data
				, pixels: data.Scan0);

			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);

			map.UnlockBits(data);

			Error.checkGLError("MaterialManager.loadTextureFromBitmap");

			return texID;
		}

	}
}