using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectActiveEvent : MonoBehaviour
{
    [SerializeField] private GameObject[] _gameObjects;
    [SerializeField] private float _showTimer;
    [SerializeField] private Health _health;
    [SerializeField] private bool _hideOnDeath;
    // Start is called before the first frame update
    void Start()
    {
        if (_hideOnDeath && _health)
            _health.onDeath += () => StartCoroutine(TriggerEvent(false));
    }

    public IEnumerator TriggerEvent(bool isTrue)
    {
        foreach (var gameObject in _gameObjects)
            gameObject.SetActive(isTrue);

        if (isTrue)
        {
            yield return new WaitForSeconds(_showTimer);
            StartCoroutine(TriggerEvent(false));
        }
        else
        {
            yield break;

        }
    }
}
