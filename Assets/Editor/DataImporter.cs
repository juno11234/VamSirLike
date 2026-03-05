using System;
using UnityEngine;
using UnityEditor;

public class DataImporter : EditorWindow
{
    // 연결할 에셋들
    private GameDataContainer targetContainer;
    private TextAsset enemyTSV;
    private TextAsset playerTSV;
    private TextAsset skillTSV;
    private TextAsset statLevelUpsTSV;

    // 유니티 상단 메뉴에 버튼 생성
    [MenuItem("VamSir Tools/Data Importer (TSV)")]
    public static void ShowWindow()
    {
        // 커스텀 창 띄우기
        GetWindow<DataImporter>("TSV 임포터");
    }

    // 에디터 창의 UI를 그리는 함수
    private void OnGUI()
    {
        GUILayout.Label("데이터 임포트 설정", EditorStyles.boldLabel);

        // 드래그 앤 드롭으로 에셋을 연결할 수 있는 필드 생성
        targetContainer = (GameDataContainer)EditorGUILayout.ObjectField("Target Container (SO)", targetContainer, typeof(GameDataContainer), false);
        enemyTSV = (TextAsset)EditorGUILayout.ObjectField("적 스탯 TSV", enemyTSV, typeof(TextAsset), false);
        playerTSV = (TextAsset)EditorGUILayout.ObjectField("플레이어 스탯 TSV", playerTSV, typeof(TextAsset), false);
        skillTSV = (TextAsset)EditorGUILayout.ObjectField("스킬 데이터 TSV", skillTSV, typeof(TextAsset), false);
        statLevelUpsTSV = (TextAsset)EditorGUILayout.ObjectField("레벨업 데이터 TSV", statLevelUpsTSV, typeof(TextAsset), false);

        GUILayout.Space(20);

        // 임포트 버튼
        if (GUILayout.Button("TSV 데이터 덮어쓰기", GUILayout.Height(40)))
        {
            if (targetContainer == null)
            {
                EditorUtility.DisplayDialog("오류", "Target Container(SO)를 연결해주세요!", "확인");
                return;
            }

            // GUI 이벤트 도중 충돌을 피하기 위해, 다음 프레임에 임포트 실행
            EditorApplication.delayCall += () => 
            {
                // 3. 데이터를 수정하기 전에 "나 이거 수정할 거니까 기록해 둬!" 라고 유니티에 알림 (안전성 확보)
                Undo.RecordObject(targetContainer, "Import TSV Data");
        
                // 실제 임포트 로직 실행
                ImportAllData();
            };
        }
    }

    // 실제 파싱 및 데이터 저장 로직
    private void ImportAllData()
    {
        // 1. 적 데이터 파싱
        if (enemyTSV != null)
        {
            targetContainer.EnemyStats.Clear(); // 기존 데이터 초기화
            string[] lines = enemyTSV.text.Split('\n');
            
            for (int i = 2; i < lines.Length; i++) // [시니어 포인트] 0, 1번째 줄은 헤더이므로 index 2부터 시작
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue; // 빈 줄 무시
                
                string[] cols = lines[i].TrimEnd('\r').Split('\t'); // TSV이므로 탭(\t)으로 분리

                EnemyStat stat = new EnemyStat
                {
                    id = int.Parse(cols[0]),
                    name = cols[1],
                    hp = float.Parse(cols[2]),
                    atk = float.Parse(cols[3]),
                    speed = float.Parse(cols[4]),
                    attackType = (AttackType)Enum.Parse(typeof(AttackType), cols[5], true),
                };
                targetContainer.EnemyStats.Add(stat);
            }
            Debug.Log($"적 데이터 {targetContainer.EnemyStats.Count}개 임포트 완료!");
        }

        // 2. 플레이어 데이터 파싱
        if (playerTSV != null)
        {
            targetContainer.PlayerStats.Clear();
            string[] lines = playerTSV.text.Split('\n');

            for (int i = 2; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                string[] cols = lines[i].TrimEnd('\r').Split('\t');

                PlayerStat stat = new PlayerStat
                {
                    id = int.Parse(cols[0]),
                    name = cols[1],
                    baseHp = float.Parse(cols[2]),
                    baseAtk = float.Parse(cols[3]),
                    baseSpeed = float.Parse(cols[4]),
                    baseCooldown = float.Parse(cols[5])
                };
                targetContainer.PlayerStats.Add(stat);
            }
            Debug.Log($"플레이어 데이터 {targetContainer.PlayerStats.Count}개 임포트 완료!");
        }
        // 3. 스킬 데이터 파싱
        if (skillTSV != null)
        {
            targetContainer.SkillData.Clear();
            string[] lines = skillTSV.text.Split('\n');
            
            for (int i = 2; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                string[] cols = lines[i].TrimEnd('\r').Split('\t');

                SkillData skill = new SkillData
                {
                    id = int.Parse(cols[0]),
                    name = cols[1],
                    job = (JobType)Enum.Parse(typeof(JobType), cols[2], true),
                    enhanceType = (EnhanceType)Enum.Parse(typeof(EnhanceType), cols[3], true),
                    baseAtk = float.Parse(cols[4]),
                    cooldown = float.Parse(cols[5]),
                    maxLevel = int.Parse(cols[6]),
                    atkPerLevel = float.Parse(cols[8]), 
                    enhancePerLevel = float.Parse(cols[9])
                };
                targetContainer.SkillData.Add(skill);
            }
            Debug.Log($"스킬 데이터 {targetContainer.SkillData.Count}개 임포트 완료!");
        }
        // 4. 레벨업 데이터 파싱
        if (statLevelUpsTSV != null)
        {
            targetContainer.StatLevelUps.Clear();
            string[] lines = statLevelUpsTSV.text.Split('\n');

            for (int i = 2; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                string[] cols = lines[i].TrimEnd('\r').Split('\t');

                StatLevelUp levelUp = new StatLevelUp
                {
                    id = int.Parse(cols[0]),
                    name = cols[1],
                    increaseStatPer = float.Parse(cols[2]),
                    minLevel = int.Parse(cols[3]),
                    maxLevel = int.Parse(cols[4]),
                    statType = (StatType)Enum.Parse(typeof(StatType), cols[5], true),
                };
                targetContainer.StatLevelUps.Add(levelUp);
            }
            Debug.Log($"레벨업 데이터 {targetContainer.StatLevelUps.Count}개 임포트 완료!");
        }

        
        EditorUtility.SetDirty(targetContainer);
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("완료", "데이터 임포트가 성공적으로 끝났습니다.", "확인");
    }
}
