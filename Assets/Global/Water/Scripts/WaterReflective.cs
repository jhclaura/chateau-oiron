using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace UnityStandardAssets.Water
{
    [ExecuteInEditMode] // Make water live-update even when not in play mode
    public class WaterReflective : MonoBehaviour
    {
        public bool disablePixelLights = true;
        public int textureSize = 256;
        public float clipPlaneOffset = 0.07f;
        public LayerMask reflectLayers = -1;
        
        private Dictionary<Camera, Camera> m_ReflectionCameras = new Dictionary<Camera, Camera>(); // Camera -> Camera table
        private Dictionary<Camera, Camera> m_RefractionCameras = new Dictionary<Camera, Camera>(); // Camera -> Camera table
        private RenderTexture m_ReflectionTexture0;
        private RenderTexture m_ReflectionTexture1;
        private RenderTexture m_RefractionTexture;
        private int m_OldReflectionTextureSize;
        private int m_OldRefractionTextureSize;
        private static bool s_InsideWater;

		private float pupilDistance;
		private Vector3 NECK_OFFSET = new Vector3(0, 0.075f, 0.08f);
        
		private void Start()
		{
			pupilDistance = Vector3.Distance(
                InputTracking.GetLocalPosition(XRNode.LeftEye),
                InputTracking.GetLocalPosition(XRNode.RightEye)
            ) / 2f;

#if UNITY_EDITOR
            Shader.EnableKeyword("WATER_EDITOR");
            Shader.DisableKeyword("WATER_NON_EDITOR");
#else
            Shader.EnableKeyword("WATER_NON_EDITOR");
            Shader.DisableKeyword("WATER_EDITOR");
#endif
        }

        // This is called when it's known that the object will be rendered by some
        // camera. We render reflections / refractions and do other updates here.
        // Because the script executes in edit mode, reflections for the scene view
        // camera will just work!
        public void OnWillRenderObject()
        {
            if (!enabled || !GetComponent<Renderer>() || !GetComponent<Renderer>().sharedMaterial ||
                !GetComponent<Renderer>().enabled)
            {
                return;
            }

            Camera cam = Camera.current;
            if (!cam)
            {
                return;
            }

            // Safeguard from recursive water reflections.
            if (s_InsideWater)
            {
                return;
            }
            s_InsideWater = true;

            Camera reflectionCamera, refractionCamera;
            CreateWaterObjects(cam, out reflectionCamera, out refractionCamera);

            // find out the reflection plane: position and normal in world space
            Vector3 pos = transform.position;
            Vector3 normal = transform.up;

            // Optionally disable pixel lights for reflection/refraction
            int oldPixelLightCount = QualitySettings.pixelLightCount;
            if (disablePixelLights)
            {
                QualitySettings.pixelLightCount = 0;
            }

            UpdateCameraModes(cam, reflectionCamera);
            UpdateCameraModes(cam, refractionCamera);

            // Render reflection
            if (cam.stereoEnabled)
            {
                if (cam.stereoTargetEye == StereoTargetEyeMask.Both || cam.stereoTargetEye == StereoTargetEyeMask.Left)
                {
                    //Vector3 eyePos = cam.transform.TransformPoint(new Vector3(-0.5f * cam.stereoSeparation, 0, 0));
                    Matrix4x4 projectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);

                    // GVR
                    Vector3 eyePos = cam.transform.TransformPoint(Vector3.left * pupilDistance);
                    //Vector3 eyePos = (cam.transform.localRotation * (NECK_OFFSET + Vector3.left * pupilDistance) - NECK_OFFSET.y * Vector3.up) * cam.transform.lossyScale.y;
                    //eyePos += cam.transform.parent.position;

                    RenderReflection(reflectionCamera, m_ReflectionTexture0, eyePos, cam.transform.rotation, projectionMatrix);
                    // GVR === swapping "_ReflectionTex0" to be "_ReflectionTex1" WTF?!
                    GetComponent<Renderer>().sharedMaterial.SetTexture("_ReflectionTex1", m_ReflectionTexture0);
                }

                if (cam.stereoTargetEye == StereoTargetEyeMask.Both || cam.stereoTargetEye == StereoTargetEyeMask.Right)
                {
                    //Vector3 eyePos = cam.transform.TransformPoint(new Vector3(0.5f * cam.stereoSeparation, 0, 0));
                    Matrix4x4 projectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

                    // GVR
                    Vector3 eyePos = cam.transform.TransformPoint(Vector3.right * pupilDistance);
                    //Vector3 eyePos = (cam.transform.localRotation * (NECK_OFFSET + Vector3.right * pupilDistance) - NECK_OFFSET.y * Vector3.up) * cam.transform.lossyScale.y;
                    //eyePos += cam.transform.parent.position;

                    RenderReflection(reflectionCamera, m_ReflectionTexture1, eyePos, cam.transform.rotation, projectionMatrix);
                    // GVR === swapping "_ReflectionTex1" to be "_ReflectionTex0" WTF?!
                    GetComponent<Renderer>().sharedMaterial.SetTexture("_ReflectionTex0", m_ReflectionTexture1);
                }
            }
            else
            {
                RenderReflection(reflectionCamera, m_ReflectionTexture0, cam.transform.position, cam.transform.rotation, cam.projectionMatrix);
                GetComponent<Renderer>().sharedMaterial.SetTexture("_ReflectionTex0", m_ReflectionTexture0);
            }

            // Restore pixel light count
            if (disablePixelLights)
            {
                QualitySettings.pixelLightCount = oldPixelLightCount;
            }

            s_InsideWater = false;
        }
        void RenderReflection(Camera reflectionCamera, RenderTexture targetTexture, Vector3 camPos, Quaternion camRot, Matrix4x4 camProjMatrix)
        {
            // Copy camera position/rotation/reflection into the reflectionCamera
            reflectionCamera.ResetWorldToCameraMatrix();
            reflectionCamera.transform.position = camPos;
            reflectionCamera.transform.rotation = camRot;
            reflectionCamera.projectionMatrix = camProjMatrix;
            reflectionCamera.targetTexture = targetTexture;
            reflectionCamera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);

            // Set custom culling matrix from the current camera
            // reflectionCamera.cullingMatrix = camProjMatrix * reflectionCamera.worldToCameraMatrix;

            // find out the reflection plane: position and normal in world space
            Vector3 pos = transform.position;
            Vector3 normal = transform.up;

            // Reflect camera around reflection plane
            float d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

            Matrix4x4 reflection = Matrix4x4.zero;
            CalculateReflectionMatrix(ref reflection, reflectionPlane);

            reflectionCamera.worldToCameraMatrix *= reflection;

            // Setup oblique projection matrix so that near plane is our reflection
            // plane. This way we clip everything below/above it for free.
            Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
            reflectionCamera.projectionMatrix = reflectionCamera.CalculateObliqueMatrix(clipPlane);

            // Set camera position and rotation
            reflectionCamera.transform.position = reflectionCamera.cameraToWorldMatrix.GetColumn(3);
            reflectionCamera.transform.rotation = Quaternion.LookRotation(reflectionCamera.cameraToWorldMatrix.GetColumn(2), reflectionCamera.cameraToWorldMatrix.GetColumn(1));

            // never render water layer
            reflectionCamera.cullingMask = ~(1 << 4) & reflectLayers.value;

            bool oldCulling = GL.invertCulling;
            GL.invertCulling = !oldCulling;
            reflectionCamera.Render();
            GL.invertCulling = oldCulling;
        }


        // Cleanup all the objects we possibly have created
        void OnDisable()
        {
            if (m_ReflectionTexture0)
            {
                DestroyImmediate(m_ReflectionTexture0);
                m_ReflectionTexture0 = null;
            }
            if (m_ReflectionTexture1)
            {
                DestroyImmediate(m_ReflectionTexture1);
                m_ReflectionTexture1 = null;
            }
            if (m_RefractionTexture)
            {
                DestroyImmediate(m_RefractionTexture);
                m_RefractionTexture = null;
            }
            foreach (var kvp in m_ReflectionCameras)
            {
                DestroyImmediate((kvp.Value).gameObject);
            }
            m_ReflectionCameras.Clear();
            foreach (var kvp in m_RefractionCameras)
            {
                DestroyImmediate((kvp.Value).gameObject);
            }
            m_RefractionCameras.Clear();
        }


        // This just sets up some matrices in the material; for really
        // old cards to make water texture scroll.
        void Update()
        {
            if (!GetComponent<Renderer>())
            {
                return;
            }
            Material mat = GetComponent<Renderer>().sharedMaterial;
            if (!mat)
            {
                return;
            }

            Vector4 waveSpeed = mat.GetVector("WaveSpeed");
            float waveScale = mat.GetFloat("_WaveScale");
            Vector4 waveScale4 = new Vector4(waveScale, waveScale, waveScale * 0.4f, waveScale * 0.45f);

            // Time since level load, and do intermediate calculations with doubles
            double t = Time.timeSinceLevelLoad / 20.0;
            Vector4 offsetClamped = new Vector4(
                (float)Math.IEEERemainder(waveSpeed.x * waveScale4.x * t, 1.0),
                (float)Math.IEEERemainder(waveSpeed.y * waveScale4.y * t, 1.0),
                (float)Math.IEEERemainder(waveSpeed.z * waveScale4.z * t, 1.0),
                (float)Math.IEEERemainder(waveSpeed.w * waveScale4.w * t, 1.0)
                );

            mat.SetVector("_WaveOffset", offsetClamped);
            mat.SetVector("_WaveScale4", waveScale4);
        }

        void UpdateCameraModes(Camera src, Camera dest)
        {
            if (dest == null)
            {
                return;
            }
            // set water camera to clear the same way as current camera
            dest.clearFlags = src.clearFlags;
            dest.backgroundColor = src.backgroundColor;
            if (src.clearFlags == CameraClearFlags.Skybox)
            {
                Skybox sky = src.GetComponent<Skybox>();
                Skybox mysky = dest.GetComponent<Skybox>();
                if (!sky || !sky.material)
                {
                    mysky.enabled = false;
                }
                else
                {
                    mysky.enabled = true;
                    mysky.material = sky.material;
                }
            }
            // update other values to match current camera.
            // even if we are supplying custom camera&projection matrices,
            // some of values are used elsewhere (e.g. skybox uses far plane)
            dest.farClipPlane = src.farClipPlane;
            dest.nearClipPlane = src.nearClipPlane;
            dest.orthographic = src.orthographic;
            if (!UnityEngine.XR.XRDevice.isPresent)
                dest.fieldOfView = src.fieldOfView;
            dest.aspect = src.aspect;
            dest.orthographicSize = src.orthographicSize;
        }


        // On-demand create any objects we need for water
        void CreateWaterObjects(Camera currentCamera, out Camera reflectionCamera, out Camera refractionCamera)
        {
            reflectionCamera = null;
            refractionCamera = null;

            // Reflection render texture
            if (!m_ReflectionTexture0 || m_OldReflectionTextureSize != textureSize)
            {
                if (m_ReflectionTexture0)
                {
                    DestroyImmediate(m_ReflectionTexture0);
                }
                m_ReflectionTexture0 = new RenderTexture(textureSize, textureSize, 16);
                m_ReflectionTexture0.name = "__WaterReflection" + GetInstanceID();
                m_ReflectionTexture0.isPowerOfTwo = true;
                m_ReflectionTexture0.hideFlags = HideFlags.DontSave;
            }
            if (currentCamera.stereoEnabled && (!m_ReflectionTexture1 || m_OldReflectionTextureSize != textureSize))
            {
                if (m_ReflectionTexture1)
                {
                    DestroyImmediate(m_ReflectionTexture1);
                }
                m_ReflectionTexture1 = new RenderTexture(textureSize, textureSize, 16);
                // m_ReflectionTexture1.name = "__WaterReflection1" + GetInstanceID();
                m_ReflectionTexture1.isPowerOfTwo = true;
                m_ReflectionTexture1.hideFlags = HideFlags.DontSave;
            }
            m_OldReflectionTextureSize = textureSize;

            // Camera for reflection
            m_ReflectionCameras.TryGetValue(currentCamera, out reflectionCamera);
            if (!reflectionCamera) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
            {
                GameObject go = new GameObject("Water Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
                reflectionCamera = go.GetComponent<Camera>();
                reflectionCamera.enabled = false;
                reflectionCamera.transform.position = transform.position;
                reflectionCamera.transform.rotation = transform.rotation;
                reflectionCamera.gameObject.AddComponent<FlareLayer>();
                go.hideFlags = HideFlags.HideAndDontSave;
                m_ReflectionCameras[currentCamera] = reflectionCamera;
            }
        }

        // Given position/normal of the plane, calculates plane in camera space.
        Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + normal * clipPlaneOffset;
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        // Calculates reflection matrix around the given plane
        static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMat.m01 = (- 2F * plane[0] * plane[1]);
            reflectionMat.m02 = (- 2F * plane[0] * plane[2]);
            reflectionMat.m03 = (- 2F * plane[3] * plane[0]);

            reflectionMat.m10 = (- 2F * plane[1] * plane[0]);
            reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMat.m12 = (- 2F * plane[1] * plane[2]);
            reflectionMat.m13 = (- 2F * plane[3] * plane[1]);

            reflectionMat.m20 = (- 2F * plane[2] * plane[0]);
            reflectionMat.m21 = (- 2F * plane[2] * plane[1]);
            reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMat.m23 = (- 2F * plane[3] * plane[2]);

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;
        }
    }
}
