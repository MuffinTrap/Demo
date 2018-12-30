namespace MuffinSpace
{
	public class DemoWrapper
	{
		public BunnyDemo Demo { get; set; }

		public void Create()
		{
			Demo = new BunnyDemo();
		}

		public DemoSettings GetDemoSettings()
		{
			return Demo.GetDemoSettings();
		}
	}
}