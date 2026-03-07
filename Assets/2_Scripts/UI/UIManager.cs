using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class UIManager : MonoBehaviour
{
    [Header("Player HP")] [SerializeField] private Slider hpSlider;

    [Header("Experience")] [SerializeField]
    private Slider expSlider;

    [SerializeField] private TextMeshProUGUI levelText;

    // VContainer가 게임 시작 시 알아서 Player와 ExpManager를 찾아 꽂아줍니다.
    [Inject]
    public void Init(PlayerController player, ExpManager expManager)
    {
        // 1. 플레이어 HP 이벤트 구독 및 초기 셋업
        player.OnHpChanged += UpdateHpUI;
        UpdateHpUI(player.CurrentHp, player.MaxHp);

        // 2. 경험치 매니저 이벤트 구독 및 초기 셋업

        expManager.OnExpChanged += UpdateExpUI;
        expManager.OnLevelUp += UpdateLevelUI;

        UpdateLevelUI(expManager.CurrentLevel);
        UpdateExpUI(expManager.CurrentExp, expManager.RequiredExp);
    }

    private void UpdateHpUI(float currentHp, float maxHp)
    {
        hpSlider.maxValue = maxHp;
        hpSlider.value = currentHp;
    }

    private void UpdateExpUI(float currentExp, float requiredExp)
    {
        expSlider.maxValue = requiredExp;
        expSlider.value = currentExp;
    }

    private void UpdateLevelUI(int level)
    {
        levelText.text = $"Lv. {level}";
    }

    // 오브젝트가 파괴될 때 이벤트 구독 해제 (메모리 누수 방지)
    private void OnDestroy()
    {
        // 실제 게임에서는 객체가 파괴될 시점을 고려해 -= 로 구독을 해제해 주는 것이 안전합니다.
    }
}