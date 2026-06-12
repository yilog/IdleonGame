using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.UI
{
    [DisallowMultipleComponent]
    public abstract class UIWindowController : MonoBehaviour
    {
        private Canvas windowCanvas;

        public string WindowId { get; private set; }
        public UIWindowMode Mode { get; private set; }
        public bool IsModal { get; private set; }
        public object OpenArgs { get; private set; }
        public UIManager Manager { get; private set; }
        public int SortingOrder => windowCanvas != null ? windowCanvas.sortingOrder : 0;

        public void Initialize(UIManager manager, UIWindowDefinition definition, object args)
        {
            Manager = manager;
            WindowId = definition.WindowId;
            Mode = definition.Mode;
            IsModal = definition.Modal;
            OpenArgs = args;
            EnsureCanvas();
            OnOpen(args);
        }

        public void SetSortingOrder(int sortingOrder)
        {
            EnsureCanvas();
            windowCanvas.overrideSorting = true;
            windowCanvas.sortingOrder = sortingOrder;
        }

        public void Close()
        {
            Manager?.CloseWindow(WindowId);
        }

        public void BringToFront()
        {
            Manager?.BringToFront(this);
        }

        protected T GetWindow<T>(string windowId) where T : UIWindowController
        {
            return Manager != null ? Manager.GetWindow<T>(windowId) : null;
        }

        protected void Publish<TEvent>(TEvent payload)
        {
            Manager?.Events.Publish(payload);
        }

        protected virtual void OnOpen(object args)
        {
        }

        protected virtual void OnClose()
        {
        }

        internal void InternalClose()
        {
            OnClose();
        }

        private void EnsureCanvas()
        {
            if (windowCanvas == null)
            {
                windowCanvas = GetComponent<Canvas>();
            }

            if (windowCanvas == null)
            {
                windowCanvas = gameObject.AddComponent<Canvas>();
            }

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }
    }
}
