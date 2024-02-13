using UnityEngine;
using UnityEngine.Serialization;

namespace ProceduralGeneration.Scripts.SavingSystem
{
    [System.Serializable]
    public struct ChunkBlockPosition
    {
        public Vector2Int P;
        public BlockType T;
    }
}
