using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ColorAnimation : MonoBehaviour
{
    [SerializeField]
    private Image thisImage;

    private Color color1 = Color.red;
    private Color color2 = Color.white;

    [SerializeField]
    private float duration = 1f; 
    private LoopType loopType = LoopType.Yoyo;
    Tween thisTween;

    private void OnEnable()
    {
        if (thisImage == null)
        {
            thisImage = GetComponent<Image>();
        }

        
        LoopBetweenColors();
    }

    private void OnDisable()
    {
        thisTween.Kill();
    }

    private void LoopBetweenColors()
    {

        thisTween = thisImage.DOColor(color1, duration).SetEase(Ease.Linear)
            .SetLoops(-1, loopType) 
            .OnStepComplete(() =>
            {
                
                Color temp = color1;
                color1 = color2;
                color2 = temp;
            });
    }
}
