using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatController : MonoBehaviour
{
    private bool boatMoving = false;
    public LineRenderer lr;

    private Vector3[] positions = new Vector3[20];
    private Vector3[] pos;
    private int index = 0;
    public float speed;
    public float toleratedDistance;

    // Start is called before the first frame update
    void Start()
    {
        if (lr == null)
            throw new System.Exception("Please attach a LineRenderer to the BoatController");
        pos = GetLinePointsInWorldSpace();
    }

    Vector3[] GetLinePointsInWorldSpace()
    {
        //Get the positions which are shown in the inspector 
        lr.GetPositions(positions);

        Debug.Log(positions);

        //the points returned are in world space
        return positions;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Space))
            Move();
    }

    void Move()
    {
        Debug.Log(pos[index]);
        transform.position = Vector3.MoveTowards(transform.position,
                                                pos[index],
                                                speed * Time.deltaTime); ;

        Vector3 yIgnoredPointPosition = new Vector3(pos[index].x, transform.position.y, pos[index].z);

        if (Vector3.Distance(transform.position, yIgnoredPointPosition) < toleratedDistance)
        {
            index += 1;
        }

        if (index == pos.Length)
            index = 0;
    }
}
