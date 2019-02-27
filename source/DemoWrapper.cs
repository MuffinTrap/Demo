namespace MuffinSpace
{
	public class DemoWrapper
	{
		public BunnyDemo Demo { get; set; }
		public bool Running { get; set; }

		public void Create()
		{
			Demo = new BunnyDemo();
			Running = false;
		}

		public DemoSettings GetDemoSettings()
		{
			return Demo.GetDemoSettings();
		}

		public void SetAudioSystem(IAudioSystem audioSystem)
		{
			Demo.SetAudioSystem(audioSystem);
		}

		public void Restart()
		{
			Demo.Restart();
		}

		public void CleanUpAndExit()
		{
			Demo.CleanUpAndExit();
		}
	}
}