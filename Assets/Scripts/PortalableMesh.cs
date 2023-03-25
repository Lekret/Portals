using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PortalableMesh : PortalableObject
{
    protected override void Awake()
    {
        base.Awake();

        var meshFilter = CloneObject.AddComponent<MeshFilter>();
        var meshRenderer = CloneObject.AddComponent<MeshRenderer>();
        meshFilter.mesh = GetComponent<MeshFilter>().mesh;
        meshRenderer.materials = GetComponent<MeshRenderer>().materials;
    }
}