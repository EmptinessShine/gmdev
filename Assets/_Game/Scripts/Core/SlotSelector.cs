using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace TD.Core
{
    public class SlotSelector : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask slotLayer;

        private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();

        public BuildSlot SelectedSlot { get; private set; }
        public event Action<BuildSlot> OnSlotSelected;

        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
            {
                return;
            }

            Pointer pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame)
            {
                return;
            }

            Vector2 screenPos = pointer.position.ReadValue();
            if (IsPointerOverUI(screenPos) || mainCamera == null)
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 200f, slotLayer))
            {
                BuildSlot slot = hit.collider.GetComponentInParent<BuildSlot>();
                SelectSlot(slot);
            }
            else
            {
                SelectSlot(null);
            }
        }

        public void ClearSelection()
        {
            SelectSlot(null);
        }

        private void SelectSlot(BuildSlot slot)
        {
            if (SelectedSlot == slot)
            {
                return;
            }

            if (SelectedSlot != null)
            {
                SelectedSlot.SetSelected(false);
            }

            SelectedSlot = slot;

            if (SelectedSlot != null)
            {
                SelectedSlot.SetSelected(true);
            }

            OnSlotSelected?.Invoke(SelectedSlot);
        }

        private bool IsPointerOverUI(Vector2 screenPos)
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = screenPos
            };

            uiRaycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, uiRaycastResults);
            return uiRaycastResults.Count > 0;
        }
    }
}
