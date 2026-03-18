using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUIButton : MonoBehaviour
{
    [SerializeField] private string _sceneName;
    
    public void ChangeScene()
    {
        // 씬 이름이 비어있는지 확인 (예외 처리)
        if (string.IsNullOrEmpty(_sceneName))
        {
            Debug.LogWarning("씬 이름이 설정되지 않았습니다!");
            return;
        }

        // 실제 씬 전환 실행
        SceneManager.LoadScene(_sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}