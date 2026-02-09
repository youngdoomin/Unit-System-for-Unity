using UnityEngine;

public class HomingProjectile : Projectile
{
    public float _rotateSpeed = 200f;
    [SerializeField] private bool _isStartMove;
    private bool _initIsStartMove;
    private Vector2 _startPos;
    [SerializeField] private float _homingTime;
    private float _homingEndTime;
    [SerializeField] private float _endPosDist;
    [SerializeField] private float _posDistRandom;
    [SerializeField] private Vector3 _initDirection;

    protected void OnEnable()
    {
        _homingEndTime = Time.time + _homingTime;
        _initIsStartMove = _isStartMove;
    }

    protected override void Start()
    {
        base.Start();
        _startPos = transform.position;
    }

    public override void Movement()
    {
        print($"{gameObject} Speed {Speed} {_direction} {Acceleration * Time.deltaTime}");
        if (_homingEndTime < Time.time)
        {
            base.Movement();
            return;
        }

        if (Speed <= 0)
            return;

        if (!_isStartMove)
        {
            _direction = (Vector2)_target.position - (Vector2)transform.position;
            _direction.Normalize();
        }
        else
        {
            _direction = _initDirection;
            var calculEndPosDist = Random.Range(_endPosDist - _posDistRandom, _endPosDist + _posDistRandom);
            if (Vector2.Distance(transform.position, _startPos) >= calculEndPosDist)
            {
                _isStartMove = false;
                _homingEndTime = Time.time + _homingTime;
            }
        }

        float rotAngle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Slerp(transform.localRotation, Quaternion.AngleAxis(rotAngle, Vector3.forward), Time.deltaTime * _rotateSpeed);

        _movement = _direction * (Speed) * Time.deltaTime;
        transform.Translate(_movement, Space.World);

        Speed += Acceleration * Time.deltaTime;
    }

    protected void OnDisable()
    {
        _isStartMove = _initIsStartMove;
    }
}
