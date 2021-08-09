using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Text GameName;
    public Text VersionText;
    public Text CompanyName;
    [Space(15)]
    public GameObject Twitter;
    private void Start()
    {
        //sets the texts to update application name, the version and company name
        GameName.text = Application.productName;
        VersionText.text = "v" + Application.version;
        CompanyName.text = Application.companyName;
    }

    public IEnumerator QuitGame()
    {
        //transitino to quit
        FindObjectOfType<TransitionController>().QuitGame();

        yield return new WaitForSeconds(1f);

        //quit lol
        Application.Quit();
    }

    //in method, so it can be called as a UnityEvent from a button, ienumerators cannot do that Im pretty sure
    public void QuitTheGame() 
    {
        StartCoroutine(QuitGame());

        Debug.Log("Quitting...");
    }

    //opens twitter with the url
    public void OpenTwitter() 
    {
        Application.OpenURL("twitter.com/JoeColleyGames");
        Debug.Log("twitter...");
    }

    //starts the word search
    public void LoadWordSearch() 
    {
        StartCoroutine(FindObjectOfType<TransitionController>().MoveScenes("WordSearch","Loading Word Search...",true));
    }

    //starts the suduko
    public void LoadSuduko() 
    {
        StartCoroutine(FindObjectOfType<TransitionController>().MoveScenes("Suduko","Loading Suduko...",true));
    }
}
