using DefaultSetting;
using DefaultSetting.Utility;
using DG.Tweening;
using SerializedTuples;
using SerializedTuples.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TobotFsm : MonoBehaviour, IUnitStat
{
    #region Fsm
    private Fsm _fsm;

    private Fsm.State _idleState;
    private Fsm.State _laserState;
    private Fsm.State _missileState;
    private Fsm.State _recoveryState;
    private Fsm.State _stunState;
    private Fsm.State _deadState;
    private Fsm.State _testTransitionState;
    #endregion

    private Health _health;
    [SerializeField] private DamageablePart[] _damageableParts;
    [SerializeField] private RecoveryPhase _recoveryTriggerPhase;
    [SerializeField] private float _recoveryCoolTime;
    [SerializeField] private float _recoveryHealthValue;
    private float _currentRecoveryValue = 0;
    private float _lastHealthValue;
    private int _previousActivePartIndex;

    private PlayerController _playerController;
    [SerializeField] private Health[] _totems;
    //[SerializeField] private Animator _recoveryTotemAnimator;
    //[SerializeField] private Animator _shieldTotemAnimator;
    (List<int> indexes, List<Health> totems) _respawnedTotems = new();
    private LineConnectEffect[] _lineConnectEffects;
    private Light2D[] _ringLightEffects;
    private MoveObjEffect[] _moveObjEffects;
    private GameObjectActiveEvent[] _gameObjectActiveEvents;
    [SerializeField] private float _stunWaitTime;
    [SerializeField] private float _stunShake;
    private float _stateEnterTime = 0;
    private bool _isBlockFlip;
    private Vector3 _initPos;

    private enum CurrentTotemState { None, Recovery, Shield };
    private CurrentTotemState _currentTotemState;

    #region Laser State Variable
    [Header("Laser")]
    [SerializeField] private LaserAttack _laserAttack;
    [SerializeField] private float _beforeLaserShootDelay;
    [SerializeField] private float _laserWaitTime = 3f;
    #endregion

    #region Missile State Varible
    [Header("Missile")]
    [SerializedTupleLabels("MinTime", "MaxTime")]
    [SerializeField] private SerializedTuple<float, float> _projectSpawnDelay;
    [SerializeField] private HomingProjectile _projectile;
    [SerializeField] private int _projectileCount = 1;
    [SerializeField] private float _beforeMissileShootDelay = 5f;
    [SerializeField] private float _missileWaitTime = 5f;
    private HomingProjectile[] _projectilePool;

    [SerializeField] private Animator missileWarningEffect;
    [SerializeField] private float missileWarningEffectTime = 2f;

    #endregion

    #region Animation
    [Header("Animation")]
    [SerializeField] private Animator _tobotAnim;
    [SerializeField] private Animator _recoveryAnim;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private bool _isSpriteShowRight;
    #endregion

    #region Animation State
    private static readonly int _anim_Spawn = Animator.StringToHash("Idle");
    private static readonly int _anim_Idle = Animator.StringToHash("Idle");
    private static readonly int _anim_StartRolling = Animator.StringToHash("StartRolling");
    private static readonly int _anim_StopRolling = Animator.StringToHash("StopRolling");
    private static readonly int _anim_Rolling = Animator.StringToHash("Rolling");
    private static readonly int _anim_ShootReady = Animator.StringToHash("ShootReady");
    private static readonly int _anim_Shoot = Animator.StringToHash("Shoot");
    private static readonly int _anim_Hide = Animator.StringToHash("Hide");
    private static readonly int _anim_Dead = Animator.StringToHash("Dead");
    #endregion

    #region Sfx
    private AudioClip _recoverySfx;
    private AudioClip _damageableHitSfx;
    private AudioClip _damageableActiveSfx;
    private AudioClip _totemsActiveSfx;
    private AudioClip _totemsDisactiveSfx;


    #endregion

    #region Temp
    [Header("Temp")]
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Light2D laserLeftEyeLight;
    [SerializeField] private Light2D laserRightEyeLight;
    [SerializeField] private float laserProductTime;
    #endregion

    private Coroutine _runningCoroutine;

    [Serializable]
    private struct RecoveryPhase
    {
        [SerializedTupleLabels("isTriggered", "percent")]
        [SerializeField] private SerializedTuple<bool, float>[] _recoveryTriggers;
        public SerializedTuple<bool, float>[] RecoveryTriggers => _recoveryTriggers;

        public bool IsActiveTrigger(float currentPercent)
        {
            print("percent " + currentPercent);
            foreach (var trigger in _recoveryTriggers)
            {
                if (!trigger.v1 && trigger.v2 >= currentPercent)
                {
                    trigger.v1 = true;
                    return true;
                }
            }

            return false;
        }

    }

    private void Awake()
    {
        _playerController = FindAnyObjectByType<PlayerController>();
        _initPos = _tobotAnim.transform.position;
        _respawnedTotems.Item1 = new();
        _respawnedTotems.Item2 = new();

        laserLeftEyeLight.enabled = false;
        laserRightEyeLight.enabled = false;
    }

    void Start()
    {
        InitProjectilePool();
        InitHealthEvent();
        InitTotems();
        InitDamageableParts();
        InitFsm();

        _fsm = new Fsm();
        _fsm.Start(_idleState);

        void InitProjectilePool()
        {
            int poolCount = 20;
            _projectilePool = new HomingProjectile[poolCount];
            GameObject missileList = new GameObject("Missile List");
            for (int i = 0; i < _projectilePool.Length; i++)
            {
                HomingProjectile projectile = Instantiate(_projectile);
                projectile.transform.SetParent(missileList.transform);
                projectile.OnCreate(_playerController.transform);
                projectile.gameObject.SetActive(false);
                _projectilePool[i] = projectile;
            }
        }
        void InitHealthEvent()
        {
            _health = GetComponentInChildren<Health>();
            _lastHealthValue = _health.MaxHealth;

            _health.onHit += () => OnHitFeedback();
            _health.onRecovery += () => OnRecoveryFeedback();
            _health.onRecoveryFinish += () => _fsm.TransitionTo(_idleState);
            _health.onDeath += () => OnDeathFeedback();
        }
        void InitDamageableParts()
        {
            Debug.Assert(_damageableParts.Length > 0, "TobotFsm/_damageableParts Length 0");

            StartCoroutine(CallbackCoroutine(Time.deltaTime, () => ActiveRandomPart()));
            foreach (var damageablePart in _damageableParts)
                damageablePart.onHit += () => ActiveRandomPart();


            void ActiveRandomPart()
            {
                int idx = GetRandomExcept(0, _damageableParts.Length - 1, ref _previousActivePartIndex);
                for (int i = 0; i < _damageableParts.Length; i++)
                    _damageableParts[i].SetInvincible(i != idx);
            }
        }
        void InitFsm()
        {
            _idleState = Fsm_IdleState;
            _laserState = FSM_LaserState;
            _missileState = Fsm_MissileState;
            _recoveryState = Fsm_RecoveryState;
            _stunState = Fsm_StunState;
            _deadState = FSM_DeadState;
        }


    }

    private void InitTotems()
    {
        Debug.Assert(_totems.Length > 0, "TobotFSM/_totems Length 0");

        _lineConnectEffects = new LineConnectEffect[_totems.Length];
        _ringLightEffects = new Light2D[_totems.Length];
        _moveObjEffects = new MoveObjEffect[_totems.Length];
        _gameObjectActiveEvents = new GameObjectActiveEvent[_totems.Length];


        for (int i = 0; i < _totems.Length; i++)
        {
            int idx = i;
            _lineConnectEffects[idx] = _totems[idx].GetComponent<LineConnectEffect>();
            _ringLightEffects[idx] = _totems[idx].GetComponentInChildren<Light2D>();
            _moveObjEffects[idx] = _totems[idx].GetComponent<MoveObjEffect>();
            _gameObjectActiveEvents[idx] = _totems[idx].GetComponent<GameObjectActiveEvent>();

            _ringLightEffects[idx].enabled = false;
            _moveObjEffects[idx]?.SetDuration(_recoveryCoolTime);

            _totems[idx].onSpawn += () =>
            {
                _ringLightEffects[idx].enabled = true;

                if (_currentTotemState.Equals(CurrentTotemState.Recovery))
                {
                    _moveObjEffects[idx]?.ShowObj(true);
                    _lineConnectEffects[idx]?.ShowLine(true);

                }

            };

            _totems[idx].onDeath += () =>
            {
                _ringLightEffects[idx].enabled = false;

                if (_currentTotemState.Equals(CurrentTotemState.Recovery))
                {
                    if (_respawnedTotems.totems.Count > 0 && _respawnedTotems.totems.All(x => x.IsDead()))
                        _fsm.TransitionTo(_stunState);
                    SetCurrentRecoveryValue();
                }
                else if (_currentTotemState.Equals(CurrentTotemState.Shield))
                {
                    StartCoroutine(_gameObjectActiveEvents[idx].TriggerEvent(true));
                }
                _lineConnectEffects[idx]?.ShowLine(false);
                _moveObjEffects[idx]?.ShowObj(false);
            };
        }

        _health.onRecovery += () =>
        {
            for (int i = 0; i < _respawnedTotems.totems.Count; i++)
            {
                _moveObjEffects[_respawnedTotems.indexes[i]]?.TriggerMove();
            }
        };
    }

    private void RespawnTotems(int respawnTotemValue)
    {
        Debug.Assert(_totems.Length > 0, "TobotFSM/_totems Length 0");
        _respawnedTotems.indexes.Clear();
        _respawnedTotems.totems.Clear();

        for (int i = 0; i < respawnTotemValue; i++)
        {
            int idx = GetRandomExcept(0, _totems.Length, _respawnedTotems.indexes.ToArray());
            _respawnedTotems.indexes.Add(idx);
            _respawnedTotems.totems.Add(_totems[idx]);
            _totems[idx].Spawn();
        }
    }

    private void KillAllTotems()
    {
        for (int i = 0; i < _respawnedTotems.totems.Count; i++)
        {
            int idx = i;
            _respawnedTotems.totems[i].TriggerDeath();
        }
    }

    private void HideShields()
    {
        for (int i = 0; i < _respawnedTotems.totems.Count; i++)
        {
            int idx = i;
            StopCoroutine(_gameObjectActiveEvents[idx].TriggerEvent(false));
        }
    }

    void Update()
    {
        if (!_isBlockFlip)
        {
            bool isTargetRight = transform.position.x < _playerController.transform.position.x;
            _renderer.flipX = _isSpriteShowRight ? !isTargetRight : isTargetRight;

            if (_renderer.flipX) //왼쪽
            {
                laserRightEyeLight.transform.localPosition = new Vector3(-0.022f, -0.032f);
                laserLeftEyeLight.transform.localPosition = new Vector3(-0.106f, -0.032f);
            }
            else
            {
                laserRightEyeLight.transform.localPosition = new Vector3(0.114f, -0.032f);
                laserLeftEyeLight.transform.localPosition = new Vector3(0.017f, -0.032f);
            }

        }
        _fsm.OnUpdate();
    }

    #region Fsm Functions
    void Fsm_IdleState(Fsm fsm, Fsm.Step step, Fsm.State state)
    {
        if (step == Fsm.Step.Enter)
        {
            _stateEnterTime = 0;
            _tobotAnim.CrossFade(_anim_Idle, 0);

        }
        else if (step == Fsm.Step.Update)
        {
            _stateEnterTime += Time.deltaTime;
            if (_stateEnterTime > 0.5f)
            {

                var randomState = GetRandomState(new Fsm.State[] { _laserState, _missileState });
                _fsm.TransitionTo(randomState);
            }

        }
    }

    void FSM_LaserState(Fsm fsm, Fsm.Step step, Fsm.State state)
    {
        if (step == Fsm.Step.Enter)
        {
            _stateEnterTime = 0;
            _tobotAnim.CrossFade(_anim_ShootReady, 0);
            _currentTotemState = CurrentTotemState.Shield;
            RespawnTotems(3);
            //ChangeTotemsAnimator(_shieldTotemAnimator);
            StartCoroutineOrganize(_laserAttack.Shoot(_playerController.transform, _beforeLaserShootDelay, () => _tobotAnim.CrossFade(_anim_Shoot, 0)));

            DOTween.To(() => laserLeftEyeLight.color, x => laserLeftEyeLight.color = x, Color.red, laserProductTime);
            DOTween.To(() => laserRightEyeLight.color, x => laserRightEyeLight.color = x, Color.red, laserProductTime);
            DOTween.To(() => globalLight.color, x => globalLight.color = x, new Color(40 / 255f, 40 / 255f, 40 / 255f), laserProductTime);

        }
        else if (step == Fsm.Step.Update)
        {
            _stateEnterTime += Time.deltaTime;
            if (_stateEnterTime >= _laserWaitTime + _beforeLaserShootDelay)
                _fsm.TransitionTo(_idleState);
            /*if (_stateEnterTime >= _beforeLaserShootDelay)
                HideShields();*/
        }
        else if (step == Fsm.Step.Exit)
        {
            _laserAttack.StopAllCoroutines();
            _currentTotemState = CurrentTotemState.None;
            KillAllTotems();
            HideShields(); // +

            DOTween.To(() => laserLeftEyeLight.color, x => laserLeftEyeLight.color = x, Color.red.GetChangeAlpha(0), laserProductTime);
            DOTween.To(() => laserRightEyeLight.color, x => laserRightEyeLight.color = x, Color.red.GetChangeAlpha(0), laserProductTime);
            DOTween.To(() => globalLight.color, x => globalLight.color = x, Color.white, laserProductTime);
        }


    }

    void Fsm_MissileState(Fsm fsm, Fsm.Step step, Fsm.State state)
    {
        if (step == Fsm.Step.Enter)
        {
            _stateEnterTime = 0;
            Animator warningEffect = Instantiate(missileWarningEffect, transform);
            Destroy(warningEffect.gameObject, missileWarningEffectTime);
            warningEffect.transform.localPosition += Vector3.up * 0.8f;

            StartCoroutineOrganize(SpawnProjectiles());
            //_fsm.TransitionTo(_idleState);
        }
        else if (step == Fsm.Step.Update)
        {
            _stateEnterTime += Time.deltaTime;
            if (_stateEnterTime > _missileWaitTime + _beforeMissileShootDelay)
                _fsm.TransitionTo(_idleState);

        }

        IEnumerator SpawnProjectiles()
        {
            yield return new WaitForSeconds(_beforeMissileShootDelay);
            int spawnCount = 0;
            for (int i = 0; i < _projectilePool.Length; i++)
            {
                HomingProjectile projectile = _projectilePool[i];

                if (projectile.gameObject.activeInHierarchy)
                    continue;

                if (spawnCount == _projectileCount)
                    break;

                float delayTime = UnityEngine.Random.Range(_projectSpawnDelay.v1, _projectSpawnDelay.v2);
                yield return new WaitForSeconds(delayTime);
                projectile.gameObject.SetActive(true);
                projectile.Spawn(transform.position);
                spawnCount++;
            }

        }

    }

    void Fsm_RecoveryState(Fsm fsm, Fsm.Step step, Fsm.State state)
    {
        if (step == Fsm.Step.Enter)
        {
            StopCoroutine(_runningCoroutine);

            _isBlockFlip = true;
            _tobotAnim.CrossFade(_anim_StartRolling, 0);
            _currentTotemState = CurrentTotemState.Recovery;
            RespawnTotems(3);
            //ChangeTotemsAnimator(_recoveryTotemAnimator);
            _health.SetInvincible(true);
            _health.StartHealthRecovery(_recoveryCoolTime);
            SetCurrentRecoveryValue();

        }
        else if (step == Fsm.Step.Exit)
        {
            _isBlockFlip = false;
            _health.SetInvincible(false);
            _health.StopHealthRecovery();
            _lastHealthValue = _health.CurrentHealth;
            _tobotAnim.CrossFade(_anim_StopRolling, 0);
            _currentTotemState = CurrentTotemState.None;
        }
    }

    void Fsm_StunState(Fsm fsm, Fsm.Step step, Fsm.State state)
    {
        if (step == Fsm.Step.Enter)
        {
            _stateEnterTime = 0;
        }
        else if (step == Fsm.Step.Update)
        {
            _stateEnterTime += Time.deltaTime;
            if (_stateEnterTime >= _stunWaitTime)
                _fsm.TransitionTo(_idleState);

            _tobotAnim.transform.position = UnityEngine.Random.insideUnitSphere * _stunShake + _initPos;
        }
        else if (step == Fsm.Step.Exit)
        {
            _tobotAnim.transform.position = _initPos;
        }
    }

    void FSM_DeadState(Fsm fsm, Fsm.Step step, Fsm.State state)
    {

    }
    #endregion

    #region Feedbacks
    private void OnHitFeedback()
    {
        float currentHealthPercent = (_health.CurrentHealth / _health.MaxHealth) * 100;
        if (_recoveryTriggerPhase.IsActiveTrigger(currentHealthPercent))
            _fsm.TransitionTo(_recoveryState);
    }

    private void OnRecoveryFeedback()
    {
        _recoveryAnim.gameObject.SetActive(true);
        _recoveryAnim.Rebind();
        StartCoroutineOrganize(CallbackCoroutine(_recoveryAnim.GetCurrentAnimatorClipInfo(0)[0].clip.length, () => _recoveryAnim.gameObject.SetActive(false)));

    }


    private void OnDeathFeedback()
    {
        _tobotAnim.CrossFade(_anim_Dead, 0);
        _fsm.TransitionTo(_deadState);
        Managers.Game.OnBossDie();
    }
    #endregion

    private Fsm.State GetRandomState(Fsm.State[] states)
        => states[UnityEngine.Random.Range(0, states.Length)];

    public string GetUnitName()
    {
        return "바인위그";
    }

    public float GetUnitMaxHP()
    {
        return _health.MaxHealth;
    }

    public float GetUnitCurrentHP()
    {
        return _health.CurrentHealth;
    }

    private void StartCoroutineOrganize(IEnumerator coroutine)
        => _runningCoroutine = StartCoroutine(coroutine);

    private IEnumerator CallbackCoroutine(float delayTime, Action action)
    {
        yield return new WaitForSeconds(delayTime);
        action?.Invoke();
    }

    private void SetCurrentRecoveryValue()
         => _health.SetRecoverValue(_recoveryHealthValue * ((float)GetTotemsActiveCount() / _totems.Length));

    private int GetTotemsActiveCount()
    {
        int count = 0;
        foreach (var item in _totems)
        {
            if (!item.IsDead())
                count++;
        }
        return count;
    }

    private void ChangeTotemsAnimator(Animator animator)
    {
        foreach (var totem in _respawnedTotems.totems)
        {
            totem.GetComponent<DamageablePart>().ChangeAnimator(animator);
        }
    }

    private int GetRandomExcept(int min, int max, ref int previousValue)
    {
        int newValue;
        do
        {
            newValue = UnityEngine.Random.Range(min, max);
        } while (newValue == previousValue);

        previousValue = newValue; // Update the previous value
        return newValue;
    }
    private int GetRandomExcept(int min, int max, int[] previousValues)
    {
        int newValue;
        do
        {
            newValue = UnityEngine.Random.Range(min, max);
        } while (previousValues.Contains(newValue));

        return newValue;
    }


    [ContextMenu("Set Test State")]
    public void TestTransition()
    {
        _testTransitionState = _recoveryState;
        _fsm.TransitionTo(_testTransitionState);
    }

}
