using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameDataContainer", menuName = "Data/GameDataContainer")]
public class GameDataContainer : ScriptableObject
{
 public List<EnemyStat> EnemyStats = new();
 public List<PlayerStat> PlayerStats = new();
 public List<SkillData> SkillData = new();
 public List<StatLevelUp>  StatLevelUps = new();
}
