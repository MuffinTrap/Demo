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
	static class Error
	{
		static public bool checkGLError(string place)
		{
			bool errorFound = false;
			while ( GL.GetError() != ErrorCode.NoError)
			{
				Console.WriteLine("GL error in " + place);
				errorFound = true;
			}
			return errorFound;
		}
	}
	
    public sealed class MainWindow : GameWindow
    {
        private bool running;

        // Syncing
        public Track redColorTrack;
        public Track greenColorTrack;
        public Device syncDevice;

        private int bpm;
        private int rowsPerBeat;

        private float songLength;
        private int syncRow;

        bool paused;
		bool spaceDown;

        bool useSync;

		Scene testScene;
        Stopwatch timer;

        public MainWindow()
            : base(400, 400, 
                  GraphicsMode.Default,
                  "OpenTK party",
                  GameWindowFlags.Default,
                  DisplayDevice.Default,
                  3, 
                  0,
                  GraphicsContextFlags.ForwardCompatible)
        {
            Title += "OpenGL version: " + GL.GetString(StringName.Version);
			Console.WriteLine("OpenTK initialized. OpenGL version: " + GL.GetString(StringName.Version));
            base.TargetUpdateFrequency = 120.0;
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
			useSync = false;
			// /// ////////////////////

			loadSyncer();
            
			testScene = new Scene();
			testScene.loadScene();

            bpm = 120;
            rowsPerBeat = 4;
            songLength = 5.0f; // seconds

            timer = new Stopwatch();

            timer.Start();
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

            Title = $"Seconds: {secondsElapsed:0} Row: {syncRow}";
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            HandleKeyboard();
			if (useSync)
			{
				Sync();
			}
        }

        private void HandleKeyboard()
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
				running = false;
                timer.Stop();
                syncDevice.Dispose();
                Exit();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (!running)
            {
                return;
            }

            Color4 backColor;
            backColor.A = 1.0f;
            backColor.R = redColorTrack.GetValue(syncRow);
            backColor.G = greenColorTrack.GetValue(syncRow);
            backColor.B = 0.3f;
            GL.ClearColor(backColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			// Draw models
			testScene.drawScene();
			
            if (Error.checkGLError("OnRenderFrame"))
            {
                running = false;
            }

            SwapBuffers();
        }
		
		void loadSyncer()
		{
			syncDevice = new Device("test", false);
            redColorTrack = syncDevice.GetTrack("redColor");
            greenColorTrack = syncDevice.GetTrack("greenColor");

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

				Console.WriteLine("Socket exception: " + socketE.Message);
				running = false;
			}
		}
    }

}
