using System;
using Domain.Enums;
using UnityEngine;

namespace Controllers
{
    public class PlayerController : MonoBehaviour
    {
        /// <summary>
        /// Здоровье игрока
        /// </summary>
        public int health = 100;

        private void Start()
        {
            UpdateHealth(100);
            UIController.instance.healthSlider.maxValue = health;
        }

        /// <summary>
        /// Получить урон
        /// </summary>
        /// <param name="damage">Количество урона</param>
        /// <param name="damageBy">Кем нанесен урон</param>
        public void TakeDamage(int damage, string damageBy, int actorIdBy)
        {
            UpdateHealth(health - damage);
            if (health <= 0)
            {
                health = 0;
                PlayerSpawnManager.instance.Die(damageBy);
                MatchManager.instance.UpdateStatsSend(actorIdBy, StatType.Kills, 1);
            }
        }

        /// <summary>
        /// Обновить здоровье
        /// </summary>
        /// <param name="value"></param>
        private void UpdateHealth(int value)
        {
            health = value;
            UIController.instance.healthSlider.value = health;
        }
    }
}