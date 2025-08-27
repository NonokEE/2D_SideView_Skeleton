using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public sealed class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }
    [System.Serializable] public class PoolEntry { public GameObject prefab; public int initialSize = 8; public bool expandable = true; }

    private readonly Dictionary<int, Queue<GameObject>> pools = new();
    private readonly Dictionary<GameObject, int> instanceToKey = new();
    private readonly Dictionary<int, int> totalCounts = new();
    private readonly Dictionary<int, string> keyToName = new();

    [SerializeField] private Transform root;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        if (root == null) { var go = new GameObject("_POOL_ROOT"); root = go.transform; DontDestroyOnLoad(go); }
        DontDestroyOnLoad(gameObject);
    }

    public void Prewarm(IEnumerable<PoolEntry> entries)
    {
        foreach (var e in entries) CreatePoolIfNeeded(e.prefab, e.initialSize, e.expandable);
    }

    public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null) return null;
        int key = prefab.GetInstanceID();
        if (!pools.TryGetValue(key, out var q) || q.Count == 0)
        {
            var go = Instantiate(prefab, pos, rot, root);
            RegisterInstance(go, key);
            return Activate(go, pos, rot);
        }
        var inst = q.Dequeue();
        return Activate(inst, pos, rot);
    }

    public bool Release(GameObject go)
    {
        if (go == null) return false;

        // 중복 Release 가드
        var tag = go.GetComponent<PooledObject>();
        if (tag != null && tag.inPool) return false;

        if (!instanceToKey.TryGetValue(go, out int key))
        {
            go.SetActive(false); // 폴백
            return false;
        }

        var p = go.GetComponent<IPoolable>();
        p?.OnDespawned();
        p?.ResetForPool();

        go.SetActive(false);
        go.transform.SetParent(root, false);

        if (tag != null) tag.inPool = true;
        pools[key].Enqueue(go);
        return true;
    }

    public bool TryRelease(GameObject go) => Release(go);

    private GameObject Activate(GameObject go, Vector3 pos, Quaternion rot)
    {
        go.transform.SetParent(null, true);
        go.transform.SetPositionAndRotation(pos, rot);

        // 풀 태그 해제
        var tag = go.GetComponent<PooledObject>();
        if (tag == null) tag = go.AddComponent<PooledObject>();
        tag.inPool = false;

        go.SetActive(true);
        var p = go.GetComponent<IPoolable>();
        p?.OnSpawned();
        return go;
    }

    private void RegisterInstance(GameObject go, int key)
    {
        if (!pools.ContainsKey(key)) pools[key] = new Queue<GameObject>();
        if (!instanceToKey.ContainsKey(go)) instanceToKey.Add(go, key);
        if (!totalCounts.ContainsKey(key)) totalCounts[key] = 0;
        totalCounts[key]++;
        if (!keyToName.ContainsKey(key)) keyToName[key] = go.name.Replace("(Clone)", "").Trim();
    }

    private void CreatePoolIfNeeded(GameObject prefab, int size, bool expandable)
    {
        if (prefab == null) return;
        int key = prefab.GetInstanceID();
        if (!pools.ContainsKey(key)) pools[key] = new Queue<GameObject>();
        for (int i = 0; i < size; i++)
        {
            var go = Instantiate(prefab, root);
            go.SetActive(false);
            if (go.GetComponent<PooledObject>() == null) go.AddComponent<PooledObject>().inPool = true;
            RegisterInstance(go, key);
            pools[key].Enqueue(go);
        }
    }

    // 통계 조회
    [System.Serializable]
    public struct PoolStats { public string prefabName; public int total; public int inactive; public int active; }
    public List<PoolStats> GetAllPoolStats()
    {
        var list = new List<PoolStats>();
        foreach (var kv in pools)
        {
            int key = kv.Key;
            int inactive = kv.Value.Count;
            int total = totalCounts.TryGetValue(key, out var t) ? t : instanceToKey.Count(v => v.Value == key);
            int active = Mathf.Max(0, total - inactive);
            string name = keyToName.TryGetValue(key, out var n) ? n : key.ToString();
            list.Add(new PoolStats { prefabName = name, total = total, inactive = inactive, active = active });
        }
        return list;
    }
}
