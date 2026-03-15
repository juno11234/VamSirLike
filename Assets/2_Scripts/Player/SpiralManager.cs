using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class SpiralManager : MonoBehaviour, ISkill
{
    [Header("Skill Settings")]
    [SerializeField] private SpiralProjectile projectilePrefab;
    [SerializeField] private float cooldown = 5f; // 발사 간격 (짧을수록 촘촘한 띠가 만들어짐)
    [SerializeField] private float angleSpeed = 5f; // 회전 속도 (높을수록 팽이처럼 빨리 돎)
    [SerializeField] private float expandSpeed = 2f; // 멀어지는 속도
    [SerializeField] private float fireDelay = 0.5f;

    private ObjectPool<SpiralProjectile> _pool;
    private Transform _projectileContainer;
    private CombatSystem _combatSystem;
    private IFighter _sender;
    private SkillData _skillData;

    private float _timer;
    private float _currentSpawnAngle; // 발사할 때마다 각도를 조금씩 틀어주기 위한 변수
    private float _damage;
    public int CurrentLevel { get; private set; }
    private int _projectileCount = 1;

    public void Init(CombatSystem combatSystem, IFighter sender, SkillData skillData)
    {
        _combatSystem = combatSystem;
        _sender = sender;
        _skillData = skillData;
        CurrentLevel = 1;
        _damage = _skillData.baseAtk;

        _projectileContainer = new GameObject("SpiralContainer").transform;

        _pool = new ObjectPool<SpiralProjectile>(
            createFunc: () => Instantiate(projectilePrefab, _projectileContainer),
            actionOnGet: (obj) => obj.gameObject.SetActive(true),
            actionOnRelease: (obj) => obj.gameObject.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj.gameObject),
            defaultCapacity: 10,
            maxSize: 20 
        );

        SpiralProjectile[] prefab = new SpiralProjectile[20];
        for (int i = 0; i < 20; i++)
        {
            prefab[i] = _pool.Get(); // 1. 강제로 20개를 생성해서 꺼냄
        }

        for (int i = 0; i < 20; i++)
        {
            _pool.Release(prefab[i]); // 2. 즉시 풀로 반납하여 비활성화 대기 상태로 만듦
        }

        _timer += cooldown;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= cooldown)
        {
            FireAsync().Forget();
            _timer -= cooldown;
        }
    }

    private async UniTaskVoid FireAsync()
    {
        for (int i = 0; i < _projectileCount; i++)
        {
            SpiralProjectile proj = _pool.Get();

            // IFighter를 MonoBehaviour로 형변환하여 플레이어의 Transform을 넘겨줍니다.
            Transform centerTransform = ((MonoBehaviour)_sender).transform;

            proj.Init(_combatSystem, _sender, centerTransform, _currentSpawnAngle, angleSpeed, expandSpeed,
                _damage, ReleaseProjectile);

            // 다음 번에 쏠 투사체의 시작 각도를 살짝 비틉니다 (모양이 예뻐짐)
            _currentSpawnAngle += 1.5f;
            
            if (i < _projectileCount - 1)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(fireDelay));
            }
        }
    }

    private void ReleaseProjectile(SpiralProjectile proj)
    {
        _pool.Release(proj);
    }

    public void LevelUp(SkillData skillData)
    {
        _damage += _skillData.atkPerLevel;
        CurrentLevel++;
        _projectileCount++;
    }
}