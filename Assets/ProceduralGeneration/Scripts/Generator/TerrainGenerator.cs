using System.Collections.Generic;
using Chunk;
using Mesh;
using ProceduralGeneration.Scripts.SavingSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Generator
{
    public class TerrainGenerator : MonoBehaviour
    {
        #region Serializable Classes

        [System.Serializable]
        public class Spawnable
        {
            public GameObject Prefab;
            public float SpawnChance;
        }

        [System.Serializable]
        public class Biome
        {
            public Color Color;
            public BlockType MainBlockType;
            public BlockType SecondaryBlockType;
            public BlockType AdditionalBlockType;

            [Range(0, 100)] public int AdditionalBlockHeightRange = 0;
            public Spawnable[] SpawnablePrefabs;
        }

        #endregion


        [SerializeField] private List<Biome> biomes;
        [SerializeField] private float _objectOffset = 0.5f;
        private Dictionary<Spawnable, GameObject> _prefabContainers = new Dictionary<Spawnable, GameObject>();

        #region Terrain Generation

        public BlockType[] GenerateTerrain(int chunkX, int chunkZ, int totalChunksX, int totalChunksZ,
            float[,] noiseData, Color[,] biomeData)
        {
            var result = new BlockType[ChunkRenderer.ChunkWidth * ChunkRenderer.ChunkHeight * ChunkRenderer.ChunkWidth];

            int noiseDataWidth = noiseData.GetLength(0);
            int noiseDataHeight = noiseData.GetLength(1);

            for (int x = 0; x < ChunkRenderer.ChunkWidth; x++)
            {
                for (int z = 0; z < ChunkRenderer.ChunkWidth; z++)
                {
                    float noiseX =
                        ((chunkX * ChunkRenderer.ChunkWidth + x) / (float)(totalChunksX * ChunkRenderer.ChunkWidth)) *
                        noiseDataWidth;
                    float noiseZ =
                        ((chunkZ * ChunkRenderer.ChunkWidth + z) / (float)(totalChunksZ * ChunkRenderer.ChunkWidth)) *
                        noiseDataHeight;

                    float height = GenerateHeightFromNoise(noiseX, noiseZ, noiseDataWidth, noiseDataHeight, noiseData);
                    Color biomeColor = biomeData[Mathf.FloorToInt(noiseX), Mathf.FloorToInt(noiseZ)];

                    Biome currentBiome = GetBiomeByColor(biomeColor);
                    for (int y = 0; y < ChunkRenderer.ChunkHeight; y++)
                    {
                        var index = x + y * ChunkRenderer.ChunkWidthSq + z * ChunkRenderer.ChunkWidth;

                        if (y == Mathf.FloorToInt(height))
                        {
                            // Top block
                            var blockType = GetBlockTypeByBiome(biomeColor, true);
                            result[index] = blockType;
                        }
                        else
                        {
                            // Check for AdditionalBlockType within the specified height range
                            if (currentBiome != null &&
                                y <= Mathf.FloorToInt(height - currentBiome.AdditionalBlockHeightRange))
                            {
                                result[index] = currentBiome.AdditionalBlockType;
                            }
                            else if (y < height)
                            {
                                // Below AdditionalBlockType range but above the terrain surface
                                result[index] = GetBlockTypeByBiome(biomeColor, false);
                            }
                            else
                            {
                                // Above the terrain surface
                                result[index] = BlockType.Air;
                            }
                        }

                        if (y == Mathf.FloorToInt(height))
                        {
                            Vector3 spawnPosition = new Vector3(
                                (chunkX * ChunkRenderer.ChunkWidth + x) * ChunkRenderer.BlockScale,
                                (y + 1) * ChunkRenderer.BlockScale, // Adjusted to spawn on the upper surface
                                (chunkZ * ChunkRenderer.ChunkWidth + z) * ChunkRenderer.BlockScale
                            );

                            // Apply the constant offset to the spawn position
                            spawnPosition += new Vector3(_objectOffset, 0, _objectOffset);

                            if (currentBiome != null)
                            {
                                // Determine the spawnable object based on chances
                                var spawnable = GetRandomSpawnable(currentBiome.SpawnablePrefabs);

                                // Spawn the selected prefab on the adjusted position
                                SpawnPrefabInBiome(spawnPosition, spawnable, transform);
                            }
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region Biome Layer Logic

        private BlockType CheckForBiomeLayer(int y, Biome currentBiome)
        {
            return BlockType.Air;
        }

        #endregion

        #region Object Spawning

        private void SpawnPrefabInBiome(Vector3 position, Spawnable spawnable, Transform chunkTransform)
        {
            if (spawnable != null && spawnable.Prefab != null)
            {
                // Check if a container for this spawnable type already exists
                if (!_prefabContainers.ContainsKey(spawnable))
                {
                    // If not, create a new container and add it to the dictionary
                    GameObject container = new GameObject($"{spawnable.Prefab.name}Container");
                    container.transform.parent = chunkTransform;
                    container.AddComponent<MeshSavingIgnore>();
                    _prefabContainers.Add(spawnable, container);
                }

                // Get the container for this spawnable type
                GameObject parentObject = _prefabContainers[spawnable];

                // Instantiate the prefab as a child of the container
                var spawnedObject = Instantiate(spawnable.Prefab, position, Quaternion.identity);
                spawnedObject.transform.parent = parentObject.transform;
            }
        }

        #endregion

        #region Helper Methods

        private Spawnable GetRandomSpawnable(Spawnable[] spawnables)
        {
            float totalChance = 0;

            // Calculate the total chance
            foreach (Spawnable spawnable in spawnables)
            {
                totalChance += spawnable.SpawnChance;
            }

            // If totalChance is zero, no valid spawnables, return null
            if (totalChance == 0)
            {
                return null;
            }

            // Generate a random value within the total chance range
            float randomValue = Random.Range(0f, totalChance);

            // Determine the spawnable object based on chances
            foreach (Spawnable spawnable in spawnables)
            {
                if (randomValue < spawnable.SpawnChance)
                {
                    return spawnable;
                }

                randomValue -= spawnable.SpawnChance;
            }

            // If no spawnable object is determined, return null
            return null;
        }

        private Biome GetBiomeByColor(Color biomeColor)
        {
            var fixedColor = GetFixedColor(biomeColor);

            foreach (Biome biome in biomes)
            {
                if (biome.Color.Equals(fixedColor))
                {
                    return biome;
                }
            }

            return null;
        }

        private float GenerateHeightFromNoise(float noiseX, float noiseZ, int noiseDataWidth, int noiseDataHeight,
            float[,] noiseData)
        {
            float normalizedX = noiseX / noiseDataWidth;
            float normalizedZ = noiseZ / noiseDataHeight;

            int pixelX = Mathf.FloorToInt(normalizedX * (noiseDataWidth - 1));
            int pixelY = Mathf.FloorToInt(normalizedZ * (noiseDataHeight - 1));

            float height = noiseData[pixelX, pixelY];
            height *= ChunkRenderer.ChunkHeight - 1; // Normalize the height
            return height;
        }

        private float RoundToThreeDecimalPlaces(float value)
        {
            float roundedValue = Mathf.Round(value * 10f) / 10f;
            return roundedValue;
        }

        private Color GetFixedColor(Color inputColor)
        {
            Color res = new Color();
            res.r = RoundToThreeDecimalPlaces(inputColor.r);
            res.b = RoundToThreeDecimalPlaces(inputColor.b);
            res.g = RoundToThreeDecimalPlaces(inputColor.g);
            return res;
        }

        private BlockType GetBlockTypeByBiome(Color biomeColor, bool isTopBlock)
        {
            var fixedColor = GetFixedColor(biomeColor);

            foreach (Biome biome in biomes)
            {
                if (biome.Color.Equals(fixedColor))
                {
                    return isTopBlock ? biome.MainBlockType : biome.SecondaryBlockType;
                }
            }

            return BlockType.Stone; // Default block type
        }

        #endregion
    }
}