public interface IPoolable
{
    void OnSpawned();    // SetActive(true) 직후
    void OnDespawned();  // SetActive(false) 직전
    void ResetForPool(); // 풀 대기로 들어가기 전에 내부 상태 원복
}
