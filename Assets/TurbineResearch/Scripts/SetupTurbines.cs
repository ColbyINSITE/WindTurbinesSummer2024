using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SetupTurbines : MonoBehaviour
{
    /*
        This script places 10 turbines in a honeycomb shape.
        3 rows, middle row has 4 turbines and the outer rows have 3.
        The script uses the middle point of all turbines to place them.
        Just attach it to an object and place it in the world to get the turbines.
    */

    public GameObject turbine;
    private Vector3 center;

    public float distance = 2222.4f;
    private float height;

    // Transform location xyz
    private float tx;
    private float ty;
    private float tz;

    // Start is called before the first frame update
    /*
    void Start()
    {   
        tx = transform.position.x;
        ty = transform.position.y;
        tz = transform.position.z;
        height = distance * (Mathf.Sqrt(3)/2);
        center = transform.position;

        List<Vector3> positions = CalculatePositions();
        foreach (Vector3 position in positions) 
        {
            Instantiate(turbine, position, transform.rotation);
        }
    }

    List<Vector3> CalculatePositions()
    {
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < 4; i++)
        {
            positions.Add(new Vector3(tx+(-1.5f+i)*distance, ty, tz));
        }
        for (int i = 0; i < 3; i++)
        {
            positions.Add(new Vector3(tx+(-1+i)*distance, ty, tz+height));
        }
        for (int i = 0; i < 3; i++)
        {
            positions.Add(new Vector3(tx+(-1+i)*distance, ty, tz-height));
        }
        return positions;
    }
    */
}
