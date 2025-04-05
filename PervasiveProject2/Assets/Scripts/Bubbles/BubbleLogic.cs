using UnityEngine;

public class BubbleLogic : MonoBehaviour
{
    private Vector2 dir;
    void Start()
    {
        Spawn();
    }
    void Update()
    {
        Vector2 velocity = dir * Time.deltaTime * 60f;
        transform.position += new Vector3(velocity.x, velocity.y, 0);

        BubbleSpawner.DebugLilypadCheck(new Vector2(transform.position.x, transform.position.y), 0.5f);

        if (BubbleSpawner.IsOffscreenForGood(
            new Vector2(transform.position.x, transform.position.y),
            dir))
        {
            Spawn();
        }
    }
    public void Spawn()
    {
        BubbleSpawner.Side side = BubbleSpawner.GetRandomSide();
        Vector2 pos = BubbleSpawner.GetSpawnPoint(side);
        transform.position = new Vector3(pos.x, pos.y, 0);
        dir = BubbleSpawner.GetRandomDirection(side);
        dir *= BubbleSpawner.GetRandomSpeed();
    }
}
