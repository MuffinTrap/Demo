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
		ShaderProgram skyboxProgram;
		ShaderProgram debugShader;

		DrawableMesh quadMesh;
		DrawableMesh lampMesh1;
		DrawableMesh lampMesh2;
		DrawableMesh starSphere;

		DrawableMesh pyramidMesh;

		DrawableMesh mountainMesh;
		DrawableMesh mountainNormals;

		SeaMesh sea;
		CameraComponent camera;

		Vector3 lampPos1;
		Vector3 lampPos2;

		Light sunLight;
		Light lampLight;
		Light lampLight2;

		public LightScene(CameraComponent mainCamera)
		{
			camera = mainCamera;
			sunLight = Light.createDirectionalLight(new Vector3(1.0f, 1.0f, 1.0f), 0.0f, new Vector3(0.0f, 0.0f, 0.0f));
			lampPos1 = new Vector3(2.0f, 3.0f, -2.0f);
			lampPos2 = new Vector3(2.0f, 15.0f, -4.0f);
			lampLight = Light.createPointLight(new Vector3(1.0f, 0.8f, 0.8f), 0.0f, 84.0f, lampPos1);
			lampLight2 = Light.createPointLight(new Vector3(0.8f, 1.0f, 0.8f), 0.0f, 52.0f
			, lampPos2);
		}

		public void setCameraFrames(List<PosAndDir> frames) { }

		public void loadScene(AssetManager assetManager)
		{
			shaderProgram = assetManager.GetShaderProgram("litobjmesh");
			skyboxProgram = assetManager.GetShaderProgram("sky");
			debugShader = assetManager.GetShaderProgram("gridmesh");

			quadMesh = assetManager.GetMesh("monu9"
			, assetManager.getMeshData("monu9.obj")
			, "monu9"
			, shaderProgram
			, new Vector3(4.0f, 0.0f, 0.0f)
			, 0.2f);

			lampMesh1 = assetManager.GetMesh("lamp1"
				, MeshDataGenerator.CreatePyramidMesh(1.0f, 1.0f, false, true)
				, "lamp"
				, shaderProgram
				, lampPos1
				, 1.0f);

			lampMesh2 = assetManager.GetMesh("lamp2"
				, MeshDataGenerator.CreatePyramidMesh(1.0f, 1.0f, false, true)
				, "lamp"
				, shaderProgram
				, lampPos2
				, 1.0f);

			starSphere = assetManager.GetMesh("starSphere"
				, MeshDataGenerator.CreateStarSphere(10.0f, 910)
				, "star_palette"
				, skyboxProgram
				, new Vector3(0, 0, 0)
				, 1.0f);

			mountainMesh = assetManager.GetMesh("mountains"
			, MeshDataGenerator.CreateMountains(160, 1, true, 1.0f, 1.0f, 6, 0.5f)
			, "mountain_palette"
			, shaderProgram
			, new Vector3(0, 4, 0)
			, 0.5f);

			// DEBUG 
			mountainNormals = assetManager.GetMesh("mountain_normals"
			, MeshDataGenerator.CreateNormalDebug(mountainMesh.Data.positions, mountainMesh.Data.normals)
			, "default"
			, debugShader
			, mountainMesh.Transform.Position
			, 0.5f);

			pyramidMesh = assetManager.GetMesh("Pyramido"
			, MeshDataGenerator.CreatePyramidMesh(3.0f, 3.0f, true, true)
			, "konata"
			, shaderProgram
			, new Vector3(0, 0, 0)
			, 2.0f);

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

			GL.DepthFunc(DepthFunction.Less);

			renderer.RenderWithShader(shaderProgram);
			renderer.RenderCamera(camera);
			renderer.RenderDirectionalLight(sunLight, 0);
			//renderer.RenderPointLight(lampLight, 1);
			//renderer.RenderPointLight(lampLight2, 2);
			// renderer.RenderMesh(quadMesh);
			// renderer.RenderMesh(pyramidMesh);
			//renderer.RenderMesh(lampMesh1);
			//renderer.RenderMesh(lampMesh2);
			
			
			
			renderer.RenderMesh(mountainMesh);

			/*
			renderer.RenderWithShader(debugShader);
			renderer.RenderCamera(camera);
			renderer.RenderMesh(mountainNormals);
			*/

			GL.DepthFunc(DepthFunction.Lequal);
			renderer.RenderWithShader(skyboxProgram);
			renderer.RenderCamera(camera);
			starSphere.Transform.Position = camera.Position;
			renderer.RenderMesh(starSphere);

			/*
			renderer.RenderWithShader(sea.seaShader);
			sea.draw();
			*/

			Error.checkGLError("LightScene.drawScene");
		}

		public void updateScene(KeyboardState keyState, MouseState mouseState) 
		{
			//quadMesh.Transform.rotateAroundY(0.01f);
			lampMesh1.Transform.Orbit(0.03f, 5.0f, 5.0f, new Vector3(0.0f, 5.0f, 0.0f));
			lampLight.transform.Position = lampMesh1.Transform.Position;
			lampMesh2.Transform.Orbit(-0.01f, 1.0f, 16.0f, new Vector3(0.0f, 1.0f, 0.0f));
			lampLight2.transform.Position = lampMesh2.Transform.Position;
			camera.Update(keyState, mouseState);
			sea.Update();
		}
	}
}