using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BattleUIManager : MonoBehaviour
{
    [Header("UI Containers")]
    [SerializeField] private GameObject _combatUI;
    [SerializeField] private GameObject _harvestUI;

    [Header("Timeline (UI)")]
    [SerializeField] private Transform _timelineContainer;
    [SerializeField] private TimelineSegment _timelineSegmentPrefab;

    [Header("Skill Buttons")]
    [SerializeField] private Button[] _skillButtons;

    [Header("Harvest Screen")]
    [SerializeField] private Image _harvestEnemyIcon;
    [SerializeField] private TextMeshProUGUI _harvestEnemyName;

    [Header("Visual Effects")]
    [SerializeField] private Image _flashScreen;

    public void ShowCombatUI(bool show) => _combatUI.SetActive(show);

    public void ShowFlash(Color color, float duration)
    {
        if (_flashScreen != null)
        {
            _flashScreen.gameObject.SetActive(true);
            _flashScreen.color = color;
            StartCoroutine(FlashRoutine(duration));
        }
    }

    private System.Collections.IEnumerator FlashRoutine(float duration)
    {
        float elapsed = 0f;
        Color color = _flashScreen.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / duration);
            _flashScreen.color = color;
            yield return null;
        }
        _flashScreen.gameObject.SetActive(false);
    }

    public void UpdateTimeline(List<BattleUnit> prediction, BattleUnit playerUnit)
    {
        if (_timelineContainer == null || _timelineSegmentPrefab == null) return;

        while (_timelineContainer.childCount < prediction.Count) Instantiate(_timelineSegmentPrefab, _timelineContainer);

        for (int i = 0; i < _timelineContainer.childCount; i++)
        {
            Transform child = _timelineContainer.GetChild(i);
            if (i >= prediction.Count)
            {
                child.gameObject.SetActive(false);
                continue;
            }

            child.gameObject.SetActive(true);
            TimelineSegment segmentUI = child.GetComponent<TimelineSegment>();
            bool isPlayer = (prediction[i] == playerUnit);
            bool isLast = (i == prediction.Count - 1);
            Sprite soulIcon = prediction[i].baseData.icon;
            segmentUI.Setup(isPlayer, soulIcon, isLast);
        }
    }

    public void UpdateSkillButtons(BattleUnit playerUnit, bool isPlayerTurn)
    {
        if (_skillButtons == null || playerUnit == null) return;

        for (int i = 0; i < _skillButtons.Length; i++)
        {
            TextMeshProUGUI btnText = _skillButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (i < playerUnit.baseData.skills.Count)
            {
                if (btnText != null) btnText.text = playerUnit.baseData.skills[i].skillName;
                _skillButtons[i].interactable = isPlayerTurn;
            }
            else
            {
                if (btnText != null) btnText.text = "---";
                _skillButtons[i].interactable = false;
            }
        }
    }

    public void SetupHarvestScreen(BattleUnit defeatedEnemy)
    {
        if (defeatedEnemy != null)
        {
            if (_harvestEnemyIcon != null && defeatedEnemy.baseData.icon != null)
            {
                _harvestEnemyIcon.sprite = defeatedEnemy.baseData.icon;
                _harvestEnemyIcon.color = Color.white;
            }
            if (_harvestEnemyName != null)
            {
                _harvestEnemyName.text = defeatedEnemy.baseData.soulName;
            }
        }
        _harvestUI.SetActive(true);
    }

    public void CloseHarvestScreen() => _harvestUI.SetActive(false);

    public System.Collections.IEnumerator FadeToBlack(float duration)
    {
        if (_flashScreen != null)
        {
            _flashScreen.gameObject.SetActive(true);
            Color fadeColor = Color.black;
            fadeColor.a = 0f;
            _flashScreen.color = fadeColor;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                fadeColor.a = Mathf.Lerp(0f, 1f, t / duration);
                _flashScreen.color = fadeColor;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }
    }
}
