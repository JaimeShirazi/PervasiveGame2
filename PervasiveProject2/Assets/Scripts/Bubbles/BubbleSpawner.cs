using System.Collections.Generic;
using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    private const float SPEED_MIN = 0.1f;
    private const float SPEED_MAX = 0.2f;
    public enum Side
    {
        Top, Right, Bottom, Left
    }
    public static Vector2 GetHalfCameraSize()
    {
        float ratio = Screen.width / (float)Screen.height;
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
    public static float GetRandomSpeed() => Mathf.Lerp(SPEED_MIN, SPEED_MAX, MathUtils.SampleBeta(2f, 2f));

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
    public static bool IsOffscreenForGood(Vector2 position, Vector2 velocity)
    {
        if (!IsOffscreen(position, out Vector2 offset)) return false;

        if (offset.x != 0)
        {
            if (Mathf.Sign(offset.x) == Mathf.Sign(velocity.x))
            {
                return true;
            }
        }
        if (offset.y != 0)
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
    public static void DebugLilypadCheck(Vector2 position, float radius)
    {
        for (int i = 0; i < lilypads.Count; i++)
        {
            lilypads[i].CheckBubble(position, radius);
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
