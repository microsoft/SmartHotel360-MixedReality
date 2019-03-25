using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SmartHotelMR
{
    public class SmartHotelManager : Singleton<SmartHotelManager>
    {
        private string _currentScene;

        public bool IsAdminMode { get; set; }

        public void LoadScene(string name, bool isAdmin = false)
        {
            StartCoroutine(LoadSceneAsync(name, isAdmin));
        }

        private IEnumerator LoadSceneAsync(string name, bool isAdmin)
        {
            if (_currentScene == name)
                yield break;

            _currentScene = name;

            IsAdminMode = isAdmin;

            var operation = SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);

            while (!operation.isDone)
            {
                yield return null;
            }

            yield return null;
        }
    }
}