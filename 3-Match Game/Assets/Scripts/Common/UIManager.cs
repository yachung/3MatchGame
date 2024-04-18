using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Const;

public class UIManager : MonoSingleton<UIManager>
{
    Dictionary<SceneType, List<UIBase>> uiStack = new Dictionary<SceneType, List<UIBase>>(); // 열린 ui stack


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

    private bool isShowingUIDone = false; // 팝업창 제외

    // 해상도 16:9 기준으로, 가로가 더 긴 단말기에 대응하기 위한 변수
    // Ex) 24:9의 해상도를 가진 단말기    (1920 * 720) : ScreenWidthRatio = 1.5  <== 최대 대응 해상도
    // Ex) 16:9의 해상도를 가진 단말기    (1280 * 720) : ScreenWidthRatio = 1
    // Ex) 18:9의 해상도를 가진 단말기                 : ScreenWidthRatio = 1.125
    // Ex) 19.5:9의 해상도를 가진 단말기  (1560 * 720) : ScreenWidthRatio = 1.218
    // Ex) 4:3의 해상도를 가진 단말기     (1280 * 960) : ScreenWidthRatio = 0.75
    public Vector2 ScreenSize = Vector2.zero;
    public Vector2 UsableScreenSize = Vector2.zero; // 사용가능한 ScreenSize ( 1이하의 비율 (ex:pad) 경우 레터박스제외한 사용가능한 사이즈 캐싱 )
    public Vector2 ScreenGapSize = Vector2.zero; // 스크린사이즈 - 사용가능한사이즈
    private double? _screenWidthRatio;
    public double ScreenWidthRatio // 스크린 비율
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
    /// 화면 터치 막음여부
    /// </summary>
    public bool IsScreenLock
    {
        get { return isScreenLock; }
        set { isScreenLock = value; }
    }

    private bool isESCLock = false;
    /// <summary>
    /// esc눌러서 닫기 기능 막음여부
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

        SetUICamera(); // UICamera 세팅
        //SetBottomCanvas(); // 최하단에 위치할 BottomCanvas 세팅. 사용안함
        //SetUILight(); // UICharacter 비출 Light 세팅
        //SetDefaultData(); // UI 공통사용할 데이터 정의. 컬러값, 팝업내블러 트윈값 등

        eventSystem = gameObject.AddComponent<EventSystem>();
        eventSystem.sendNavigationEvents = false;
        standardInputModule = gameObject.AddComponent<StandaloneInputModule>();
        //StartCoroutine(coUpdate());
    }

    void SetUICamera() // UICamera 세팅
    {
        GameObject cameraObj = new GameObject("@UICamera");
        cameraObj.transform.SetParent(transform);
        uiCamera = cameraObj.AddComponent<Camera>();
        uiCamera.clearFlags = CameraClearFlags.Depth;
        uiCamera.orthographic = true;
        uiCamera.orthographicSize = 36f;
        uiCamera.farClipPlane = distanceMax + 100; // 딱맞을경우 깜빡깜빡하는문제가 있어서 좀더 길게보도록함
        uiCamera.depth = 1;

        ResetUICameraPosition();
        //ResetUICameraCullingMask();
    }

    void ResetUICameraPosition() // UICamera 포지션 세팅
    {
        uiCamera.transform.localPosition = new Vector3(0f, 0f, -700f);
    }

    /// <summary>
    /// 스크린 비율, 사이즈, 사용가능범위등 갱신
    /// </summary>
    public void RefreshScreenWidthRatio()
    {
        // 간혹 화면이 회전되기 전에 스크린 사이즈를 받아오는 경우가 있으므로 예외처리 해준다.
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

    public void SetDefaultUICanvas(UIBase ui) // ui의 기본 Canvas값 세팅
    {
        SetScreenMatchValue(ui.UICanvasScaler);

        ui.UICanvas.renderMode = RenderMode.ScreenSpaceCamera;
        ui.UICanvas.worldCamera = uiCamera;
        ui.UICanvas.planeDistance = distanceMax;
        ui.UICanvas.sortingOrder = sortBase;
    }

    public void SetScreenMatchValue(CanvasScaler cs) // ui의 Canvas MatchValue세팅
    {
        if (ScreenWidthRatio < 1) // 1280 * 720 (16:9) 비율보다 가로가 좁으면
            cs.matchWidthOrHeight = 0f;
        else                    // 1280 * 720이거나 가로가 더 넓은 와이드면
            cs.matchWidthOrHeight = 1f;
    }

    /// <returns>UI 이름으로 최상단인지 체크</returns>
    public bool IsTopUI(string uiName)
    {
        //return uiStack.Count > 0 && string.Equals(PeekUIStack().GetType().FullName, uiName);
        return GetUIStack().Count > 0 && string.Equals(PeekUIStack().uiName, uiName);
    }

    

    #region UIStack
    
    /// <returns>uiStack 리스트를 전부 가져옴</returns>
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
            SortAllUICanvas(); // 전체 uiStack Canvas 재정렬
        }
    }

    void PushUIStack(UIBase ui)
    {
        RemoveUIStack(ui.uiName);

        GetTopUI(true)?.OnBackendUI();

        GetUIStack().Insert(0, ui);
        SortAllUICanvas(); // 전체 uiStack Canvas 재정렬
    }

    public void InsertUIStack(UIBase ui, UIBase targetUI, Dictionary<UIOptionsKey, object> options = null) // uiStack 에 insert
    {
        RemoveUIStack(ui.uiName); // stack에 이미 존재하면 뺌

        GetTopUI(true)?.OnBackendUI();

        GetUIStack().Insert(GetUIStack().IndexOf(targetUI), ui); // insert
        SortAllUICanvas();  // 전체 uiStack Canvas 재정렬

        //리소스 로드 영향 안받는 함수
        //ui.SetFrameBackground(options);
        ui.SetBaseDataAtFirst(); //최초 진입 시 필요한 데이터 세팅
        ui.Initialize(options); //각 ui의 데이터 초기화

        //리소스 로드 및 생성, 세팅
        //ui.SetUIResources(() =>
        {
            ui.FinishedShow(); // FinishedShow : 리소스 로드 완료 후 ui 세팅 마무리 후 활성화
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
    /// UIPopup을 포함해서 최상단 UIBase리스트
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
    UIBase PeekUIStack() // 최상위 ui peek
    {
        return GetUIStack().Count > 0 ? GetUIStack()[0] : null;
    }

    UIBase PeekUIStack(int index) // index로 uiStack내 ui가져옴
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
    public void Remove(string uiName) // 이름으로 uiStack내 ui삭제
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
            //    if (cachedUINames.Count > MaxCachedUICount)    // 캐시 UI 맥스치 넘어가면
            //    {
            //        string destroyUiName = cachedUINames[0];
            //        if (!uiStack.Contains(cachedUIs[destroyUiName]))    // 현재 켜져있지 않은 UI만(uiStack에 없는거만) 삭제 (Back을 통해 갈 수 있기때문에 삭제하면 안된다)
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

                    //현재 지워지는 UI가 팝업일경우 이전 TOP UI가 팝업일 경우 팝업의 PlayBgm를 실행시킨다.
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

    public UIBase GetUI(string uiName) // 이름으로 uiStack내 ui가져옴
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
    //    // 기존 팝업닫혀서 사운드 1초정도 나오는현상 막기위해 로비하고 전투 가장 베이스로 내려갔을때
    //    // bgm 딜레이 1초강제로 준다.
    //    bool delayBgm = false;
    //    if (GetUIStack().Count == 1)
    //    {
    //        if (GetUIStack()[0].contentsType == ContentsType.Lobby)
    //            delayBgm = true;
    //    }

    //    UIBase topUI = GetTopUI();
    //    if (topUI != null)
    //        topUI.PlayBGM(delayBgm ? 1f : 0); // 이전 ui PlayBGM

    //    ShowLobbyEffectMask(); // 로비면 LobbyEffectMask Show
    //}

    //public void RemoveAll() // 최상단UI를 제외한 uiStack 내 모든 UI삭제. 
    //{
    //    if (GetUIStack().Count > 1)
    //    {
    //        //최하위 UI 빼고 UIStack 정보 제거.
    //        var tempUIStack = new List<UIBase>(GetUIStack().ToArray()); //깊은 복사(원본데이터를 사용해서 제거와 참조를 동시에 하면 위험하므로)
    //        for (int i = 0; i < tempUIStack.Count - 1; i++)
    //        {
    //            UIBase ui = GetUI(tempUIStack[i].uiName);

    //            ui.Hide();
    //            RemoveUIStack(ui.name);
    //        }

    //        //모두 제거후 최상위 UI 처리(아마 로비UI/월드UI/전투UI이지 않을까 싶음)
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

    /// <returns>uiStack중 가장 최상단의 ui를 가져옴</returns>
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

    public void SortAllUICanvas() // uiStack내 전체 canvas sortingOrder 재정렬
    {
        int orderIdx = 0;
        for (int i = 0; i < GetUIStack().Count; i++)
        {
            if (GetUIStack()[i].isIgnoreSortingOrder == true)
            {
                continue;
            }

            GetUIStack()[i].UICanvas.planeDistance = distanceMax - (GetUIStack().Count - 1 - orderIdx) * distanceOffset; // stack인덱스에 따라 카메라거리 적용
            GetUIStack()[i].UICanvas.sortingOrder = sortBase + (GetUIStack().Count - 1 - orderIdx) * sortOffset; // stack인덱스에 따라 소트값 적용

            //if (GetUIStack()[i].EffBg != null) // 이펙트 배경 들어있으면 이펙트도 소트 재정렬
            //{
            //    for (int j = 0; j < GetUIStack()[i].EffBg.Length; j++)
            //    {
            //        GetUIStack()[i].EffBg[j].baseSortOrder = GetUIStack()[i].UICanvas.sortingOrder;
            //        GetUIStack()[i].EffBg[j].Apply();
            //    }
            //}

            for (int j = 0; j < GetUIStack()[i].UISubCanvas.Count; j++) // ui내에 다른 Sub Canvas들이 존재하면 해당 Canvas들도 stack인덱스에 맞게 소트값 적용
            {
                GetUIStack()[i].UISubCanvas[j].sortingOrder = sortBase + (GetUIStack().Count - 1 - orderIdx) * sortOffset;
                GetUIStack()[i].UISubCanvas[j].sortingOrder += GetUIStack()[i].UISubCanvasOriginSort[j];
            }
            orderIdx++;
        }
    }
}
