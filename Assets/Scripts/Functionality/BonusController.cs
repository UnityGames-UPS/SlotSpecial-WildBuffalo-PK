using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Best.SocketIO;

public class BonusController : MonoBehaviour
{
    [SerializeField]
    private Button Spin_Button;
    [SerializeField]
    private RectTransform Wheel_Transform;
    [SerializeField]
    private BoxCollider2D[] point_colliders;
    [SerializeField]
    private TMP_Text[] Bonus_Text;
    [SerializeField]
    private CanvasGroup main_Bonus_Object;
    [SerializeField]
    private CanvasGroup Bonus_Info_Wheel;
    [SerializeField]
    private GameObject Bonus_Info_Group;
    [SerializeField]
    private SlotBehaviour slotManager;
    [SerializeField]
    private AudioController _audioManager;
    [SerializeField]
    private GameObject PopupPanel;
    [SerializeField]
    private TMP_Text Win_Text;
    [SerializeField]
    private Transform Loose_Transform;
    [SerializeField]
    private SocketIOManager m_SocketManager;

    internal bool isCollision = false;

    private Tween wheelRoutine;

    private float elasticIntensity = 5f;

    private int stopIndex = 0;


    private void Start()
    {
        if (Spin_Button) Spin_Button.onClick.RemoveAllListeners();
        if (Spin_Button) Spin_Button.onClick.AddListener(Spinbutton);
    }

    internal void StartBonus(int stop, SlotBehaviour.bonusWheelType wheelType)
    {
       
        ResetColliders();
        if (PopupPanel) PopupPanel.SetActive(false);
        if (Win_Text) Win_Text.gameObject.SetActive(false);
        if (Loose_Transform) Loose_Transform.gameObject.SetActive(false);
        if (_audioManager) _audioManager.SwitchBGSound(true);

       
        switch (wheelType)
        {
            case SlotBehaviour.bonusWheelType.small:
              
                PopulateWheel(m_SocketManager.initialData.smallWheelFeature);
                break;

            case SlotBehaviour.bonusWheelType.medium:
                
                PopulateWheel(m_SocketManager.initialData.mediumWheelFeature);
                break;

            case SlotBehaviour.bonusWheelType.large:
               
                PopulateWheel(m_SocketManager.initialData.largeWheelFeature);       
                break;

            default:
                return;
        }
        
        stopIndex = stop;
        Bonus_Info_Group.gameObject.SetActive(true);
        Bonus_Info_Wheel.transform.localScale = new Vector2(7.74f, 7.74f);
        Bonus_Info_Wheel.alpha = 0;
        Bonus_Info_Wheel.transform.DOScale(new Vector2(1.375f, 1.375f), 0.3f).SetEase(Ease.Flash); 
        Bonus_Info_Wheel.DOFade(1f, 0.4f).SetEase(Ease.Linear);
        if (Spin_Button) Spin_Button.interactable = true;
        Spin_Button.gameObject.SetActive(false);
        DOVirtual.DelayedCall(2f, () =>
        {  Spinbutton();         
        });
        //if (slotManager.IsAutoSpin || slotManager.IsFreeSpin)
        //{
        //    Spin_Button.gameObject.SetActive(false);
        //    DOVirtual.DelayedCall(1f, () => {

        //        Spinbutton();
        //    });
        //}
        //else
        //{
        //    Spin_Button.gameObject.SetActive(true);
        //}
    }

    private void Spinbutton()
    {
        isCollision = false;
        if (Spin_Button) Spin_Button.interactable = false;
        main_Bonus_Object.gameObject.SetActive(true);
        main_Bonus_Object.alpha = 0f;
        main_Bonus_Object.DOFade(1f, 0.6f).SetEase(Ease.Flash).OnComplete(delegate
        {

            RotateWheel();
            DOVirtual.DelayedCall(1.5f, () =>
            {
                Bonus_Info_Group.gameObject.SetActive(false);
                TurnCollider(stopIndex);
            });
        });
    }

    internal void PopulateWheel(List<int> bonusdata)
    {
        Debug.Log("updateRewardsRan");
        for (int i = 0; i < bonusdata.Count; i++)
        {
            if (i < 4)
            {
                if (Bonus_Text[i]) Bonus_Text[i].text = (bonusdata[i]+" Spins").ToString();
               
            }
            else
            {
                //if (Bonus_Text[i]) Bonus_Text[i].text = (bonusdata[i] * m_SocketManager.initialData.Bets[slotManager.BetCounter] +"X").ToString();
                if (Bonus_Text[i]) Bonus_Text[i].text = (bonusdata[i]  + "X").ToString();
            }
            
        }
    }

    private void RotateWheel()
    {
        if (Wheel_Transform) Wheel_Transform.localEulerAngles = new Vector3(0, 0, 359);
        if (Wheel_Transform) wheelRoutine =  Wheel_Transform.DORotate(new Vector3(0, 0, 0), 0.6f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
        _audioManager.PlayBonusAudio("cycleSpin");
    }

    private void ResetColliders()
    {
        foreach(BoxCollider2D col in point_colliders)
        {
            col.enabled = false;
        }
    }

    private void TurnCollider(int point)
    {
        if (point_colliders[point]) point_colliders[point].enabled = true;
    }

    internal void StopWheel()
    {
        if (wheelRoutine != null)
        {
            wheelRoutine.Pause(); // Pause the rotation

            // Apply an elastic effect to the paused rotation
            Wheel_Transform.DORotate(Wheel_Transform.eulerAngles + Vector3.forward * Random.Range(-elasticIntensity, elasticIntensity), 1f)
                .SetEase(Ease.OutElastic);
        }
        if (Bonus_Text[stopIndex].text.Equals("NO \nBONUS")) 
        {
            if (Loose_Transform) Loose_Transform.gameObject.SetActive(true);
            if (Loose_Transform) Loose_Transform.localScale = Vector3.zero;
            if (PopupPanel) PopupPanel.SetActive(true);
            if (Loose_Transform) Loose_Transform.DOScale(Vector3.one, 1f);
            PlayWinLooseSound(false);
        }
        else
        {
            if (Win_Text) Win_Text.gameObject.SetActive(true);
            if (Bonus_Text[stopIndex].text.Contains("Spins"))
            { 
                if (Win_Text) Win_Text.text = "You Win " + Bonus_Text[stopIndex].text;
            }
            else
            {
                if (Win_Text) Win_Text.text = "You Win " + Bonus_Text[stopIndex].text + " Multiplier";
            }
            
            if (PopupPanel) PopupPanel.SetActive(true);
           // if (Win_Text) Win_Text.transform.DOScale(Vector3.one, 1f);
            PlayWinLooseSound(true);
        }
        DOVirtual.DelayedCall(3f, () =>
        {
           
            ResetColliders();
            if (_audioManager) _audioManager.SwitchBGSound(false);
            main_Bonus_Object.DOFade(0, 0.5f).SetEase(Ease.Linear).OnComplete(delegate
        {
            if (main_Bonus_Object) main_Bonus_Object.gameObject.SetActive(false);
        });
           
            slotManager.CheckWinPopups();
        });
    }

    internal void PlayWinLooseSound(bool isWin)
    {
        if (isWin)
        {
            _audioManager.PlayBonusAudio("win");
        }
        else
        {
            _audioManager.PlayBonusAudio("lose");
        }
    }
}
