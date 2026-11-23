using UnityEngine;
using TD.Data;
using TD.Towers;

namespace TD.Core
{
    public class BuildSlot : MonoBehaviour
    {
        [SerializeField] private Transform towerAnchor;
        [SerializeField] private GameObject highlightVisual;

        private bool isSelected;

        public Tower CurrentTower { get; private set; }
        public bool IsOccupied => CurrentTower != null;

        private void Awake()
        {
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            if (highlightVisual != null)
            {
                highlightVisual.SetActive(selected);
            }

            if (IsOccupied)
            {
                CurrentTower.SetRangeVisible(selected);
            }
        }

        public bool TryBuild(TowerData towerData)
        {
            if (IsOccupied || towerData == null || towerData.prefab == null || GameManager.Instance == null)
            {
                return false;
            }

            int cost = towerData.GetLevel(0).cost;
            if (!GameManager.Instance.TrySpendGold(cost))
            {
                return false;
            }

            Transform anchor = towerAnchor != null ? towerAnchor : transform;
            CurrentTower = Instantiate(towerData.prefab, anchor.position, anchor.rotation);
            CurrentTower.Initialize(towerData);
            CurrentTower.SetRangeVisible(isSelected);
            return true;
        }

        public bool TryUpgrade()
        {
            if (!IsOccupied || !CurrentTower.CanUpgrade || GameManager.Instance == null)
            {
                return false;
            }

            int cost = CurrentTower.GetNextUpgradeCost();
            if (!GameManager.Instance.TrySpendGold(cost))
            {
                return false;
            }

            CurrentTower.Upgrade();
            return true;
        }

        public int GetSellValue()
        {
            if (!IsOccupied || GameManager.Instance == null || GameManager.Instance.Config == null)
            {
                return 0;
            }

            return Mathf.RoundToInt(CurrentTower.TotalInvestedGold * GameManager.Instance.Config.sellRefundPercent);
        }

        public bool TrySell()
        {
            if (!IsOccupied || GameManager.Instance == null)
            {
                return false;
            }

            int refund = GetSellValue();
            GameManager.Instance.AddGold(refund);
            Destroy(CurrentTower.gameObject);
            CurrentTower = null;
            return true;
        }
    }
}
