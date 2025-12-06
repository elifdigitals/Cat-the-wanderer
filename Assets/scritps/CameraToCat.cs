using UnityEngine;

public class CameraToCat : MonoBehaviour
{
    [Header("=== NORMAL FOLLOW MODE ===")]
    public Transform target;            // обычная цель — игрок
    public float smoothSpeed = 0.125f;  // скорость сглаживания (как у тебя)
    public Vector3 offset;              // смещение камеры относительно игрока

    [Header("=== DIALOG MODE ===")]
    public float dialogSmoothSpeed = 0.125f;   // скорость движения камеры во время диалога
    private Transform dialogTarget = null;     // цель камеры во время диалога
    private bool isInDialogue = false;         // флаг, активен ли диалог

    private void LateUpdate()
    {
        if (!isInDialogue)
        {
            FollowPlayer();
        }
        else
        {
            FocusOnNPC();
        }
    }

    // Обычное движение камеры за игроком (тот же алгоритм, что ты давал)
    private void FollowPlayer()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    // Фокусировка на NPC во время диалога (не портим поведение FollowPlayer)
    private void FocusOnNPC()
    {
        if (dialogTarget == null) return;

        Vector3 desiredPosition = dialogTarget.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, dialogSmoothSpeed);
        transform.position = smoothedPosition;
    }

    // Вызывается, когда начинать диалог с NPC
    public void StartDialogue(Transform npc)
    {
        dialogTarget = npc;
        isInDialogue = true;
    }

    // Вызывается по завершении диалога
    public void EndDialogue()
    {
        dialogTarget = null;
        isInDialogue = false;
    }
}
