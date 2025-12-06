using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NPCInteraction : MonoBehaviour
{
    [Header("NPC model (optional)")]
    public GameObject npcModel;

    [Header("Interaction")]
    public float interactRadius = 2f;
    public bool requirePlayerInRadius = true;

    [Header("Dialogue")]
    [TextArea(2, 6)]
    public string[] dialogueLines;

    [Header("NPC Info")]
    public string npcName = "NPC";

    [Header("References")]
    public Transform playerTransform;      // drag player сюда
    public MonoBehaviour playerController; // сюда перетащи скрипт управления игроком (например PlayerMovement)
                                           // если оставить пустым — отключение движения не произойдёт
    public DialogueUI dialogueUI;          // drag DialogueUI сюда
    public CameraToCat cameraScript;       // drag Main Camera (с CameraToCat) сюда

    // internal
    private bool playerInRange = false;
    private bool inDialogue = false;
    private bool playerControllerWasEnabled = false;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void Update()
    {
        if (requirePlayerInRadius && playerTransform != null)
        {
            float d = Vector2.Distance(playerTransform.position, transform.position);
            playerInRange = d <= interactRadius;
        }
        else
        {
            playerInRange = true;
        }

        if (!inDialogue && playerInRange)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(RunDialogue());
            }
        }
    }

    IEnumerator RunDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("NPCInteraction: Нет реплик у " + name);
            yield break;
        }

        inDialogue = true;

        // --- Блокируем движение игрока, если назначен playerController ---
        if (playerController != null)
        {
            playerControllerWasEnabled = playerController.enabled;
            playerController.enabled = false;
        }

        // Сигнал камере сфокусироваться
        if (cameraScript != null)
            cameraScript.StartDialogue(transform);

        // Показ UI (и имя NPC)
        if (dialogueUI != null)
        {
            dialogueUI.ShowPanel(true, transform, npcName);
            int idx = 0;
            dialogueUI.SetText(dialogueLines[idx]);

            while (idx < dialogueLines.Length)
            {
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter));
                idx++;
                if (idx < dialogueLines.Length)
                    dialogueUI.SetText(dialogueLines[idx]);
            }

            dialogueUI.ShowPanel(false);
        }

        // Снять фокус камеры
        if (cameraScript != null)
            cameraScript.EndDialogue();

        // Восстановить управление игроком (если блокировали)
        if (playerController != null)
            playerController.enabled = playerControllerWasEnabled;

        inDialogue = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
