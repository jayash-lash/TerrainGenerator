using UnityEngine;

namespace ScriptableObjects
{
  [CreateAssetMenu(menuName = "Block/Normal block")]
  public class BlockInfo : ScriptableObject
  {
    public BlockType Type;
    public Vector2 PixelOffset;
    public  AudioClip AudioClip;

    public virtual Vector2 GetPixelOffset(Vector3Int normal)
    {
      return PixelOffset;
    }
  }
}
