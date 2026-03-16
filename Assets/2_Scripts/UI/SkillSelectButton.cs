using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SkillSelectButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private Button button;
    private int _skillId;
    private Action<int> _onClickCallback;
    private SkillBase _currentSkill;

    // UI 매니저가 이 버튼에 스킬 데이터를 꽂아줄 때 호출
    public void Init(SkillData data, SkillBase currentSkill, Action<int> onClick)
    {
        _skillId = data.id;
        _currentSkill = currentSkill;
        
        nameText.text = data.name; // 스킬 이름 
        descText.text = $"current Damage: {data.baseAtk}\n current Cooltime: {data.cooldown} second"; // 스킬 설명 조립

        _onClickCallback = onClick;

        // 기존 리스너 제거 후 새로 달기 (풀링 재사용 대비)
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClickButton);
    }

    private void OnClickButton()
    {
        _onClickCallback?.Invoke(_skillId);
    }
}