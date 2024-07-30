using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BezierFollow : MonoBehaviour
{
    [SerializeField]
    private Transform[] routes;

    [SerializeField]
    private Transform[] centerPointsA; // First points to define the center for each route
    [SerializeField]
    private Transform[] centerPointsB; // Second points to define the center for each route

    private int routeToGo;
    private float tParam;
    private Vector3 objectPosition;
    private float speedModifier;
    private bool coroutineAllowed;
    private Vector3 centerOfCurve;

    private void OnDrawGizmos()
    {
        if (routes == null || routes.Length == 0)
        {
            return;
        }

        for (int i = 0; i < routes.Length; i++)
        {
            if (routes[i] == null || routes[i].childCount < 4)
            {
                continue;
            }

            Vector3 p0 = routes[i].GetChild(0).position;
            Vector3 p1 = routes[i].GetChild(1).position;
            Vector3 p2 = routes[i].GetChild(2).position;
            Vector3 p3 = routes[i].GetChild(3).position;

            Vector3 previousPoint = p0;

            for (float t = 0; t <= 1; t += 0.05f)
            {
                Vector3 point = Mathf.Pow(1 - t, 3) * p0 +
                                3 * Mathf.Pow(1 - t, 2) * t * p1 +
                                3 * (1 - t) * Mathf.Pow(t, 2) * p2 +
                                Mathf.Pow(t, 3) * p3;

                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }
        }
    }

    void Start()
    {
        routeToGo = 0;
        tParam = 0f;
        speedModifier = 0.2f;
        coroutineAllowed = true;

        if (centerPointsA == null || centerPointsB == null ||
            centerPointsA.Length != routes.Length || centerPointsB.Length != routes.Length)
        {
            Debug.LogError("Center points arrays are not assigned properly or do not match the number of routes.");
        }

        Debug.Log("BezierFollow script initialized");
        if (routes == null || routes.Length == 0)
        {
            Debug.LogError("Routes array is not assigned or empty");
        }
    }

    void Update()
    {
        if (coroutineAllowed)
        {
            Debug.Log("Starting Coroutine");
            StartCoroutine(GoByTheRoute(routeToGo));
        }
    }

    private IEnumerator GoByTheRoute(int routeNumber)
    {
        coroutineAllowed = false;

        if (routes == null)
        {
            Debug.LogError("Routes array is null");
            yield break;
        }

        if (routeNumber >= routes.Length)
        {
            Debug.LogError("Route number " + routeNumber + " is out of range");
            yield break;
        }

        Transform route = routes[routeNumber];
        if (route == null)
        {
            Debug.LogError("Route " + routeNumber + " is not assigned");
            yield break;
        }

        if (route.childCount < 4)
        {
            Debug.LogError("Route " + routeNumber + " does not have 4 control points");
            yield break;
        }

        if (centerPointsA[routeNumber] == null || centerPointsB[routeNumber] == null)
        {
            Debug.LogError("Center points for route " + routeNumber + " are not assigned");
            yield break;
        }

        centerOfCurve = (centerPointsA[routeNumber].position + centerPointsB[routeNumber].position) / 2f;

        Vector3 p0 = route.GetChild(0).position;
        Vector3 p1 = route.GetChild(1).position;
        Vector3 p2 = route.GetChild(2).position;
        Vector3 p3 = route.GetChild(3).position;

        Debug.Log("Starting movement along route " + routeNumber);

        while (tParam < 1)
        {
            tParam += Time.deltaTime * speedModifier;

            objectPosition = Mathf.Pow(1 - tParam, 3) * p0 +
                             3 * Mathf.Pow(1 - tParam, 2) * tParam * p1 +
                             3 * (1 - tParam) * Mathf.Pow(tParam, 2) * p2 +
                             Mathf.Pow(tParam, 3) * p3;

            transform.position = objectPosition;

            // Rotate the cylinder to face the center of the curve
            Vector3 directionToCenter = centerOfCurve - transform.position;
            transform.rotation = Quaternion.LookRotation(directionToCenter, Vector3.up);

#if UNITY_EDITOR
            EditorUtility.SetDirty(transform);
#endif

            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Finished one route");

        tParam = 0f;

        routeToGo += 1;

        if (routeToGo > routes.Length - 1)
        {
            routeToGo = 0;
        }

        coroutineAllowed = true;
    }
}