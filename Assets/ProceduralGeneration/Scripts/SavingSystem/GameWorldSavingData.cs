using System.Collections.Generic;
using UnityEngine;

namespace ProceduralGeneration.Scripts.SavingSystem
{
    [System.Serializable]
    public class GameWorldSavingData
    {
        public List<ChunkSavingData> D = new List<ChunkSavingData>();

        public ChunkSavingData GetChunkData(Vector2Int chunkPosition)
        {
            foreach (var chunkData in D)
            {
                if(chunkData.C.x == chunkPosition.x && chunkData.C.y == chunkPosition.y)
                    return chunkData;
            }
            return null;
        }
    }
}
