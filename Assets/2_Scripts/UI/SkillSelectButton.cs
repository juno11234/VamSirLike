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

    // UI 매니저가 이 버튼에 스킬 데이터를 꽂아줄 때 호출
    public void Init(SkillData data, Action<int> onClick)
    {
        _skillId = data.id;
        
        // (주의: data.skillName 이나 data.description 등은 실제 TSV/SkillData 필드명에 맞게 수정하세요!)
        nameText.text = data.name; // 스킬 이름 
        descText.text = $"damage: {data.baseAtk}\n cooltime: {data.cooldown} second"; // 스킬 설명 조립
        
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
