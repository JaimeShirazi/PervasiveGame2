using UnityEngine;
using TMPro;

public class ScorePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private LilypadVisual.FrogColour colour;
    private void OnEnable()
    {
        switch (colour)
        {
            case LilypadVisual.FrogColour.Pink:
            default:
                ScoreCounter.OnPinkScore += OnScore;
                break;
            case LilypadVisual.FrogColour.Purple:
                ScoreCounter.OnPurpleScore += OnScore;
                break;
        }
    }
    private void OnDisable()
    {
        switch (colour)
        {
            case LilypadVisual.FrogColour.Pink:
            default:
                ScoreCounter.OnPinkScore -= OnScore;
                break;
            case LilypadVisual.FrogColour.Purple:
                ScoreCounter.OnPurpleScore -= OnScore;
                break;
        }
    }
    private float animStart;
    private Vector3 from, to;
    void OnScore(int score)
    {
        text.text = score.ToString();
        animStart = Time.time;
        from = transform.parent.position + (((transform.parent.up * 0.274f) - (Vector3.forward * 0.15f)) * transform.parent.localScale.x);
        to = transform.parent.position + (((transform.parent.up * 0.37f) - (Vector3.forward * 0.15f)) * transform.parent.localScale.x);
    }
    private void Update()
    {
        float animTime = Time.time - animStart;
        float alpha = Mathf.Min(Mathf.InverseLerp(0, 0.2f, animTime), Mathf.InverseLerp(4f, 3f, animTime));

        transform.position = Vector3.Lerp(from, to, animTime / 4f);

        transform.rotation = Quaternion.identity;

        text.color = new Color(1, 1, 1, alpha);
    }
}
