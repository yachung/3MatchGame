using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoSingleton<UIManager>
{
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


}
