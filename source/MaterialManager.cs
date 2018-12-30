
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using OpenTK.Graphics.OpenGL;

namespace MuffinSpace
{
	public class TextureMap
	{
		public string textureName = "";
		public int textureGLIndex = -1;

		public string getInfoString()
		{
			return " Texture: " + textureName + " GLi: " + textureGLIndex;
		}
		
		public TextureMap(string nameParam, int GLIndexParam)
		{
			textureName = nameParam;
			textureGLIndex = GLIndexParam;
		}
	}

	public class Material
	{
		public string materialName = "";
		public Dictionary<ShaderUniformName, TextureMap> textureMaps;

		public Material(string nameParam)
		{
			materialName = nameParam;
			textureMaps = new Dictionary<ShaderUniformName, TextureMap>();
		}
		public TextureMap GetMap(ShaderUniformName uniformName)
		{
			if (textureMaps.ContainsKey(uniformName))
			{
				return textureMaps[uniformName];
			}
			else
			{
				Logger.LogError(Logger.ErrorState.Limited, "Material " + materialName + " does not have a map for " + ShaderUniformManager.GetSingleton().GetUniformName(uniformName) + "");
				return MaterialManager.GetSingleton().GetDefaultMap(uniformName);
			}
		}
		public string getInfoString()
		{
			return materialName + ", " + textureMaps.Count + " defined maps";
		}
	}

	public class MaterialManager
	{
		// Sets all materials that apply
		public void SetMaterialToShader(Material meshMaterial, ShaderProgram program)
		{
			if (program == null)
			{
				Logger.LogError(Logger.ErrorState.Limited, "MaterialManager> No program given, cannot set material");
				return;
			}
			if (meshMaterial == null)
			{
				SetMaterialToShader(defaultMaterial, program);
				return;
			}
			ShaderUniformManager man = ShaderUniformManager.GetSingleton();

			// Must try each supported texture uniform
			foreach (KeyValuePair<ShaderUniformName, TextureMap> entry in defaultMaterial.textureMaps)
			{
				ShaderUniformName uniform = entry.Key;
				if (!man.DoesShaderSupportUniform(program, uniform))
				{
					continue;
				}
				int location = man.GetDataLocation(program, uniform);
				if (location == ShaderUniformManager.GetInvalidDataLocation())
				{
					Logger.LogError(Logger.ErrorState.Limited, "MaterialManager: Shader says to support texture map " + man.GetUniformName(uniform) + " but there is no location for it");
					continue;
				}
					

				// IMPORTANT PART !
				if (meshMaterial.textureMaps.ContainsKey(uniform))
				{
					SetTextureUniform(program, location, uniform, meshMaterial.textureMaps[uniform]);
				}

				else
				{
					if (defaultMaterial.textureMaps.ContainsKey(uniform))
					{
						SetTextureUniform(program, location, uniform, defaultMaterial.textureMaps[uniform]);
					}
					else 
					{
						Logger.LogError(Logger.ErrorState.Limited, "MaterialManager: Shader wants a texture map " + man.GetUniformName(uniform) + " that is not in default material");
					}
				}
			}
		}

		public void SetTextureUniform(ShaderProgram shaderProgram, int location, ShaderUniformName uniform, TextureMap map)
		{
			int textureUnit = -1;
			if (uniform == ShaderUniformName.DiffuseMap)
			{
				GL.ActiveTexture(TextureUnit.Texture0);
				textureUnit = 0;
			}
			else if (uniform == ShaderUniformName.IlluminationMap)
			{
				GL.ActiveTexture(TextureUnit.Texture1);
				textureUnit = 1;
			}
			else if (uniform == ShaderUniformName.NormalMap)
			{
				GL.ActiveTexture(TextureUnit.Texture2);
				textureUnit = 2;
			}
			else if (uniform == ShaderUniformName.RoughnessMap)
			{
				GL.ActiveTexture(TextureUnit.Texture3);
				textureUnit = 3;
			}
			if (textureUnit == -1)
			{
				Logger.LogError(Logger.ErrorState.Limited, "No defined texture unit for uniform " + ShaderUniformManager.GetSingleton().GetUniformName(uniform) + ", cannot bind");
			}
			GL.BindTexture(TextureTarget.Texture2D, map.textureGLIndex);
			shaderProgram.SetSamplerUniform(location, textureUnit);
		}

		public List<Material> materials;
		private List<TextureMap> colorMaps;
		private Material defaultMaterial;

		private MaterialManager()
		{
			materials = new List<Material>();
			colorMaps = new List<TextureMap>();
			colorMaps.Add(new TextureMap("white", createTexture(Color.White)));
			colorMaps.Add(new TextureMap("black", createTexture(Color.Black)));
			Color normalMapColor = Color.FromArgb(127, 127, 255);
			colorMaps.Add(new TextureMap("normalMap", createTexture(normalMapColor)));
			Color roughnessMapColor = Color.FromArgb(127, 127, 127);
			colorMaps.Add(new TextureMap("roughnessMap", createTexture(roughnessMapColor)));

			defaultMaterial = new Material("default");
			defaultMaterial.textureMaps.Add(ShaderUniformName.DiffuseMap, GetColorTextureByName("white"));
			defaultMaterial.textureMaps.Add(ShaderUniformName.IlluminationMap, GetColorTextureByName("black"));
			defaultMaterial.textureMaps.Add(ShaderUniformName.NormalMap, GetColorTextureByName("normalMap"));
			defaultMaterial.textureMaps.Add(ShaderUniformName.RoughnessMap, GetColorTextureByName("roughnessMap"));

			materials.Add(defaultMaterial);

			Material lampMat = new Material("lamp");
			lampMat.textureMaps.Add(ShaderUniformName.DiffuseMap, GetColorTextureByName("white"));
			lampMat.textureMaps.Add(ShaderUniformName.IlluminationMap, GetColorTextureByName("white"));
			materials.Add(lampMat);
		}

		private static MaterialManager singleton = null;
		public static MaterialManager GetSingleton()
		{
			if (singleton == null)
			{
				singleton = new MaterialManager();
			}
			return singleton;
		}

		public void printLoadedAssets()
		{
			foreach (Material m in materials)
			{
				Logger.LogInfoLinePart("Loaded material " + m.materialName + " (", ConsoleColor.Gray);
				Logger.LogInfoLinePart("" + m.textureMaps.Count, ConsoleColor.Red);
				Logger.LogInfoLinePart(") maps", ConsoleColor.Gray);
				Logger.LogInfoLineEnd();
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

		public TextureMap GetColorTextureByName(string textureName)
		{
			foreach (TextureMap m in colorMaps)
			{
				if (m.textureName == textureName)
				{
					return m;
				}
			}
			Logger.LogError(Logger.ErrorState.Limited, "No Color Texture with name " + textureName + " exists in MaterialManager");
			return null;
		}

		public bool AddNewMaterial(Material material)
		{
			if (doesMaterialExist(material.materialName))
			{
				return false;
			}
			materials.Add(material);
			return true;
		}

		public TextureMap GetDefaultMap(ShaderUniformName uniformName)
		{
			if (defaultMaterial.textureMaps.ContainsKey(uniformName))
			{
				return defaultMaterial.textureMaps[uniformName];
			}
			else
			{
				Logger.LogError(Logger.ErrorState.Critical, "MaterialManager's default material does not have map for " + ShaderUniformManager.GetSingleton().GetUniformName(uniformName));
				return null;
			}
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

			Logger.LogInfoLinePart("Loading material from file ", ConsoleColor.Gray);
			Logger.LogInfoLinePart(materialFileName, ConsoleColor.Cyan);
			Logger.LogInfoLineEnd();

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
				newMaterial = new Material(materialName);
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
					// Diffuse map
					// map_Kd filename.png
					string textureName = line.Split(space)[1];
					int textureGLIndex = loadTexture(textureName);
					TextureMap diffuse = new TextureMap(textureName, textureGLIndex);
					newMaterial.textureMaps.Add(ShaderUniformName.DiffuseMap, diffuse);

					Logger.LogInfoLinePart("  Diffuse map :", ConsoleColor.Gray);
					Logger.LogInfoLinePart(textureName, ConsoleColor.Cyan);
					Logger.LogInfoLineEnd();
				}
				else if (line.Contains("map_Ki"))
				{
					// Illumination map
					// map_Ki filename_i.png
					string textureName = line.Split(space)[1];
					int textureGLIndex = loadTexture(textureName);
					TextureMap illumination = new TextureMap(textureName, textureGLIndex);
					newMaterial.textureMaps.Add(ShaderUniformName.IlluminationMap, illumination);

					Logger.LogInfoLinePart("  Illumination map :", ConsoleColor.Gray);
					Logger.LogInfoLinePart(textureName, ConsoleColor.Cyan);
					Logger.LogInfoLineEnd();
				}
				else if (line.Contains("map_Kn"))
				{
					// Normal map
					// map_Kn filename_n.png
					string textureName = line.Split(space)[1];
					int textureGLIndex = loadTexture(textureName);
					TextureMap normal = new TextureMap(textureName, textureGLIndex);
					newMaterial.textureMaps.Add(ShaderUniformName.NormalMap, normal);

					Logger.LogInfoLinePart("  Normal map :", ConsoleColor.Gray);
					Logger.LogInfoLinePart(textureName, ConsoleColor.Cyan);
					Logger.LogInfoLineEnd();
				}
				else if (line.Contains("map_Kr"))
				{
					// Roughness map
					// map_Kr filename_r.png
					string textureName = line.Split(space)[1];
					int textureGLIndex = loadTexture(textureName);
					TextureMap roughness = new TextureMap(textureName, textureGLIndex);
					newMaterial.textureMaps.Add(ShaderUniformName.RoughnessMap, roughness);

					Logger.LogInfoLinePart("  Roughness map :", ConsoleColor.Gray);
					Logger.LogInfoLinePart(textureName, ConsoleColor.Cyan);
					Logger.LogInfoLineEnd();
				}
			} while (line != null);

			sourceFile.Close();
			materials.Add(newMaterial);

			return newMaterial;
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

			// Don't loop
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

			// Don't interpolate
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);

			GL.BindTexture(TextureTarget.Texture2D, 0);

			Error.checkGLError("MaterialManager.loadTextureFromBitmap");

			return texID;
		}

	}
}