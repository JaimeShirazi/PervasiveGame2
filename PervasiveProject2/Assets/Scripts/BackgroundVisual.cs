#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class BackgroundVisual : MonoBehaviour
{
    [SerializeField] private MeshRenderer render;
    [SerializeField] private LilypadVisual lilypad1;
    [SerializeField] private LilypadVisual lilypad2;

    void Update()
    {
        if (render == null || lilypad1 == null || lilypad2 == null) return;

        Vector2 size = BubbleSpawner.GetHalfCameraSize();

        for (int i = 1; i < 3; i++)
        {
            LilypadVisual target = i == 1 ? lilypad1 : lilypad2;

            float height = size.y / target.transform.localScale.y;
            float width = height * (size.x / size.y);

            Vector4 padParams = new Vector4(width, height, 0.5f - target.xRatio * width, (height - 1) * -0.5f);
            float padAngle = target.transform.localEulerAngles.z;

#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                render.sharedMaterial.SetVector("_LilypadShadow" + i, padParams);
                render.sharedMaterial.SetFloat("_LilypadAngle" + i, padAngle);
            }
            else
            {
#endif
                render.material.SetVector("_LilypadShadow" + i, padParams);
                render.material.SetFloat("_LilypadAngle" + i, padAngle);
#if UNITY_EDITOR
            }
#endif
        }
    }
}
