using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhoneCamera : MonoBehaviour
{

    private bool isCamera;
    private WebCamTexture cameraTexture;
    private Texture bckgDefault;
    private static Texture2D boxOutlineTexture;

    public Color colorTag1 = new Color(0.3843137f, 0, 0.9333333f, 0.1f);
    public Color colorTag2 = new Color(0.3843137f, 0, 0.9333333f, 0.1f);
    public Color colorTag3 = new Color(0.3843137f, 0, 0.9333333f, 0.1f);

    public RawImage bckg;
    public AspectRatioFitter fit;
    public Yolov5Detector yolov5Detector;
    public GameObject boxContainer;
    public GameObject boxPrefab;
    public GameObject background;

    public int WINDOW_SIZE = 416;

    private int framesCount = 0;
    private float timeCount = 0.0f;
    private float refreshTime = 1.0f;



    // Start is called before the first frame update
    void Start()
    {
		QualitySettings.vSyncCount = 1;
		bckgDefault = bckg.texture;
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            isCamera = false;
            return;
        }

        for (int i = 0; i < devices.Length; i++)
        {
            if (!devices[i].isFrontFacing)
                cameraTexture = new WebCamTexture(devices[i].name, WINDOW_SIZE, WINDOW_SIZE);
        }

        if (cameraTexture == null)
        {
            if (devices.Length != 0)
                cameraTexture = new WebCamTexture(devices[0].name, WINDOW_SIZE, WINDOW_SIZE);
            else
            {
                isCamera = false;
                return;
            }
            
        }

        float ratio = ((RectTransform)background.transform).rect.width / 640;
        boxContainer.transform.localScale = new Vector2(ratio, ratio);

		float scaleY = cameraTexture.videoVerticallyMirrored ? -1f : 1f;
		bckg.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

		int orient = -cameraTexture.videoRotationAngle;
		bckg.rectTransform.localEulerAngles = new Vector3(0, 0, orient);

		//ProcessImg();

	}
	public void UseCamera()
	{
		isCamera = true;
		cameraTexture.Play();
		bckg.texture = cameraTexture;

	}
	public void OffCamera()
	{
		if(isCamera == true)
		{
			cameraTexture.Stop();
			bckg.texture = null;
			isCamera = false;
		}
	}
	void RunDetection(Texture2D result)
	{
		yolov5Detector.Detect(result.GetPixels32(), result.width, boxes =>
		{
			Resources.UnloadUnusedAssets();

			foreach (Transform child in boxContainer.transform)
			{
				Destroy(child.gameObject);
			}

			for (int i = 0; i < boxes.Count; i++)
			{
				Debug.Log(boxes[i].ToString());
				GameObject newBox = Instantiate(boxPrefab);
				newBox.name = boxes[i].Label + " " + boxes[i].Confidence;
				newBox.GetComponent<Image>().color =  colorTag3;
				newBox.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = boxes[i].Confidence.ToString();
				newBox.transform.SetParent(boxContainer.transform);
				newBox.transform.localPosition = new Vector3(boxes[i].Rect.x - WINDOW_SIZE / 2, boxes[i].Rect.y - WINDOW_SIZE / 2);
				newBox.transform.localScale = new Vector2(boxes[i].Rect.width / 100, boxes[i].Rect.height / 100);
			}
		});
	}
	public void ProcessImg()
	{
		Texture2D newTex = (Texture2D)bckg.texture;
		RenderTexture rt = new RenderTexture(WINDOW_SIZE, WINDOW_SIZE, 24);
		RenderTexture.active = rt;
		Graphics.Blit(newTex, rt);
		Texture2D result = new Texture2D(WINDOW_SIZE, WINDOW_SIZE);
		result.ReadPixels(new Rect(0, 0, WINDOW_SIZE, WINDOW_SIZE), 0, 0);
		result.Apply();

		RunDetection(result);
	}
    // Update is called once per frame
    void Update()
    {
        if (!isCamera)
            return;

		Texture2D tx2d = new Texture2D(cameraTexture.width, cameraTexture.height);
		// Gets all color data from web cam texture and then Sets that color data in texture2d
		tx2d.SetPixels(cameraTexture.GetPixels());
		// Applying new changes to texture2d
		tx2d.Apply();
		
		RenderTexture rt = new RenderTexture(WINDOW_SIZE, WINDOW_SIZE, 24);
		RenderTexture.active = rt;
		Graphics.Blit(tx2d, rt);
		Texture2D result = new Texture2D(WINDOW_SIZE, WINDOW_SIZE);
		result.ReadPixels(new Rect(0, 0, WINDOW_SIZE, WINDOW_SIZE), 0, 0);
		result.Apply();

		RunDetection(result);
	}

}
