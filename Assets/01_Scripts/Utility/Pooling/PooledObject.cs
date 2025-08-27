using UnityEngine;

public class PooledObject : MonoBehaviour
{
    [HideInInspector] public bool inPool;  // true면 대기열 상태
}
