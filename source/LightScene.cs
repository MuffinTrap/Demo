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

		public Material seaMaterial;
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

		
		public bool SetUniform(ShaderProgram shaderProgram, int location, ShaderUniformName dataName)
		{
			if (dataName == ShaderUniformName.NormalMap)
			{
				GL.ActiveTexture(TextureUnit.Texture1);
				GL.BindTexture(TextureTarget.Texture2D, seaMaterial.GetMap(dataName).textureGLIndex);
				shaderProgram.SetSamplerUniform(location, 1);
				return true;
			}
			return false;
		}

		public void draw()
		{
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
		DrawableMesh lampMesh1;
		DrawableMesh lampMesh2;
		SeaMesh sea;
		CameraComponent camera;

		Vector3 lampPos1;
		Vector3 lampPos2;

		Light lampLight;
		Light lampLight2;

		public LightScene(CameraComponent mainCamera)
		{
			camera = mainCamera;
			// sunLight = Light.createDirectionalLight(new Vector3(1.0f, 1.0f, 1.0f), 0.3f, new Vector3(0.3f, -1.0f, -0.3f));
			lampPos1 = new Vector3(2.0f, 3.0f, -2.0f);
			lampPos2 = new Vector3(2.0f, 6.0f, -4.0f);
			lampLight = Light.createPointLight(new Vector3(1.0f, 0.5f, 0.5f), 0.0f, 64.0f, lampPos1);
			lampLight2 = Light.createPointLight(new Vector3(0.8f, 1.0f, 0.8f), 0.0f, 92.0f
			, lampPos2);
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

			lampMesh1 = assetManager.GetMesh("lamp1"
				, MeshDataGenerator.CreatePyramidMesh(0.1f, 0.1f, true, true)
				, "default"
				, shaderProgram
				, lampPos1
				, 1.0f);

			lampMesh2 = assetManager.GetMesh("lamp2"
				, MeshDataGenerator.CreatePyramidMesh(0.1f, 0.1f, true, true)
				, "default"
				, shaderProgram
				, lampPos2
				, 1.0f);

			sea = new SeaMesh();
			sea.seaShader = assetManager.GetShaderProgram("heightMapTerrain");
			sea.seaTerrain = assetManager.GetMesh("sea"
			, MeshDataGenerator.CreateTerrain(30, 30, 2, true, 1.0f, 1.0f) // Terrain has no normals.
			, "sea_noise"
			, sea.seaShader
			, new Vector3(0, -1.0f, 0)
			, 1.0f);

			//sea.seaTerrain.Transform.setRotationX(MathHelper.Pi / 2.0f);
			sea.seaMaterial = assetManager.GetMaterial("sea_height");
			 
			sea.GetCustomLocations();

			// Lighting

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);
			GL.CullFace(CullFaceMode.Back);
			GL.Enable(EnableCap.CullFace);

			Error.checkGLError("LightScene.loadScene");
		}


		public void drawScene(float cameraFrame) 
		{
			ShaderUniformManager uniformManager = ShaderUniformManager.GetSingleton();
			Renderer renderer = Renderer.GetSingleton();

			renderer.RenderWithShader(shaderProgram);
			renderer.RenderCamera(camera);
			renderer.RenderLight(lampLight, 0);
			renderer.RenderLight(lampLight2, 1);
			renderer.RenderMesh(quadMesh);

			quadMesh.draw();
			lampMesh1.draw();
			lampMesh2.draw();

			/*
			renderer.RenderWithShader(sea.seaShader);
			sea.draw();
			*/

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