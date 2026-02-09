using System;
using UnityEngine;

public class FlyState : BaseState
{
    [Header("Movement")]
    /// the speed of the object (relative to the level's speed)
    [Tooltip("the speed of the object (relative to the level's speed)")]
    [SerializeField] protected StateValue _stateValue;
    /// the current direction of the object
    [Tooltip("the current direction of the object")]
    protected Vector3 _direction;
    protected Vector3 _movement;

    [Serializable]
    public class StateValue
    {
        public float speedValue;
        public float accelerationValue;
    }

    public void InitializedStateValue(FlyStateConfig flyStateConfig)
    {
        _stateValue = flyStateConfig.Generate();
    }

    protected override void Fsm_Step_Enter() 
    {
        _direction = (Vector2)CopyFsmData().TargetPlayer.transform.position - (Vector2)transform.position;
        _direction.Normalize();
    }
    protected override void Fsm_Step_Update() 
    {
        Movement();
    }

    public override bool IsActionabled()
    {
        return true;
    }

    public virtual void Movement()
    {
        print($"{gameObject} Speed {_stateValue.speedValue} {_direction} {_stateValue.accelerationValue * Time.deltaTime}");
        
        if (_stateValue.speedValue <= 0)
            return;

        _direction = (Vector2)CopyFsmData().TargetPlayer.transform.position - (Vector2)transform.position;
        _direction.Normalize();

        _movement = _direction * (_stateValue.speedValue) * Time.deltaTime;
        transform.Translate(_movement, Space.World);

        _stateValue.speedValue += _stateValue.accelerationValue * Time.deltaTime;
    }
}
