using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HarvestManager : MonoBehaviour
{
    public static HarvestManager Instance;

    public GameObject harvestPanel;

    [Header("Datos del Enemigo Vencido")]
    public Image newSoulIcon;
    public TextMeshProUGUI newSoulName;

    [Header("Tus 3 Huecos de Equipo")]
    public TextMeshProUGUI[] teamSlotNames; // Asegúrate de que el tamaño sea 3 en el Inspector
    public Image[] teamSlotIcons;           // Asegúrate de que el tamaño sea 3 en el Inspector
    public Button[] replaceButtons;         // Asegúrate de que el tamaño sea 3 en el Inspector

    public Button discardButton;
    private SoulData pendingSoul;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if(harvestPanel != null) harvestPanel.SetActive(false);
    }

    public void OpenHarvestScreen(SoulData defeatedSoul)
    {
        if (defeatedSoul == null) return;
        pendingSoul = defeatedSoul;
        
        harvestPanel.SetActive(true);

        // Mostrar datos del enemigo
        if (newSoulName != null) newSoulName.text = defeatedSoul.soulName;
        if (newSoulIcon != null) {
            newSoulIcon.sprite = defeatedSoul.icon;
            newSoulIcon.color = Color.white;
        }

        // Actualizar slots del equipo
        for (int i = 0; i < 3; i++)
        {
            int index = i; // Captura de variable para el listener
            replaceButtons[i].onClick.RemoveAllListeners();

            if (i < PlayerTeam.Instance.souls.Count)
            {
                SoulInstance soulEnEquipo = PlayerTeam.Instance.souls[i]; // Cambiado a SoulInstance
                teamSlotNames[i].text = soulEnEquipo.data.soulName; // Añadido .data
                
                if (teamSlotIcons[i] != null && soulEnEquipo.data.icon != null)
                {
                    teamSlotIcons[i].sprite = soulEnEquipo.data.icon; // Añadido .data
                    teamSlotIcons[i].color = Color.white;
                }
                replaceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = "Reemplazar";
            }
            else
            {
                teamSlotNames[i].text = "Vacío";
                teamSlotIcons[i].sprite = null;
                teamSlotIcons[i].color = new Color(0,0,0,0);
                replaceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = "Equipar";
            }

            replaceButtons[i].onClick.AddListener(() => OnSlotClicked(index));
        }
    }

    private void OnSlotClicked(int index)
    {
        PlayerTeam.Instance.AddOrReplaceSoul(pendingSoul, index);
        CloseScreen();
    }

    public void CloseScreen()
    {
        harvestPanel.SetActive(false);
        // Notificar al BattleManager que restaure el mundo
        BattleManager.Instance.CloseHarvestScreen(); 
    }
}