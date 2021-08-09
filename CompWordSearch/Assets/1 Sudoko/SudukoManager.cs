using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SudukoManager : MonoBehaviour
{
    #region Vars
    public int Seed;
    public int RandomNumbers = 10;
    [Space(15)]
    public bool isPlaying = true;
    public float GameTime = 0;
    private float LerpTime = .5f;
    [Space(15)]
    public bool wantPlaceNotes = false;
    public GameObject WantButtonGameObject;
    [Space(15)]
    public int PlaceX;
    public int PlaceY;
    [Space(15)]
    public int Mistakes;
    [Space(15)]
    private int BoardSize = 9;
    [Space(15)]
    public List<Vector2> AllShowAtStart;
    public List<GameObject> AllGridPrefabs;
    [Space(15)]
    public Transform GridParent;
    public Transform ButtonParent;
    [Space(15)]
    public GameObject PlaceNotesButton;
    [Space(15)]
    public GameObject ButtonPrefab;
    public GameObject GridPrefab;
    [Space(15)]
    public GameObject PauseButton;
    public GameObject OnGameComplete;
    public GameObject OnPauseGame;

    public int[,] CorrectBoard;
    public int[,] PlayerBoard;
    [Space(15)]
    public Color SelectedColor;
    public Color InRowInColumnInSquare;

    #endregion

    private void Start()
    {
        CorrectBoard = new int[BoardSize, BoardSize];
        PlayerBoard = new int[BoardSize, BoardSize];

        WantPlaceNotesOrNot();

        PlaceRandomNumbers();

        SolvePuzzle();

        SetUpGrid();
        MakeBoardUI();
        PlaceButtons();
    }

    void UpdateGrid(int y, int x, int updateValue)
    {
        foreach (GameObject i in AllGridPrefabs)
        {
            if (i.GetComponent<SudukoNumberController>().x == x && i.GetComponent<SudukoNumberController>().y == y)
            {
                i.GetComponentInChildren<Text>().text = updateValue.ToString();

                Text[] texts = i.GetComponentInChildren<GridLayoutGroup>().GetComponentsInChildren<Text>();

                foreach (Text j in texts) 
                {
                    j.text = " ";
                }

                return;
            }
        }
    }
    void SetUpGrid()
    {
        GridParent.GetComponent<GridLayoutGroup>().constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        GridParent.GetComponent<GridLayoutGroup>().constraintCount = BoardSize;
    }
    void MakeBoardUI()
    {
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                GameObject number = Instantiate(GridPrefab, Vector3.zero, Quaternion.identity, GridParent);

                number.GetComponent<SudukoNumberController>().x = x;
                number.GetComponent<SudukoNumberController>().y = y;

                if (AllShowAtStart.Contains(new Vector2(y,x)))
                    number.GetComponentInChildren<Text>().text = (CorrectBoard[y, x]).ToString();
                else
                    number.GetComponentInChildren<Text>().text = " ";

                AllGridPrefabs.Add(number);
            }
        }
    }

    #region Make the Correct Puzzle
    void PlaceRandomNumbers()
    {
        Seed = Random.Range(0, 1000000);

        Random.InitState(Seed);

        int count = 0;
        while (count < RandomNumbers)
        {
            int x = Random.Range(0, BoardSize);
            int y = Random.Range(0, BoardSize);

            int value = Random.Range(1, BoardSize + 1);

            if (isPossible(y, x, value))
            {
                CorrectBoard[y, x] = value;
                PlayerBoard[y, x] = value;

                AllShowAtStart.Add(new Vector2(y, x));
                count++;
            }
        }
    }
    bool isPossible(int y, int x, int n)
    {
        //goes through rows
        for (int i = 0; i < BoardSize; i++)
        {
            if (CorrectBoard[y, i] == n)
            {
                return false;
            }
        }

        //goes through cols
        for (int i = 0; i < BoardSize; i++)
        {
            if (CorrectBoard[i, x] == n)
            {
                return false;
            }
        }

        int xZero = Mathf.RoundToInt(x / 3) * 3;
        int yZero = Mathf.RoundToInt(y / 3) * 3;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (CorrectBoard[yZero + i, xZero + j] == n)
                    return false;
            }

        }
        return true;
    }
    bool SolvePuzzle()
    {
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if (CorrectBoard[y, x] == 0)
                {
                    for (int i = 1; i < BoardSize + 1; i++)
                    {
                        if (isPossible(y, x, i))
                        {
                            CorrectBoard[y, x] = i;
                            if (SolvePuzzle())
                                return true;
                            else
                            {
                                CorrectBoard[y, x] = 0;
                            }
                        }
                    }
                    return false;
                }
            }
        }
        return true;
    }
    #endregion

    #region Placing Numbers
    public void CursorDetection(GameObject selectedGameObject)
    {
        SudukoNumberController sCon = selectedGameObject.GetComponent<SudukoNumberController>();

        PlaceX = sCon.x;
        PlaceY = sCon.y;

        foreach (Transform i in GridParent)
        {
            i.GetComponent<Image>().color = Color.white;
        }

        foreach (Transform i in GridParent)
        {
            SudukoNumberController iCon = i.GetComponent<SudukoNumberController>();

            if (iCon.x == sCon.x || iCon.y == sCon.y ||
                Mathf.RoundToInt(iCon.x / 3) * 3 == Mathf.RoundToInt(sCon.x / 3) * 3 && Mathf.RoundToInt(iCon.y / 3) * 3 == Mathf.RoundToInt(sCon.y / 3) * 3)
            {
                i.GetComponent<Image>().color = InRowInColumnInSquare;
            }
        }

        selectedGameObject.GetComponent<Image>().color = SelectedColor;
    }
    public void WantPlaceNotesOrNot()
    {
        wantPlaceNotes = !wantPlaceNotes;

        if (wantPlaceNotes)
            WantButtonGameObject.GetComponentInChildren<Text>().text = "Notes: On";
        else
            WantButtonGameObject.GetComponentInChildren<Text>().text = "Notes: Off";
    }
    void PlaceButtons()
    {
        for (int i = 0; i < BoardSize; i++)
        {
            GameObject button = Instantiate(ButtonPrefab, Vector3.zero, Quaternion.identity, ButtonParent);
            button.GetComponent<ButtonSudukoController>().Number = i + 1;
            button.GetComponentInChildren<Text>().text = (i + 1).ToString();
        }
    }
    public void PlaceNumber(int index)
    {
        if (!wantPlaceNotes)
        {
            if (PlaceX != -1 && PlaceY != -1)
            {
                if (CorrectBoard[PlaceY, PlaceX] == PlayerBoard[PlaceY, PlaceX])
                {
                    UpdateGrid(PlaceY, PlaceX, index);
                    PlayerBoard[PlaceY, PlaceX] = index;
                }
                else
                {
                    Mistakes++;
                }

                Debug.Log(HasWon());
            }
        }
        else
        {
            foreach (GameObject i in AllGridPrefabs)
            {
                if (i.GetComponent<SudukoNumberController>().x == PlaceX && i.GetComponent<SudukoNumberController>().y == PlaceY)
                {
                    if (i.GetComponentsInChildren<Text>()[index].text == index.ToString())
                        i.GetComponentsInChildren<Text>()[index].text = " ";
                    else 
                        i.GetComponentsInChildren<Text>()[index].text = index.ToString();
                }
            }
        }
    }
    void PlaceNumbersWithKeyBoard() 
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            PlaceNumber(1);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            PlaceNumber(2);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            PlaceNumber(3);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            PlaceNumber(4);
        if (Input.GetKeyDown(KeyCode.Alpha5))
            PlaceNumber(5);
        if (Input.GetKeyDown(KeyCode.Alpha6))
            PlaceNumber(6);
        if (Input.GetKeyDown(KeyCode.Alpha7))
            PlaceNumber(7);
        if (Input.GetKeyDown(KeyCode.Alpha8))
            PlaceNumber(8);
        if (Input.GetKeyDown(KeyCode.Alpha9))
            PlaceNumber(9);
    }
    bool HasWon() 
    {
        for (int y = 0; y < BoardSize; y++) 
        {
            for (int x = 0; x < BoardSize; x++) 
            {
                if (CorrectBoard[y, x] != PlayerBoard[y, x])
                    return false;
            }
        }
        return true;
    }
    #endregion

    public void PauseGame() 
    {
        if (isPlaying)
            OnPauseGame.transform.DOScale(Vector3.one, LerpTime);
        else
            OnPauseGame.transform.DOScale(Vector3.zero, LerpTime);

        isPlaying = !isPlaying;
    }
    public void LoadPuzzle()
    {
        StartCoroutine(FindObjectOfType<TransitionController>().MoveScenes("Suduko", "Loading Suduko...", true));
    }

    public void ToMainMenu()
    {
        StartCoroutine(FindObjectOfType<TransitionController>().MoveScenes("MainMenu", "Loading Main Menu...", false));
    }

    private void FixedUpdate()
    {
        if(isPlaying)
            GameTime += Time.deltaTime;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            PauseGame();

        if (isPlaying)
            PlaceNumbersWithKeyBoard();
    }
}
