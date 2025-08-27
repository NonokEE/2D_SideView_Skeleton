using UnityEngine;

[CreateAssetMenu(fileName = "PoolPreloadConfig", menuName = "Configs/Pooling/PreloadConfig")]
public class PoolPreloadConfig : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public GameObject prefab;
        [Min(0)] public int initialSize;
        public bool expandable;
    }

    public Entry[] entries;
}
