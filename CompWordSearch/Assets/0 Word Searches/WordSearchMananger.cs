using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Net;

public class WordSearchMananger : MonoBehaviour
{
    //todo
    //implament multiplayer - both comp and friendly
    //get it on steam, then make it for mobile
    //ads/ ingame purchases

    #region Vars

    public bool isPlaying = true;
    public bool WantRandomLetters = false;
    private int RandomSeed = 5;
    public int NumberOfWords = 10;
    [Space(15)]
    public float TimeToComplete = 0;
    public float LerpTime;
    [Space(15)]
    public string SelectedLetters;
    [HideInInspector]
    public bool CanSelect = false;
    [Space(15)]
    public Color SelectColor;
    public Color Correct;
    public Color Wrong;
    public List<GameObject> SelectedLettersList;
    [HideInInspector]
    public List<GameObject> CorrectLettersList;
    [Space(15)]
    private int BoardSize = 15;
    [HideInInspector]
    public int WordsLeft = 0;
    [Space(15)]
    [HideInInspector]
    public List<string> WordsToFind;
    [Space(15)]
    public Text InGameTimeText;
    public Text CompleteTimeText;
    [Header("Prefabs")]
    public GameObject LetterPrefab;
    public GameObject WordPrefab;
    [Space(15)]
    public Transform WordSearchGridParent;
    public Transform WordsInWordSearchParent;
    [Space(15)]
    public GameObject OnWordSearchComplete;
    public GameObject OnWordSearchPause;
    public Button PauseButton;
    [Space(15)]
    private char BlankChar = '.';
    public char[,] WordSearchBoard;

    private string URL = "https://randomword.com/";

    #endregion
    private void Start()
    {
        MakeWordSearch();
    }

    /// <summary>
    /// creates a word search
    /// </summary>
    public void MakeWordSearch()
    {
        //set the size of the board with the grid layout group
        WordSearchGridParent.GetComponent<GridLayoutGroup>().constraintCount = BoardSize;

        //loads the word from the selected puzzle
        LoadWordsToFind();

        //random seed
        RandomSeed = Random.Range(0, 1000000);
        WordSearchBoard = MakeGrid(RandomSeed);

        //make a grid
        MakeUiGrid(WordSearchBoard);

        //tell the user what words to find
        PlaceWhatWordsAreInThePuzzle();

        WordsLeft = HowManyWordsAreLeft();
        TimeToComplete = 0;
    }

    List<string> GetDataFromWebpage(string url)
    {
        List<string> randomSet = new List<string>();
        WebClient client = new WebClient();

        while (randomSet.Count < NumberOfWords)
        {
            //make a URL call to the given URL, which is random word generator
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string content = client.DownloadString(url);

            //search the html or xml for this string VVV
            string searchFor = "random_word";

            //get start and end index of the word that you want
            int start = content.IndexOf(searchFor);
            int end = content.IndexOf("</div>", start - searchFor.Length);
            string word = null;

            //add the letters from the info above into a string
            for (int j = searchFor.Length + 2; j < end - start; j++)
            {
                word += content[start + j];
            }

            //double check that it can even fit on the board
            if (word.Length < BoardSize - 3) 
            {
                randomSet.Add(word);
            }
        }
        return randomSet;
    }

    #region Create Word Search

    /// <summary>
    /// load what words to find
    /// </summary>
    void LoadWordsToFind()
    {
        WordsToFind = GetDataFromWebpage(URL);
    }

    public char[,] MakeGrid(int seed)
    {
        //make a new char 2d array with board size dimensions
        char[,] newGrid = new char[BoardSize, BoardSize];

        //I was new and I filled board with blank chars, but it does that by default so this is extra
        newGrid = InitBoard();

        //places words on the board
        newGrid = PlaceWordsInTheBoard();

        if (WantRandomLetters)
            newGrid = FillTheBoardWithRandomChars(newGrid, seed);

        return newGrid;
    }

    /// <summary>
    /// fills the board with the blank char
    /// </summary>
    char[,] InitBoard()
    {
        char[,] newGrid = new char[BoardSize, BoardSize];

        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                newGrid[i, j] = BlankChar;
            }
        }

        return newGrid;
    }

    /// <summary>
    /// places a word on the board
    /// </summary>
    /// <param name="placeForwards"></param>
    /// <param name="word"></param>
    /// <param name="startX"></param>
    /// <param name="StartY"></param>
    /// <param name="xSlope"></param>
    /// <param name="ySlope"></param>
    /// <param name="newGrid"></param>
    void PlaceWord(bool placeForwards, string word, int startX, int StartY, int xSlope, int ySlope, char[,] newGrid)
    {
        //forwards
        if (placeForwards)
        {
            //places the word you want by a slope of x / y
            for (int i = 0; i < word.Length; i++)
            {
                newGrid[startX + i * xSlope, StartY + i * ySlope] = word[i];
            }
        }
        //backwards
        else
        {
            //places the word you want by a slope of x / y, but backwards
            for (int i = 0; i < word.Length; i++)
            {
                newGrid[startX + i * xSlope, StartY + i * ySlope] = word[word.Length - 1 - i];
            }
        }
    }

    /// <summary>
    /// places the word that are in the list on the board
    /// </summary>
    char[,] PlaceWordsInTheBoard()
    {
        char[,] newGrid = new char[BoardSize, BoardSize];

        //from the list of words
        foreach (string word in WordsToFind)
        {
            bool canPlace = true;

            while (canPlace)
            {
                //random cords x and y in the board
                int placeCordsX = Random.Range(0, BoardSize);
                int placeCordsY = Random.Range(0, BoardSize);

                //0 - horizontal
                //1 - Vertical
                //2 - Pos Diagonal
                //3 - Neg Diagonal
                int howToPlace = Random.Range(0, 4);


                //the placing methods are pretty much the same, just build for what they need to do
                void PlaceHorizontal()
                {
                    // check if you can place the word
                    if (word.Length + placeCordsX < BoardSize)
                    {
                        //check if you want the word to be placed forwards or backwards
                        if (Random.Range(0, 2) == 0)
                        {
                            //check if the word is valid
                            if (IsValidForHorz(true, word, placeCordsX, placeCordsY, newGrid))
                            {
                                //place it forwards!
                                PlaceWord(true, word, placeCordsX, placeCordsY, 1, 0, newGrid);
                                canPlace = false;
                            }
                        }
                        else
                        {
                            //place it backwards!
                            if (IsValidForHorz(false, word, placeCordsX, placeCordsY, newGrid))
                            {
                                PlaceWord(false, word, placeCordsX, placeCordsY, 1, 0, newGrid);
                                canPlace = false;
                            }
                        }
                    }
                }

                void PlaceVert()
                {
                    if (word.Length + placeCordsY < BoardSize)
                    {
                        if (Random.Range(0, 2) == 0)
                        {
                            if (IsValidForVert(true, word, placeCordsX, placeCordsY, newGrid))
                            {

                                PlaceWord(true, word, placeCordsX, placeCordsY, 0, 1, newGrid);
                                canPlace = false;
                            }
                        }
                        else
                        {
                            if (IsValidForVert(false, word, placeCordsX, placeCordsY, newGrid))
                            {
                                PlaceWord(false, word, placeCordsX, placeCordsY, 0, 1, newGrid);
                                canPlace = false;
                            }
                        }
                    }
                }

                void PositiveDiagonals()
                {
                    if (word.Length + placeCordsX < BoardSize && word.Length + placeCordsY < BoardSize)
                    {
                        if (Random.Range(0, 2) == 0)
                        {
                            if (isValidForDiagonalPos(true, word, placeCordsX, placeCordsY, newGrid))
                            {
                                PlaceWord(true, word, placeCordsX, placeCordsY, 1, 1, newGrid);
                                canPlace = false;
                            }
                        }
                        else
                        {
                            if (isValidForDiagonalPos(false, word, placeCordsX, placeCordsY, newGrid))
                            {
                                PlaceWord(false, word, placeCordsX, placeCordsY, 1, 1, newGrid);
                                canPlace = false;
                            }
                        }
                    }
                }

                void NegativeDiagonals()
                {
                    if (word.Length + placeCordsX < BoardSize && word.Length + placeCordsY < BoardSize && placeCordsX > word.Length && placeCordsY > word.Length)
                    {
                        if (Random.Range(0, 2) == 0)
                        {
                            if (IsValidForDiagonalNeg(true, word, placeCordsX, placeCordsY, newGrid))
                            {
                                PlaceWord(true, word, placeCordsX, placeCordsY, 1, -1, newGrid);
                                canPlace = false;
                            }
                        }
                        else
                        {
                            if (IsValidForDiagonalNeg(false, word, placeCordsX, placeCordsY, newGrid))
                            {
                                PlaceWord(false, word, placeCordsX, placeCordsY, 1, -1, newGrid);
                                canPlace = false;
                            }
                        }
                    }
                }

                if (howToPlace == 0)
                    PlaceHorizontal();
                if (howToPlace == 1)
                    PlaceVert();
                if (howToPlace == 2)
                    PositiveDiagonals();
                if (howToPlace == 3)
                    NegativeDiagonals();
            }
        }

        return newGrid;
    }

    #region IsValids
    //checks if the word can be placed, i think there might be a way to get this under one method but I gave up and this worked so happens
    bool IsValidForHorz(bool checkForward, string word, int startX, int startY, char[,] grid)
    {
        if (checkForward)
        {
            for (int i = 0; i < word.Length; i++)
            {
                if (char.IsLetter(grid[startX + i, startY]))
                    if ((int)grid[startX + i, startY] != (int)(word[i]))
                        return false;
            }
        }
        else
        {
            for (int i = 0; i < word.Length; i++)
            {
                if (char.IsLetter(grid[startX + i, startY]))
                    if ((int)grid[startX + i, startY] != (int)(word[word.Length - 1 - i]))
                        return false;
            }
        }
        return true;
    }
    bool IsValidForVert(bool checkForward, string word, int startX, int startY, char[,] grid)
    {
        if (checkForward)
        {
            for (int i = 0; i < word.Length; i++)
            {
                if (char.IsLetter(grid[startX, startY + i]))
                    if ((int)grid[startX, startY + i] != (int)(word[i]))
                        return false;
            }
        }
        else
        {
            for (int i = 0; i < word.Length; i++)
            {
                if (char.IsLetter(grid[startX, startY + i]))
                    if ((int)grid[startX, startY + i] != (int)(word[word.Length - 1 - i]))
                        return false;
            }
        }

        return true;
    }
    bool isValidForDiagonalPos(bool checkForward, string word, int startX, int startY, char[,] grid)
    {
        if (checkForward)
        {
            for (int i = 0; i < word.Length; i++)
            {
                if (char.IsLetter(grid[startX + i, startY + i]))
                    if ((int)grid[startX + i, startY + i] != (int)(word[i]))
                        return false;
            }
        }
        else
        {
            for (int i = 0; i < word.Length; i++)
            {
                if (char.IsLetter(grid[startX + i, startY + i]))
                    if ((int)grid[startX + i, startY + i] != (int)(word[word.Length - 1 - i]))
                        return false;
            }
        }


        return true;
    }
    bool IsValidForDiagonalNeg(bool checkForward, string word, int startX, int startY, char[,] grid)
    {
        if (checkForward)
        {
            for (int i = 0; i < word.Length; i++)
            {
                if (char.IsLetter(grid[startX + i, startY - i]))
                    if ((int)grid[startX + i, startY - i] != (int)(word[i]))
                        return false;
            }
        }
        else
        {
            for (int i = 0; i < word.Length; i++)
            {
                if (char.IsLetter(grid[startX + i, startY - i]))
                    if ((int)grid[startX + i, startY - i] != (int)(word[word.Length - 1 - i]))
                        return false;
            }
        }
        return true;
    }

    #endregion

    /// <summary>
    /// takes any place that is not fill in already and turns it into a random letter
    /// </summary>
    char[,] FillTheBoardWithRandomChars(char[,] grid, int seed)
    {
        //this is used to fill the board with random letters after you get the wanted words down
        char[,] newGrid = grid;

        Random.InitState(seed);
        
        //go through board
        for (int col = 0; col < BoardSize; col++)
        {
            for (int row = 0; row < BoardSize; row++)
            {
                //if there is not a letter at the spot
                if (!char.IsLetter(newGrid[row, col]))
                {
                    //place a random letter
                    newGrid[row, col] = RandomLetter();
                }
            }
        }
        return newGrid;
    }

    /// <summary>
    /// returns a random lower case letter
    /// </summary>
    /// <returns></returns>
    char RandomLetter()
    {
        //ascii range for upper case letters
        //https://en.cppreference.com/w/cpp/language/ascii
        return (char)Random.Range(97, 123);
    }

    /// <summary>
    /// makes the Ui elements that appear on the screen
    /// </summary>
    public void MakeUiGrid(char[,] grid)
    {
        //spawns in the letters that make the grid you play on
        for (int col = 0; col < BoardSize; col++)
        {
            for (int row = 0; row < BoardSize; row++)
            {
                GameObject letter = SpawnLetter();

                letter.GetComponent<WordSearchLetterController>().row = row;
                letter.GetComponent<WordSearchLetterController>().col = col;

                //checks if you need to update the text on the letter
                if (grid[row, col] != BlankChar)
                {
                    letter.GetComponentInChildren<Text>().text = grid[row, col].ToString();
                }
                else
                {
                    letter.GetComponentInChildren<Text>().text = BlankChar.ToString();
                }
            }
        }
    }

    // houses the spawning of the letter
    GameObject SpawnLetter()
    {
        //pro tip: learn about grid layout groups. they are super helpful for all ui
        GameObject letter = Instantiate(LetterPrefab, Vector3.zero, Quaternion.identity, WordSearchGridParent);
        letter.GetComponentInChildren<Text>().rectTransform.sizeDelta = letter.GetComponent<RectTransform>().sizeDelta * 4;

        return letter;
    }

    /// <summary>
    /// sets the words that are on the side
    /// </summary>
    void PlaceWhatWordsAreInThePuzzle()
    {
        //theres are the words that you are looking for displayed on the left side of the board
        foreach (string i in WordsToFind)
        {
            GameObject word = Instantiate(WordPrefab, Vector3.zero, Quaternion.identity, WordsInWordSearchParent);
            word.transform.localScale = Vector3.zero;

            word.GetComponentInChildren<Text>().text = i;
        }

        //lerp theses bad boys in
        //ALSO GET DOTWEEN FROM UNITY ASSET STORE - ITS THE BEST THING EVER
        foreach (Transform i in WordsInWordSearchParent) 
        {
            i.DOScale(Vector3.one, LerpTime).SetEase(Ease.InOutQuint);
        }
    }

    #endregion

    #region Finding a Word
    /// <summary>
    /// highlights what the player is selecting
    /// </summary>
    void FillInSelectedLetters()
    {
        if (SelectedLettersList.Count > 0)
        {
            //set the color of the selected letter list to the select color
            foreach (GameObject i in SelectedLettersList)
            {
                i.GetComponentInChildren<Image>().color = SelectColor;
            }
        }
    }

    /// <summary>
    /// clears the highlights and reset letters status
    /// </summary>
    public void ClearHighlights()
    {
        //return to defaults
        foreach (GameObject i in SelectedLettersList)
        {
            i.GetComponentInChildren<Image>().color = i.GetComponent<WordSearchLetterController>().DefaultColor;
            i.GetComponent<WordSearchLetterController>().isSelected = false;
        }

        //clear list
        SelectedLettersList.Clear();
    }

    /// <summary>
    /// adds the letter to the selected string
    /// </summary>
    /// <param name="letter"></param>
    public void AddLetterToSelected(GameObject letter)
    {
        SelectedLetters += WordSearchBoard[letter.GetComponent<WordSearchLetterController>().row, letter.GetComponent<WordSearchLetterController>().col].ToString();
    }

    /// <summary>
    /// if the letters isnt added to list, add it
    /// </summary>
    /// <param name="letter"></param>
    public void AddToSelected(GameObject letter)
    {
        if (!SelectedLettersList.Contains(letter)) 
        {
            SelectedLettersList.Add(letter);
        }
    }

    /// <summary>
    /// check if the string made returns a word in the to find array
    /// </summary>
    /// <returns></returns>
    public bool CheckIfLettersMakeAWord()
    {
        string reverseSelected = null;

        if (SelectedLetters == null) return false;

        //flips the word around
        if (SelectedLetters.Length > 0)
        {
            char[] reverseCharArray = SelectedLetters.ToCharArray();
            System.Array.Reverse(reverseCharArray);
            reverseSelected = new string(reverseCharArray);
        }
        else
            return false;

        //is there a word that is forwards or backwards in the list?
        if (WordsToFind.Contains(SelectedLetters) || WordsToFind.Contains(reverseSelected)) 
        {
            return true;
        }

        return false;
    }
    #endregion

    #region Check Word Info
    /// <summary>
    /// makes the word correct
    /// </summary>
    void MakesLettersOnBoardCorrect()
    {
        foreach (GameObject i in CorrectLettersList)
        {
            i.GetComponentInChildren<Image>().color = Correct;
            i.GetComponent<WordSearchLetterController>().isSelected = false;
        }
    }

    /// <summary>
    /// makes a correct word to find the correct color on a correct find
    /// </summary>
    void MakesListWordCorrect()
    {
        string reverseSelected = null;

        char[] reverseCharArray = SelectedLetters.ToCharArray();
        System.Array.Reverse(reverseCharArray);
        reverseSelected = new string(reverseCharArray);

        foreach (Transform i in WordsInWordSearchParent)
            if (i.GetComponentInChildren<Text>().text == SelectedLetters || i.GetComponentInChildren<Text>().text == reverseSelected)
                i.GetComponentInChildren<Text>().color = Correct;
    }

    /// <summary>
    /// returns how many words are left to find
    /// </summary>
    /// <returns></returns>
    int HowManyWordsAreLeft()
    {
        int total = 0;
        foreach (Transform i in WordsInWordSearchParent)
            if (i.GetComponentInChildren<Text>().color != Correct)
                total++;

        return total;
    }

    /// <summary>
    /// what to do whne a word search is complete
    /// </summary>
    void OnPuzzleComplete()
    {
        OnWordSearchComplete.transform.DOScale(Vector3.one, LerpTime);
        CompleteTimeText.text = "Congrats! You got all the words in " + TimeToComplete.ToString("F1") + " seconds!";
    }

    /// <summary>
    /// what to do when the selected letters IS a word
    /// 
    /// </summary>
    void IsAWord()
    {
        foreach (GameObject i in SelectedLettersList)
        {
            CorrectLettersList.Add(i);
        }

        MakesListWordCorrect();
        MakesLettersOnBoardCorrect();

        WordsLeft = HowManyWordsAreLeft();

        if (WordsLeft == 0)
        {
            OnPuzzleComplete();
        }

        SelectedLettersList.Clear();
        CanSelect = false;
        SelectedLetters = null;
    }

    /// <summary>
    /// what to do when the selected letters is NOT a word
    /// </summary>
    IEnumerator NotAWord()
    {
        CanSelect = false;

        foreach (GameObject i in SelectedLettersList)
        {
            i.GetComponentInChildren<Image>().color = Wrong;
        }

        yield return new WaitForSeconds(.15f);

        ClearHighlights();

        MakesLettersOnBoardCorrect();

        SelectedLetters = null;
    }

    /// <summary>
    /// the act of getting the words on the board, mouse input
    /// </summary>
    void SelectLettersOnBoard()
    {
        if (Input.GetMouseButtonDown(0))
            CanSelect = true;
        if (Input.GetMouseButtonUp(0))
        {
            if (CheckIfLettersMakeAWord())
                IsAWord();
            else
                StartCoroutine(NotAWord());
        }
    }

    #endregion

    #region Pause Menu
    void OpenPauseMenu()
    {
        isPlaying = false;
        OnWordSearchPause.transform.DOScale(Vector3.one, LerpTime / 2);
    }

    void ClosePauseMenu()
    {
        OnWordSearchPause.transform.DOScale(Vector3.zero, LerpTime / 3);
        isPlaying = true;
    }

    public void OpenOrCloseMenu()
    {
        if (isPlaying)
            OpenPauseMenu();
        else
            ClosePauseMenu();
    }

    public void LoadPuzzle()
    {
        StartCoroutine(FindObjectOfType<TransitionController>().MoveScenes("WordSearch", "Loading Word Search...", true));
    }

    public void ToMainMenu()
    {
        StartCoroutine(FindObjectOfType<TransitionController>().MoveScenes("MainMenu", "Loading Main Menu...", false));
    }
    #endregion

    private void FixedUpdate()
    {
        if (WordsLeft > 0 && isPlaying) 
        {
            TimeToComplete += Time.deltaTime;

            string secs = TimeToComplete % 60 <= 10 ? "0" + (TimeToComplete % 60).ToString("F0") : (TimeToComplete % 60).ToString("F0");
            string mins = (TimeToComplete / 60).ToString("F0");
            InGameTimeText.text = mins + " : " + secs;
        }

        if (WordsLeft == 0)
            isPlaying = false;

        if (Input.GetMouseButton(0))
            FillInSelectedLetters();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
            WantRandomLetters = !WantRandomLetters;

        if (Input.GetKeyDown(KeyCode.Escape))
            OpenOrCloseMenu();

        SelectLettersOnBoard();
    }
}
