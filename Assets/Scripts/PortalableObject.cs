using Cysharp.Threading.Tasks;
using UnityEngine;

public class PortalableObject : MonoBehaviour
{
    private GameObject _cloneObject;
    private Portal _inPortal;
    private Portal _outPortal;
    private int _inPortalCount;
    private bool _isDestroyed;
    private Collider[] _colliders;


    protected Portal InPortal => _inPortal;


    protected Portal OutPortal => _outPortal;


    protected GameObject CloneObject => _cloneObject;


    protected virtual void Awake()
    {
        _cloneObject = ProvideCloneObject();
        _cloneObject.SetActive(false);
        _cloneObject.transform.localScale = transform.localScale;
        _colliders = GetComponentsInChildren<Collider>(true);
    }


    private void OnDestroy()
    {
        _isDestroyed = true;
    }


    protected virtual GameObject ProvideCloneObject() => new($"{name}(PortalClone)");


    private async UniTask StartUpdatingClone()
    {
        while (!_isDestroyed && _inPortalCount > 0 && _inPortal && _outPortal)
        {
            UpdateActiveClone();
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
        }
    }


    protected virtual void UpdateActiveClone()
    {
        var inTransform = _inPortal.transform;
        var outTransform = _outPortal.transform;
        _cloneObject.transform.position =
            PortalUtils.CalculateWarpPosition(inTransform, outTransform, transform.position);
        _cloneObject.transform.rotation =
            PortalUtils.CalculateWarpRotation(inTransform, outTransform, transform.rotation);
    }


    public void EnterPortal(Portal inPortal, Portal outPortal, Collider wallCollider)
    {
        _inPortal = inPortal;
        _outPortal = outPortal;

        if (wallCollider)
            foreach (var coll in _colliders)
                Physics.IgnoreCollision(coll, wallCollider, true);

        _inPortalCount++;
        _cloneObject.SetActive(true);

        if (_inPortalCount == 1)
            StartUpdatingClone().Forget();
    }


    public void ExitPortal(Collider wallCollider)
    {
        if (wallCollider)
            foreach (var coll in _colliders)
                Physics.IgnoreCollision(coll, wallCollider, false);

        _inPortalCount--;

        if (_inPortalCount == 0)
        {
            _cloneObject.SetActive(false);
            _cloneObject.transform.position = new Vector3(-1000.0f, 1000.0f, -1000.0f);
        }
    }


    public void Warp()
    {
        OnWarp();
        SwapPortals();
    }


    protected virtual void OnWarp()
    {
        var inTransform = _inPortal.transform;
        var outTransform = _outPortal.transform;

        transform.position = PortalUtils.CalculateWarpPosition(inTransform, outTransform, transform.position);
        transform.rotation = PortalUtils.CalculateWarpRotation(inTransform, outTransform, transform.rotation);

        if (TryGetComponent(out Rigidbody rb))
            rb.velocity = PortalUtils.CalculateWarpDirection(inTransform, outTransform, rb.velocity);
    }


    private void SwapPortals()
    {
        (_inPortal, _outPortal) = (_outPortal, _inPortal);
    }
}