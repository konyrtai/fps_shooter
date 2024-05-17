namespace Domain.Enums
{
    /// <summary>
    /// Состояние игры
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// Ожидание
        /// </summary>
        Waiting = 0,
        
        /// <summary>
        /// Игра
        /// </summary>
        Playing = 1,
        
        /// <summary>
        /// Завершение
        /// </summary>
        Ending = 2,
    }
}