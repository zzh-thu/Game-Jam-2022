using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GentlePulse : MonoBehaviour
{
    public float minSize = .9f;
    public float maxSize = 1.1f;
    public float frequency = 4f;

    private float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.one * map(Mathf.Sin(Time.timeSinceLevelLoad * frequency), -1f, 1f, minSize, maxSize);
    }
}
