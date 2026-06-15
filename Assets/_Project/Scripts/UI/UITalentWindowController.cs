using IdleonGame.Data;
using IdleonGame.Talents;
using UnityEngine;
using UnityEngine.UI;

namespace IdleonGame.UI
{
    public sealed class UITalentWindowController : UIWindowController
    {
        public const string WindowIdConst = "ui_talent";

        [SerializeField] private RectTransform talentListRoot;
        [SerializeField] private Button characterTabButton;
        [SerializeField] private Button classTabButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Image selectedIconImage;
        [SerializeField] private Text selectedNameText;
        [SerializeField] private Text selectedDescriptionText;
        [SerializeField] private Text selectedNextEffectText;
        [SerializeField] private Text pointsText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Sprite nodeBackgroundSprite;
        [SerializeField] private Sprite selectedNodeBackgroundSprite;
        [SerializeField] private Sprite tabActiveSprite;
        [SerializeField] private Sprite tabInactiveSprite;

        private TalentType currentTab = TalentType.Character;
        private TalentDefinition selectedTalent;

        protected override void OnOpen(object args)
        {
            var rectTransform = transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(560f, 640f);
            }

            BindButtons();
            SelectTab(TalentType.Character);
        }

        private void Update()
        {
            RefreshDetails();
        }

        private void BindButtons()
        {
            if (characterTabButton != null)
            {
                characterTabButton.onClick.RemoveListener(OnCharacterTabClicked);
                characterTabButton.onClick.AddListener(OnCharacterTabClicked);
            }

            if (classTabButton != null)
            {
                classTabButton.onClick.RemoveListener(OnClassTabClicked);
                classTabButton.onClick.AddListener(OnClassTabClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
                closeButton.onClick.AddListener(Close);
            }

            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);
                upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
            }
        }

        private void OnCharacterTabClicked()
        {
            SelectTab(TalentType.Character);
        }

        private void OnClassTabClicked()
        {
            SelectTab(TalentType.Class);
        }

        private void SelectTab(TalentType tab)
        {
            currentTab = tab;
            RefreshTabs();
            RebuildTalentList();
        }

        private void RebuildTalentList()
        {
            if (talentListRoot == null)
            {
                return;
            }

            for (var i = talentListRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(talentListRoot.GetChild(i).gameObject);
            }

            var database = TalentDatabase.Instance;
            if (database == null)
            {
                selectedTalent = null;
                RefreshDetails();
                return;
            }

            selectedTalent = null;
            foreach (var talent in database.GetTalentsByType(currentTab))
            {
                if (talent == null)
                {
                    continue;
                }

                if (currentTab == TalentType.Class && !CanShowClassTalent(talent))
                {
                    continue;
                }

                selectedTalent ??= talent;
                CreateTalentNode(talent);
            }

            RefreshDetails();
        }

        private void CreateTalentNode(TalentDefinition talent)
        {
            var nodeObject = new GameObject(talent.TalentId, typeof(RectTransform), typeof(Image), typeof(Button));
            nodeObject.transform.SetParent(talentListRoot, false);
            var background = nodeObject.GetComponent<Image>();
            background.sprite = talent == selectedTalent ? selectedNodeBackgroundSprite : nodeBackgroundSprite;
            background.type = Image.Type.Sliced;

            var iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(nodeObject.transform, false);
            var icon = iconObject.GetComponent<Image>();
            icon.sprite = talent.Icon;
            icon.raycastTarget = false;
            Stretch(icon.rectTransform, new Vector2(4f, 22f), new Vector2(-4f, -4f));

            var levelObject = new GameObject("Level", typeof(RectTransform), typeof(Text));
            levelObject.transform.SetParent(nodeObject.transform, false);
            var levelText = levelObject.GetComponent<Text>();
            levelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            levelText.fontSize = 13;
            levelText.alignment = TextAnchor.MiddleCenter;
            levelText.color = Color.white;
            levelText.raycastTarget = false;
            levelText.text = $"LV {PlayerRuntimeDataService.EnsureExists().Data.GetTalentLevel(talent.TalentId)}";
            SetRect(levelText.rectTransform, Vector2.zero, new Vector2(1f, 0f), new Vector2(0f, 11f), new Vector2(0f, 20f));

            var button = nodeObject.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                selectedTalent = talent;
                RebuildTalentList();
            });
        }

        private bool CanShowClassTalent(TalentDefinition talent)
        {
            var playerClass = PlayerRuntimeDataService.EnsureExists().Data.playerClass;
            return talent.ClassType == PlayerClassType.None || talent.ClassType == playerClass;
        }

        private void RefreshTabs()
        {
            SetTabState(characterTabButton, currentTab == TalentType.Character, "Character Talents");
            SetTabState(classTabButton, currentTab == TalentType.Class, "Class Talents");
        }

        private void RefreshDetails()
        {
            var runtime = PlayerRuntimeDataService.EnsureExists().Data;
            SetText(pointsText, $"POINTS LEFT: {runtime.talentUpgradePoints}");

            if (selectedTalent == null)
            {
                SetText(selectedNameText, string.Empty);
                SetText(selectedDescriptionText, string.Empty);
                SetText(selectedNextEffectText, string.Empty);
                if (selectedIconImage != null)
                {
                    selectedIconImage.sprite = null;
                }

                if (upgradeButton != null)
                {
                    upgradeButton.interactable = false;
                }

                return;
            }

            var currentLevel = runtime.GetTalentLevel(selectedTalent.TalentId);
            SetText(selectedNameText, $"{selectedTalent.DisplayName}  LV {currentLevel}/{selectedTalent.MaxLevel}");
            SetText(selectedDescriptionText, selectedTalent.Description);
            SetText(selectedNextEffectText, selectedTalent.GetNextUpgradeDescription(currentLevel));

            if (selectedIconImage != null)
            {
                selectedIconImage.sprite = selectedTalent.Icon;
            }

            if (upgradeButton != null)
            {
                upgradeButton.interactable = runtime.talentUpgradePoints > 0 && currentLevel < selectedTalent.MaxLevel;
            }
        }

        private void OnUpgradeButtonClicked()
        {
            if (PlayerRuntimeDataService.EnsureExists().TryUpgradeTalent(selectedTalent))
            {
                RebuildTalentList();
            }
        }

        private static void SetTabState(Button button, bool active, string text)
        {
            if (button == null)
            {
                return;
            }

            var image = button.GetComponent<Image>();
            var controller = button.GetComponentInParent<UITalentWindowController>();
            if (image != null && controller != null)
            {
                image.sprite = active ? controller.tabActiveSprite : controller.tabInactiveSprite;
            }

            var label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = text;
            }
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }
    }
}
