#if UNITY_EDITOR

using System.IO;
using ProceduralGeneration.Scripts.Mesh;
using ProceduralGeneration.Scripts.SavingSystem;
using UnityEditor;
using UnityEngine;

namespace Mesh
{
    public class MeshSaver : MonoBehaviour
    {
        [Header("Prefab Settings")] 
        public string _savingPrefName = "Terrain";
        [SerializeField] private string _directoryPath = "Assets/";
        [SerializeField] private string _folderName = "GeneratedMeshes/";

        [SerializeField] private Transform _targetMesh;

        private void CheckFolder(string path, string folderName)
        {
            if (AssetDatabase.IsValidFolder(path + folderName)) return;
            AssetDatabase.CreateFolder(path, folderName);
        }

        private void DeleteFilesInFolder(string path, string folderName)
        {
            string folderPath = path + folderName + "/";
            string[] files = Directory.GetFiles(folderPath);
            foreach (string file in files)
            {
                File.Delete(file);
            }
        }

        private void SaveMesh(UnityEngine.Mesh mesh, string path, string folderName, string fileName = "Mesh")
        {
            CheckFolder(path + "/", folderName);

            var savePath = path + "/" + folderName + "/" + fileName + ".asset";
            AssetDatabase.CreateAsset(mesh, savePath);
            AssetDatabase.SaveAssets();
            Debug.Log("Mesh saved as asset at: " + savePath);
        }

        [ContextMenu("Save")]
        private void SaveTerrain()
        {
            var savingPath = _directoryPath + _folderName;

            // Delete existing files in the folder
            DeleteFilesInFolder(_directoryPath, _folderName);

            for (int i = 0; i < _targetMesh.childCount; i++)
            {
                Transform child = _targetMesh.GetChild(i);
            
                var meshIgnorer = child.GetComponent<MeshSavingIgnore>();
                if (meshIgnorer != null)
                {
                    Destroy(meshIgnorer);
                    continue;
                }
            
                UnityEngine.Mesh meshToSave = Instantiate(child.GetComponent<MeshFilter>().sharedMesh);
                SaveMesh(meshToSave, savingPath, "Meshes", "Mesh" + i);
                child.GetComponent<MeshCollider>().sharedMesh = meshToSave;
                child.GetComponent<MeshFilter>().sharedMesh = meshToSave;
            }

            CheckFolder(_directoryPath, _folderName);
            string path = savingPath + "/" + _savingPrefName + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(_targetMesh.gameObject, path);
            Debug.Log("Prefab saved at: " + path);
            
            // BlockPositionSaver[] blockPositionSaver = FindObjectsOfType<BlockPositionSaver>();
            // if (blockPositionSaver != null)
            // {
            //     foreach (var blockPosition in blockPositionSaver)
            //     {
            //         blockPosition.SaveTopBlockPositionsToJson();
            //     }
            //     
            // }
            
            ChunksDataSaver.Singleton.SaveData();
        }
    }
}

#endif