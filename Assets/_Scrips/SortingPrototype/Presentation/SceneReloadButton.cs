using UnityEngine;
using UnityEngine.SceneManagement;

namespace SortingPrototype.Presentation
{
    public sealed class SceneReloadButton : MonoBehaviour
    {
        public void ReloadCurrentScene()
        {
            var activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}
