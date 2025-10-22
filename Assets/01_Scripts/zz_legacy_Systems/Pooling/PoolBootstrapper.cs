using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class PoolBootstrapper : MonoBehaviour
{
    [SerializeField] private PoolPreloadConfig preloadConfig;
    [SerializeField] private PoolManager.PoolEntry[] preload; // 인스펙터 직접 지정 분

    private void Awake()
    {
        if (PoolManager.Instance == null)
        {
            var pm = new GameObject("PoolManager").AddComponent<PoolManager>();
            DontDestroyOnLoad(pm.gameObject);
        }

        // Config + 인스펙터 병합
        var list = new System.Collections.Generic.List<PoolManager.PoolEntry>();
        if (preloadConfig != null && preloadConfig.entries != null)
        {
            foreach (var e in preloadConfig.entries)
                list.Add(new PoolManager.PoolEntry { prefab = e.prefab, initialSize = e.initialSize, expandable = e.expandable });
        }
        if (preload != null)
            list.AddRange(preload);

        if (list.Count > 0)
            PoolManager.Instance.Prewarm(list);
    }
}
