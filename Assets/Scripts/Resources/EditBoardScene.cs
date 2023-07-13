using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EditBoardScene : MonoBehaviour
{
    private GameBoard gameBoard;
    public UserInput userInput;
    public Settings gameSettings;

    MinoEnum? minoClickedOriginalState = null;
    Point? minoLastHovered = null;

    public void Awake()
    {
        Application.targetFrameRate = 60;

        userInput.LoadSettingsFromFile();
        userInput.SaveSettingsToFile();

        StartNewGame();
    }

    public void StartNewGame()
    {
        gameBoard = new GameBoard(gameSettings.Style);

        gameBoard.MakeBoard(null);
        gameBoard.DrawGameBoard();

        SetUpEventHandlers();
    }

    // Update is called once per frame
    void Update()
    {
        ProcessUserInput();
        DrawGameBoard();

        if (!Input.GetMouseButton(0))
        {
            minoClickedOriginalState = null;
            minoLastHovered = null;
        }
    }

    private void ProcessUserInput()
    {
        int keysPressed = userInput.GetKeysPressed;
        if (UserInput.TestKey(KeyPressed.Menu, userInput.GetKeysPressed))
        {
            // Then exit this scene and load the title screen.
            SceneManager.LoadScene("TitleScene");
            SceneManager.UnloadSceneAsync("EditBoard");
        }
    }

    private void DrawGameBoard()
    {
        gameBoard.DrawGameBoard();
    }

    private void SetUpEventHandlers()
    {
        foreach (GameObject space in gameBoard._gameSurface)
        {
            (int x, int y) = GetMinoCoordinates(space.name);
            if (gameBoard._boardState[x, y] != MinoEnum.Border)
            {
                var clickHandler = space.GetComponent<Mino.MinoClicked>();
                clickHandler.OnClick += RespondToUserClick;

                var hoverHandler = space.GetComponent<Mino.MinoHovered>();
                hoverHandler.OnHover += RespondToHovering;
            }
        }
    }

    private (int, int) GetMinoCoordinates (string minoName)
    {
        string[] split = minoName.Split(" ");
        int.TryParse(split[0], out int x);
        int.TryParse(split[1], out int y);

        return (x, y);
    }

#nullable enable
    private void RespondToUserClick (object? sender, MinoEventArgs? eventArgs)
    {
        if (eventArgs is null)
        {
            return;
        }

        (int x, int y) = GetMinoCoordinates(eventArgs.GameObject.name);

        MinoEnum minoEnum = gameBoard._boardState[x, y];

        if (minoEnum == MinoEnum.Empty)
        {
            gameBoard._boardState[x, y] = MinoEnum.Preset;
            SpriteRenderer sr = gameBoard._gameSurface[x, y].GetComponent<SpriteRenderer>();
            string filename = Mino.GetSpriteFilename(MinoEnum.Preset, gameSettings.Style);

            minoClickedOriginalState = MinoEnum.Empty;
            minoLastHovered = new Point(x, y);
        }
        else
        {
            gameBoard._boardState[x, y] = MinoEnum.Empty;
            SpriteRenderer sr = gameBoard._gameSurface[x, y].GetComponent<SpriteRenderer>();
            string filename = Mino.GetSpriteFilename(MinoEnum.Empty, gameSettings.Style);

            minoClickedOriginalState = MinoEnum.Preset;
            minoLastHovered = new Point(x, y);
        }
    }

    private void RespondToHovering(object? sender, MinoEventArgs? eventArgs)
    {
        if (eventArgs is null ||
            !minoLastHovered.HasValue ||
            !Input.GetMouseButton(0))
        {
            return;
        }

        GameObject mino = eventArgs.GameObject;

        (int x, int y) = GetMinoCoordinates(eventArgs.GameObject.name);

        MinoEnum minoEnum = gameBoard._boardState[x, y];

        if (minoEnum == minoClickedOriginalState.Value)
        {
            if (minoLastHovered.Value.X != x || minoLastHovered.Value.Y != y)
            {
                gameBoard._boardState[x, y] = (minoEnum == MinoEnum.Empty ? MinoEnum.Preset : MinoEnum.Empty);
                minoLastHovered = new Point(x, y);
                Debug.Log($"Hovered over {x}, {y}, {minoEnum} state.");
            }
        }
    }
#nullable disable

    public void SaveBoardState()
    {
        string saveFileName = EditorUtility.SaveFilePanel("Save Board State", ".", "board", "board");

        if (saveFileName is not null)
        {
            string boardState = JsonSerializer.Serialize(gameBoard.SerializeGameBoard(), new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(saveFileName, boardState, System.Text.Encoding.UTF8);
        }
    }

    public void LoadBoardState()
    {
        string openFileName = EditorUtility.OpenFilePanel("Load Board State", ".", ".board");
        if (openFileName is not null && File.Exists(openFileName))
        {
            MinoEnum[,] boardState = JsonSerializer.Deserialize<MinoEnum[,]>(openFileName);
            gameBoard.SetGameBoard(boardState);
        }
    }
}