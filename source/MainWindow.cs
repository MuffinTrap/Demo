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

		private class Button
		{
			bool down;
			Key keyName;

			public Button(Key tiedKey)
			{
				keyName = tiedKey;
				down = false;
			}
			public bool Pressed(KeyboardState keyState)
			{
				if (keyState.IsKeyDown(keyName))
				{
					down = true;
					return false;
				}
				else if (down && keyState.IsKeyUp(keyName))
				{
					down = false;
					return true;
				}
				return false;
			}
		}

		Button InputEnabledButton;
		Button ReloadDemoButton;
		Button CameraModeButton;
		Button CameraSpeedUpButton;
		Button CameraSpeedDownButton;
		Button PrintFrameButton;
		Button RestartDemoButton;

		bool inputEnabled = true;

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
            Title += "OpenGL version: " + GL.GetString(StringName.Version);

			Logger.LogPhase("OpenTK initialized. OpenGL version: " + GL.GetString(StringName.Version));
			base.TargetUpdateFrequency = 120;
			base.Location = new Point(0, 0);
        }

        protected override void OnResize(EventArgs e)
        {
			renderer.ResizeScreen(Width, Height);
        }

        protected override void OnLoad(EventArgs e)
        {
			InputEnabledButton = new Button(Key.F4);
			ReloadDemoButton = new Button(Key.F5);
			RestartDemoButton = new Button(Key.F6);

			CameraModeButton = new Button(Key.F9);
			CameraSpeedDownButton = new Button(Key.F10);
			CameraSpeedUpButton = new Button(Key.F11);
			PrintFrameButton = new Button(Key.F12);

			tunableManager = TunableManager.GetSingleton();
			syncSystem = SyncSystem.GetSingleton();
			renderer = Renderer.GetSingleton();

			string dataFolder = "data";
			AssetManager.WorkingDir = dataFolder;
			AssetManager assetManager = AssetManager.GetSingleton();

			Logger.LogPhase("Asset manager is created");
			assetManager.LoadAll();
			assetManager.printLoadedAssets();
			Logger.LogPhase("Assets have been loaded");

			tunableManager.ReloadValues();
			Logger.LogPhase("Config file have been loaded");

			demoWrapper = new DemoWrapper();
			demoWrapper.Create();
			demoSettings = demoWrapper.Demo.GetDemoSettings();


            CursorVisible = true;

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
				demoWrapper.SetAudioSystem(audioSystem);
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
				demoWrapper.Demo.Load(asman, syncSystem);
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
				Title = demoSettings.WindowTitle;
				Title += "Seconds: " + syncSystem.GetSecondsElapsed() + " Row: " + syncSystem.GetSyncRow() + " Scene : " + syncSystem.Scene + " progress: " + syncSystem.SceneProgress;
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

			if (InputEnabledButton.Pressed(keyState))
			{
				inputEnabled = !inputEnabled;
				
			}
			if (CameraModeButton.Pressed(keyState))
			{
				renderer.GetCamera().FreeMode = !renderer.GetCamera().FreeMode;

			}
			if (CameraSpeedUpButton.Pressed(keyState))
			{
				renderer.GetCamera().Speed += renderer.GetCamera().SpeedStep;

			}
			if (CameraSpeedDownButton.Pressed(keyState))
			{
				renderer.GetCamera().Speed -= renderer.GetCamera().SpeedStep;
			}

			if (PrintFrameButton.Pressed(keyState))
			{
				Logger.LogInfo("Frame Pos: " + Logger.PrintVec3(renderer.GetCamera().Position) + " Dir: " + Logger.PrintVec3(renderer.GetCamera().Direction));
			}

			if (ReloadDemoButton.Pressed(keyState))
			{
				loadCompleted = false;
				LoadDemo();
			}
			if (RestartDemoButton.Pressed(keyState))
			{
				if (demoSettings.SyncEnabled)
				{
					syncSystem.Restart();
					demoWrapper.Restart();
				}
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

			if (demoSettings.SyncEnabled)
			{
				demoWrapper.Demo.Sync(syncSystem);
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
