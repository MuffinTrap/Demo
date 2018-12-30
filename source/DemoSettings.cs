using System;
using OpenTK;

public sealed class DemoSettings
{
	private static readonly DemoSettings defaults = new DemoSettings();
	private DemoSettings()
	{
		AudioEnabled = false;
		SyncEnabled = false;
		CameraSetting = CameraMode.Free;
		AudioSetting = AudioMode.OpenAL;
		Resolution = new Vector2(800, 600);
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

}
