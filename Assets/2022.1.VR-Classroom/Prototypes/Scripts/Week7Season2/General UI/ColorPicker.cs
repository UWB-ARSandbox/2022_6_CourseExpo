using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

// Limitation: Does not seem to work in VR due to how expo UI is handled. Preset color buttons serve as an alternative for now.

// Much credit to PabloMakes for the following tutorial: https://www.youtube.com/watch?v=rKhFYxUNL6A
// I have modified his implementation below and attempted to explain everything that may be confusing.

[Serializable]
public class ColorEvent : UnityEvent<Color> { }
// By inheriting from UnityEvent, this ^ can be called/used within the Unity Inspector window.

public class ColorPicker : MonoBehaviour
{
    public ColorEvent OnColorPreview;
    public ColorEvent OnColorSelect;
    RectTransform Rect;
    Texture2D ColorTexture;

    void Start()
    {
        Rect = GetComponent<RectTransform>();
        ColorTexture = GetComponent<Image>().mainTexture as Texture2D;
    }

    void Update()
    {
        // Check to see whether mouse position is over the color picker image or not
        if (RectTransformUtility.RectangleContainsScreenPoint(Rect, Input.mousePosition))
        {
            Vector2 delta;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect, Input.mousePosition, null, out delta);

            float width = Rect.rect.width;
            float height = Rect.rect.height;
            delta += new Vector2(width * .5f, height * .5f);

            float x = Mathf.Clamp(delta.x / width, 0f, 1f);
            float y = Mathf.Clamp(delta.y / height, 0f, 1f);

            int texX = Mathf.RoundToInt(x * ColorTexture.width);
            int texY = Mathf.RoundToInt(y * ColorTexture.height);

            // Get the actual color from the cursor's placement on the image
            Color color = ColorTexture.GetPixel(texX, texY);

            // The '?' is a shorthand version of a null check for our event
            OnColorPreview?.Invoke(color);

            if (Input.GetMouseButtonDown(0))
            {
                // '?' null check used once again
                OnColorSelect?.Invoke(color);
            }
        }
    }

    public void SetUserColor(Color color)
    {
            // Update player color
            FindObjectOfType<XpoPlayer>().ColorUser(color);
    }
}
