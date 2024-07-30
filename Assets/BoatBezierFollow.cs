using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using Unity.XR.CoreUtils;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BoatBezierFollow : MonoBehaviour
{
    [SerializeField]
    private Transform[] routes;

    [SerializeField]
    private Transform[] centerPointsA; // First points to define the center for each route
    [SerializeField]
    private Transform[] centerPointsB; // Second points to define the center for each route

    [SerializeField]
    private XROrigin xrOrigin; // Reference to the XR Origin

    [SerializeField]
    private Collider playerDetectionTrigger; // Reference to the player detection trigger

    private int routeToGo;
    private float tParam;
    private Vector3 objectPosition;
    private float speedModifier;
    private bool coroutineAllowed;
    private Vector3 centerOfCurve;
    private bool playerOnBoat = false;
    private Vector3 previousBoatPosition;
    private Quaternion previousBoatRotation;

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
        speedModifier = 0.1f;
        coroutineAllowed = true;

        if (centerPointsA == null || centerPointsB == null)
        {
            Debug.LogError("Center points arrays are not assigned properly or do not match the number of routes.");
        }

        if (playerDetectionTrigger == null)
        {
            Debug.LogError("Player detection trigger is not assigned.");
        }

        Debug.Log("BezierFollow script initialized");
        if (routes == null || routes.Length == 0)
        {
            Debug.LogError("Routes array is not assigned or empty");
        }

        previousBoatPosition = transform.position;
        previousBoatRotation = transform.rotation;
    }

    void Update()
    {
        if (coroutineAllowed)
        {
            StartCoroutine(GoByTheRoute(routeToGo));
        }

        // Update player position and rotation manually if on boat
        if (playerOnBoat && xrOrigin != null)
        {
            // Calculate boat's displacement and rotation change
            Vector3 boatDisplacement = transform.position - previousBoatPosition;
            Quaternion boatRotationChange = transform.rotation * Quaternion.Inverse(previousBoatRotation);

            // Update player position based on boat's movement
            xrOrigin.transform.position += boatDisplacement;

            // Update player rotation based on boat's rotation change
            xrOrigin.transform.RotateAround(transform.position, Vector3.up, boatRotationChange.eulerAngles.y);

            previousBoatPosition = transform.position;
            previousBoatRotation = transform.rotation;
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

        while (tParam < 1)
        {
            tParam += Time.deltaTime * speedModifier;

            objectPosition = Mathf.Pow(1 - tParam, 3) * p0 +
                             3 * Mathf.Pow(1 - tParam, 2) * tParam * p1 +
                             3 * (1 - tParam) * Mathf.Pow(tParam, 2) * p2 +
                             Mathf.Pow(tParam, 3) * p3;

            transform.position = objectPosition;

            // Rotate the boat to face the center of the curve
            Vector3 directionToCenter = centerOfCurve - transform.position;
            transform.rotation = Quaternion.LookRotation(directionToCenter, Vector3.up);

            // Ensure XR Origin moves with the boat
            if (playerOnBoat && xrOrigin != null)
            {
                // Calculate boat's displacement and rotation change
                Vector3 boatDisplacement = transform.position - previousBoatPosition;
                Quaternion boatRotationChange = transform.rotation * Quaternion.Inverse(previousBoatRotation);

                // Update player position based on boat's movement
                xrOrigin.transform.position += boatDisplacement;

                // Update player rotation based on boat's rotation change
                xrOrigin.transform.RotateAround(transform.position, Vector3.up, boatRotationChange.eulerAngles.y);

                previousBoatPosition = transform.position;
                previousBoatRotation = transform.rotation;
            }

            previousBoatPosition = transform.position;
            previousBoatRotation = transform.rotation;

#if UNITY_EDITOR
            EditorUtility.SetDirty(transform);
#endif

            yield return new WaitForEndOfFrame();
        }

        tParam = 0f;

        routeToGo += 1;

        if (routeToGo > routes.Length - 1)
        {
            routeToGo = 0;
        }

        coroutineAllowed = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (xrOrigin != null && other.gameObject == xrOrigin.gameObject)
        {
            playerOnBoat = true;
            previousBoatPosition = transform.position;
            previousBoatRotation = transform.rotation;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (xrOrigin != null && other.gameObject == xrOrigin.gameObject)
        {
            playerOnBoat = false;
        }
    }
}