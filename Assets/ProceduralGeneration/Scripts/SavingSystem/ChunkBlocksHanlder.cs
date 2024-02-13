using UnityEngine;

namespace ProceduralGeneration.Scripts.SavingSystem
{
    public class ChunkBlocksHanlder : MonoBehaviour
    {
        private ChunkSavingData _chunkSavingData;
        public ChunkSavingData ChunkSavingData => _chunkSavingData;

        private void Start()
            => CatchData();

        private void CatchData()
        {
            var pos = gameObject.transform.position;
            _chunkSavingData =
                ChunksDataSaver.Singleton.GameWorldSavingData.GetChunkData(new Vector2Int((int)pos.x,
                    (int)pos.z));
        }
    }
}