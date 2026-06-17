using UnityEngine;

namespace IdleonGame.Markers
{
    [DisallowMultipleComponent]
    public sealed class SceneMarkerView : MonoBehaviour
    {
        [SerializeField] private Vector3 worldOffset = Vector3.zero;

        private Transform followTarget;
        private bool isFollowing;

        public void ShowAt(Vector3 worldPosition)
        {
            followTarget = null;
            isFollowing = false;
            transform.position = worldPosition + worldOffset;
            gameObject.SetActive(true);
        }

        public void Follow(Transform target)
        {
            followTarget = target;
            isFollowing = followTarget != null;
            if (isFollowing)
            {
                transform.position = followTarget.position + worldOffset;
            }

            gameObject.SetActive(isFollowing);
        }

        public void Hide()
        {
            followTarget = null;
            isFollowing = false;
            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (!isFollowing)
            {
                return;
            }

            if (followTarget == null)
            {
                Hide();
                return;
            }

            transform.position = followTarget.position + worldOffset;
        }
    }
}
