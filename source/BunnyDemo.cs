using System.Collections.Generic;

using RocketNet;
using OpenTK;

namespace MuffinSpace
{
	public class BunnyDemo : IDemo
	{
		Audio music;
		DemoSettings settings;

		// From MainWindow
		IAudioSystem audioSystem = null;

		// Fadeout scene
		ShaderProgram guiShader;
		int fadeAlphaLocation = 0;
		float fadeAlpha = 0.0f;
		DrawableMesh fadeoutQuad;
		Track fadeoutAlpha;

		DrawableMesh starSphere;

		MountainScene mountains;
		MonolithSpaceScene monolithSpace;
		SeaScene seaScene;
		BunnyScene bunnyScene;

		public class BunnyScene : IDemo
		{
			DrawableMesh bunnyMesh;
			public void Load(IAudioSystem audioSystemParam, AssetManager assetManager, SyncSystem syncSystem)
			{
				ShaderProgram bunnyShader = assetManager.GetShaderProgram("bunny");
				TunableManager tm = TunableManager.GetSingleton();
				Vector3 bunnypos = tm.GetVec3("bunny.position");
				Logger.LogInfo("loaded bunny pos " + Logger.PrintVec3(bunnypos));
				bunnyMesh = assetManager.GetMesh("bunny", "bunny.obj", "default", bunnyShader, bunnypos);

			}

			public void Sync(SyncSystem syncer)
			{

			}
			public void Draw(Renderer renderer)
			{
				renderer.RenderObject(bunnyMesh);
			}
		}

		public class MonolithSpaceScene : IDemo
		{
			DrawableMesh greetMesh;
			DrawableMesh fontTestMesh;
			DrawableMesh monolith;
			DrawableMesh mono_normals;

			float textAlpha = 1.0f;

			Light moon;

			public void Load(IAudioSystem audioSystemParam, AssetManager assetManager, SyncSystem syncSystem)
			{
				Logger.LogInfo("Loading Monolith in space scene");

				ShaderProgram texShader = assetManager.GetShaderProgram("texturedobjmesh");
				ShaderProgram objShader = assetManager.GetShaderProgram("litobjmesh");
				ShaderProgram gridShader = assetManager.GetShaderProgram("gridmesh");

				TextGenerator textgen = TextGenerator.GetSingleton();
				PixelFont commodore = textgen.GetFont("commodore");

				moon = Light.CreateDirectionalLight(new Vector3(0.5f, 0.5f, 1.0f), 0.3f, 0.4f, new Vector3(-0.4f, -0.2f, 0.4f));

				Logger.LogInfo("Created directional moon light " + moon.GetInfoString());
				greetMesh = assetManager.CreateMesh("greeting"
				, MeshDataGenerator.CreateTextMesh("Gambatte Minnasan", commodore)
				, "commodore_font"
				, texShader
				, new Vector3(1.5f, 12, 0));

				fontTestMesh = assetManager.CreateMesh("font_test"
				, MeshDataGenerator.CreateQuadMesh(false, true)
				, "commodore_font"
				, texShader
				, new Vector3(-1, 3, 0));

				monolith = assetManager.CreateMesh("monolith"
				, MeshDataGenerator.CreateCubeMesh(new Vector3(1.5f, 2.0f, 0.15f), true, true)
				, "konata"
				, objShader
				, new Vector3(0.0f, 12.0f, 0.0f));

				mono_normals = assetManager.CreateMesh("mono_normals"
				, MeshDataGenerator.CreateNormalDebug(monolith.Data.positions, monolith.Data.normals)
				, "default"
				, gridShader
				, monolith.Transform.Position);
			}

			public void Sync(SyncSystem syncer)
			{

			}

			public void Draw(Renderer renderer)
			{
				//renderer.RenderObject(fontTestMesh);
				renderer.SetActiveShader(greetMesh.ShaderProgram);
				greetMesh.ShaderProgram.SetFloatUniform(ShaderUniformName.Alpha, textAlpha);
				renderer.RenderObject(greetMesh);

				// renderer.ActivateLight(moon, 0);
				renderer.RenderObject(monolith);
				// renderer.RenderObject(mono_normals);
				Error.checkGLError("Monolith Draw");
			}
		}

		public class MountainScene : IDemo
		{
			DrawableMesh mountains;
			DrawableMesh mountain_normals;
			DrawableMesh pyra;
			Light starLight;
			Light testLight;

			public void Load(IAudioSystem audioSystemParam, AssetManager assetManager, SyncSystem syncSystem)
			{
				Logger.LogInfo("Loading Mountain scene");
				ShaderProgram objShader = assetManager.GetShaderProgram("litobjmesh");
				ShaderProgram gridShader = assetManager.GetShaderProgram("gridmesh");

				starLight = Light.CreateDirectionalLight(new Vector3(1.0f, 1.0f, 1.0f), 0.2f, 0.8f, new Vector3(0.4f, -1.0f, 0.0f));
				Logger.LogInfo("Created directional star light " + starLight.GetInfoString());

				testLight = Light.CreatePointLight(new Vector3(1.0f, 1.0f, 1.0f), 56.0f, new Vector3(0, 5.0f, 0.0f));
				Logger.LogInfo("Created Point  light " + testLight.GetInfoString());

				mountains = assetManager.CreateMesh("mountains"
			, MeshDataGenerator.CreateMountains(40, true, 4, 0.5f, 1)
			, "mountain_palette"
			, objShader
			, new Vector3(0, 0, 0));

				mountain_normals = assetManager.CreateMesh("mono_normals"
				, MeshDataGenerator.CreateNormalDebug(mountains.Data.positions, mountains.Data.normals)
				, "default"
				, gridShader
				, mountains.Transform.Position);

				pyra = assetManager.CreateMesh("pyra"
				, MeshDataGenerator.CreatePyramidMesh(1.0f, 1.0f, false, false)
				, "default"
				, gridShader
				, new Vector3(0, 5, 0));


			}

			public void Sync(SyncSystem syncer)
			{
			}

			public void Draw(Renderer renderer)
			{
				renderer.SetClearColor(OpenTK.Graphics.Color4.DarkSlateBlue);
				renderer.ActivateLight(starLight, 0);
				renderer.ActivateLight(testLight, 1);
				//	renderer.RenderObject(mountain_normals);
				renderer.RenderObject(pyra);
				renderer.RenderObject(mountains);
				//				renderer.RenderSky(starSphere);

				Error.checkGLError("MountainScene Draw");
			}
		}

		public class SeaScene : IDemo
		{

			DrawableMesh seaMesh;
			ShaderProgram seaShader;

			int texOffset1Location;
			int texOffset2Location;

			Vector2 offset1;
			Vector2 offset2;

			public void Load(IAudioSystem audioSystemParam, AssetManager assetManager, SyncSystem syncSystem)
			{

				seaShader = assetManager.GetShaderProgram("heightMapTerrain");
				texOffset1Location = seaShader.GetCustomUniformLocation("ucUVoffset1");
				texOffset2Location = seaShader.GetCustomUniformLocation("ucUVoffset2");

				seaMesh = assetManager.CreateMesh("sea"
				, MeshDataGenerator.CreateTerrain(40, 40, 1, true, true, 1.0f, 1.0f)
				, "sea"
				, seaShader
				, new Vector3(0, 0.4f, 0));

				offset1 = new Vector2(0, 0);
				offset2 = new Vector2(0, 0);
			}

			public void Sync(SyncSystem syncer)
			{
				offset1.X += 0.0001f;
				offset2.X += 0.003f;
			}
			public void Draw(Renderer renderer)
			{
				renderer.SetActiveShader(seaMesh.ShaderProgram);
				seaShader.SetVec2Uniform(texOffset1Location, offset1);
				seaShader.SetVec2Uniform(texOffset2Location, offset2);
				renderer.RenderObject(seaMesh);

				Error.checkGLError("SeaScene Draw");
			}

		}
		public BunnyDemo()
		{
			settings = DemoSettings.GetDefaults();
		}

		public void Load(IAudioSystem audioSystemParam, AssetManager assetManager, SyncSystem syncSystem)
		{
			if (settings.AudioEnabled)
			{
				audioSystem = audioSystemParam;
				string audioFileName = "../data/music/bosca.wav";
				music = audioSystem.LoadAudioFile(audioFileName);
			}
			Logger.LogInfo("Loading Bunny Demo");

			mountains = new MountainScene();
			mountains.Load(audioSystem, assetManager, syncSystem);

			monolithSpace = new MonolithSpaceScene();
			monolithSpace.Load(audioSystem, assetManager, syncSystem);

			seaScene = new SeaScene();
			seaScene.Load(audioSystem, assetManager, syncSystem);

			bunnyScene = new BunnyScene();
			bunnyScene.Load(audioSystem, assetManager, syncSystem);

			// Fade scene
			guiShader = assetManager.GetShaderProgram("gui");
			if (guiShader == null)
			{
				Logger.LogError(Logger.ErrorState.Critical, "Did not get gui shader");
			}
			fadeAlphaLocation = guiShader.GetCustomUniformLocation("uAlpha");
			fadeoutAlpha = syncSystem.GetTrack("FadeOut");

			Material fadeoutMaterial = new Material("blackfadeout");
			fadeoutMaterial.textureMaps.Add(ShaderUniformName.DiffuseMap, MaterialManager.GetSingleton().GetColorTextureByName("black"));
			MaterialManager.GetSingleton().AddNewMaterial(fadeoutMaterial);

			fadeoutQuad = assetManager.CreateMesh("black_overlay"
				, MeshDataGenerator.CreateQuadMesh(false, true)
				, fadeoutMaterial.materialName
				, guiShader
				, new Vector3(-2.0f, -0.5f, 0.0f));

			ShaderProgram skyboxProgram = assetManager.GetShaderProgram("sky");
			starSphere = assetManager.CreateMesh("stars"
										 , MeshDataGenerator.CreateStarSphere(90.0f, 500, 0.05f, 0.8f)
										 , "star_palette"
										 , skyboxProgram
										 , new Vector3(0, 0, 0));

			starSphere.Transform.SetRotationAxis(new Vector3(0.3f, 0.8f, 0.0f).Normalized());

			Logger.LogPhase("Bunny demo is loaded");
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
			UpdateFadeout(syncer.SceneProgress);
			UpdateStarRotation();
			mountains.Sync(syncer);
			monolithSpace.Sync(syncer);
			seaScene.Sync(syncer);
			bunnyScene.Sync(syncer);

		}

		private void UpdateFadeout(float progress)
		{
			fadeAlpha = progress;
		}

		private void UpdateStarRotation()
		{
				// Rotate star sphere world matrix
				starSphere.Transform.SetRotation(0.0f);
		}

		public void Draw(Renderer renderer)
		{
			if (fadeAlpha > 0.0f)
			{
				DrawFadeout(renderer, fadeAlpha);
			}
			
			mountains.Draw(renderer);
			monolithSpace.Draw(renderer);
			bunnyScene.Draw(renderer);
			seaScene.Draw(renderer);

			renderer.RenderSky(starSphere);
			Error.checkGLError("BunnyDemo Draw");
		}

		private void DrawFadeout(Renderer renderer, float progress)
		{
			if (guiShader == null)
			{
				Logger.LogError(Logger.ErrorState.Critical, "No shader to render with");
				return;
			}
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
}