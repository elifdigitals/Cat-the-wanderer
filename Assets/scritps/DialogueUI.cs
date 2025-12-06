using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;                 // Панель диалога (фон)
    public TextMeshProUGUI dialogueTextTMP;  // Основной текст диалога
    public TextMeshProUGUI nameTextTMP;      // Имя NPC в левом верхнем углу

    [Header("Positioning")]
    public bool stickAboveNPC = true;        
    public Vector3 screenOffset = new Vector3(0, 100, 0);

    private Transform currentTargetNPC;
    private Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;

        if (panel == null)
            Debug.LogError("DialogueUI: панель не назначена!");

        if (dialogueTextTMP == null)
            Debug.LogError("DialogueUI: dialogueTextTMP не назначен!");

        if (nameTextTMP == null)
            Debug.LogError("DialogueUI: nameTextTMP не назначен!");

        panel.SetActive(false);
    }

    void Update()
    {
        if (panel.activeSelf && stickAboveNPC && currentTargetNPC != null && mainCam != null)
        {
            Vector3 screenPos = mainCam.WorldToScreenPoint(currentTargetNPC.position);

            if (screenPos.z < 0)
            {
                panel.SetActive(false);
                return;
            }

            panel.transform.position = screenPos + screenOffset;
        }
    }

    // Показ / скрытие панели диалога
    public void ShowPanel(bool show, Transform targetNPC = null, string npcName = null)
    {
        panel.SetActive(show);

        if (show)
        {
            currentTargetNPC = targetNPC;

            if (!string.IsNullOrEmpty(npcName))
                nameTextTMP.text = npcName;
        }
        else
        {
            currentTargetNPC = null;
        }
    }

    // Устанавливаем текст реплики
    public void SetText(string text)
    {
        dialogueTextTMP.text = text;
    }
}
