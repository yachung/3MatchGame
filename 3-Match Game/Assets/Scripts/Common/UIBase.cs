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

        //UIBase �� ��ӹ޴� UI���� @ImgBackground ����̸��� ��ư�� �������
        //img�κ� ������ ���� �ʰ� �ؾ��ҋ��� (�˾��� �ƴ� ��üȭ�� ����� ȭ���ϰ��) 
        //btnImgBackground.onClick.RemoveAllListeners(); ���ش�
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

        //UIBase �� ��ӹ޴� UI���� @BlurBackground ����̸��� Blur��ư�� �������
        //Blur�κ� ������ ���� �ʰ� �ؾ��ҋ��� (�˾��� �ƴ� ��üȭ�� ����� ȭ���ϰ��) 
        //btnBlurBackground.onClick.RemoveAllListeners(); ���ش�
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

        //UIBase �� ��ӹ޴� UI���� @Frame ����̸��� ������ RectTransform �� �������
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
    //Show ȣ�� �� �Ź� ȣ�� �Ϲ����λ�Ȳ���� Initialize�� ���� ���� �� �ʱ�ȭ(Resource ���� x)
    public abstract void Initialize(Dictionary<UIOptionsKey, object> options);

    public void FinishedShow()
    {
        UpdateUI(eUpdateUIType.FinishedShow);
        gameObject.SetActive(true);

        //if (effBg != null)
        //{
        //    for (int i = 0; i < effBg.Length; i++)
        //    {
        //        effBg[i].gameObject.SetActive(true); // Ǯ���Ұ� �ƴ϶󼭰� �����ִٸ� �� ����
        //    }
        //}
    }

    public void Close()
    {
        OnClose();

        UIManager.Instance.Remove(uiName);
    }

    /// <summary>
    /// �������� Callback 
    /// Close�� UI������ �������� Hide�� UI�� ��Ȱ��ȭ �Ǿ������� ������
    /// UI1�� �����ʰ� UI2�� �ܹ��׸޴��� �������� OnClose�� ȣ����� ����
    /// </summary>
    public virtual void OnClose()
    {

    }

    #region Override Function
    /// <summary>
    /// UI�� �������� ���ѹ��� ȣ���ϴ� �Լ� Resource ���� �ȹ޴� Data ���� ����
    /// </summary>
    protected virtual void SetBaseData()
    {
        isPopup = false;
        isRefreshPrevUI = true;
    }


    /// <summary>
    /// ���� ���ſ�� ���� Ÿ�ֿ̹� ȣ�� Show->Initialize�� �������� FinishedShow���� ȣ��, �ٸ� UI�� �����鼭 TopUI�� ������ BackEntry���� ȣ�� �׿ܿ� ��Ʈ��ũ ������ ���ؼ��� ȣ��
    /// </summary>
    public virtual void UpdateUI(eUpdateUIType updateType = eUpdateUIType.None)
    {
        //ContentsManager.Instance.UpdateAllContentsUIObject(contentsType);
    }

    /// <summary>
    /// �ٸ� UI�� �����鼭 �ڷΰ��� ȣ��Ǵ� Callback
    /// </summary>
    public virtual void OnBackendUI()
    {
    }

    /// <summary>
    /// �ڷΰ���� �� ����� ó������� �Ұ͵��� ���ݴϴ�.
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
        //        effBg[i].gameObject.SetActive(true); // Ǯ���Ұ� �ƴ϶󼭰� �����ִٸ� �� ����
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
