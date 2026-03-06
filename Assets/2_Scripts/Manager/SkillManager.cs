using System.Collections.Generic;
using UnityEngine;

public interface ISkill
{
    void Init(CombatSystem combatSystem, SkillData skillData);
}

public class SkillManager : MonoBehaviour
{
    private CombatSystem _combatSystem;
    private IFighter _sender;
    private DataManager _dataManager;

    // 현재 보유 중인 무기 리스트
    private List<ISkill> _activeWeapons = new List<ISkill>();

    public void Init(CombatSystem combatSystem, IFighter sender, DataManager dataManager)
    {
        _combatSystem = combatSystem;
        _sender = sender;
        _dataManager = dataManager;

        // 시작할 때 자식으로 있는 모든 무기를 찾아서 초기화
        //Todo: 기본공격만 가지고 있고 나머지는 레벨업시 추가됨
        ISkill[] childWeapons = GetComponentsInChildren<ISkill>(true);

        foreach (var weapon in childWeapons)
        {
            // 임시로 무기 스크립트의 이름이나 타입을 보고 ID를 매칭하는 방식
            // (실제로는 무기 Prefab을 Addressables로 동적 생성하는 방식을 더 추천합니다)
            int skillId = GetSkillIdByType(weapon.GetType());
            SkillData data = _dataManager.GetSkillData(skillId);

            if (data != null)
            {
                weapon.Init(_combatSystem, data);
                _activeWeapons.Add(weapon);
            }
        }
    }

    public void AddWeapon(int skillId, GameObject weaponPrefab)
    {
        SkillData data = _dataManager.GetSkillData(skillId);
        GameObject weaponObj = Instantiate(weaponPrefab, transform); // 매니저의 자식으로 생성
        ISkill newWeapon = weaponObj.GetComponent<ISkill>();

        newWeapon.Init(_combatSystem, data);
        _activeWeapons.Add(newWeapon);
    }

    private int GetSkillIdByType(System.Type weaponType)
    {
        if (weaponType == typeof(CircleAttack)) return 4001;
        if (weaponType == typeof(ProjectileTargetScanner)) return 4002;
        return 0;
    }
}