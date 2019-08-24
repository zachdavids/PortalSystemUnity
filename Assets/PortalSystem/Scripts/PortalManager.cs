using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    [SerializeField] private Camera m_Camera;
    [SerializeField] private Portal RedPortal;
    [SerializeField] private Portal BluePortal;

    // Start is called before the first frame update
    void Start()
    {
        m_Camera = new Camera();
        m_Camera.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject portalObject in GameObject.FindGameObjectsWithTag("Portal"))
        {
            Portal portal = portalObject.GetComponent<Portal>();
            Portal target = portal.Target;
            if (portal && target)
            {
                m_Camera.transform.localPosition = ConvertLocation(portal.transform, target.transform);
                m_Camera.transform.rotation = ConvertRotation(portal.transform.rotation, target.transform.rotation);
                m_Camera.nearClipPlane = m_Camera.transform.localPosition.magnitude;
                m_Camera.targetTexture = portal.renderTexture;
                m_Camera.Render();
            }
        }
    }

    private Vector3 ConvertLocation(Transform portalTransform, Transform targetTransform)
    {
        Vector3 lookPosition = targetTransform.worldToLocalMatrix.MultiplyPoint3x4(Camera.main.transform.position);
        lookPosition = new Vector3(-lookPosition.x, lookPosition.y, -lookPosition.z);

        return lookPosition;
    }

    private Quaternion ConvertRotation(Quaternion portalRotation, Quaternion targetRotation)
    {
        Quaternion difference = portalRotation * Quaternion.Inverse(targetRotation * Quaternion.Euler(0, 180, 0));

        return difference * Camera.main.transform.rotation;
    }
}
