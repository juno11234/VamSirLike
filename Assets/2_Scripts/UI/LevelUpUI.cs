using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

public class LevelUpUI : MonoBehaviour
{
    [SerializeField] private GameObject panel; // 레벨업 UI 전체 패널 (평소엔 꺼둠)
    [SerializeField] private SkillSelectButton[] buttons; // 3개의 버튼 연결

    private SkillManager _skillManager;
    private DataManager _dataManager;
    private ExpManager _expManager;

    [Inject]
    public void Init(ExpManager expManager, PlayerController player, DataManager dataManager)
    {
        _dataManager = dataManager;
        // 플레이어 자식으로 있는 SkillManager를 찾아옵니다.
        _skillManager = player.GetComponentInChildren<SkillManager>();

        // 레벨업 이벤트 구독!
        _expManager = expManager;
        _expManager.OnLevelUp += ShowLevelUpUI;


        panel.SetActive(false);
    }

    private void ShowLevelUpUI(int newLevel)
    {
        // 1. 게임 일시 정지 (유니티의 모든 Update, 물리 연산, 애니메이션 정지)
        Time.timeScale = 0f;
        panel.SetActive(true);

        // 2. 전체 스킬 목록 가져와서 무작위로 섞기 (간단한 셔플)
        List<SkillData> allSkills = _dataManager.GetSkillList();
        for (int i = 0; i < allSkills.Count; i++)
        {
            int rand = Random.Range(i, allSkills.Count);
            (allSkills[i], allSkills[rand]) = (allSkills[rand], allSkills[i]);
        }

        // 3. 섞인 리스트의 앞에서 3개를 뽑아 버튼에 할당
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i < allSkills.Count)
            {
                // 버튼 클릭 시 OnSkillSelected 함수가 실행되도록 연결
                SkillBase currentSkill = _skillManager.GetCurrentSkillData(allSkills[i].id);
                buttons[i].Init(allSkills[i], currentSkill, OnSkillSelected);
                
                buttons[i].gameObject.SetActive(true);
            }
            else
            {
                buttons[i].gameObject.SetActive(false); // 남는 버튼 숨김
            }
        }
    }

    private void OnSkillSelected(int skillId)
    {
        // 1. UI 닫고 게임 재개
        panel.SetActive(false);
        Time.timeScale = 1f;

        // 2. 스킬 매니저에게 해당 스킬 추가/레벨업 명령! (비동기)
        _skillManager.AddOrLevelUpWeaponAsync(skillId).Forget();
    }

    private void OnDestroy()
    {
        _expManager.OnLevelUp -= ShowLevelUpUI;
    }
}