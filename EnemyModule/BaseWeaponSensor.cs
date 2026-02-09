using UnityEngine;

public class BaseWeaponSensor : MonoBehaviour
{
    [field: SerializeField] public LayerMask CheckLayerMask { get; private set; }
    [HideInInspector]
    public bool checkLayerResult = false;

    protected virtual void Awake()
    {
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        CheckLayer(collision);
        return;
    }

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        CheckLayer(collision);
        return;
    }

    private void CheckLayer(Collider2D collision)
    {
        //선택한 레이어 충돌 확인
        if ((CheckLayerMask & (1 << collision.gameObject.layer)) != 0)
            checkLayerResult = true;
        else
            checkLayerResult = false;
    }

    protected virtual void OnAttackEffect(Collider2D collision) { }
}
