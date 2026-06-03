using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CurrentData
{
    public int level;
    public float damage;
    public float cooldown;
    public bool isProjectile;

    public CurrentData(int level, float damage, float cooldown, bool isProjectile)
    {
        this.level = level;
        this.damage = damage;
        this.cooldown = cooldown;
        this.isProjectile = isProjectile;
    }
}

public abstract class SkillBase : MonoBehaviour
{
    [SerializeField] protected LayerMask enemyLayer;

    protected CombatSystem CombatSystem;
    protected IFighter Sender;
    protected SkillData SkillData;

    protected int CurrentLevel;
    protected float Damage;
    protected float Cooldown;

    public virtual void Init(CombatSystem combatSystem, IFighter sender, SkillData skillData)
    {
        CombatSystem = combatSystem;
        Sender = sender;
        SkillData = skillData;

        CurrentLevel = 1;
        Damage = SkillData.baseAtk;
        Cooldown = SkillData.cooldown;
    }

    public virtual void LevelUp(SkillData skillData)
    {
        if (CurrentLevel >= SkillData.maxLevel) return;
        Damage += SkillData.atkPerLevel;
        Cooldown = Mathf.Max(0.1f, Cooldown - 0.5f);
        CurrentLevel++;
    }

    public CurrentData GetCurrentData()
    {
        bool isProjectile = SkillData.enhanceType == EnhanceType.Projectile;

        return new CurrentData(CurrentLevel, Damage, Cooldown, isProjectile);
    }
}

public class SkillManager : MonoBehaviour
{
    private CombatSystem _combatSystem;
    private IFighter _sender;
    private DataManager _dataManager;

    // 현재 보유 중인 무기 리스트
    private Dictionary<int, SkillBase> _activeWeapons = new Dictionary<int, SkillBase>();

    public void Init(CombatSystem combatSystem, IFighter sender, DataManager dataManager, int[] startingSkillIds)
    {
        _combatSystem = combatSystem;
        _sender = sender;
        _dataManager = dataManager;

        foreach (int skillId in startingSkillIds)
        {
            StartCoroutine(AddOrLevelUpWeaponCoroutine(skillId));
        }
    }

    public void AddOrLevelUpWeapon(int skillId)
    {
        StartCoroutine(AddOrLevelUpWeaponCoroutine(skillId));
    }

    private IEnumerator AddOrLevelUpWeaponCoroutine(int skillId)
    {
        SkillData data = _dataManager.GetSkillData(skillId);
        if (data == null) yield break;

        // 이미 가지고 있는 무기라면 레벨업
        if (_activeWeapons.TryGetValue(skillId, out SkillBase existingWeapon))
        {
            existingWeapon.LevelUp(data);
            yield break;
        }

        // 새로운 무기라면 Addressable로 프리팹 비동기 로드
        AsyncOperationHandle<GameObject> handle =
            Addressables.LoadAssetAsync<GameObject>(data.prefabKey);
        yield return handle;

        // Init 전에 Update가 실행되지 않도록 inactive로 생성
        GameObject prefab = handle.Result;
        prefab.SetActive(false);
        GameObject instance = Instantiate(prefab, transform);
        prefab.SetActive(true);

        SkillBase newWeapon = instance.GetComponent<SkillBase>();
        newWeapon.Init(_combatSystem, _sender, data);
        instance.SetActive(true);
        _activeWeapons.Add(skillId, newWeapon);
    }

    public SkillBase GetCurrentSkillData(int skillId)
    {
        if (_activeWeapons.TryGetValue(skillId, out SkillBase skill))
        {
            return skill;
        }

        return null;
    }

    private void OnDestroy()
    {
        foreach (KeyValuePair<int, SkillBase> weapon in _activeWeapons)
        {
            Destroy(weapon.Value.gameObject);
        }
    }
}