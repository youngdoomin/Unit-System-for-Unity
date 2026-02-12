using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectReceiver : MonoBehaviour
{
    [SerializeField] private SlowStatusEffect slowStatus;
    private EffectableEntity _effectableEntity;

    private void Start()
    {
        _effectableEntity = GetComponent<EffectableEntity>();
    }

    [ContextMenu("TriggerSlow")]
    public void TriggerSlowStatus()
    {
        _effectableEntity.ApplyEffect(slowStatus);
    }
}
