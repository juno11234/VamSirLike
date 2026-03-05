using UnityEngine;

public struct InGameEvent
{
    public enum EventType
    {
        Unknown,
        Combat,
        Heal
    }

    public EventType Type;
    public IFighter Sender;
    public IFighter Receiver;
    
    // 데미지나 회복량 등 수치를 담는 범용 변수
    public float Amount; 
}

public interface IFighter
{
    public Collider2D MainCollider { get; }
    public void TakeDamage(InGameEvent combatEvent);
    public void Heal(InGameEvent healthEvent);
}