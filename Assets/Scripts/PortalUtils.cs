using UnityEngine;

public static class PortalUtils
{
    private const float WARP_POSITION_OUT_ADDITIONAL_MOVEMENT = 0.2f;
    private static Quaternion HalfTurn = Quaternion.Euler(0.0f, 180.0f, 0.0f);


    public static Vector3 CalculateWarpPosition(Transform inTransform, Transform outTransform, Vector3 position)
    {
        var relativePos = inTransform.InverseTransformPoint(position);
        relativePos = HalfTurn * relativePos;

        var outAdditionalMovement = -outTransform.forward * WARP_POSITION_OUT_ADDITIONAL_MOVEMENT;
        return outTransform.TransformPoint(relativePos) + outAdditionalMovement;
    }


    public static Quaternion CalculateWarpRotation(Transform inTransform, Transform outTransform, Quaternion rotation)
    {
        var relativeRot = Quaternion.Inverse(inTransform.rotation) * rotation;
        relativeRot = HalfTurn * relativeRot;

        return outTransform.rotation * relativeRot;
    }


    public static Vector3 CalculateWarpDirection(Transform inTransform, Transform outTransform, Vector3 direction)
    {
        var relativeVel = inTransform.InverseTransformDirection(direction);
        relativeVel = HalfTurn * relativeVel;

        return outTransform.TransformDirection(relativeVel);
    }
}