using UnityEngine;

public class LilypadVisual : MonoBehaviour
{
    public enum FrogColour
    {
        Pink, Purple
    }
    [SerializeField] private FrogColour colour;
    public FrogColour Colour => colour;

    [SerializeField] private float size = 4f;

    [Range(0f, 1f)]
    public float xRatio = 0.25f;

    [SerializeField] private MeshRenderer frog;

    [SerializeField] private Material awake, asleep;

    const float SPEED = 20f;
    private float target;
    public float angle;
    public void SetSegment(int target)
    {
        if (target < 0)
        {
            frog.material = asleep;
        }
        else
        {
            frog.material = awake;
            this.target = target * (360f / 7f);
        }
    }
    void OnEnable()
    {
        BubbleSpawner.AddLilypad(this);
    }
    void OnDisable()
    {
        BubbleSpawner.RemoveLilypad(this);
    }
    void Update()
    {
        Vector2 camHalfSize = BubbleSpawner.GetHalfCameraSize();

        transform.position = new Vector3(Mathf.Lerp(-1, 1, xRatio) * camHalfSize.x, 0, 0);

        float delta = Mathf.DeltaAngle(angle, target);
        angle += delta * Time.deltaTime * SPEED;
        angle %= 360f;
        if (angle < 0) angle += 360f;

        transform.localEulerAngles = new Vector3(0f, 0f, 360f - angle);

        transform.localScale = Vector3.one * size;
    }
    /// <summary>
    /// Reflects the dir only if the direction is coming toward the lilypad
    /// </summary>
    public Vector2 ReflectDir(Vector2 center, Vector2 dir)
    {
        Vector2 normal = (center - new Vector2(transform.position.x, transform.position.y)).normalized;

        if (Vector2.Dot(normal, dir) > 0) return dir;

        return dir - 2f * Vector2.Dot(dir, normal) * normal;
    }
    private Vector2 GetDegreesAsDir(float angle)
    {
        angle *= Mathf.Deg2Rad;
        return new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
    }
    private Vector2[] GetWedgeAsTriangle()
    {
        Vector2[] points = new Vector2[3];
        points[0] = new Vector2(transform.position.x, transform.position.y);
        Vector2 GetDir(float baseAngle, float offset)
        {
            return GetDegreesAsDir(baseAngle + offset);
        }
        points[1] = points[0] + (GetDir(angle, -(360f / 7f) * 0.5f) * transform.localScale.x * 0.5f);
        points[2] = points[0] + (GetDir(angle, (360f / 7f) * 0.5f) * transform.localScale.x * 0.5f);
        return points;
    }
    private bool CircleTouchesWedge(Vector2 center, float radius)
    {
        Vector2[] points = GetWedgeAsTriangle();
        Debug.DrawLine((Vector3)points[0] - Vector3.forward, (Vector3)points[1] - Vector3.forward, Color.red, 0.2f);
        Debug.DrawLine((Vector3)points[0] - Vector3.forward, (Vector3)points[2] - Vector3.forward, Color.blue, 0.2f);
        return CircleTouchesBorder(center, radius, points[0], points[1]) || CircleTouchesBorder(center, radius, points[0], points[2]);
    }

    // Returns true if the circle intersects the line segment
    private static bool CircleTouchesBorder(Vector2 circleCenter, float radius, Vector2 lineStart, Vector2 lineEnd)
    {
        // Vector from start to end of the line
        Vector2 lineDir = lineEnd - lineStart;
        // Vector from start of line to the circle center
        Vector2 toCircle = circleCenter - lineStart;

        // Project the circle center onto the line segment (clamped to segment)
        float t = Vector2.Dot(toCircle, lineDir.normalized);

        //You would typically clamp t to be [0, lineDir.magnitude]
        //However, in this case we don't want the touch to count if it's from outside of the wedge region
        if (t < 0 || t > lineDir.magnitude) return false;

        // Closest point on the segment to the circle center
        Vector2 closestPoint = lineStart + lineDir.normalized * t;

        Debug.DrawLine((Vector3)closestPoint - Vector3.forward, (Vector3)circleCenter - Vector3.forward, Color.green, 0.2f);

        return Vector2.Distance(closestPoint, circleCenter) < radius;
    }
    public enum BubbleResponse
    {
        None, Bounce, Pop
    }
    public BubbleResponse CheckBubble(Vector2 center, float radius)
    {
        if (GetIntersectionAngleRange(center, radius, out float angle1, out float angle2))
        {
            float wallStart = angle + (360f / 7f) * 0.5f;
            float wallEnd   = angle - (360f / 7f) * 0.5f;

            while (wallStart > wallEnd) wallStart -= 360f;
            while (angle1    > angle2)  angle1    -= 360f;
            
            bool RangeOverlaps(float a1, float a2, float b1, float b2)
            {
                bool PointIsWithin(float point, float min, float max) => (point > min && point < max);
                return PointIsWithin(a1, b1, b2) || PointIsWithin(a2, b1, b2)
                    || PointIsWithin(b1, a1, a2) || PointIsWithin(b2, a1, a2);
            }

            bool bubbleIn = RangeOverlaps(wallStart, wallEnd, angle1, angle2)
                || RangeOverlaps(wallStart, wallEnd, angle1 - 360, angle2 - 360)
                || RangeOverlaps(wallStart - 360, wallEnd - 360, angle1, angle2);

            if (bubbleIn)
            {
                return BubbleResponse.Bounce;
            }
            else
            {
                if (CircleTouchesWedge(center, radius))
                {
                    return BubbleResponse.Pop;
                }
            }
        }
        else if (Vector2.Distance(center, new Vector2(transform.position.x, transform.position.y)) < transform.localScale.x)
        {
            if (CircleTouchesWedge(center, radius))
            {
                return BubbleResponse.Pop;
            }
        }
        return BubbleResponse.None;
    }
    #region Angle functions
    /// <summary>
    /// Normalizes an angle to [0,360)
    /// </summary>
    public static float NormalizeAngle(float angle)
    {
        return (angle % 360 + 360) % 360;
    }

    /// <summary>
    /// Checks if 'angle' is within the angular range defined by 'rangeStart' and 'rangeEnd',
    /// taking into account wrapping around 360°.
    /// </summary>
    public static bool IsAngleInRange(float angle, float rangeStart, float rangeEnd)
    {
        angle = NormalizeAngle(angle);
        rangeStart = NormalizeAngle(rangeStart);
        rangeEnd = NormalizeAngle(rangeEnd);

        // If the range does not wrap around 360.
        if (rangeStart <= rangeEnd)
            return angle >= rangeStart && angle <= rangeEnd;
        else // The range wraps (e.g. from 359 to 1)
            return angle >= rangeStart || angle <= rangeEnd;
    }

    /// <summary>
    /// Determines if two angular ranges overlap.
    /// Each range is defined by a start and an end angle, and a range such as 359° to 1° 
    /// is interpreted as covering 359° -> 360° and 0° -> 1°.
    /// </summary>
    public static bool DoAngleRangesOverlap(float aStart, float aEnd, float bStart, float bEnd)
    {
        // Check if either endpoint of range A is within range B,
        // or either endpoint of range B is within range A.
        return IsAngleInRange(aStart, bStart, bEnd) || IsAngleInRange(aEnd, bStart, bEnd) ||
               IsAngleInRange(bStart, aStart, aEnd) || IsAngleInRange(bEnd, aStart, aEnd);
    }
    #endregion
    private bool GetIntersectionAngleRange(
        Vector2 centerB, float radiusB,
        out float angle1, out float angle2)
    {
        Vector2 centerA = new Vector2(transform.position.x, transform.position.y);
        float radiusA = transform.localScale.x / 2f;

        angle1 = 0f;
        angle2 = 0f;

        // Distance between the centers
        float d = Vector2.Distance(centerA, centerB);

        // Check for intersection conditions: circles must intersect in a chord.
        if (d > radiusA + radiusB || d < Mathf.Abs(radiusA - radiusB))
        {
            return false;
        }

        // Calculate the half-angle using the law of cosines (in radians)
        float cosHalfAngle = (d * d + radiusA * radiusA - radiusB * radiusB) / (2f * d * radiusA);
        // Clamp to avoid any numerical issues
        cosHalfAngle = Mathf.Clamp(cosHalfAngle, -1f, 1f);
        float halfAngle = Mathf.Acos(cosHalfAngle);

        // Calculate the angle (in radians) from centerA to centerB,
        // originally relative to the positive horizontal axis, increasing counterclockwise.
        float phi = Mathf.Atan2(centerB.y - centerA.y, centerB.x - centerA.x);

        // The two intersection points on circle A (in radians) relative to the positive x-axis.
        float rawAngle1 = phi + halfAngle;
        float rawAngle2 = phi - halfAngle;

        // Convert from radians (relative to positive x-axis) to degrees.
        // Then, transform so that 0 degrees = positive vertical axis and angles increase clockwise.
        // This transformation is: angle = (90 - (rawAngle * Rad2Deg)) mod 360.

        angle1 = (90f - rawAngle1 * Mathf.Rad2Deg) % 360f;
        angle2 = (90f - rawAngle2 * Mathf.Rad2Deg) % 360f;

        // Ensure the angles are positive (0 to 360)
        if (angle1 < 0) angle1 += 360f;
        if (angle2 < 0) angle2 += 360f;

        return true;
    }
}
