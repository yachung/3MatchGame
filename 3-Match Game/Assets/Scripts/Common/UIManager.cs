using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Const;

public class UIManager : MonoSingleton<UIManager>
{
    Dictionary<SceneType, List<UIBase>> uiStack = new Dictionary<SceneType, List<UIBase>>(); // ���� ui stack


    //UICamera & Canvas
    Camera uiCamera = null;
    public Camera UICamera => uiCamera;
    EventSystem eventSystem = null;
    public EventSystem EventSystem => eventSystem;
    StandaloneInputModule standardInputModule = null;
    public StandaloneInputModule StandardInputModule => standardInputModule;

    float distanceMax = 2000f;
    float distanceOffset = 100f;
    int sortBase = 200;
    int sortOffset = 100;

    private bool isShowingUIDone = false; // �˾�â ����

    // �ػ� 16:9 ��������, ���ΰ� �� �� �ܸ��⿡ �����ϱ� ���� ����
    // Ex) 24:9�� �ػ󵵸� ���� �ܸ���    (1920 * 720) : ScreenWidthRatio = 1.5  <== �ִ� ���� �ػ�
    // Ex) 16:9�� �ػ󵵸� ���� �ܸ���    (1280 * 720) : ScreenWidthRatio = 1
    // Ex) 18:9�� �ػ󵵸� ���� �ܸ���                 : ScreenWidthRatio = 1.125
    // Ex) 19.5:9�� �ػ󵵸� ���� �ܸ���  (1560 * 720) : ScreenWidthRatio = 1.218
    // Ex) 4:3�� �ػ󵵸� ���� �ܸ���     (1280 * 960) : ScreenWidthRatio = 0.75
    public Vector2 ScreenSize = Vector2.zero;
    public Vector2 UsableScreenSize = Vector2.zero; // ��밡���� ScreenSize ( 1������ ���� (ex:pad) ��� ���͹ڽ������� ��밡���� ������ ĳ�� )
    public Vector2 ScreenGapSize = Vector2.zero; // ��ũ�������� - ��밡���ѻ�����
    private double? _screenWidthRatio;
    public double ScreenWidthRatio // ��ũ�� ����
    {
        get
        {
            if (_screenWidthRatio.HasValue == false)
                RefreshScreenWidthRatio();

            return _screenWidthRatio.Value;
        }
    }

    private bool? _isHeightMatchUI;
    // CancasScaler Match Value
    public bool IsHeightMatchUI
    {
        get
        {
            if (_screenWidthRatio < 1)
            {
                _isHeightMatchUI = true;
            }
            else
                _isHeightMatchUI = false;

            return _isHeightMatchUI.Value;
        }
    }

    private bool isScreenLock = false;
    /// <summary>
    /// ȭ�� ��ġ ��������
    /// </summary>
    public bool IsScreenLock
    {
        get { return isScreenLock; }
        set { isScreenLock = value; }
    }

    private bool isESCLock = false;
    /// <summary>
    /// esc������ �ݱ� ��� ��������
    /// </summary>
    public bool IsESCLock
    {
        get { return isESCLock; }
        set { isESCLock = value; }
    }

    // Start is called before the first frame update
    void Awake()
    {
        base.Awake();

        SetUICamera(); // UICamera ����
        //SetBottomCanvas(); // ���ϴܿ� ��ġ�� BottomCanvas ����. ������
        //SetUILight(); // UICharacter ���� Light ����
        //SetDefaultData(); // UI �������� ������ ����. �÷���, �˾����� Ʈ���� ��

        eventSystem = gameObject.AddComponent<EventSystem>();
        eventSystem.sendNavigationEvents = false;
        standardInputModule = gameObject.AddComponent<StandaloneInputModule>();
        //StartCoroutine(coUpdate());
    }

    void SetUICamera() // UICamera ����
    {
        GameObject cameraObj = new GameObject("@UICamera");
        cameraObj.transform.SetParent(transform);
        uiCamera = cameraObj.AddComponent<Camera>();
        uiCamera.clearFlags = CameraClearFlags.Depth;
        uiCamera.orthographic = true;
        uiCamera.orthographicSize = 36f;
        uiCamera.farClipPlane = distanceMax + 100; // ��������� ���������ϴ¹����� �־ ���� ��Ժ�������
        uiCamera.depth = 1;

        ResetUICameraPosition();
        //ResetUICameraCullingMask();
    }

    void ResetUICameraPosition() // UICamera ������ ����
    {
        uiCamera.transform.localPosition = new Vector3(0f, 0f, -700f);
    }

    /// <summary>
    /// ��ũ�� ����, ������, ��밡�ɹ����� ����
    /// </summary>
    public void RefreshScreenWidthRatio()
    {
        // ��Ȥ ȭ���� ȸ���Ǳ� ���� ��ũ�� ����� �޾ƿ��� ��찡 �����Ƿ� ����ó�� ���ش�.
        double width = Mathf.Max(Screen.width, Screen.height);
        double height = Mathf.Min(Screen.width, Screen.height);
        //_screenWidthRatio = Mathf.Max(1f, width / (height * 16f / 9f));
        _screenWidthRatio = width / (height * 16 / 9);

        Rect rt = Screen.safeArea;

        ScreenSize = new Vector2((float)width, (float)height);
        Debug.Log("ScreenSize = " + ScreenSize);

        UsableScreenSize = ScreenSize;
        if (IsHeightMatchUI)
        {
            float usableHeight = ScreenSize.x / 16f * 9f;
            UsableScreenSize = new Vector2(ScreenSize.x, usableHeight);
        }

        float fixRatio = (float)System.Math.Round(_screenWidthRatio.Value, 3);
        Debug.Log("ScreenWidthRatio = " + _screenWidthRatio.Value + " -> " + fixRatio);

        if (fixRatio > 1.5f)
            UsableScreenSize = new Vector2(UsableScreenSize.x / fixRatio * 1.5f, UsableScreenSize.y);
        Debug.Log("UsableScreenSize = " + UsableScreenSize);

        ScreenGapSize = ScreenSize - UsableScreenSize;
        Debug.Log("ScreenGapSize = " + ScreenGapSize);

        _screenWidthRatio = fixRatio;
    }

    public void SetDefaultUICanvas(UIBase ui) // ui�� �⺻ Canvas�� ����
    {
        SetScreenMatchValue(ui.UICanvasScaler);

        ui.UICanvas.renderMode = RenderMode.ScreenSpaceCamera;
        ui.UICanvas.worldCamera = uiCamera;
        ui.UICanvas.planeDistance = distanceMax;
        ui.UICanvas.sortingOrder = sortBase;
    }

    public void SetScreenMatchValue(CanvasScaler cs) // ui�� Canvas MatchValue����
    {
        if (ScreenWidthRatio < 1) // 1280 * 720 (16:9) �������� ���ΰ� ������
            cs.matchWidthOrHeight = 0f;
        else                    // 1280 * 720�̰ų� ���ΰ� �� ���� ���̵��
            cs.matchWidthOrHeight = 1f;
    }

    /// <returns>UI �̸����� �ֻ������ üũ</returns>
    public bool IsTopUI(string uiName)
    {
        //return uiStack.Count > 0 && string.Equals(PeekUIStack().GetType().FullName, uiName);
        return GetUIStack().Count > 0 && string.Equals(PeekUIStack().uiName, uiName);
    }

    

    #region UIStack
    
    /// <returns>uiStack ����Ʈ�� ���� ������</returns>
    public List<UIBase> GetUIStack()
    {
        if (uiStack.ContainsKey(SceneManager.Instance.currentScene) == false)
            uiStack.Add(SceneManager.Instance.currentScene, new List<UIBase>());

        return uiStack[SceneManager.Instance.currentScene];
    }

    public void RemoveAllUIStack()
    {
        for (int i = 0; i < GetUIStack().Count; i++)
        {
            RemoveUIStack(GetUIStack()[i].uiName);
        }
    }

    public void RemoveUIStack(string uiName)
    {
        UIBase findUI = null;
        findUI = GetUIStack().Find(x => x.uiName.Equals(uiName));

        if (findUI != null)
        {
            GetUIStack().Remove(findUI);
            SortAllUICanvas(); // ��ü uiStack Canvas ������
        }
    }

    void PushUIStack(UIBase ui)
    {
        RemoveUIStack(ui.uiName);

        GetTopUI(true)?.OnBackendUI();

        GetUIStack().Insert(0, ui);
        SortAllUICanvas(); // ��ü uiStack Canvas ������
    }

    public void InsertUIStack(UIBase ui, UIBase targetUI, Dictionary<UIOptionsKey, object> options = null) // uiStack �� insert
    {
        RemoveUIStack(ui.uiName); // stack�� �̹� �����ϸ� ��

        GetTopUI(true)?.OnBackendUI();

        GetUIStack().Insert(GetUIStack().IndexOf(targetUI), ui); // insert
        SortAllUICanvas();  // ��ü uiStack Canvas ������

        //���ҽ� �ε� ���� �ȹ޴� �Լ�
        //ui.SetFrameBackground(options);
        ui.SetBaseDataAtFirst(); //���� ���� �� �ʿ��� ������ ����
        ui.Initialize(options); //�� ui�� ������ �ʱ�ȭ

        //���ҽ� �ε� �� ����, ����
        //ui.SetUIResources(() =>
        {
            ui.FinishedShow(); // FinishedShow : ���ҽ� �ε� �Ϸ� �� ui ���� ������ �� Ȱ��ȭ
        }
        //);
    }

    UIBase PopUIStack()
    {
        UIBase ui = null;
        if (GetUIStack().Count > 0)
        {
            ui = GetUIStack()[0];
            GetUIStack().RemoveAt(0);
        }
        return ui;
    }

    /// <summary>
    /// UIPopup�� �����ؼ� �ֻ�� UIBase����Ʈ
    /// </summary>
    /// <returns></returns>
    List<UIBase> PeekUIBaseList()
    {
        if (GetUIStack().Count <= 0)
            return null;

        List<UIBase> result = new List<UIBase>();

        for (int i = 0; i < GetUIStack().Count; i++)
        {
            result.Add(GetUIStack()[i]);
            if (GetUIStack()[i].isPopup == false)
            {
                break;
            }
        }

        return result;
    }
    UIBase PeekUIStack() // �ֻ��� ui peek
    {
        return GetUIStack().Count > 0 ? GetUIStack()[0] : null;
    }

    UIBase PeekUIStack(int index) // index�� uiStack�� ui������
    {
        if (GetUIStack().Count > 0)
        {
            if (GetUIStack().Count > index)
                return GetUIStack()[index];
            else
                return GetUIStack()[0];
        }
        else
        {
            return null;
        }
    }
    public void Remove(string uiName) // �̸����� uiStack�� ui����
    {
        UIBase ui = GetUI(uiName);

        if (ui != null)
        {
            ui.Hide();
            RemoveUIStack(uiName);
            //if(!ui.isPopup)
            //{
            //    if (!cachedUINames.Contains(uiName))
            //    {
            //        cachedUINames.Add(uiName);
            //    }
            //    else
            //    {
            //        cachedUINames.Remove(uiName);
            //        cachedUINames.Add(uiName);
            //    }
            //    if (cachedUINames.Count > MaxCachedUICount)    // ĳ�� UI �ƽ�ġ �Ѿ��
            //    {
            //        string destroyUiName = cachedUINames[0];
            //        if (!uiStack.Contains(cachedUIs[destroyUiName]))    // ���� �������� ���� UI��(uiStack�� ���°Ÿ�) ���� (Back�� ���� �� �� �ֱ⶧���� �����ϸ� �ȵȴ�)
            //        {
            //            cachedUINames.RemoveAt(0);
            //            GameObject obj = cachedUIs[destroyUiName].gameObject;
            //            cachedUIs.Remove(destroyUiName);
            //            Destroy(obj);
            //        }
            //    }
            //}


            List<UIBase> prevUIList = PeekUIBaseList();
            if (prevUIList != null)
            {

                if (prevUIList.Count > 0)
                {

                    if (ui.isRefreshPrevUI)
                    {
                        for (int i = 0; i < prevUIList.Count; i++)
                        {
                            prevUIList[i]?.BackEntry();
                        }
                    }

                    //���� �������� UI�� �˾��ϰ�� ���� TOP UI�� �˾��� ��� �˾��� PlayBgm�� �����Ų��.
                    //if (ui is UIPopupBase)
                    //{
                    //    UIBase prevTopUI = GetTopUI(true) as UIPopupBase;

                    //    if (prevTopUI != null)
                    //    {
                    //        prevTopUI.PlayBGM();
                    //    }

                    //    return;
                    //}

                    //PlayPreviousBGM();

                }
            }
        }
    }

    public UIBase GetUI(string uiName) // �̸����� uiStack�� ui������
    {
        for (int i = 0, count = GetUIStack().Count; i < count; ++i)
        {
            if (string.Equals(GetUIStack()[i].uiName, uiName))
            {
                return GetUIStack()[i];
            }
        }
        return null;
    }

    //private void PlayPreviousBGM()
    //{
    //    // ���� �˾������� ���� 1������ ���������� �������� �κ��ϰ� ���� ���� ���̽��� ����������
    //    // bgm ������ 1�ʰ����� �ش�.
    //    bool delayBgm = false;
    //    if (GetUIStack().Count == 1)
    //    {
    //        if (GetUIStack()[0].contentsType == ContentsType.Lobby)
    //            delayBgm = true;
    //    }

    //    UIBase topUI = GetTopUI();
    //    if (topUI != null)
    //        topUI.PlayBGM(delayBgm ? 1f : 0); // ���� ui PlayBGM

    //    ShowLobbyEffectMask(); // �κ�� LobbyEffectMask Show
    //}

    //public void RemoveAll() // �ֻ��UI�� ������ uiStack �� ��� UI����. 
    //{
    //    if (GetUIStack().Count > 1)
    //    {
    //        //������ UI ���� UIStack ���� ����.
    //        var tempUIStack = new List<UIBase>(GetUIStack().ToArray()); //���� ����(���������͸� ����ؼ� ���ſ� ������ ���ÿ� �ϸ� �����ϹǷ�)
    //        for (int i = 0; i < tempUIStack.Count - 1; i++)
    //        {
    //            UIBase ui = GetUI(tempUIStack[i].uiName);

    //            ui.Hide();
    //            RemoveUIStack(ui.name);
    //        }

    //        //��� ������ �ֻ��� UI ó��(�Ƹ� �κ�UI/����UI/����UI���� ������ ����)
    //        UIBase topUI = GetTopUI();
    //        if (topUI != null)
    //        {
    //            if (topUI.isRefreshPrevUI)
    //                topUI.BackEntry();

    //            topUI.PlayBGM();

    //            ShowLobbyEffectMask();
    //        }
    //    }
    //}

    /// <returns>uiStack�� ���� �ֻ���� ui�� ������</returns>
    public UIBase GetTopUI(bool isIncludePopup = false) // 
    {
        UIBase ui = null;
        int idx = 0;
        int count = 0;
        while (idx < GetUIStack().Count && ui == null && count++ < 200)
        {
            if (!isIncludePopup && GetUIStack()[idx].isPopup)
            {
                idx++;
                continue;
            }
            ui = GetUIStack()[idx];
        }
        return ui;
    }

    public T GetTopUI<T>(bool isIncludePopup = false) where T : UIBase
    {
        UIBase ui = GetTopUI(isIncludePopup);
        object temp = null;
        try
        {
            temp = System.Convert.ChangeType(ui, typeof(T));
        }
        catch
        {
            temp = null;
        }

        return temp as T;
    }
    #endregion

    public void SortAllUICanvas() // uiStack�� ��ü canvas sortingOrder ������
    {
        int orderIdx = 0;
        for (int i = 0; i < GetUIStack().Count; i++)
        {
            if (GetUIStack()[i].isIgnoreSortingOrder == true)
            {
                continue;
            }

            GetUIStack()[i].UICanvas.planeDistance = distanceMax - (GetUIStack().Count - 1 - orderIdx) * distanceOffset; // stack�ε����� ���� ī�޶�Ÿ� ����
            GetUIStack()[i].UICanvas.sortingOrder = sortBase + (GetUIStack().Count - 1 - orderIdx) * sortOffset; // stack�ε����� ���� ��Ʈ�� ����

            //if (GetUIStack()[i].EffBg != null) // ����Ʈ ��� ��������� ����Ʈ�� ��Ʈ ������
            //{
            //    for (int j = 0; j < GetUIStack()[i].EffBg.Length; j++)
            //    {
            //        GetUIStack()[i].EffBg[j].baseSortOrder = GetUIStack()[i].UICanvas.sortingOrder;
            //        GetUIStack()[i].EffBg[j].Apply();
            //    }
            //}

            for (int j = 0; j < GetUIStack()[i].UISubCanvas.Count; j++) // ui���� �ٸ� Sub Canvas���� �����ϸ� �ش� Canvas�鵵 stack�ε����� �°� ��Ʈ�� ����
            {
                GetUIStack()[i].UISubCanvas[j].sortingOrder = sortBase + (GetUIStack().Count - 1 - orderIdx) * sortOffset;
                GetUIStack()[i].UISubCanvas[j].sortingOrder += GetUIStack()[i].UISubCanvasOriginSort[j];
            }
            orderIdx++;
        }
    }
}
