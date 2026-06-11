using UnityEngine;

namespace PeopleFlow.UI
{
    public abstract class UIBase : MonoBehaviour
    {
        public abstract void Show();
        public abstract void Hide();
        public abstract void Init(params object[] dependencies);
        public virtual void SetData(params object[] data) { }
    }
}