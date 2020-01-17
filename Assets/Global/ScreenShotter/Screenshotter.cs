using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Screenshotter : MonoBehaviour {

    //public CameraClearFlags flags;
    public bool doUnityWay;
    public bool doJPEG;

    public string savePath;
    public string shotSirName;
    public bool capturing;
    public float captureInterval = 1f;

    public RenderTexture tempRenderTexture;

    private Texture2D tempTexture2D;
    private Camera camera;

    public int captureWidth = 1080;
    public int captureHeight = 720;

    private bool needsCapture = false;

	// Use this for initialization
	void Start () {
        camera = GetComponent<Camera>();
        if(tempRenderTexture==null)
            tempRenderTexture = new RenderTexture(captureWidth, captureHeight, 16, RenderTextureFormat.ARGB32);
        tempRenderTexture.antiAliasing = 4;
        tempTexture2D = new Texture2D(captureWidth, captureHeight, TextureFormat.ARGB32, false);

        InvokeRepeating("Capture", 0f, captureInterval);

        if (savePath == "")
        {
            savePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), Application.productName);
        }
	}

    private void Update()
    {
        if (Input.GetKeyDown("s") || OVRInput.GetDown(OVRInput.Button.One))
        {
            if (doUnityWay)
            {
                string filename = "ScreenShots/" + shotSirName + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
                ScreenCapture.CaptureScreenshot(filename, 4);
            }
            else
                capturing = true;
        }
    }

    void Capture()
    {
        if (!capturing) return;

        string filename = Path.Combine(savePath, shotSirName + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        filename += doJPEG ? ".jpg" : ".png";
        RenderTexture oldRenderTexture = camera.targetTexture;
        CameraClearFlags oldClearFlags = camera.clearFlags;

        camera.targetTexture = tempRenderTexture;
        camera.Render();
        RenderTexture.active = tempRenderTexture;

        tempTexture2D.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
        byte[] pngData = doJPEG ? tempTexture2D.EncodeToJPG() : tempTexture2D.EncodeToPNG();
        System.IO.File.WriteAllBytes(filename, pngData);
        Debug.Log(string.Format("Took screenshot to: {0}", filename));

        camera.targetTexture = oldRenderTexture;
        camera.clearFlags = oldClearFlags;


        capturing = false;
    }
}
