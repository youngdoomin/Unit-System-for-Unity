using System;
using System.Collections;
using UnityEngine;

public class BaseState : MonoBehaviour, IBaseState
{
    // delay
    [SerializeField] private float _coolTime;
    private float _currentCoolTime;

    public Action<BaseState> StateEnabled;
    public Action<BaseState> StateDisabled;

    public Func<FsmData> CopyFsmData;

    private void OnEnable()
    {
        StartCoroutine(StateEnabledTrigger());
    }
    private void OnDisable()
    {
        StartCoroutine(StateDisabledTrigger());
    }

    private IEnumerator StateEnabledTrigger()
    {
        yield return new WaitUntil(() => StateEnabled != null);
        StateEnabled.Invoke(this);
    }
    private IEnumerator StateDisabledTrigger()
    {
        yield return new WaitUntil(() => StateDisabled != null);
        StateDisabled.Invoke(this);
    }

    public virtual void Initialized() { }
    public virtual void Disabled() { }

    public virtual bool IsActionabled()
    {
        return (_coolTime <= _currentCoolTime);
    }

    public void Fsm_ActionState(Fsm fsm, Fsm.Step step, Fsm.State state)
    {
        if (step == Fsm.Step.Enter)
        {
            Fsm_Step_Enter();
        }
        else if (step == Fsm.Step.Update)
        {
            Fsm_Step_Update();
        }
        else if (step == Fsm.Step.Exit)
        {
            Fsm_Step_Exit();
        }
    }

    protected virtual void Fsm_Step_Enter() { }
    protected virtual void Fsm_Step_Update() { }
    protected virtual void Fsm_Step_Exit() { _currentCoolTime = 0; }

    protected virtual void Update()
    {
        if (_coolTime > _currentCoolTime)
            _currentCoolTime += Time.deltaTime;
    }
}