using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    #region Attributes

    [SerializeField] private GameObject _portalPrefab = null;

    private Camera _camera = null;
    private GameObject _redPortal = null;
    private GameObject _bluePortal = null;

    #endregion

    #region Portal Spawning

    public void SpawnBluePortal(Vector3 start, Vector3 end)
    {
        if (_bluePortal)
        {
            if (_redPortal)
            {
                _redPortal.GetComponent<Portal>().target = null;
            }

            Destroy(_bluePortal);
        }

        _bluePortal = SpawnPortal(_redPortal, start, end, Color.blue);
    }

    public void SpawnRedPortal(Vector3 start, Vector3 end)
    {
        if (_redPortal)
        {
            if (_bluePortal)
            {
                _bluePortal.GetComponent<Portal>().target = null;
            }

            Destroy(_redPortal);
        }

        _redPortal = SpawnPortal(_bluePortal, start, end, Color.red);
    }

    GameObject SpawnPortal(GameObject target, Vector3 start, Vector3 end, Color color)
    {
        GameObject portal = null;

        RaycastHit hit;
        if (Physics.Raycast(start, end, out hit, 5000))
        {
            portal = Instantiate(_portalPrefab, hit.point + (hit.normal * 0.31f), Quaternion.LookRotation(hit.normal, Vector3.up));
            Portal portalComponent = portal.GetComponent<Portal>();
            portalComponent.color = color;

            if (target)
            {
                portalComponent.target = target;
                target.GetComponent<Portal>().target = portal;
            }
        }

        return portal;
    }

    private Vector3 Convertlocation(Transform portalTransform, Transform targetTransform, out Vector3 nearClip)
    {
        nearClip = targetTransform.worldToLocalMatrix.MultiplyPoint3x4(Camera.main.transform.position);
        nearClip = new Vector3(-nearClip.x, nearClip.y, -nearClip.z);

        return portalTransform.localToWorldMatrix.MultiplyPoint3x4(nearClip);
    }

    private Quaternion ConvertRotation(Quaternion portalRotation, Quaternion targetRotation)
    {
        Quaternion difference = portalRotation * Quaternion.Inverse(targetRotation * Quaternion.Euler(0, 180, 0));

        return difference * Camera.main.transform.rotation;
    }



    #endregion

    #region Placement System

    bool VerifyPortalPlacement(GameObject portal, ref Vector3 origin)
    {
        Vector3 originalOrigin = origin;

        Transform portalTransform = portal.transform;
        Vector3 forward = portalTransform.forward;
        Vector3 right = portalTransform.right;
        Vector3 up = portalTransform.up;

        //Check if portal is overlapping linked portal
        GameObject target = portal.GetComponent<Portal>().target;
        if (target)
        {
            FitPortalAroundTargetPortal(portal, target, forward, right, up, ref origin);
        }

        //Check if portal fits on surface
        Vector3 portalExtent = portal.GetComponent<BoxCollider>().size;
        Vector3 topEdge = up * portalExtent.z;
        Vector3 bottomEdge = -topEdge;
        Vector3 rightEdge = right * portalExtent.y;
        Vector3 leftEdge = -rightEdge;

        if (!FitPortalOnSurface(portal, forward, right, up, topEdge, bottomEdge, rightEdge, leftEdge, ref origin))
        {
            return false;
        }

        return true;
    }

    private void FitPortalAroundTargetPortal(GameObject portal, GameObject target, Vector3 forward, Vector3 right, Vector3 up, ref Vector3 origin)
    {
        Vector3 targetforward = target.transform.forward;

        //Reposition if portals are on the same face
        if (Mathf.Approximately(Vector3.Dot(forward, targetforward), 1.0f))
        {
            Vector3 distance = origin - target.transform.position;
            Vector3 rightProjection = Vector3.Dot(distance, right) * right;
            Vector3 upProjection = Vector3.Dot(distance, up) * up;

            float rightProjectionLength = rightProjection.magnitude;
            float upProjectionLength = upProjection.magnitude;

            if (rightProjectionLength < 1.0f)
            {
                rightProjection = right;
            }

            Vector3 size = portal.GetComponent<BoxCollider>().size;
            if (upProjectionLength < size.z && rightProjectionLength < size.y)
            {
                rightProjection.Normalize();
                origin += rightProjection * (size.y - rightProjectionLength + 1.0f);
            }
        }
    }

    private bool FitPortalOnSurface(GameObject portal, Vector3 forward, Vector3 right, Vector3 up, Vector3 topEdge, Vector3 bottomEdge, Vector3 rightEdge, Vector3 leftEdge, ref Vector3 origin, int recursionCount = 6)
    {
        if (recursionCount == 0)
        {
            return false;
        }

        Vector3[] corners = new Vector3[4];
        corners[0] = origin + topEdge + leftEdge;
        corners[1] = origin + topEdge + rightEdge;
        corners[2] = origin + bottomEdge + leftEdge;
        corners[3] = origin + bottomEdge + rightEdge;

        RaycastHit hit = new RaycastHit();
        for (int i = 0; i < 4; ++i)
        {
            if (TraceCorner(portal, origin, corners[i], forward, right, up, ref hit))
            {
                //float Distance = Vector3.(corners[i], Vector3.Cross(hit.normal, forward), hit.point) + 5.0f;
                origin += hit.normal;// * Distance;

                return FitPortalOnSurface(portal, forward, right, up, topEdge, bottomEdge, rightEdge, leftEdge, ref origin, recursionCount - 1);
            }
        }

        return true;
    }

    private bool TraceCorner(GameObject portal, Vector3 start, Vector3 end, Vector3 forward, Vector3 right, Vector3 up, ref RaycastHit hit)
    {
        bool foundHit = false;

        //Check inner surface for intersections
        RaycastHit innerHit;
        Vector3 direction = (end - forward) - (start - forward);
        if (Physics.Raycast(start - forward, direction, out innerHit, direction.magnitude))
        {
            hit = innerHit;
            foundHit = true;
        }

        //Check outer surface for intersections
        RaycastHit outerHit;
        direction = (end + forward) + (start - forward);
        if (Physics.Raycast(start + forward, direction, out outerHit, direction.magnitude))
        {
            hit = outerHit;
            foundHit = true;
        }

        //Determine the closer intersection, if any
        if (innerHit.collider && outerHit.collider)
        {
            hit = (innerHit.distance <= outerHit.distance) ? innerHit : outerHit;
        }

        //Check if corner overlaps surface, if not we reached the end of surface and fake it as collision
        RaycastHit overlapHit;

        float fraction = 0.0f;
        direction = end - start;
        Vector3 location;
        while (fraction <= 1.0f)
        {
            location = start + (direction * fraction);
            if (!Physics.Raycast(start, direction, out overlapHit, direction.magnitude))
            {
                //Found an edge now determine its normal
                Vector3 rightProjection = Vector3.Project(direction, right);
                Vector3 upProjection = Vector3.Project(direction, up);

                direction = location + (rightProjection * 0.05f) - (upProjection * 0.05f) - (forward * 2) -
                                    location + (rightProjection * 0.05f) - (upProjection * 0.05f) + (forward * 2);

                bool vertical = Physics.Raycast(
                    location + (rightProjection * 0.05f) - (upProjection * 0.05f) + (forward * 2),
                    direction,
                    out overlapHit,
                    direction.magnitude
                );

                direction = location + (upProjection * 0.05f) - (rightProjection * 0.05f) - (forward * 2) -
                    location + (upProjection * 0.05f) - (rightProjection * 0.05f) + (forward * 2);

                bool horizontal = Physics.Raycast(
                    location + (upProjection * 0.05f) - (rightProjection * 0.05f) + (forward * 2),
                    direction,
                    out overlapHit,
                    direction.magnitude
                );

                if (vertical)
                {
                    overlapHit.normal = -Vector3.ClampMagnitude(upProjection, 1.0f);
                }

                if (horizontal)
                {
                    overlapHit.normal = -Vector3.ClampMagnitude(rightProjection, 1.0f);
                }

                //Corner, choose one
                if (!vertical && !horizontal)
                {
                    overlapHit.normal = -Vector3.ClampMagnitude(upProjection, 1.0f);
                }

                overlapHit.point = location;
                overlapHit.distance = Vector3.Distance(start, overlapHit.point);

                if (foundHit)
                {
                    if (overlapHit.distance <= hit.distance)
                    {
                        hit = overlapHit;
                    }
                }
                else
                {
                    hit = overlapHit;
                }

                foundHit = true;
                break;
            }

            overlapHit = new RaycastHit();
            fraction += 0.05f;
        }

        return foundHit;
    }

    #endregion

    #region Monobehaviour Functions

    // start is called before the first frame update
    void Start()
    {
        _camera = gameObject.AddComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject portalObject in GameObject.FindGameObjectsWithTag("Portal"))
        {
            GameObject target = portalObject.GetComponent<Portal>().target;
            if (target)
            {
                Vector3 nearClip;
                _camera.transform.position = Convertlocation(portalObject.transform, target.transform, out nearClip);
                _camera.transform.rotation = ConvertRotation(portalObject.transform.rotation, target.transform.rotation);
                _camera.nearClipPlane = nearClip.magnitude;
                _camera.targetTexture = target.GetComponent<Portal>().renderTexture;
                _camera.Render();
            }
        }
    }

    #endregion
}
