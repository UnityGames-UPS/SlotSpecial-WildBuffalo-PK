using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

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
    private TMP_Text Balance_text;
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
    private Tween BalanceTween;
    internal bool IsAutoSpin = false;
    internal bool IsFreeSpin = false;
    private bool IsSpinning = false;
    private bool CheckSpinAudio = false;
    internal bool CheckPopups = false;
    internal bool IsHoldSpin = false;
    internal int BetCounter = 0;
    private double currentBalance = 0;
    private double currentTotalBet = 0;
    protected int Lines = 20;
    [SerializeField]
    private int IconSizeFactor = 100;       
    private int numberOfSlots = 5;          
    private bool StopSpinToggle;
    private float SpinDelay=0.2f;
    private bool IsTurboOn;
    internal bool WasAutoSpinOn;
    private float boostDuration = 2f;
    private bool boostDone;
    private Coroutine BoxAnimRoutine = null;
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

        if(StopSpin_Button) StopSpin_Button.onClick.RemoveAllListeners();
        if(StopSpin_Button) StopSpin_Button.onClick.AddListener(()=> {audioController.PlayButtonAudio(); StopSpinToggle=true; StopSpin_Button.gameObject.SetActive(false);});

        if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(AutoSpin);

        if(Turbo_Button) Turbo_Button.onClick.RemoveAllListeners();
        if(Turbo_Button) Turbo_Button.onClick.AddListener(TurboToggle);

        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(() => { StopAutoSpin(); WasAutoSpinOn = false;});

        if (FSBoard_Object) FSBoard_Object.SetActive(false);

        tweenHeight = (15 * IconSizeFactor) - 280;
    }

    void TurboToggle(){
        audioController.PlayButtonAudio();
        if(IsTurboOn){
            IsTurboOn=false;
            Turbo_Button.GetComponent<ImageAnimation>().StopAnimation();
            Turbo_Button.image.sprite=TurboToggleSprites[0];
            Turbo_Button.image.color=new Color(0.86f,0.86f,0.86f,1);
        }
        else{
            IsTurboOn=true;
            Turbo_Button.GetComponent<ImageAnimation>().StartAnimation();
            Turbo_Button.image.color=new Color(1,1,1,1);
        }
    }

    #region Autospin


    internal void StartSpinRoutine()
    {
        if (!IsSpinning)
        {
            IsHoldSpin = false;
            Invoke("AutoSpinHold", 2f);
        }
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
        
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        ToggleButtonGrp(true);
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
        Debug.Log("wasautospin : " + WasAutoSpinOn);
        if(WasAutoSpinOn)
        {
            AutoSpin();
        }
        else{
            ToggleButtonGrp(true);
        }
        IsFreeSpin = false;
    }
    #endregion

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
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
        BetCounter = SocketManager.initialData.Bets.Count - 1;
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
       
    }

    private void ChangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (IncDec)
        {
            BetCounter++;
            if (BetCounter >= SocketManager.initialData.Bets.Count)
            {
                BetCounter = 0; 
            }
        }
        else
        {
            BetCounter--;
            if (BetCounter < 0)
            {
                BetCounter = SocketManager.initialData.Bets.Count - 1; 
            }
        }
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        
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
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        if (TotalWin_text) TotalWin_text.text = "0.000";
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("F3");
        currentBalance = SocketManager.playerdata.Balance;
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
        uiManager.InitialiseUIData(SocketManager.initUIData.AbtLogo.link, SocketManager.initUIData.AbtLogo.logoSprite, SocketManager.initUIData.ToULink, SocketManager.initUIData.PopLink, SocketManager.initUIData.paylines);
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
            int boostchance = UnityEngine.Random.Range(0, 15);
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


   
    private IEnumerator TweenRoutine()
    {
       
        if (currentBalance < currentTotalBet && !IsFreeSpin) 
        {
            CompareBalance();
            StopAutoSpin();
            yield return new WaitForSeconds(1);
            ToggleButtonGrp(true);
            yield break;
        }
        
       
        CheckSpinAudio = true;

        IsSpinning = true;

        ToggleButtonGrp(false);
       
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
            BalanceDeduction();
        }
        
        SocketManager.AccumulateResult(BetCounter);
        yield return new WaitUntil(() => SocketManager.isResultdone);
        if (IsAutoSpin)
        {
            WasAutoSpinOn = true;
        }
        bonus_AnimString.Clear();
        for (int j = 0; j < SocketManager.resultData.ResultReel.Count; j++)
        {
           
            List<int> resultnum = SocketManager.resultData.FinalResultReel[j]?.Split(',')?.Select(Int32.Parse)?.ToList();
            for (int i = 0; i < 5; i++)
            {            
                if (images[i].slotImages[j]) images[i].slotImages[j].sprite = myImages[resultnum[i]];
                if (Tempimages[i].slotImages[j]) Tempimages[i].slotImages[j].sprite = myImages[resultnum[i]];
                if (SocketManager.resultData.freeSpinAdded && resultnum[i] == 13 || resultnum[i] == 12)
                {
                    bonus_AnimString.Add(i.ToString() +","+ j.ToString());
                }
                PopulateAnimationSprites(Tempimages[i].slotImages[j].gameObject.GetComponent<ImageAnimation>(), resultnum[i]);
            }
        }
        boostDone = true;
        if (IsTurboOn || IsFreeSpin){

            StopSpinToggle = true;
            yield return new WaitForSeconds(0.1f);
        }
        else{
            
            for (int i=0;i<5;i++)
            {
                yield return new WaitForSeconds(0.1f);       
                if (StopSpinToggle)
                {
                    break;
                }
            }
            StopSpin_Button.gameObject.SetActive(false);
        }
       
        for (int i = 0; i < Slot_Transform.Length; i++)
        {
            yield return StopTweening(5, Slot_Transform[i], i, StopSpinToggle);
        }
        StopSpinToggle=false;

        yield return alltweens[^1].WaitForCompletion();
        KillAllTweens();

        if(SocketManager.playerdata.currentWining>0){
            SpinDelay=1.2f;
        }
        else{
            SpinDelay=0.2f;
        }
        List<int> points_anim = null;
        
        CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit, bonus_AnimString, SocketManager.resultData.jackpot);

        if (bonus_AnimString.Count>2)
        {
            if (SocketManager.resultData.linesToEmit.Count == 0)
            {
                if (Win_Object) Win_Object.SetActive(true);
                for (int i = 0; i < bonus_AnimString.Count; i++)
                {
                    
                    for (int j = 0; j < 5; j++)
                    {
                        points_anim = bonus_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();
                        int k = 0;
                      
                        while (k < points_anim.Count)
                        {
                            Tempimages[points_anim[k]].slotImages[points_anim[k + 1]].gameObject.SetActive(true);
                            k += 2;
                        }
                    }
                }

                yield return new WaitForSeconds(1);
                if (IsAutoSpin || IsFreeSpin)
                {
                    for (int i = 0; i < bonus_AnimString.Count; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            points_anim = bonus_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();
                            int k = 0;
                            Debug.Log(points_anim[k]);
                            while (k < points_anim.Count)
                            {
                                Tempimages[points_anim[k]].slotImages[points_anim[k + 1]].gameObject.SetActive(false);
                                k += 2;
                            }
                        }
                    }
                    if (Win_Object) Win_Object.SetActive(false);
                }
            }
        }



        CheckPopups = true;

        if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.currentWining.ToString("F3");
        BalanceTween?.Kill();
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("F3");
        currentBalance = SocketManager.playerdata.Balance;
        wheelType = bonusWheelType.none;
        bool isBonusGame = false;
        if (SocketManager.resultData.isSmallWheelTriggered)
        {
            wheelType = bonusWheelType.small;
            isBonusGame = true;
            CheckBonusGame();

        }
        if (SocketManager.resultData.isMediumWheelTriggered)
        {
            wheelType = bonusWheelType.medium;
            isBonusGame = true;
            CheckBonusGame();
        }
        if (SocketManager.resultData.isLargeWheelTriggered)
        {
            wheelType = bonusWheelType.large;
            isBonusGame = true;
            CheckBonusGame();
        }
           
        if (!isBonusGame && SocketManager.resultData.linesToEmit.Count == 0)
        {
            CheckWinPopups();
        }

        if (!IsAutoSpin && !IsFreeSpin)
        {
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
            IsSpinning = false;
        }

        yield return new WaitUntil(() => !CheckPopups);



        if (isBonusGame)
        {
            yield return new WaitForSeconds(1f);
        }
        
        if (SocketManager.resultData.isFreeSpin)
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
                yield return new WaitForSeconds(0.1f);
            }
        }
        
    }

    private void BalanceDeduction()
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
            balance = double.Parse(Balance_text.text);
            Debug.Log(balance);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }
        double initAmount = balance;
    
        balance = balance - bet;
        Debug.Log(initAmount + " " + balance);
        BalanceTween =DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = initAmount.ToString("F3");
        });
    }

    internal void CheckWinPopups()
    {
        if (SocketManager.resultData.WinAmout >= currentTotalBet * 10 && SocketManager.resultData.WinAmout < currentTotalBet * 15)
        {
            uiManager.PopulateWin(1, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 15 && SocketManager.resultData.WinAmout < currentTotalBet * 20)
        {
            uiManager.PopulateWin(2, SocketManager.resultData.WinAmout);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 20)
        {
            uiManager.PopulateWin(3, SocketManager.resultData.WinAmout);
        }
        else
        {
            Debug.Log("checkwinpopupsRan");
            CheckPopups = false;
        }
    }

    internal void CheckBonusGame()
    {
        if (wheelType != bonusWheelType.none)
        {
            _bonusManager.StartBonus(SocketManager.resultData.indexToStop, wheelType);
        }
        else
        {
            CheckWinPopups();
        }
       
    }

   
    private void CheckPayoutLineBackend(List<int> LineId, List<string> points_AnimString, List<string> bonusAnimlist, double jackpot = 0)
    {
        List<int> y_points = null;
        List<int> points_anim = null;
        for (int i = 0; i < Tempimages.Count; i++)
        {
            for (int j = 0; j < Tempimages[i].slotImages.Count; j++)
            {
                Tempimages[i].slotImages[j].gameObject.SetActive(false);
            }
        }
        if (LineId.Count > 0 || points_AnimString.Count > 0)
        {
            if (jackpot <= 0)
            {
                if (audioController) audioController.PlayWLAudio("win");
            }

            for (int i = 0; i < LineId.Count; i++)
            {
                y_points = y_string[LineId[i] + 1]?.Split(',')?.Select(Int32.Parse)?.ToList();
                PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count);
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
            else
            {
                for (int i = 0; i < points_AnimString.Count; i++)
                {
                    points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                    for (int k = 0; k < points_anim.Count; k++)
                    {
                        if (points_anim[k] >= 10)
                        {
                            StartGameAnimation(Tempimages[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject);
                        }
                        else
                        {
                            StartGameAnimation(Tempimages[0].slotImages[points_anim[k]].gameObject);
                        }
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
            if (IsAutoSpin)
            {
                WasAutoSpinOn = true;
                IsAutoSpin = false;
                StopCoroutine(AutoSpinCoroutine());
                Debug.Log("callBoxRoutine");
            }
           

            BoxAnimRoutine = StartCoroutine(BoxRoutine(LineId, SocketManager.resultData.symbolsToEmit));
        }
    }

    void callAutoSpinAgain()
    {
        Debug.Log("callAutoSpinAgain");
        if (AutoSpinStop_Button.gameObject.activeSelf)
        {
            AutoSpin();
        }
    }

    private IEnumerator BoxRoutine(List<int> LineIDs, List<List<string>> points_AnimString)
    {
        float delayTime = 0;
        if (WasAutoSpinOn)
        {
            delayTime = 1;
        }
        else
        {
            delayTime = 2;
        }
        
        PayCalculator.DontDestroyLines.Clear();
        PayCalculator.DontDestroyLines.TrimExcess();
        PayCalculator.ResetLines();
        List<int> points_anim = null;
        int localCount = 0;
        while (true)
        {

            if (SocketManager.resultData.freeSpinCount == 0)
            {
                if (WasAutoSpinOn)
                {
                    if (LineIDs.Count > 1)
                    {
                        if (localCount > 0)
                        {                       
                            AutoSpin();
                            break;
                        }
                        localCount++;
                    }
                    else
                    {
                        Invoke("callAutoSpinAgain", 3f);
                    }
                }
            }
            List<int> y_points = null;
            if (bonus_AnimString.Count > 2 || SocketManager.resultData.freeSpinAdded)
            {
                for (int i = 0; i < bonus_AnimString.Count; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        points_anim = bonus_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();
                        int k = 0;
                        Debug.Log(points_anim[k]);
                        while (k < points_anim.Count)
                        {
                            Tempimages[points_anim[k]].slotImages[points_anim[k + 1]].gameObject.SetActive(true);
                            k += 2;
                        }
                    }
                }

                yield return new WaitForSeconds(delayTime);

                for (int i = 0; i < bonus_AnimString.Count; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        points_anim = bonus_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();
                        int k = 0;
                        Debug.Log(points_anim[k]);
                        while (k < points_anim.Count)
                        {
                            Tempimages[points_anim[k]].slotImages[points_anim[k + 1]].gameObject.SetActive(false);
                            k += 2;
                        }
                    }
                }

            }
            if (LineIDs.Count > 0)
            {
                
                for (int i = 0; i < points_AnimString.Count; i++)
                {
                   
                    for (int j = 0; j < points_AnimString[i].Count; j++)
                    {
                        points_anim = points_AnimString[i][j]?.Split(',')?.Select(Int32.Parse)?.ToList();
                        int k = 0;
                        while (k < points_anim.Count)
                        {
                            Tempimages[points_anim[k]].slotImages[points_anim[k + 1]].gameObject.SetActive(true);
                            k += 2;
                        }
                    }
                  
                    yield return new WaitForSeconds(2f);
                   
                    for (int j = 0; j < points_AnimString[i].Count; j++)
                    {
                        points_anim = points_AnimString[i][j]?.Split(',')?.Select(Int32.Parse)?.ToList();
                        int k = 0;
                        while (k < points_anim.Count)
                        {
                            Tempimages[points_anim[k]].slotImages[points_anim[k + 1]].gameObject.SetActive(false);
                            k += 2;
                        }
                    }
                    PayCalculator.DontDestroyLines.Clear();
                    PayCalculator.DontDestroyLines.TrimExcess();
                    PayCalculator.ResetLines();
                }
            }

            for (int i = 0; i < points_AnimString.Count; i++)
            {
             
                for (int j = 0; j < points_AnimString[i].Count; j++)
                {
                    points_anim = points_AnimString[i][j]?.Split(',')?.Select(Int32.Parse)?.ToList();
                    int k = 0;
                    while (k < points_anim.Count)
                    {
                        Tempimages[points_anim[k]].slotImages[points_anim[k + 1]].gameObject.SetActive(true);
                        k += 2;
                    }
                }
            }
            yield return new WaitForSeconds(delayTime);
            for (int i = 0; i < points_AnimString.Count; i++)
            {
                for (int j = 0; j < points_AnimString[i].Count; j++)
                {
                    points_anim = points_AnimString[i][j]?.Split(',')?.Select(Int32.Parse)?.ToList();
                    int k = 0;
                    while (k < points_anim.Count)
                    {
                        Tempimages[points_anim[k]].slotImages[points_anim[k + 1]].gameObject.SetActive(false);
                        k += 2;
                    }
                }
            }
          
            PayCalculator.DontDestroyLines.Clear();
            PayCalculator.DontDestroyLines.TrimExcess();
            PayCalculator.ResetLines();
            CheckPopups = false;
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

    internal void CallCloseSocket()
    {
        SocketManager.CloseSocket();
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
        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
        temp.StartAnimation();
        TempList.Add(temp);
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
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.2f) .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Restart).SetDelay(0);
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

