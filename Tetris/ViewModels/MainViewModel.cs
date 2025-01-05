using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using Tetris.Model;
using Tetris.Model.Persistence;

namespace Tetris.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IImage[] _tileImages = new IImage[]
        {
            new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Single Blocks/TileEmpty2.png"))),
            new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Single Blocks/LightBlue.png"))),
            new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Single Blocks/Blue.png"))),
            new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Single Blocks/Orange.png"))),
            new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Single Blocks/Yellow.png"))),
            new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Single Blocks/Green.png"))),
            new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Single Blocks/Purple.png"))),
            new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Single Blocks/Red.png")))
        };

        private readonly IImage[] _blockImages = new IImage[]
        {
            new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Shape Blocks/BlockEmpty.png"))),
            new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Shape Blocks/I.png"))),
            new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Shape Blocks/J.png"))),
            new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Shape Blocks/L.png"))),
            new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Shape Blocks/O.png"))),
            new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Shape Blocks/S.png"))),
            new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Shape Blocks/T.png"))),
            new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Shape Blocks/Z.png")))
        };

        private ITetrisDataAccess _dataAccess = new TetrisDataAccess();
        private GameState _gameState = null!;
        private BlockCell[,] _imageControls = null!;
        private bool _isPaused;
        private bool _isPink;
        private DateTime _startTime;
        private DateTime _pauseStartTime;
        private double _pauseMinutes;
        private double _pauseSeconds;

        public ObservableCollection<BlockCell> GameCanvasImages { get; private set; }

        [ObservableProperty]
        private IImage _nextBlockImage =
            new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/start.png")));

        public event EventHandler? LoadGame;
        public event EventHandler<GameState>? SaveGame;


        public DelegateCommand NewEasyGameCommand { get; private set; }
        public DelegateCommand NewMediumGameCommand { get; private set; }
        public DelegateCommand NewHardGameCommand { get; private set; }
        public DelegateCommand? KeyInputCommand { get; private set; }
        public DelegateCommand NewGameCommand { get; private set; }
        public DelegateCommand PauseCommand { get; private set; }
        public DelegateCommand PinkCommand { get; private set; }
        public DelegateCommand LoadGameCommand { get; private set; }
        public DelegateCommand SaveGameCommand { get; private set; }
        public DelegateCommand? MovementCommand { get; private set; }

        [ObservableProperty] private int _canvasWidth = 420;
        [ObservableProperty] private int _canvasHeight = 650;
        [ObservableProperty] private SolidColorBrush _backgroundColor = new SolidColorBrush(Colors.Gray);

        public string Score => _gameState == null ? "Score: 0" : $"Score: {_gameState.Score.ToString()}";
        public bool GetState => _gameState != null;

        [ObservableProperty] private string _pausedText = "Pause";
        [ObservableProperty] private string _timeText = "";

        [ObservableProperty] private bool _pausedVisibility = false;
        [ObservableProperty] private bool _gameStartingVisibility = true;
        [ObservableProperty] private bool _newGameVisibility = false;
        [ObservableProperty] private bool _saveVisibility = false;
        [ObservableProperty] private bool _moveVisibility = false;

        public MainViewModel()
        {
            GameCanvasImages = new ObservableCollection<BlockCell>();

            NewEasyGameCommand = new DelegateCommand(async void (param) => await NewEasyGame());
            NewMediumGameCommand = new DelegateCommand(async void (param) => await NewMediumGame());
            NewHardGameCommand = new DelegateCommand(async void (param) => await NewHardGameAsync());
            NewGameCommand = new DelegateCommand(async void (param) => await NewGame());
            PauseCommand = new DelegateCommand(param => Pause());
            PinkCommand = new DelegateCommand(param => PinkMode());
            LoadGameCommand = new DelegateCommand(param => OnLoadGame());
            SaveGameCommand = new DelegateCommand(param => OnSaveGame());
        }

        private void SetupGameCanvas(GameGrid grid, int cellSize)
        {
            _imageControls = new BlockCell[grid.Rows, grid.Columns];
            GameCanvasImages.Clear();

            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    BlockCell imageControl = new BlockCell(_tileImages[0])
                    {
                        Width = cellSize,
                        Height = cellSize,
                        TopPosition = (r - 2) * cellSize + 10,
                        LeftPosition = c * cellSize
                    };
                    GameCanvasImages.Add(imageControl);
                    _imageControls[r, c] = imageControl;
                }
            }

            OnPropertyChanged(nameof(GameCanvasImages));
        }

        private void DrawGrid(GameGrid grid)
        {
            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    int id = grid[r, c];
                    _imageControls[r, c].ImageSource = _tileImages[id];
                }
            }
        }

        private void DrawBlock(Block block)
        {
            foreach (Position p in block.TilePositions())
            {
                _imageControls[p.Row, p.Column].ImageSource = _tileImages[block.Id];
            }
        }

        private void DrawNextBlock(BlockQueue blockQueue)
        {
            Block next = blockQueue.NextBlock;
            NextBlockImage = _blockImages[next.Id];
        }

        private void Draw()
        {
            DrawGrid(_gameState.GameGrid);
            DrawBlock(_gameState.CurrentBlock);
            DrawNextBlock(_gameState.BlockQueue);
        }

        private async Task GameLoop()
        {
            Draw();
            while (!_gameState.GameOver)
            {
                await Task.Delay(600);
                if (!_isPaused)
                {
                    _gameState.MoveBlockDown();
                    Draw();
                }
            }


            StopGame();
            NextBlockImage = new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/start.png")));
            ScoreChanged(_gameState, EventArgs.Empty);
            PausedVisibility = false;
            MoveVisibility = false;
            TimeText =
                $"Time: {(DateTime.Now - _startTime).TotalMinutes - _pauseMinutes:F0} mins\n\t{((DateTime.Now - _startTime).TotalSeconds - _pauseSeconds) % 60:F0} sec";
        }

        private void OnKeyDown(string key)
        {
            switch (key)
            {
                case "l":
                    _gameState.MoveBlockLeft();
                    break;
                case "r":
                    _gameState.MoveBlockRight();
                    break;
                case "d":
                    _gameState.MoveBlockDown();
                    break;
                case "u":
                    _gameState.RotateBlockCW();
                    break;
                case "z":
                    _gameState.RotateBlockCCW();
                    break;
                case "x":
                    _gameState.DropBlock();
                    break;
                default:
                    return;
            }

            Draw();
        }


        //Commands and methods from here

        private async Task NewEasyGame()
        {
            _gameState = new GameState(18, 4, _dataAccess);
            _gameState.ScoreChanged += ScoreChanged;
            await InitializeGameAsync();
        }

        private async Task NewMediumGame()
        {
            _gameState = new GameState(18, 8, _dataAccess);
            _gameState.ScoreChanged += ScoreChanged;
            await InitializeGameAsync();
        }

        private async Task NewHardGameAsync()
        {
            _gameState = new GameState(_dataAccess);
            _gameState.ScoreChanged += ScoreChanged;
            await InitializeGameAsync();
        }

        public async Task InitializeGameAsync()
        {
            GameStartingVisibility = false;
            PausedVisibility = true;
            MoveVisibility = true;

            switch (_gameState.GameDifficulty)
            {
                case GameDifficulty.Easy:
                    if (OperatingSystem.IsWindows())
                    {
                        SetupGameCanvas(_gameState.GameGrid, 35);

                        CanvasHeight = 570;
                        CanvasWidth = 140;
                    }
                    else if (OperatingSystem.IsAndroid())
                    {
                        SetupGameCanvas(_gameState.GameGrid, 42);

                        CanvasHeight = 680;
                        CanvasWidth = 168;
                    }

                    break;
                case GameDifficulty.Medium:
                    if (OperatingSystem.IsWindows())
                    {
                        SetupGameCanvas(_gameState.GameGrid, 35);

                        CanvasHeight = 570;
                        CanvasWidth = 280;
                    }
                    else if (OperatingSystem.IsAndroid())
                    {
                        SetupGameCanvas(_gameState.GameGrid, 40);
                        CanvasHeight = 650;
                        CanvasWidth = 320;
                    }

                    break;
                case GameDifficulty.Hard:

                    if (OperatingSystem.IsWindows())
                    {
                        SetupGameCanvas(_gameState.GameGrid, 35);

                        CanvasHeight = 570;
                        CanvasWidth = 420;
                    }
                    else if (OperatingSystem.IsAndroid())
                    {
                        SetupGameCanvas(_gameState.GameGrid, 33);
                        CanvasHeight = 538;
                        CanvasWidth = 400;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Draw();
            _startTime = DateTime.Now;
            if (OperatingSystem.IsWindows())
            {
                KeyInputCommand = new DelegateCommand(param => OnKeyDown(param?.ToString() ?? string.Empty));
                OnPropertyChanged(nameof(KeyInputCommand));
            }
            else if (OperatingSystem.IsAndroid())
            {
                MovementCommand = new DelegateCommand(param => OnKeyDown(param?.ToString() ?? string.Empty));
                OnPropertyChanged(nameof(MovementCommand));
            }

            await GameLoop();
        }

        private void StopGame()
        {
            if (OperatingSystem.IsWindows())
            {
                KeyInputCommand = null;
                OnPropertyChanged(nameof(KeyInputCommand));
            }
            else if (OperatingSystem.IsAndroid())
            {
                MovementCommand = null;
                OnPropertyChanged(nameof(MovementCommand));
            }

            NewGameVisibility = true;
            MoveVisibility = false;
        }

        private void ContinueGame()
        {
            if (OperatingSystem.IsWindows())
            {
                KeyInputCommand = new DelegateCommand(param => OnKeyDown(param?.ToString() ?? string.Empty));
                OnPropertyChanged(nameof(KeyInputCommand));
            }
            else if (OperatingSystem.IsAndroid())
            {
                MovementCommand = new DelegateCommand(param => OnKeyDown(param?.ToString() ?? string.Empty));
                OnPropertyChanged(nameof(MovementCommand));
            }

            _isPaused = false;

            SaveVisibility = false;
            GameStartingVisibility = false;
            PausedVisibility = true;
            MoveVisibility = true;
            PausedText = "Pause";
            NewGameVisibility = false;
        }

        private async Task NewGame()
        {
            bool gameWasOver = _gameState.GameOver;
            if (_gameState.GameDifficulty == GameDifficulty.Easy)
            {
                _gameState = new GameState(18, 4, _dataAccess);
            }
            else if (_gameState.GameDifficulty == GameDifficulty.Medium)
            {
                _gameState = new GameState(18, 8, _dataAccess);
            }
            else
            {
                _gameState = new GameState(_dataAccess);
            }

            _pauseSeconds = 0;
            _pauseMinutes = 0;
            ContinueGame();
            _startTime = DateTime.Now;
            ScoreChanged(_gameState, EventArgs.Empty);
            _gameState.ScoreChanged += ScoreChanged;
            TimeText = "";

            if (gameWasOver)
            {
                await GameLoop();
            }
        }

        private void Pause()
        {
            if (!_isPaused)
            {
                _pauseStartTime = DateTime.Now;
                StopGame();
                _isPaused = true;
                SaveVisibility = true;
                PausedText = "Play";
            }
            else
            {
                _pauseMinutes += (DateTime.Now - _pauseStartTime).TotalMinutes;
                _pauseSeconds += (DateTime.Now - _pauseStartTime).TotalSeconds;
                ContinueGame();
            }
        }

        private void PinkMode()
        {
            if (!_isPink)
            {
                for (int i = 0; i < _tileImages.Length; i++)
                {
                    _tileImages[i] =
                        new Bitmap(AssetLoader.Open(new Uri($"avares://Tetris/Assets/Single Blocks/Pink{i}.png")));
                }

                _isPink = true;
                BackgroundColor = new SolidColorBrush(Color.FromArgb(255, 247, 166, 200));
            }
            else
            {
                _isPink = false;
                _tileImages[0] =
                    new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Single Blocks/TileEmpty2.png")));
                _tileImages[1] =
                    new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Single Blocks/LightBlue.png")));
                _tileImages[2] = new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Single Blocks/Blue.png")));
                _tileImages[3] =
                    new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Single Blocks/Orange.png")));
                _tileImages[4] =
                    new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Single Blocks/Yellow.png")));
                _tileImages[5] =
                    new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Single Blocks/Green.png")));
                _tileImages[6] =
                    new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Single Blocks/Purple.png")));
                _tileImages[7] = new Bitmap(AssetLoader.Open(new Uri("avares://Tetris/Assets/Single Blocks/Red.png")));
                BackgroundColor = new SolidColorBrush(Colors.Gray);
            }

            Draw();
        }

        public void LoadSave(GameState gameState)
        {
            _gameState = gameState;
            _gameState.ScoreChanged += ScoreChanged;
            ScoreChanged(_gameState, EventArgs.Empty);
        }

        private void OnLoadGame()
        {
            LoadGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnSaveGame()
        {
            SaveGame?.Invoke(this, _gameState);
        }

        private void ScoreChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(Score));
        }

        public void BackgroundPause(bool appActive)
        {
            _isPaused = appActive;
            Pause();
        }
    }
}