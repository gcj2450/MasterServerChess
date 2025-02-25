﻿using MasterServerToolkit.Logging;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MasterServerToolkit.UI
{
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasGroup))]
    public class UIView : MonoBehaviour, IUIView
    {
        #region INSPECTOR

        [Header("Identity Settings"), SerializeField]
        protected string id = "New View Id";

        [Header("Shared Settings"), SerializeField]
        protected bool hideOnStart = true;
        [SerializeField]
        protected bool allwaysOnTop = false;
        [SerializeField]
        protected bool ignoreHideAll = false;

        [Header("Events")]
        public UnityEvent OnShowEvent;
        public UnityEvent OnHideEvent;
        public UnityEvent OnShowFinishedEvent;
        public UnityEvent OnHideFinishedEvent;

        #endregion

        private Dictionary<string, Component> children;
        protected Dictionary<string, IUIViewComponent> uiViewComponents;
        protected IUIViewTweener uiViewTweener;
        protected CanvasGroup canvasGroup;

        public bool IsVisible { get; private set; } = true;
        public string Id => id;

        public RectTransform Rect => transform as RectTransform;

        public bool IgnoreHideAll { get => ignoreHideAll; set => ignoreHideAll = value; }

        protected virtual void Awake()
        {
            if (!canvasGroup)
                canvasGroup = GetComponent<CanvasGroup>();

            uiViewComponents = new Dictionary<string, IUIViewComponent>();
            children = new Dictionary<string, Component>();

            Rect.anchoredPosition = Vector2.zero;

            if (!string.IsNullOrEmpty(id))
            {
                ViewsManager.Register(id, this);
            }
            else
            {
                Logs.Warn($"Id field is empty therefore this UIView cannot be registered in {nameof(ViewsManager)}");
            }

            RegisterAllUIViewComponents();

            foreach (var uiViewComponent in uiViewComponents.Values)
            {
                uiViewComponent.OnOwnerAwake();
            }

            if (hideOnStart)
            {
                Hide(true);
            }
        }

        protected virtual void Start()
        {
            foreach (var uiComponent in uiViewComponents.Values)
            {
                uiComponent.OnOwnerStart();
            }
        }

        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(Id))
            {
                id = name;
            }

            if (!GetComponent<CanvasGroup>())
            {
                gameObject.AddComponent<CanvasGroup>();
            }
        }

        protected virtual void OnDestroy()
        {
            OnShowEvent.RemoveAllListeners();
            OnHideEvent.RemoveAllListeners();
            OnShowFinishedEvent.RemoveAllListeners();
            OnHideFinishedEvent.RemoveAllListeners();
        }

        protected virtual void RegisterAllUIViewComponents()
        {
            uiViewTweener = GetComponent<IUIViewTweener>();

            if (uiViewTweener != null) uiViewTweener.UIView = this;

            foreach (var uiViewComponent in GetComponentsInChildren<IUIViewComponent>(true))
            {
                var key = uiViewComponent.GetType().Name;

                if (!uiViewComponents.ContainsKey(key))
                {
                    uiViewComponent.Owner = this;
                    uiViewComponents.Add(key, uiViewComponent);
                }
                else
                {
                    Debug.LogError($"{key} is allready registered");
                }
            }
        }

        public T ViewComponent<T>() where T : class, IUIViewComponent
        {
            var key = typeof(T).Name;

            if (uiViewComponents.ContainsKey(key))
            {
                return (T)uiViewComponents[key];
            }
            else
            {
                Debug.LogError($"{key} is not registered");
                return null;
            }
        }

        public T ChildComponent<T>(string childName) where T : Component
        {
            string childId = $"{childName}_{typeof(T).Name}";

            if (children.TryGetValue(childId, out Component child))
            {
                return (T)child;
            }
            else
            {
                var newChild = GetComponentsInChildren<T>(true).Where(c => c.name == childName).FirstOrDefault();

                if (newChild)
                {
                    children.Add(childId, newChild);
                }

                return newChild;
            }
        }

        public virtual void Show(bool instantly = false)
        {
            if (uiViewTweener != null && !instantly)
            {
                IsVisible = true;

                OnShowEvent?.Invoke();

                NotifyComponentsOnShow(true);

                if (allwaysOnTop) transform.SetAsLastSibling();

                uiViewTweener.OnFinished(() =>
                {
                    SetCanvasActive(true);
                    OnShowFinishedEvent?.Invoke();
                });

                uiViewTweener.PlayShow();
            }
            else
            {
                IsVisible = true;

                OnShowEvent?.Invoke();

                NotifyComponentsOnShow(true);

                if (allwaysOnTop) transform.SetAsLastSibling();

                SetCanvasActive(true);

                OnShowFinishedEvent?.Invoke();
            }
        }

        public virtual void Hide(bool instantly = false)
        {
            if (uiViewTweener != null && !instantly)
            {
                IsVisible = false;

                OnHideEvent?.Invoke();

                NotifyComponentsOnShow(false);

                uiViewTweener.OnFinished(() =>
                {
                    SetCanvasActive(false);
                    OnHideFinishedEvent?.Invoke();
                });

                uiViewTweener.PlayHide();
            }
            else
            {
                IsVisible = false;

                OnHideEvent?.Invoke();

                NotifyComponentsOnShow(false);

                SetCanvasActive(false);

                OnHideFinishedEvent?.Invoke();
            }
        }

        public virtual void Toggle(bool instantly = false)
        {
            if (IsVisible)
            {
                Hide(instantly);
            }
            else
            {
                Show(instantly);
            }
        }

        private Transform FindInDescendants(Transform parent, string childName)
        {
            if (parent.childCount == 0) return null;

            //Debug.Log($"Looking for child [{childName}] in [{parent.name}]");

            Transform result = null;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);

                if (child.name == childName)
                {
                    result = child;

                    //Debug.Log($"Child [{childName}] was found in [{parent.name}]");

                    break;
                }
                else
                {
                    result = FindInDescendants(child, childName);

                    if (result) break;
                }
            }

            return result;
        }

        private void SetCanvasActive(bool active)
        {
            if (canvasGroup)
            {
                canvasGroup.interactable = active;
                canvasGroup.blocksRaycasts = active;
                canvasGroup.alpha = active ? 1f : 0f;
            }
        }

        private void NotifyComponentsOnShow(bool show)
        {
            if (show)
            {
                OnShow();

                foreach (var uiComponent in uiViewComponents.Values)
                {
                    uiComponent.OnOwnerShow(this);
                }
            }
            else
            {
                OnHide();

                foreach (var uiComponent in uiViewComponents.Values)
                {
                    uiComponent.OnOwnerHide(this);
                }
            }
        }

        protected virtual void OnShow() { }

        protected virtual void OnHide() { }
    }
}
