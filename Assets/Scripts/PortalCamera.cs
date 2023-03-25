using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class PortalCamera : MonoBehaviour
{
    [SerializeField] private Camera _portalCamera;


    [SerializeField] [Range(1, 5)] private int _recursionRenderIterations = 1;


    private Camera _mainCamera;
    private Portal[] _portals;
    private readonly Queue<RenderTexture> _renderTexturePool = new();


    private void Start()
    {
        _mainCamera = GetComponent<Camera>();
        _portals = FindObjectsOfType<Portal>();
    }


    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += UpdateCamera;
    }


    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= UpdateCamera;
    }


    private void UpdateCamera(ScriptableRenderContext context, Camera _)
    {
        ExtractRenderTextures();
        RenderPortals(context);
    }


    private void ExtractRenderTextures()
    {
        for (var i = 0; i < _portals.Length; i++)
        {
            var portal = _portals[i];

            if (portal.Renderer.isVisible)
                continue;

            if (portal.Renderer.material.mainTexture is RenderTexture renderTexture)
            {
                _renderTexturePool.Enqueue(renderTexture);
                portal.Renderer.material.mainTexture = null;
            }
        }
    }


    private void RenderPortals(ScriptableRenderContext context)
    {
        var renderedCount = 0;

        for (var i = 0; i < _portals.Length; i++)
        {
            var portal = _portals[i];

            if (!portal.Renderer.isVisible)
                continue;

            renderedCount++;

            var renderTexture = portal.Renderer.material.mainTexture as RenderTexture;

            if (renderTexture == null)
            {
                renderTexture = GetRenderTexture();
                portal.Renderer.material.mainTexture = renderTexture;
            }

            _portalCamera.targetTexture = renderTexture;

            for (var iter = _recursionRenderIterations - 1; iter >= 0; --iter)
                RenderCamera(portal, portal.OtherPortal, iter, context);
        }

        if (renderedCount > 1)
            Debug.LogWarning("More than one portal is considered visible, it's too expensive");
    }


    private RenderTexture GetRenderTexture()
    {
        if (_renderTexturePool.TryDequeue(out var renderTexture))
            return renderTexture;

        return new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
    }


    private void RenderCamera(Portal inPortal, Portal outPortal, int iterationID, ScriptableRenderContext context)
    {
        var inTransform = inPortal.transform;
        var outTransform = outPortal.transform;

        var cameraTransform = _portalCamera.transform;
        cameraTransform.position = transform.position;
        cameraTransform.rotation = transform.rotation;

        for (var i = 0; i <= iterationID; ++i)
        {
            cameraTransform.position =
                PortalUtils.CalculateWarpPosition(inTransform, outTransform, cameraTransform.position);
            cameraTransform.rotation =
                PortalUtils.CalculateWarpRotation(inTransform, outTransform, cameraTransform.rotation);
        }

        var p = new Plane(-outTransform.forward, outTransform.position);
        var clipPlaneWorldSpace = new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance);

        var clipPlaneCameraSpace =
            Matrix4x4.Transpose(Matrix4x4.Inverse(_portalCamera.worldToCameraMatrix)) * clipPlaneWorldSpace;

        var newMatrix = _mainCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
        _portalCamera.projectionMatrix = newMatrix;

        UniversalRenderPipeline.RenderSingleCamera(context, _portalCamera);
    }
}