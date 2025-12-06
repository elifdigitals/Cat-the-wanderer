using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeLocation : MonoBehaviour
{
    public string sceneNameChangeTo;

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            SceneManager.LoadScene(sceneNameChangeTo);
        }
    }
}
