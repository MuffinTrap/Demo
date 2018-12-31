using System;
using System.Diagnostics;

using OpenTK.Input;

using RocketNet;

namespace MuffinSpace
{

	public class SyncSystem
	{
		public static SyncSystem GetSingleton()
		{
			if (singleton == null)
			{
				singleton = new SyncSystem();
			}
			return singleton;
		}
		private static SyncSystem singleton;

        public Device syncDevice;

        Stopwatch timer;

		Track sceneNumber;
		Track cameraFrame;
		Track audioTrack;

		public int Scene { get; private set; }
		public int Frame { get; private set; }
		public float FrameProgress { get; private set; }
		public float SceneProgress { get; private set; }
		public float AudioTrack { get; private set; }

        private int bpm;
        private int rowsPerBeat;

        private float songLength;
        private int syncRow;

		float secondsElapsed = 0.0f;

        bool paused;
		bool spaceDown;
		bool running;

		private SyncSystem()
		{
			paused = true;
			spaceDown = false;

			bpm = 120;
			rowsPerBeat = 4;
			songLength = 5.0f; // seconds

			timer = new Stopwatch();

			syncDevice = new Device("Demo", false);
			sceneNumber = syncDevice.GetTrack("Scene");
			cameraFrame = syncDevice.GetTrack("CameraFrame");
			audioTrack = syncDevice.GetTrack("Audio");
		}

		private void Connect()
		{
			try
			{
				syncDevice.Connect();
			}
			catch(System.Net.Sockets.SocketException socketE)
			{
				Logger.LogError(Logger.ErrorState.Critical, "Socket exception: " + socketE.Message);
			}
				
			syncDevice.IsPlaying = DemoPlaying;
			syncDevice.Pause = PauseFromEditor;
			syncDevice.SetRow = SetRowFromEditor;
		}

		// Functions given to Device
        public bool DemoPlaying()
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

		public void Start()
		{
			// Timing
			Connect();
			timer.Start();
			running = true;
		}

        public void Sync()
        {
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
				Connect();
            }

			float sceneValue = sceneNumber.GetValue(syncRow); ;
			float sceneFloor = (float)Math.Floor(sceneValue);
			Scene = (int)sceneFloor;
			SceneProgress = sceneValue - sceneFloor;
			float frameValue = cameraFrame.GetValue(syncRow);
			float frameFloor = (float)Math.Floor(frameValue);
			Frame = (int)frameFloor;
			FrameProgress = frameValue - frameFloor;

			AudioTrack = (float)Math.Floor(audioTrack.GetValue(syncRow));

        }

		public void Stop()
		{
			timer.Stop();
			running = false;
		}

		public void CleanAndExit()
		{
			syncDevice.Dispose();
		}

		public void UpdateInput(KeyboardState keyState)
		{
			if (keyState.IsKeyDown(Key.Space))
			{
				spaceDown = true;
			}
			
			if (spaceDown && keyState.IsKeyUp(Key.Space))
			{
				spaceDown = false;
				paused = !paused;
			}
		}

		public void SetAudioProperties(int bpmParam, float songLengthSeconds)
		{
			bpm = bpmParam;
			songLength = songLengthSeconds;
		}

		public bool IsPaused()
		{
			return paused;
		}

		public float GetSecondsElapsed()
		{
			return secondsElapsed;
		}

		public int GetSyncRow()
		{
			return syncRow;
		}

	}

}