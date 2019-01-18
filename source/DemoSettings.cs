using System;
using OpenTK;

public sealed class DemoSettings
{
	private static readonly DemoSettings defaults = new DemoSettings();
	private DemoSettings()
	{
		AudioEnabled = false;
		SyncEnabled = true;
		CameraSetting = CameraMode.Frames;
		AudioEngineSetting = AudioEngine.System;
		Resolution = new Vector2(1024, 720);
		UpdatesPerSecond = 120.0;
	}

	public static DemoSettings GetDefaults()
	{
		return defaults;
	}

	public enum CameraMode
	{ 
		Free,
		Frames,
	};

	public enum AudioEngine
	{
		Dummy,
		System,
		NAudio
	}

	public bool AudioEnabled{ get; set; }
	public bool SyncEnabled{ get; set; }
	public CameraMode CameraSetting { get; set; }
	public AudioEngine AudioEngineSetting { get; set; }
	public Vector2 Resolution { get; set; }
	public double UpdatesPerSecond { get; set; }
	public string WindowTitle { get; set; }
	public string SyncFilePrefix { get; set; }

}
