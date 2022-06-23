using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PageController : MonoBehaviour
{
    public TextMeshProUGUI txtObjInfo;
    private int currentPage = 1;
    private int totalPages;
    public Button btnPrevious;
    public Button btnNext;

    // Start is called before the first frame update
    void Start()
    {
        txtObjInfo = GetComponent<TextMeshProUGUI>();
        totalPages = txtObjInfo.textInfo.pageCount;
        txtObjInfo.pageToDisplay = 1;

        foreach (Button b in GetComponentsInChildren<Button>()) {
            if (b.name == "btn_Previous") {
                btnPrevious = b;
                btnPrevious.onClick.AddListener(PrevPage);
                btnPrevious.gameObject.SetActive(false);
            } else {
                btnNext = b;
                btnNext.onClick.AddListener(NextPage);
            }
        }
    }

    private void PrevPage() {
        if (currentPage > 1) {
            txtObjInfo.pageToDisplay--;
            currentPage--;
            btnNext.gameObject.SetActive(true);
        }
        if (currentPage == 1) {
            btnPrevious.gameObject.SetActive(false);
        }
    }

    private void NextPage() {
        totalPages = txtObjInfo.textInfo.pageCount;
        if (currentPage < totalPages) {
            txtObjInfo.pageToDisplay++;
            currentPage++;
            btnPrevious.gameObject.SetActive(true);
        }
        if (currentPage == totalPages) {
            btnNext.gameObject.SetActive(false);
        }
    }
}
