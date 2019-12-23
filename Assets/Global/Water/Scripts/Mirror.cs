using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode] // Make mirror live-update even when not in play mode
public class Mirror : MonoBehaviour
{
    public LayerMask reflectLayers;
    public int textureSize = 512;
    public float clipPlaneOffset = .03f;

    [SerializeField]
    public bool disablePixelLights = true;
    [SerializeField]
    [Tooltip("Mirror will use up to the anti-aliasing level specified in QualitySettings, without exceeding this value (1, 2, 4, or 8)")]
    public int maxAntiAliasing = 1;

    private class ReflectionData
    {
        public RenderTexture leftTexture;
        public RenderTexture rightTexture;
        public MaterialPropertyBlock propertyBlock;
    }
    private Dictionary<Camera, ReflectionData> m_Reflections = new Dictionary<Camera, ReflectionData>(); // Camera -> ReflectionData table

    private Camera mirrorCamera;
    private Skybox mirrorSkybox;

    private static bool s_InsideRendering = false;
    private static int LeftTexturePropertyID;
    private static int RightTexturePropertyID;

    private static readonly Rect DefaultRect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);

    private void OnValidate()
    {
        // Enfore only valid values for maxAntiAliasing
        if (maxAntiAliasing != 1 && maxAntiAliasing != 2 && maxAntiAliasing !=4 && maxAntiAliasing != 8)
        {
            maxAntiAliasing = 1;
        }
    }

    private void Awake()
    {
        RightTexturePropertyID = Shader.PropertyToID("_RightReflectionTex");
        LeftTexturePropertyID = Shader.PropertyToID("_LeftReflectionTex");
    }

    // This is called when it's known that this mirror will be rendered by some camera. We render reflections
    // and do other updates here. Because the script executes in edit mode, reflections for the scene view
    // camera will just work!
    public void OnWillRenderObject()
    {
        var rend = GetComponent<Renderer>();
        if (!enabled || !rend || !rend.enabled)
        {
            return;
        }

        Camera cam = Camera.current;
        if (!cam || cam == mirrorCamera)
        {
            return;
        }

        // Safeguard from recursive reflections.
        if (s_InsideRendering)
        {
            return;
        }
        s_InsideRendering = true;

        ReflectionData reflectionData = GetReflectionData(cam);

        // Optionally disable pixel lights for reflection
        int oldPixelLightCount = QualitySettings.pixelLightCount;
        if (disablePixelLights)
            QualitySettings.pixelLightCount = 0;

        UpdateCameraModes(cam);

        // Sure would be nice if we could automatically do stereo instanced rendering to a split texture
        if (cam.stereoEnabled)
        {
			#if UNITY_STANDALONE_WIN
			if (cam.stereoTargetEye == StereoTargetEyeMask.Both || cam.stereoTargetEye == StereoTargetEyeMask.Left)
			{
			Vector3 eyePos = cam.transform.TransformPoint(SteamVR.instance.eyes[0].pos);
			Quaternion eyeRot = cam.transform.rotation * SteamVR.instance.eyes[0].rot;
			Matrix4x4 projectionMatrix = GetSteamVRProjectionMatrix(cam, Valve.VR.EVREye.Eye_Left);

			RenderMirror(reflectionData.leftTexture, eyePos, eyeRot, projectionMatrix, DefaultRect);
			}

			if (cam.stereoTargetEye == StereoTargetEyeMask.Both || cam.stereoTargetEye == StereoTargetEyeMask.Right)
			{
			Vector3 eyePos = cam.transform.TransformPoint(SteamVR.instance.eyes[1].pos);
			Quaternion eyeRot = cam.transform.rotation * SteamVR.instance.eyes[1].rot;
			Matrix4x4 projectionMatrix = GetSteamVRProjectionMatrix(cam, Valve.VR.EVREye.Eye_Right);

			RenderMirror(reflectionData.rightTexture, eyePos, eyeRot, projectionMatrix, DefaultRect);
			}
			#else
			if (cam.stereoTargetEye == StereoTargetEyeMask.Both || cam.stereoTargetEye == StereoTargetEyeMask.Left)
			{
				Vector3 eyePos = UnityEngine.XR.InputTracking.GetLocalPosition( UnityEngine.XR.XRNode.LeftEye );
				Quaternion eyeRot = UnityEngine.XR.InputTracking.GetLocalRotation( UnityEngine.XR.XRNode.LeftEye );
				Matrix4x4 projectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);

				RenderMirror(reflectionData.leftTexture, eyePos, eyeRot, projectionMatrix, DefaultRect);
			}

			if (cam.stereoTargetEye == StereoTargetEyeMask.Both || cam.stereoTargetEye == StereoTargetEyeMask.Right)
			{
				Vector3 eyePos = UnityEngine.XR.InputTracking.GetLocalPosition( UnityEngine.XR.XRNode.RightEye );
				Quaternion eyeRot = UnityEngine.XR.InputTracking.GetLocalRotation( UnityEngine.XR.XRNode.RightEye );
				Matrix4x4 projectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

				RenderMirror(reflectionData.rightTexture, eyePos, eyeRot, projectionMatrix, DefaultRect);
			}
			#endif
        }
        else
        {
            RenderMirror(reflectionData.leftTexture, cam.transform.position, cam.transform.rotation, cam.projectionMatrix, DefaultRect);
        }

        // Apply the property block containing the appropriate reflection texture reference to this mirror's renderer
        rend.SetPropertyBlock(reflectionData.propertyBlock);

        // Restore pixel light count
        if (disablePixelLights)
            QualitySettings.pixelLightCount = oldPixelLightCount;

        s_InsideRendering = false;
    }

    void RenderMirror(RenderTexture targetTexture, Vector3 camPosition, Quaternion camRotation, Matrix4x4 camProjectionMatrix, Rect camViewport)
    {
        // Copy camera position/rotation/projection data into the reflectionCamera
        mirrorCamera.ResetWorldToCameraMatrix();
        mirrorCamera.transform.position = camPosition;
        mirrorCamera.transform.rotation = camRotation;
        mirrorCamera.projectionMatrix = camProjectionMatrix;
        mirrorCamera.targetTexture = targetTexture;
        mirrorCamera.rect = camViewport;

        // find out the reflection plane: position and normal in world space
        Vector3 pos = transform.position;
        Vector3 normal = transform.up;

        // Reflect camera around reflection plane
        Vector4 worldSpaceClipPlane = Plane(pos, normal, clipPlaneOffset);
        mirrorCamera.worldToCameraMatrix *= CalculateReflectionMatrix(worldSpaceClipPlane);

        // Setup oblique projection matrix so that near plane is our reflection
        // plane. This way we clip everything behind it for free.
        Vector4 cameraSpaceClipPlane = CameraSpacePlane(mirrorCamera, pos, normal, clipPlaneOffset);
        mirrorCamera.projectionMatrix = mirrorCamera.CalculateObliqueMatrix(cameraSpaceClipPlane);

        // Set camera position and rotation (even though it will be ignored by the render pass because we
        // have explicitly set the worldToCameraMatrix). We do this because some render effects may rely 
        // on the position/rotation of the camera.
//        mirrorCamera.transform.position = mirrorCamera.cameraToWorldMatrix.GetPosition();
//        mirrorCamera.transform.rotation = mirrorCamera.cameraToWorldMatrix.GetRotation();

        bool oldInvertCulling = GL.invertCulling;
        GL.invertCulling = !oldInvertCulling;

        mirrorCamera.Render();

        GL.invertCulling = oldInvertCulling;
    }

    // Cleanup all the objects we possibly have created
    void OnDisable()
    {
        if (mirrorCamera)
        {
            DestroyImmediate(mirrorCamera.gameObject);
            mirrorCamera = null;
        }

        foreach (ReflectionData reflectionData in m_Reflections.Values)
        {
            DestroyImmediate(reflectionData.leftTexture);
            if (reflectionData.rightTexture) DestroyImmediate(reflectionData.rightTexture);
        }
        m_Reflections.Clear();
    }


    private void UpdateCameraModes(Camera src)
    {
        // Lazy init the mirror camera
        if (!mirrorCamera)
        {
            GameObject go = new GameObject("MirrorCam" + gameObject.name, typeof(Camera), typeof(Skybox));
            go.hideFlags = HideFlags.HideAndDontSave | HideFlags.NotEditable;
            mirrorSkybox = go.GetComponent<Skybox>();
            mirrorCamera = go.GetComponent<Camera>();
            mirrorCamera.enabled = false;
            mirrorCamera.cullingMask = reflectLayers;
        }

        // set camera to clear the same way as current camera
        mirrorCamera.clearFlags = src.clearFlags;
        mirrorCamera.backgroundColor = src.backgroundColor;
        if (src.clearFlags == CameraClearFlags.Skybox)
        {
            Skybox sky = src.GetComponent(typeof(Skybox)) as Skybox;
            if (!sky || !sky.material)
            {
                mirrorSkybox.enabled = false;
            }
            else
            {
                mirrorSkybox.enabled = true;
                mirrorSkybox.material = sky.material;
            }
        }
        // update other values to match current camera.
        // even if we are supplying custom camera&projection matrices,
        // some of values are used elsewhere (e.g. skybox uses far plane)
        mirrorCamera.farClipPlane = src.farClipPlane;
        mirrorCamera.nearClipPlane = src.nearClipPlane;
        mirrorCamera.orthographic = src.orthographic;
        mirrorCamera.fieldOfView = src.fieldOfView;
        mirrorCamera.aspect = src.aspect;
        mirrorCamera.orthographicSize = src.orthographicSize;
    }

    // On-demand create any objects we need
    private ReflectionData GetReflectionData(Camera currentCamera)
    {
        ReflectionData reflectionData = null;
        if (!m_Reflections.TryGetValue(currentCamera, out reflectionData))
        {
            reflectionData = new ReflectionData();
            reflectionData.propertyBlock = new MaterialPropertyBlock();
            m_Reflections[currentCamera] = reflectionData;
        }

        int width = textureSize;
        int height = textureSize;
        int antiAliasing = Mathf.Min(QualitySettings.antiAliasing, maxAntiAliasing);

        // Apparently when anti-aliasing is turned off in the quality settings, the value is 0 rather than 1 as expected... :(
        antiAliasing = Mathf.Max(1, antiAliasing);

        if (!reflectionData.leftTexture || reflectionData.leftTexture.width != width || reflectionData.leftTexture.height != height || reflectionData.leftTexture.antiAliasing != antiAliasing)
        {
            if (reflectionData.leftTexture)
                DestroyImmediate(reflectionData.leftTexture);
            reflectionData.leftTexture = new RenderTexture(width, height, 24);
            reflectionData.leftTexture.antiAliasing = antiAliasing;
            reflectionData.leftTexture.hideFlags = HideFlags.DontSave;
            reflectionData.propertyBlock.SetTexture(LeftTexturePropertyID, reflectionData.leftTexture);
        }

        // For stereo cameras, we are going to render into a double-wide texture
        if (currentCamera.stereoEnabled)
        {
            if (!reflectionData.rightTexture || reflectionData.rightTexture.width != width || reflectionData.rightTexture.height != height || reflectionData.rightTexture.antiAliasing != antiAliasing)
            {
                if (reflectionData.rightTexture)
                    DestroyImmediate(reflectionData.rightTexture);
                reflectionData.rightTexture = new RenderTexture(width, height, 24);
                reflectionData.rightTexture.antiAliasing = antiAliasing;
                reflectionData.rightTexture.hideFlags = HideFlags.DontSave;
                reflectionData.propertyBlock.SetTexture(RightTexturePropertyID, reflectionData.rightTexture);
            }
        } else
        {
            reflectionData.propertyBlock.SetTexture(RightTexturePropertyID, reflectionData.leftTexture);
        }

        return reflectionData;
    }

    private static Vector4 Plane(Vector3 pos, Vector3 normal, float offset)
    {
        return new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(pos, normal) - offset);
    }

    // Given position/normal of the plane, calculates plane in camera space.
    private static Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float offset)
    {
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(pos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized;
        return Plane(cpos, cnormal, offset);
    }

    // Calculates reflection matrix around the given plane
    private static Matrix4x4 CalculateReflectionMatrix(Vector4 plane)
    {
        Matrix4x4 reflectionMat = Matrix4x4.identity;

        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;

        return reflectionMat;
    }
	#if UNITY_STANDALONE_WIN
    private static Matrix4x4 GetSteamVRProjectionMatrix(Camera cam, Valve.VR.EVREye eye)
    {
        Valve.VR.HmdMatrix44_t proj = SteamVR.instance.hmd.GetProjectionMatrix(eye, cam.nearClipPlane, cam.farClipPlane);
        Matrix4x4 m = new Matrix4x4();
        m.m00 = proj.m0;
        m.m01 = proj.m1;
        m.m02 = proj.m2;
        m.m03 = proj.m3;
        m.m10 = proj.m4;
        m.m11 = proj.m5;
        m.m12 = proj.m6;
        m.m13 = proj.m7;
        m.m20 = proj.m8;
        m.m21 = proj.m9;
        m.m22 = proj.m10;
        m.m23 = proj.m11;
        m.m30 = proj.m12;
        m.m31 = proj.m13;
        m.m32 = proj.m14;
        m.m33 = proj.m15;
        return m;
    }
	#endif
}