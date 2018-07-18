
namespace OpenTkConsole
{
	public struct Audio
	{
		public string debugName;
		public int id;
	}

	interface IAudioSystem
	{
		bool Initialize();
		Audio LoadAudioFile(string filenameAndPath);
		void PlayAudioFile(Audio audioFile);
		void Shutdown();
	}
}