using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
  public class BlockDataBase : ScriptableObject
  {
    [SerializeField] private BlockInfo[] _blocks;

    private Dictionary<BlockType, BlockInfo> _blockCached = new();

    private void OnEnable()
    {
      _blockCached.Clear();
    
      foreach (var blockInfo in _blocks)
      {
        _blockCached.Add(blockInfo.Type, blockInfo);
      }
    }

    public BlockInfo GetInfo(BlockType type)
    {
      if (_blockCached.TryGetValue(type, out var blockInfo))
      {
        return blockInfo;
      }

      return null;
    }
  }
}
