using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BoostFxController : MonoBehaviour
{
    public void PlayHiddenSwapFx(Image hiddenImage)
    {
        if (hiddenImage == null || !hiddenImage.gameObject.activeInHierarchy)
        {
            return;
        }

        Vector3 baseScale = hiddenImage.transform.localScale;
        float scaleFactor = Mathf.Max(Mathf.Abs(baseScale.x), Mathf.Abs(baseScale.y), Mathf.Abs(baseScale.z));
        if (scaleFactor <= 0f)
        {
            scaleFactor = 1f;
        }

        hiddenImage.transform.DOKill();
        hiddenImage.DOKill();
        hiddenImage.color = Color.white;

        Sequence seq = DOTween.Sequence();
        seq.Append(hiddenImage.transform.DOPunchScale(new Vector3(0.14f, 0.14f, 0f) * scaleFactor, 0.18f, 8, 0.8f));
        seq.Join(hiddenImage.DOColor(new Color(1f, 0.95f, 0.75f, 1f), 0.09f));
        seq.Append(hiddenImage.DOColor(Color.white, 0.12f));
        seq.OnComplete(() =>
        {
            hiddenImage.transform.localScale = baseScale;
        });
    }

    public void PlayHiddenRemoveFx(Image hiddenImage)
    {
        if (hiddenImage == null || !hiddenImage.gameObject.activeInHierarchy)
        {
            return;
        }

        TrayItem ownerTray = hiddenImage.GetComponentInParent<TrayItem>();

        Vector3 baseScale = hiddenImage.transform.localScale;
        float scaleFactor = Mathf.Max(Mathf.Abs(baseScale.x), Mathf.Abs(baseScale.y), Mathf.Abs(baseScale.z));
        if (scaleFactor <= 0f)
        {
            scaleFactor = 1f;
        }

        hiddenImage.transform.DOKill();
        hiddenImage.DOKill();
        hiddenImage.color = Color.white;

        Sequence seq = DOTween.Sequence();
        seq.Append(hiddenImage.transform.DOPunchScale(new Vector3(0.18f, 0.18f, 0f) * scaleFactor, 0.12f, 8, 0.75f));
        seq.Append(hiddenImage.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));
        seq.Join(hiddenImage.DOFade(0f, 0.2f));
        seq.OnComplete(() =>
        {
            hiddenImage.gameObject.SetActive(false);
            hiddenImage.transform.localScale = baseScale;
            hiddenImage.color = Color.white;

            if (ownerTray != null && ownerTray.gameObject.activeInHierarchy && !ownerTray.HasAnyFood())
            {
                ownerTray.gameObject.SetActive(false);
            }
        });
    }

    public IEnumerator ResolveBoost(bool waitForVisibleAnimation, bool isLevelComplete, List<GrillStation> grillStations)
    {
        if (waitForVisibleAnimation)
        {
            // Hidden remove effect lasts about 0.32s; wait a bit longer so state is finalized.
            yield return new WaitForSeconds(0.36f);
        }

        if (isLevelComplete || grillStations == null)
        {
            yield break;
        }

        foreach (GrillStation grill in grillStations)
        {
            if (grill == null || !grill.gameObject.activeInHierarchy) continue;
            if (grill.TrayContainer == null || !grill.TrayContainer.gameObject.activeInHierarchy) continue;

            grill.ResolveAfterBoost();
        }
    }
}
