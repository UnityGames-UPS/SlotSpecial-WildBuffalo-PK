using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ImageAnimation : MonoBehaviour
{
	public enum ImageState
	{
		NONE,
		PLAYING,
		PAUSED
	}


	[SerializeField] private RectTransform thisTransfom;

    public static ImageAnimation Instance;

	public List<Sprite> textureArray;

	public Image rendererDelegate;

	public bool useSharedMaterial = true;

	public bool doTweenAnimation = false;

	public bool doLoopAnimation = true;
	[SerializeField] private bool StartOnAwake;

	[HideInInspector]
	public ImageState currentAnimationState;

	private int indexOfTexture;

	private float idealFrameRate = 0.0416666679f;

	private float delayBetweenAnimation;

	public float AnimationSpeed = 5f;

	public float delayBetweenLoop;
	private Tweener tweenAnim;
	
	public int count;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		if(StartOnAwake)
		{
			StartAnimation();
		}
	}

	private void OnEnable()
	{
        if (StartOnAwake && textureArray.Count > 0 || StartOnAwake && doTweenAnimation)
		{
			thisTransfom.localScale = new Vector2(1f, 1f);
            StartAnimation();
        }
      

    }

	private void OnDisable()
	{
		//rendererDelegate.sprite = textureArray[0];
		
        StopAnimation();
	}

	private void AnimationProcess()
	{
		
			SetTextureOfIndex();
			indexOfTexture++;

			if (indexOfTexture == textureArray.Count)
			{
				indexOfTexture = 0;
				if (doLoopAnimation)
				{
					Invoke("AnimationProcess", delayBetweenAnimation + delayBetweenLoop);
				}
			}
			else
			{
				Invoke("AnimationProcess", delayBetweenAnimation);
			}
		
		

	}

	public void StartAnimation()
	{
		
		if (!doTweenAnimation)
		{
			indexOfTexture = 0;
			if (currentAnimationState == ImageState.NONE)
			{
				RevertToInitialState();
				delayBetweenAnimation = idealFrameRate * (float)textureArray.Count / AnimationSpeed;
				currentAnimationState = ImageState.PLAYING;
				Invoke("AnimationProcess", delayBetweenAnimation);
			}
		}
		else
		{
            thisTransfom.localScale = new Vector2(1f, 1f);
            tweenAnim = thisTransfom.DOScale(new Vector2(1.2f, 1.2f),0.2f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);			
		}
	}

	public void PauseAnimation()
	{
		if (currentAnimationState == ImageState.PLAYING)
		{
           
            CancelInvoke("AnimationProcess");
			currentAnimationState = ImageState.PAUSED;
		}
	}

	public void ResumeAnimation()
	{
		if (currentAnimationState == ImageState.PAUSED && !IsInvoking("AnimationProcess"))
		{
			Invoke("AnimationProcess", delayBetweenAnimation);
			currentAnimationState = ImageState.PLAYING;
		}
	}

	public void StopAnimation()
	{
		if (!doTweenAnimation)
		{
			if (currentAnimationState != 0)
			{
				rendererDelegate.sprite = textureArray[0];
				CancelInvoke("AnimationProcess");
				currentAnimationState = ImageState.NONE;
			}
		}
		else
        {
			
            DOTween.Kill(thisTransfom);
            thisTransfom.localScale = new Vector2(1f, 1f);
           
           
        }

	}

	public void RevertToInitialState()
	{
		indexOfTexture = 0;
		if (!doTweenAnimation)
		{
			SetTextureOfIndex();
		}

		
	}

	private void SetTextureOfIndex()
	{
		
		if (useSharedMaterial)
		{
			rendererDelegate.sprite = textureArray[indexOfTexture];
		}
		else
		{
			rendererDelegate.sprite = textureArray[indexOfTexture];
		}
	}
}
