using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string firstLevelSceneName = "Level1_Stealth";
    [SerializeField] private RectTransform startButtonHitArea;
    [SerializeField] private bool enableFallbackInput = true;

    private Button startButton;
    private bool isLoadingScene;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ResolveStartButton();
        BindStartButton();
    }

    private void ResolveStartButton()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (Button button in buttons)
        {
            if (button.name != "Startbutton") continue;

            startButton = button;

            if (startButtonHitArea == null)
                startButtonHitArea = button.GetComponent<RectTransform>();

            return;
        }

        GameObject startButtonObject = GameObject.Find("Startbutton");
        if (startButtonObject == null) return;

        startButton = startButtonObject.GetComponent<Button>();

        if (startButtonHitArea == null)
            startButtonHitArea = startButtonObject.GetComponent<RectTransform>();
    }

    private void BindStartButton()
    {
        if (startButton == null) return;

        startButton.onClick.RemoveListener(StartGame);
        startButton.onClick.AddListener(StartGame);
    }

    private void Update()
    {
        if (!enableFallbackInput) return;
        if (isLoadingScene) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
            return;
        }

        if (Input.GetMouseButtonDown(0) && IsPointerOverStartButton())
        {
            StartGame();
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
        if (startButtonHitArea == null) return false;

        return RectTransformUtility.RectangleContainsScreenPoint(startButtonHitArea, Input.mousePosition);
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
