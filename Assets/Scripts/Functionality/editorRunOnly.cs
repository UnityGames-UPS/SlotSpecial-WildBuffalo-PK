using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[ExecuteInEditMode]
public class editorRunOnly : MonoBehaviour
{
    public SlotBehaviour slotmanager;
    public Image[] reelOne,reelTwo,reelThree,reelFour,reelFive;
    // Start is called before the first frame update

    void OnEnable()
    {
        Debug.Log("Script executing in edit mode");
        shuffleInitialMatrix();
    }
   

    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < reelTwo.Length; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, 13);
            reelFive[i].sprite = slotmanager.myImages[randomIndex];
        }
    }
    
}
