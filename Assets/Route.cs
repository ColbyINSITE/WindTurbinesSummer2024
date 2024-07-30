using UnityEngine;

[ExecuteInEditMode]
public class Route : MonoBehaviour
{
    [SerializeField]
    private Transform[] controlPoints;

    private Vector3 gizmosPosition;

    private void OnDrawGizmos()
    {
        if (controlPoints == null || controlPoints.Length != 4)
        {
            Debug.LogError("Please assign exactly 4 control points to the Route script.");
            return;
        }

        for (int i = 0; i <= 3; i++)
        {
            if (controlPoints[i] == null)
            {
                Debug.LogError("Control point " + i + " is not assigned.");
                return;
            }
        }

        for (float t = 0; t <= 1; t += 0.05f)
        {
            gizmosPosition = Mathf.Pow(1 - t, 3) * controlPoints[0].position +
                             3 * Mathf.Pow(1 - t, 2) * t * controlPoints[1].position +
                             3 * (1 - t) * Mathf.Pow(t, 2) * controlPoints[2].position +
                             Mathf.Pow(t, 3) * controlPoints[3].position;

            Gizmos.DrawSphere(gizmosPosition, 0.25f);
        }

        Gizmos.DrawLine(controlPoints[0].position, controlPoints[1].position);
        Gizmos.DrawLine(controlPoints[1].position, controlPoints[2].position);
        Gizmos.DrawLine(controlPoints[2].position, controlPoints[3].position);
    }
}
