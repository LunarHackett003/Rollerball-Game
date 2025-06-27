using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisRotator : MonoBehaviour
{
    [SerializeField] Vector3 axis;
    [SerializeField] float speed;
    private void FixedUpdate()
    {
        transform.RotateAround(transform.position, transform.parent.TransformDirection(axis), speed);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, transform.parent.TransformDirection(axis));
    }
    [ContextMenu("Normalise direction")]
    void Normalize()
    {
        axis.Normalize();
    }
}
