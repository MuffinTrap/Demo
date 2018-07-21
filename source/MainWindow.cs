using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;


using RocketNet;

namespace OpenTkConsole
{
    public sealed class MainWindow : GameWindow
    {
        private bool running;

        // Syncing
        public Device syncDevice;

		Track sceneNumber;
		Track cameraFrame;

		private int currentSceneIndex = 0;
		private float currentCameraFrame = 0.0f;

        private int bpm;
        private int rowsPerBeat;

        private float songLength;
        private int syncRow;

        bool paused;
		bool spaceDown;
		
		List<IScene> scenes;
        Stopwatch timer;

		// Audio
		IAudioSystem audioSystem;
		Audio testSong;

        public MainWindow()
            : base(800, 460, 
                  GraphicsMode.Default,
                  "OpenTK party",
                  GameWindowFlags.Default,
                  DisplayDevice.Default,
                  3, 
                  0,
                  GraphicsContextFlags.ForwardCompatible)
        {
            Title += "OpenGL version: " + GL.GetString(StringName.Version);

			Logger.LogPhase("OpenTK initialized. OpenGL version: " + GL.GetString(StringName.Version));
			base.TargetUpdateFrequency = DemoSettings.GetDefaults().UpdatesPerSecond;

			base.Location = new Point(510, 10);

        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
        }

        protected override void OnLoad(EventArgs e)
        {
            CursorVisible = true;
            running = true;
			paused = true;
			spaceDown = false;

			// SYNC
			loadSyncer();
			bpm = 120;
			rowsPerBeat = 4;
			songLength = 5.0f; // seconds

			string dataFolder = "data";
			AssetManager.WorkingDir = dataFolder;
			AssetManager assetManager = AssetManager.GetAssetManagerSingleton();

			Logger.LogPhase("Asset manager is created");
			assetManager.LoadAll();
			assetManager.printLoadedAssets();

			// Materials and scenes
			// Pass syncer to scenes.

			CameraComponent mainCamera = new CameraComponent();
			mainCamera.ActivateForDrawing();
			scenes = new List<IScene>();
			try
			{
				scenes.Add(new TestScene(mainCamera));
				scenes.Add(new LightScene(mainCamera)); // This scene handles the camera update
				//scenes.Add(new Scene2D());  // This is the gui scene
				//scenes.Add(assetManager.GetScene("tia.sce"));

				foreach (IScene s in scenes)
				{
					s.loadScene(assetManager);
				}
			}
			catch (Exception exception)
			{
				Logger.LogError(Logger.ErrorState.Critical, "Caugh exception when loading scene " + exception.Message);
			}

			Logger.LogPhase("Scenes have been loaded");
			

			// Audio
			if (DemoSettings.GetDefaults().AudioEnabled)
			{
				audioSystem = new OpenALAudio();
				initAudio();
			}
			

			// Timing
			timer = new Stopwatch();
            timer.Start();

			Logger.LogPhase("OnLoad complete");
        }

        public bool demoPlaying()
        {
            return running;
        }

        public void SetRowFromEditor(int row)
        {
            syncRow = row;
        }

        public void PauseFromEditor(bool pause)
        {
            paused = pause;
        }

        void Sync()
        {
            float secondsElapsed = 0.0f;
            // update sync values only when playing
            if (!paused)
            {
                // Calculate sync row.
                long elapsedMS = timer.ElapsedMilliseconds;
                secondsElapsed = (float)(elapsedMS / (float)1000);
                float minutesElapsed = secondsElapsed / 60.0f;

                if (secondsElapsed > songLength)
                {
                    // loop around;
                    timer.Restart();
                }

                float beatsElapsed = (float)bpm * minutesElapsed;
                float rowsElapsed = rowsPerBeat * beatsElapsed;
                float floatRow = rowsElapsed;
                int currentRow = (int)Math.Floor(floatRow);

                syncRow = currentRow;
            }

            bool updateOk = syncDevice.Update(syncRow);
            if (!updateOk)
            {
                connectSyncer();
            }

			currentSceneIndex = (int)Math.Floor(sceneNumber.GetValue(syncRow));
			currentCameraFrame = cameraFrame.GetValue(syncRow);

			Title = $"Seconds: {secondsElapsed:0} Row: {syncRow}";
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
				}
			}

			HandleKeyboardAndUpdateScene();
			if (DemoSettings.GetDefaults().SyncEnabled)
			{
				Sync();
			}
        }

        private void HandleKeyboardAndUpdateScene()
        {
            var keyState = Keyboard.GetState();

			if (keyState.IsKeyDown(Key.Space))
			{
				spaceDown = true;
			}
			
			if (spaceDown && keyState.IsKeyUp(Key.Space))
			{
				spaceDown = false;
				paused = !paused;
			}
			
            if (keyState.IsKeyDown(Key.Escape))
            {
				ExitProgram();
               
            }

			// Pass input to scene

			var mouseState = Mouse.GetState();

			if (DemoSettings.GetDefaults().SyncEnabled)
			{
				scenes[currentSceneIndex].updateScene(keyState, mouseState);
			}
			else if (!paused)
			{
				foreach (IScene s in scenes)
				{
					s.updateScene(keyState, mouseState);
				}
			}
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (!running)
            {
                return;
            }

			// Take scene number from track.
			// Draw that scene

            Color4 backColor;
            backColor.A = 1.0f;
			backColor.R = 0.1f;
			backColor.G = 0.1f;
            backColor.B = 0.1f;
            GL.ClearColor(backColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			// Draw models

			if (DemoSettings.GetDefaults().SyncEnabled)
			{
				scenes[currentSceneIndex].drawScene(currentCameraFrame);
			}
			else
			{
				foreach (IScene s in scenes)
				{
					s.drawScene(0);
				}
			}

			// Scene drawing ends
			
            if (Error.checkGLError("OnRenderFrame"))
            {
                running = false;
            }

            SwapBuffers();
        }
		
		void loadSyncer()
		{
			syncDevice = new Device("test", false);
			sceneNumber = syncDevice.GetTrack("Scene");
			cameraFrame = syncDevice.GetTrack("CameraFrame");
			
			if (DemoSettings.GetDefaults().SyncEnabled)
			{
				connectSyncer();
				
				syncDevice.IsPlaying = demoPlaying;
				syncDevice.Pause = PauseFromEditor;
				syncDevice.SetRow = SetRowFromEditor;
			}
		}
		
		void connectSyncer()
		{
			try
			{
				syncDevice.Connect();

			} catch(System.Net.Sockets.SocketException socketE)
			{

				Logger.LogError(Logger.ErrorState.Critical, "Socket exception: " + socketE.Message);
				running = false;
			}
		}

		void initAudio()
		{
			string audioFileName = "../data/music/bosca.wav";
			if (audioSystem.Initialize())
			{
				testSong = audioSystem.LoadAudioFile(audioFileName);
			}
		}

		void startAudio()
		{
			audioSystem.PlayAudioFile(testSong);
		}

		void shutDownAudio()
		{
			audioSystem.Shutdown();
		}

		private void ExitProgram()
		{
			running = false;
			timer.Stop();
			Logger.LogPhase("Exit Program");

			cleanupAndExit();
		}

		void cleanupAndExit()
		{
			syncDevice.Dispose();
			if (DemoSettings.GetDefaults().AudioEnabled)
			{
				shutDownAudio();
			}
			Logger.ResetColors();
			Exit();
		}
    }

}
