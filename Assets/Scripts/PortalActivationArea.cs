using System;
using UnityEngine;

public class PortalActivationArea : MonoBehaviour
{
    [SerializeField] private Portal _portal;

    private PortalActivationArea _otherPortalActivationArea;

    private void Awake()
    {
        _otherPortalActivationArea = _portal.OtherPortal.GetComponentInChildren<PortalActivationArea>();
        SetPlayerNear(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other))
            SetPlayerNear(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other))
            SetPlayerNear(false);
    }

    private bool IsPlayer(Collider other)
    {
        throw new NotImplementedException($"TODO {nameof(IsPlayer)}");
    }

    private void SetPlayerNear(bool isNear)
    {
        if (isNear)
        {
            Activate();
            _otherPortalActivationArea.Activate();
        }
        else
        {
            Deactivate();
            _otherPortalActivationArea.Deactivate();
        }

        _portal.Renderer.enabled = isNear;
    }


    private void Activate()
    {
        _portal.Activate();
    }


    private void Deactivate()
    {
        _portal.Deactivate();
    }
}