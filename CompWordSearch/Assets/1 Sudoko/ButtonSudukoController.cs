using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSudukoController : MonoBehaviour
{
    public int Number;

    private SudukoManager sm;
    private void Start()
    {
        sm = FindObjectOfType<SudukoManager>();
    }

    public void PlaceNumber() 
    {
        FindObjectOfType<SudukoManager>().PlaceNumber(Number);
    }
}
