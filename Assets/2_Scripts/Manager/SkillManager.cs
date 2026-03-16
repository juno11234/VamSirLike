using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public interface ISkill
{
    int CurrentLevel { get; }
    void Init(CombatSystem combatSystem, IFighter sender, SkillData skillData);
    void LevelUp(SkillData skillData);
}

public class SkillManager : MonoBehaviour
{
    private CombatSystem _combatSystem;
    private IFighter _sender;
    private DataManager _dataManager;

    // 현재 보유 중인 무기 리스트
    private Dictionary<int, ISkill> _activeWeapons = new Dictionary<int, ISkill>();

    public void Init(CombatSystem combatSystem, IFighter sender, DataManager dataManager, int[] startingSkillIds)
    {
        _combatSystem = combatSystem;
        _sender = sender;
        _dataManager = dataManager;

        foreach (int skillId in startingSkillIds)
        {
            AddOrLevelUpWeaponAsync(skillId).Forget();
        }
    }

    // 2. 외부에서 매개변수로 Prefab을 넘겨주는 방식 대신, Addressable로 직접 로드!
    // (TSV 스킬 데이터에 "PrefabName" 같은 컬럼이 있다고 가정)
    public async UniTask AddOrLevelUpWeaponAsync(int skillId)
    {
        SkillData data = _dataManager.GetSkillData(skillId);
        if (data == null) return;

        // 이미 가지고 있는 무기라면 레벨업!
        if (_activeWeapons.TryGetValue(skillId, out ISkill existingWeapon))
        {
            existingWeapon.LevelUp(data); // 레벨업 로직 호출
            return;
        }

        // 새로운 무기라면 Addressable로 프리팹 비동기 로드
        // (주의: Addressables 주소는 게임 기획에 맞게 수정 필요)
        GameObject weaponPrefab = await Addressables.InstantiateAsync(data.prefabKey, transform).ToUniTask();

        ISkill newWeapon = weaponPrefab.GetComponent<ISkill>();
        newWeapon.Init(_combatSystem, _sender, data);

        _activeWeapons.Add(skillId, newWeapon);
    }
}