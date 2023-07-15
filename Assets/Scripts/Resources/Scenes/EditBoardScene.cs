using SFB;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
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