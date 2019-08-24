using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    [SerializeField] private GameObject m_PortalPrefab;
    private GameObject RedPortal;
    private GameObject BluePortal;
    private Camera m_Camera;

    // Start is called before the first frame update
    void Start()
    {
        m_Camera = gameObject.AddComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject portalObject in GameObject.FindGameObjectsWithTag("Portal"))
        {
            Portal portal = portalObject.GetComponent<Portal>();
            Portal target = portal.target;
            if (portal && target)
            {
                m_Camera.transform.position = ConvertLocation(portal.transform, target.transform);
                m_Camera.transform.rotation = ConvertRotation(portal.transform.rotation, target.transform.rotation);
                //todo fix clip plane

                Vector3 lookPosition = target.transform.worldToLocalMatrix.MultiplyPoint3x4(Camera.main.transform.position);
                lookPosition = new Vector3(-lookPosition.x, lookPosition.y, -lookPosition.z);

                m_Camera.nearClipPlane = lookPosition.magnitude;
                m_Camera.targetTexture = target.renderTexture;
                m_Camera.Render();
            }
        }
    }

    public void SpawnBluePortal(Vector3 start, Vector3 end)
    {
        if (BluePortal)
        {
            if (RedPortal)
            {
                RedPortal.GetComponent<Portal>().target = null;
            }

            Destroy(BluePortal);
        }

        BluePortal = SpawnPortal(RedPortal, start, end, Color.blue);
    }

    public void SpawnRedPortal(Vector3 start, Vector3 end)
    {
        if (RedPortal)
        {
            if (BluePortal)
            {
                BluePortal.GetComponent<Portal>().target = null;
            }

            Destroy(RedPortal);
        }

        RedPortal = SpawnPortal(BluePortal, start, end, Color.red);
    }

    GameObject SpawnPortal(GameObject target, Vector3 start, Vector3 end, Color color)
    {
        GameObject portal = null;

        RaycastHit hit;
        if (Physics.Raycast(start, end, out hit, 5000))
        {
            portal = Instantiate(m_PortalPrefab, hit.point + (hit.normal * 0.31f), Quaternion.LookRotation(hit.normal, Vector3.up));
            Portal portalComponent = portal.GetComponent<Portal>();
            portalComponent.color = color;
            portalComponent.surface = hit.transform.gameObject;

            if (target)
            {
                Portal targetPortalComponent = target.GetComponent<Portal>();
                portalComponent.target = targetPortalComponent;
                targetPortalComponent.target = portalComponent;
            }
        }

        return portal;
    }

    private Vector3 ConvertLocation(Transform portalTransform, Transform targetTransform)
    {
        Vector3 lookPosition = targetTransform.worldToLocalMatrix.MultiplyPoint3x4(Camera.main.transform.position);
        lookPosition = new Vector3(-lookPosition.x, lookPosition.y, -lookPosition.z);

        return portalTransform.localToWorldMatrix.MultiplyPoint3x4(lookPosition);
    }

    private Quaternion ConvertRotation(Quaternion portalRotation, Quaternion targetRotation)
    {
        Quaternion difference = portalRotation * Quaternion.Inverse(targetRotation * Quaternion.Euler(0, 180, 0));

        return difference * Camera.main.transform.rotation;
    }
}
