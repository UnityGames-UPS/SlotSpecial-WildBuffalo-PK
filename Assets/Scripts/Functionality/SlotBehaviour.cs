using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using Best.SocketIO;


public class SlotBehaviour : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField]
    internal Sprite[] myImages;

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;
    [SerializeField]
    private List<SlotImage> Tempimages;

    [Header("Slots Elements")]
    [SerializeField]
    private LayoutElement[] Slot_Elements;

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;

    [SerializeField]
    private Transform[] boost_Positions;

    [Header("Line Button Objects")]
    [SerializeField]
    private List<GameObject> StaticLine_Objects;

    [Header("Line Button Texts")]
    [SerializeField]
    private List<TMP_Text> StaticLine_Texts;

    private Dictionary<int, string> y_string = new Dictionary<int, string>();

    [Header("Buttons")]
    [SerializeField]
    private Button SlotStart_Button;
    [SerializeField]
    private Button AutoSpin_Button;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField]
    private Button MaxBet_Button;
    [SerializeField]
    private Button TBetPlus_Button;
    [SerializeField]
    private Button TBetMinus_Button;
    [SerializeField] private Button Turbo_Button;
    [SerializeField] private Button StopSpin_Button;

    [Header("Animated Sprites")]
    [SerializeField]
    private Sprite[] Bonus_Sprite;
    [SerializeField]
    private Sprite[] FreeSpin_Sprite;
    [SerializeField]
    private Sprite[] Jackpot_Sprite;
    [SerializeField]
    private Sprite[] WildBuffalo_Sprite;
    [SerializeField]
    private Sprite[] MajorBlondyGirl_Sprite;
    [SerializeField]
    private Sprite[] MajorDarkMan_Sprite;
    [SerializeField]
    private Sprite[] MajorGingerGirl_Sprite;
    [SerializeField]
    private Sprite[] RuneFehu_Sprite;
    [SerializeField]
    private Sprite[] RuneGebo_Sprite;
    [SerializeField]
    private Sprite[] RuneMannaz_Sprite;
    [SerializeField]
    private Sprite[] RuneOthala_Sprite;
    [SerializeField]
    private Sprite[] GoldenBonus_Sprite;
    [SerializeField]
    private Sprite[] Scatter_Sprite;
    [SerializeField]
    private Sprite[] Wild_Sprite;

    [Header("Miscellaneous UI")]
    [SerializeField]
    private TMP_Text balance_text;
    [SerializeField]
    private TMP_Text TotalBet_text;
    [SerializeField]
    private TMP_Text LineBet_text;
    [SerializeField]
    private TMP_Text TotalWin_text;

    [Header("Audio Management")]
    [SerializeField]
    private AudioController audioController;

    [SerializeField]
    private UIManager uiManager;

    [Header("BonusGame Popup")]
    [SerializeField]
    private BonusController _bonusManager;

    [Header("Free Spins Board")]
    [SerializeField]
    private GameObject FSBoard_Object;
    [SerializeField]
    private TMP_Text FSnum_text;

    int tweenHeight = 0;

    [SerializeField]
    private GameObject Image_Prefab;
    [SerializeField]
    private GameObject Win_Object;
    [SerializeField]
    private RectTransform boost_obj;
    [SerializeField] Sprite[] TurboToggleSprites;
    [SerializeField]
    private PayoutCalculation PayCalculator;
    private double currentBalance = 0;
    private List<Tweener> alltweens = new List<Tweener>();
    private List<string> bonus_AnimString = new List<string>();
    private Tweener WinTween = null;

    [SerializeField]
    private List<ImageAnimation> TempList;

    [SerializeField]
    private SocketIOManager SocketManager;

    private Coroutine AutoSpinRoutine = null;
    private Coroutine FreeSpinRoutine = null;
    private Coroutine tweenroutine;
    private Tween balanceTween;
    internal bool IsAutoSpin = false;
    internal bool IsFreeSpin = false;
    private bool IsSpinning = false;
    private bool CheckSpinAudio = false;
    internal bool CheckPopups = false;
    internal bool IsHoldSpin = false;
    internal int BetCounter = 0;
    private double currentbalance = 0;
    private double currentTotalBet = 0;
    protected int Lines = 50;
    [SerializeField]
    private int IconSizeFactor = 100;
    private int numberOfSlots = 5;
    private bool StopSpinToggle;
    private float SpinDelay = 0.2f;
    private bool IsTurboOn;
    internal bool WasAutoSpinOn;
    private float boostDuration = 2f;
    private bool boostDone;
    internal bool spinDone;
    private bool hasSkippedAnimation;
    private Coroutine BoxAnimRoutine = null;
    public float delayTime = 0.3f;
    internal bool isBonusGame = false;
    internal bool CheckAnimation = false;
    internal enum bonusWheelType
    {
        none,
        small,
        medium,
        large
    }
    bonusWheelType wheelType = bonusWheelType.none;

    private void Start()
    {
        IsAutoSpin = false;

        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

        if (TBetPlus_Button) TBetPlus_Button.onClick.RemoveAllListeners();
        if (TBetPlus_Button) TBetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); });

        if (TBetMinus_Button) TBetMinus_Button.onClick.RemoveAllListeners();
        if (TBetMinus_Button) TBetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); });

        if (MaxBet_Button) MaxBet_Button.onClick.RemoveAllListeners();
        if (MaxBet_Button) MaxBet_Button.onClick.AddListener(MaxBet);

        if (StopSpin_Button) StopSpin_Button.onClick.RemoveAllListeners();
        if (StopSpin_Button) StopSpin_Button.onClick.AddListener(() => { audioController.PlayButtonAudio(); StopSpinToggle = true; StopSpin_Button.gameObject.SetActive(false); });

        if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(AutoSpin);

        if (Turbo_Button) Turbo_Button.onClick.RemoveAllListeners();
        if (Turbo_Button) Turbo_Button.onClick.AddListener(TurboToggle);

        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(() => { IsAutoSpin = false; StopAutoSpin(); });

        if (FSBoard_Object) FSBoard_Object.SetActive(false);

        tweenHeight = (15 * IconSizeFactor) - 280;
    }

    void TurboToggle()
    {
        audioController.PlayButtonAudio();
        if (IsTurboOn)
        {
            IsTurboOn = false;
            Turbo_Button.GetComponent<ImageAnimation>().StopAnimation();
            Turbo_Button.image.sprite = TurboToggleSprites[0];
            Turbo_Button.image.color = new Color(0.86f, 0.86f, 0.86f, 1);
        }
        else
        {
            IsTurboOn = true;
            Turbo_Button.GetComponent<ImageAnimation>().StartAnimation();
            Turbo_Button.image.color = new Color(1, 1, 1, 1);
        }
    }

    #region Autospin


    internal void StartSpinRoutine()
    {
        // if (!IsSpinning)
        // {
        IsHoldSpin = false;
        Invoke("AutoSpinHold", 2f);
        // }
    }

    internal void StopSpinRoutine()
    {
        CancelInvoke("AutoSpinHold");
        if (IsAutoSpin)
        {
            IsAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            StartCoroutine(StopAutoSpinCoroutine());
        }
    }


    private void AutoSpin()
    {
        if (!IsAutoSpin)
        {

            IsAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());

        }
    }


    private void AutoSpinHold()
    {
        Debug.Log("Auto Spin Started");
        IsHoldSpin = true;
        AutoSpin();
    }


    private void StopAutoSpin()
    {
        Debug.Log("autoSpinStop");
        // if (!IsFreeSpin)
        // {
        //     ToggleButtonGrp(true);
        // }

        if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
        if (IsAutoSpin)
        {
            audioController.PlayButtonAudio();
            StartCoroutine(StopAutoSpinCoroutine());
        }
        IsAutoSpin = false;


    }

    private IEnumerator AutoSpinCoroutine()
    {
        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            yield return new WaitForSeconds(SpinDelay);
        }
        if (!IsAutoSpin || !IsFreeSpin)
        {
            ToggleButtonGrp(true);
        }

    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        //   Debug.Log(WasAutoSpinOn);
        if (!IsFreeSpin)
        {
            ToggleButtonGrp(true);
        }

        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            tweenroutine = null;
            AutoSpinRoutine = null;
            StopCoroutine(StopAutoSpinCoroutine());
        }

    }
    #endregion

    #region FreeSpin
    internal void FreeSpin(int spins)
    {
        if (!IsFreeSpin)
        {
            if (FSnum_text) FSnum_text.text = spins.ToString();
            if (FSBoard_Object) FSBoard_Object.SetActive(true);
            IsFreeSpin = true;
            ToggleButtonGrp(false);

            if (FreeSpinRoutine != null)
            {
                StopCoroutine(FreeSpinRoutine);
                FreeSpinRoutine = null;
            }
            FreeSpinRoutine = StartCoroutine(FreeSpinCoroutine(spins));
        }
    }

    private IEnumerator FreeSpinCoroutine(int spinchances)
    {
        int i = 0;
        while (i < spinchances)
        {
            uiManager.FreeSpins--;
            if (FSnum_text) FSnum_text.text = uiManager.FreeSpins.ToString();
            StartSlots();
            yield return tweenroutine;
            yield return new WaitForSeconds(SpinDelay);
            i++;
        }
        if (FSBoard_Object) FSBoard_Object.SetActive(false);
        //  Debug.Log("wasautospin : " + WasAutoSpinOn);
        if (WasAutoSpinOn)
        {
            AutoSpin();
        }
        else
        {
            Debug.Log("freespinrounitetogglegroup");
            ToggleButtonGrp(true);
        }
        IsFreeSpin = false;
    }
    #endregion

    private void Comparebalance()
    {
        if (currentbalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
        }
    }

    #region LinesCalculation
    //Fetch Lines from backend
    internal void FetchLines(string LineVal, int count)
    {

        y_string.Add(count + 1, LineVal);
        //StaticLine_Texts[count].text = (count + 1).ToString();
        //StaticLine_Objects[count].SetActive(true);
    }

    //Generate Static Lines from button hovers
    internal void GenerateStaticLine(TMP_Text LineID_Text)
    {
        Debug.Log("lines");
        DestroyStaticLine();
        int LineID = 1;
        try
        {
            LineID = int.Parse(LineID_Text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Exception while parsing " + e.Message);
        }
        List<int> y_points = null;
        y_points = y_string[LineID]?.Split(',')?.Select(Int32.Parse)?.ToList();
        PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, true);
    }

    //Destroy Static Lines from button hovers
    internal void DestroyStaticLine()
    {
        PayCalculator.ResetStaticLine();
    }
    #endregion

    private void MaxBet()
    {
        if (audioController) audioController.PlayButtonAudio();
        BetCounter = SocketManager.initialData.bets.Count - 1;
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.bets[BetCounter] * Lines;

    }

    private void ChangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (IncDec)
        {
            BetCounter++;
            if (BetCounter >= SocketManager.initialData.bets.Count)
            {
                BetCounter = 0;
            }
        }
        else
        {
            BetCounter--;
            if (BetCounter < 0)
            {
                BetCounter = SocketManager.initialData.bets.Count - 1;
            }
        }
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.bets[BetCounter] * Lines;
        uiManager.InitialiseUIData(SocketManager.initUIData.paylines);

    }

    #region InitialFunctions
    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < Tempimages.Count; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, 14);
                Tempimages[i].slotImages[j].sprite = myImages[randomIndex];
            }
        }
    }

    internal void SetInitialUI()
    {
        BetCounter = 0;
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.bets[BetCounter] * Lines).ToString();
        if (TotalWin_text) TotalWin_text.text = "0.000";
        if (balance_text) balance_text.text = SocketManager.playerdata.balance.ToString("F3");
        currentbalance = SocketManager.playerdata.balance;
        currentTotalBet = SocketManager.initialData.bets[BetCounter] * Lines;
        Comparebalance();
        uiManager.InitialiseUIData(SocketManager.initUIData.paylines);
    }
    #endregion

    private void OnApplicationFocus(bool focus)
    {
        audioController.CheckFocusFunction(focus, CheckSpinAudio);
    }


    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();
        switch (val)
        {
            case 12:
                animScript.doTweenAnimation = true;
                break;
            case 11:
                animScript.doTweenAnimation = true;
                break;
            case 13:
                animScript.doTweenAnimation = true;
                break;
            case 9:
                for (int i = 0; i < WildBuffalo_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(WildBuffalo_Sprite[i]);
                }
                animScript.AnimationSpeed = 15f;
                animScript.doTweenAnimation = false;
                break;
            case 5:
                animScript.doTweenAnimation = true;
                break;
            case 6:
                animScript.doTweenAnimation = true;
                break;
            case 7:
                animScript.doTweenAnimation = true;
                break;
            case 8:
                animScript.doTweenAnimation = true;
                break;
            case 0:
                animScript.doTweenAnimation = true;
                break;
            case 1:
                animScript.doTweenAnimation = true;
                break;
            case 2:
                animScript.doTweenAnimation = true;
                break;
            case 3:
                animScript.doTweenAnimation = true;
                break;
            case 4:
                animScript.doTweenAnimation = true;
                break;
            case 10:
                animScript.doTweenAnimation = true;
                break;

        }
    }

    #region SlotSpin

    private void StartSlots(bool autoSpin = false)
    {
        if (audioController) audioController.PlaySpinButtonAudio();
        TotalWin_text.text = "0.000";
        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }
        WinningsAnim(false);
        if (SlotStart_Button) SlotStart_Button.interactable = false;
        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }
        PayCalculator.ResetLines();
        tweenroutine = StartCoroutine(TweenRoutine());
    }

    private IEnumerator boostAnimFunc(int tweenNum)
    {
        if (tweenNum > 0)
        {
            int boostchance = UnityEngine.Random.Range(0, 30);
            if (boostchance < 3)
            {
                boostDone = false;
                boost_obj.gameObject.SetActive(true);
                alltweens[tweenNum].timeScale = 14f;
                boost_obj.position = boost_Positions[tweenNum].position;
                audioController.PlayBoostSpinAudio();
                yield return new WaitForSeconds(boostDuration);

                alltweens[tweenNum].timeScale = 1f;
                boost_obj.gameObject.SetActive(false);
            }

            boostDone = true;
        }
        else
        {
            boostDone = true;
        }
    }

    internal void skipAnim()
    {
        uiManager.AnimSkip_Button.gameObject.SetActive(false);
        delayTime = 0;
    }

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
        }
    }






    private IEnumerator TweenRoutine()
    {
        Debug.Log("Dev Test :" + 1);
        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }
        uiManager.AnimSkip_Button.gameObject.SetActive(false);
        if (currentbalance < currentTotalBet && !IsFreeSpin)
        {
            Comparebalance();
            StopAutoSpin();
            yield return new WaitForSeconds(1);
            ToggleButtonGrp(true);
            yield break;
        }


        CheckSpinAudio = true;

        IsSpinning = true;

        ToggleButtonGrp(false);

        //    Debug.Log("Dev Test :" + 2);
        for (int i = 0; i < numberOfSlots; i++)
        {
            InitializeTweening(Slot_Transform[i]);
        }
        if (!IsTurboOn && !IsFreeSpin && !IsAutoSpin)
        {
            StopSpin_Button.gameObject.SetActive(true);
        }
        if (!IsFreeSpin)
        {
            balanceDeduction();
        }

        //  Debug.Log("Dev Test :" + 3);
        SocketManager.AccumulateResult(BetCounter);
        yield return new WaitUntil(() => SocketManager.isResultdone);
        //     Debug.Log("Dev Test :" + 4);

        bonus_AnimString.Clear();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                //  Debug.Log("Dev Test : " + 1);
                int resultNum = int.Parse(SocketManager.resultData.matrix[i][j]);
                // Debug.Log("Dev Test : " + 2);
                if (images[j].slotImages[i]) images[j].slotImages[i].sprite = myImages[resultNum];
                // Debug.Log("Dev Test : " + 3);
                if (Tempimages[j].slotImages[i]) Tempimages[j].slotImages[i].sprite = myImages[resultNum];
                // Debug.Log("Dev Test : " + 4);
                if (SocketManager.resultData.isFreeSpin && resultNum == 13 || resultNum == 12)
                {
                    bonus_AnimString.Add(i.ToString() + "," + j.ToString());
                }

                PopulateAnimationSprites(Tempimages[j].slotImages[i].GetComponent<ImageAnimation>(), resultNum);
                //  Debug.Log("Dev Test : " + 6);
                // Tempimages[j].slotImages[i].GetComponent<Image>().sprite = myImages[resultNum];
            }
        }
        //   Debug.Log("Dev Test :" + 5);
        boostDone = true;
        if (IsTurboOn || IsFreeSpin)
        {


            StopSpinToggle = true;
            yield return new WaitForSeconds(0.1f);
        }
        else
        {

            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(0.1f);
                if (StopSpinToggle)
                {
                    break;
                }
            }
            StopSpin_Button.gameObject.SetActive(false);
        }

        //    Debug.Log("Dev Test :" + 6);
        for (int i = 0; i < Slot_Transform.Length; i++)
        {
            yield return StopTweening(5, Slot_Transform[i], i, StopSpinToggle);
        }
        StopSpinToggle = false;

        //  Debug.Log("Dev Test :" + 7);
        yield return alltweens[^1].WaitForCompletion();
        KillAllTweens();

        //   Debug.Log("Dev Test :" + 8);
        if (SocketManager.resultData.payload.winAmount > 0)
        {
            SpinDelay = 1.2f;
        }
        else
        {
            SpinDelay = 0.2f;
        }
        //   Debug.Log("Dev Test :" + 9);


        CheckForFeaturesAnimation();

        if (SocketManager.resultData.isGoldenBonusTriggered || SocketManager.resultData.issmallBonusTriggered || SocketManager.resultData.ismediumBonusTriggered || SocketManager.resultData.islargeBonusTriggered)
        {
            yield return new WaitForSeconds(2f);
            WinningsAnim(false);
            StopGameAnimation();
        }
        else
        {


            // if (!IsAutoSpin && !IsFreeSpin && !SocketManager.resultData.isFreeSpinTriggered)
            // {
            //     ToggleButtonGrp(true); ;
            // }
        }
        //     Debug.Log("Dev Test :" + 10);
        CheckAnimation = true;
        // CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit, bonus_AnimString, SocketManager.resultData.jackpot);
        if (SocketManager.resultData.payload.winAmount > 0)
        {
            List<int> winLine = new();
            foreach (var item in SocketManager.resultData.payload.wins)
            {
                winLine.Add(item.line);
            }
            CheckPayoutLineBackend(winLine);
            //  if (m_Gamble_Button) m_Gamble_Button.interactable = true;

        }
        else
        {
            CheckAnimation = false;
        }
        //   Debug.Log("Dev Test :" + 11);
        if (TotalWin_text) TotalWin_text.text = SocketManager.resultData.payload.winAmount.ToString("F3");
        if (balance_text) balance_text.text = SocketManager.playerdata.balance.ToString("F3");
        yield return new WaitUntil(() => !CheckAnimation);
        //   Debug.Log("Dev Test :" + 12);


        List<int> points_anim = null;
        isBonusGame = false;
        CheckPopups = false;
        wheelType = bonusWheelType.none;
        List<int> wheelFeature = new List<int>();
        spinDone = true;
        //    Debug.Log("Dev Test :" + 13);
        if (SocketManager.resultData.issmallBonusTriggered)
        {
            Debug.Log("ranSmall");
            spinDone = false;
            wheelType = bonusWheelType.small;
            isBonusGame = true;
            CheckPopups = true;
            wheelFeature = SocketManager.bonusdata.bonus.smallWheelFeature.featureValues;
        }
        if (SocketManager.resultData.ismediumBonusTriggered)
        {
            spinDone = false;
            Debug.Log("ranMedium");
            wheelType = bonusWheelType.medium;
            isBonusGame = true;
            CheckPopups = true;
            wheelFeature = SocketManager.bonusdata.bonus.mediumWheelFeature.featureValues;
        }
        if (SocketManager.resultData.islargeBonusTriggered)
        {
            spinDone = false;
            Debug.Log("ranlarge");
            wheelType = bonusWheelType.large;
            isBonusGame = true;
            CheckPopups = true;
            wheelFeature = SocketManager.bonusdata.bonus.largeWheelFeature.featureValues;
        }




        //  Debug.Log("Dev Test :" + 14);
        //  Debug.Log("Dev Test :" + 1 + CheckPopups);

        if (isBonusGame)
        {
            if (SocketManager.resultData.bonusIndex > 3 && SocketManager.resultData.payload.wins.Count > 0)
            {
                if (TotalWin_text) TotalWin_text.text = SocketManager.resultData.payload.winAmount.ToString("F3");
                double lineWin = SocketManager.resultData.payload.winAmount;
                double multiplier = (double)wheelFeature[SocketManager.resultData.bonusIndex];
                Debug.Log(lineWin + "  " + multiplier);
                lineWin = lineWin / multiplier;
                if (TotalWin_text) TotalWin_text.text = lineWin.ToString("F3");
            }
            else if (SocketManager.resultData.freeSpinCount > 0 && SocketManager.resultData.payload.wins.Count > 0)
            {
                if (TotalWin_text) TotalWin_text.text = SocketManager.resultData.payload.winAmount.ToString("F3");
                if (balance_text) balance_text.text = SocketManager.playerdata.balance.ToString("F3");
            }

        }
        //   Debug.Log("Dev Test :" + 15);


        if (TotalWin_text) TotalWin_text.text = SocketManager.resultData.payload.winAmount.ToString("F3");
        if (balance_text) balance_text.text = SocketManager.playerdata.balance.ToString("F3");
        if (isBonusGame)
        {

            CheckBonusGame();
        }

        //  Debug.Log("Dev Test :" + 16);
        //  Debug.Log("Dev Test :" + 2 + CheckPopups);

        yield return new WaitUntil(() => !CheckPopups);
        //   Debug.Log("Dev Test :" + 2.5);

        delayTime = 0.3f;

        CheckPopups = true;
        // if (isBonusGame)
        // {

        //     CheckBonusGame();
        // }

        balanceTween?.Kill();
        currentbalance = SocketManager.playerdata.balance;
        if (!isBonusGame && SocketManager.resultData.payload.wins.Count == 0)
        {

            CheckWinPopups();
        }
        //  Debug.Log("Dev Test :" + 17);

        //   Debug.Log("Dev Test :" + 5);

        yield return new WaitUntil(() => spinDone);
        //  Debug.Log("Dev Test :" + 6 + IsAutoSpin);



        //   Debug.Log("Dev Test :" + 19);



        if (TotalWin_text) TotalWin_text.text = SocketManager.resultData.payload.winAmount.ToString("F3");
        if (balance_text) balance_text.text = SocketManager.playerdata.balance.ToString("F3");
        if (SocketManager.resultData.isFreeSpin || SocketManager.resultData.isFreeSpinTriggered)
        {

            if (IsFreeSpin)
            {
                IsFreeSpin = false;
                if (FreeSpinRoutine != null)
                {
                    StopCoroutine(FreeSpinRoutine);
                    FreeSpinRoutine = null;
                }
            }

            uiManager.FreeSpinProcess((int)SocketManager.resultData.freeSpinCount);

            if (IsAutoSpin)
            {
                WasAutoSpinOn = true;
                StopAutoSpin();
                //  yield return new WaitForSeconds(0.1f);
            }
        }
        //   Debug.Log("Dev Test :" + 20);
        if (!IsAutoSpin && !SocketManager.resultData.isFreeSpin)
        {
            //     Debug.Log("calledfromhereintweentwo");
            //    Debug.Log("Dev Test :" + 8 + IsAutoSpin);
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
            IsSpinning = false;
        }

        //  Debug.Log("Dev Test :" + 21);
        if (IsAutoSpin)
        {
            callAutoSpinAgain();
        }
        //  Debug.Log("Dev Test :" + 22);
        // else if (!IsFreeSpin && !IsAutoSpin)
        // {
        //     ToggleButtonGrp(true);
        // }
        // else if (!IsFreeSpin && !IsAutoSpin)
        // {
        //     ToggleButtonGrp(true);
        // }


    }

    private void balanceDeduction()
    {
        double bet = 0;
        double balance = 0;
        try
        {
            bet = double.Parse(TotalBet_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            balance = double.Parse(balance_text.text);

        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }
        double initAmount = balance;

        balance = balance - bet;
        if (balance_text) balance_text.text = balance.ToString("F3");

    }

    internal void CheckWinPopups()
    {
        if (SocketManager.resultData.payload.winAmount >= currentTotalBet * 10 && SocketManager.resultData.payload.winAmount < currentTotalBet * 15)
        {
            uiManager.PopulateWin(1, SocketManager.resultData.payload.winAmount);
        }
        else if (SocketManager.resultData.payload.winAmount >= currentTotalBet * 15 && SocketManager.resultData.payload.winAmount < currentTotalBet * 20)
        {
            uiManager.PopulateWin(2, SocketManager.resultData.payload.winAmount);
        }
        else if (SocketManager.resultData.payload.winAmount >= currentTotalBet * 20)
        {
            uiManager.PopulateWin(3, SocketManager.resultData.payload.winAmount);
        }
        else
        {

            CheckPopups = false;
        }
    }

    internal void CheckBonusGame()
    {
        if (wheelType != bonusWheelType.none)
        {
            _bonusManager.StartBonus(SocketManager.resultData.bonusIndex, wheelType);
        }
        else
        {
            Debug.Log("checkWinPopUpsCalledFromHereCheckBonus");
            CheckWinPopups();
        }

    }


    private void CheckPayoutLineBackend(List<int> LineId, double jackpot = 0)
    {

        List<int> y_points = null;
        if (LineId.Count > 0)
        {
            if (Win_Object) Win_Object.SetActive(true);
            if (jackpot <= 0)
            {
                if (audioController) audioController.PlayWLAudio("win");
            }


            if (jackpot > 0)
            {
                if (audioController) audioController.PlayWLAudio("megaWin");
                for (int i = 0; i < Tempimages.Count; i++)
                {
                    for (int k = 0; k < Tempimages[i].slotImages.Count; k++)
                    {
                        StartGameAnimation(Tempimages[i].slotImages[k].gameObject);
                    }
                }
            }

            WinningsAnim(true);
        }
        else
        {

            if (audioController) audioController.StopWLAaudio();
        }

        CheckSpinAudio = false;
        if (SocketManager.resultData.freeSpinCount > 0)
        {
            AutoSpinStop_Button.interactable = false;
        }
        else
        {
            AutoSpinStop_Button.interactable = true;
        }

        if (LineId.Count > 0)
        {
            if (SocketManager.resultData.freeSpinCount > 0)
            {
                uiManager.AnimSkip_Button.gameObject.SetActive(true);
                CheckPopups = true;
            }

            if (BoxAnimRoutine != null)
            {
                StopCoroutine(BoxAnimRoutine);

                BoxAnimRoutine = null;
            }

            BoxAnimRoutine = StartCoroutine(WinAnimation(SocketManager.resultData.payload.wins));

        }
        else
        {
            CheckAnimation = false;
        }
    }

    List<List<string>> ConvertResult(List<int> LineId)
    {
        List<List<string>> coords = new();

        for (int j = 0; j < LineId.Count; j++)
        {
            // Create a new list for this line
            List<string> lineCoords = new();

            for (int k = 0; k < SocketManager.resultData.payload.wins[j].positions.Count; k++)
            {
                int rowIndex = SocketManager.initialData.lines[LineId[j]][k];
                int columnIndex = k;

                string kel = rowIndex.ToString() + "," + columnIndex.ToString();
                lineCoords.Add(kel);
            }

            coords.Add(lineCoords); // Add the line's coordinates list
        }

        return coords;
    }
    void callAutoSpinAgain()
    {

        if (AutoSpinStop_Button.gameObject.activeSelf)
        {
            AutoSpin();
        }
    }
    private IEnumerator WinAnimation(List<Win> wins)
    {
        if (SocketManager.resultData.payload.winAmount > 0)
        {
            // Keep playing animations until one of the stop conditions is true
            while (true)
            {
                for (int i = 0; i < wins.Count; i++)
                {
                    int LineIds = wins[i].line;
                    for (int k = 0; k < SocketManager.resultData.payload.wins[i].positions.Count; k++)
                    {
                        int row = SocketManager.initialData.lines[LineIds][k];
                        int col = k;

                        var symbol = Tempimages[col].slotImages[row];
                        StartGameAnimation(symbol.gameObject);
                    }

                    // Timing based on spin mode
                    if (IsAutoSpin || IsTurboOn)
                        yield return new WaitForSeconds(0.3f);
                    else
                        yield return new WaitForSeconds(0.6f);

                    setactivefalse();


                }
                // ✅ Stop immediately if any condition is true
                if (IsAutoSpin || IsTurboOn || IsFreeSpin ||
                    SocketManager.resultData.isFreeSpinTriggered ||
                    SocketManager.resultData.isGoldenBonusTriggered ||
                    SocketManager.resultData.issmallBonusTriggered ||
                    SocketManager.resultData.ismediumBonusTriggered ||
                    SocketManager.resultData.islargeBonusTriggered)

                {
                    CheckAnimation = false;
                    yield break; // exits coroutine cleanly
                }
                else
                {
                    CheckAnimation = false;
                }
            }
        }
    }
    void setactivefalse()
    {
        for (int i = 0; i < Tempimages.Count; i++)
        {
            for (int j = 0; j < Tempimages[i].slotImages.Count; j++)
            {
                var symbol = Tempimages[i].slotImages[j];
                symbol.gameObject.SetActive(false);
            }
        }
    }



    private void WinningsAnim(bool IsStart)
    {
        if (IsStart)
        {
            if (Win_Object) Win_Object.SetActive(true);
            WinTween = TotalWin_text.gameObject.GetComponent<RectTransform>().DOScale(new Vector2(1.2f, 1.2f), 0.3f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
        }
        else
        {
            WinTween.Kill();
            TotalWin_text.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
            if (Win_Object) Win_Object.SetActive(false);
        }
    }

    #endregion
    private void CheckForFeaturesAnimation()
    {
        bool playScatter = false;
        bool playBonus = false;
        bool playFreespin = false;
        if (SocketManager.resultData.isGoldenBonusTriggered)
        {
            playScatter = true;
        }
        if (SocketManager.resultData.issmallBonusTriggered || SocketManager.resultData.ismediumBonusTriggered || SocketManager.resultData.islargeBonusTriggered)
        {
            playBonus = true;
        }
        // if (SocketManager.resultData.isFreeSpinTriggered)
        // {
        //     playFreespin = true;
        // }
        PlayFeatureAnimation(playScatter, playBonus, playFreespin);
    }
    private void PlayFeatureAnimation(bool scatter = false, bool bonus = false, bool freeSpin = false)
    {
        for (int i = 0; i < SocketManager.resultData.matrix.Count; i++)
        {
            for (int j = 0; j < SocketManager.resultData.matrix[i].Count; j++)
            {

                if (int.TryParse(SocketManager.resultData.matrix[i][j], out int parsedNumber))
                {
                    if (scatter && parsedNumber == 13)
                    {
                        WinningsAnim(true);
                        StartGameAnimation(Tempimages[j].slotImages[i].gameObject);
                    }
                    if (bonus && parsedNumber == 12)
                    {
                        WinningsAnim(true);
                        StartGameAnimation(Tempimages[j].slotImages[i].gameObject);
                    }
                    if (freeSpin && parsedNumber == 10)
                    {
                        WinningsAnim(true);
                        StartGameAnimation(Tempimages[j].slotImages[i].gameObject);
                    }
                }

            }
        }
    }

    internal void CallCloseSocket()
    {
        StartCoroutine(SocketManager.CloseSocket());
    }


    void ToggleButtonGrp(bool toggle)
    {
        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (MaxBet_Button) MaxBet_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if (TBetMinus_Button) TBetMinus_Button.interactable = toggle;
        if (TBetPlus_Button) TBetPlus_Button.interactable = toggle;

    }


    private void StartGameAnimation(GameObject animObjects)
    {
        // Debug.Log(animObjects.transform.name + "Ashu Test:" + animObjects.transform.parent.parent.name + "Ashu Test:" + animObjects.transform.parent.name);

        // animObjects.transform.parent.parent.gameObject.SetActive(true);

        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
        temp.StartAnimation();
        TempList.Add(temp);
        animObjects.SetActive(true);
    }


    private void StopGameAnimation()
    {

        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();

        }
        TempList.Clear();
        TempList.TrimExcess();
        if (BoxAnimRoutine != null)
        {
            StopCoroutine(BoxAnimRoutine);
            BoxAnimRoutine = null;
        }
        if (Win_Object) Win_Object.SetActive(false);
        for (int i = 0; i < Tempimages.Count; i++)
        {
            foreach (Image s in Tempimages[i].slotImages)
            {
                s.gameObject.SetActive(false);
            }
        }
        TempList.Clear();
        TempList.TrimExcess();
    }


    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.2f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Restart).SetDelay(0);
        tweener.Play();
        alltweens.Add(tweener);
    }


    private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index, bool isStop)
    {

        if (!isStop)
        {
            StartCoroutine(boostAnimFunc(index));
            yield return new WaitUntil(() => boostDone);
        }
        alltweens[index].Kill();
        int tweenpos = (reqpos * IconSizeFactor) - IconSizeFactor;
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        alltweens[index] = slotTransform.DOLocalMoveY(-tweenpos + 100, 0.5f).SetEase(Ease.OutElastic).OnComplete(delegate
        {
            if (!isStop)
            {
                Debug.Log("playing stop sound");
                audioController.PlayWLAudio("spinStop");
            }
            else
            {
                if (index == alltweens.Count - 1)
                {
                    audioController.PlayWLAudio("spinStop");
                }
            }

        });
        if (!isStop)
        {
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            yield return null;
        }
    }

    private void KillAllTweens()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion

}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}





