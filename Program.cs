// Classe para representar um ponto (posição) no jogo
public class Point
{
    public int X { get; set; }
    public int Y { get; set; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static bool operator ==(Point a, Point b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Point a, Point b) => !(a == b);

    public override bool Equals(object obj)
    {
        if (obj is Point other)
            return X == other.X && Y == other.Y;
        return false;
    }

    public override int GetHashCode() => (X << 16) | Y;

    public Point Clone() => new Point(X, Y);
}

// Classe para representar itens que aparecem no jogo
public class GameObject
{
    public Point Position { get; set; }
    public ConsoleColor Color { get; set; }
    public char Symbol { get; set; }
    public int Value { get; set; }
    public int Duration { get; set; } // Para efeitos temporários
    public bool IsActive { get; set; } = true;

    public GameObject(int x, int y, char symbol, ConsoleColor color, int value = 0, int duration = 0)
    {
        Position = new Point(x, y);
        Symbol = symbol;
        Color = color;
        Value = value;
        Duration = duration;
    }

    public virtual void Draw()
    {
        Console.ForegroundColor = Color;
        Console.SetCursorPosition(Position.X, Position.Y);
        Console.Write(Symbol);
    }
}

// Classe específica para comida
public class Food : GameObject
{
    public FoodType Type { get; set; }

    public Food(int x, int y, FoodType type) 
        : base(x, y, GetSymbol(type), GetColor(type), GetValue(type), GetDuration(type))
    {
        Type = type;
    }

    private static char GetSymbol(FoodType type)
    {
        return type switch
        {
            FoodType.Regular => '●',
            FoodType.Special => '♦',
            FoodType.SpeedBoost => '◄',
            FoodType.Shield => '■',
            FoodType.Portal => '○',
            _ => '*'
        };
    }

    private static ConsoleColor GetColor(FoodType type)
    {
        return type switch
        {
            FoodType.Regular => ConsoleColor.Red,
            FoodType.Special => ConsoleColor.Magenta,
            FoodType.SpeedBoost => ConsoleColor.Yellow,
            FoodType.Shield => ConsoleColor.Blue,
            FoodType.Portal => ConsoleColor.Cyan,
            _ => ConsoleColor.White
        };
    }

    private static int GetValue(FoodType type)
    {
        return type switch
        {
            FoodType.Regular => 10,
            FoodType.Special => 30,
            FoodType.SpeedBoost => 5,
            FoodType.Shield => 20,
            FoodType.Portal => 15,
            _ => 1
        };
    }

    private static int GetDuration(FoodType type)
    {
        return type switch
        {
            FoodType.SpeedBoost => 30, // Duração em ciclos do jogo
            FoodType.Shield => 40,
            _ => 0
        };
    }
}

// Classe para obstáculos
public class Obstacle : GameObject
{
    public ObstacleType Type { get; set; }
    public Direction MovingDirection { get; set; }

    public Obstacle(int x, int y, ObstacleType type)
        : base(x, y, type == ObstacleType.Wall ? '█' : '▲', type == ObstacleType.Wall ? ConsoleColor.Gray : ConsoleColor.DarkRed)
    {
        Type = type;
        if (type == ObstacleType.MovingObstacle)
            MovingDirection = (Direction)new Random().Next(4);
    }

    public void Move(int width, int height, List<Obstacle> obstacles)
    {
        if (Type != ObstacleType.MovingObstacle)
            return;

        Point newPosition = Position.Clone();

        switch (MovingDirection)
        {
            case Direction.Up: newPosition.Y--; break;
            case Direction.Down: newPosition.Y++; break;
            case Direction.Left: newPosition.X--; break;
            case Direction.Right: newPosition.X++; break;
        }

        // Verificar colisão com bordas ou outros obstáculos
        if (newPosition.X <= 0 || newPosition.X >= width - 1 || newPosition.Y <= 0 || newPosition.Y >= height - 1 ||
            obstacles.Any(o => o != this && o.Position.X == newPosition.X && o.Position.Y == newPosition.Y))
        {
            // Mudar direção quando colidir
            MovingDirection = (Direction)(((int)MovingDirection + 2) % 4);
            return;
        }

        Position = newPosition;
    }
}

// Classe principal do jogo
public class SnakeGame
{
    // Constantes e configurações
    private const int MinWidth = 40;
    private const int MinHeight = 20;
    private const int MaxLevel = 10;
    private int Width { get; set; }
    private int Height { get; set; }

    // Estado do jogo
    private List<Point> Snake { get; set; }
    private Direction CurrentDirection { get; set; }
    private Direction NextDirection { get; set; }
    private GameState State { get; set; }
    private int Score { get; set; }
    private int Level { get; set; }
    private int GameSpeed { get; set; }
    private int InitialGameSpeed { get; set; }
    private Random Random { get; set; }

    // Efeitos e status
    private bool HasShield { get; set; }
    private int ShieldDuration { get; set; }
    private int SpeedBoostDuration { get; set; }
    private int FoodCounter { get; set; }
    private int SpecialFoodCounter { get; set; }
    private int TicksSinceLastFood { get; set; }

    // Game objects
    private List<Food> Foods { get; set; }
    private List<Obstacle> Obstacles { get; set; }
    private Point Portal1 { get; set; }
    private Point Portal2 { get; set; }

    // Highscores e estatísticas
    private List<(string Name, int Score, int Level)> Highscores { get; set; }
    private Stopwatch GameTimer { get; set; }
    private int MovesMade { get; set; }
    private int FoodEaten { get; set; }
    private int SpecialItemsCollected { get; set; }

    // Construtor
    public SnakeGame(int width = MinWidth, int height = MinHeight)
    {
        Width = Math.Max(width, MinWidth);
        Height = Math.Max(height, MinHeight);
        Snake = new List<Point>();
        Random = new Random();
        Foods = new List<Food>();
        Obstacles = new List<Obstacle>();
        Highscores = LoadHighscores();
        GameTimer = new Stopwatch();
        InitializeGame();
    }

    // Configuração inicial do jogo
    private void InitializeGame()
    {
        Console.CursorVisible = false;
        Console.Title = "Snake Game Deluxe";

        // Configurações iniciais
        Snake.Clear();
        Foods.Clear();
        Obstacles.Clear();

        Level = 1;
        Score = 0;
        FoodCounter = 0;
        SpecialFoodCounter = 0;
        MovesMade = 0;
        FoodEaten = 0;
        SpecialItemsCollected = 0;
        HasShield = false;
        ShieldDuration = 0;
        SpeedBoostDuration = 0;
        State = GameState.Running;
        InitialGameSpeed = 150;
        GameSpeed = InitialGameSpeed;
        
        // Posicionamento inicial da cobra
        int startX = Width / 2;
        int startY = Height / 2;
        CurrentDirection = Direction.Right;
        NextDirection = Direction.Right;
        
        // Criar corpo inicial da cobra
        Snake.Add(new Point(startX, startY));
        Snake.Add(new Point(startX - 1, startY));
        Snake.Add(new Point(startX - 2, startY));

        // Adicionar comida inicial
        GenerateFood();
        
        // Configurar o nível
        SetupLevel(Level);
        
        GameTimer.Restart();
    }

    // Configuração específica de cada nível
    private void SetupLevel(int level)
    {
        Obstacles.Clear();
        
        // Dificuldade progressiva baseada no nível
        int obstacleCount = level * 2;
        InitialGameSpeed = Math.Max(50, 150 - (level * 10));
        GameSpeed = InitialGameSpeed;
        
        // Nível 1 não tem obstáculos
        if (level == 1)
            return;
            
        // Criar obstáculos estáticos para cada nível
        for (int i = 0; i < obstacleCount; i++)
        {
            int x, y;
            bool validPosition;
            
            do
            {
                x = Random.Next(1, Width - 1);
                y = Random.Next(1, Height - 1);
                validPosition = IsValidPosition(x, y);
            } while (!validPosition);
            
            Obstacles.Add(new Obstacle(x, y, ObstacleType.Wall));
        }
        
        // Níveis mais altos têm obstáculos que se movem
        if (level >= 5)
        {
            for (int i = 0; i < level - 4; i++)
            {
                int x, y;
                bool validPosition;
                
                do
                {
                    x = Random.Next(5, Width - 5);
                    y = Random.Next(5, Height - 5);
                    validPosition = IsValidPosition(x, y);
                } while (!validPosition);
                
                Obstacles.Add(new Obstacle(x, y, ObstacleType.MovingObstacle));
            }
        }
        
        // Níveis 8+ têm labirintos mais complexos
        if (level >= 8)
        {
            // Criar alguns padrões de labirinto
            CreateMazePattern(level);
        }
        
        // Níveis 7+ têm portais
        if (level >= 7)
        {
            GeneratePortals();
        }
    }
    
    private void CreateMazePattern(int level)
    {
        int patternType = level % 3;
        
        switch (patternType)
        {
            case 0: // Padrão em cruz
                for (int i = Width / 4; i < Width * 3 / 4; i++)
                {
                    if (IsValidPosition(i, Height / 2))
                        Obstacles.Add(new Obstacle(i, Height / 2, ObstacleType.Wall));
                }
                for (int i = Height / 4; i < Height * 3 / 4; i++)
                {
                    if (IsValidPosition(Width / 2, i))
                        Obstacles.Add(new Obstacle(Width / 2, i, ObstacleType.Wall));
                }
                break;
                
            case 1: // Padrão em caixas
                for (int box = 0; box < 3; box++)
                {
                    int boxX = Width / 4 + (box * Width / 6);
                    int boxY = Height / 3;
                    int boxSize = Math.Min(Width, Height) / 8;
                    
                    for (int i = 0; i < boxSize; i++)
                    {
                        if (IsValidPosition(boxX + i, boxY))
                            Obstacles.Add(new Obstacle(boxX + i, boxY, ObstacleType.Wall));
                        if (IsValidPosition(boxX + i, boxY + boxSize))
                            Obstacles.Add(new Obstacle(boxX + i, boxY + boxSize, ObstacleType.Wall));
                        if (IsValidPosition(boxX, boxY + i))
                            Obstacles.Add(new Obstacle(boxX, boxY + i, ObstacleType.Wall));
                        if (IsValidPosition(boxX + boxSize, boxY + i))
                            Obstacles.Add(new Obstacle(boxX + boxSize, boxY + i, ObstacleType.Wall));
                    }
                }
                break;
                
            case 2: // Padrão em ziguezague
                for (int i = 0; i < 3; i++)
                {
                    int startX = Width / 6;
                    int endX = Width * 5 / 6;
                    int y = Height / 4 + (i * Height / 4);
                    
                    if (i % 2 == 0)
                    {
                        for (int x = startX; x < endX; x++)
                        {
                            if (IsValidPosition(x, y))
                                Obstacles.Add(new Obstacle(x, y, ObstacleType.Wall));
                        }
                    }
                    else
                    {
                        for (int x = startX; x < endX; x++)
                        {
                            if (x < startX + Width / 6 || x > endX - Width / 6)
                            {
                                if (IsValidPosition(x, y))
                                    Obstacles.Add(new Obstacle(x, y, ObstacleType.Wall));
                            }
                        }
                    }
                }
                break;
        }
    }
    
    private void GeneratePortals()
    {
        int x1, y1, x2, y2;
        bool validPosition1, validPosition2;
        
        do
        {
            x1 = Random.Next(2, Width - 2);
            y1 = Random.Next(2, Height - 2);
            validPosition1 = IsValidPosition(x1, y1);
        } while (!validPosition1);
        
        do
        {
            x2 = Random.Next(2, Width - 2);
            y2 = Random.Next(2, Height - 2);
            // Garantir que os portais estão a uma distância mínima um do outro
            validPosition2 = IsValidPosition(x2, y2) && 
                             Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)) > Width / 3;
        } while (!validPosition2);
        
        Portal1 = new Point(x1, y1);
        Portal2 = new Point(x2, y2);
    }
    
    // Verifica se uma posição está livre de obstáculos e da cobra
    private bool IsValidPosition(int x, int y)
    {
        // Verificar se não está na cobra
        if (Snake.Any(segment => segment.X == x && segment.Y == y))
            return false;
            
        // Verificar se não está em um obstáculo
        if (Obstacles.Any(obs => obs.Position.X == x && obs.Position.Y == y))
            return false;
            
        // Verificar se não está em uma comida
        if (Foods.Any(food => food.Position.X == x && food.Position.Y == y))
            return false;
            
        // Verificar se não está em um portal
        if ((Portal1 != null && Portal1.X == x && Portal1.Y == y) ||
            (Portal2 != null && Portal2.X == x && Portal2.Y == y))
            return false;
            
        return true;
    }
    
    // Gera um novo item de comida
    private void GenerateFood()
    {
        FoodType foodType = DetermineNextFoodType();
        int x, y;
        bool validPosition;
        
        do
        {
            x = Random.Next(1, Width - 1);
            y = Random.Next(1, Height - 1);
            validPosition = IsValidPosition(x, y);
        } while (!validPosition);
        
        Foods.Add(new Food(x, y, foodType));
        TicksSinceLastFood = 0;
    }
    
    private FoodType DetermineNextFoodType()
    {
        FoodCounter++;
        
        // A cada X comidas regulares, gerar uma comida especial
        if (FoodCounter % 5 == 0)
        {
            SpecialFoodCounter++;
            
            // Alternar entre diferentes tipos de comidas especiais
            return (FoodType)(SpecialFoodCounter % 3 + 1);
        }
        
        // Comida regular na maioria das vezes
        return FoodType.Regular;
    }
    
    // Loop principal do jogo
    public void Run()
    {
        ShowMainMenu();
        
        while (State != GameState.GameOver && State != GameState.Victory)
        {
            if (State == GameState.Running)
            {
                ProcessInput();
                Update();
                Render();
                Thread.Sleep(GameSpeed);
            }
            else if (State == GameState.Paused)
            {
                ProcessPausedInput();
                Thread.Sleep(100);
            }
        }
        
        GameOver();
    }
    
    // Menu principal
    private void ShowMainMenu()
    {
        bool exitMenu = false;
        int selectedOption = 0;
        string[] options = { "Novo Jogo", "Instruções", "Pontuações", "Sair" };
        
        while (!exitMenu)
        {
            Console.Clear();
            DrawLogo();
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n\n");
            
            for (int i = 0; i < options.Length; i++)
            {
                if (i == selectedOption)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($" > {options[i]} < ");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"   {options[i]}   ");
                }
            }
            
            ConsoleKeyInfo key = Console.ReadKey(true);
            
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    selectedOption = (selectedOption - 1 + options.Length) % options.Length;
                    break;
                case ConsoleKey.DownArrow:
                    selectedOption = (selectedOption + 1) % options.Length;
                    break;
                case ConsoleKey.Enter:
                    switch (selectedOption)
                    {
                        case 0: // Novo Jogo
                            exitMenu = true;
                            break;
                        case 1: // Instruções
                            ShowInstructions();
                            break;
                        case 2: // Pontuações
                            ShowHighscores();
                            break;
                        case 3: // Sair
                            Environment.Exit(0);
                            break;
                    }
                    break;
            }
        }
    }
    
    private void DrawLogo()
    {
        string[] logo = {
            @" _____             _          _____                      ",
            @"/  ___|           | |        |  __ \                     ",
            @"\ `--.  _ __   __ | | _ ___  | |  \/ __ _ _ __ ___   ___ ",
            @" `--. \| '_ \ / _` | |/ / _ \ | | __ / _` | '_ ` _ \ / _ \",
            @"/\__/ /| | | | (_| |   <  __/ | |_\ \ (_| | | | | | |  __/",
            @"\____/ |_| |_|\__,_|_|\_\___|  \____/\__,_|_| |_| |_|\___|",
            @"                                                          ",
            @"                      PROF. TOM                            "
        };
        
        Console.ForegroundColor = ConsoleColor.Green;
        foreach (string line in logo)
        {
            Console.WriteLine(line);
        }
    }
    
    private void ShowInstructions()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("=== INSTRUÇÕES ===\n");
        Console.WriteLine("Controles:");
        Console.WriteLine("Setas direcionais - Mover a cobra");
        Console.WriteLine("P - Pausar o jogo");
        Console.WriteLine("ESC - Sair para o menu principal\n");
        
        Console.WriteLine("Itens:");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("● ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("- Comida regular (10 pontos)");
        
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("♦ ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("- Comida especial (30 pontos)");
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("◄ ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("- Boost de velocidade (5 pontos)");
        
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("■ ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("- Escudo (20 pontos, protege contra uma colisão)");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("○ ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("- Portal (15 pontos, teleporta para outro local)\n");
        
        Console.WriteLine("Obstáculos:");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write("█ ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("- Parede (colisão causa Game Over)");
        
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write("▲ ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("- Obstáculo móvel (muito perigoso!)\n");
        
        Console.WriteLine("Objetivos:");
        Console.WriteLine("- Comer o máximo de comida possível");
        Console.WriteLine("- Avançar nos níveis (a cada 100 pontos)");
        Console.WriteLine("- Evitar colidir com obstáculos e com o próprio corpo");
        Console.WriteLine("- Completar todos os 10 níveis para vencer o jogo\n");
        
        Console.WriteLine("Pressione qualquer tecla para voltar...");
        Console.ReadKey(true);
    }
    
    private void ShowHighscores()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("=== MELHORES PONTUAÇÕES ===\n");
        
        if (Highscores.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Nenhuma pontuação registrada ainda.");
        }
        else
        {
            for (int i = 0; i < Math.Min(10, Highscores.Count); i++)
            {
                var score = Highscores[i];
                Console.ForegroundColor = i < 3 ? ConsoleColor.Green : ConsoleColor.White;
                Console.WriteLine($"{i + 1}. {score.Name} - {score.Score} pts (Nível {score.Level})");
            }
        }
        
        Console.WriteLine("\nPressione qualquer tecla para voltar...");
        Console.ReadKey(true);
    }
    
    // Processa entrada do usuário
    private void ProcessInput()
    {
        if (Console.KeyAvailable)
        {
            ConsoleKey key = Console.ReadKey(true).Key;
            
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    if (CurrentDirection != Direction.Down)
                        NextDirection = Direction.Up;
                    break;
                case ConsoleKey.DownArrow:
                    if (CurrentDirection != Direction.Up)
                        NextDirection = Direction.Down;
                    break;
                case ConsoleKey.LeftArrow:
                    if (CurrentDirection != Direction.Right)
                        NextDirection = Direction.Left;
                    break;
                case ConsoleKey.RightArrow:
                    if (CurrentDirection != Direction.Left)
                        NextDirection = Direction.Right;
                    break;
                case ConsoleKey.P:
                    State = GameState.Paused;
                    GameTimer.Stop();
                    DrawPauseScreen();
                    break;
                case ConsoleKey.Escape:
                    State = GameState.GameOver;
                    break;
            }
        }
    }
    
    private void ProcessPausedInput()
    {
        if (Console.KeyAvailable)
        {
            ConsoleKey key = Console.ReadKey(true).Key;
            
            switch (key)
            {
                case ConsoleKey.P:
                case ConsoleKey.Enter:
                    State = GameState.Running;
                    GameTimer.Start();
                    break;
                case ConsoleKey.Escape:
                    State = GameState.GameOver;
                    break;
            }
        }
    }
    
    private void DrawPauseScreen()
    {
        Console.SetCursorPosition(Width / 2 - 5, Height / 2);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("JOGO PAUSADO");
        
        Console.SetCursorPosition(Width / 2 - 15, Height / 2 + 2);
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Pressione P ou ENTER para continuar");
        
        Console.SetCursorPosition(Width / 2 - 14, Height / 2 + 3);
        Console.Write("Pressione ESC para sair do jogo");
    }
    
    // Atualiza o estado do jogo
    private void Update()
    {
        // Atualizar a direção da cobra
        CurrentDirection = NextDirection;
        
        // Obter a posição atual da cabeça
        Point head = Snake[0];
        Point newHead = new Point(head.X, head.Y);
        
        // Mover a cabeça na direção atual
        switch (CurrentDirection)
        {
            case Direction.Up:
                newHead.Y--;
                break;
            case Direction.Down:
                newHead.Y++;
                break;
            case Direction.Left:
                newHead.X--;
                break;
            case Direction.Right:
                newHead.X++;
                break;
        }
        
        MovesMade++;
        
        // Verificar teletransporte pelos portais
        if (Portal1 != null && Portal2 != null)
        {
            if (newHead.X == Portal1.X && newHead.Y == Portal1.Y)
            {
                newHead.X = Portal2.X;
                newHead.Y = Portal2.Y;
            }
            else if (newHead.X == Portal2.X && newHead.Y == Portal2.Y)
            {
                newHead.X = Portal1.X;
                newHead.Y = Portal1.Y;
            }
        }
        
        // Verificar colisão com as bordas do mapa
        if (newHead.X < 0 || newHead.X >= Width || newHead.Y < 0 || newHead.Y >= Height)
        {
            if (HasShield)
            {
                HasShield = false;
                ShieldDuration = 0;
                
                // Colocar a cobra de volta em uma posição segura
                newHead.X = Math.Max(0, Math.Min(Width - 1, newHead.X));
                newHead.Y = Math.Max(0, Math.Min(Height - 1, newHead.Y));
            }
            else
            {
                State = GameState.GameOver;
                return;
            }
        }
        
        // Verificar colisão com obstáculos
        foreach (var obstacle in Obstacles)
        {
            if (newHead.X == obstacle.Position.X && newHead.Y == obstacle.Position.Y)
            {
                if (HasShield)
                {
                    HasShield = false;
                    ShieldDuration = 0;
                    
                    // Se for um obstáculo móvel, apenas removê-lo
                    if (obstacle.Type == ObstacleType.MovingObstacle)
                    {
                        Obstacles.Remove(obstacle);
                    }
                    
                    // Mover para uma posição segura
                    do
                    {
                        newHead = Snake[0].Clone();
                    } while (!IsValidPosition(newHead.X, newHead.Y));
                }
                else
                {
                    State = GameState.GameOver;
                    return;
                }
                break;
            }
        }
        
        // Mover obstáculos móveis
        foreach (var obstacle in Obstacles.Where(o => o.Type == ObstacleType.MovingObstacle).ToList())
        {
            obstacle.Move(Width, Height, Obstacles);
        }
        
        // Verificar colisão com o próprio corpo
        for (int i = 1; i < Snake.Count; i++)
        {
            if (newHead.X == Snake[i].X && newHead.Y == Snake[i].Y)
            {
                if (HasShield)
                {
                    HasShield = false;
                    ShieldDuration = 0;
                    
                    // Mover para uma posição segura
                    do
                    {
                        newHead = Snake[0].Clone();
                    } while (!IsValidPosition(newHead.X, newHead.Y));
                }
                else
                {
                    State = GameState.GameOver;
                    return;
                }
                break;
            }
        }
        
        // Atualizar a cobra
        Snake.Insert(0, newHead);
        if (Snake.Count > SnakeLength)
        {
            Snake.RemoveAt(Snake.Count - 1);
        }
    }
}