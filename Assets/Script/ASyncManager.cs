using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ASyncManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private Image loadingSlider;

    public void LoadLevelBtn(int sceneId)
    {
        StartCoroutine(LoadLevelASync(sceneId));
    }

    IEnumerator LoadLevelASync(int sceneId)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneId);

        loadingScreen.SetActive(true);

        while (!loadOperation.isDone)
        {
            float progressValue = Mathf.Clamp01(loadOperation.progress / 0.9f);
            loadingSlider.fillAmount = progressValue;
            yield return null;
        }
        
    }
}
