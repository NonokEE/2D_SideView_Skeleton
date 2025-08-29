using UnityEngine;

public class WeaponHudOnGUI : MonoBehaviour
{
    [SerializeField] private PlayerEntity player;
    [SerializeField] private Vector2 screenOffset = new Vector2(0f, -40f); // 스크린 픽셀 오프셋(머리 위)
    [SerializeField] private GUIStyle style;

    private string currentWeaponName = "";

    private void Awake()
    {
        if (player == null) player = GetComponentInParent<PlayerEntity>();
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                normal = { textColor = Color.white }
            };
        }
    }

    private void OnEnable()
    {
        var wm = player?.WeaponManager;
        if (wm != null)
        {
            wm.OnWeaponChanged += OnWeaponChanged;
            currentWeaponName = wm.Current != null ? wm.Current.weaponName : "";
        }
    }

    private void OnDisable()
    {
        var wm = player?.WeaponManager;
        if (wm != null) wm.OnWeaponChanged -= OnWeaponChanged;
    }

    private void OnWeaponChanged(BaseWeapon w)
    {
        currentWeaponName = w != null ? w.weaponName : "";
    }

    private void OnGUI()
    {
        if (string.IsNullOrEmpty(currentWeaponName)) return;
        if (Camera.main == null || player == null) return;

        Vector3 worldPos = player.transform.position + Vector3.up * 1.2f; // 머리 위 약간
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        if (screenPos.z < 0) return; // 카메라 뒤

        // GUI 좌표(좌상단 원점) 변환
        float x = screenPos.x + screenOffset.x;
        float y = Screen.height - screenPos.y + screenOffset.y;

        GUI.Label(new Rect(x - 50f, y - 10f, 100f, 20f), currentWeaponName, style);
    }
}
