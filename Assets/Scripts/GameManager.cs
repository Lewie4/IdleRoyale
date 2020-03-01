using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public List<Player> Players
    {
        get { return players; }
    }

    public Vector2 SpawnArea
    {
        get { return spawnArea;  }
    }

    public Transform Circle
    {
        get { return circle; }
    }

    [SerializeField] SimpleCameraController cameraController;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] int playerCount;
    [SerializeField] Vector2 spawnArea;

    private List<Player> players = new List<Player>();
    [SerializeField] Leaderboard leaderboard;

    [SerializeField] Transform circle;
    [SerializeField] float circleTime;
    float circleProgress;
    Vector3 circleStartPos;
    Vector3 circleFinalPos;
    Vector3 circleStartScale;
    Vector3 circleFinalScale;

    private Queue<Player> targetQueue = new Queue<Player>();

    void Start()
    {
        for (int i = 0; i < playerCount; i++)
        {
            var newPlayer = Instantiate(playerPrefab, new Vector3(Random.Range(-spawnArea.x, spawnArea.x), 1, Random.Range(-spawnArea.y, spawnArea.y)), Quaternion.identity, transform).GetComponent<Player>();
            newPlayer.playerName = "Player " + i;
            players.Add(newPlayer);
        }

        circleStartPos = circle.position;
        circleFinalPos = new Vector3(Random.Range(-spawnArea.x, spawnArea.x), circleStartPos.y, Random.Range(-spawnArea.y, spawnArea.y));

        circleStartScale = circle.localScale;
        circleFinalScale = new Vector3(circleStartScale.x / 10f, circleStartScale.y, circleStartScale.z / 10f);
    }

    private void Update()
    {
        for(int i = 0; i < 300; i++)
        {
            if (targetQueue.Count <= 0)
            {
                break;
            }

            var player = targetQueue.Dequeue();
            player.GetTarget();
        }

        if (circleProgress < 1)
        {
            circleProgress += Time.deltaTime / circleTime;
            circle.position = Vector3.Lerp(circleStartPos, circleFinalPos, circleProgress);
            circle.localScale = Vector3.Lerp(circleStartScale, circleFinalScale, circleProgress);
        }

    }

    public void RequestTarget(Player player)
    {
        targetQueue.Enqueue(player);
    }

    public void SnapToLocation(Transform location)
    {
        cameraController.SnapToLocation(location);
    }

    public void UpdateLeaderboard(LeaderboardType leaderboardType)
    {
        leaderboard.UpdateLeaderboard(leaderboardType);
    }
}
