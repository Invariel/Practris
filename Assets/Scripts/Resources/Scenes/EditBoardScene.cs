using SFB;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = Unity.Mathematics.Random;

public class EditBoardScene : MonoBehaviour
{
    private GameBoard gameBoard;
    public UserInput userInput;
    public Settings gameSettings;

    MinoEnum? minoClickedOriginalState = null;
    Point? minoLastHovered = null;

    private bool changingGarbageLines = false;
    public TMP_InputField garbageRowsInput;

    private Random random;


    public void Awake()
    {
        Application.targetFrameRate = 60;

        userInput.LoadSettingsFromFile();
        userInput.SaveSettingsToFile();

        StartNewGame();

        random = new Random();
        random.InitState();
    }

    public void StartNewGame()
    {
        gameBoard = new GameBoard(gameSettings.Style);

        gameBoard.MakeBoard(null);
        gameBoard.DrawGameBoard();

        SetUpEventHandlers();
    }

    public void ClearBoard()
    {
        MinoEnum[,] board = gameBoard._boardState;

        for (int x = 0; x < board.GetLength(0); ++ x)
        {
            for (int y = 0; y < board.GetLength(1); ++ y)
            {
                if (gameBoard._boardState[x, y] != MinoEnum.Border)
                {
                    gameBoard._boardState[x, y] = MinoEnum.Empty;
                }
            }
        }
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
            SceneManager.LoadSceneAsync(Constants.GetScene(Constants.Scene.TITLE));
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

        gameBoard._boardState[x, y] = (minoEnum == MinoEnum.Empty ? MinoEnum.Preset : MinoEnum.Empty);
        minoClickedOriginalState = minoEnum;
        minoLastHovered = new Point(x, y);
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

        if (minoEnum == minoClickedOriginalState!.Value)
        {
            if (minoLastHovered.Value.X != x || minoLastHovered.Value.Y != y)
            {
                gameBoard._boardState[x, y] = (minoEnum == MinoEnum.Empty ? MinoEnum.Preset : MinoEnum.Empty);
                minoLastHovered = new Point(x, y);
            }
        }
    }
#nullable disable

    public void InputFieldChanged()
    {
        if (!int.TryParse(garbageRowsInput.text, out int value))
        {
            value = Math.Max(0, Math.Min(value, 18));
            garbageRowsInput.text = "0";
        }
        else
        {
            int clipped = Math.Max(0, Math.Min(value, 18));
            if (value != clipped)
            {
                garbageRowsInput.text = clipped.ToString();
            }
        }
    }

    public void GenerateGarbage()
    {
        ClearBoard();

        MinoEnum[,] board = gameBoard._boardState;

        if (int.TryParse(garbageRowsInput.text, out int lines))
        {
            lines = Math.Max(0, Math.Min(lines, 18));

            for (int y = 1; y < lines; ++y)
            {
                bool hasGap = false;
                for (int x = 1; x < board.GetLength(0) - 1; ++x)
                {
                    if (x == board.GetLength(0) - 1 && !hasGap)
                    {
                        board[x, y] = MinoEnum.Empty;
                    }
                    else if (random.NextBool())
                    {
                        board[x, y] = MinoEnum.Preset;
                    }
                    else
                    {
                        hasGap = true;
                    }
                }
            }
        }
    }

    public void SaveBoardState()
    {
        string saveFileName = StandaloneFileBrowser.SaveFilePanel("Save Board State", ".", "board", "board");

        if (saveFileName is not null)
        {
            string boardState = JsonSerializer.Serialize(gameBoard.SerializeGameBoard(), new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(saveFileName, boardState, System.Text.Encoding.UTF8);
        }
    }

    public void LoadBoardState()
    {
        string[] openFileName = StandaloneFileBrowser.OpenFilePanel("Load Board State", ".", "board", false);

        if (openFileName.Any() && File.Exists(openFileName[0]))
        {
            string[] lines = JsonSerializer.Deserialize<string[]>(File.ReadAllText(openFileName[0]));
            MinoEnum[,] boardState = gameBoard.DeserializeGameBoard(lines);
            gameBoard.SetGameBoard(boardState);
        }
    }

    public void StartGame()
    {
        StaticData.editedGameBoard = gameBoard.CopyBoardState();
        SceneManager.LoadSceneAsync(Constants.GetScene(Constants.Scene.PLAYFIELD));
    }
}