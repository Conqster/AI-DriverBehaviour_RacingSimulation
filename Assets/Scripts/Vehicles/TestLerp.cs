using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLerp : MonoBehaviour
{
    public float start = 30.0f;
    public float end = 60f;

    [Space]
    public float current;

    [Space]
    [SerializeField, Range(0f, 1f)] private float ratio = 0.0f;


    [Space]
    [Header("Second Text")]
    [SerializeField, Range(30, 90)] private float current2 = 30;

    [Space]
    public float ratio2 = 0.0f;
    public float ratiofun = 0.0f;

    private void Update()
    {

        ratio2 = current2 / (30 + 60);

        ratiofun = Mathf.InverseLerp(30, 60, current2);

        current = Mathf.Lerp(start, end, ratiofun);
    }
}
