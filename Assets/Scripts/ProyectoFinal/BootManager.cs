using UnityEngine;
using UnityEngine.SceneManagement;

namespace RootsOfLife
{
    /// <summary>
    /// Se ejecuta una sola vez al inicio del juego.
    /// Garantiza que todos los singletons existan y carga la escena de menú.
    ///
    /// INSTRUCCIONES DE USO:
    ///   1. Crea una escena llamada "Boot" (o la que prefieras).
    ///   2. Añade un GameObject vacío con este componente.
    ///   3. Ponla como primera escena en Build Settings.
    /// </summary>
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
