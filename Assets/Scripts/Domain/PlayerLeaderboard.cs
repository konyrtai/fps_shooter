using TMPro;
using UnityEngine;

namespace Domain
{
    public class PlayerLeaderboard : MonoBehaviour
    {
        /// <summary>
        /// Имя игрока
        /// </summary>
        public TMP_Text PlayerName;

        /// <summary>
        /// Количество убийств
        /// </summary>
        public TMP_Text KillsCount;
        
        /// <summary>
        /// Количество смертей
        /// </summary>
        public TMP_Text DeathsCount;

        public void Set(string playerName, int killsCount, int deathsCount)
        {
            PlayerName.text = playerName;
            KillsCount.text = killsCount.ToString();
            DeathsCount.text = deathsCount.ToString();
        }
    }
}