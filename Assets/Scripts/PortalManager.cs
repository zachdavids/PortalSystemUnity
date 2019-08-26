using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    #region Attributes

    [SerializeField] private GameObject _portalPrefab = null;
    [SerializeField] private Camera _camera = null;

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
                Portal targetPortalComponent = target.GetComponent<Portal>();
                portalComponent.target = targetPortalComponent;
                targetPortalComponent.target = portalComponent;
            }
        }

        return portal;
    }

    private Vector3 ConvertLocation(Transform portalTransform, Transform targetTransform, out Vector3 nearClip)
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

    #region Monobehaviour Functions

    // Start is called before the first frame update
    void Start()
    {
        _camera = gameObject.AddComponent<Camera>();
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
                Vector3 nearClip;
                _camera.transform.position = ConvertLocation(portal.transform, target.transform, out nearClip);
                _camera.transform.rotation = ConvertRotation(portal.transform.rotation, target.transform.rotation);
                _camera.nearClipPlane = nearClip.magnitude;
                _camera.targetTexture = target.renderTexture;
                _camera.Render();
            }
        }
    }

    #endregion
}
