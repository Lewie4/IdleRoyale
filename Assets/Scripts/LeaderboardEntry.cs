using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderboardEntry : MonoBehaviour
{
    public Player player;

    [SerializeField] private TextMeshProUGUI playerText;
    [SerializeField] private TextMeshProUGUI valueText;

    private int position;

    public void UpdateEntry(Player newPlayer, string value)
    {
        player = newPlayer;

        playerText.text = position + ". " + player.playerName;
        playerText.color = player.currentStats.health > 0 ? Color.white : Color.red;
        playerText.gameObject.SetActive(true);

        valueText.text = value;
        valueText.gameObject.SetActive(true);
    }

    public void SetPosition(int pos)
    {
        position = pos;
    }

    public void OnClick()
    {
        GameManager.Instance.SnapToLocation(player.transform);
    }
}
