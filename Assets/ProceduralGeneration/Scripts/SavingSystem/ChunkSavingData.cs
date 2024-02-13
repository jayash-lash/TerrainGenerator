using System.Collections.Generic;
using UnityEngine;

namespace ProceduralGeneration.Scripts.SavingSystem
{
    [System.Serializable]
    public class ChunkSavingData
    {
        public Vector2Int C;
        public List<ChunkBlockPosition> B = new List<ChunkBlockPosition>();

        public ChunkBlockPosition GetBlock(Vector2Int pos)
        {
            foreach (var block in B)
            {
                if (block.P + C != pos) continue;
                return block;
            }
            return new ChunkBlockPosition();
        }
    }
}