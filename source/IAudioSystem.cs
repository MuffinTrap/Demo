
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

		void SetAudio(Audio audioFile);
		void PlayAudio(Audio audioFile);
		void PauseAudio(Audio audioFile);
		void StopAudio(Audio audioFile);
		void SetAudioProgress(Audio audioFile, float progress);

		void CleanAndExit();
	}
}