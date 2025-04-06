using UnityEngine;

[ExecuteInEditMode]
public class ViewportRescaler : MonoBehaviour
{
    [SerializeField] private Camera cam;
    void Update()
    {
        if (cam == null) return;

        float trueAspectRatio = Screen.width / (float)Screen.height;

        float trueWidth = (Screen.height / 9f) * 16f;
        float trueHeight = (Screen.width / 16f) * 9f;

        if (Screen.width == Mathf.FloorToInt(trueWidth) || Screen.width == Mathf.CeilToInt(trueWidth)
         || Screen.height == Mathf.FloorToInt(trueHeight) || Screen.height == Mathf.CeilToInt(trueHeight))
        {
            cam.rect = new Rect(0, 0, 1, 1);
        }
        else if (trueAspectRatio > (16f / 9f))
        {
            float discrepancy = (16f / 9f) / trueAspectRatio;
            cam.rect = new Rect((1f - discrepancy) / 2f, 0, discrepancy, 1f);
        }
        else if (trueAspectRatio < (16f / 9f))
        {
            trueAspectRatio = Screen.height / (float)Screen.width;
            float discrepancy = (9f / 16f) / trueAspectRatio;
            cam.rect = new Rect(0, (1f - discrepancy) / 2f, 1f, discrepancy);
        }
    }
}
