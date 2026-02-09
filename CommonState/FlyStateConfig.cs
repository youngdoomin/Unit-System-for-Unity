using UnityEngine;
using static FlyState;

[CreateAssetMenu(fileName = "FlyStateConfig", menuName = "Scriptable Object/StateSO/FlyStateConfig", order = 0)]
public class FlyStateConfig : ScriptableObject
{
    public float speed;
    public float acceleration;

    public StateValue Generate()
    {
        return new StateValue
        {
            speedValue = speed,
            accelerationValue = acceleration
        };
    }
}