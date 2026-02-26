using TMPro;
using UnityEngine;

public class HUDController2D : MonoBehaviour
{
    [SerializeField] private PlayerStats2D stats;

    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI livesText;

    [SerializeField] private int maxHP = 10;
    [SerializeField] private int maxEnergy = 20;

    private void Start()
    {
        if (stats == null)
            stats = FindFirstObjectByType<PlayerStats2D>();
    }

    private void Update()
    {
        if (stats == null)
        {
            stats = FindFirstObjectByType<PlayerStats2D>();
            return;
        }

        if (hpText != null)
            hpText.text = $"HP: {stats.Health}/{maxHP}";

        if (energyText != null)
            energyText.text = $"EN: {stats.Energy}/{maxEnergy}";

        if (livesText != null)
            livesText.text = $"Lives: {stats.Lives}";
    }
}
