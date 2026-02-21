using UnityEngine;

public class UIManager : MonoBehaviour
{
    // Singleton instance
    public static UIManager Instance;

    // References to UI screens
    public GameObject[] uiScreens;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to show a specific UI screen
    public void ShowScreen(string screenName)
    {
        foreach (GameObject screen in uiScreens)
        {
            screen.SetActive(screen.name == screenName);
        }
    }

    // Method to hide all screens
    public void HideAllScreens()
    {
        foreach (GameObject screen in uiScreens)
        {
            screen.SetActive(false);
        }
    }

    // Example method to switch to a specific menu
    public void SwitchToMenu(string menuName)
    {
        HideAllScreens();
        ShowScreen(menuName);
    }
}