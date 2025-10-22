using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform weaponSocket;                     // Player 하위 소켓(없으면 자동 생성)
    [SerializeField] private List<GameObject> weaponPrefabs = new();
    [SerializeField] private int defaultIndex = 0;

    public System.Action<BaseWeapon> OnWeaponChanged;

    private readonly List<BaseWeapon> instances = new();
    private BaseEntity owner;
    private int currentIndex = -1;
    private bool isSwitching;

    public BaseWeapon Current => (currentIndex >= 0 && currentIndex < instances.Count) ? instances[currentIndex] : null;
    public int Count => instances.Count;

    public void Initialize(BaseEntity weaponOwner)
    {
        owner = weaponOwner;

        // 소켓 자동 생성
        if (weaponSocket == null)
        {
            var socketGO = new GameObject("WeaponSocket");
            socketGO.transform.SetParent(transform);
            socketGO.transform.localPosition = Vector3.zero;
            socketGO.transform.localRotation = Quaternion.identity;
            socketGO.transform.localScale = Vector3.one;
            weaponSocket = socketGO.transform;
        }

        // 프리인스턴스
        instances.Clear();
        for (int i = 0; i < weaponPrefabs.Count; i++)
        {
            var prefab = weaponPrefabs[i];
            if (prefab == null) continue;

            var go = Instantiate(prefab, weaponSocket);
            go.name = prefab.name; // (Clone) 제거
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.SetActive(false);

            if (go.TryGetComponent<BaseWeapon>(out var w))
            {
                instances.Add(w);
            }
            else
            {
                Debug.LogError($"WeaponManager: {prefab.name}에 BaseWeapon이 없습니다.");
                Destroy(go);
            }
        }

        // 기본 장착
        if (instances.Count > 0)
        {
            int idx = Mathf.Clamp(defaultIndex, 0, instances.Count - 1);
            SelectByIndex(idx);
        }
        else
        {
            Debug.LogWarning("WeaponManager: 등록된 무기가 없습니다.");
        }
    }

    public void SelectByIndex(int index)
    {
        if (isSwitching) return;
        if (instances.Count == 0) return;
        if (index < 0 || index >= instances.Count) return;
        if (index == currentIndex) return;

        isSwitching = true;

        // 이전 비장착
        if (currentIndex >= 0)
        {
            var oldW = instances[currentIndex];
            if (oldW != null)
            {
                try { oldW.OnUnequip(); } catch { /* no-op */ }
                oldW.gameObject.SetActive(false);
            }
        }

        // 신규 장착
        currentIndex = index;
        var newW = instances[currentIndex];
        newW.gameObject.SetActive(true);
        newW.OnEquip(owner);

        OnWeaponChanged?.Invoke(newW);
        isSwitching = false;
    }

    public void Next()
    {
        if (instances.Count <= 1) return;
        int next = (currentIndex + 1 + instances.Count) % instances.Count;
        SelectByIndex(next);
    }

    public void Prev()
    {
        if (instances.Count <= 1) return;
        int prev = (currentIndex - 1 + instances.Count) % instances.Count;
        SelectByIndex(prev);
    }

    // 인스펙터 편의 메서드
    public void SetWeaponSocket(Transform socket) => weaponSocket = socket;
    public void SetWeaponPrefabs(List<GameObject> list) { weaponPrefabs = list; }
    public Transform GetWeaponSocket() => weaponSocket;
    public IReadOnlyList<BaseWeapon> GetInstances() => instances;
}
