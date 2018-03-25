using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using OpenTK.Audio.OpenAL;

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

		bool useSync = false;

		// Audio
		bool useAudio = false;
		
		List<IScene> scenes;
        Stopwatch timer;

        public MainWindow()
            : base(854, 480, 
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
            base.TargetUpdateFrequency = 120.0;

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
			useSync = true;
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
			scenes = new List<IScene>();
			try
			{
				//scenes.Add(new RotatingScene());
				scenes.Add(new TestScene());
				scenes.Add(new Scene2D());
				scenes.Add(assetManager.GetScene("tia.sce"));

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
			useAudio = false;

			if (useAudio)
			{
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
			if (useSync)
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

			// Take scene number from track
			scenes[currentSceneIndex].updateScene(keyState, mouseState);

			/*
			foreach (IScene s in scenes)
			{
				s.updateScene(keyState, mouseState);
			}
			*/
        }

		private void ExitProgram()
		{
			running = false;
			timer.Stop();
			Logger.LogPhase("Exit Program");

			cleanupAndExit();
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
			
			scenes[currentSceneIndex].drawScene(currentCameraFrame);
			/*
			foreach(IScene s in scenes)
			{
				s.drawScene();
			}
			*/

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
			
			if (useSync)
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
			IntPtr nullDevice = System.IntPtr.Zero;
			IList<string> allDevices = Alc.GetString(nullDevice, AlcGetStringList.DeviceSpecifier);
			foreach(string s in allDevices)
			{
				Logger.LogInfo("OpenAL device " + s);
			}

			// Open preferred device
			ContextHandle alContext;
			IntPtr ALDevicePtr = Alc.OpenDevice(null);
			if (ALDevicePtr != null)
			{
				int[] deviceAttributes = null;
				alContext = Alc.CreateContext(ALDevicePtr, deviceAttributes);
				Alc.MakeContextCurrent(alContext);
			}
			else
			{
				Logger.LogError(Logger.ErrorState.Critical, "Could not get AL device");
				return;
			}

			string alRenderer = AL.Get(ALGetString.Renderer);
			string alVendor = AL.Get(ALGetString.Vendor);
			string alVersion = AL.Get(ALGetString.Version);

			Logger.LogInfo(string.Format("OpenAL Renderer {0}  Vendor {1}  Version {2}", alRenderer, alVendor, alVersion));


			Error.checkALError("initAudio");
			int alBuffer = AL.GenBuffer();
			Error.checkALError("initAudio genBuffer");

			
			int frequenzy = 44100;

			// Buffer data
			bool dataisVorbis = false;

			string vorbisEXTName = "AL_EXT_vorbis";
			if (AL.IsExtensionPresent(vorbisEXTName) && dataisVorbis)
			{
				Logger.LogInfo("AL can use vorbis");
				IntPtr vorbisBuffer = System.IntPtr.Zero;
				int vorbisSize = 0;
				AL.BufferData(alBuffer, ALFormat.VorbisExt, vorbisBuffer, vorbisSize, frequenzy);
			}
			else
			{
				// Load wav
				
				FileStream audioFile = File.Open("../data/music/bosca.wav", FileMode.Open, FileAccess.Read);
				long wavSize = audioFile.Length;
				byte[] audioContents = new byte[wavSize];

				audioFile.Read(audioContents, 0, (int)wavSize);
				IntPtr wavBuffer = Marshal.AllocHGlobal(audioContents.Length);
				Marshal.Copy(audioContents, 0, wavBuffer, audioContents.Length);

				AL.BufferData(alBuffer, ALFormat.Stereo16, wavBuffer, (int)wavSize, frequenzy);
				Marshal.FreeHGlobal(wavBuffer);
				audioFile.Close();
			}

			Error.checkALError("initAudio bufferAudio");

			int alSource = AL.GenSource();
			Error.checkALError("initAudio genSource");

			// Attach buffer to source.
			AL.Source(alSource, ALSourcei.Buffer, alBuffer);

			// Set listener and source to same place
			Vector3 listenerPos = new Vector3(0, 0, 0);
			Vector3 sourcePos = new Vector3(0, 0, 0);
			AL.Listener(ALListener3f.Position, ref listenerPos);
			AL.Source(alSource, ALSource3f.Position, ref sourcePos);

			// Play buffer
			AL.SourcePlay(alSource);
		}

		void shutDownAudio()
		{
			ContextHandle alContext = Alc.GetCurrentContext();
			IntPtr alDevice = Alc.GetContextsDevice(alContext);
			ContextHandle emptyContext = ContextHandle.Zero;
			Alc.MakeContextCurrent(emptyContext);
			Alc.DestroyContext(alContext);
			Alc.CloseDevice(alDevice);
		}

		void cleanupAndExit()
		{
			syncDevice.Dispose();
			if (useAudio)
			{
				shutDownAudio();
			}
			Exit();
		}
    }

}
