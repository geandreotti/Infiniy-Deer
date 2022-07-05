using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireMotion : MonoBehaviour
{
    private Vector3 _startPosition;
    [SerializeField] private float _motionSpeed;
    [SerializeField] private float _motionMultiplier;
    void Start()
    {
        _startPosition = transform.position;
    }

    void Update()
    {
        transform.position = _startPosition + new Vector3(_motionMultiplier * Mathf.Sin(Time.time * _motionSpeed), _motionMultiplier * Mathf.Sin(Time.time * _motionSpeed), _motionMultiplier * Mathf.Sin(Time.time * _motionSpeed));

    }
}
