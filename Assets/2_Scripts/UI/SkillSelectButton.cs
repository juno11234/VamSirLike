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
        if (currentSkill == null)
        {
            descText.text = $"Level 1\n " + "\n" +
                            $"Damage: {data.baseAtk}\n " + "\n" +
                            $"CoolTime: {data.cooldown} second";
        }
        else
        {
            CurrentData currentData = currentSkill.GetCurrentData();
            descText.text =
                $"currentLevel {currentData.level}\n" +
                $"nextLevel {currentData.level + 1}\n" + "\n" +
                $"currentDamage: {currentData.damage}\n" +
                $"nextDamage: {currentData.damage + data.atkPerLevel}\n" + "\n" +
                $"currentCoolTime: {currentData.cooldown} sec\n" +
                $"nextCoolTime: {currentData.cooldown - 0.5f} sec";
        }


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