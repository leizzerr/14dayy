using UnityEngine;
using UnityEngine.UI;

public class SanityUI : MonoBehaviour
{
    public Image fillImage;
    public Image trailImage;
    public float trailCatchUpSharpness = 0.3f;

    float trailValue = 1f;

    public void SetFill(float value)
    {
        if (fillImage != null) fillImage.fillAmount = value;

        if (trailImage != null)
        {
            trailValue = value < trailValue
                ? Mathf.Lerp(trailValue, value, Time.deltaTime * trailCatchUpSharpness)
                : value;
            trailImage.fillAmount = trailValue;
        }
    }
}
