using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "Block/Side block")]
    public class BlockInfoSides : BlockInfo
    {
        
        public Vector2 PixelOffsetUp;
        public Vector2 PixelOffsetDown;

        public override Vector2 GetPixelOffset(Vector3Int normal)
        {
            if (normal == Vector3Int.up) return PixelOffsetUp;
            if (normal == Vector3Int.down) return PixelOffsetDown;

            return base.GetPixelOffset(normal);
        }
    }
}