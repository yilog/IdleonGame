using UnityEngine;

namespace IdleonGame.Character
{
    [DisallowMultipleComponent]
    public sealed class CharacterStats : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth = 100;
        [SerializeField] private int maxMana = 50;
        [SerializeField] private int currentMana = 50;
        [SerializeField] private int baseAttack = 5;
        [SerializeField] private int defense = 0;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public int MaxMana => maxMana;
        public int CurrentMana => currentMana;
        public int BaseAttack => baseAttack;
        public int Defense => defense;
        public bool IsDead => currentHealth <= 0;

        private void Awake()
        {
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        }

        public void Configure(int health, int mana, int attack, int armor)
        {
            maxHealth = Mathf.Max(1, health);
            currentHealth = maxHealth;
            maxMana = Mathf.Max(0, mana);
            currentMana = maxMana;
            baseAttack = Mathf.Max(0, attack);
            defense = Mathf.Max(0, armor);
        }

        public void ConfigureSnapshot(int newMaxHealth, int newCurrentHealth, int newMaxMana, int newCurrentMana, int newBaseAttack, int newDefense)
        {
            maxHealth = Mathf.Max(1, newMaxHealth);
            currentHealth = Mathf.Clamp(newCurrentHealth, 0, maxHealth);
            maxMana = Mathf.Max(0, newMaxMana);
            currentMana = Mathf.Clamp(newCurrentMana, 0, maxMana);
            baseAttack = Mathf.Max(0, newBaseAttack);
            defense = Mathf.Max(0, newDefense);
        }

        public bool SpendMana(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (currentMana < amount)
            {
                return false;
            }

            currentMana -= amount;
            return true;
        }

        public int ApplyDamage(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return 0;
            }

            var applied = Mathf.Min(currentHealth, amount);
            currentHealth -= applied;
            return applied;
        }
    }
}
