using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASL_SceneLoader : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="SceneToLoad">The name of the scene to load</param>
    public void ASL_LoadScene(string SceneToLoad)
    {
        ASL.ASLHelper.SendAndSetNewScene(SceneToLoad);
    }
}
