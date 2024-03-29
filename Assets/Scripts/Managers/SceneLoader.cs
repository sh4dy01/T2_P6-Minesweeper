using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class SceneLoader : MonoBehaviour
    {
        #region Singleton
        public static SceneLoader Instance { get; private set; }
        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(Instance);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        #endregion
        
        public static void LoadGameScene()
        {
            MusicManager.Instance.SetGameMusic();
            SceneManager.LoadScene("game");
        }

        public static void LoadLobbyScene()
        {
            MusicManager.Instance.SetLobbyMusic();
            GameManager.Instance.Reset();
            SceneManager.LoadScene("Lobby");
        }
        
        public static void QuitGame()
        {
            Application.Quit();
        }
    }
}
