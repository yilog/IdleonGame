#if UNITY_EDITOR
using IdleonGame.Data;
using IdleonGame.Upgrades;
using UnityEditor;
using UnityEngine;

namespace IdleonGame.Editor
{
    public static class ResetPlayerRuntimeDataTool
    {
        private const string PlayerPrefsKey = "IdleonGame.PlayerRuntimeData";

        [MenuItem("IdleonGame/Tools/Reset Player Runtime Data")]
        public static void ResetData()
        {
            var data = new PlayerRuntimeData
            {
                coins = 100 * CurrencyFormatter.CopperPerGold
            };

            PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
            Debug.Log("Player runtime data reset. Coins kept at 100 gold.");
        }
    }
}
#endif
