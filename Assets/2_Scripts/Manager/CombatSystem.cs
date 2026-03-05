using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

public class CombatSystem : ITickable,IDisposable
{
    private const int Max_Event_Count = 1000;

    public class Callback
    {
        public Action<InGameEvent> OnCombatEvent;
        public Action<InGameEvent> OnHealEvent;
    }

    private readonly Dictionary<Collider2D, IFighter> _monstersDict = new Dictionary<Collider2D, IFighter>();
    private readonly Queue<InGameEvent> _eventQueue = new Queue<InGameEvent>();
    public readonly Callback EventCallback = new Callback();

    public void Tick()
    {
        // 큐에 있는 모든 이벤트를 해당 프레임에 지연 없이 즉시 처리합니다.
        while (_eventQueue.Count > 0)
        {
            // struct이므로 Dequeue할 때도 GC가 발생하지 않습니다.
            InGameEvent inGameEvent = _eventQueue.Dequeue();

            switch (inGameEvent.Type)
            {
                case InGameEvent.EventType.Combat:
                    inGameEvent.Receiver.TakeDamage(inGameEvent);
                    // 콜백 발생! (UI 업데이트나 이펙트 재생에 활용)
                    EventCallback.OnCombatEvent?.Invoke(inGameEvent);
                    break;
                    
                case InGameEvent.EventType.Heal:
                    inGameEvent.Receiver.Heal(inGameEvent);
                    EventCallback.OnHealEvent?.Invoke(inGameEvent);
                    break;
            }
        }
    }
    
    public void AddInGameEvent(InGameEvent e)
    {
        _eventQueue.Enqueue(e);
    }

    public void RegisterMonster(IFighter monster)
    {
        if (_monstersDict.TryAdd(monster.MainCollider, monster) == false)
        {
            Debug.LogWarning("몬스터가 이미 존재 덮어씀");
            _monstersDict[monster.MainCollider] = monster;
        }
    }

    public void RemoveMonster(IFighter monster)
    {
        _monstersDict.Remove(monster.MainCollider);
    }
    public IFighter GetMonster(Collider2D coll)
    {
        if (_monstersDict.TryGetValue(coll, out var monster))
        {
            return monster;
        }
        return null;
    }
    public void Dispose()
    {
        _monstersDict.Clear();
        _eventQueue.Clear();
    }
}