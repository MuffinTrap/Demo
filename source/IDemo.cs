
namespace MuffinSpace
{
	// Demo interface
	interface IDemo
	{
		void Load(AssetManager assetManager, SyncSystem syncSystem);
		void Sync(SyncSystem syncer);
		void Draw(Renderer renderer);
	}

}