using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flowing : MonoBehaviour
{
    [SerializeField] private float offset;
    [SerializeField] private float speed;
    private Vector3 _startPos;

    private void Start()
    {
        _startPos = transform.position;
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position = _startPos + Mathf.Sin(Time.time * 0.001f * speed) * Vector3.up * offset;
    }
}
