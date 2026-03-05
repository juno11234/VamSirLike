using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

public class CombatSystem : ITickable
{
    private const int Max_Evenet_Count = 10;

    public class Callback
    {
        public Action<CombatEvent> OnCombatEvent;
        public Action<HealthEvent> OnHealEvent;
    }

    private Dictionary<Collider2D, IFighter> _monstersDict = new Dictionary<Collider2D, IFighter>();
    private Queue<InGameEvent> _eventQueue = new Queue<InGameEvent>();
    public readonly Callback EventCallback = new Callback();

    public void Tick()
    {
        int processCount = 0;

        while (_eventQueue.Count > 0 && processCount < Max_Evenet_Count)
        {
            var inGameEvent = _eventQueue.Dequeue();
            switch (inGameEvent.Type)
            {
                case InGameEvent.EventType.Combat:
                    var combatEvent = inGameEvent as CombatEvent;
                    inGameEvent.Receiver.TakeDamage(combatEvent);
                    break;
                case InGameEvent.EventType.Heal:
                    var healEvent = inGameEvent as HealthEvent;
                    inGameEvent.Receiver.Heal(healEvent);
                    break;
            }

            processCount++;
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
}