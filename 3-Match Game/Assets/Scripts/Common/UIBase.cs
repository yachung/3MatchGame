using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Const;

public abstract class UIBase : MonoBehaviour
{
    public enum eUpdateUIType
    {
        None,
        FinishedShow,
        BackEntry,
        NetworkError,
    }

    protected Canvas uiCanvas;
    protected CanvasScaler uiCanvasScaler;
    protected List<Canvas> uiSubCanvas;
    protected List<int> uiSubCanvasOriginSort;
    protected RectTransform rect;

    protected Image imgBackground;
    protected Button btnBackground;
    protected RectTransform rtFrame;
    protected CanvasGroup cgFrame;
    [HideInInspector] public bool ignoreHide = false;
    [HideInInspector] public bool isIgnoreSortingOrder = false;
    [HideInInspector] public bool isPopup = false;
    public Canvas UICanvas => uiCanvas;
    public CanvasScaler UICanvasScaler => uiCanvasScaler;
    public List<Canvas> UISubCanvas => uiSubCanvas;
    public List<int> UISubCanvasOriginSort => uiSubCanvasOriginSort;
    public RectTransform Rect => rect;
    public string uiName { get; set; }
    public bool isRefreshPrevUI { get; set; }

    bool isSetBaseData = false;

    protected virtual void Awake()
    {
        uiCanvas = this.GetComponent<Canvas>();
        uiCanvasScaler = this.GetComponent<CanvasScaler>();
        Canvas[] canvasArr = this.GetComponentsInChildren<Canvas>(true);
        uiSubCanvas = new List<Canvas>();
        uiSubCanvasOriginSort = new List<int>();
        for (int i = 0; i < canvasArr.Length; i++)
        {
            if (canvasArr[i] == uiCanvas)
                continue;
            uiSubCanvas.Add(canvasArr[i]);
            uiSubCanvasOriginSort.Add(canvasArr[i].sortingOrder);
        }

        rect = this.GetComponent<RectTransform>();

        UIManager.Instance.SetDefaultUICanvas(this);

        GameObject bgObj = null;
        if (this.transform.Find("ImgBackground") != null)
            bgObj = this.transform.Find("ImgBackground").gameObject;
        else if (this.transform.Find("@ImgBackground") != null)
            bgObj = this.transform.Find("@ImgBackground").gameObject;
        else if (this.transform.Find("@BlurBackground") != null)
            bgObj = this.transform.Find("@BlurBackground").gameObject;

        //UIBase 를 상속받는 UI에서 @ImgBackground 라는이름의 버튼이 있을경우
        //img부분 눌러서 닫지 않게 해야할떄는 (팝업이 아닌 전체화면 블러배경 화면일경우) 
        //btnImgBackground.onClick.RemoveAllListeners(); 해준다
        if (this.transform.Find("@ImgBackground") != null)
        {
            imgBackground = this.transform.Find("@ImgBackground").GetComponent<Image>();
            btnBackground = this.transform.Find("@ImgBackground").GetComponent<Button>();
            if (btnBackground != null)
            {
                btnBackground.onClick.RemoveAllListeners();
                btnBackground.onClick.AddListener(OnClickClose);
            }
        }

        //UIBase 를 상속받는 UI에서 @BlurBackground 라는이름의 Blur버튼이 있을경우
        //Blur부분 눌러서 닫지 않게 해야할떄는 (팝업이 아닌 전체화면 블러배경 화면일경우) 
        //btnBlurBackground.onClick.RemoveAllListeners(); 해준다
        //if (this.transform.Find("@BlurBackground") != null)
        //{
        //    imgBlurBackground = this.transform.Find("@BlurBackground").GetComponent<Image>();
        //    btnBlurBackground = this.transform.Find("@BlurBackground").GetComponent<Button>();
        //    if (btnBlurBackground != null)
        //    {
        //        btnBlurBackground.onClick.RemoveAllListeners();
        //        btnBlurBackground.onClick.AddListener(OnClickClose);
        //    }
        //    matBlurBackground = Instantiate(imgBlurBackground.material);
        //    imgBlurBackground.material = matBlurBackground;
        //}

        //UIBase 를 상속받는 UI에서 @Frame 라는이름의 프레임 RectTransform 이 있을경우
        if (this.transform.Find("@Frame") != null)
        {
            rtFrame = this.transform.Find("@Frame").GetComponent<RectTransform>();
            cgFrame = rtFrame.GetComponent<CanvasGroup>();
            if (cgFrame == null)
                cgFrame = rtFrame.gameObject.AddComponent<CanvasGroup>();
        }

        isIgnoreSortingOrder = false;
    }

    public void SetBaseDataAtFirst()
    {
        if (isSetBaseData == true)
            return;

        isSetBaseData = true;
        //ReddotManager.Instance.SetContentsType(contentsType);

        SetBaseData();
    }
    //Show 호출 시 매번 호출 일반적인상황에서 Initialize를 통해 각종 값 초기화(Resource 영향 x)
    public abstract void Initialize(Dictionary<UIOptionsKey, object> options);

    public void FinishedShow()
    {
        UpdateUI(eUpdateUIType.FinishedShow);
        gameObject.SetActive(true);

        //if (effBg != null)
        //{
        //    for (int i = 0; i < effBg.Length; i++)
        //    {
        //        effBg[i].gameObject.SetActive(true); // 풀링할게 아니라서걍 껏다켯다만 할 예정
        //    }
        //}
    }

    public void Close()
    {
        OnClose();

        UIManager.Instance.Remove(uiName);
    }

    /// <summary>
    /// 닫혔을때 Callback 
    /// Close는 UI상으로 닫혔을때 Hide는 UI가 비활성화 되었을때로 구분함
    /// UI1을 닫지않고 UI2를 햄버그메뉴로 열었을때 OnClose가 호출되지 않음
    /// </summary>
    public virtual void OnClose()
    {

    }

    #region Override Function
    /// <summary>
    /// UI가 생성된후 단한번만 호출하는 함수 Resource 영향 안받는 Data 정보 세팅
    /// </summary>
    protected virtual void SetBaseData()
    {
        isPopup = false;
        isRefreshPrevUI = true;
    }


    /// <summary>
    /// 각종 갱신요소 갱신 타이밍에 호출 Show->Initialize가 끝난이후 FinishedShow에서 호출, 다른 UI가 닫히면서 TopUI로 왔을때 BackEntry에서 호출 그외에 네트워크 에러를 통해서도 호출
    /// </summary>
    public virtual void UpdateUI(eUpdateUIType updateType = eUpdateUIType.None)
    {
        //ContentsManager.Instance.UpdateAllContentsUIObject(contentsType);
    }

    /// <summary>
    /// 다른 UI가 열리면서 뒤로갈때 호출되는 Callback
    /// </summary>
    public virtual void OnBackendUI()
    {
    }

    /// <summary>
    /// 뒤로가기로 재 입장시 처리해줘야 할것들을 해줍니다.
    /// </summary>
    /// 
    public virtual void BackEntry()
    {
        gameObject.SetActive(true);
        UpdateUI(eUpdateUIType.BackEntry);

        //if (effBg != null)
        //{
        //    for (int i = 0; i < effBg.Length; i++)
        //    {
        //        effBg[i].gameObject.SetActive(true); // 풀링할게 아니라서걍 껏다켯다만 할 예정
        //    }
        //}

        //uiLinkNode.ChangeFirst();
    }

    public virtual void Hide()
    {
        Debug.Log(string.Format("{0} Hide", uiName));
        if (this != null)
        {
            //if (effBg != null)
            //{
            //    for (int i = 0; i < effBg.Length; i++)
            //    {
            //        effBg[i].gameObject.SetActive(false);
            //    }
            //}

            gameObject.SetActive(false);
            //UIManager.Instance.HideToolTip();
        }
    }

    public virtual void OnClickClose()
    {
        OnClickClose(isRefreshBackUI: true);
    }

    public virtual void OnClickClose(bool isRefreshBackUI)
    {
        if (!UIManager.Instance.IsTopUI(uiName)) return;

        //AudioManager.Instance.PlayClickSound();
        Close();
    }
    #endregion
}
