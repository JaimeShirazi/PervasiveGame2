using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    private const float SPEED_MIN = 0.06f;
    private const float SPEED_MAX = 0.14f;
    private const float SPEED_INCREASE = 0.00015f;
    private const float NEW_BUBBLE_INTERVAL = 7f;
    private const float NEW_BUBBLE_INTERVAL_INCREASE = 12f;
    private IEnumerator SpawnBubblesLoop()
    {
        while (true)
        {
            bubbles.Add(Instantiate(bubblePrefab).GetComponent<BubbleLogic>());
            bubbles[^1].Spawn();
            yield return new WaitForSeconds(NEW_BUBBLE_INTERVAL + (bubbles.Count - 1) * NEW_BUBBLE_INTERVAL_INCREASE);
        }
    }

    [SerializeField] private GameObject bubblePrefab;
    private List<BubbleLogic> bubbles = new();
    public enum Side
    {
        Top, Right, Bottom, Left
    }
    public static Vector2 GetHalfCameraSize()
    {
        //float ratio = Screen.width / (float)Screen.height;
        float ratio = 16f / 9f; //Effectively, "nuh-uh"ing the aspect ratio. Uncomment above and remove this to remove the aspect ratio lock
        float height = Camera.main.orthographicSize;
        float width = height * ratio;
        return new Vector2(width, height);
    }
    public static Side GetRandomSide() => (Side)Random.Range(0, 4);
    public static Vector2 GetSpawnPoint(Side side)
    {
        Vector2 size = GetHalfCameraSize();

        return side switch
        {
            Side.Top => new Vector2(Random.Range(-size.x, size.x), size.y),
            Side.Right => new Vector2(size.x, Random.Range(-size.y, size.y)),
            Side.Bottom => new Vector2(Random.Range(-size.x, size.x), -size.y),
            Side.Left or _ => new Vector2(-size.x, Random.Range(-size.y, size.y))
        };
    }
    public static Vector2 GetRandomDirection(Side side)
    {
        //angle is clockwise where 0 degrees is a direction vector of (0, 1)
        float angle = MathUtils.SampleBeta(5f, 5f) * 180f;

        angle += side switch
        {
            Side.Top => 90,
            Side.Right => 180,
            Side.Bottom => 270,
            Side.Left or _ => 0
        };

        angle *= Mathf.Deg2Rad;

        return new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
    }
    public static float GetRandomSpeed() => Mathf.Lerp(SPEED_MIN, SPEED_MAX, MathUtils.SampleBeta(2f, 2f)) + SPEED_INCREASE * Time.timeSinceLevelLoad;

    private static bool IsOffscreen(Vector2 position, out Vector2 offset)
    {
        offset = Vector2.zero;

        Vector2 size = GetHalfCameraSize();

        if (position.x > size.x) offset.x = position.x - size.x;
        else if (position.x < -size.x) offset.x = position.x + size.x;

        if (position.y > size.y) offset.y = position.y - size.y;
        else if (position.y < -size.y) offset.y = position.y + size.y;

        return offset != Vector2.zero;
    }
    public static bool IsOffscreenForGood(Vector2 position, Vector2 velocity, float radius)
    {
        if (!IsOffscreen(position, out Vector2 offset)) return false;

        if (Mathf.Abs(offset.x) > radius)
        {
            if (Mathf.Sign(offset.x) == Mathf.Sign(velocity.x))
            {
                return true;
            }
        }
        if (Mathf.Abs(offset.y) > radius)
        {
            if (Mathf.Sign(offset.y) == Mathf.Sign(velocity.y))
            {
                return true;
            }
        }
        return false;
    }
    private static List<LilypadVisual> lilypads = new();
    public static void AddLilypad(LilypadVisual visual) => lilypads.Add(visual);
    public static void RemoveLilypad(LilypadVisual visual) => lilypads.Remove(visual);
    public static LilypadVisual.BubbleResponse CheckLilypads(Vector2 position, float radius, out LilypadVisual sender)
    {
        sender = null;
        for (int i = 0; i < lilypads.Count; i++)
        {
            LilypadVisual.BubbleResponse response = lilypads[i].CheckBubble(position, radius);
            if (response != LilypadVisual.BubbleResponse.None)
            {
                sender = lilypads[i];
                return response;
            }
        }
        return LilypadVisual.BubbleResponse.None;
    }
    void Start()
    {
        ScoreCounter.Reset();
        StartCoroutine(SpawnBubblesLoop());
    }
}
