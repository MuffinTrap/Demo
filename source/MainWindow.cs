using System;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;



namespace MuffinSpace
{
	public sealed class MainWindow : GameWindow
    {
		private bool running = false;
		bool loadCompleted = false;

		bool inputEnabled = true;
		bool f6Down = false;
		bool f9Down = false;

		SyncSystem syncSystem;
		Renderer renderer;
		DemoWrapper demoWrapper;
		IAudioSystem audioSystem;
		DemoSettings demoSettings;
		TunableManager tunableManager;
		TestScene testScene;

        public MainWindow()
            : base(1066, 600, 
                  GraphicsMode.Default,
                  "OpenTK party",
                  GameWindowFlags.Default,
                  DisplayDevice.Default,
                  3,	// OpenGL Major Version minimum
                  3,	// OpenGL Minor Version minimum
                  GraphicsContextFlags.ForwardCompatible)
        {
			#if (MUFFIN_PLATFORM_WINDOWS)
				Logger.LogInfo("Windows platform defined");
			#elif (MUFFIN_PLATFORM_LINUX)
				Logger.LogInfo("Linux platform defined");
			#endif

            Title += "OpenGL version: " + GL.GetString(StringName.Version);

			Logger.LogPhase("OpenTK initialized. OpenGL version: " + GL.GetString(StringName.Version));
			base.TargetUpdateFrequency = 120;
			base.Location = new Point(510, 10);
        }

        protected override void OnResize(EventArgs e)
        {
			renderer.ResizeScreen(Width, Height);
        }

        protected override void OnLoad(EventArgs e)
        {
			demoWrapper = new DemoWrapper();
			demoWrapper.Create();
			demoSettings = demoWrapper.Demo.GetDemoSettings();
            CursorVisible = true;
			tunableManager = TunableManager.GetSingleton();

			// SYNC
			syncSystem = SyncSystem.GetSingleton();

			renderer = Renderer.GetSingleton();

			string dataFolder = "data";
			AssetManager.WorkingDir = dataFolder;
			AssetManager assetManager = AssetManager.GetSingleton();

			Logger.LogPhase("Asset manager is created");
			assetManager.LoadAll();
			assetManager.printLoadedAssets();
			Logger.LogPhase("Assets have been loaded");

			// MenuSystem.GetSingleton().ReadFromFile(dataFolder + "/tunables.json");

			
			// Audio
			if (demoSettings.AudioEnabled)
			{
				Logger.LogPhase("Initializing audio system");
				audioSystem = new OpenALAudio();
				bool audioInitOk = audioSystem.Initialize();
				if (!audioInitOk)
				{
					Logger.LogError(Logger.ErrorState.Critical, "Audio system failed to initialize.");
					return;
				}
				Logger.LogPhase("Audio has been initialized");
			}
			
			if (demoSettings.SyncEnabled)
			{
				syncSystem.Start();
			}


			testScene = new TestScene();
			testScene.Load(assetManager);


			LoadDemo();
			Logger.LogPhase("OnLoad complete");
			loadCompleted = true;
            running = true;
        }

		private void LoadDemo()
		{
			AssetManager asman = AssetManager.GetSingleton();
			TunableManager tm = TunableManager.GetSingleton();
			tm.ReloadValues();
			try
			{
				demoWrapper.Demo.Load(audioSystem, asman, syncSystem);
			}
			catch (Exception exception)
			{
				Logger.LogError(Logger.ErrorState.Critical, "Caught exception when creating Demo instance " + exception.Message);
				return;
			}
			loadCompleted = true;
		}

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
			if (running && (Logger.ProgramErrorState == Logger.ErrorState.Critical
			|| Logger.ProgramErrorState == Logger.ErrorState.Limited))
			{
				Logger.LogPhase("Error detected, program has stopped. ESC to Exit");
				running = false;

				if (demoSettings.SyncEnabled)
				{
					syncSystem.Stop();
				}
			}

			if (demoSettings.SyncEnabled)
			{
				syncSystem.Sync();
				Title = $"Seconds: {syncSystem.GetSecondsElapsed():0} Row: {syncSystem.GetSyncRow()}";
			}

			HandleKeyboardAndUpdateDemo();
        }

        private void HandleKeyboardAndUpdateDemo()
        {
            KeyboardState keyState = Keyboard.GetState();
			MouseState mouseState = Mouse.GetState();

            if (keyState.IsKeyDown(Key.Escape))
            {
				ExitProgram();
            }

			if (keyState.IsKeyDown(Key.F6))
			{
				f6Down = true;
				
			}
			if (keyState.IsKeyUp(Key.F6) && f6Down)
			{
				f6Down = false;
				inputEnabled = !inputEnabled;
			}

			if (keyState.IsKeyDown(Key.F9))
			{
				f9Down = true;
			}
			if (keyState.IsKeyUp(Key.F9) && f9Down)
			{
				f9Down = false;
				loadCompleted = false;
				LoadDemo();
			}
			
			if (!running)
			{
				return;
			}

			if (!loadCompleted)
			{
				return;
			}

			syncSystem.UpdateInput(keyState);
			if (inputEnabled)
			{
				renderer.UpdateInput(keyState, mouseState);
			}
			testScene.Update();

			demoWrapper.Demo.Sync(syncSystem);
			if (!demoSettings.SyncEnabled)
			{
				return;
			}

        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (!running)
            {
                return;
            }
			if (!loadCompleted)
			{
				return;
			}

			renderer.StartFrame();

			testScene.Draw();

			demoWrapper.Demo.Draw(renderer);

			renderer.EndFrame();
            SwapBuffers();

			Error.checkGLError("OnRenderFrame");
        }
		
		private void ExitProgram()
		{
			running = false;
			Logger.LogPhase("Exit Program");

			CleanupAndExit();
		}

		void CleanupAndExit()
		{
			syncSystem.Stop();
			syncSystem.CleanAndExit();

			if (demoSettings.AudioEnabled)
			{
				audioSystem.CleanAndExit();
			}

			Logger.ResetColors();

			Exit();
		}
    }
}
