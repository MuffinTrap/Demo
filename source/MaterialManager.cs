
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

		public string getInfoString()
		{
			return "Name: " + materialName + " Texture: " + textureName + " GLi: " + textureGLIndex;
		}
	}

	public class MaterialManager
	{
		public List<Material> materials;

		public MaterialManager()
		{
			materials = new List<Material>();
			createMaterial("white", Color.White);
			createMaterial("green", Color.ForestGreen);
		}

		public void printLoadedAssets()
		{
			
				foreach (Material m in materials)
				{
					Logger.LogInfo("Loaded material " + m.getInfoString());
				}
		}

		public Material GetMaterialByName(string materialName)
		{
			foreach (Material m in materials)
			{
				if (m.materialName == materialName)
				{
					return m;
				}
			}
			Logger.LogError(Logger.ErrorState.Limited, "No material with name " + materialName + " exists in MaterialManager");
			return null;
		}

		private bool doesMaterialExist(string materialName)
		{
			foreach (Material m in materials)
			{
				if (m.materialName == materialName)
				{
					return true;
				}
			}
			return false;
		}

		public void loadAllFromDir(string directoryName)
		{
			string topDir = Directory.GetCurrentDirectory();

			string[] directories = Directory.GetDirectories(directoryName);
			foreach (string dir in directories)
			{
				Directory.SetCurrentDirectory(dir);
				string[] files = Directory.GetFiles(dir);
				foreach (string fileEntry in files)
				{
					if (fileEntry.EndsWith(".mtl"))
					{
						loadMaterial(fileEntry);
					}
				}
			}

			Directory.SetCurrentDirectory(topDir);
		}

		public Material loadMaterial(string materialFileName)
		{
			StreamReader sourceFile = null;

			try
			{
				sourceFile = new StreamReader(materialFileName);
			}
			catch(Exception streamException)
			{
				Logger.LogError(Logger.ErrorState.Limited, "Failed to load material from file " + materialFileName + " Error:" + streamException.Message);
				return null;
			}

			string line = null;
			string materialName = null;
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

			if (doesMaterialExist(materialName))
			{
				sourceFile.Close();
				return GetMaterialByName(materialName);
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

			Logger.LogInfo("Loaded material from file " + materialFileName);

			return newMaterial;
		}

		void createMaterial(string materialName, Color materialColor)
		{
			if (doesMaterialExist(materialName))
			{
				return;
			}

			Material newMaterial = new Material();
			newMaterial.materialName = materialName;
			newMaterial.diffuse = new Vector3(1,1,1);
			newMaterial.alpha = new Vector3(0,0,0);
			newMaterial.specular = new Vector3(0,0,0);
			newMaterial.textureName = "";
			
			newMaterial.textureGLIndex = createTexture(materialColor);
			
			materials.Add(newMaterial);

			Logger.LogInfo("Created material with name " + materialName);
		}
		
		int createTexture(System.Drawing.Color textureColor)
		{
			int size = 4;
			Bitmap map = new Bitmap(size, size);
				for (int x = 0; x < size; x++)
				{
					for (int y = 0; y < size; y++)
					{
						map.SetPixel(x, y, textureColor);
					}
				}
			int textureId = loadTextureFromBitmap(map);
			return textureId;
		}

		int loadTexture(string textureFileName)
		{
            Logger.LogInfo("loadTexture " + textureFileName);

			int textureId = -1;
			
			try
			{
				Bitmap map = new Bitmap(textureFileName);
				textureId = loadTextureFromBitmap(map); 
			}
			catch (FileNotFoundException e)
			{
				Logger.LogError(Logger.ErrorState.Limited, "Load texture did not find file:" + e.Message);
			}
			catch (ArgumentException e)
			{
				Logger.LogError(Logger.ErrorState.Limited, "Load texture did not find file:" + e.Message);
			}

			Logger.LogInfo("Loaded texture from file " + textureFileName);

			return textureId;
		}

		int loadTextureFromBitmap(Bitmap map)
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

			map.UnlockBits(data);

			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

			// Don't use mipmaps
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);

			Error.checkGLError("MaterialManager.loadTextureFromBitmap");

			return texID;
		}

	}
}