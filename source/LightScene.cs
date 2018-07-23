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
	public class SeaMesh : IShaderDataOwner
	{
		public DrawableMesh seaTerrain;
		public ShaderProgram seaShader;

		public Material normalMapMaterial;
		Random randomGenerator = new Random();

		// Texcoord offsets
		int offset1Location;
		int offset2Location;

		Vector2 offset1;
		Vector2 offset2;

		Vector2 offset1Dir;
		Vector2 offset2Dir;

		float offset1Timer;
		float offset2Timer;

		public SeaMesh()
		{
			offset1 = new Vector2(0, 0);
			offset2 = new Vector2(0.5f, 0.5f);
		}

		public void GetCustomLocations()
		{
			offset1Location = seaShader.GetCustomUniformLocation("uCustomUVoffset1");
			offset2Location = seaShader.GetCustomUniformLocation("uCustomUVoffset2");
		}

		public void Update()
		{
			double dt = 1.0 / DemoSettings.GetDefaults().UpdatesPerSecond;
			float fdt = (float)dt;
			// Move offsets
			float offsetSpeed = 0.05f;
			offset1 += offset1Dir * offsetSpeed * fdt;
			offset2 += offset2Dir * offsetSpeed * fdt;

			offset1Timer -= fdt;
			if (offset1Timer < 0)
			{
				offset1Dir = RandomDir2().Normalized();
				offset1Timer = (float)randomGenerator.NextDouble() * 5.5f;
			}
			offset2Timer -= fdt;
			if (offset2Timer < 0)
			{
				offset2Dir = RandomDir2().Normalized();
				offset2Timer = (float)randomGenerator.NextDouble() * 8.2f;
			}

		}

		private Vector2 RandomDir2()
		{
			return new Vector2(
				 (float)randomGenerator.NextDouble() * 2.0f - 1.0f
				 , (float)randomGenerator.NextDouble() * 2.0f - 1.0f);

		}

		public void ActivateForDrawing()
		{
			ShaderUniformManager.GetSingleton().RegisterDataOwner(this, ShaderUniformName.NormalMap);
			seaTerrain.ActivateForDrawing();

		}
		public void SetUniform(ShaderProgram shaderProgram, int location, ShaderUniformName dataName)
		{
			if (dataName == ShaderUniformName.NormalMap)
			{
				GL.ActiveTexture(TextureUnit.Texture1);
				GL.BindTexture(TextureTarget.Texture2D, normalMapMaterial.textureGLIndex);
				shaderProgram.SetSamplerUniform(location, 1);
			}
		}

		public void draw()
		{
			ShaderUniformManager.GetSingleton().SetData(seaTerrain.ShaderProgram, ShaderUniformName.NormalMap);
			seaShader.SetVec2Uniform(offset1Location, offset1);
			seaShader.SetVec2Uniform(offset2Location, offset2);
			seaTerrain.draw();

			GL.ActiveTexture(TextureUnit.Texture1);
			GL.BindTexture(TextureTarget.Texture2D, 0);
		}
	}
	public class LightScene : IScene
	{
		ShaderProgram shaderProgram;
		DrawableMesh quadMesh;
		SeaMesh sea;
		CameraComponent camera;

		DirectionalLight sunLight;

		public LightScene(CameraComponent mainCamera)
		{
			camera = mainCamera;
			sunLight = new DirectionalLight(new Vector3(1.0f, 1.0f, 1.0f), new Vector3(0.3f, -1.0f, -0.3f), 0.3f);
		}

		public void setCameraFrames(List<PosAndDir> frames) { }

		public void loadScene(AssetManager assetManager)
		{
			shaderProgram = assetManager.GetShaderProgram("litobjmesh");

			quadMesh = assetManager.GetMesh("monu9"
			, assetManager.getMeshData("monu9.obj")
			, "monu9"
			, shaderProgram
			, new Vector3(4.0f, 0.0f, 0.0f)
			, 0.2f);

			sea = new SeaMesh();
			sea.seaShader = assetManager.GetShaderProgram("heightMapTerrain");
			sea.seaTerrain = assetManager.GetMesh("sea"
			, MeshDataGenerator.CreateTerrain(30, 30, 2, true, 1.0f, 1.0f) // Terrain has no normals.
			, "sea_noise"
			, sea.seaShader
			, new Vector3(0, -1.0f, 0)
			, 1.0f);

			//sea.seaTerrain.Transform.setRotationX(MathHelper.Pi / 2.0f);
			sea.normalMapMaterial = assetManager.GetMaterial("sea_height");
			sea.GetCustomLocations();

			// Lighting
			sunLight.ActivateForDrawing();

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);
			GL.CullFace(CullFaceMode.Back);
			GL.Enable(EnableCap.CullFace);

			Error.checkGLError("LightScene.loadScene");
		}


		public void drawScene(float cameraFrame) 
		{
			ShaderUniformManager uniformManager = ShaderUniformManager.GetSingleton();

			camera.ActivateForDrawing();

			quadMesh.ActivateForDrawing();
			uniformManager.ActivateShader(shaderProgram);
			quadMesh.draw();

			sea.ActivateForDrawing();
			uniformManager.ActivateShader(sea.seaShader);
			sea.draw();

			Error.checkGLError("LightScene.drawScene");
		}

		public void updateScene(KeyboardState keyState, MouseState mouseState) 
		{
			//quadMesh.Transform.rotateAroundY(0.01f);
			camera.Update(keyState, mouseState);
			sea.Update();
		}
	}
}