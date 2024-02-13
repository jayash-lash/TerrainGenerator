using System;
using System.Collections.Generic;
using Generator;
using ProceduralGeneration.Scripts.SavingSystem;
using ScriptableObjects;
using UnityEngine;

namespace Chunk
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ChunkRenderer : MonoBehaviour
    {
        public const int ChunkWidth = 60;
        public const int ChunkWidthSq = ChunkWidth * ChunkWidth;
        public const int ChunkHeight = 90;
        public const float BlockScale = 1f;
        public const int UvResolution = 1024;

        public ChunkData ChunkData;
        public GameWorld ParentWorld { get; set; }
        [SerializeField] private BlockDataBase _blocksData;

        private UnityEngine.Mesh _chunkMesh;

        private ChunkData _leftChunk;
        private ChunkData _rightChunk;
        private ChunkData _forwardChunk;
        private ChunkData _backChunk;

        private List<Vector3> _vertices = new List<Vector3>(); //heigt
        private List<Vector2> _uvs = new List<Vector2>(); //Texture UV coordinates

        private static int[] _triangles;

        public static void InitTriangles()
        {
            _triangles = new int[65536 * 6 / 4];

            var vertexNum = 4;
            for (int i = 0; i < _triangles.Length; i += 6)
            {
                _triangles[i] = vertexNum - 4;
                _triangles[i + 1] = vertexNum - 3;
                _triangles[i + 2] = vertexNum - 2;

                _triangles[i + 3] = vertexNum - 3;
                _triangles[i + 4] = vertexNum - 1;
                _triangles[i + 5] = vertexNum - 2;
                vertexNum += 4;
            }
        }

        public void Init()
        {
            ParentWorld.ChunkDatas.TryGetValue(ChunkData.ChunkPosition + Vector2Int.left, out _leftChunk);
            ParentWorld.ChunkDatas.TryGetValue(ChunkData.ChunkPosition + Vector2Int.right, out _rightChunk);
            ParentWorld.ChunkDatas.TryGetValue(ChunkData.ChunkPosition + Vector2Int.up, out _forwardChunk);
            ParentWorld.ChunkDatas.TryGetValue(ChunkData.ChunkPosition + Vector2Int.down, out _backChunk);
            _chunkMesh = new UnityEngine.Mesh();

            RegenerateMesh();

            GetComponent<MeshFilter>().mesh = _chunkMesh;
        }


        public void SpawnBlock(Vector3Int blockPosition, BlockType blockType)
        {
            var index = blockPosition.x + blockPosition.y * ChunkWidthSq + blockPosition.z * ChunkWidth;
            ChunkData.Blocks[index] = blockType;
            RegenerateMesh();
        }

        public void DestroyBlock(Vector3Int blockPosition)
        {
            var index = blockPosition.x + blockPosition.y * ChunkWidthSq + blockPosition.z * ChunkWidth;
            ChunkData.Blocks[index] = BlockType.Air;
            RegenerateMesh();
        }

        private void RegenerateMesh()
        {
            ChunkSavingData chunkSavingData = new ChunkSavingData();
            _vertices.Clear();
            _uvs.Clear();

            int maxY = 0;
            for (var y = 0; y < ChunkHeight; y++)
            {
                for (var x = 0; x < ChunkWidth; x++)
                {
                    for (var z = 0; z < ChunkWidth; z++)
                    {
                        if (GenerateBlock(x, y, z, out ChunkBlockPosition chunkData))
                        {
                            if (maxY < y) maxY = y;
                            if (chunkData.T != BlockType.Air)
                            {
                                chunkSavingData.B.Add(chunkData);
                            }
                        }
                    }
                }
            }

            _chunkMesh.triangles = Array.Empty<int>();
            _chunkMesh.vertices = _vertices.ToArray();
            _chunkMesh.uv = _uvs.ToArray();
            _chunkMesh.SetTriangles(_triangles, 0, _vertices.Count * 6 / 4, 0, false);

            _chunkMesh.Optimize();

            _chunkMesh.RecalculateNormals();
            Vector3 boundsSize = new Vector3(ChunkWidth, maxY, ChunkWidth) * BlockScale;
            _chunkMesh.bounds = new Bounds(boundsSize / 2, boundsSize);

            var pos = gameObject.transform.position;
            chunkSavingData.C = new Vector2Int((int)pos.x, (int)pos.z);
            ChunksDataSaver.Singleton.AddData(chunkSavingData);
            GetComponent<MeshCollider>().sharedMesh = _chunkMesh;
        }

        private bool GenerateBlock(int x, int y, int z, out ChunkBlockPosition chunkData)
        {
            chunkData = default;
            var blockPosition = new Vector3Int(x, y, z);


            var blockType = GetBlockAtPosition(blockPosition);
            if (blockType == BlockType.Air) return false;


            if (GetBlockAtPosition(blockPosition + Vector3Int.right) == 0)
            {
                GenerateRightSide(blockPosition);
                AddUvs(blockType, Vector3Int.right);
            }

            if (GetBlockAtPosition(blockPosition + Vector3Int.left) == 0)
            {
                GenerateLeftSide(blockPosition);
                AddUvs(blockType, Vector3Int.left);
            }

            if (GetBlockAtPosition(blockPosition + Vector3Int.forward) == 0)
            {
                GenerateFrontSide(blockPosition);
                AddUvs(blockType, Vector3Int.forward);
            }

            if (GetBlockAtPosition(blockPosition + Vector3Int.back) == 0)
            {
                GenerateBackSide(blockPosition);
                AddUvs(blockType, Vector3Int.back);
            }

            if (GetBlockAtPosition(blockPosition + Vector3Int.up) == 0)
            {
                var data = new ChunkBlockPosition();
                data.P = new Vector2Int(blockPosition.x, blockPosition.z);
                data.T = blockType;
                chunkData = data;
                GenerateTopSide(blockPosition);
                AddUvs(blockType, Vector3Int.up);
            }

            if (blockPosition.y > 0 && GetBlockAtPosition(blockPosition + Vector3Int.down) == 0)
            {
                GenerateBottomSide(blockPosition);
                AddUvs(blockType, Vector3Int.down);
            }
            
            return true;
        }


        private BlockType GetBlockAtPosition(Vector3Int blockPosition)
        {
            if (blockPosition.x is >= 0 and < ChunkWidth && blockPosition.y is >= 0 and < ChunkHeight &&
                blockPosition.z is >= 0 and < ChunkWidth)
            {
                var index = blockPosition.x + blockPosition.y * ChunkWidthSq + blockPosition.z * ChunkWidth;
                return ChunkData.Blocks[index];
            }
            else
            {
                if (blockPosition.y < 0 || blockPosition.y >= ChunkHeight) return BlockType.Air;

                if (blockPosition.x < 0)
                {
                    if (_leftChunk == null)
                    {
                        return BlockType.Air;
                    }

                    blockPosition.x += ChunkWidth;
                    var index = blockPosition.x + blockPosition.y * ChunkWidthSq + blockPosition.z * ChunkWidth;
                    return _leftChunk.Blocks[index];
                }

                if (blockPosition.x >= ChunkWidth)
                {
                    if (_rightChunk == null)
                    {
                        return BlockType.Air;
                    }

                    blockPosition.x -= ChunkWidth;
                    var index = blockPosition.x + blockPosition.y * ChunkWidthSq + blockPosition.z * ChunkWidth;
                    return _rightChunk.Blocks[index];
                }

                if (blockPosition.z < 0)
                {
                    if (_backChunk == null)
                    {
                        return BlockType.Air;
                    }

                    blockPosition.z += ChunkWidth;
                    var index = blockPosition.x + blockPosition.y * ChunkWidthSq + blockPosition.z * ChunkWidth;
                    return _backChunk.Blocks[index];
                }

                if (blockPosition.z >= ChunkWidth)
                {
                    if (_forwardChunk == null)
                    {
                        return BlockType.Air;
                    }

                    blockPosition.z -= ChunkWidth;
                    var index = blockPosition.x + blockPosition.y * ChunkWidthSq + blockPosition.z * ChunkWidth;
                    return _forwardChunk.Blocks[index];
                }

                return BlockType.Air;
            }
        }

        private void GenerateRightSide(Vector3Int blockPosition)
        {
            _vertices.Add((new Vector3(1, 0, 0) + blockPosition) * BlockScale);
            _vertices.Add((new Vector3(1, 1, 0) + blockPosition) * BlockScale);
            _vertices.Add((new Vector3(1, 0, 1) + blockPosition) * BlockScale);
            _vertices.Add((new Vector3(1, 1, 1) + blockPosition) * BlockScale);
        }

        private void GenerateLeftSide(Vector3Int blockPosition)
        {
            _vertices.Add((new Vector3(0, 0, 0) + blockPosition) * BlockScale);
            _vertices.Add((new Vector3(0, 0, 1) + blockPosition) * BlockScale);
            _vertices.Add((new Vector3(0, 1, 0) + blockPosition) * BlockScale);
            _vertices.Add((new Vector3(0, 1, 1) + blockPosition) * BlockScale);
        }

        private void GenerateFrontSide(Vector3Int blockPosition)
        {
            _vertices.Add((new Vector3(0, 0, 1) + blockPosition) * BlockScale);
            _vertices.Add((new Vector3(1, 0, 1) + blockPosition) * BlockScale);
            _vertices.Add((new Vector3(0, 1, 1) + blockPosition) * BlockScale);
            _vertices.Add((new Vector3(1, 1, 1) + blockPosition) * BlockScale);
        }

        private void GenerateBackSide(Vector3Int blockPosition)
        {
            _vertices.Add((new Vector3(0, 0, 0) + blockPosition) * BlockScale);
            _vertices.Add((new Vector3(0, 1, 0) + blockPosition) * BlockScale);
            _vertices.Add((new Vector3(1, 0, 0) + blockPosition) * BlockScale);
            _vertices.Add((new Vector3(1, 1, 0) + blockPosition) * BlockScale);
        }

        private void GenerateTopSide(Vector3Int blockPosition)
        {
            _vertices.Add((new Vector3(0, 1, 0) + blockPosition) * BlockScale);
            _vertices.Add((new Vector3(0, 1, 1) + blockPosition) * BlockScale);
            _vertices.Add((new Vector3(1, 1, 0) + blockPosition) * BlockScale);
            _vertices.Add((new Vector3(1, 1, 1) + blockPosition) * BlockScale);
        }

        private void GenerateBottomSide(Vector3Int blockPosition)
        {
            _vertices.Add((new Vector3(0, 0, 0) + blockPosition) * BlockScale);
            _vertices.Add((new Vector3(1, 0, 0) + blockPosition) * BlockScale);
            _vertices.Add((new Vector3(0, 0, 1) + blockPosition) * BlockScale);
            _vertices.Add((new Vector3(1, 0, 1) + blockPosition) * BlockScale);
        }

        private void AddUvs(BlockType blockType, Vector3Int normal)
        {
            Vector2 uv;

            var info = _blocksData.GetInfo(blockType);

            if (info != null)
            {
                uv = info.GetPixelOffset(normal) / UvResolution;
            }
            else
            {
                uv = new Vector2(0, 0);
            }

            for (int i = 0; i < 4; i++)
            {
                _uvs.Add(uv);
            }
        }
    }
}