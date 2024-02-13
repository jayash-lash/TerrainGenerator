using System.Collections.Generic;
using System.IO;
using Chunk;
using Generator;
using UnityEngine;

namespace ProceduralGeneration.Scripts.Mesh
{
    public class BlockPositionSaver : MonoBehaviour
    {
        [SerializeField] private GameWorld _gameWorld;
        [SerializeField] private string _directorySavePath = "Assets/ProceduralGeneration/GeneratedMesh/";
        [SerializeField] private string _folderSavePath = "Position";

        [ContextMenu("Save")]
        public void SaveTopBlockPositionsToJson()
        {
            foreach (var chunkData in _gameWorld.ChunkDatas.Values)
            {
                Vector2Int chunkPosition = chunkData.ChunkPosition;
                List<BlockData> topBlocksInChunk = new List<BlockData>();

                for (int x = 0; x < ChunkRenderer.ChunkWidth; x++)
                {
                    for (int z = 0; z < ChunkRenderer.ChunkWidth; z++)
                    {
                        BlockData blockData = GetTopBlockDataInChunk(chunkData, x, z);
                        if (blockData != null)
                        {
                            topBlocksInChunk.Add(blockData);
                        }
                    }
                }

                SaveChunkToJson(chunkPosition, topBlocksInChunk);
            }
        }

        private void SaveChunkToJson(Vector2Int chunkPosition, List<BlockData> topBlocksInChunk)
        {
            var data = new ChunkDataJson
            {
                ChunkPosition = chunkPosition,
                TopBlockData = topBlocksInChunk
            };

            string json = JsonUtility.ToJson(data);
            
            string fileName = $"Chunk_{chunkPosition.x}_{chunkPosition.y}.json";
            string filePath = Path.Combine(_directorySavePath + _folderSavePath, fileName);

            File.WriteAllText(filePath, json);

            Debug.Log($"Saved {fileName}");
        }

        private BlockData GetTopBlockDataInChunk(ChunkData chunkData, int x, int z)
        {
            for (int y = ChunkRenderer.ChunkHeight - 1; y >= 0; y--)
            {
                int index = x + y * ChunkRenderer.ChunkWidthSq + z * ChunkRenderer.ChunkWidth;
                if (chunkData.Blocks[index] != BlockType.Air)
                {
                    Vector3Int position = new Vector3Int(x + chunkData.ChunkPosition.x * ChunkRenderer.ChunkWidth, y,
                        z + chunkData.ChunkPosition.y * ChunkRenderer.ChunkWidth);

                    BlockType blockType = chunkData.Blocks[index];

                    return new BlockData { Position = position, Type = blockType };
                }
            }

            return null;
        }

        [System.Serializable]
        private class ChunkDataJson
        {
            public Vector2Int ChunkPosition;
            public List<BlockData> TopBlockData;
        }

        [System.Serializable]
        private class BlockData
        {
            public Vector3Int Position;
            public BlockType Type;
        }
    }
}
