using System;
using System.Diagnostics;
using System.IO;

using OpenTK.Input;

using RocketNet;

namespace MuffinSpace
{
	public enum SyncMode
	{
		Client,
		Player,
		Manual
	}

	public enum SyncState
	{
		Paused,
		Running,
		Finished
	}

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

        private Device syncDevice;

        Stopwatch timer;

		Track sceneNumber;
		Track cameraFrame;

		public int Scene { get; private set; }
		public int Frame { get; private set; }
		public float FrameProgress { get; private set; }
		public float SceneProgress { get; private set; }

		private SyncMode operationMode = SyncMode.Manual;
		private float manualSceneAdvanceRate = 0.1f;

		public SyncState State { get; private set; }

        private int bpm;
        private int rowsPerBeat;

        private float songLength;
        private float syncRow;

		float secondsElapsed = 0.0f;

		private SyncSystem()
		{
			State = SyncState.Paused;

			bpm = 120;
			rowsPerBeat = 4;
			songLength = 5.0f; // seconds

			timer = new Stopwatch();

			Scene = 0;
			SceneProgress = 0.0f;
			FrameProgress = 0.0f;
			Frame = 0;

		}

		private bool Connect()
		{
			bool connected = false;
			try
			{
				connected = syncDevice.Connect();
			}
			catch(System.Net.Sockets.SocketException socketE)
			{
				Logger.LogError(Logger.ErrorState.Critical, "Socket exception: " + socketE.Message);
			}
				
			syncDevice.IsPlaying = DemoPlaying;
			syncDevice.Pause = PauseFromEditor;
			syncDevice.SetRow = SetRowFromEditor;

			return connected;
		}

		// Functions given to Device
        public bool DemoPlaying()
        {
			return (State == (SyncState.Running));
        }

        public void SetRowFromEditor(int row)
        {
            syncRow = row;
        }

        public void PauseFromEditor(bool pause)
        {
            if (pause)
			{
				State = SyncState.Paused;
			}
			else
			{
				State = SyncState.Running;
			}
        }

		public SyncMode GetOperationMode()
		{
			return operationMode;
		}

		public void Start(string fileNamePrefix)
		{
			operationMode = SyncMode.Manual;

			syncDevice = new Device(fileNamePrefix, false);
			bool editorPresent = Connect();
			if (editorPresent)
			{
				operationMode = SyncMode.Client;
			}
			else
			{
				syncDevice = new Device(fileNamePrefix, true);
				operationMode = SyncMode.Player;
			}

			if (operationMode != SyncMode.Manual)
			{
				sceneNumber = syncDevice.GetTrack("Scene");
				cameraFrame = syncDevice.GetTrack("CameraFrame");
			}
			else
			{
				Logger.LogInfo("Syncer file nor editor was found. Enabling manual mode");
			}
			timer.Start();
			if (operationMode == SyncMode.Player)
			{
				State = SyncState.Running;
			}
		}

		public void StartManual()
		{
			operationMode = SyncMode.Manual;
			State = SyncState.Paused;
		}

        public void Sync()
        {
            // update sync values only when playing
            if (State == SyncState.Running)
            {
                // Calculate sync row.
                long elapsedMS = timer.ElapsedMilliseconds;
                secondsElapsed = (float)(elapsedMS / (float)1000);
                float minutesElapsed = secondsElapsed / 60.0f;

                if (secondsElapsed > songLength)
                {
					if (operationMode == SyncMode.Player)
					{
						// Only in this case
						State = SyncState.Finished;
					}
					else
					{
						State = SyncState.Paused;
					}
                }

                float beatsElapsed = (float)bpm * minutesElapsed;
                float rowsElapsed = rowsPerBeat * beatsElapsed;
                syncRow = rowsElapsed;
            }

			if (operationMode != SyncMode.Manual)
			{
				bool updateOk = syncDevice.Update((int)Math.Floor(syncRow));
				if (!updateOk && operationMode == SyncMode.Client)
				{
					Connect();
				}

				float sceneValue = sceneNumber.GetValue(syncRow);
				float sceneFloor = (float)Math.Floor(sceneValue);
				Scene = (int)sceneFloor;
				SceneProgress = sceneValue - sceneFloor;
				float frameValue = cameraFrame.GetValue(syncRow);
				float frameFloor = (float)Math.Floor(frameValue);
				Frame = (int)frameFloor;
				FrameProgress = frameValue - frameFloor;
			}
        }

		public void Restart()
		{
			timer.Restart();
			syncRow = 0;
			State = SyncState.Running;
		}

		public void Pause()
		{
			timer.Stop();
			State = SyncState.Paused;
		}

		public void Run()
		{
			timer.Start();
			State = SyncState.Running;
		}

		public void CleanAndExit()
		{
			Pause();
			if (operationMode != SyncMode.Manual)
			{
				syncDevice.Dispose();
			}
		}

		
		// Manual mode scene change and progress
		public void ChangeToNextScene()
		{
			if (operationMode == SyncMode.Manual)
			{
				Scene++;
			}
		}

		public void ChangeToPrevScene()
		{
			if (operationMode == SyncMode.Manual)
			{
				Scene--;
				if (Scene < 0)
				{
					Scene = 0;
				}
			}
		}
		public void RetreatSceneProgress()
		{
			if (operationMode == SyncMode.Manual)
			{
				SceneProgress -= manualSceneAdvanceRate;
				if (SceneProgress < 0.0f)
				{
					SceneProgress = 0.0f;
				}
			}
		}
		public void AdvanceSceneProgress()
		{
			if (operationMode == SyncMode.Manual)
			{
				SceneProgress += manualSceneAdvanceRate;
				if (SceneProgress > 1.0f)
				{
					SceneProgress = 1.0f;
				}
			}
		}

		public void SetAudioProperties(int bpmParam, float songLengthSeconds, int rowsPerPeatParam)
		{
			bpm = bpmParam;
			songLength = songLengthSeconds;
			rowsPerBeat = rowsPerPeatParam;
		}

		public void PrintEditorRowAmount()
		{
			float beats = bpm * (songLength / 60.0f);
			float rowsForBeats = (float)Math.Ceiling(rowsPerBeat * beats);
			Logger.LogInfoLinePart("With " + bpm + " bpm " + songLength + " seconds and " + rowsPerBeat + " rows per beat,", ConsoleColor.White);
			Logger.LogInfoLinePart(" " + rowsForBeats, ConsoleColor.Red);
			Logger.LogInfoLinePart(" rows are needed in editor.", ConsoleColor.White);
			Logger.LogInfoLineEnd();
		}

		public void SetManualSceneAdvanceRate(float rate)
		{
			if (rate > 0.0f && rate < 1.0f)
			{
				manualSceneAdvanceRate = rate;
			}
		}

		public bool IsPaused()
		{
			return (State == SyncState.Paused);
		}

		public float GetSecondsElapsed()
		{
			return secondsElapsed;
		}

		public float GetSyncRow()
		{
			return syncRow;
		}

		public Track GetTrack(string trackName)
		{
			if (operationMode == SyncMode.Manual)
			{
				Track dummy = new Track();
				return dummy;
			}
			else
			{
				return syncDevice.GetTrack(trackName);
			}
		}
	}
}