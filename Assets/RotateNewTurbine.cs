using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateNewTurbine : MonoBehaviour
{
    public GameObject objectToRotateAround;
    public float rotationSpeed;

    void Update()
    {
        transform.RotateAround(objectToRotateAround.transform.position, new Vector3(0, 0.1f, 1), Time.deltaTime * rotationSpeed);
    }
}
