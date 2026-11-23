using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TD.Core;
using TD.Data;
using TD.Towers;

namespace TD.UI
{
    public class BuildPanelUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SlotSelector slotSelector;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private GameObject buildGroup;
        [SerializeField] private GameObject manageGroup;

        [Header("Build Buttons")]
        [SerializeField] private TowerData[] availableTowers;
        [SerializeField] private Button[] buildButtons;
        [SerializeField] private TMP_Text[] buildButtonLabels;

        [Header("Manage Buttons")]
        [SerializeField] private TMP_Text towerInfoText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TMP_Text upgradeButtonLabel;
        [SerializeField] private Button sellButton;
        [SerializeField] private TMP_Text sellButtonLabel;

        private BuildSlot currentSlot;

        private void Start()
        {
            for (int i = 0; i < buildButtons.Length; i++)
            {
                int index = i;
                if (buildButtons[i] != null)
                {
                    buildButtons[i].onClick.AddListener(() => OnBuildClicked(index));
                }
            }

            if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeClicked);
            if (sellButton != null) sellButton.onClick.AddListener(OnSellClicked);

            if (slotSelector != null)
            {
                slotSelector.OnSlotSelected += HandleSlotSelected;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged += HandleGoldChanged;
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }

            RefreshUI();
        }

        private void OnDestroy()
        {
            if (slotSelector != null)
            {
                slotSelector.OnSlotSelected -= HandleSlotSelected;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged -= HandleGoldChanged;
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void HandleSlotSelected(BuildSlot slot)
        {
            currentSlot = slot;
            RefreshUI();
        }

        private void HandleGoldChanged(int _)
        {
            RefreshUI();
        }

        private void HandleStateChanged(GameState state)
        {
            if (state != GameState.Playing && panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
            else
            {
                RefreshUI();
            }
        }

        private void OnBuildClicked(int index)
        {
            if (currentSlot == null || availableTowers == null || index < 0 || index >= availableTowers.Length)
            {
                return;
            }

            if (currentSlot.TryBuild(availableTowers[index]))
            {
                RefreshUI();
            }
        }

        private void OnUpgradeClicked()
        {
            if (currentSlot != null && currentSlot.TryUpgrade())
            {
                RefreshUI();
            }
        }

        private void OnSellClicked()
        {
            if (currentSlot != null && currentSlot.TrySell())
            {
                if (slotSelector != null)
                {
                    slotSelector.ClearSelection();
                }
                else
                {
                    RefreshUI();
                }
            }
        }

        private void RefreshUI()
        {
            bool canShow = currentSlot != null &&
                           GameManager.Instance != null &&
                           GameManager.Instance.State == GameState.Playing;

            if (panelRoot != null) panelRoot.SetActive(canShow);
            if (!canShow) return;

            bool occupied = currentSlot.IsOccupied;
            if (buildGroup != null) buildGroup.SetActive(!occupied);
            if (manageGroup != null) manageGroup.SetActive(occupied);

            int currentGold = GameManager.Instance.Gold;

            if (!occupied)
            {
                for (int i = 0; i < buildButtons.Length; i++)
                {
                    bool hasData = availableTowers != null && i < availableTowers.Length && availableTowers[i] != null;
                    if (buildButtons[i] != null)
                    {
                        buildButtons[i].gameObject.SetActive(hasData);
                    }

                    if (!hasData) continue;

                    TowerData data = availableTowers[i];
                    int cost = data.GetLevel(0).cost;
                    if (buildButtons[i] != null) buildButtons[i].interactable = currentGold >= cost;
                    if (i < buildButtonLabels.Length && buildButtonLabels[i] != null)
                    {
                        buildButtonLabels[i].text = $"{data.towerName}\n{cost}g";
                    }
                }
            }
            else
            {
                Tower tower = currentSlot.CurrentTower;
                if (towerInfoText != null)
                {
                    towerInfoText.text = $"{tower.Data.towerName} (Lv.{tower.LevelNumber})";
                }

                if (upgradeButton != null && upgradeButtonLabel != null)
                {
                    if (tower.CanUpgrade)
                    {
                        int upCost = tower.GetNextUpgradeCost();
                        upgradeButton.interactable = currentGold >= upCost;
                        upgradeButtonLabel.text = $"Upgrade\n{upCost}g";
                    }
                    else
                    {
                        upgradeButton.interactable = false;
                        upgradeButtonLabel.text = "MAX";
                    }
                }

                if (sellButtonLabel != null)
                {
                    sellButtonLabel.text = $"Sell\n+{currentSlot.GetSellValue()}g";
                }
            }
        }
    }
}
