namespace Domain.Enums
{
    /// <summary>
    /// Типы событий
    /// </summary>
    public enum EventType : byte
    {
        /// <summary>
        /// Добавлен новый игрок
        /// </summary>
        NewPlayer = 0,
        
        /// <summary>
        /// Список всех игроков
        /// </summary>
        ListPlayers = 1,
        
        /// <summary>
        /// Обновление статистики
        /// </summary>
        UpdateStats = 2,
    }
}