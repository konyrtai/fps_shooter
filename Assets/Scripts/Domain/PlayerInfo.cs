namespace Domain
{
    [System.Serializable]
    public class PlayerInfo
    {
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string Name;
        
        /// <summary>
        /// Id пользователя 
        /// </summary>
        public int Actor;

        /// <summary>
        /// Количество убийств
        /// </summary>
        public int KillsCount;

        /// <summary>
        /// Количество смертей
        /// </summary>
        public int DeathsCount;

        public PlayerInfo(string name, int actor, int killsCount, int deathsCount)
        {
            Name = name;
            Actor = actor;
            KillsCount = killsCount;
            DeathsCount = deathsCount;
        }
    }
}