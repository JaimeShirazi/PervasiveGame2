using UnityEngine;

public class LilypadVisual : MonoBehaviour
{
    [SerializeField] private float size = 4f;

    [Range(0f, 1f)]
    [SerializeField] private float xRatio = 0.25f;

    [SerializeField] private MeshRenderer frog;

    [SerializeField] private Material awake, asleep;

    const float SPEED = 20f;
    private float target;
    private float angle;
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
    public Vector2 ReflectDir(Vector2 contact, Vector2 dir)
    {
        Vector2 normal = (contact - new Vector2(transform.position.x, transform.position.y)).normalized;

        return dir - 2f * Vector2.Dot(dir, normal) * normal;
    }
    private Vector2[] GetWedgeAsTriangle()
    {
        Vector2[] points = new Vector2[3];
        points[0] = new Vector2(transform.position.x, transform.position.y);
        Vector2 GetDir(float baseAngle, float offset)
        {
            float targetAngle = (baseAngle + offset) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(targetAngle), Mathf.Cos(targetAngle));
        }
        points[1] = GetDir(angle, -(7f / 360f) * 0.5f);
        points[2] = GetDir(angle, (7f / 360f) * 0.5f);
        points[1] *= transform.localScale.x; points[2] *= transform.localScale.x;
        points[1] += points[0]; points[2] += points[0];
        return points;
    }
    /*public bool IsCircleFullyInside(Vector2 circleCenter, float circleRadius)
    {
        //First, check if circle intersects any of the vertices
        Vector2[] points = GetWedgeAsTriangle();
        for (int i = 0; i < points.Length; i++)
        {
            if (Vector2.Distance(points[i], circleCenter) < circleRadius)
        }

        Vector2.Project

        // Calculate the vector from the wedge tip to the circle center.
        Vector2 toCircle = circleCenter - new Vector2(transform.position.x, transform.position.y);
        float distance = toCircle.magnitude;

        // If the circle's center is too close to the tip, the circle might extend behind the tip.
        if (distance < circleRadius)
        {
            return false;
        }

        // Convert wedge angle to radians and calculate half of it.
        float halfWedgeAngle = (7f / 360f) * Mathf.Deg2Rad / 2f;

        // Get the angle between the wedge direction and the vector to the circle.
        // Dot product gives the cosine of the angle between the two normalized vectors.
        float angleOffset = Mathf.Acos(Vector2.Dot(toCircle.normalized, new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad))));

        // Calculate the angular radius of the circle as seen from the wedge tip.
        float circleAngularRadius = Mathf.Asin(circleRadius / distance);

        // The circle is fully inside if the sum of the offset and the circle's angular radius is within half the wedge's angle.
        return angleOffset + circleAngularRadius <= halfWedgeAngle;
    }*/

    private bool CircleTouchesWedge(Vector2 center, float radius)
    {
        Vector2[] points = GetWedgeAsTriangle();
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
        t = Mathf.Clamp(t, 0, lineDir.magnitude);

        // Closest point on the segment to the circle center
        Vector2 closestPoint = lineStart + lineDir.normalized * t;

        // Distance from the circle center to the closest point
        float distanceSqr = (closestPoint - circleCenter).sqrMagnitude;

        // Check if the distance is less than or equal to radius
        return distanceSqr <= radius * radius;
    }
    public void CheckBubble(Vector2 center, float radius)
    {
        if (GetIntersectionAngleRange(center, radius, out float angle1, out float angle2))
        {
            if (!DoAngleRangesOverlap(angle - (7f / 360f) * 0.5f, angle + (7f / 360f) * 0.5f, angle1, angle2))
            {
                //Bounce
                Debug.Log("Bubble should bounce");
                //Use ReflectDir
            }
            else
            {
                if (CircleTouchesWedge(center, radius))
                {
                    //Pop
                    Debug.Log("Bubble should pop");
                }
            }
        }
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
    private bool GetIntersectionAngleRange(Vector2 centerB, float radiusB, out float angle1, out float angle2)
    {
        Vector2 centerA = new Vector2(transform.position.x, transform.position.y);
        float radiusA = transform.localScale.x / 2f;

        angle1 = 0f;
        angle2 = 0f;

        // Distance between the centers
        float d = Vector2.Distance(centerA, centerB);

        // Check for intersection conditions: circles must intersect at two points
        if (d > radiusA + radiusB || d < Mathf.Abs(radiusA - radiusB))
        {
            return false;
        }

        // Calculate the half-angle using the law of cosines
        float cosHalfAngle = (d * d + radiusA * radiusA - radiusB * radiusB) / (2f * d * radiusA);
        // Clamp the value to avoid numerical issues
        cosHalfAngle = Mathf.Clamp(cosHalfAngle, -1f, 1f);
        float halfAngle = Mathf.Acos(cosHalfAngle);

        // Calculate the angle of the line from centerA to centerB
        float phi = Mathf.Atan2(centerB.y - centerA.y, centerB.x - centerA.x);

        // The two intersection points on circle A occur at these angles
        angle1 = phi - halfAngle;
        angle2 = phi + halfAngle;

        float TransformAngle(float angle)
        {
            angle *= Mathf.Rad2Deg;
            //angle's current range is (-180, 180) and 0 is right, counterclockwise
            if (angle < 0) angle = 360 + angle;
            //angle's current range is (0, 360) and 0 is right, counterclockwise
            angle -= 90;
            //angle's current range is (-90, 270) and 0 is right, counterclockwise
            if (angle < 0) angle += 360;
            //angle's current range is (0, 360) and 0 is up, counterclockwise
            angle = 360 - angle;
            //angle's current range is (0, 360) and 0 is up, clockwise
            return angle;
        }

        angle1 = TransformAngle(angle2);
        angle2 = TransformAngle(angle1);
        return true;
    }
}
