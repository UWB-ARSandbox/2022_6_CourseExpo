using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TMProInputFieldClickPC : MonoBehaviour, IClickable {
    ForumManager forumManager;
    // Start is called before the first frame update
    void Start() {
        forumManager = transform.parent.transform.parent.GetComponent<ForumManager>();
    }

    public void IClickableClicked() {
        forumManager.OpenInput();
    }
}
