using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrapper : MonoBehaviour
{
    private void Start()
    {
        
        InitializeFramework();
        InitializeManagers();
        LoadGameplay();
    }

    private void InitializeFramework()
    {

    }

    private void InitializeManagers()
    {

    }

    private void LoadGameplay()
    {
        SceneManager.LoadScene("Gameplay_0");
    }
}
