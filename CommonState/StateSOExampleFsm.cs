using UnityEngine;

public class BooFsm : BaseFsm
{
    [SerializeField] private FlyStateConfig _flyConfig;

    protected override void Fsm_IdleState(Fsm fsm, Fsm.Step step, Fsm.State state)
    {
        base.Fsm_IdleState(fsm, step, state);
        if (step == Fsm.Step.Enter)
        {
            // _anim.CrossFade(Anim_Idle, 0);
        }
        else if (step == Fsm.Step.Update)
        {
            //print($"{GetActionableStates().baseStates[0]}, {GetActionableStates().states[0]}");
            var actionStates = GetActionableStates();
            if (actionStates.states == null || actionStates.states.Count == 0 || actionStates.baseStates == null || actionStates.baseStates.Count == 0)
                return;

            int random = Random.Range(0, actionStates.states.Count);
            InitializeStateConfig(actionStates.baseStates[random]);
            
            var actionState = actionStates.states[random];
            fsm.TransitionTo(actionState);
        }
        else if (step == Fsm.Step.Exit)
        {

        }
    }
    protected override void Fsm_StatusEffectState(Fsm fsm, Fsm.Step step, Fsm.State state)
    {
        base.Fsm_StatusEffectState(fsm, step, state);
    }
    protected override void Fsm_DeadState(Fsm fsm, Fsm.Step step, Fsm.State state)
    {
        base.Fsm_DeadState(fsm, step, state);
    }

    protected override void InitializeStateConfig(BaseState baseState)
    {
        FlyState flyState = baseState as FlyState;
        flyState?.InitializedStateValue(_flyConfig);

        // 관련 Config 처리 추가 필요
    }
}
