using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class TransitionController : MonoBehaviour
{
    public GameObject Transition;
    public Text LoadingText;
    [Space(15)]
    public float TransitionTime;
    [Space(15)]
    public float SceneChangeTime;

    private RectTransform rt;

    private static TransitionController instance;

    private void Start()
    {
        DontDestroyOnLoad(this);

        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        rt = GetComponent<RectTransform>();
        OpenMainMenu();
    }

    public void QuitGame() 
    {
        LoadingText.text = "Thank you for playing!";
        Vector3 topPoint = new Vector3(0, 1080, 0);
        Vector3 midPoint = Vector3.zero;

        Transition.GetComponent<RectTransform>().anchoredPosition = topPoint;
        Transition.GetComponent<RectTransform>().DOAnchorPos(midPoint, TransitionTime - .1f).SetEase(Ease.OutCubic);
    }

    public void OpenMainMenu() 
    {
        Vector3 leftPoint = new Vector3(-1920, 0, 0);

        Transition.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        Transition.GetComponent<RectTransform>().DOAnchorPos(leftPoint, TransitionTime * 2).SetEase(Ease.OutCubic);
    }

    public IEnumerator MoveScenes(string sceneName,string loadingText, bool wantRight) 
    {
        LoadingText.text = loadingText;

        Vector3 rightPoint = new Vector3(1920, 0, 0);
        Vector3 leftPoint = new Vector3(-1920, 0, 0);

        if (wantRight)
        {
            Transition.GetComponent<RectTransform>().anchoredPosition = leftPoint;
            Transition.GetComponent<RectTransform>().DOAnchorPos(rightPoint, TransitionTime * 2).SetEase(Ease.OutCubic);
        }
        else 
        {
            Transition.GetComponent<RectTransform>().anchoredPosition = rightPoint;
            Transition.GetComponent<RectTransform>().DOAnchorPos(leftPoint, TransitionTime * 2).SetEase(Ease.OutCubic);
        }

        yield return new WaitForSeconds(SceneChangeTime);

        SceneManager.LoadScene(sceneName);
    }
}
