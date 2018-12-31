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
        private bool running;

		SyncSystem syncSystem;
		Renderer renderer;
		DemoWrapper demoWrapper;
		IAudioSystem audioSystem;
		DemoSettings demoSettings;
		TestScene testScene;

        public MainWindow()
            : base(800, 600, 
                  GraphicsMode.Default,
                  "OpenTK party",
                  GameWindowFlags.Default,
                  DisplayDevice.Default,
                  3,	// OpenGL Major Version minimum
                  3,	// OpenGL Minor Version minimum
                  GraphicsContextFlags.ForwardCompatible)
        {
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
            running = true;


			demoWrapper = new DemoWrapper();
			demoWrapper.Create();
			demoSettings = demoWrapper.Demo.GetDemoSettings();
            CursorVisible = true;

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

			try
			{
				demoWrapper.Demo.Load(audioSystem, assetManager);
			}
			catch (Exception exception)
			{
				Logger.LogError(Logger.ErrorState.Critical, "Caught exception when creating Demo instance " + exception.Message);
				return;
			}

			Logger.LogPhase("OnLoad complete");
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
			if (running)
			{
				if (Logger.ProgramErrorState == Logger.ErrorState.Critical
				|| Logger.ProgramErrorState == Logger.ErrorState.Limited)
				{
					Logger.LogPhase("Error detected, program has stopped. ESC to Exit");
					running = false;

					if (demoSettings.SyncEnabled)
					{
						syncSystem.Stop();
					}
				}
			}


			if (demoSettings.SyncEnabled)
			{
				syncSystem.Sync();
				Title = $"Seconds: {syncSystem.GetSecondsElapsed():0} Row: {syncSystem.GetSyncRow()}";
			}

			testScene.Update();
			HandleKeyboardAndUpdateDemo();
        }

        private void HandleKeyboardAndUpdateDemo()
        {
            KeyboardState keyState = Keyboard.GetState();
			MouseState mouseState = Mouse.GetState();

			syncSystem.UpdateInput(keyState);
			renderer.UpdateInput(keyState, mouseState);
			
            if (keyState.IsKeyDown(Key.Escape))
            {
				ExitProgram();
            }

			testScene.Update();

			if (!demoSettings.SyncEnabled)
			{
				return;
			}

			//demoWrapper.Demo.Sync(syncSystem);
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (!running)
            {
                return;
            }

			testScene.Draw();

			demoWrapper.Demo.Draw(syncSystem, renderer);

			renderer.EndFrame();
            SwapBuffers();

            if (Error.checkGLError("OnRenderFrame"))
            {
                running = false;
            }
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
