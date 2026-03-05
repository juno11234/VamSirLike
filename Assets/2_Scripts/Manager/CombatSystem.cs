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

    private Dictionary<Collider, IFighter> _monstersDict = new Dictionary<Collider, IFighter>();
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
}