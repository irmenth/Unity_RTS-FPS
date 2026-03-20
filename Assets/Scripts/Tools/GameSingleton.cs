using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSingleton : MonoBehaviour
{
    private static GameSingleton instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SceneManager.LoadScene(1);
    }
}
