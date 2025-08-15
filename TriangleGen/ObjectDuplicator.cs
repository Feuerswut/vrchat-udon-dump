using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ObjectDuplicator : UdonSharpBehaviour
{
    [Header("Object to Duplicate")]
    public GameObject objectToDuplicate;

    [Header("Target Transforms")]
    public Transform[] targetTransforms;

    [Header("Optional Offsets")]
    public Vector3[] positionOffsets; // same length as targetTransforms

    [Header("Ignore Scale Flags")]
    public bool[] ignoreScaleFlags; // same length as targetTransforms

    [Header("Ignore Rotation Flags")]
    public bool[] ignoreRotationFlags; // same length as targetTransforms

    public void DuplicateObjects()
    {
        if (objectToDuplicate == null || targetTransforms.Length == 0) return;

        for (int i = 0; i < targetTransforms.Length; i++)
        {
            Transform target = targetTransforms[i];
            if (target == null) continue;

            GameObject clone = VRCInstantiate(objectToDuplicate);

            // Disable the clone immediately to prevent ClientSim warnings
            clone.SetActive(false);

            // Remove all Udon behaviours in the clone hierarchy
            RemoveUdonBehaviours(clone);

            // Position with optional offset
            Vector3 offset = (positionOffsets.Length > i) ? positionOffsets[i] : Vector3.zero;
            clone.transform.position = target.position + offset;

            // Rotation
            bool ignoreRot = (ignoreRotationFlags.Length > i) ? ignoreRotationFlags[i] : false;
            clone.transform.rotation = ignoreRot ? objectToDuplicate.transform.rotation : target.rotation;

            // Scale
            bool ignoreScl = (ignoreScaleFlags.Length > i) ? ignoreScaleFlags[i] : false;
            clone.transform.localScale = ignoreScl ? objectToDuplicate.transform.localScale : target.localScale;

            // Re-enable the clone after cleanup
            clone.SetActive(true);
        }
    }

    private void RemoveUdonBehaviours(GameObject obj)
    {
        if (obj == null) return;

        UdonBehaviour[] udons = obj.GetComponentsInChildren<UdonBehaviour>(true);
        foreach (UdonBehaviour ub in udons)
        {
            if (ub != null)
            {
                ub.enabled = false; // Prevent ClientSim from initializing
                Destroy(ub);
            }
        }
    }
}
