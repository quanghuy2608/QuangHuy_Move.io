using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum GameState
{
    MainMenu,
    GamePlay,
    Pause,
    GameOver,
    Victory,
    Shopping,
    SkinSelect,
    WeaponSeclect,
}

public class GameManager : Singleton<GameManager>
{
    private GameState gameState;

    private void Start()
    {
        ChangeState(GameState.MainMenu);
    }

    public void ChangeState(GameState gameState)
    {
        this.gameState = gameState;
    }

    public bool IsState(GameState gameState)
    {
        return this.gameState == gameState;
    }
}
