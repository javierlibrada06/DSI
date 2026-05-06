using UnityEngine;
using UnityEngine.SceneManagement;

namespace RootsOfLife
{
    public class BootManager : MonoBehaviour
    {
        [SerializeField] private string firstScene = "MenuScene";

        private void Awake()
        {
            EnsureSingleton<SaveManager>("SaveManager");
            EnsureSingleton<GameSession>("GameSession");
            EnsureSingleton<AudioManager>("AudioManager");
        }

        private void Start()
        {
            SceneManager.LoadScene(firstScene);
        }

        private static void EnsureSingleton<T>(string goName) where T : Component
        {
            if (FindObjectOfType<T>() != null) return;
            var go = new GameObject(goName);
            go.AddComponent<T>();
            DontDestroyOnLoad(go);
        }
    }
}
