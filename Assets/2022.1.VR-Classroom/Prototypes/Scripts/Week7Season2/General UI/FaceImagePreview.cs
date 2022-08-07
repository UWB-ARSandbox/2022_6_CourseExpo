using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Script to grab the face image texture using the current player ID yada yada. Replaces the texture of 
// the object it's attached to (currently, the image componet of the "avatar preview" in the UI)
public class FaceImagePreview : MonoBehaviour
{
    public GameObject colorPreview;
    
    // These changes only need to be made once when the colorpicker UI is pulled up, instead of constant updates
    void OnEnable()
    {
        // Load the local player's current face image
        Texture2D t2D = FindObjectOfType<GameManager>().GetXpoPlayer().m_GhostPlayer.curFaceTexture;
        // Set this object's texture to that same texture
        GetComponent<Image>().sprite = Sprite.Create(t2D, new Rect(0.0f, 0.0f, t2D.width, t2D.height), new Vector2(0.5f, 0.5f));

        // Load the local player's current avatar color
        Color c = FindObjectOfType<XpoPlayer>().m_GhostPlayer.body.material.color;
        // Set the color preview object to that same color
        colorPreview.GetComponent<Image>().color = c;
    }
}
