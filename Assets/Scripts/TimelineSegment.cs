using UnityEngine;
using UnityEngine.UI;

public class TimelineSegment : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image _lineBackground; 
    [SerializeField] private Image _circleBackground; 
    [SerializeField] private Image _iconImage;        
    [SerializeField] private Image _arrowHead; 

    [Header("Colors")]
    [SerializeField] private Color _playerColor = new Color(0.2f, 0.5f, 1f); 
    [SerializeField] private Color _enemyColor = new Color(1f, 0.3f, 0.3f);  

    public void Setup(bool isPlayer, Sprite iconSprite, bool isLastSegment)
    {
        Color factionColor = isPlayer ? _playerColor : _enemyColor;

        if (_lineBackground != null) 
        {
            _lineBackground.color = factionColor;
        }

        if (_iconImage != null)
        {
            if (iconSprite != null) 
            { 
                _iconImage.sprite = iconSprite; 
                _iconImage.color = Color.white; 
            }
            else 
            { 
                _iconImage.color = Color.clear; 
            }
        }

        if (_arrowHead != null)
        {
            _arrowHead.gameObject.SetActive(true);
            
            if (_arrowHead.TryGetComponent<Image>(out Image arrowImg))
            {
                arrowImg.color = factionColor;
            }
        }
    }
}