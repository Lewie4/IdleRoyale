using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum LeaderboardType
{
    None,
    Kills,
    Health
}

public class Leaderboard : Singleton<Leaderboard>
{
    public LeaderboardType leaderboardType;
    [SerializeField] private bool isAlive;

    [SerializeField] TMPro.TextMeshProUGUI title;
    [SerializeField] TMPro.TextMeshProUGUI remaining;

    [SerializeField] List<LeaderboardEntry> leaderboardEntries;

    private List<Player> sortedPlayers = new List<Player>();

    public void Start()
    {
        for(int i = 0; i < leaderboardEntries.Count; i++)
        {
            leaderboardEntries[i].SetPosition(i + 1);
        }

        title.text = leaderboardType.ToString();
    }

    public void NextLeaderboardType()
    {    
        ChangeLeaderboardType(GetNext(leaderboardType));
    }

    public void ChangeLeaderboardType(LeaderboardType newType)
    {
        leaderboardType = newType;

        title.text = leaderboardType.ToString();

        UpdateLeaderboard(leaderboardType);
    }

    public void UpdateLeaderboard(LeaderboardType typeCheck)
    {
        if (typeCheck == leaderboardType)
        {
            List<Player> players = new List<Player>(GameManager.Instance.Players);
            int startingPlayers = players.Count;

            var alive = players.Where(o => o.currentStats.health > 0).ToList();
            int remainingPlayers = alive.Count;

            remaining.text = remainingPlayers + "/" + startingPlayers;

            if (isAlive)
            {
                players = alive;
            }

            switch(typeCheck)
            {
                case LeaderboardType.Health:
                    {
                        sortedPlayers = players.OrderByDescending(o => o.currentStats.health).ToList();
                        break;
                    }
                case LeaderboardType.Kills:
                case LeaderboardType.None:
                default:
                    {
                        sortedPlayers = players.OrderByDescending(o => o.kills).ToList();
                        break;
                    }
            }            

            for (int i = 0; i < leaderboardEntries.Count; i++)
            {
                leaderboardEntries[i].UpdateEntry(sortedPlayers[i], sortedPlayers[i].kills.ToString());
            }
        }
    }

    public static LeaderboardType GetNext(LeaderboardType value)
    {
        return (from LeaderboardType val in System.Enum.GetValues(typeof(LeaderboardType))
                where val > value
                orderby val
                select val).DefaultIfEmpty().First();
    }

}
