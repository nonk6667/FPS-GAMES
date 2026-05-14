using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string firstLevelSceneName = "Level1_Stealth";
    [SerializeField] private string startButtonName = "Startbutton";
    [SerializeField] private string quitButtonName = "Quitbutton";
    [SerializeField] private RectTransform startButtonHitArea;
    [SerializeField] private bool enableFallbackInput = true;

    private Button startButton;
    private Button quitButton;
    private RectTransform quitButtonHitArea;
    private Canvas menuCanvas;
    private bool isLoadingScene;

    private void Awake()
    {
        UnlockCursor();
        EnsureUiInfrastructure();
        ResolveMenuReferences();
        PrepareButton(startButton, startButtonHitArea);
        PrepareButton(quitButton, quitButtonHitArea);
        BindButtons();
    }

    private void OnEnable()
    {
        UnlockCursor();
        ResolveMenuReferences();
        BindButtons();
    }

    private void ResolveMenuReferences()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (Button button in buttons)
        {
            if (IsNamedButton(button, startButtonName, "start"))
            {
                startButton = button;

                if (startButtonHitArea == null)
                    startButtonHitArea = button.GetComponent<RectTransform>();
            }

            if (IsNamedButton(button, quitButtonName, "quit"))
            {
                quitButton = button;
                quitButtonHitArea = button.GetComponent<RectTransform>();
            }
        }

        if (startButton == null)
        {
            startButton = FindOrCreateButton(startButtonName, ref startButtonHitArea);
        }

        if (quitButton == null)
        {
            quitButton = FindOrCreateButton(quitButtonName, ref quitButtonHitArea);
        }

        if (startButton != null && menuCanvas == null)
        {
            menuCanvas = startButton.GetComponentInParent<Canvas>();
        }
    }

    private void BindStartButton()
    {
        if (startButton == null) return;

        startButton.onClick.RemoveListener(StartGame);
        startButton.onClick.AddListener(StartGame);
    }

    private void BindButtons()
    {
        BindStartButton();

        if (quitButton == null) return;

        quitButton.onClick.RemoveListener(QuitGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void Update()
    {
        if (!enableFallbackInput) return;
        if (isLoadingScene) return;

        if (WasStartKeyPressed())
        {
            StartGame();
            return;
        }

        if (WasPrimaryPointerPressed(out Vector2 pointerPosition))
        {
            TryHandlePointerFallback(pointerPosition);
        }
    }

    public void StartGame()
    {
        if (isLoadingScene) return;

        LoadScene(firstLevelSceneName);
    }

    public void LoadScene(string sceneName)
    {
        isLoadingScene = SceneLoadHelper.LoadScene(sceneName, "Main menu");
    }

    private bool IsPointerOverStartButton()
    {
        return IsPointerOverStartButton(GetPointerPosition());
    }

    private bool IsPointerOverStartButton(Vector2 pointerPosition)
    {
        return IsPointerOverRect(startButtonHitArea, pointerPosition);
    }

    private bool IsPointerOverQuitButton(Vector2 pointerPosition)
    {
        return IsPointerOverRect(quitButtonHitArea, pointerPosition);
    }

    private void TryHandlePointerFallback(Vector2 pointerPosition)
    {
        if (IsPointerOverQuitButton(pointerPosition))
        {
            QuitGame();
            return;
        }

        if (IsPointerOverStartButton(pointerPosition))
        {
            StartGame();
        }
    }

    private bool IsPointerOverRect(RectTransform rectTransform, Vector2 pointerPosition)
    {
        if (rectTransform == null) return false;

        Camera eventCamera = GetEventCamera(rectTransform);

        if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, pointerPosition, eventCamera))
        {
            return true;
        }

        return IsPointerInsideWorldCorners(rectTransform, pointerPosition, eventCamera);
    }

    private bool IsPointerInsideWorldCorners(RectTransform rectTransform, Vector2 pointerPosition, Camera eventCamera)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        Vector2 min = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[0]);
        Vector2 max = min;

        for (int i = 1; i < corners.Length; i++)
        {
            Vector2 screenCorner = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[i]);
            min = Vector2.Min(min, screenCorner);
            max = Vector2.Max(max, screenCorner);
        }

        Rect screenRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        return screenRect.Contains(pointerPosition);
    }

    private Camera GetEventCamera(RectTransform rectTransform)
    {
        Canvas owningCanvas = rectTransform.GetComponentInParent<Canvas>();
        if (owningCanvas == null || owningCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return owningCanvas.worldCamera != null ? owningCanvas.worldCamera : Camera.main;
    }

    private bool WasStartKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null &&
            (Keyboard.current.enterKey.wasPressedThisFrame ||
             Keyboard.current.numpadEnterKey.wasPressedThisFrame ||
             Keyboard.current.spaceKey.wasPressedThisFrame))
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.Space))
        {
            return true;
        }
#endif

        return false;
    }

    private bool WasPrimaryPointerPressed(out Vector2 pointerPosition)
    {
        pointerPosition = GetPointerPosition();

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            pointerPosition = Mouse.current.position.ReadValue();
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetMouseButtonDown(0))
        {
            pointerPosition = Input.mousePosition;
            return true;
        }
#endif

        return false;
    }

    private Vector2 GetPointerPosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.mousePosition;
#else
        return Vector2.zero;
#endif
    }

    private void OnGUI()
    {
        if (!enableFallbackInput) return;
        if (isLoadingScene) return;

        Event currentEvent = Event.current;
        if (currentEvent == null || currentEvent.type != EventType.MouseDown || currentEvent.button != 0)
        {
            return;
        }

        Vector2 pointerPosition = currentEvent.mousePosition;
        pointerPosition.y = Screen.height - pointerPosition.y;

        if (IsPointerOverQuitButton(pointerPosition) || IsPointerOverStartButton(pointerPosition))
        {
            TryHandlePointerFallback(pointerPosition);
            currentEvent.Use();
        }
    }

    private void EnsureUiInfrastructure()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();

        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }
        else if (eventSystem.GetComponent<BaseInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }

        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            if (canvas.GetComponent<GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }

            if (menuCanvas == null)
            {
                menuCanvas = canvas;
            }
        }
    }

    private Button FindOrCreateButton(string buttonName, ref RectTransform hitArea)
    {
        GameObject buttonObject = GameObject.Find(buttonName);
        if (buttonObject == null) return null;

        Button button = buttonObject.GetComponent<Button>();
        if (button == null)
        {
            button = buttonObject.AddComponent<Button>();
        }

        if (hitArea == null)
        {
            hitArea = buttonObject.GetComponent<RectTransform>();
        }

        return button;
    }

    private void PrepareButton(Button button, RectTransform hitArea)
    {
        if (button == null) return;

        Graphic targetGraphic = button.targetGraphic;

        if (targetGraphic == null)
        {
            targetGraphic = button.GetComponent<Graphic>();
        }

        if (targetGraphic != null)
        {
            targetGraphic.raycastTarget = true;
            button.targetGraphic = targetGraphic;
        }

        button.interactable = true;
        button.enabled = true;

        int uiLayer = LayerMask.NameToLayer("UI");
        if (hitArea != null && uiLayer >= 0 && hitArea.gameObject.layer != uiLayer)
        {
            hitArea.gameObject.layer = uiLayer;
        }
    }

    private bool IsNamedButton(Button button, string exactName, string fallbackKeyword)
    {
        string buttonName = button.name;
        return buttonName == exactName ||
               buttonName.ToLowerInvariant().Contains(fallbackKeyword);
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
