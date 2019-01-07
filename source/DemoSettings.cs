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
		AudioSetting = AudioMode.OpenAL;
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

	public enum AudioMode
	{
		Midi,
		OpenAL
	}

	public bool AudioEnabled{ get; set; }
	public bool SyncEnabled{ get; set; }
	public CameraMode CameraSetting { get; set; }
	public AudioMode AudioSetting { get; set; }
	public Vector2 Resolution { get; set; }
	public double UpdatesPerSecond { get; set; }
	public string WindowTitle { get; set; }
	public string SyncFilePrefix { get; set; }

}
