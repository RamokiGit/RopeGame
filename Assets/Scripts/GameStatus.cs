using UnityEngine;

public static class GameStatus
{
    public static GameState CurrentGameState;
}

public enum GameState
{
    Pause,
    Game,
    CompleteLevel
}