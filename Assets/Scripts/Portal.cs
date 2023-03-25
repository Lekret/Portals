using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private const float WarpDistanceThreshold = 2;


    [SerializeField] private Portal _otherPortal;


    [SerializeField] private Collider _portalCollider;


    [SerializeField] private Collider _wallCollider;


    [SerializeField] private Renderer _renderer;


    private readonly HashSet<PortalableObject> _portalableObjects = new();
    private readonly Queue<PortalableObject> _warpedObjects = new();
    private AsyncTriggerEnterTrigger _asyncTriggerEnterTrigger;
    private AsyncTriggerExitTrigger _asyncTriggerExitTrigger;
    private bool _isDestroyed;
    private CancellationTokenSource _activationSource;


    public Portal OtherPortal => _otherPortal;


    public Renderer Renderer => _renderer;


    public void Activate()
    {
        if (_isDestroyed)
            return;

        _activationSource?.Cancel();
        _activationSource = new CancellationTokenSource();
        var token = _activationSource.Token;
        StartWarpLoop(token).Forget();
        StartEnterLoop(token).Forget();
        StartExitLoop(token).Forget();
    }


    public void Deactivate()
    {
        if (_isDestroyed)
            return;

        _activationSource?.Cancel();
    }


    private void Awake()
    {
        if (_otherPortal == null)
            Debug.LogError($"$[{nameof(Portal)}] OtherPortal is null");

        _asyncTriggerEnterTrigger = _portalCollider.GetAsyncTriggerEnterTrigger();
        _asyncTriggerExitTrigger = _portalCollider.GetAsyncTriggerExitTrigger();
    }


    private void OnDestroy()
    {
        _activationSource?.Cancel();
        _isDestroyed = true;
    }


    private async UniTask StartWarpLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            TryWarpObjects();
            await UniTask.Yield(PlayerLoopTiming.EarlyUpdate);
        }
    }


    private void TryWarpObjects()
    {
        foreach (var portalableObject in _portalableObjects)
        {
            if (!CanWarpObject(portalableObject))
                continue;

            portalableObject.Warp();
            _warpedObjects.Enqueue(portalableObject);
        }

        while (_warpedObjects.TryDequeue(out var obj))
            ExitPortal(obj);
    }


    private async UniTask StartEnterLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _asyncTriggerEnterTrigger)
            TryEnterPortal(await _asyncTriggerEnterTrigger.OnTriggerEnterAsync(ct));
    }


    private async UniTask StartExitLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _asyncTriggerExitTrigger)
            TryExitPortal(await _asyncTriggerExitTrigger.OnTriggerExitAsync(ct));
    }


    private void TryEnterPortal(Collider other)
    {
        if (!other.TryGetComponent(out PortalableObject obj))
            return;

        if (CanWarpObject(obj))
            return;

        if (_portalableObjects.Add(obj))
            obj.EnterPortal(this, _otherPortal, _wallCollider);
    }


    private void TryExitPortal(Collider other)
    {
        if (other.TryGetComponent(out PortalableObject obj))
            ExitPortal(obj);
    }


    private void ExitPortal(PortalableObject obj)
    {
        if (_portalableObjects.Remove(obj))
            obj.ExitPortal(_wallCollider);
    }


    private bool CanWarpObject(PortalableObject obj)
    {
        var objPos = transform.InverseTransformPoint(obj.transform.position);

        if (objPos.z <= 0)
            return false;

        if (Mathf.Abs(objPos.z) > WarpDistanceThreshold)
            return false;

        return true;
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!_otherPortal)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, _otherPortal.transform.position);
    }
#endif
}