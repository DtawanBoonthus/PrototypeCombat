using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Scripts
{
    public class CloseAttack : MonoBehaviour
    {
        private async void OnEnable()
        {
            await UniTask.WaitForSeconds(0.3f);
            gameObject.SetActive(false);
        }
    }
}