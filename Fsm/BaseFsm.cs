using DefaultSetting;
using System.Collections.Generic;
using UnityEngine;
using static DefaultSetting.Define;

public struct FsmData
{
    private Fsm _fsm;
    private BaseFsm _baseFsm; // 추가: BaseFsm 레퍼런스
    private PlayerController _targetPlayer;
    private CharacterFlip _characterFlip;
    private BaseAnimationController _animatorController;

    public FsmData(Fsm fsm, BaseFsm baseFsm, PlayerController targetPlayer,
        CharacterFlip characterFlip, BaseAnimationController animator)
    {
        _fsm = fsm;
        _baseFsm = baseFsm;
        _targetPlayer = targetPlayer;
        _characterFlip = characterFlip;
        _animatorController = animator;
    }

    public PlayerController TargetPlayer => _targetPlayer;
    public CharacterFlip CharacterFlip => _characterFlip;
    public BaseAnimationController AnimatorController => _animatorController;
    public float EnterTime => _fsm.EnterTime;

    public BaseFsm BaseFsm => _baseFsm; // BaseFsm 접근자 추가
}

public class BaseFsm : MonoBehaviour, IUnitStat
{
    private bool _isInitialized = false;

    protected static readonly int _anim_Spawn = Animator.StringToHash("Spawn");
    protected static readonly int _anim_Idle = Animator.StringToHash("Idle");
    protected static readonly int _anim_StatusEffect = Animator.StringToHash("StatusEffect");
    protected static readonly int _anim_Dead = Animator.StringToHash("Dead");

    [SerializeField] private string _unitName;
    private Fsm _fsm;

    private Fsm.State _spawnState;
    private Fsm.State _idleState;
    private Fsm.State _statusEffectState;
    private Fsm.State _deadState;

    private List<BaseState> _activeStates = new();
    private BaseState[] _allStates;
    private BaseState _currentState;

    private Health _health;
    private float _lastHealthValue;

    protected PlayerController _playerController;
    protected CharacterFlip _characterFlip;
    protected BaseAnimationController _animatorCtrl;

    [SerializeField] private float _stunWaitTime;

    private bool _isBlockFlip;
    private Vector3 _initPos;

    private FsmData GetFsmData()
    {
        var data = new FsmData(_fsm, this, _playerController, _characterFlip, _animatorCtrl);
        return data;
    }

    private void Awake()
    {
        _playerController = FindAnyObjectByType<PlayerController>();
        _characterFlip = GetComponentInChildren<CharacterFlip>();
        _animatorCtrl = GetComponentInChildren<BaseAnimationController>();
        _allStates = GetComponentsInChildren<BaseState>(true);

        foreach (var item in _allStates)
        {
            item.StateEnabled += AddState;
            item.StateDisabled += RemoveState;
        }
    }

    private void Start()
    {
        InitHealthEvent();
        InitFlip();
        InitFsm();

        _fsm = new Fsm();
        _fsm.Start(_spawnState);

        _isInitialized = true;

        void InitHealthEvent()
        {
            _health = GetComponentInChildren<Health>();
            _lastHealthValue = _health.MaxHealth;

            _health.onHit += () => OnHitFeedback();
            _health.onRecovery += () => OnRecoveryFeedback();
            _health.onRecoveryFinish += () => _fsm.TransitionTo(_idleState);
            _health.onDeath += () => OnDeathFeedback();
        }
        void InitFlip()
        {
            var renderer = GetComponentInChildren<SpriteRenderer>();
            _characterFlip?.Init(renderer, _playerController.transform);
        }
    }

    private void Update()
    {
        _fsm.OnUpdate();
    }

    protected virtual void InitFsm()
    {
        _spawnState = Fsm_SpawnState;
        _idleState = Fsm_IdleState;
        _statusEffectState = Fsm_StatusEffectState;
        _deadState = Fsm_DeadState;

        foreach (var state in _activeStates)
        {
            state.CopyFsmData = GetFsmData;
            state.Initialized();
        }
    }

    protected virtual void InitializeStateConfig(BaseState baseState) { }

    #region IBossStat
    public float GetUnitMaxHP()
    {
        return _health.MaxHealth;
    }
    public float GetUnitCurrentHP()
    {
        return _health.CurrentHealth;
    }
    public string GetUnitName()
    {
        return _unitName;
    }
    #endregion

    #region State - Add, Remove, TransitionToState
    public void AddState(BaseState state)
    {
        if (state != null && _activeStates != null && _activeStates.Contains(state) == false)
        {
            _activeStates.Add(state);

            if (_isInitialized)
            {
                state.CopyFsmData = GetFsmData;
                state.Initialized();
            }
        }
    }
    public void RemoveState(BaseState state)
    {
        if (state != null && _activeStates != null && _activeStates.Contains(state) == true)
        {
            _activeStates.Remove(state);
            state.Disabled();
        }
    }
    public void TransitionToState(BaseFsmStateType state)
    {
        switch (state)
        {
            case BaseFsmStateType.Spawn:
                _fsm.TransitionTo(_spawnState);
                break;
            case BaseFsmStateType.Idle:
                _fsm.TransitionTo(_idleState);
                break;
            case BaseFsmStateType.StatusEffect:
                _fsm.TransitionTo(_statusEffectState);
                break;
        }
    }
    protected (List<BaseState> baseStates, List<Fsm.State> states) GetActionableStates()
    {
        var currentBaseState = new List<BaseState>();
        var actionableState = new List<Fsm.State>();
        foreach (var state in _activeStates)
        {
            if (state.IsActionabled())
            {
                currentBaseState.Add(state);
                actionableState.Add(state.Fsm_ActionState);
            }
        }
        return (currentBaseState, actionableState);
    }
    #endregion

    #region FSM_Status
    protected virtual void Fsm_SpawnState(Fsm fsm, Fsm.Step step, Fsm.State state) { }
    protected virtual void Fsm_IdleState(Fsm fsm, Fsm.Step step, Fsm.State state) { }
    protected virtual void Fsm_StatusEffectState(Fsm fsm, Fsm.Step step, Fsm.State state) { }
    protected virtual void Fsm_DeadState(Fsm fsm, Fsm.Step step, Fsm.State state) { }
    #endregion

    #region Feedbacks
    protected virtual void OnHitFeedback() { }
    protected virtual void OnRecoveryFeedback() { }
    protected virtual void OnDeathFeedback()
    {
        _fsm.TransitionTo(_deadState);
        Managers.Game.OnBossDie();
    }
    #endregion
}