using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 객체 충돌 시 씬 전환 테스트 클래스.
/// </summary>
public class EncounterTest : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("SampleScene");
        }
    }
}