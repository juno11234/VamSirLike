using System;
using UnityEngine;

public class CircleAttack : MonoBehaviour
{
    [Header("Weapon Stats")] [SerializeField]
    private float damage = 5f; // 한 번에 입힐 데미지

    [SerializeField] private float radius = 3f; // 타격 범위 (반지름)
    [SerializeField] private float cooldown = 1f; // 몇 초마다 타격할지 (틱)
    [SerializeField] private LayerMask enemyLayer; // 몬스터만 골라내기 위한 레이어 마스크

    [SerializeField] private ParticleSystem attackEffect;
    private float _timer;

    // 가비지(GC)를 절대 생성하지 않는 고정 배열 (최대 50마리 동시 타격)
    private Collider2D[] _results = new Collider2D[50];

    private void Start()
    {
        float diameter = (radius * 2f) + (radius * 0.5f);
        attackEffect.transform.localScale = new Vector3(diameter, diameter, 1f);
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        // 쿨타임이 찰 때마다 광역 타격 발동!
        if (_timer >= cooldown)
        {
            PulseAttack();
            attackEffect.Stop();
            attackEffect.Play();
            _timer = 0f;
        }
    }

    private void PulseAttack()
    {
        // 1. 최신식 '검색 필터'를 하나 만듭니다. (가비지 안 나옴, Struct 구조체)
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayer); // 적 레이어만 잡도록 세팅
        filter.useLayerMask = true; // 레이어 마스크 사용 활성화
        filter.useTriggers = true; // (선택) 적의 콜라이더가 isTrigger여도 잡아냄

        // 2. 이름은 그냥 OverlapCircle이지만, filter와 _results 배열을 넣으면 Zero GC(NonAlloc)로 작동합니다!
        int hitCount = Physics2D.OverlapCircle(transform.position, radius, filter, _results);

        // 3. 스캔된 몬스터 수(hitCount)만큼 반복문을 돌며 데미지를 입힙니다.
        for (int i = 0; i < hitCount; i++)
        {
            if (_results[i].TryGetComponent(out EnemyController enemy))
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}