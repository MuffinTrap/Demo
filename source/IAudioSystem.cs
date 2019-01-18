
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

		void PlayAudio(Audio audioFile);
		void StopAudio(Audio audioFile);
		void RestartAudio(Audio audioFile);

		void CleanAndExit();
	}

	public class DummyAudioSystem : IAudioSystem
	{
		public bool Initialize() { return true; }
		public Audio LoadAudioFile(string filenameAndPath) { return new Audio(); }

		public void PlayAudio(Audio audioFile) { }
		public void StopAudio(Audio audioFile) { }
		public void RestartAudio(Audio audioFile) { }

		public void CleanAndExit() { }
	}
}