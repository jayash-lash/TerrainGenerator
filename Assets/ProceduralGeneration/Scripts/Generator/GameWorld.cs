using System.Collections.Generic;
using Chunk;
using ProceduralGeneration.Scripts.BlockTypeUI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Generator
{
    public class GameWorld : MonoBehaviour
    {
        public Dictionary<Vector2Int, ChunkData> ChunkDatas = new Dictionary<Vector2Int, ChunkData>(); // Cordinates of chunk

        [SerializeField] private ChunkRenderer _chunkPrefab;
        [SerializeField] private BlockPanelUI _blockPanelUI;
        [SerializeField] private TerrainGenerator _generator;
        [SerializeField] private Texture2D _mapTexture;
        [SerializeField] private Texture2D _biomeTexture2D;
        [SerializeField] private int _scaleFactor;
        [SerializeField] private bool _shouldGenerateWorld;
        private Camera _mainCamera;
        private Vector2Int _currentPlayerChunk;
        private float[,] _fixedMapData;
        private Color[,] _fixedBiomeData;

        private Vector2Int _worldSize;

        private void InitWorldSize()
        {
            var chunksCountPerLine = _fixedMapData.GetLength(0) / ChunkRenderer.ChunkWidth;
            _worldSize = new Vector2Int(chunksCountPerLine / 10, chunksCountPerLine / 10);
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            if(!_shouldGenerateWorld) return;
            ChunkRenderer.InitTriangles();
            _fixedMapData = Resize(_mapTexture, _mapTexture.width * _scaleFactor, _mapTexture.height * _scaleFactor);
            _fixedBiomeData = ResizeBiomeData(_biomeTexture2D, _biomeTexture2D.width * _scaleFactor, _biomeTexture2D.height * _scaleFactor);
            InitWorldSize();
            Generate();
        }

        private void Update()
        {
            CheckInput();
        }

        public float[,] Resize(Texture2D originalTexture, int newWidth, int newHeight)
        {
            var newData = new float[newWidth, newHeight];
            var ratioX = 1.0f / ((float)newWidth / (originalTexture.width - 1));
            var ratioY = 1.0f / ((float)newHeight / (originalTexture.height - 1));

            for (int i = 0; i < newHeight; i++)
            {
                for (int j = 0; j < newWidth; j++)
                {
                    float pixelX = j * ratioX;
                    float pixelY = i * ratioY;
                    newData[j, i] = originalTexture.GetPixel((int)pixelX, (int)pixelY).r;
                }
            }
            return newData;
        }

        public Color[,] ResizeBiomeData(Texture2D originalTexture, int newWidth, int newHeight)
        {
            var newData = new Color[newWidth, newHeight];
            var ratioX = 1.0f / ((float)newWidth / (originalTexture.width - 1));
            var ratioY = 1.0f / ((float)newHeight / (originalTexture.height - 1));

            for (int i = 0; i < newHeight; i++)
            {
                for (int j = 0; j < newWidth; j++)
                {
                    float pixelX = j * ratioX;
                    float pixelY = i * ratioY;
                    newData[j, i] = originalTexture.GetPixel((int)pixelX, (int)pixelY);
                }
            }
            return newData;
        }

        private void Generate()
        {
            // Create a common parent container for all chunks
            // GameObject chunksContainer = new GameObject("ChunksContainer");
            // chunksContainer.transform.parent = transform;
            // chunksContainer.AddComponent<MeshSavingIgnore>();

            for (var x = 0; x < _worldSize.x; x++)
            {
                for (var y = 0; y < _worldSize.y; y++)
                {
                    var chunkPosition = new Vector2Int(x, y);
                    LoadChunkAt(chunkPosition);
                }
            }

            for (var x = 0; x < _worldSize.x; x++)
            {
                for (var y = 0; y < _worldSize.y; y++)
                {
                    var chunkPosition = new Vector2Int(x, y);
                    var chunkData = ChunkDatas[chunkPosition];
                    SpawnChunkRender(chunkData, transform); // Pass the chunksContainer as the parent
                }
            }
        }


        private void SpawnChunkRender(ChunkData chunkData, Transform parent)
        {
            var xPos = chunkData.ChunkPosition.x * ChunkRenderer.ChunkWidth * ChunkRenderer.BlockScale;
            var zPos = chunkData.ChunkPosition.y * ChunkRenderer.ChunkWidth * ChunkRenderer.BlockScale;

            var chunk = Instantiate(_chunkPrefab, new Vector3(xPos, 0, zPos), Quaternion.identity);
            chunk.transform.SetParent(parent);
            chunk.ChunkData = chunkData;
            chunk.ParentWorld = this;
            chunk.Init();
            chunkData.Renderer = chunk;
        }


        private void LoadChunkAt(Vector2Int chunkPosition)
        {
            var chunkData = new ChunkData();
            chunkData.ChunkPosition = chunkPosition; // Cordinates of chunk

            // Update the call to match the new logic
            chunkData.Blocks = _generator.GenerateTerrain(chunkPosition.x, chunkPosition.y, _worldSize.x, _worldSize.y, _fixedMapData, _fixedBiomeData);

            ChunkDatas.Add(chunkPosition, chunkData); // Cordinates of chunk
        }

        private void CheckInput()
        {  
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            {
                var isDestroying = Input.GetMouseButtonUp(0);

                var ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
                if (Physics.Raycast(ray, out var hitInfo))
                {
                    Vector3 blockCenter;
                    if (isDestroying)
                    {
                        blockCenter = hitInfo.point - hitInfo.normal * ChunkRenderer.BlockScale / 2; // world coordinates
                    }
                    else
                    {
                        blockCenter =
                            hitInfo.point + hitInfo.normal * ChunkRenderer.BlockScale / 2; // world coordinates
                    }

                    var blockWorldPosition = Vector3Int.FloorToInt(blockCenter / ChunkRenderer.BlockScale);
                    var chunkPosition = GetChunkContainingBlock(blockWorldPosition);

                    if (ChunkDatas.TryGetValue(chunkPosition, out var chunkData))
                    {
                        var chunkOrigin = new Vector3Int(chunkPosition.x * ChunkRenderer.ChunkWidth, 0,
                            chunkPosition.y * ChunkRenderer.ChunkWidth);
                        if (isDestroying)
                        {
                            chunkData.Renderer.DestroyBlock(blockWorldPosition - chunkOrigin);
                        }
                        else
                        {
                            var selectedBlockInfo = _blockPanelUI.GetSelectedBlockType();
                            chunkData.Renderer.SpawnBlock(blockWorldPosition - chunkOrigin, selectedBlockInfo);
                        }
                    }
                }
            }
        }

        public Vector2Int GetChunkContainingBlock(Vector3Int blockWorldPosition)
        {
            var chunkPosition = new Vector2Int(blockWorldPosition.x / ChunkRenderer.ChunkWidth, blockWorldPosition.z / ChunkRenderer.ChunkWidth);

            if (blockWorldPosition.x < 0) chunkPosition.x--;
            if (blockWorldPosition.y < 0) chunkPosition.y--;

            return chunkPosition;
        }
    }
}
