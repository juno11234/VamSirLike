using System;
using UnityEngine;


// 적 - 공격 타입
public enum AttackType 
{
    Melee,
    Ranged,
    Boss
}

// 플레이어 - 스킬 강화 타입
public enum EnhanceType
{
    Range,
    Projectile
} 

// 플레이어 - 직업 종류
public enum JobType 
{
    Warrior,
    Mage
}

// 플레이어 - 스탯 종류
public enum StatType
{
    HP,
    ATK,
    Speed,
    Cooldown
}
// 1001~ 적 스탯 구조체
[Serializable]
public struct EnemyStat
{
    public int id;
    public string name;
    public float hp;
    public float atk;
    public float speed;
    public AttackType attackType;
}

// 3001~ 플레이어 스탯 구조체
[Serializable]
public struct PlayerStat
{
    public int id;
    public string name;
    public float baseHp;
    public float baseAtk;
    public float baseSpeed;
    public float baseCooldown;
}

// 4001~ 스킬 데이터 구조체
[Serializable]
public struct SkillData
{
    public int id;
    public string name;
    public JobType job;
    public EnhanceType enhanceType;
    public float baseAtk;
    public float cooldown;
    public int maxLevel;
    public float atkPerLevel; // 레벨업당 공격력 증가량
    public float enhancePerLevel; // 레벨업당 강화수치(범위/개수) 증가량
}

// 5001~ 스탯 레벨업 구조체
[Serializable]
public struct StatLevelUp
{
    public int id;
    public string name;
    public float increaseStatPer;
    public int minLevel;
    public int maxLevel;
    public StatType statType;
}