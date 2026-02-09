using UnityEngine;

public class NewProjectile : BaseWeaponSensor
{
    protected Health _health;

    [Header("About Projectile")]
    [SerializeField] private bool _isUpdatePoint = false; // 포인트 업데이트 여부(타겟이 있을 시, 업데이트 가능됨)
    private Vector2 _spawnPoint; // 스폰 포인트
    private Transform _target; // 타겟팅
    private Vector2 _point; // 포인트
    private Vector2 _movement; // 이동 값
    [SerializeField] private float _damage;

    [Header("Movement")]
    [SerializeField] protected float _speed = 200; // 속도
    [SerializeField] protected float _maxSpeed = 200; // 최고 속도
    [SerializeField] protected float _acceleration = 0; // 가속도

    protected Vector2 _direction; // 진행 방향

    protected override void Awake()
    {
        base.Awake();
        _health = GetComponent<Health>();
    }

    protected virtual void Start()
    {
        if (_health)
            _health.onDeath += () => gameObject.SetActive(false);
    }

    protected virtual void OnEnable()
    {
        _direction = _point - (Vector2)transform.position;
        _direction.Normalize();
        float rotAngle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotation = Quaternion.AngleAxis(rotAngle, Vector3.forward);
        transform.rotation = rotation;
    }

    void FixedUpdate()
    {
        if (_target && _isUpdatePoint)
        {
            _point = _target.position;
            _direction = _point - (Vector2)transform.position;
        }
        Movement();
    }

    public virtual void OnCreate(Transform target = null, bool isUpdatePoint = false)
    {
        _target = target;
        _isUpdatePoint = isUpdatePoint;
    }

    public void Spawn(Vector2 spawnPoint, Vector2 point)
    {
        _spawnPoint = spawnPoint;
        transform.position = _spawnPoint;
        _point = (_target) ? _target.position : point;
        _direction = _point - _spawnPoint;
        _health?.Spawn();
    }

    public virtual void Movement()
    {
        var deltaTime = Time.deltaTime;
        print($"{gameObject} " +
            $"\n [Speed]: {_speed} " +
            $"\n [Direction]: {_direction}" +
            $"\n [Add Acceleration]: {_acceleration * deltaTime}");

        if (_speed <= 0 || _direction == Vector2.zero)
            return;

        _direction.Normalize();

        // rotate
        float rotAngle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotation = Quaternion.AngleAxis(rotAngle, Vector3.forward);
        transform.rotation = rotation;

        // move
        _movement = _direction * _speed * deltaTime;
        transform.Translate(_movement, Space.World);

        _speed += _acceleration * deltaTime;
        if (_speed >= _maxSpeed)
            _speed = _maxSpeed;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (checkLayerResult == false)
            return;

        collision.GetComponent<UnitController>()?.OnHitUnit(_damage);
        collision.GetComponent<DamageablePart>()?.SendDamage(_damage);
        _health?.Damage(9999);
    }
}
