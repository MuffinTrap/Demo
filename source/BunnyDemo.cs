using System.Collections.Generic;

using OpenTK;

namespace MuffinSpace
{
	public class BunnyDemo : IDemo
	{
		List<Scene> scenes;
		Audio music;
		DemoSettings settings;

		// From MainWindow
		IAudioSystem audioSystem = null;

		// Fadeout scene
		ShaderProgram guiShader;
		int fadeAlphaLocation = 0;
		float fadeAlpha;
		DrawableMesh fadeoutQuad;

		MountainScene mountains;

		public class MountainScene : IDemo
		{
			// DrawableMesh mountains;
			// DrawableMesh starSphere;

			public void Load(IAudioSystem audioSystemParam, AssetManager assetManager)
			{
				ShaderProgram objShader = assetManager.GetShaderProgram("litobjmesh");
				ShaderProgram skyboxProgram = assetManager.GetShaderProgram("sky");

			}
			public void Sync(SyncSystem syncer)
			{
				// Rotate star sphere world matrix

			}
			public void Draw(SyncSystem syncer, Renderer renderer)
			{
				// Draw mountains
				// Draw stars
			}
		}

		public BunnyDemo()
		{
			settings = DemoSettings.GetDefaults();
		}

		public void Load(IAudioSystem audioSystemParam, AssetManager assetManager)
		{
			if (settings.AudioEnabled)
			{
				audioSystem = audioSystemParam;
				string audioFileName = "../data/music/bosca.wav";
				music = audioSystem.LoadAudioFile(audioFileName);
			}

			scenes = new List<Scene>();

			mountains = new MountainScene();


			// Fade scene
			guiShader = assetManager.GetShaderProgram("gui");
			fadeAlphaLocation = guiShader.GetCustomUniformLocation("uCustomAlpha");

			Material fadeoutMaterial = new Material("blackfadeout");
			fadeoutMaterial.textureMaps.Add(ShaderUniformName.DiffuseMap, MaterialManager.GetSingleton().GetColorTextureByName("black"));
			MaterialManager.GetSingleton().AddNewMaterial(fadeoutMaterial);

			fadeoutQuad = assetManager.GetMesh("black_overlay"
				, MeshDataGenerator.CreateQuadMesh(false, true)
				, fadeoutMaterial.materialName
				, guiShader
				, new Vector3(-2.0f, -0.5f, 0.0f)
				, 3.0f);
		}

		public void Start()
		{
			if (settings.AudioEnabled)
			{
				audioSystem.SetAudio(music);
				audioSystem.PlayAudio(music);
			}
		}

		public void Sync(SyncSystem syncer)
		{
			// Get scene combination by Scene value
			if (syncer.Scene == 0)
			{
				UpdateFadeout(syncer.SceneProgress);
			}
		}

		private void UpdateFadeout(float progress)
		{
			fadeAlpha = progress;
		}

		public void Draw(SyncSystem syncer, Renderer renderer)
		{
			if (syncer.Scene == 0)
			{
				DrawFadeout(renderer, fadeAlpha);
			}
		}

		private void DrawFadeout(Renderer renderer, float progress)
		{
			renderer.GetCamera().EnableOrthogonal();
			renderer.SetActiveShader(guiShader);
			guiShader.SetFloatUniform(fadeAlphaLocation, progress);
			renderer.RenderGui(fadeoutQuad);
			renderer.GetCamera().EnablePerspective();
		}

		public DemoSettings GetDemoSettings()
		{
			return settings;
		}
	}

	/*
			Scene lightScene = new Scene();

			Light sunLight;
			Light lampLight;
			Light lampLight2;

			Vector3 lampPos1 = new Vector3(2.0f, 3.0f, -2.0f);
			Vector3 lampPos2 = new Vector3(2.0f, 15.0f, -4.0f);

			sunLight = Light.createDirectionalLight(new Vector3(1.0f, 1.0f, 1.0f), 0.0f, new Vector3(0.0f, 0.0f, 0.0f));
			lampLight = Light.createPointLight(new Vector3(1.0f, 0.8f, 0.8f), 0.0f, 84.0f, lampPos1);
			lampLight2 = Light.createPointLight(new Vector3(0.8f, 1.0f, 0.8f), 0.0f, 52.0f
			, lampPos2);

			DrawableMesh quadMesh = assetManager.GetMesh("monu9"
			, assetManager.getMeshData("monu9.obj")
			, "monu9"
			, objShader
			, new Vector3(4.0f, 0.0f, 0.0f)
			, 0.2f);

			DrawableMesh lampMesh1 = assetManager.GetMesh("lamp1"
				, MeshDataGenerator.CreatePyramidMesh(1.0f, 1.0f, false, true)
				, "lamp"
				, objShader
				, lampPos1
				, 1.0f);

			DrawableMesh lampMesh2 = assetManager.GetMesh("lamp2"
				, MeshDataGenerator.CreatePyramidMesh(1.0f, 1.0f, false, true)
				, "lamp"
				, objShader
				, lampPos2
				, 1.0f);

			DrawableMesh starSphere = assetManager.GetMesh("starSphere"
				, MeshDataGenerator.CreateStarSphere(10.0f, 910)
				, "star_palette"
				, skyboxProgram
				, new Vector3(0, 0, 0)
				, 1.0f);

			DrawableMesh mountainMesh = assetManager.GetMesh("mountains"
			, MeshDataGenerator.CreateMountains(160, 1, true, 1.0f, 1.0f, 6, 0.5f)
			, "mountain_palette"
			, objShader
			, new Vector3(0, 4, 0)
			, 0.5f);

			// DEBUG 
			DrawableMesh mountainNormals = assetManager.GetMesh("mountain_normals"
			, MeshDataGenerator.CreateNormalDebug(mountainMesh.Data.positions, mountainMesh.Data.normals)
			, "default"
			, debugShader
			, mountainMesh.Transform.Position
			, 0.5f);

			lightScene.AddDrawable(quadMesh);
			lightScene.AddDrawable(mountainMesh);
			lightScene.AddDrawable(lampMesh1);
			lightScene.AddDrawable(lampMesh2);
			lightScene.AddDrawable(starSphere);

			*/
}