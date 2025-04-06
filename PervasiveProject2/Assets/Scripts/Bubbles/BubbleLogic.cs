using System.Collections;
using UnityEngine;

public class BubbleLogic : MonoBehaviour
{
    private Vector2 dir;
    void Update()
    {
        if (popping) return;

        Vector2 velocity = dir * Time.deltaTime * 60f;
        
        Vector2 prevCenter = new Vector2(transform.position.x, transform.position.y);

        transform.position = new Vector3(transform.position.x + velocity.x, transform.position.y + velocity.y, 0);
        Vector2 center = new Vector2(transform.position.x, transform.position.y);

        //Mario 64 collision detection approach baby
        for (int i = 0; i < 4; i++)
        {
            Vector2 subdivCenter = Vector2.Lerp(prevCenter, center, i / 3.0f);
            switch (BubbleSpawner.CheckLilypads(subdivCenter, 0.3251953125f, out LilypadVisual sender))
            {
                case LilypadVisual.BubbleResponse.None:
                default:
                    break;
                case LilypadVisual.BubbleResponse.Bounce:
                    dir = sender.ReflectDir(subdivCenter, dir.normalized) * dir.magnitude;
                    i = int.MaxValue - 2;
                    break;
                case LilypadVisual.BubbleResponse.Pop:
                    //I dont have time to waste making this nice
                    switch (sender.Colour)
                    {
                        case LilypadVisual.FrogColour.Pink:
                        default:
                            ScoreCounter.IncrementPink();
                            break;
                        case LilypadVisual.FrogColour.Purple:
                            ScoreCounter.IncrementPurple();
                            break;
                    }
                    StartCoroutine(PopRoutine());
                    return;
            }
        }

        if (BubbleSpawner.IsOffscreenForGood(
            new Vector2(transform.position.x, transform.position.y),
            dir,
            0.3251953125f))
        {
            Spawn();
        }
    }
    bool popping;
    IEnumerator PopRoutine()
    {
        popping = true;
        GetComponent<MeshRenderer>().material.SetFloat("_TimeStart", Time.time);
        yield return new WaitForSeconds(4f * 2f * 0.0833333333f);
        Spawn();
        popping = false;
    }
    public void Spawn()
    {
        GetComponent<MeshRenderer>().material.SetFloat("_TimeStart", float.MaxValue);
        BubbleSpawner.Side side = BubbleSpawner.GetRandomSide();
        Vector2 pos = BubbleSpawner.GetSpawnPoint(side);
        transform.position = new Vector3(pos.x, pos.y, 0);
        dir = BubbleSpawner.GetRandomDirection(side);
        dir *= BubbleSpawner.GetRandomSpeed();
    }
}
