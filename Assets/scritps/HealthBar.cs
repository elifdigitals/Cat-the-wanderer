using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public PlayerHealth player;
    private Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
        slider.maxValue = player.maxHp;
        slider.value = player.hp;
    }

    void Update()
    {
        slider.value = player.hp;
    }
}
