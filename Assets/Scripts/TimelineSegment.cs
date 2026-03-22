using UnityEngine;
using UnityEngine.UI;

public class TimelineSegment : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image lineBackground; 
    public Image circleBackground; // El fondo del círculo blanco
    public Image iconImage;        // <-- NUEVO: La imagen del monstruo
    public Image arrowHead; 

    [Header("Colores")]
    public Color playerColor = new Color(0.2f, 0.5f, 1f); 
    public Color enemyColor = new Color(1f, 0.3f, 0.3f);  

    public void Setup(bool isPlayer, Sprite iconSprite, bool isLastSegment)
    {
        // Decidir el color según el bando
        Color factionColor = isPlayer ? playerColor : enemyColor;

        // 1. Pintar el cuerpo de la línea (La raya recta)
        if (lineBackground != null) 
        {
            lineBackground.color = factionColor;
        }

        // 2. Asignar el retrato del monstruo
        if (iconImage != null)
        {
            if (iconSprite != null) 
            { 
                iconImage.sprite = iconSprite; 
                iconImage.color = Color.white; 
            }
            else 
            { 
                iconImage.color = Color.clear; 
            }
        }

        // 3. Activar y pintar la punta de la flecha para TODOS los turnos
        if (arrowHead != null)
        {
            arrowHead.gameObject.SetActive(true);
            
            if (arrowHead.TryGetComponent<Image>(out Image arrowImg))
            {
                arrowImg.color = factionColor;
            }
        }
    }
}