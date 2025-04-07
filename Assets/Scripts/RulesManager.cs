using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public struct RulesSpriteEntry
{
    public Image image;
    public Sprite defaultSprite;
    public Sprite rulesActiveSprite;
}

public class RulesManager : MonoBehaviour
{
    [SerializeField] private RulesSpriteEntry[] spriteEntries;
    [SerializeField] private GameObject rulesPanel;
    [SerializeField] private GameObject navigationPanel;
    [SerializeField] private GameObject controllerCloseButton;
    [SerializeField] private GameObject controllerConfirmButton;
    [SerializeField] private GameObject keyboardCloseButton;
    [SerializeField] private InputActionReference cancelAction;

    private void OnEnable()
    {
        cancelAction.action.Enable();
    }

    private void OnDisable()
    {
        cancelAction.action.Disable();
    }

    void Update()
    {
        if (cancelAction.action.WasPerformedThisFrame() && rulesPanel.activeSelf)
        {
            SetRulesActive(false);
            navigationPanel.SetActive(true);
            controllerCloseButton.SetActive(false);
            controllerConfirmButton.SetActive(true);
        }

        UpdateSprites();
    }

    public void SetRulesActive(bool isActive)
    {
        rulesPanel.SetActive(isActive);
        UpdateSprites();
        
        // Optional: Add event system handling here if needed
        // if (isActive) EventSystem.current.SetSelectedGameObject(...);
    }

    private void UpdateSprites()
    {
        foreach (var entry in spriteEntries)
        {
            if (entry.image == null) continue;

            Sprite targetSprite = rulesPanel.activeSelf ? 
                entry.rulesActiveSprite : 
                entry.defaultSprite;

            if (targetSprite != null)
            {
                entry.image.sprite = targetSprite;
                entry.image.color = new Color(
                    entry.image.color.r,
                    entry.image.color.g,
                    entry.image.color.b,
                    1f
                );
            }
        }
    }
}