using System.Collections.Generic;

using RocketNet;
using OpenTK;

namespace MuffinSpace
{
	public class GreetPage
	{
		public	float progress;
		public List<Greeting> greets;
		public GreetPage()
		{
			greets = new List<Greeting>();
		}
	}
	public class Greeting
	{
		public DrawableMesh textMesh;
		public float alpha;
	}

	public class Telescope
	{
		public DrawableMesh tower;
		public DrawableMesh antenna;
	}

	public class BunnyDemo : IDemo
	{
		DemoSettings settings;

		// Scenes

		int mountainsNumber;
		int moonNumber;
		int bunnyNumber;
		int seaNumber;
		int crystalsNumber;

		// Audio 

		IAudioSystem audioSystem = null;
		Audio music;

		// Fadeout

		ShaderProgram guiShader;
		int fadeAlphaLocation = 0;
		float fadeAlpha = 0.0f;
		DrawableMesh fadeoutQuad;
		SyncTrack fadeoutAlpha;

		// Sky box

		DrawableMesh starSkyBox;
		Material starSkyboxMaterial;

		SyncTrack skyRotation;

		Light moon;
		Light starAmbient;

		// SEA

		DrawableMesh seaMesh;
		ShaderProgram seaShader;
		Vector2 offset1;
		Vector2 offset2;
		Vector2 seaUVSpeed1;
		Vector2 seaUVSpeed2;
		Matrix4 seaNormalMatrix;
		int texOffset1Location;
		int texOffset2Location;
		int normalMatrixLocation;

		// Meshes 

		DrawableMesh bunnyMesh;
		DrawableMesh mountains;
		DrawableMesh moonMesh;

		SyncTrack bunnyRotation;

		DrawableMesh monolith;
		SyncTrack monolithRotation;
		SyncTrack monolithElevation;

		Telescope telescope;
		SyncTrack telescopeRotation;
		SyncTrack telescopeElevation;

		// Greetings 

		Vector3 greetOffset;
		Vector3 greetSpacing;
		float greetScale;
		List<GreetPage> group_greetings;

		// Credits
		List<GreetPage> credits;



		// Name and title

		List<GreetPage> title;
		Greeting groupNameGreet;
		Greeting demoNameGreet;

		SyncTrack textAlpha;

		public BunnyDemo()
		{
			settings = DemoSettings.GetDefaults();
		}

		public void Load(AssetManager assetManager, SyncSystem syncSystem)
		{
			TunableManager tm = TunableManager.GetSingleton();

			mountainsNumber = tm.GetInt("scene_number.mountains");
			moonNumber = tm.GetInt("scene_number.moon");
			bunnyNumber = tm.GetInt("scene_number.bunny");
			seaNumber = tm.GetInt("scene_number.sea");
			crystalsNumber = tm.GetInt("scene_number.crystals");

			// Audio
				string audioFileName = tm.GetString("audio.filename");
				music = audioSystem.LoadAudioFile(audioFileName);

				syncSystem.SetAudioProperties(tm.GetInt("audio.bpm"), tm.GetFloat("audio.length_seconds")
						, tm.GetInt("audio.rows_per_beat"));
				syncSystem.SetManualSceneAdvanceRate(tm.GetFloat("sync.manual_scene_advance_speed"));

			// Camera
				CameraComponent camera = Renderer.GetSingleton().GetCamera();
				camera.FOV = tm.GetFloat("camera.fov");
				camera.Speed = tm.GetFloat("camera.speed");
				camera.SpeedStep = tm.GetFloat("camera.speed_step");
				camera.Near = tm.GetFloat("camera.near_plane");
				camera.Far = tm.GetFloat("camera.far_plane");
				camera.CreateMatrices();

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

			// Shaders
				ShaderProgram bunnyShader = assetManager.GetShaderProgram(tm.GetString("bunny.shader"));
				ShaderProgram texShader = assetManager.GetShaderProgram("texturedobjmesh");
				ShaderProgram gridShader = assetManager.GetShaderProgram("gridmesh");
				ShaderProgram objShader = assetManager.GetShaderProgram("litobjmesh");
				ShaderProgram skyboxProgram = assetManager.GetShaderProgram("skybox");
				ShaderProgram starProgram = assetManager.GetShaderProgram("sky");

				guiShader = assetManager.GetShaderProgram("gui");


			// Fade 
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


			// Skybox
				starSkyBox = assetManager.CreateMesh("stars"
											 , MeshDataGenerator.CreateSkybox()
											 , null
											 , skyboxProgram
											 , new Vector3(0, 0, 0));

				starSkyboxMaterial = assetManager.GetMaterial("skybox");

				starSkyBox.Transform.SetRotationAxis(tm.GetVec3("mountain_scene.star_rotation_axis"));
				skyRotation = syncSystem.GetTrack("Sky_R");

			// Lights
				moon = Light.CreateDirectionalLight(tm.GetVec3("moon_ambient.color")
				, tm.GetFloat("moon_ambient.ambient"), tm.GetFloat("moon_ambient.intensity")
				, tm.GetVec3("moon_ambient.direction"));

				starAmbient = Light.CreateDirectionalLight(tm.GetVec3("star_ambient.color")
				, tm.GetFloat("star_ambient.ambient"), tm.GetFloat("star_ambient.intensity")
				, tm.GetVec3("star_ambient.direction"));

			// Models
				bunnyMesh = assetManager.GetMesh("bunny"
				, tm.GetString("bunny.model")
				, tm.GetString("bunny.material")
				, bunnyShader
				, tm.GetVec3("bunny.position"));

			bunnyMesh.Transform.Scale = tm.GetFloat("bunny.scale");
			bunnyMesh.Transform.SetRotationAxis(new Vector3(0, 1, 0));
			bunnyRotation = syncSystem.GetTrack("bunny_R");

				monolith = assetManager.CreateMesh("monolith"
				, MeshDataGenerator.CreateCubeMesh(tm.GetVec3("monolith.size")
				, true, true)
				, tm.GetString("monolith.material")
				, objShader
				, tm.GetVec3("monolith.position"));

			monolith.Transform.SetRotationAxis(new Vector3(0, 1, 0));
			monolith.Transform.Scale = tm.GetFloat("monolith.scale");
			monolithRotation = syncSystem.GetTrack("mono_R");
			monolithElevation = syncSystem.GetTrack("mono_Y");

			telescope = new Telescope();
			telescope.tower = assetManager.CreateMesh("tele_tower"
			, MeshDataGenerator.CreateCubeMesh(new Vector3(1,1,1), true, true)
			, tm.GetString("telescope.material")
			, objShader
			, tm.GetVec3("telescope.tower_position"));
			telescope.tower.Transform.Scale = tm.GetFloat("telescope.scale");
			telescope.tower.Transform.SetRotationAxis(new Vector3(0, 1, 0));
			
			telescope.antenna = assetManager.CreateMesh("tele_antenna"
			, MeshDataGenerator.CreateCubeMesh(new Vector3(0.1f, 1.5f, 1.5f), true, true)
			, tm.GetString("telescope.material")
			, objShader
			, tm.GetVec3("telescope.antenna_position"));
			telescope.antenna.Transform.Scale = tm.GetFloat("telescope.scale");
			telescope.antenna.Transform.Parent = telescope.tower.Transform;
			telescope.antenna.Transform.SetRotationAxis(new Vector3(0, 0, 1));
			/*
			telescope.tower = assetManager.GetMesh("tele_tower"
			, tm.GetString("telescope.tower_model")
			, tm.GetString("telescope.material")
			, objShader
			, tm.GetVec3("telescope_tower.position"));

			telescope.tower.Transform.Scale = tm.GetFloat("telescope_tower.scale");
			
			telescope.antenna = assetManager.GetMesh("tele_antenna"
			, tm.GetString("telescope.antenna_model")
			, tm.GetString("telescope.material")
			, objShader
			, tm.GetVec3("telescope_antenna.position"));
			telescope.antenna.Transform.Scale = tm.GetFloat("telescope_antenna.scale");
			*/

			telescopeElevation = syncSystem.GetTrack("Tele_Elev");
			telescopeRotation = syncSystem.GetTrack("Tele_Rot");

			// Sea
				seaShader = assetManager.GetShaderProgram("heightMapTerrain");
				texOffset1Location = seaShader.GetCustomUniformLocation("ucUVoffset1");
				texOffset2Location = seaShader.GetCustomUniformLocation("ucUVoffset2");
				normalMatrixLocation = seaShader.GetCustomUniformLocation("ucNormalRotationMatrix");
				seaNormalMatrix = Matrix4.CreateFromAxisAngle(new Vector3(1, 0, 0), MathHelper.DegreesToRadians(-90));

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

			// Mountains
			mountains = assetManager.CreateMesh("mountains"
				, MeshDataGenerator.CreateMountains(
					tm.GetFloat("mountain_scene.mountain_size")
					, true, tm.GetInt("mountain_scene.mountain_iterations")
					, tm.GetFloat("mountain_scene.mountain_height_variation")
					, tm.GetInt("mountain_scene.mountain_random_seed"))
				, tm.GetString("mountain_scene.mountain_material")
				, objShader
				, tm.GetVec3("mountain_scene.mountain_position"));


			moonMesh = assetManager.CreateMesh("moon_mesh"
				, MeshDataGenerator.CreateQuadMesh(false, true)
				, tm.GetString("moon.material")
				, objShader
				, new Vector3(0,0,0) + tm.GetVec3("moon.distance"));
			moonMesh.Transform.Scale = tm.GetFloat("moon.scale");


			Logger.LogInfo("Creating greetings");
			// Title, Greets and credits
				TextGenerator textgen = TextGenerator.GetSingleton();
				string greet_font = tm.GetString("text.font");
				PixelFont greetFont = textgen.GetFont(greet_font);
				string greet_material = tm.GetString("text.material");
				ShaderProgram greetShader = assetManager.GetShaderProgram(tm.GetString("text.shader"));

			// Title
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

			// Greets
				greetOffset = tm.GetVec3("text.offset");
				greetSpacing = tm.GetVec3("text.spacing");
				greetScale = tm.GetFloat("text.scale");

				group_greetings = new List<GreetPage>();
			CreateGreets(tm, assetManager, "monolith_greets", greetFont, greet_material, greetShader, monolith.Transform, ref group_greetings);
			// Credits
				credits = new List<GreetPage>();
			CreateGreets(tm, assetManager, "monolith_credits", greetFont, greet_material, greetShader, monolith.Transform, ref credits);

			textAlpha = syncSystem.GetTrack("text_A");

			Logger.LogInfo("Creating title and name");
			title = new List<GreetPage>();
			GreetPage titlePage = new GreetPage();
			titlePage.greets.Add(groupNameGreet);
			titlePage.greets.Add(demoNameGreet);

			title.Add(titlePage);

			Logger.LogPhase("Bunny demo is loaded");
			syncSystem.PrintEditorRowAmount();
		}

		public void CreateGreets(TunableManager tm, AssetManager assetManager
		, string greetName
		, PixelFont greetFont
		, string greet_material
		, ShaderProgram greetShader
		, TransformComponent parentTransform
		, ref List<GreetPage> greetsOut)
		{
				int gp = tm.GetInt(greetName + ".greet_pages");
				for (int pi = 0; pi < gp; pi++)
				{
					GreetPage page = new GreetPage();
					page.greets = new List<Greeting>();
					string pageName = greetName + "_page_" + (pi + 1);
					int greet_amount = tm.GetInt(pageName + "." + "greet_amount");
					for (int gi = 0; gi < greet_amount; gi++)
					{
						Greeting greet = new Greeting();

						greet.textMesh = assetManager.CreateMesh("greet" + gi
						, MeshDataGenerator.CreateTextMesh(tm.GetString(pageName + "." + "greet_" + (gi + 1))
						, greetFont)
						, greet_material
						, greetShader
						, new Vector3(0, 0, 0));

						greet.alpha = 1.0f;
						page.greets.Add(greet);
					}
					greetsOut.Add(page);
				}

				for (int pi = 0; pi < greetsOut.Count; pi++)
				{
					GreetPage p = greetsOut[pi];
					for (int gi = 0; gi < p.greets.Count; gi++)
					{
						Greeting g = p.greets[gi];
						g.textMesh.Transform.Parent = parentTransform;
						g.textMesh.Transform.Position = greetOffset + (greetSpacing * gi);
						g.textMesh.Transform.Scale = greetScale;
					}
				}

		}


		// SYNC FUNCTIONS

		public void SyncGreets(SyncSystem syncer, List<GreetPage> greetPages)
		{
			// TODO: Sync alpha divided between greets
			float alphaPerPage = 1.0f / greetPages.Count;
			float wholeAlpha = syncer.GetTrackValue(textAlpha);
			for (int pi = 0; pi < greetPages.Count; pi++)
			{
				GreetPage p = greetPages[pi];
				p.progress = System.Math.Min(1.0f, (wholeAlpha - (alphaPerPage * pi)) / alphaPerPage);
				float alphaPerGreet = 1.0f / p.greets.Count;
				for (int gi = 0; gi < p.greets.Count; gi++)
				{
					Greeting g = p.greets[gi];
					g.alpha = System.Math.Min(1.0f, (p.progress - (alphaPerGreet * gi)) / alphaPerGreet);
				}
			}
		}

		void SyncBunny(SyncSystem syncer)
		{
			bunnyMesh.Transform.SetRotation(syncer.GetTrackValue(bunnyRotation));
		}

		void SyncMoon(SyncSystem syncer)
		{
			// Moon is a billboard
			moonMesh.Transform.SetRotationMatrix(Renderer.GetSingleton().GetCamera().GetRotationMatrix());
		}

		public void DrawGreets(Renderer renderer, List<GreetPage> greetPages)
		{
			// Render greeting pages
			foreach(GreetPage p in greetPages)
			{
				for (int gi = 0; gi < p.greets.Count; gi++)
				{
					Greeting g = p.greets[gi];
					DrawSingleGreet(renderer, g);
				}
			}
		}

		public void DrawSingleGreet(Renderer renderer, Greeting g)
		{
			renderer.SetActiveShader(g.textMesh.ShaderProgram);
			g.textMesh.ShaderProgram.SetFloatUniform(ShaderUniformName.Alpha, g.alpha);
			renderer.RenderObject(g.textMesh);
		}

		public void SyncSea(SyncSystem syncer)
		{
			offset1 = seaUVSpeed1 * syncer.SceneProgress;
			offset2 = seaUVSpeed2 * syncer.SceneProgress;
		}

		public void SyncMonolith(SyncSystem syncer)
		{
			float rotations = syncer.GetTrackValue(monolithRotation);
			monolith.Transform.SetRotation(rotations * MathHelper.PiOver2);
			float x = monolith.Transform.Position.X;
			float z = monolith.Transform.Position.Z;
			monolith.Transform.Position = new Vector3(x, syncer.GetTrackValue(monolithElevation), z);
		}

		public void DrawSea(Renderer renderer)
		{
			renderer.SetActiveShader(seaMesh.ShaderProgram);
			seaShader.SetVec2Uniform(texOffset1Location, offset1);
			seaShader.SetVec2Uniform(texOffset2Location, offset2);
			seaShader.SetMatrix4Uniform(normalMatrixLocation, ref seaNormalMatrix);
			renderer.RenderObject(seaMesh);
		}

		public void SetAudioSystem(IAudioSystem audioSystemParam)
		{
			audioSystem = audioSystemParam;
		}

		public void Start()
		{
			if (GetDemoSettings().AudioEnabled)
			{
				audioSystem.PlayAudio(music);
			}
		}


		private void UpdateFadeout(SyncSystem syncer)
		{
			fadeAlpha = syncer.GetTrackValue(fadeoutAlpha);
		}

		private void SyncSkybox(SyncSystem syncer)
		{
			// Rotate star sphere world matrix
			float rot = syncer.GetTrackValue(skyRotation) * MathHelper.PiOver2;
			starSkyBox.Transform.SetRotation(rot);
		}

		private void SyncTelescope(Telescope scope, SyncSystem syncer)
		{
			float rotRad = MathHelper.DegreesToRadians(syncer.GetTrackValue(telescopeRotation));
			scope.tower.Transform.SetRotation(rotRad);

			float elevRad = MathHelper.DegreesToRadians(syncer.GetTrackValue(telescopeElevation));
			scope.antenna.Transform.SetRotation(elevRad);
		}

		public void Sync(SyncSystem syncer)
		{
			UpdateFadeout(syncer);

			int s = syncer.Scene;
			if (s == mountainsNumber)
			{
				SyncTelescope(telescope, syncer);
				SyncSkybox(syncer);
				SyncGreets(syncer, title);
			}
			if (s == moonNumber)
			{
				SyncMoon(syncer);
				SyncMonolith(syncer);
				SyncGreets(syncer, group_greetings);
			}
			if (s == bunnyNumber)
			{
				// Sync lights
				SyncBunny(syncer);
			}
			if (s == seaNumber)
			{
				SyncMoon(syncer);
				SyncSea(syncer);
				SyncMonolith(syncer);
				SyncGreets(syncer, credits);
				SyncSkybox(syncer);
			}
			if (s == crystalsNumber)
			{
				// Crystals
			}
		}

		public void Draw(Renderer renderer)
		{
			renderer.SetActiveSkybox(starSkyboxMaterial);
			renderer.SetSkyboxRotation(starSkyBox.Transform.GetRotationMatrix());
			renderer.RenderSky(starSkyBox);

			int s = SyncSystem.GetSingleton().Scene;
			if (s == mountainsNumber)
			{
				renderer.ActivateLight(starAmbient, 0);
				renderer.RenderMesh(mountains);
				renderer.RenderMesh(telescope.tower);
				renderer.RenderMesh(telescope.antenna);

				// Transparent things last!
				DrawGreets(renderer, title);
			}
			if (s == moonNumber)
			{
				renderer.ActivateLight(moon, 0);
				// renderer.RenderMesh(moonMesh);
				renderer.RenderMesh(monolith);

				DrawGreets(renderer, group_greetings);
			}
			if (s == bunnyNumber)
			{
				// bunny point lights???
				renderer.ActivateLight(starAmbient, 0);
				renderer.RenderMesh(bunnyMesh);
			}
			if (s == seaNumber)
			{
				// Monolith dancing lights
				renderer.ActivateLight(moon, 0);
				// renderer.RenderMesh(moonMesh);
				DrawSea(renderer);
				renderer.RenderMesh(monolith);
				
				DrawGreets(renderer, credits);
			}
			if (s == crystalsNumber)
			{

			}


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
			string audio_engine = tm.GetString("demosettings.audio_engine");
			if (audio_engine == "system")
			{
				settings.AudioEngineSetting = DemoSettings.AudioEngine.System;
			}

			settings.SyncEnabled = tm.GetBool("demosettings.sync_enabled");

			settings.Resolution = tm.GetVec2("demosettings.resolution");
			settings.Fullscreen = tm.GetBool("demosettings.fullscreen");
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

		public void Stop()
		{
			if (GetDemoSettings().AudioEnabled)
			{
				audioSystem.StopAudio(music);
			}
		}
	}
}