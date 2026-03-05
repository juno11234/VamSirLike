using UnityEngine;

public abstract class InGameEvent
{
    public enum EventType
    {
        Unknown,
        Combat,
        Heal
    }

    public IFighter Sender { get; set; }
    public IFighter Receiver { get; set; }
    public abstract EventType Type { get; }
}

public class CombatEvent : InGameEvent
{
    public int Damage { get; set; }
    public Collider Collider { get; set; }

    public override EventType Type => EventType.Combat;
}

public class HealthEvent : InGameEvent
{
    public int HealAmount { get; set; }
    public override EventType Type => EventType.Heal;
}

public interface IFighter
{
    public void TakeDamage(CombatEvent combatEvent);
    public void Heal(HealthEvent healthEvent);
}