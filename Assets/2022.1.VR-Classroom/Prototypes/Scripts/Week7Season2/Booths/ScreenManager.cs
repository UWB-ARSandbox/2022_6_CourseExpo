using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using ASL;
using CourseXpo;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Extensions.SceneTransitions;
using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using ContentType = ContentManager.ContentType;
using UnityEngine.XR;

public class ScreenManager : MonoBehaviour, IClickable
{
    /*
     * New model for content manager:
     *
     * lists of different types of content objects within
     * the single booth, say, multiple screens,
     * or multiple grabbable objects or even a mix of the two
     * (or more) types of content in a single booth
     *  
     */
    public static bool IsAnyScreenAttached = false;
    public bool Completed { 
        get 
        { 
            switch (contentType) {
                case ContentType.content: return contentStarted && reachedEnd;
                case ContentType.example: return contentStarted && completedProblem && reachedEnd;
                default: return false;
            } 
        } 
    }

    private bool completedProblem = false;

    private bool contentStarted = false;

    private bool reachedEnd = false;

    //Start
    public GameObject pnl_Start;
    public TextMeshProUGUI txt_ContentName;
    public TextMeshProUGUI txt_ContentDesc;

    //Main Content Display
    public GameObject pnl_TableContent;
    public Image img_ContentImage;
    private image image_curCachedImage;
    private Texture2D txtr_toConvert;
    public TextMeshProUGUI txt_ContentText;
    public TextMeshProUGUI txt_ProblemText;

    //Example Navigation
    public TextMeshProUGUI txt_ExampleName;
    public GameObject pnl_ExampleNavigation;
    public Button btn_NavigateProblem;
    public Button btn_NavigateSolution;
    public Button btn_NavigateBack;

    //Content Scrolling
    public Button btn_Next;
    public Button btn_Last;

    //Media
    public GameObject pnl_MediaGridView;
    public Image[] img_linkPreview = new Image[4];
    public TextMeshProUGUI[] txt_links = new TextMeshProUGUI[4];
    public Button btn_LastPage;
    public Button btn_NextPage;

    //Discussion
    public GameObject pnl_DiscussionView;
    //TODO figure out how to display discussions in a meaningful way

    private List<object>
        Content = new List<object>(); //content object list has no restriction for future-proofing

    private List<object>
        Content2 = null; //for Problem/Solution

    private List<object>
        _curListObjects = null;

    private Dictionary<image, Sprite> cachedImages = new Dictionary<image, Sprite>();
    private ContentManager.ContentType contentType;
    private int num_lengthTotalContent = -1;

    private int _contentIterator = -1;
    private Type lastType = null;
    private bool _screenAttached = false;
    private BoothManager boothManager;
    public Button btn_Link;

    [Range(0.0f, 5.0f)]
    public float _lengthFromFace = 2.25f;

    [Range(0.5f, 5.0f)]
    public float _distanceFromGround = 1.23f;
    private GameObject _firstPersonPlayer = null;

    void Start()
    {
        LinkObjects();
        boothManager = GetComponentInParent<BoothManager>();
        StartCoroutine(LoadImage());
    }


    void FixedUpdate()
    {
        if (_firstPersonPlayer == null) _firstPersonPlayer = GameObject.Find("FirstPersonPlayer(Clone)");
        //do the screen attaching
        if (_screenAttached)
        {
            if (XRSettings.isDeviceActive)
            {
                transform.position =
                    _firstPersonPlayer.transform.position + Vector3.up * 0.9f +
                    _firstPersonPlayer.transform.forward * _lengthFromFace;
            } else {
                transform.position =
                    Camera.main.transform.position +
                    Camera.main.transform.forward * 1.75f;
            }
        }
        else
        {
            transform.localPosition = Vector3.up * _distanceFromGround;
        }
        //do the screen player look-at tracking
        if (Camera.main != null)
        {
            //find the look direction from the position of the screen to the player
            var templookDir = -(Camera.main.transform.position - transform.position).normalized;
            //we do not want the screen to care about up and down rotation
            if (!_screenAttached) templookDir.y = 0;
            transform.forward = templookDir;

        }
        GetComponent<Collider>().isTrigger = _screenAttached;
    }

    public void StartContent()
    {
        if (!boothManager.boothVerified) return;
        //'attach' the screen to move with the player's camera view; is interesting when combined with moving around, more of a bug than a feature
        AttachToPlayer();
        //if we've already started the content, no need to call the start code
        if (contentStarted) return;

        if (!contentStarted) contentStarted = true;

        ResetContent();

        switch (contentType)
        {
            case ContentType.example:
                StartExample();
                //this requires more state than the others because it describes steps to solve a problem, hence Problem and Solution
                break;

            case ContentType.media:
                //still using the content scrolling, but now using it to keep track of the end of a page of links
                ViewMedia();
                break;

            case ContentType.content:
                //the framework for viewing the in-order content
                ViewContent();
                break;

            case ContentType.discussion:
                //Persistent text chat about a set of content
                ViewDiscussion();
                break;

            default:
                Debug.LogError("Content Type not recognized. If you believe this is in error, please contact the Expo administrator.");
                return;
        }
    }

    private void AttachToPlayer()
    {
        if (!XRSettings.isDeviceActive) Cursor.lockState = CursorLockMode.None;
        _screenAttached = true;
        IsAnyScreenAttached = true;
    }

    /// <summary>
    /// Detaches all LookAt screens from the player; used to call from a static context the nonstatic singular DetachScreenFromPlayer()
    /// </summary>
    public static void DetachScreensFromPlayer()
    {
        foreach (var contentManager in FindObjectsOfType<ScreenManager>())
        {
            contentManager.DetachScreenFromPlayer();
        }
    }

    /// <summary>
    /// Detaches the screen from in front of the player's view
    /// </summary>
    public void DetachScreenFromPlayer()
    {
        _screenAttached = false;
        IsAnyScreenAttached = false;
        if (!XRSettings.isDeviceActive) Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Loads up the next in-order Content from the relevant list
    /// </summary>
    private void QueueContent(int delta = 0)
    {
        _contentIterator += delta;

        switch (contentType)
        {
            case ContentType.content:
                if (_contentIterator == _curListObjects.Count - 1) reachedEnd = true;
                break;
            case ContentType.example:
                if (_contentIterator == _curListObjects.Count - 1 && _curListObjects.Count == Content.Count) completedProblem = true;
                if (_contentIterator == _curListObjects.Count - 1 && _curListObjects.Count == Content2.Count) reachedEnd = true;
                break;
        }

        if (_curListObjects.Count == Content.Count && contentType == ContentType.example && Content[0] is string str)
        {
            txt_ProblemText.text = "Problem: " + str;
            txt_ProblemText.rectTransform.anchoredPosition = Vector2.zero;
            return;
        }

        switch (_curListObjects[_contentIterator])
        {
            case string txt:
                if (txt != txt_ContentText.text) txt_ContentText.text = txt;
                break;

            case image img:
                //do http request if we don't have the image cached
                if (!cachedImages.ContainsKey(img))
                {
                    GetImage(img);
                }

                image_curCachedImage = img;

                if (delta > 0 && _contentIterator + delta < _curListObjects.Count)
                {
                    //if we are not at the end, look if the next item is text, and if so, display it too
                    //otherwise don't do anything
                    if (_curListObjects[_contentIterator + delta] is string s)
                    {
                        QueueContent(delta);
                    }
                }

                break;

            case video v:
                //load video into container object
                break;

            case null:
                Debug.LogError("Invalid Content Access!");
                break;
        }

    }

    /// <summary>
    /// Starts the Example in the booth
    /// </summary>
    private void StartExample()
    {
        pnl_Start.SetActive(false);

        //pnl_TableContent.SetActive(true); obsolete; start on the navigation buttons
        pnl_ExampleNavigation.SetActive(true);
        btn_NavigateBack.gameObject.SetActive(true);
    }

    /// <summary>
    /// Starts the Discussion in the booth
    /// </summary>
    private void ViewDiscussion()
    {
        pnl_Start.SetActive(false);
        //todo implement discussion in meaningful way
    }

    private void ViewContent()
    {
        pnl_Start.SetActive(false);
        _curListObjects = Content;
        pnl_TableContent.SetActive(true);
        QueueContent(1);
        UpdateContentControls();
    }

    private void ViewMedia()
    {
        pnl_Start.SetActive(false);

        pnl_MediaGridView.SetActive(true);
    }

    private void LinkObjects()
    {
        foreach (CanvasRenderer obj in GetComponentsInChildren<CanvasRenderer>(true))
        {
            switch (obj.gameObject.name)
            {
                case "pnl_Start":
                    pnl_Start = obj.gameObject;
                    pnl_Start.SetActive(true);
                    break;
                case "pnl_TableContent":
                    pnl_TableContent = obj.gameObject;
                    pnl_TableContent.SetActive(false);
                    break;
                case "pnl_MediaGridView":
                    pnl_MediaGridView = obj.gameObject;
                    pnl_MediaGridView.SetActive(false);
                    break;
                case "pnl_ExampleNavigation":
                    pnl_ExampleNavigation = obj.gameObject;
                    pnl_ExampleNavigation.SetActive(false);
                    break;

                case "txt_ContentName":
                    txt_ContentName = obj.GetComponent<TextMeshProUGUI>();
                    txt_ContentName.gameObject.SetActive(true);
                    break;
                case "txt_ContentDesc":
                    txt_ContentDesc = obj.GetComponent<TextMeshProUGUI>();
                    txt_ContentDesc.gameObject.SetActive(true);
                    break;

                case "txt_ContentText":
                    txt_ContentText = obj.GetComponent<TextMeshProUGUI>();
                    txt_ContentText.gameObject.SetActive(true);
                    break;

                case "txt_ProblemText":
                    txt_ProblemText = obj.GetComponent<TextMeshProUGUI>();
                    txt_ProblemText.gameObject.SetActive(false);
                    break;

                case "img_ContentImage":
                    img_ContentImage = obj.GetComponent<Image>();
                    img_ContentImage.gameObject.SetActive(true);
                    break;

                case "btn_Next":
                    btn_Next = obj.GetComponent<Button>();
                    btn_Next.onClick.AddListener(Next);
                    btn_Next.gameObject.SetActive(true);
                    break;

                case "btn_Last":
                    btn_Last = obj.GetComponent<Button>();
                    btn_Last.onClick.AddListener(Last);
                    btn_Last.gameObject.SetActive(true);
                    break;

                case "btn_Link":
                    btn_Link = obj.GetComponent<Button>();
                    btn_Link.onClick.AddListener(OpenCurrentLink);
                    btn_Link.gameObject.SetActive(false);
                    break;

                case "btn_Problem":
                    btn_NavigateProblem = obj.GetComponent<Button>();
                    btn_NavigateProblem.onClick.AddListener(NavigateProblem);
                    btn_NavigateProblem.gameObject.SetActive(true);
                    break;

                case "btn_Solution":
                    btn_NavigateSolution = obj.GetComponent<Button>();
                    btn_NavigateSolution.onClick.AddListener(NavigateSolution);
                    btn_NavigateSolution.gameObject.SetActive(false);
                    break;

                case "btn_Back":
                    btn_NavigateBack = obj.GetComponent<Button>();
                    btn_NavigateBack.onClick.AddListener(NavigateBack);
                    btn_NavigateBack.gameObject.SetActive(false);
                    break;

                case "txt_ExampleName":
                    txt_ExampleName = obj.GetComponent<TextMeshProUGUI>();
                    txt_ExampleName.gameObject.SetActive(true);
                    break;

                case "btn_NextPage":
                    btn_LastPage = obj.GetComponent<Button>();
                    btn_LastPage.onClick.AddListener(LastPage);
                    btn_LastPage.gameObject.SetActive(true);
                    break;
                case "btn_LastPage":
                    btn_NextPage = obj.GetComponent<Button>();
                    btn_NextPage.onClick.AddListener(NextPage);
                    btn_NextPage.gameObject.SetActive(true);
                    break;

                default:
                    break;
            }
        }
    }

    private void OpenCurrentLink()
    {
        Application.OpenURL(txt_ContentText.text);
    }

    private void LastPage()
    {
        //use content iterator to flip through 4 link and their previews at a time

    }

    private void NextPage()
    {

    }

    //Navigate back to the Problem/Solution choice menu
    private void NavigateBack()
    {
        pnl_TableContent.SetActive(false);
        pnl_ExampleNavigation.SetActive(true);
        btn_NavigateSolution.gameObject.SetActive(true);
        _contentIterator = -1;
        ResetContent();
    }

    //Navigate to the problem of the example
    private void NavigateSolution()
    {
        pnl_TableContent.SetActive(true);
        pnl_ExampleNavigation.SetActive(false);
        txt_ProblemText.rectTransform.localPosition = Vector2.up * 1338;
        txt_ContentText.gameObject.SetActive(true);
        img_ContentImage.gameObject.SetActive(true);
        btn_NavigateBack.gameObject.SetActive(false);
        _curListObjects = Content2;
        QueueContent(1);
        UpdateContentControls();
    }

    //Navigate to the problem of the example
    private void NavigateProblem()
    {
        pnl_TableContent.SetActive(true);
        txt_ProblemText.gameObject.SetActive(true);
        txt_ContentText.gameObject.SetActive(false);
        img_ContentImage.gameObject.SetActive(false);
        pnl_ExampleNavigation.SetActive(false);
        _curListObjects = Content;
        QueueContent(1);
        UpdateContentControls();
    }

    private void GetImage(image img)
    {
        StartCoroutine(DownloadImage(img));
    }

    IEnumerator DownloadImage(image img)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(img.link);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("WebRequest Error!: " + www.error);
        }
        else
        {
            txtr_toConvert = DownloadHandlerTexture.GetContent(www);
        }
    }

    void ResetContent()
    {
        txt_ContentText.text = String.Empty;
        img_ContentImage.sprite = Sprite.Create(new Texture2D(1, 1), new Rect(0, 0, 1, 1), 0.5f * Vector2.one, 1);
        img_ContentImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,1);
        img_ContentImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,1);
    }

    //Loads the current image when it is done downloading
    private IEnumerator LoadImage()
    {
        while (true)
        {
            //when the current image is not null
            while (image_curCachedImage == null) yield return new WaitForSecondsRealtime(0.5f);

            //this coroutine will load it into the cachedImages dictionary
            //but if it does exist it wont do that
            if (!cachedImages.ContainsKey(image_curCachedImage))
            {
                //when the texture is downloaded from the other coroutine
                while (txtr_toConvert == null) yield return new WaitForSecondsRealtime(0.5f);

                var imgRect = new Rect(0, 0, txtr_toConvert.width, txtr_toConvert.height);

                cachedImages[image_curCachedImage] = Sprite.Create(txtr_toConvert, imgRect, 0.5f * Vector2.one, 100);

                txtr_toConvert = null;
            }

            //but either way it will set the img_ContentImage
            //sprite to image_curCachedImage
            img_ContentImage.sprite = cachedImages[image_curCachedImage];
            var tempWidth = cachedImages[image_curCachedImage].textureRect.width;
            var tempHeight = cachedImages[image_curCachedImage].textureRect.height;

            var tempRatio = tempHeight / tempWidth;
            if (tempRatio > 1)
            {
                tempRatio = 1 / tempRatio;
                tempHeight = tempRatio;
                tempWidth = 1.0f;
            } else
            {
                tempWidth = tempRatio;
                tempHeight = 1.0f;
            }


            img_ContentImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tempWidth * 2500f);
            img_ContentImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tempHeight * 2500f);

            image_curCachedImage = null;
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    void Next()
    {
        if (_contentIterator < _curListObjects.Count) QueueContent(1);
        UpdateContentControls();
    }

    void Last()
    {
        if (_contentIterator > 0) QueueContent(-1);
        UpdateContentControls();
    }

    void UpdateContentControls()
    {
        btn_Last.gameObject.SetActive(_contentIterator != 0);
        btn_Next.gameObject.SetActive(_contentIterator != _curListObjects.Count - 1);
        btn_Link.gameObject.SetActive(_curListObjects[_contentIterator] is links);
    }

    public void LoadContent(string name, List<object> content, List<object> content2 = null, ContentManager.ContentType type = ContentManager.ContentType.content)
    {
        Content = content;
        Content2 = content2;
        contentType = type;
        txt_ContentName.text = name;
        switch (contentType)
        {
            case ContentType.example:
                txt_ExampleName.text = name;
                txt_ContentDesc.text = "Example: Problem & Solution";
                break;
            case ContentType.content:
                txt_ContentDesc.text = "Content Slides";
                break;
            case ContentType.media:
                txt_ContentDesc.text = "Media Links";
                break;
            case ContentType.discussion:
                txt_ContentText.text = "Content to Discuss";
                break;
        }
    }

    public void IClickableClicked()
    {
        if (boothManager.boothVerified) StartContent();
    }
}
