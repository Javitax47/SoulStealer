using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExplorationUIManager : MonoBehaviour
{
    public static ExplorationUIManager Instance;

    [Header("Soul Slots UI")]
    public GameObject[] soulSlots; // Los 3 contenedores de alma
    public Image[] soulIcons;
    public Slider[] hpSliders;
    public TextMeshProUGUI[] nameTexts;

    [Header("Currency")]
    public TextMeshProUGUI fragmentsText;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        UpdateTeamUI();
    }

    public void UpdateTeamUI()
    {
        var team = PlayerTeam.Instance.souls;

        for (int i = 0; i < soulSlots.Length; i++)
        {
            if (i < team.Count)
            {
                soulSlots[i].SetActive(true);
                soulIcons[i].sprite = team[i].data.icon;
                hpSliders[i].maxValue = team[i].data.maxHP;
                hpSliders[i].value = team[i].currentHP;
                if (nameTexts[i] != null) nameTexts[i].text = team[i].data.soulName;
            }
            else
            {
                soulSlots[i].SetActive(false); // Ocultar si no hay alma
            }
        }

        if (fragmentsText != null)
            fragmentsText.text = PlayerTeam.Instance.soulFragments.ToString();
    }
}