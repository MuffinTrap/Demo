using System.Collections.Generic;

using RocketNet;
using OpenTK;

namespace MuffinSpace
{
	public class GreetPage
	{
		public	float progress;
		public List<Greeting> greets;
	}
	public class Greeting
	{
		public DrawableMesh textMesh;
		public float alpha;
	}

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
		float starRotSpeed;
		float starRot;

		Light moon;

		MountainScene mountains;
		MonolithSpaceScene monolithSpace;
		SeaScene seaScene;
		BunnyScene bunnyScene;


		// Detect starting music
		bool syncWasPaused = true;

		public class BunnyScene : IDemo
		{
			DrawableMesh bunnyMesh;
			public void Load(AssetManager assetManager, SyncSystem syncSystem)
			{
				TunableManager tm = TunableManager.GetSingleton();
				ShaderProgram bunnyShader = assetManager.GetShaderProgram(tm.GetString("bunny.shader"));
				bunnyMesh = assetManager.GetMesh("bunny"
				, tm.GetString("bunny.model")
				, tm.GetString("bunny.material")
				, bunnyShader
				, tm.GetVec3("bunny.position"));
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
			DrawableMesh monolith;

			Vector3 greetOffset;
			Vector3 greetSpacing;
			float greetScale;

			List<GreetPage> greets;


			public void Load(AssetManager assetManager, SyncSystem syncSystem)
			{
				Logger.LogInfo("Loading Monolith in space scene");
				TunableManager tm = TunableManager.GetSingleton();

				ShaderProgram texShader = assetManager.GetShaderProgram("texturedobjmesh");
				ShaderProgram objShader = assetManager.GetShaderProgram("litobjmesh");
				ShaderProgram gridShader = assetManager.GetShaderProgram("gridmesh");

				TextGenerator textgen = TextGenerator.GetSingleton();



				monolith = assetManager.CreateMesh("monolith"
				, MeshDataGenerator.CreateCubeMesh(tm.GetVec3("monolith_greet.size")
				, true, true)
				, tm.GetString("monolith_greet.material")
				, objShader
				, tm.GetVec3("monolith_greet.position"));

				string greet_font = tm.GetString("monolith_greet.greet_font");
				PixelFont greetFont = textgen.GetFont(greet_font);
				string greet_material = tm.GetString("monolith_greet.greet_material");
				ShaderProgram greetShader = assetManager.GetShaderProgram(tm.GetString("monolith_greet.greet_shader"));

				greets = new List<GreetPage>();
				int gp = tm.GetInt("monolith_greet.greet_pages");
				for (int pi = 0; pi < gp; pi++)
				{
					GreetPage page = new GreetPage();
					page.greets = new List<Greeting>();
					string pageName = "monolith_greet_page_" + (pi + 1);
					int greet_amount = tm.GetInt(pageName + "." + "greet_amount");
					for (int gi = 0; gi < greet_amount; gi++)
					{
						Greeting greet = new Greeting();

						greet.textMesh = assetManager.CreateMesh("greet" + gi
						, MeshDataGenerator.CreateTextMesh(tm.GetString(pageName + "." + "greet_" + (gi + 1))
						, greetFont)
						, greet_material
						, greetShader
						, new Vector3(0,0,0));

						greet.alpha = 1.0f;
						page.greets.Add(greet);
					}
					greets.Add(page);
				}

				greetOffset = tm.GetVec3("monolith_greet.greet_offset");
				greetSpacing = tm.GetVec3("monolith_greet.greet_spacing");
				greetScale = tm.GetFloat("monolith_greet.greet_scale");

				for (int pi = 0; pi < greets.Count; pi++)
				{
					GreetPage p = greets[pi];
					for (int gi = 0; gi < p.greets.Count; gi++)
					{
						Greeting g = p.greets[gi];
							// Set monolith as parent transform so it can rotate
						g.textMesh.Transform.Position = greetOffset + (greetSpacing * gi);
						g.textMesh.Transform.Scale = greetScale;
					}
				}
			}

			public void Sync(SyncSystem syncer)
			{
				for (int pi = 0; pi < greets.Count; pi++)
				{
					greets[pi].progress = syncer.SceneProgress;
				}
			}

			public void Draw(Renderer renderer)
			{
				// Render monolith
				renderer.RenderObject(monolith);

				// Render greeting pages
				foreach(GreetPage p in greets)
				{
					for (int gi = 0; gi < p.greets.Count; gi++)
					{
						Greeting g = p.greets[gi];
						g.alpha = p.progress;

						renderer.SetActiveShader(g.textMesh.ShaderProgram);
						g.textMesh.ShaderProgram.SetFloatUniform(ShaderUniformName.Alpha, g.alpha);
						renderer.RenderObject(g.textMesh);
					}
				}

				Error.checkGLError("Monolith Draw");
			}
		}

		public class MountainScene : IDemo
		{
			DrawableMesh mountains;
			Greeting groupNameGreet;
			Greeting demoNameGreet;

			public void Load(AssetManager assetManager, SyncSystem syncSystem)
			{
				Logger.LogInfo("Loading Mountain scene");
				TunableManager tm = TunableManager.GetSingleton();
				ShaderProgram objShader = assetManager.GetShaderProgram("litobjmesh");
				ShaderProgram gridShader = assetManager.GetShaderProgram("gridmesh");



				mountains = assetManager.CreateMesh("mountains"
					, MeshDataGenerator.CreateMountains(
						tm.GetFloat("mountain_scene.mountain_size")
						, true, tm.GetInt("mountain_scene.mountain_iterations")
						, tm.GetFloat("mountain_scene.mountain_height_variation")
						, tm.GetInt("mountain_scene.mountain_random_seed"))
					, tm.GetString("mountain_scene.mountain_material")
					, objShader
					, tm.GetVec3("mountain_scene.mountain_position"));

				TextGenerator textgen = TextGenerator.GetSingleton();
				string greet_font = tm.GetString("monolith_greet.greet_font");
				PixelFont greetFont = textgen.GetFont(greet_font);
				string greet_material = tm.GetString("monolith_greet.greet_material");
				ShaderProgram greetShader = assetManager.GetShaderProgram(tm.GetString("monolith_greet.greet_shader"));
				DrawableMesh groupName = assetManager.CreateMesh("groupText"
				, MeshDataGenerator.CreateTextMesh("FCCCF", greetFont)
				, greet_material
				, greetShader
				, tm.GetVec3("mountain_scene.group_name_position"));

				groupNameGreet = new Greeting();
				groupNameGreet.textMesh = groupName;
				groupNameGreet.alpha = 0.0f;

				DrawableMesh demoName = assetManager.CreateMesh("groupText"
				, MeshDataGenerator.CreateTextMesh("Lepus Minor", greetFont)
				, greet_material
				, greetShader
				, tm.GetVec3("mountain_scene.demo_name_position"));

				demoNameGreet = new Greeting();
				demoNameGreet.textMesh = demoName;
				demoNameGreet.alpha = 0.0f;
			}

			public void Sync(SyncSystem syncer)
			{
				groupNameGreet.alpha = syncer.SceneProgress;
				demoNameGreet.alpha = syncer.SceneProgress;
			}

			public void Draw(Renderer renderer)
			{
				renderer.RenderObject(mountains);

				renderer.SetActiveShader(groupNameGreet.textMesh.ShaderProgram);
				groupNameGreet.textMesh.ShaderProgram.SetFloatUniform(ShaderUniformName.Alpha, groupNameGreet.alpha);
				renderer.RenderObject(groupNameGreet.textMesh);
				demoNameGreet.textMesh.ShaderProgram.SetFloatUniform(ShaderUniformName.Alpha, demoNameGreet.alpha);
				renderer.RenderObject(demoNameGreet.textMesh);

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
			Vector2 seaUVSpeed1;
			Vector2 seaUVSpeed2;

			public void Load(AssetManager assetManager, SyncSystem syncSystem)
			{
				TunableManager tm = TunableManager.GetSingleton();

				seaShader = assetManager.GetShaderProgram("heightMapTerrain");
				texOffset1Location = seaShader.GetCustomUniformLocation("ucUVoffset1");
				texOffset2Location = seaShader.GetCustomUniformLocation("ucUVoffset2");

				Vector2 seaSize = tm.GetVec2("sea.size");
				float seaTrianglesDensity = tm.GetFloat("sea.detail_level");
				Vector2 seaUVRepeat = tm.GetVec2("sea.UV_repeat");
				seaUVSpeed1 = tm.GetVec2("sea.UV_speed_1");
				seaUVSpeed2 = tm.GetVec2("sea.UV_speed_2");
				seaMesh = assetManager.CreateMesh("sea"
				, MeshDataGenerator.CreateTerrain(seaSize.X, seaSize.Y, seaTrianglesDensity, true, true
				, seaUVRepeat.X, seaUVRepeat.Y)
				, tm.GetString("sea.material")
				, seaShader
				, tm.GetVec3("sea.position"));

				offset1 = new Vector2(0, 0);
				offset2 = new Vector2(0, 0);
			}

			public void Sync(SyncSystem syncer)
			{
				offset1 = seaUVSpeed1 * syncer.SceneProgress;
				offset2 = seaUVSpeed2 * syncer.SceneProgress;
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

		/// <summary>
		/// //////////////////////////////// BUNNY DEMO 
		/// </summary>

		public BunnyDemo()
		{
			settings = DemoSettings.GetDefaults();
		}

		public void SetAudioSystem(IAudioSystem audioSystemParam)
		{
			audioSystem = audioSystemParam;
		}

		public void Load(AssetManager assetManager, SyncSystem syncSystem)
		{
			TunableManager tm = TunableManager.GetSingleton();
			if (settings.AudioEnabled)
			{
				string audioFileName = tm.GetString("audio.filename");
				music = audioSystem.LoadAudioFile(audioFileName);
			}
			syncSystem.SetAudioProperties(tm.GetInt("audio.bpm"), tm.GetFloat("audio.length_seconds")
					, tm.GetInt("audio.rows_per_beat"));
			syncSystem.SetManualSceneAdvanceRate(tm.GetFloat("sync.manual_scene_advance_speed"));

			Logger.LogInfo("Loading Bunny Demo");

			CameraComponent camera = Renderer.GetSingleton().GetCamera();
			camera.FOV = MathHelper.DegreesToRadians(tm.GetFloat("camera.fov"));
			camera.Speed = tm.GetFloat("camera.speed");
			camera.SpeedStep = tm.GetFloat("camera.speed_step");

			// Camera frames
			List<PosAndDir> frames = new List<PosAndDir>();
			int frameAmount = tm.GetInt("camera_frames.amount");
			for (int frameI = 0; frameI < frameAmount; frameI++)
			{
				Vector3 pos = tm.GetVec3("camera_frames.frame_" + frameI + "_pos");
				Vector3 dir = tm.GetVec3("camera_frames.frame_" + frameI + "_dir");
				frames.Add(new PosAndDir(pos, dir));
			}
			Renderer.GetSingleton().SetCameraFrames(frames);

			mountains = new MountainScene();
			mountains.Load(assetManager, syncSystem);

			monolithSpace = new MonolithSpaceScene();
			monolithSpace.Load(assetManager, syncSystem);

			seaScene = new SeaScene();
			seaScene.Load(assetManager, syncSystem);

			bunnyScene = new BunnyScene();
			bunnyScene.Load(assetManager, syncSystem);

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
				, tm.GetVec3("fade.position"));

			fadeoutQuad.Transform.Scale = tm.GetFloat("fade.scale");

			ShaderProgram skyboxProgram = assetManager.GetShaderProgram("sky");
			starSphere = assetManager.CreateMesh("stars"
										 , MeshDataGenerator.CreateStarSphere(90.0f, 500, 0.05f, 0.8f)
										 , "star_palette"
										 , skyboxProgram
										 , new Vector3(0, 0, 0));

			starSphere.Transform.SetRotationAxis(tm.GetVec3("mountain_scene.star_rotation_axis"));
			starRotSpeed = tm.GetFloat("mountain_scene.star_rotation_speed");

			moon = Light.CreateDirectionalLight(tm.GetVec3("moon.color")
			, tm.GetFloat("moon.ambient"), tm.GetFloat("moon.intensity")
			, tm.GetVec3("moon.direction"));
			Logger.LogInfo("Created directional moon light " + moon.GetInfoString());

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
			UpdateFadeout(fadeoutAlpha.GetValue(syncer.GetSyncRow()));
			UpdateStarRotation(syncer.SceneProgress);

			switch(syncer.Scene)
			{
				case 0:
				mountains.Sync(syncer);
					break;
				case 1:
				monolithSpace.Sync(syncer);
					break;
				case 2:
				seaScene.Sync(syncer);
					break;
				case 3:
				bunnyScene.Sync(syncer);
					break;
			}
		}

		private void UpdateFadeout(float progress)
		{
			fadeAlpha = progress;
		}

		private void UpdateStarRotation(float progress)
		{
			// Rotate star sphere world matrix
			starRot = starRotSpeed * progress;
			starSphere.Transform.SetRotation(starRot);
		}

		public void Draw(Renderer renderer)
		{
			renderer.ActivateLight(moon, 0);
			
			switch(SyncSystem.GetSingleton().Scene)
			{
				case 0:
				mountains.Draw(renderer);
					break;
				case 1:
					monolithSpace.Draw(renderer);
					break;
				case 2:
				seaScene.Draw(renderer);
					break;
				case 3:
				bunnyScene.Draw(renderer);
					break;
			}

			renderer.RenderSky(starSphere);

			if (fadeAlpha > 0.0f)
			{
				DrawFadeout(renderer, fadeAlpha);
			}
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
			guiShader.SetFloatUniform(ShaderUniformName.Alpha, fadeAlpha);
			renderer.RenderGui(fadeoutQuad);
			renderer.GetCamera().EnablePerspective();
		}

		public DemoSettings GetDemoSettings()
		{
			TunableManager tm = TunableManager.GetSingleton();
			settings.AudioEnabled = tm.GetBool("demosettings.audio_enabled");
			settings.SyncEnabled = tm.GetBool("demosettings.sync_enabled");
			settings.Resolution = tm.GetVec2("demosettings.resolution");
			settings.UpdatesPerSecond = tm.GetInt("demosettings.updates_per_second");
			settings.WindowTitle = tm.GetString("demosettings.window_title");
			settings.SyncFilePrefix = tm.GetString("sync.track_file_prefix");
			return settings;
		}

		public void Restart()
		{
			if (GetDemoSettings().AudioEnabled)
			{
				audioSystem.RestartAudio(music);
			}
		}
	}
}