using System.IO;
using UnityEngine;

namespace ProceduralGeneration.Scripts.SavingSystem
{
    public class ChunksDataSaver : MonoBehaviour
    {
        public static ChunksDataSaver Singleton { get; set; }

        private GameWorldSavingData _gameWorldSavingData = new GameWorldSavingData();
        public GameWorldSavingData GameWorldSavingData => _gameWorldSavingData;
        private string _fileName = "ChunksData";
        private string _path;
        
        private void Awake()
        {
            Singleton = this;
            InitPath();
            TryLoadData();
        }

        private void InitPath()
        {
            _path = "Assets/ProceduralGeneration/GeneratedMesh/" + _fileName + ".json";
        }

        public void AddData(ChunkSavingData addingData)
        {
            for (int i = 0; i < _gameWorldSavingData.D.Count; i++)
            {
                if( _gameWorldSavingData.D[i].C != addingData.C) continue;
                _gameWorldSavingData.D[i] = addingData;
            }
            _gameWorldSavingData.D.Add(addingData);
        }

        private void TryLoadData()
        {
            if (!File.Exists(_path)) return;
            _gameWorldSavingData = JsonUtility.FromJson<GameWorldSavingData>(File.ReadAllText(_path));
        }
        
        public void SaveData()
        {
            string jsonData = JsonUtility.ToJson(_gameWorldSavingData);
            File.WriteAllText(_path, jsonData);
        }
    }
}