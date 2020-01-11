using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTeleporter : MonoBehaviour
{
    public OVRInput.Controller controller;
    public GameObject dot;
    public Transform player;
    public Transform cameraEye;
    public LayerMask floorLayer;
    //
    public Color color;
    public Material pointerMaterial;
    public float thickness = 0.002f;
    public GameObject holder;
    public GameObject pointer;
    public bool addRigidBody = false;
    //
    private bool teleportModeOn = false;
    private bool isTeleporting;

	void Start ()
	{
		if (dot.activeSelf)
			dot.SetActive (false);

        holder = new GameObject();
        holder.name = "PointerHolder";
        holder.transform.parent = this.transform;
        holder.transform.localPosition = Vector3.zero;
        holder.transform.localRotation = Quaternion.identity;

        pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pointer.name = "Pointer";
        pointer.transform.parent = holder.transform;
        pointer.transform.localScale = new Vector3(thickness, thickness, 100f);
        pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
        pointer.transform.localRotation = Quaternion.identity;
        BoxCollider b_collider = pointer.GetComponent<BoxCollider>();
        if (addRigidBody)
        {
            if (b_collider)
            {
                b_collider.isTrigger = true;
            }
            Rigidbody rigidBody = pointer.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
        }
        else
        {
            if (b_collider)
            {
                Destroy(b_collider);
            }
        }

        if (pointerMaterial == null)
        {
            pointerMaterial = new Material(Shader.Find("Unlit/Color"));
            pointerMaterial.SetColor("_Color", color);
        }
        pointer.GetComponent<MeshRenderer>().material = pointerMaterial;

        //floorLayer = 1 << 8;

        holder.SetActive(false);
    }

    private void Update()
    {
        if (isTeleporting) return;

        Vector3 thumbstickPosition = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, controller);
        bool shouldBePointing = !Mathf.Approximately(thumbstickPosition.sqrMagnitude, 0);
        //bool thumbstickDown = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, controller);
        if (!shouldBePointing)
        {            
            if (teleportModeOn)
            {
                teleportModeOn = false;
                DoFadeTeleport();
                Debug.Log("teleport!");
                return;
            }
            else
            {
                return;
            }
        }

        Ray raycast = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(raycast, out hit, 100f, floorLayer))
        {
            teleportModeOn = true;
            dot.transform.position = hit.point;
            dot.SetActive(true);

        }
        else
        {
            teleportModeOn = false;
            dot.SetActive(false);
        }
    }

	public void DoFadeTeleport()
	{
        isTeleporting = true;
        EnvironmentManager.Instance.FadeOut(Color.black, 0.3f);
		Invoke ("Teleport", 0.3f);
	}

	void Teleport()
	{
		Vector3 eyePos = cameraEye.localPosition;
		eyePos.y = 0;
		player.position = dot.transform.position - eyePos;

		dot.SetActive (false);
        EnvironmentManager.Instance.FadeIn(0.3f);
		Invoke ("Reset", 0.3f);
	}

	void Reset()
	{
        isTeleporting = false;
	}
}
