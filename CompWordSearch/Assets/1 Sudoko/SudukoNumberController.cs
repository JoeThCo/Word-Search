using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class SudukoNumberController : MonoBehaviour
{
    public int x;
    public int y;
    [Space(15)]
    public float ScaleSize = 1.5f;
    public float ScaleTime = .25f;
    [Space(15)]
    public List<GameObject> AllNoteTexts;
    public int[,] Notes;
    private int noteGridSize = 3;
    [Space(15)]
    public Transform NotesGrid;
    public GameObject NotesText;

    private SudukoManager sm;

    private void Start()
    {
        sm = FindObjectOfType<SudukoManager>();
        
        SpawnNoteTexts();
    }

    public void SetColor() 
    {
        sm.CursorDetection(gameObject);
    }

    void SpawnNoteTexts()
    {
        for (int i = 0; i < noteGridSize * noteGridSize; i++)
        {
            GameObject noteText = Instantiate(NotesText, Vector3.zero, Quaternion.identity, NotesGrid);
            noteText.GetComponent<Text>().text = " ";
            AllNoteTexts.Add(noteText);
        }
    }

    public void MakeLetterBigger(bool wantBig)
    {
        if (wantBig)
            gameObject.transform.DOScale(ScaleSize, ScaleTime);
        else
            gameObject.transform.DOScale(1, ScaleTime);
    }
}
