using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeImage : MonoBehaviour
{
	public RawImage rawImage;
	public Texture2D[] listOfImages;
	int index = 0;

	public void ChangeImages()
	{
		index++;
		if (index >= listOfImages.Length)
			index = 0;

		rawImage.texture = listOfImages[index];
	}
}
