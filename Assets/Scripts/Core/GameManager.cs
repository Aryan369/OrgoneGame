using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private GameState _gameState = GameState.Play;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        if (Gamepad.current != null)
        {
            if (Gamepad.current.startButton.wasPressedThisFrame)
            {
                if (_gameState != GameState.Paused)
                {
                    PauseGame();
                }
                else
                {
                    ResumeGame();
                }
            }
        }
        else if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (_gameState != GameState.Paused)
                {
                    PauseGame();
                }
                else
                {
                    ResumeGame();
                }
            }
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        SceneManager.LoadScene("PauseMenu", LoadSceneMode.Additive);
        _gameState = GameState.Paused;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        SceneManager.UnloadScene("PauseMenu");
        _gameState = GameState.Play;
    }
}

public enum GameState
{
    Play,
    Paused
}