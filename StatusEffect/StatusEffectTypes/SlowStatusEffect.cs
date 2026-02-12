using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SlowStatusEffect", menuName = "Scriptable Object/StatusEffect/SlowStatusEffect", order = 0)]
public class SlowStatusEffect : StatusEffect
{
    [SerializeField] private float _decreaseSpeed;
    private PlayerMove _playerMove;
    public override void ApplyEffect(GameObject target)
    {
        _playerMove = target.GetComponent<PlayerMove>();
        _playerMove.SetDecelerationValue(_decreaseSpeed);
    }

    public override void RemoveEffect(GameObject target)
    {
        _playerMove.SetDecelerationValue(0);
    }
}
