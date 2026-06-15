using IdleonGame.Character;
using IdleonGame.Data;
using IdleonGame.Player;
using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.UI
{
    public sealed class BattleHudWindowController : UIWindowController
    {
        [SerializeField] private Text playerNameText;
        [SerializeField] private Image classIconImage;
        [SerializeField] private Text classNameText;
        [SerializeField] private Text levelText;
        [SerializeField] private Image hpFillImage;
        [SerializeField] private Text hpText;
        [SerializeField] private Image mpFillImage;
        [SerializeField] private Text mpText;
        [SerializeField] private Image xpFillImage;
        [SerializeField] private Text xpText;
        [SerializeField] private Button autoHuntButton;
        [SerializeField] private Text autoHuntText;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button talentButton;
        [SerializeField] private Button mapButton;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Sprite noClassIcon;
        [SerializeField] private Sprite warriorIcon;
        [SerializeField] private Sprite archerIcon;
        [SerializeField] private Sprite mageIcon;

        private CharacterStats playerStats;
        private PlayerAttack playerAttack;
        private PlayerClickInteractor clickInteractor;

        private void Update()
        {
            RefreshReferences();
            RefreshView();
        }

        protected override void OnOpen(object args)
        {
            var rectTransform = transform as RectTransform;
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            BindButtons();
            RefreshReferences();
            RefreshView();
        }

        private void BindButtons()
        {
            if (autoHuntButton != null)
            {
                autoHuntButton.onClick.RemoveListener(OnAutoHuntButtonClicked);
                autoHuntButton.onClick.AddListener(OnAutoHuntButtonClicked);
            }

            if (attackButton != null)
            {
                attackButton.onClick.RemoveListener(OnAttackButtonClicked);
                attackButton.onClick.AddListener(OnAttackButtonClicked);
            }

            if (inventoryButton != null)
            {
                inventoryButton.onClick.RemoveListener(OnInventoryButtonClicked);
                inventoryButton.onClick.AddListener(OnInventoryButtonClicked);
            }

            if (talentButton != null)
            {
                talentButton.onClick.RemoveListener(OnTalentButtonClicked);
                talentButton.onClick.AddListener(OnTalentButtonClicked);
            }

            if (mapButton != null)
            {
                mapButton.onClick.RemoveListener(OnMapButtonClicked);
                mapButton.onClick.AddListener(OnMapButtonClicked);
            }

            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);
                upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
            }
        }

        public void OnAutoHuntButtonClicked()
        {
            RefreshReferences();
            clickInteractor?.ToggleAutoHunt();
            RefreshView();
        }

        public void OnAttackButtonClicked()
        {
            RefreshReferences();
            playerAttack?.TryUseRangedAttack();
        }

        public void OnInventoryButtonClicked()
        {
            Debug.Log("Inventory button clicked. Inventory window is not implemented yet.");
        }

        public void OnTalentButtonClicked()
        {
            Manager?.OpenWindow(UITalentWindowController.WindowIdConst);
        }

        public void OnMapButtonClicked()
        {
            Manager?.OpenWindow(UIMapWindowController.WindowIdConst);
        }

        public void OnUpgradeButtonClicked()
        {
            Manager?.OpenWindow(UIUpgradeWindowController.WindowIdConst);
        }

        private void RefreshReferences()
        {
            if (playerStats != null && playerAttack != null && clickInteractor != null)
            {
                return;
            }

            var player = FindObjectOfType<PlayerController>();
            if (player == null)
            {
                return;
            }

            if (playerStats == null)
            {
                playerStats = player.GetComponent<CharacterStats>();
            }

            if (playerAttack == null)
            {
                playerAttack = player.GetComponent<PlayerAttack>();
            }

            if (clickInteractor == null)
            {
                clickInteractor = player.GetComponent<PlayerClickInteractor>();
            }
        }

        private void RefreshView()
        {
            var runtime = PlayerRuntimeDataService.EnsureExists().Data;
            PlayerRuntimeDataService.Instance?.SyncFromStats(playerStats);
            var maxHealth = Mathf.Max(1, runtime.maxHealth);
            var currentHealth = Mathf.Clamp(runtime.currentHealth, 0, maxHealth);
            var maxMana = Mathf.Max(0, runtime.maxMana);
            var currentMana = Mathf.Clamp(runtime.currentMana, 0, maxMana);
            var xpMax = System.Math.Max(1d, runtime.experienceToNextLevel);
            var currentXp = System.Math.Min(System.Math.Max(0d, runtime.experience), xpMax);

            SetText(playerNameText, runtime.playerName);
            SetText(classNameText, GetClassName(runtime.playerClass));
            SetText(levelText, $"LV. {runtime.level}");
            SetText(hpText, $"{currentHealth}/{maxHealth}");
            SetText(mpText, $"{currentMana}/{maxMana}");
            SetText(xpText, $"{FormatNumber(currentXp)} / {FormatNumber(xpMax)}");
            SetFill(hpFillImage, currentHealth, maxHealth);
            SetFill(mpFillImage, currentMana, maxMana);
            SetFill(xpFillImage, currentXp, xpMax);

            if (classIconImage != null)
            {
                classIconImage.sprite = GetClassIcon(runtime.playerClass);
            }

            if (autoHuntText != null)
            {
                autoHuntText.text = clickInteractor != null && clickInteractor.IsAutoHunting ? "ON" : "OFF";
            }
        }

        private Sprite GetClassIcon(PlayerClassType playerClass)
        {
            return playerClass switch
            {
                PlayerClassType.Warrior => warriorIcon,
                PlayerClassType.Archer => archerIcon,
                PlayerClassType.Mage => mageIcon,
                _ => noClassIcon
            };
        }

        private static string GetClassName(PlayerClassType playerClass)
        {
            return playerClass switch
            {
                PlayerClassType.Warrior => "WARRIOR",
                PlayerClassType.Archer => "ARCHER",
                PlayerClassType.Mage => "MAGE",
                _ => "NO CLASS"
            };
        }

        private static void SetFill(Image image, double current, double max)
        {
            if (image != null)
            {
                image.fillAmount = max > 0 ? Mathf.Clamp01((float)(current / max)) : 0f;
            }
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static string FormatNumber(double value)
        {
            if (value >= 1000000000d)
            {
                return $"{Mathf.FloorToInt((float)(value / 1000000000d))}B";
            }

            if (value >= 1000000d)
            {
                return $"{Mathf.FloorToInt((float)(value / 1000000d))}M";
            }

            if (value >= 1000d)
            {
                return $"{Mathf.FloorToInt((float)(value / 1000d))}K";
            }

            return Mathf.FloorToInt((float)value).ToString();
        }
    }
}
