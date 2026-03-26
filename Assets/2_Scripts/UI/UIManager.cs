using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class UIManager : MonoBehaviour
{
    [Header("Player HP")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] Vector3 hpBarOffset = new Vector3(0, -1f, 0);

    [Header("Experience")]
    [SerializeField]
    private Slider expSlider;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Play Time")]
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("GameOver UI")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private TextMeshProUGUI scoreText;

    private Transform _playerTransform;
    private Camera _mainCamera;
    private ExpManager _expManager;
    private float _playTime;
    private int _lastSeconds = -1; // 이전 프레임의 초(Second)를 기억할 변수
    
    // VContainer가 게임 시작 시 알아서 Player와 ExpManager를 찾아 꽂아줍니다.
    [Inject]
    public void Init(PlayerController player, ExpManager expManager)
    {
        _playerTransform = player.transform;
        _mainCamera = Camera.main;
        _expManager = expManager;
        
        // 1. 플레이어 HP 이벤트 구독 및 초기 셋업
        player.OnHpChanged += UpdateHpUI;
        player.OnDeath += GameOver;
        UpdateHpUI(player.CurrentHp, player.MaxHp);
        
        // 2. 경험치 매니저 이벤트 구독 및 초기 셋업
        _expManager.OnExpChanged += UpdateExpUI;
        _expManager.OnLevelUp += UpdateLevelUI;

        UpdateLevelUI(_expManager.CurrentLevel);
        UpdateExpUI(_expManager.CurrentExp, _expManager.RequiredExp);
        gameOverUI.SetActive(false);
    }

    private void Update()
    {
        _playTime += Time.deltaTime;
        int currentSeconds = Mathf.FloorToInt(_playTime % 60f);
        
        if (_lastSeconds != currentSeconds)
        {
            _lastSeconds = currentSeconds;
            int minutes = Mathf.FloorToInt(_playTime / 60f);
    
            // 핵심: .text에 문자열을 조합해서 넣지 않고, SetText 함수를 사용합니다.
            timeText.SetText("{0:00}:{1:00}", minutes, currentSeconds);
        }
    }

    private void LateUpdate()
    {
        // 플레이어의 월드 좌표(3D/2D)를 화면 픽셀 좌표(UI)로 변환
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(_playerTransform.position + hpBarOffset);

        // 체력 바 UI의 위치를 변경
        hpSlider.transform.position = screenPos;
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

    private void GameOver()
    {
        Time.timeScale = 0;
        int minutes = Mathf.FloorToInt(_playTime / 60f);
        int seconds = Mathf.FloorToInt(_playTime % 60f);
        gameOverUI.SetActive(true);
        scoreText.text = $"Score: {_expManager.Score}  Time: {minutes:00}:{seconds:00} ";
    }

    // 오브젝트가 파괴될 때 이벤트 구독 해제 (메모리 누수 방지)
    private void OnDestroy()
    {
        _expManager.OnExpChanged -= UpdateExpUI;
        _expManager.OnLevelUp -= UpdateLevelUI;
    }
}