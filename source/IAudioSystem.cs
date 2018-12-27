
namespace MuffinSpace
{
	public struct Audio
	{
		public string debugName;
		public int id;
	}

	public interface IAudioSystem
	{
		bool Initialize();
		Audio LoadAudioFile(string filenameAndPath);
		void PlayAudioFile(Audio audioFile);
		void CleanAndExit();
	}
}