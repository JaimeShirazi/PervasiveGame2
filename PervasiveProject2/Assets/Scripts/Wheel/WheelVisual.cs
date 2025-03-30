using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.HableCurve;

[RequireComponent(typeof(Image))]
public class WheelVisual : MonoBehaviour
{
    private Image wheel;
    void Awake()
    {
        wheel = GetComponent<Image>();
    }
    public void SetSegments(int segments)
    {
        wheel.material.SetFloat("_Segments", segments);
    }
    public void SetSegment(int segment)
    {
        wheel.material.SetFloat("_ActiveSegment", segment);
    }
    /// <param name="cursor">(x: [0, 1], y: [0, 1]</param>
    public void SetCursor(Vector2 cursor)
    {
        wheel.material.SetVector("_Cursor", cursor);
    }
}
