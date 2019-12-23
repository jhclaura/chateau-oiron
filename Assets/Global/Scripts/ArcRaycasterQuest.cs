using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcRaycasterQuest : MonoBehaviour
{
    [Tooltip("Tracking space of OVRCameraRig, VR space is relative to this")]
    public Transform trackingSpace;
    [Tooltip("How manu angles from world up the surface can point and still be valid. Avoids casting onto walls.")]
    public float surfaceAngle = 5;
    [Tooltip("Any layers the raycast should not affect")]
    public LayerMask excludeLayers;

    // Raycasting is relative to tracking space, not world space
    public Vector3 Up { get { return trackingSpace.up; } }
    public Vector3 Right { get { return trackingSpace.right; } }
    public Vector3 Forward { get { return trackingSpace.forward; } }

    // Where the curve starts (usually at the controller)
    public Vector3 Start { get { return ControllerPosition; } }
    // Did the ray hit anything?
    public bool MakingContact { get; protected set; }
    // If it did, what was the normal
    public Vector3 Normal { get; protected set; }
    // Where the ray actually hit
    public Vector3 HitPoint { get; protected set; }

    public OVRInput.Controller Controller
    {
        get
        {
            OVRInput.Controller controller = OVRInput.GetConnectedControllers();
            if ((controller & OVRInput.Controller.LTouch) == OVRInput.Controller.LTouch)
            {
                return OVRInput.Controller.LTouch;
            }
            else if ((controller & OVRInput.Controller.RTouch) == OVRInput.Controller.RTouch)
            {
                return OVRInput.Controller.RTouch;
            }
            return OVRInput.GetActiveController();
        }
    }

    public Vector3 ControllerPosition
    {
        get
        {
#if UNITY_EDITOR
            Debug.LogWarning("Controller position not available in editor");
#endif
            Vector3 position = OVRInput.GetLocalControllerPosition(Controller);

            return trackingSpace.localToWorldMatrix.MultiplyPoint(position);
        }
    }

    public Vector3 ControllerForward
    {
        get
        {
#if UNITY_EDITOR
            Debug.LogWarning("Controller orientation not available in editor");
#endif
            Quaternion orientation = OVRInput.GetLocalControllerRotation(Controller);
            Vector3 worldForward = trackingSpace.localToWorldMatrix.MultiplyVector(orientation * Vector3.forward);

            return worldForward.normalized;
        }
    }

    public Vector3 ControllerUp
    {
        get
        {
#if UNITY_EDITOR
            Debug.LogWarning("Controller orientation not available in editor");
#endif
            Quaternion orientation = OVRInput.GetLocalControllerRotation(Controller);
            Vector3 worldForward = trackingSpace.localToWorldMatrix.MultiplyVector(orientation * Vector3.up);

            return worldForward.normalized;
        }
    }

    public Vector3 ControllerRight
    {
        get
        {
#if UNITY_EDITOR
            Debug.LogWarning("Controller orientation not available in editor");
#endif
            Quaternion orientation = OVRInput.GetLocalControllerRotation(Controller);
            Vector3 worldForward = trackingSpace.localToWorldMatrix.MultiplyVector(orientation * Vector3.right);

            return worldForward.normalized;
        }
    }
    
    [Tooltip("Horizontal distance of end point from controller")]
    public float distance = 15.0f;
    [Tooltip("Vertical of end point from controller")]
    public float dropHeight = 5.0f;
    [Tooltip("Height of bezier control (0 is at mid point)")]
    public float controlHeight = 5.0f;
    [Tooltip("How many segments to use for curve, must be at least 3. More segments = better quality")]
    public int segments = 10;

    // Where the curve ends
    public Vector3 End { get; protected set; }

    public Vector3 Control
    {
        get
        {
            Vector3 midPoint = Start + (End - Start) * 0.5f;
            return midPoint + ControllerUp * controlHeight;
        }
    }

    private void Awake()
    {
        if (trackingSpace == null && OVRManager.instance != null)
        {
            GameObject cameraObject = OVRManager.instance.gameObject;
            trackingSpace = cameraObject.transform.Find("TrackingSpace");
            Debug.LogWarning("Tracking space not set for BezierRaycaster");
        }
        if (trackingSpace == null)
        {
            Debug.LogError("Tracking MUST BE set for BezierRaycaster");
        }
    }

    private void Update()
    {
        // if moving joy stick
        Vector3 thumbstickPosition = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, Controller);
        bool thumbstickMoving = !Mathf.Approximately(thumbstickPosition.sqrMagnitude, 0);
        if (thumbstickMoving)
        {
            MakingContact = false;
            End = HitPoint = ControllerPosition + ControllerForward * distance + (ControllerUp * -1.0f) * dropHeight;

            RaycastHit hit;
            Vector3 last = Start;
            float recip = 1.0f / (float)(segments - 1);

            for (int i = 1; i < segments; ++i)
            {
                float t = (float)i * recip;
                Vector3 sample = SampleCurve(Start, End, Control, Mathf.Clamp01(t));

                if (Physics.Linecast(last, sample, out hit, ~excludeLayers))
                {
                    float angle = Vector3.Angle(Vector3.up, hit.normal);
                    if (angle < surfaceAngle)
                    {
                        HitPoint = hit.point;
                        Normal = hit.normal;
                        MakingContact = true;
                    }
                }

                last = sample;
            }
        }
    }

    Vector3 SampleCurve(Vector3 start, Vector3 end, Vector3 control, float time)
    {
        return Vector3.Lerp(Vector3.Lerp(start, control, time), Vector3.Lerp(control, end, time), time);
    }
}
