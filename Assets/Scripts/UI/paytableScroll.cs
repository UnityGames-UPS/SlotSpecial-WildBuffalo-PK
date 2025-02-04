using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections;

public class PaytableScroll : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{

    [SerializeField]
    private ScrollRect scrollRect;
    [SerializeField]
    private RectTransform contentMain;
    [SerializeField]
    private RectTransform content;
    [SerializeField]
    private float snapSpeed = 10f; 
    [SerializeField]
    private float snapThreshold = 0.2f;
    [SerializeField]
    private float contentItemWidth = 2209f;
    private float contentOffset = 0;
    [SerializeField]
    private float contentOffsetcaliberate = -346.4698f;
    [SerializeField]
    private float contentheightOffset = 253.15f;
    [SerializeField]
    private RectTransform[] items;
    [SerializeField]
    private Image[] indicator;
    [SerializeField]
    private Sprite indicatorOn;
    [SerializeField]
    private Sprite indicatorOff;
    [SerializeField]
    private Vector2 targetPosition;
    public int closestItemIndex;
    public float dragduration;
    [SerializeField]
    float speedthreshold;
    [SerializeField]
    float swipeThreshold;
    public Vector2 swipeDistance;

    void Start()
    {
       
        items = new RectTransform[content.childCount];
        indicator[0].sprite = indicatorOn;
        for (int i = 0; i < content.childCount; i++)
        {
            items[i] = content.GetChild(i).GetComponent<RectTransform>();
        }
    }

    

    public void OnBeginDrag(PointerEventData eventData)
    {
        resetDrag();
        swipeDistance = eventData.position;
        dragduration = Time.time;
        
    }

    void resetDrag()
    {
        contentOffset = contentOffsetcaliberate;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragduration = Mathf.Abs(dragduration - Time.time);
        swipeDistance -= eventData.position;
        SnapToClosestItem();
    }

    private void SnapToClosestItem()
    {
        Debug.Log("functioncalled");
       
        float closestDistance = Mathf.Infinity;
        RectTransform closestItem = null;
        scrollRect.velocity = Vector2.zero;
       
        if (swipeDistance.magnitude > swipeThreshold && dragduration < speedthreshold)
        {
            
            if (swipeDistance.x < 0)
            {
                Debug.Log("left"+swipeDistance.magnitude);
                closestItemIndex--;
                if (closestItemIndex < 0)
                {
                    closestItemIndex = 0;               
                }
                closestItem = items[closestItemIndex];
            }
            else
            {
                Debug.Log("right" +swipeDistance.magnitude);
                closestItemIndex++;
                if (closestItemIndex > items.Length - 1)
                {
                    closestItemIndex = items.Length - 1;           
                }
                closestItem = items[closestItemIndex];
            }
        }
        else
        {
            for (int i = 0; i < items.Length; i++)
            {
                float distance = Vector2.Distance(contentMain.transform.position, items[i].transform.position);
              
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestItem = items[i];
                    closestItemIndex = i;

                }
            }
        }

        Debug.Log(closestItem);
        if (closestItem != null)
        {
            float offset = contentItemWidth * closestItemIndex;
            contentOffset = offset - contentOffset;
            contentOffset = -contentOffset;
            targetPosition = new Vector2(contentOffset, contentheightOffset);
           
            for (int i = 0; i < indicator.Length; i++)
            {
                indicator[i].sprite = indicatorOff;
            }
            indicator[closestItemIndex].sprite = indicatorOn;
            content.DOAnchorPos(targetPosition, snapSpeed);
        }
    }

}
