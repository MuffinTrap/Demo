using System.Collections.Generic;

using RocketNet;
using OpenTK;
using System;

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
	public class Greeting : IShaderDataOwner
	{
		public DrawableMesh textMesh;
		public float alpha;
		public Vector3 color;

		static List<ShaderUniformName> greetingUniforms = new List<ShaderUniformName> { ShaderUniformName.Alpha, ShaderUniformName.DiffuseColor };

		public bool SetUniform(ShaderProgram program, int location, ShaderUniformName name)
		{
			switch (name)
			{
				case ShaderUniformName.Alpha:
					program.SetFloatUniform(location, alpha);
					break;
				case ShaderUniformName.DiffuseColor:
					program.SetVec3Uniform(location, color);
					break;
				default:
					return false;
			}
			return true;
		}

		public List<ShaderUniformName> GetUniforms()
		{
			return greetingUniforms;
		}
	}

	public class Telescope
	{
		public DrawableMesh tower;
		public DrawableMesh antenna;
	}

	public class LightMesh : IShaderDataOwner
	{
		public Light light;
		public float sphereRadius;
		public float activeRadius;
		public float fullRange;
		public DrawableMesh mesh;

		static List<ShaderUniformName> lightUniforms = new List<ShaderUniformName> { ShaderUniformName.DiffuseColor, ShaderUniformName.Alpha };

		public bool SetUniform(ShaderProgram program, int location, ShaderUniformName name)
		{
			switch (name)
			{
				case ShaderUniformName.DiffuseColor:
					program.SetVec3Uniform(location, light.color);
					break;
				case ShaderUniformName.Alpha:
					program.SetFloatUniform(location, 1.0f);
					break;
				default:
					return false;
			}
			return true;
		}
		public List<ShaderUniformName> GetUniforms()
		{
			return lightUniforms;
		}
	}

	public class Crystal
	{
		public DrawableMesh mesh;
		public float speed;
	}

	public class BunnyDemo : IDemo
	{
		DemoSettings settings;

		// Scenes

		int mountainsNumber;
		int moonNumber;
		int bunnyNumber;
		int warpNumber;
		int seaNumber;
		int crystalsNumber;

		// Audio 

		IAudioSystem audioSystem = null;
		Audio music;

		SyncTrack cameraFOV;
		SyncTrack warpFOV;

		// Fadeout

		ShaderProgram guiShader;
		int fadeAlphaLocation = 0;
		float fadeAlpha = 0.0f;
		DrawableMesh fadeoutQuad;
		SyncTrack fadeoutAlpha;

		// Sky box

		DrawableMesh starSkyBox;
		Material starSkyboxMaterial;

		DrawableMesh planetSkyBox;
		Material planetSkyboxMaterial;

		SyncTrack skyRotation;

		DrawableMesh mountains;
		Light mountainLight;
		// SEA

		DrawableMesh seaMesh;
		ShaderProgram seaShader;

		DrawableMesh planetMountains;
		Light planetLight;

		// Meshes 

		// Bunny
		DrawableMesh bunnyMesh;
		SyncTrack bunnyRotation;
		List<LightMesh> bunnyLights;
		List<int> bunnyLightsSizesLocations;
		SyncTrack bunnyLightProgress;

		// Crystals
		List<Crystal> crystalMeshes;


		DrawableMesh monolith;
		DrawableMesh monolith_normals;
		SyncTrack monolithRotation;
		SyncTrack monolithElevation;

		Telescope telescope;
		SyncTrack telescopeRotation;
		SyncTrack telescopeElevation;
		Vector3 mainTelescopePosition;
		List<Vector3> telescopeArrayPositions;

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
			warpNumber = tm.GetInt("scene_number.warp");
			seaNumber = tm.GetInt("scene_number.sea");
			crystalsNumber = tm.GetInt("scene_number.crystals");

			// Audio
			Logger.LogInfo("Loading demo audio");
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
			Logger.LogInfo("Loading demo frames");
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
			Logger.LogInfo("Loading demo shaders");
			ShaderProgram diamondShader = assetManager.GetShaderProgram("diamond");
			ShaderProgram texShader = assetManager.GetShaderProgram("texturedobjmesh");
			ShaderProgram gridShader = assetManager.GetShaderProgram("gridmesh");
			ShaderProgram objShader = assetManager.GetShaderProgram("litobjmesh");
			ShaderProgram skyboxProgram = assetManager.GetShaderProgram("skybox");
			ShaderProgram starProgram = assetManager.GetShaderProgram("sky");
			guiShader = assetManager.GetShaderProgram("gui");

			cameraFOV = syncSystem.GetTrack("FOV");
			warpFOV = syncSystem.GetTrack("W_FOV");


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

			mountainLight = Light.CreateDirectionalLight(tm.GetVec3("mountain_light.color")
			, tm.GetFloat("mountain_light.ambient"), tm.GetFloat("mountain_light.intensity")
			, tm.GetVec3("mountain_light.direction"));

			// Models
			Logger.LogInfo("Loading demo models");
			Logger.LogInfo(" - Bunny");
			bunnyMesh = assetManager.GetMesh("bunny"
			, tm.GetString("bunny.model")
			, tm.GetString("bunny.material")
			, assetManager.GetShaderProgram(tm.GetString("bunny.shader"))
			, tm.GetVec3("bunny.position"));

			bunnyMesh.Transform.Scale = tm.GetFloat("bunny.scale");
			bunnyMesh.Transform.SetRotationAxis(new Vector3(0, 1, 0));
			bunnyRotation = syncSystem.GetTrack("bunny_R");

			bunnyLights = new List<LightMesh>();
			int bunnyLightAmount = tm.GetInt("bunny_lights.amount");
			LoadLightMeshList(tm, assetManager, bunnyMesh.Transform, "bunny_lights", bunnyLightAmount, texShader, ref bunnyLights);

			bunnyLightsSizesLocations = new List<int>();
			for (int i = 0; i < bunnyLights.Count; i++)
			{
				int location = bunnyMesh.ShaderProgram.GetCustomUniformLocation("ucLightSizes[" + i + "]");
				Logger.LogInfo("Location of bunny light size " + i + " is " + location);
				bunnyLightsSizesLocations.Add(location);
			}

			bunnyLightProgress = syncSystem.GetTrack("bunny_L");

			Logger.LogInfo(" - Monolith");
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

			// DEBUG
			monolith_normals = assetManager.CreateMesh("mono_N"
			, MeshDataGenerator.CreateNormalDebug(monolith.Data.positions, monolith.Data.normals)
			, null
			, gridShader
			, monolith.Transform.GetWorldPosition());

			monolith_normals.Transform.Parent = monolith.Transform;

			Logger.LogInfo(" - Telescope");
			telescope = new Telescope();
			telescope.tower = assetManager.CreateMesh("tele_tower"
			, MeshDataGenerator.CreateCubeMesh(new Vector3(1,1,1), true, true)
			, tm.GetString("telescope.material")
			, objShader
			, tm.GetVec3("telescope.tower_position"));
			telescope.tower.Transform.SetRotationAxis(new Vector3(0, 1, 0));
			
			telescope.antenna = assetManager.CreateMesh("tele_antenna"
			, MeshDataGenerator.CreateCubeMesh(new Vector3(0.1f, 1.5f, 1.5f), true, true)
			, tm.GetString("telescope.material")
			, objShader
			, tm.GetVec3("telescope.antenna_position"));
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

			mainTelescopePosition = tm.GetVec3("telescope.tower_position");

			// Telescope array
			int teleAmount = tm.GetInt("telescope_array.amount");
			Vector3 teleArrayCorner = tm.GetVec3("telescope_array.position");
			Vector3 teleArrayDir1 = tm.GetVec3("telescope_array.dir");
			Vector3 teleArrayDir2 = tm.GetVec3("telescope_array.dir2");
			float spacing = tm.GetFloat("telescope_array.spacing");
			telescopeArrayPositions = new List<Vector3>();
			for (int t = 0; t < teleAmount; t++)
			{
				telescopeArrayPositions.Add(teleArrayCorner + teleArrayDir1 * spacing * (t + 1));
				telescopeArrayPositions.Add(teleArrayCorner + teleArrayDir2 * spacing * (t + 1));
			}

			// Sea
			Logger.LogInfo(" - Sea");
			seaShader = assetManager.GetShaderProgram("heightMapTerrain");

			Vector2 seaSize = tm.GetVec2("sea.size");
			float seaTrianglesDensity = tm.GetFloat("sea.detail_level");
			Vector2 seaUVRepeat = tm.GetVec2("sea.UV_repeat");
			seaMesh = assetManager.CreateMesh("sea"
			, MeshDataGenerator.CreateTerrain(seaSize.X, seaSize.Y, seaTrianglesDensity, true, true
			, seaUVRepeat.X, seaUVRepeat.Y)
			, tm.GetString("sea.material")
			, objShader
			, tm.GetVec3("sea.position"));

			// Planet mountains
			Logger.LogInfo(" - Planet Mountains");
			planetMountains = assetManager.CreateMesh("planet_mountains"
				, MeshDataGenerator.CreateMountains(
					tm.GetFloat("planet_mountain.size")
					, true, tm.GetInt("planet_mountain.iterations")
					, tm.GetFloat("planet_mountain.height_variation")
					, tm.GetFloat("planet_mountain.flat_start")
					, tm.GetFloat("planet_mountain.flat_end")
					, tm.GetInt("planet_mountain.random_seed"))
				, tm.GetString("planet_mountain.material")
				, objShader
				, tm.GetVec3("planet_mountain.position"));

			planetLight = Light.CreateDirectionalLight(tm.GetVec3("planet_light.color")
			, tm.GetFloat("planet_light.ambient"), tm.GetFloat("planet_light.intensity")
			, tm.GetVec3("planet_light.direction"));

			planetSkyBox = assetManager.CreateMesh("planet_skybox"
										 , MeshDataGenerator.CreateSkybox()
										 , null
										 , skyboxProgram
										 , new Vector3(0, 0, 0));

			planetSkyboxMaterial = assetManager.GetMaterial("planet_skybox");


			// Mountains
			Logger.LogInfo(" - Mountains");
			mountains = assetManager.CreateMesh("mountains"
				, MeshDataGenerator.CreateMountains(
					tm.GetFloat("mountain.size")
					, true, tm.GetInt("mountain.iterations")
					, tm.GetFloat("mountain.height_variation")
					, tm.GetFloat("mountain.flat_start")
					, tm.GetFloat("mountain.flat_end")
					, tm.GetInt("mountain.random_seed"))
				, tm.GetString("mountain.material")
				, objShader
				, tm.GetVec3("mountain.position"));

			// Crystals
			Logger.LogInfo(" - Crystals");

			crystalMeshes = new List<Crystal>();
			int crystalAmount = tm.GetInt("crystals.amount");
			Random randomizer = new Random(0);
			float areaSize = tm.GetFloat("crystals.area_size");
			float areaTop = tm.GetFloat("crystals.area_top");
			Vector3 areaPos = tm.GetVec3("crystals.area_pos");
			float maxSpeed = tm.GetFloat("crystals.max_speed");
			float sizeMin = tm.GetFloat("crystals.min_size");
			float sizeMax = tm.GetFloat("crystals.max_size");
			for (int s = 0; s < crystalAmount; s++)
			{

				Crystal c = new Crystal();

				Vector3 position = new Vector3((float)randomizer.NextDouble() - 0.5f
				, (float)randomizer.NextDouble()
				, (float)randomizer.NextDouble() - 0.5f);

				position = position * new Vector3(areaSize, areaSize, areaSize);
				position += areaPos;

				c.mesh = assetManager.CreateMesh("crystal_" + s
				, MeshDataGenerator.CreateStar(new Vector3(1.0f, 0, 0), new Vector3(0, 1, 0)
				, 1.0f, 2.0f, 0.5f, 0.8f, true)
				, tm.GetString("crystals.material")
				, assetManager.GetShaderProgram(tm.GetString("crystals.shader"))
				, position);

				c.mesh.Transform.Scale = sizeMin + (float)randomizer.NextDouble() * (sizeMax - sizeMin);
				c.mesh.Transform.SetRotationAxis(position.Normalized());
				c.speed = (float)randomizer.NextDouble() * maxSpeed;
				crystalMeshes.Add(c);
			}

			Logger.LogInfo("Creating greetings");
			// Title, Greets and credits
				TextGenerator textgen = TextGenerator.GetSingleton();
				string greet_font = tm.GetString("text.font");
				PixelFont greetFont = textgen.GetFont(greet_font);
				string greet_material = tm.GetString("text.material");
				ShaderProgram greetShader = assetManager.GetShaderProgram(tm.GetString("text.shader"));
			float textStep = tm.GetFloat("text.step");

			// Title
				DrawableMesh groupName = assetManager.CreateMesh("groupText"
				, MeshDataGenerator.CreateTextMesh("FCCCF", greetFont, textStep)
				, greet_material
				, greetShader
				, tm.GetVec3("mountain_scene.group_name_position"));
			groupName.Transform.Scale = tm.GetFloat("mountain_scene.group_name_scale");

				groupNameGreet = new Greeting();
				groupNameGreet.textMesh = groupName;
				groupNameGreet.alpha = 0.0f;
				groupNameGreet.color = tm.GetVec3("mountain_text.color");
				

				DrawableMesh demoName = assetManager.CreateMesh("groupText"
				, MeshDataGenerator.CreateTextMesh("Lepus Minor", greetFont, textStep)
				, greet_material
				, greetShader
				, tm.GetVec3("mountain_scene.demo_name_position"));

				demoNameGreet = new Greeting();
				demoNameGreet.textMesh = demoName;
				demoNameGreet.alpha = 0.0f;
				demoNameGreet.color = tm.GetVec3("mountain_text.color");

			// Greets
				greetOffset = tm.GetVec3("text.offset");
				greetSpacing = tm.GetVec3("text.spacing");
				greetScale = tm.GetFloat("text.scale");

				group_greetings = new List<GreetPage>();
			Vector3 monoGreetColor = tm.GetVec3("monolith_greets.color");
			CreateGreets(tm, assetManager, "monolith_greets", greetFont, greet_material, monoGreetColor, greetShader, textStep, monolith.Transform, ref group_greetings);
			// Credits
				credits = new List<GreetPage>();
			Vector3 creditGreetColor = tm.GetVec3("monolith_credits.color");
			CreateGreets(tm, assetManager, "monolith_credits", greetFont, greet_material, creditGreetColor, greetShader, textStep, monolith.Transform, ref credits);

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

		void LoadLightMeshList(TunableManager tm, AssetManager assetManager, TransformComponent parentTransform, string lightArrayName, int amount, ShaderProgram shader, ref List<LightMesh> list)
		{
			for (int ml = 0; ml < amount; ml++)
			{
				LightMesh lm = new LightMesh();
				string lightId = lightArrayName + "." + (ml + 1);

				lm.sphereRadius = tm.GetFloat(lightId + "_size");
				Vector3 position = tm.GetVec3(lightId + "_pos") + parentTransform.GetWorldPosition();
				Logger.LogInfo("Light mesh list " + lightId + "_pos: " + Logger.PrintVec3(position));
				lm.mesh = assetManager.CreateMesh(lightId
					, MeshDataGenerator.CreateCubeMesh(new Vector3(lm.sphereRadius), false, false)
					, "default"
					, shader
					, position); 
				// lm.mesh.Transform.Parent = parentTransform;

				lm.fullRange = tm.GetFloat(lightId + "_range");
				Vector3 color = tm.GetVec3(lightId + "_color");
				lm.light = Light.CreatePointLight(color, lm.fullRange, position);

				list.Add(lm);
			}
		}

		public void CreateGreets(TunableManager tm, AssetManager assetManager
		, string greetName
		, PixelFont greetFont
		, string greet_material
		, Vector3 greetColor
		, ShaderProgram greetShader
		, float letterStep
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
						, greetFont, letterStep)
						, greet_material
						, greetShader
						, new Vector3(0, 0, 0));

						greet.alpha = 1.0f;
						greet.color = greetColor;
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
						Vector3 offset = greetOffset + (greetSpacing * gi);
						g.textMesh.Transform.Translation = new Vector4(offset, 1.0f);
						g.textMesh.Transform.Scale = greetScale;
					}
				}
		}

		// SYNC FUNCTIONS

		private void SyncCamera(SyncSystem syncer)
		{
			Renderer renderer = Renderer.GetSingleton();
			renderer.SyncCameraFrame();
			CameraComponent camera = renderer.GetCamera();
			camera.FOV = 90 + syncer.GetTrackValue(cameraFOV);
			camera.CreateMatrices();
		}

		private void SyncFadeout(SyncSystem syncer)
		{
			fadeAlpha = syncer.GetTrackValue(fadeoutAlpha);
		}


		public float DivideToPages(float valuePerPage, int pageIndex, float trackValue)
		{
			float value = (trackValue - (valuePerPage * pageIndex)) / valuePerPage;
			if (value > 1.0f)
			{
				value = 0;
			}
			return value;
		}

		public float DivideToItems(float valuePerItem, int itemIndex, float trackValue)
		{
			float value = MathHelper.Clamp((trackValue - (valuePerItem * itemIndex)) / valuePerItem, 0.0f, 1.0f);
			return value;
		}

		void SyncBunny(SyncSystem syncer)
		{
			bunnyMesh.Transform.SetRotation(syncer.GetTrackValue(bunnyRotation));
		}

		public void SyncGreets(SyncSystem syncer, List<GreetPage> greetPages)
		{
			float alphaPerPage = 1.0f / greetPages.Count;
			float wholeAlpha = syncer.GetTrackValue(textAlpha);
			for (int pi = 0; pi < greetPages.Count; pi++)
			{
				GreetPage p = greetPages[pi];
				p.progress = DivideToPages(alphaPerPage, pi, wholeAlpha);
				float alphaPerGreet = 1.0f / p.greets.Count;
				for (int gi = 0; gi < p.greets.Count; gi++)
				{
					Greeting g = p.greets[gi];
					g.alpha = DivideToItems(alphaPerGreet, gi, p.progress);
				}
			}
		}

		void SyncCrystals(SyncSystem syncer)
		{
			for (int s = 0; s < crystalMeshes.Count; s++)
			{
				Vector4 ot = crystalMeshes[s].mesh.Transform.Translation;
				crystalMeshes[s].mesh.Transform.Translation = new Vector4(ot.X, syncer.SceneProgress * crystalMeshes[s].speed, ot.Z, 1.0f);
				crystalMeshes[s].mesh.Transform.SetRotation(syncer.SceneProgress * crystalMeshes[s].speed / 10.0f * MathHelper.TwoPi);
			}
		}

		public void SyncMonolith(SyncSystem syncer)
		{
			float rotations = syncer.GetTrackValue(monolithRotation);
			monolith.Transform.SetRotation(rotations * MathHelper.Pi);
			Vector3 monoPos = monolith.Transform.GetWorldPosition();
			monolith.Transform.Translation = new Vector4(monoPos.X, syncer.GetTrackValue(monolithElevation), monoPos.Z, 1.0f);
		}

		private void SyncSkybox(SyncSystem syncer)
		{
			// Rotate star sphere world matrix
			// Rotate lighting
			float rot = syncer.GetTrackValue(skyRotation) * MathHelper.TwoPi;
			starSkyBox.Transform.SetRotation(rot);
			mountainLight.Direction = new Vector3(new Vector4(0, 0, 1, 0) * starSkyBox.Transform.rotationTransformMatrix);
		}

		private void SyncTelescope(Telescope scope, SyncSystem syncer)
		{
			float rotRad = MathHelper.DegreesToRadians(syncer.GetTrackValue(telescopeRotation));
			scope.tower.Transform.SetRotation(rotRad);

			float elevRad = MathHelper.DegreesToRadians(syncer.GetTrackValue(telescopeElevation));
			scope.antenna.Transform.SetRotation(elevRad);
		}

		private void SyncLightMeshList(List<LightMesh> list, float trackValue)
		{
			float listIntensity = DivideToPages(1.0f, 0, trackValue);
			float valuePerItem = (1.0f / list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				LightMesh lm = list[i];
				float itemValue = DivideToItems(valuePerItem, i, listIntensity);
				lm.activeRadius = lm.sphereRadius * itemValue;
				lm.light.SetAttenuation(lm.fullRange * itemValue);
				lm.mesh.Transform.Scale = lm.sphereRadius * itemValue;
			}
		}


		// DRAW FUNCTIONS

		public void DrawTelescopeArray(Renderer renderer)
		{
			for (int t = 0; t < telescopeArrayPositions.Count; t++)
			{
				DrawTelescope(renderer, telescopeArrayPositions[t]);
			}
		}

		public void DrawTelescope(Renderer renderer, Vector3 position)
		{
			telescope.tower.Transform.Translation = new Vector4(position, 1.0f);
			renderer.RenderMesh(telescope.tower);
			renderer.RenderMesh(telescope.antenna);
		}

		public void DrawSea(Renderer renderer)
		{
			renderer.SetActiveShader(seaMesh.ShaderProgram);
			// seaShader.SetVec2Uniform(texOffset1Location, offset1);
			// seaShader.SetVec2Uniform(texOffset2Location, offset2);
			// seaShader.SetMatrix4Uniform(normalMatrixLocation, ref seaNormalMatrix);
			renderer.RenderObject(seaMesh);
			// renderer.RenderObject(seaMesh_normals);
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
			if (g.alpha > 0.0f)
			{
				renderer.RenderShaderDataOwnerMesh(g.textMesh, g);
			}
		}

		public void DrawCrystals(Renderer renderer)
		{
			for (int s = 0; s < crystalMeshes.Count; s++)
			{
				renderer.RenderMesh(crystalMeshes[s].mesh);
			}
		}

		public void DrawMonolith(Renderer renderer)
		{
			renderer.RenderMesh(monolith);
			// renderer.RenderMesh(monolith_normals);
		}

		public void DrawLightMeshList(Renderer renderer, List<LightMesh> lightMeshList)
		{
			for (int i = 0; i < lightMeshList.Count; i++)
			{
				renderer.RenderShaderDataOwnerMesh(lightMeshList[i].mesh, lightMeshList[i]);
			}
			for (int i = 0; i < lightMeshList.Count; i++)
			{
				lightMeshList[i].light.Position = lightMeshList[i].mesh.Transform.GetWorldPosition();
				renderer.ActivateLight(lightMeshList[i].light, renderer.FirstPointLightIndex + i);
			}
		}

		public void DrawBunny(Renderer renderer)
		{
			renderer.SetActiveShader(bunnyMesh.ShaderProgram);
			// send light size data
			for (int i = 0; i < bunnyLights.Count; i++)
			{
				bunnyMesh.ShaderProgram.SetFloatUniform(bunnyLightsSizesLocations[i], bunnyLights[i].activeRadius);
			}

			renderer.RenderMesh(bunnyMesh);
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

		private void DrawStarSky(Renderer renderer, bool warp = false)
		{
			renderer.SetActiveSkybox(starSkyboxMaterial);
			renderer.SetSkyboxRotation(starSkyBox.Transform.CreateRotationMatrixFromAxisAngle());
			if (warp)
			{
				CameraComponent cam = renderer.GetCamera();
				float prevFOV = cam.FOV;
				cam.FOV = 90 + SyncSystem.GetSingleton().GetTrackValue(warpFOV);
				cam.CreateMatrices();
				renderer.RenderSky(starSkyBox);
				cam.FOV = prevFOV;
				cam.CreateMatrices();
			}
			else
			{
				renderer.RenderSky(starSkyBox);
			}
		}

		private void DrawPlanetSkybox(Renderer renderer)
		{
			renderer.SetActiveSkybox(planetSkyboxMaterial);
			renderer.SetSkyboxRotation(planetSkyBox.Transform.CreateRotationMatrixFromAxisAngle());
			renderer.RenderSky(planetSkyBox);
		}


		// SYNC AND DRAW

		public void Sync(SyncSystem syncer)
		{
			SyncCamera(syncer);
			SyncFadeout(syncer);
			SyncSkybox(syncer);

			int s = syncer.Scene;
			if (s == mountainsNumber)
			{
				SyncTelescope(telescope, syncer);
				SyncGreets(syncer, title);
			}
			else if (s == moonNumber)
			{
				SyncMonolith(syncer);
				SyncGreets(syncer, group_greetings);
			}
			else if (s == bunnyNumber)
			{
				// Sync lights
				SyncBunny(syncer);
				SyncLightMeshList(bunnyLights, syncer.GetTrackValue(bunnyLightProgress));
			}
			else if (s == warpNumber)
			{
				SyncBunny(syncer);
				SyncLightMeshList(bunnyLights, syncer.GetTrackValue(bunnyLightProgress));
			}
			else if (s == seaNumber || s == crystalsNumber)
			{
				SyncMonolith(syncer);
				SyncGreets(syncer, credits);
			}
			if (s == crystalsNumber)
			{
				// Crystals
				SyncCrystals(syncer);
			}
		}

		public void Draw(Renderer renderer)
		{

			int s = SyncSystem.GetSingleton().Scene;
			if (s == mountainsNumber)
			{
				DrawStarSky(renderer);
				renderer.ActivateLight(mountainLight, 0);

				renderer.RenderMesh(mountains);

				DrawTelescope(renderer, mainTelescopePosition);
				DrawTelescopeArray(renderer);

				// Transparent things last!
				DrawGreets(renderer, title);
			}
			else if (s == moonNumber)
			{
				DrawStarSky(renderer);
				renderer.ActivateLight(mountainLight, 0);
				DrawMonolith(renderer);

				DrawGreets(renderer, group_greetings);
			}
			else if (s == bunnyNumber)
			{
				DrawStarSky(renderer);
				DrawLightMeshList(renderer, bunnyLights);
				DrawBunny(renderer);
			}
			else if (s == warpNumber)
			{
				DrawStarSky(renderer, true);
				DrawLightMeshList(renderer, bunnyLights);
				DrawBunny(renderer);
			}
			else if (s == seaNumber || s == crystalsNumber)
			{
				DrawPlanetSkybox(renderer);
				renderer.ActivateLight(planetLight, 0);
				renderer.RenderObject(planetMountains);
				DrawSea(renderer);
				DrawMonolith(renderer);
				
				DrawGreets(renderer, credits);
			}
			if (s == crystalsNumber)
			{
				DrawCrystals(renderer);
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