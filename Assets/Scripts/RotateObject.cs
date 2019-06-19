using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    private float axisY = 0;
    public float spd = 10;
    // Update is called once per frame
    void Update()
    {
        axisY = Time.deltaTime * spd;
        transform.Rotate(0, axisY, 0);
    }
}
