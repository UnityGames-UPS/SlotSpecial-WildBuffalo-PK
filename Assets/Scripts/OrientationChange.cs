using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class OrientationChange : MonoBehaviour
{
  [SerializeField] private RectTransform UIWrapper;
  [SerializeField] private CanvasScaler CanvasScaler;
  [SerializeField] private float MatchWidth = 0.25f;
  [SerializeField] private float MatchHeight = 1f;
  [SerializeField] private float PortraitMatchWandH = 0.5f;
  [SerializeField] private float transitionDuration = 0.5f;
  [SerializeField] private float waitForRotation = 1f;
  [SerializeField] private float tabletScale = 0.9f;   // scale for tablets

  private Vector2 ReferenceAspect;
  private Tween matchTween;
  private Tween rotationTween;
  private Tween scaleTween;
  private Coroutine rotationRoutine;
  private bool isLandscape;

  private void Awake()
  {
    ReferenceAspect = CanvasScaler.referenceResolution;
  }

  void SwitchDisplay(string dimensions)
  {
    if (rotationRoutine != null) StopCoroutine(rotationRoutine);
    rotationRoutine = StartCoroutine(RotationCoroutine(dimensions));
  }

  IEnumerator RotationCoroutine(string dimensions)
  {
    yield return new WaitForSecondsRealtime(waitForRotation);

    string[] parts = dimensions.Split(',');
    if (parts.Length == 2 && int.TryParse(parts[0], out int width) && int.TryParse(parts[1], out int height) && width > 0 && height > 0)
    {
      Debug.Log($"Unity: Received Dimensions - Width: {width}, Height: {height}");

      isLandscape = width > height;

      // 🎯 ROTATION
      Quaternion targetRotation = isLandscape ? Quaternion.identity : Quaternion.Euler(0, 0, -90);
      if (rotationTween != null && rotationTween.IsActive()) rotationTween.Kill();
      rotationTween = UIWrapper.DOLocalRotateQuaternion(targetRotation, transitionDuration).SetEase(Ease.OutCubic);

      // 🎯 MATCH WIDTH/HEIGHT
      float currentAspectRatio = isLandscape ? (float)width / height : (float)height / width;
      float referenceAspectRatio = ReferenceAspect.x / ReferenceAspect.y;

      float targetMatch = isLandscape ?
          (currentAspectRatio > referenceAspectRatio ? MatchHeight : MatchWidth) :
          PortraitMatchWandH;

      if (matchTween != null && matchTween.IsActive()) matchTween.Kill();
      matchTween = DOTween.To(() => CanvasScaler.matchWidthOrHeight, x => CanvasScaler.matchWidthOrHeight = x, targetMatch, transitionDuration).SetEase(Ease.InOutQuad);

      // 🛡 TABLET / IPAD SAFE SCALE
      float aspect = (float)Screen.height / Screen.width;
      aspect = Mathf.Max(aspect, 1f / aspect);  // Normalize (always >= 1)

      // Tablet if aspect ratio is around 4:3 or slightly wider (1.3 to 1.6)
      bool isTablet = aspect >= 1.3f && aspect <= 1.6f;

      float targetScale = isTablet ? tabletScale : 1f;

      if (scaleTween != null && scaleTween.IsActive()) scaleTween.Kill();
      scaleTween = UIWrapper.DOScale(targetScale, transitionDuration).SetEase(Ease.OutCubic);

      Debug.Log($"matchWidthOrHeight set to: {targetMatch}, UI Scale: {targetScale}");
    }
    else
    {
      Debug.LogWarning("Unity: Invalid format received in SwitchDisplay");
    }
  }

#if UNITY_EDITOR
  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.Space))
    {
      SwitchDisplay(Screen.width + "," + Screen.height);
    }
  }
#endif
}
