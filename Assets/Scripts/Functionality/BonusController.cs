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
    private float[] wheelStops = new float[12]
       {
        0f,   // slot 0 → 12 o’clock
        30f,  // slot 1
        60f,  // slot 2
        90f,  // slot 3
        120f, // slot 4
        150f, // slot 5
        180f, // slot 6 → 6 o’clock
        210f, // slot 7
        240f, // slot 8
        270f, // slot 9
        300f, // slot 10
        330f  // slot 11 → just before 12 o’clock
       };

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
    private Coroutine spinRoutine;

    private Dictionary<int, int> shuffledIndexMap = new Dictionary<int, int>();


    private void Start()
    {
        if (Spin_Button)
        {
            Spin_Button.onClick.RemoveAllListeners();
            Spin_Button.onClick.AddListener(Spinbutton);
        }
    }

    internal void StartBonus(int stop, SlotBehaviour.bonusWheelType wheelType)
    {
        stopIndex = stop;

        if (PopupPanel) PopupPanel.SetActive(false);
        if (Win_Text) Win_Text.gameObject.SetActive(false);
        if (Loose_Transform) Loose_Transform.gameObject.SetActive(false);

        if (_audioManager) _audioManager.SwitchBGSound(true);

        // Fill wheel values
        switch (wheelType)
        {
            case SlotBehaviour.bonusWheelType.small:
                PopulateWheel(m_SocketManager.bonusdata.bonus.smallWheelFeature.featureValues);
                break;
            case SlotBehaviour.bonusWheelType.medium:
                PopulateWheel(m_SocketManager.bonusdata.bonus.mediumWheelFeature.featureValues);
                break;
            case SlotBehaviour.bonusWheelType.large:
                PopulateWheel(m_SocketManager.bonusdata.bonus.largeWheelFeature.featureValues);
                break;
            default:
                return;
        }

        // Animate wheel info popup
        Bonus_Info_Group.gameObject.SetActive(true);
        Bonus_Info_Wheel.transform.localScale = new Vector2(7.74f, 7.74f);
        Bonus_Info_Wheel.alpha = 0;
        Bonus_Info_Wheel.transform.DOScale(new Vector2(1.375f, 1.375f), 0.3f).SetEase(Ease.Flash);
        Bonus_Info_Wheel.DOFade(1f, 0.4f).SetEase(Ease.Linear);

        Spin_Button.gameObject.SetActive(false);

        // Auto-spin after short delay
        DOVirtual.DelayedCall(2f, Spinbutton);
    }

    private void Spinbutton()
    {
        if (Spin_Button) Spin_Button.interactable = false;

        main_Bonus_Object.gameObject.SetActive(true);
        main_Bonus_Object.alpha = 0f;

        main_Bonus_Object.DOFade(1f, 0.6f).SetEase(Ease.Flash).OnComplete(() =>
        {
            //RotateWheel();

            // Stop automatically after a short delay
            DOVirtual.DelayedCall(2f, () =>
            {
                StartSpin(m_SocketManager.resultData.bonusIndex);
                Bonus_Info_Group.gameObject.SetActive(false);
            });
        });
    }

    // internal void PopulateWheel(List<int> bonusdata)
    // {
    //     for (int i = 0; i < bonusdata.Count; i++)
    //     {
    //         if (Bonus_Text[i])
    //         {
    //             if (i < 6)
    //                 Bonus_Text[i].text = bonusdata[i] + " Spins";
    //             else
    //                 Bonus_Text[i].text = bonusdata[i] + "X";
    //         }
    //     }
    // }

    internal void PopulateWheel(List<int> bonusdata)
    {
        // 1. Fill in natural order first (0–5 Spins, 6–11 Multipliers)
        for (int i = 0; i < bonusdata.Count; i++)
        {
            if (Bonus_Text[i])
            {
                if (i < 6)
                    Bonus_Text[i].text = bonusdata[i] + " Spins";
                else
                    Bonus_Text[i].text = bonusdata[i] + "X";
            }
        }

        // 2. Make a list of indices [0..11]
        List<int> indices = new List<int>();
        for (int i = 0; i < bonusdata.Count; i++)
            indices.Add(i);

        // 3. Shuffle the indices
        for (int i = 0; i < indices.Count; i++)
        {
            int rand = Random.Range(i, indices.Count);
            (indices[i], indices[rand]) = (indices[rand], indices[i]);
        }

        // 4. Apply shuffle to TMP_Text (swap text according to shuffled order)
        string[] originalTexts = new string[Bonus_Text.Length];
        for (int i = 0; i < Bonus_Text.Length; i++)
            originalTexts[i] = Bonus_Text[i].text;

        shuffledIndexMap.Clear();

        for (int newIndex = 0; newIndex < indices.Count; newIndex++)
        {
            int originalIndex = indices[newIndex];

            // set shuffled text
            Bonus_Text[newIndex].text = originalTexts[originalIndex];

            // map: server originalIndex → new UI slot index
            shuffledIndexMap[originalIndex] = newIndex;
        }
    }
    internal void StartSpin(int stopIndexFromServer)
    {
        if (spinRoutine != null) StopCoroutine(spinRoutine);

        // Convert server index to shuffled wheel index
        if (shuffledIndexMap.TryGetValue(stopIndexFromServer, out int newIndex))
        {
            stopIndex = newIndex;
        }
        else
        {
            stopIndex = stopIndexFromServer; // fallback (no shuffle case)
        }

        // Reset wheel
        Wheel_Transform.localEulerAngles = Vector3.zero;

        // Start spinning
        spinRoutine = StartCoroutine(SpinWheelRoutine(stopIndex));
    }

    // internal void StartSpin(int stopIndex)
    // {
    //     if (spinRoutine != null) StopCoroutine(spinRoutine);

    //     // Reset wheel at start (always 0°)
    //     Wheel_Transform.localEulerAngles = Vector3.zero;

    //     // Start spinning
    //     spinRoutine = StartCoroutine(SpinWheelRoutine(stopIndex));
    // }

    private IEnumerator SpinWheelRoutine(int stopIndex)
    {
        Debug.Log("Wheel Test: start.   ----------------------------------------");
        float startAngle = 0f;
        float targetAngle = wheelStops[stopIndex]; // slot position (0–330)

        Debug.Log("Wheel Test: targetangle" + targetAngle);
        Debug.Log("Wheel Test: targettext" + Bonus_Text[stopIndex].text);
        Debug.Log("Wheel Test: Stop " + stopIndex);
        // Always clockwise → subtract angle
        float finalAngle = (4 * 360f) + targetAngle; // 4 full spins + stop

        float duration = 3f; // total spin time
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ease out for natural stop
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            float current = Mathf.Lerp(startAngle, finalAngle, eased);
            Wheel_Transform.localEulerAngles = new Vector3(0, 0, current);

            yield return null;
        }

        // Snap to exact final angle
        Wheel_Transform.localEulerAngles = new Vector3(0, 0, finalAngle);

        Debug.Log("Wheel Test: Stop " + stopIndex);
        HandleResult();

        spinRoutine = null;
    }





    private void RotateWheel()
    {
        Wheel_Transform.eulerAngles = Vector3.zero;

        // Infinite clockwise spin
        wheelRoutine = Wheel_Transform
            .DORotate(new Vector3(0, 0, -360f), 0.6f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1);

        _audioManager.PlayBonusAudio("cycleSpin");
    }

    internal void StopWheel()
    {
        if (wheelRoutine != null)
        {
            wheelRoutine.Kill();
            wheelRoutine = null;
        }

        float currentZ = Wheel_Transform.eulerAngles.z;
        float targetZ = wheelStops[m_SocketManager.resultData.bonusIndex];

        // Convert to clockwise system (negative rotation in Unity)
        float rawTargetZ = -targetZ;

        // Ensure target is ahead in clockwise direction
        while (rawTargetZ < currentZ - 360f)
        {
            rawTargetZ += 360f;
        }

        // Add extra clockwise spins
        float finalTargetZ = rawTargetZ - 720f;

        Wheel_Transform
            .DORotate(new Vector3(0, 0, finalTargetZ), 3f, RotateMode.FastBeyond360)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                Debug.Log("Stopped at index: " + m_SocketManager.resultData.bonusIndex);
                HandleResult();
            });
    }

    private void HandleResult()
    {
        if (Bonus_Text[stopIndex].text.Equals("NO \nBONUS"))
        {
            if (Loose_Transform)
            {
                Loose_Transform.gameObject.SetActive(true);
                Loose_Transform.localScale = Vector3.zero;
                Loose_Transform.DOScale(Vector3.one, 1f);
            }
            if (PopupPanel) PopupPanel.SetActive(true);
            PlayWinLooseSound(false);
        }
        else
        {
            if (Win_Text)
            {
                Win_Text.gameObject.SetActive(true);
                if (Bonus_Text[stopIndex].text.Contains("Spins"))
                    Win_Text.text = "You Win " + Bonus_Text[stopIndex].text;
                else
                    Win_Text.text = "You Win " + Bonus_Text[stopIndex].text + " Multiplier";
            }
            if (PopupPanel) PopupPanel.SetActive(true);
            PlayWinLooseSound(true);
        }

        // Fade out and reset
        DOVirtual.DelayedCall(3f, () =>
        {
            if (_audioManager) _audioManager.SwitchBGSound(false);

            main_Bonus_Object.DOFade(0, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
            {
                if (main_Bonus_Object) main_Bonus_Object.gameObject.SetActive(false);
                Debug.Log("checkWinPopUpsCalledFromHereAfterSpin");
                slotManager.CheckWinPopups();
                slotManager.isBonusGame = false;
                slotManager.spinDone = true;
            });
        });
    }

    internal void PlayWinLooseSound(bool isWin)
    {
        if (isWin) _audioManager.PlayBonusAudio("win");
        else _audioManager.PlayBonusAudio("lose");
    }
}