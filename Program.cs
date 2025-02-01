using Spectre.Console;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

class Program
{
    static void DisplayMaze(int[,] maze, Character[] players)
    {
        // Crear el contenido del laberinto
        var mazeBuilder = new StringBuilder();

        for (int i = 0; i < maze.GetLength(0); i++)
        {
            for (int j = 0; j < maze.GetLength(1); j++)
            {
                // Determinar si hay un jugador en la celda
                var playerInCell = players[0].Position == (i, j) ? "[bold green]J1[/]" :
                                   players[1].Position == (i, j) ? "[bold blue]J2[/]" : null;

                // Construir el carácter de la celda
                if (playerInCell != null)
                {
                    mazeBuilder.Append(playerInCell);
                }
                else if (maze[i, j] == 1) // Obstáculo
                {
                    mazeBuilder.Append("[red]██[/]");
                }
                else if (maze[i, j] == -1 || maze[i, j] == -2 || maze[i, j] == -3)
                {
                    mazeBuilder.Append("[yellow]TT[/]");
                }
                else // Camino
                {
                    mazeBuilder.Append("[white]  [/]");
                }
            }
            mazeBuilder.AppendLine(); // Nueva línea para la siguiente fila
        }

        // Crear un panel con el contenido del laberinto
        var panel = new Panel(mazeBuilder.ToString())
        {
            Border = BoxBorder.Double,
            Header = new PanelHeader("MazeRunners"),
        };

        // Mostrar el panel
        AnsiConsole.Clear();
        AnsiConsole.Write(panel);
    }

    static void DisplayMazeLimited(int[,] maze, Character[] players, Character currentPlayer, int viewSize)
    {
        var mazeBuilder = new StringBuilder();
        int halfView = viewSize / 2;

        int size = maze.GetLength(0);

        // Calcular los límites de la vista centrada en el jugador
        int minX = Math.Max(0, currentPlayer.Position.X - halfView);
        int maxX = Math.Min(size - 1, currentPlayer.Position.X + halfView);
        int minY = Math.Max(0, currentPlayer.Position.Y - halfView);
        int maxY = Math.Min(size - 1, currentPlayer.Position.Y + halfView);

        for (int i = minX; i <= maxX; i++)
        {
            for (int j = minY; j <= maxY; j++)
            {
                // Determinar si hay un jugador en la celda
                var playerInCell = players[0].Position == (i, j) ? "[bold green]J1[/]" :
                                   players[1].Position == (i, j) ? "[bold blue]J2[/]" : null;

                // Construir el carácter de la celda
                if (playerInCell != null)
                {
                    mazeBuilder.Append(playerInCell);
                }
                else if (maze[i, j] == 1) // Obstáculo
                {
                    mazeBuilder.Append("[red]██[/]");
                } else if (maze[i, j] == -1 || maze[i, j] == -2 || maze[i, j] == -3) // Trampa
                {
                    if (currentPlayer.IsRevealingTraps && Math.Abs(currentPlayer.Position.X - i) <= 2 && Math.Abs(currentPlayer.Position.Y - j) <= 2)
                    {
                        mazeBuilder.Append("[yellow]TT[/]"); // Representación visual de una trampa revelada
                    } else
                    {
                        mazeBuilder.Append("[white]  [/]"); // Camino sin revelar
                    }
                }
                else // Camino
                {
                    mazeBuilder.Append("[white]  [/]"); // Casilla libre
                }
            }
            mazeBuilder.AppendLine(); // Nueva línea para la siguiente fila
        }

        // Crear un panel con el contenido del laberinto
        var panel = new Panel(mazeBuilder.ToString())
        {
            Border = BoxBorder.Double,
            Header = new PanelHeader("MazeRunners"),
        };

        // Mostrar el panel
        AnsiConsole.Clear();
        AnsiConsole.Write(panel);
    }

    static ((int, int) Player1, (int, int) Player2) GetRandomStartPosition(int[,] maze)
    {
        var random = new Random();
        (int, int) player1;
        (int, int) player2;

        do
        {
            player1 = (random.Next(maze.GetLength(0)), random.Next(maze.GetLength(1)));
        } while (maze[player1.Item1, player1.Item2] != 0);

        do
        {
            player2 = (random.Next(maze.GetLength(0)), random.Next(maze.GetLength(1)));
        } while (maze[player2.Item1, player2.Item2] != 0 || player2 == player1);

        return (player1, player2);
    }

    static void MovePlayer(int[,] maze, (int, int) exit, Character player, Character[] players)
    {
        Console.WriteLine($"{player.Name}, es tu turno!");
        Console.WriteLine($"Puedes moverte {player.Speed} pasos en este turno.");

        for (int i = 0; i < player.Speed; i++)
        {
            Console.WriteLine($"Paso {i + 1}/{player.Speed}: Usa W (arriba), A (izquierda), S (abajo), D (derecha), o presiona H para usar tu habilidad:");
            char input = Console.ReadKey(true).KeyChar;

            // Opción para activar habilidad
            if (input == 'h' || input == 'H')
            {
                if (player.CooldownRemaining > 0)
                {
                    Console.WriteLine($"La habilidad de {player.Name} está en enfriamiento por {player.CooldownRemaining} turno(s).");
                }
                else
                {
                    player.UseAbility(player); // Ejecutar la habilidad
                    DisplayMazeLimited(maze, players, player, 10);
                }
                i--; // No cuenta como un movimiento
                continue;
            }

            // Direcciones posibles
            var directions = new Dictionary<char, (int X, int Y)>
            {
                { 'w', (-1, 0) }, // Arriba
                { 's', (1, 0) },  // Abajo
                { 'a', (0, -1) }, // Izquierda
                { 'd', (0, 1) },   // Derecha
                 { 'W', (-1, 0) }, // Arriba
                { 'S', (1, 0) },  // Abajo
                { 'A', (0, -1) }, // Izquierda
                { 'D', (0, 1) }   // Derecha
            };

            // Validar el input
            if (!directions.ContainsKey(input))
            {
                i--; // No cuenta el movimiento inválido
                continue;
            }

            // Calcular la nueva posición
            (int X, int Y) = directions[input];
            (int, int) newPosition = (player.Position.X + X, player.Position.Y + Y);

            // Verificar si el movimiento es válido
            if (IsValidMove(maze, newPosition))
            {
                player.Position = newPosition;
                if (CheckVictory(player, exit)) break;
                DisplayMazeLimited(maze, players, player, 10); // Mostrar el laberinto después de cada paso
                if(CheckForTrap(maze, player)) break;
            }
            else
            {
                DisplayMazeLimited(maze, players, player, 10);
                i--; // No cuenta el movimiento inválido
            }
        }
    }

    static bool IsValidMove(int[,] maze, (int X, int Y) position)
    {
        int size = maze.GetLength(0);
        return position.X >= 0 && position.X < size &&
            position.Y >= 0 && position.Y < size &&
            maze[position.X, position.Y] != 1; // No obstáculos
    }

    static void PlaceTraps(int[,] maze, int numberOfTraps)
    {
        var random = new Random();
        int placedTraps = 0;

        while (placedTraps < numberOfTraps)
        {
            int x = random.Next(maze.GetLength(0));
            int y = random.Next(maze.GetLength(1));

            // Solo colocar trampas en caminos válidos (no obstáculos ni salida)
            if (maze[x, y] == 0)
            {
                maze[x, y] = random.Next(-3, 0); // Asignar un tipo de trampa (-1, -2, -3)
                placedTraps++;
            }
        }
    }

    static bool CheckForTrap(int[,] maze, Character player)
    {
        if(player.IsImmune) return false; 

        int trapType = maze[player.Position.X, player.Position.Y];

        maze[player.Position.X, player.Position.Y] = 0; // Una vez activada, la trampa se elimina

        if (player.Name == "Berserker" && (trapType == -1 || trapType == -2 || trapType == -3))
        {
            player.Speed++;
            Console.WriteLine($"{player.Name} ha caido en una trampa. Enfurece y su velocidad aumenta!");
            Console.WriteLine("Presiona Enter para continuar...");
            Console.ReadLine();
            return false;
        }

        switch (trapType)
        {
            case -1: // Volver a la casilla inicial
                Console.WriteLine($"{player.Name} cayó en una trampa y regresa a la casilla inicial.");
                Console.WriteLine("Presiona Enter para continuar...");
                Console.ReadLine();
                player.Position = player.StartPosition;
                return true;

            case -2: // Perder velocidad
                Console.WriteLine($"{player.Name} cayó en una trampa y pierde 1 de velocidad.");
                Console.WriteLine("Presiona Enter para continuar...");
                Console.ReadLine();
                player.Speed = Math.Max(1, player.Speed - 1); // Velocidad mínima de 1
                return true;

            case -3: // Perder un turno
                Console.WriteLine($"{player.Name} cayó en una trampa y perdera el proximo turno.");
                Console.WriteLine("Presiona Enter para continuar...");
                Console.ReadLine();
                player.SkipTurn = true;
                return true;

            default:
                return false; // No hay trampa en esta casilla
        }
    }

    static bool CheckVictory(Character player, (int X, int Y) exit)
    {
        if (player.Position == exit)
        {
            Console.WriteLine($"{player.Name} ha llegado a la salida y es el ganador. ¡Felicidades!");
            return true;
        }
        return false;
    }

    static void GameLoop(int[,] maze, (int, int) exit, Character[] players)
    {
        bool gameRunning = true;
        int currentPlayer = 0;

        while (gameRunning)
        {
            DisplayMazeLimited(maze, players, players[currentPlayer], 10);

            if (players[currentPlayer].SkipTurn)
            {
                Console.WriteLine($"{players[currentPlayer].Name} pierde este turno.");
                Console.WriteLine("Presiona Enter continuar...");
                Console.ReadLine();
                players[currentPlayer].SkipTurn = false;
                currentPlayer = (currentPlayer + 1) % players.Length;
                continue;
            }

            MovePlayer(maze, exit, players[currentPlayer], players);

            if (CheckVictory(players[currentPlayer], exit))
            {
                DisplayMaze(maze, players);
                Console.WriteLine($"{players[currentPlayer].Name} ha llegado a la salida y es el ganador. ¡Felicidades!");

                gameRunning = false;
                continue;
            }

            DisplayMazeLimited(maze, players, players[currentPlayer], 10);

            // Pausar hasta que el jugador presione Enter para cambiar el turno
            Console.WriteLine("Presiona Enter para finalizar tu turno...");
            Console.ReadLine();

            // Reducir cooldowns al finalizar el turno
            players[currentPlayer].ReduceCooldown();
            players[currentPlayer].ReduceImmunity();
            players[currentPlayer].ReduceRevealTraps();

            // Cambiar de jugador
            currentPlayer = (currentPlayer + 1) % players.Length;
        }
    }

    static void Main(string[] args)
    {
        // Mensaje de bienvenida
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold yellow]¡Bienvenido a MazeRunners![/]");
        AnsiConsole.MarkupLine("Dos jugadores competirán para escapar del laberinto.");
        AnsiConsole.WriteLine();

        int size = 32;
        MazeGenerator mazeGenerator = new MazeGenerator(size);
        mazeGenerator.GenerateMaze();
        int [,] maze = mazeGenerator.Board;
        (int, int) exitPosition = mazeGenerator.Exit;

        PlaceTraps(maze, 10); // Inicializando 10 trampas en el laberinto

        var explorer = new Character(
            "Explorador",
            "Revela las trampas en un radio de 2 casillas durante 2 turnos.",
            4,
            (player) => {
                // Activar la revelación de trampas y la inmunidad
                player.ActivateRevealTraps(2); // Revela trampas durante 2 turnos
                player.ActivateImmunity(3); // Inmunidad durante 3 turnos

                Console.WriteLine($"{player.Name} ahora es inmune a las trampas durante 3 turnos y puede ver trampas cercanas durante 2 turnos.");
                Console.WriteLine("Presiona Enter para continuar...");
                Console.ReadLine();
            },
            4
        );

        var mage = new Character(
            "Mago",
            "Coloca un portal en una casilla y luego se teletransporta a él desde cualquier lugar del mapa.",
            3,
            (player) =>
            {
                if (!player.HasPortal)
                {
                    player.PortalPosition = player.Position;
                    player.HasPortal = true;
                    Console.WriteLine("Portal colocado.");
                    Console.WriteLine("Presiona Enter para continuar...");
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("¿Quieres teletransportarte al portal? (s/n)");
                    char response = Console.ReadKey(true).KeyChar;
                    if (response == 's' || response == 'S')
                    {
                        player.Position = player.PortalPosition;
                    }
                }
            },
            0
        );

        var berserker = new Character(
            "Berserker",
            "Aumenta su velocidad en 1 cada vez que cae en una trampa. Es rápido y letal.",
            2, 
            (player) => {
                Console.WriteLine("La habilidad de Berserker es pasiva!");
                Console.WriteLine("Presiona Enter para continuar...");
                Console.ReadLine();
            },
            0
        );

        var healer = new Character(
            "Sanador",
            "Restaura un punto de su velocidad si ha sido reducida por una trampa.",
            4,
            (player) =>
            {   
                // Restaurar la velocidad del propio sanador
                if (player.Speed < player.StartSpeed)
                {
                    player.Speed++;
                    Console.WriteLine($"{player.Name} se ha sanado.");
                }

                // Sanar jugadores en la misma casilla
                /* var otherPlayers = players.Where(p => p != player && p.Position == player.Position);
                foreach (var otherPlayer in otherPlayers)
                {
                    if (otherPlayer.Speed < otherPlayer.BaseSpeed)
                    {
                        otherPlayer.Speed++;
                        Console.WriteLine($"{player.Name} ha restaurado la velocidad de {otherPlayer.Name}.");
                    }
                } */

                Console.WriteLine("Presiona Enter para continuar...");
                Console.ReadLine();
            },
            4
        );

        var trickster = new Character(
            "Embaucador",
            "Coloca una trampa en su posición, pero es tonto y nunca recuerda donde la dejo.",
            4, // Velocidad base
            (player) =>
            {   
                // Generar una trampa aleatoria
                var random = new Random();
                int trapType = random.Next(-3, 0); // Tipos de trampas

                // Registrar la trampa en la posición actual
                (int X, int Y) = player.Position;
                maze[X, Y] = trapType; // Colocar la trampa en el laberinto

                Console.WriteLine($"{player.Name} ha colocado una trampa.");
                Console.WriteLine("Presiona Enter para continuar...");
                Console.ReadLine();
            },
            5
        );

        // Personajes disponibles
        Character[] characters = [ explorer, mage, berserker, healer, trickster ];

        // Arreglo para guardar los personajes elegidos
        Character[] selectedCharacters = new Character[2];

        // Ciclo para que cada jugador elija un personaje
        for (int i = 0; i < 2; i++)
        {
            AnsiConsole.MarkupLine($"[bold green]Jugador {i + 1}, elige tu personaje:[/]");

            List<string> selectableCharacters = [];

            // Evitamos que el jugador 2 pueda elegir el mismo personaje que el jugador 1
            for (int j = 0; j < 5; j++)
            {
                if (i > 0 && characters[j].Name == selectedCharacters[0].Name) continue;
                selectableCharacters.Add(characters[j].Name);
            }

            // Mostrar opciones de personajes
            var options = new SelectionPrompt<string>()
                .Title("")
                .PageSize(5)
                .AddChoices([.. selectableCharacters]);

            string choice = AnsiConsole.Prompt(options);

            // Asignar el personaje elegido al jugador actual
            selectedCharacters[i] = characters.First(c => c.Name == choice);

            AnsiConsole.MarkupLine($"[bold cyan]Jugador {i + 1} eligió: {selectedCharacters[i].Name}[/]");
            AnsiConsole.WriteLine();
        }

        // Mostrar los personajes elegidos
        AnsiConsole.MarkupLine("[bold yellow]¡Los jugadores han elegido sus personajes![/]");
        for (int i = 0; i < selectedCharacters.Length; i++)
        {
            AnsiConsole.MarkupLine($"[bold green]Jugador {i + 1}:[/] {selectedCharacters[i].Name} - {selectedCharacters[i].Description}");
        }

        Console.WriteLine("\nPresiona Enter continuar...");
        Console.ReadLine();

        // Configurar posiciones iniciales
        (selectedCharacters[0].StartPosition, selectedCharacters[1].StartPosition) = GetRandomStartPosition(maze);
        selectedCharacters[0].Position = selectedCharacters[0].StartPosition;
        selectedCharacters[1].Position = selectedCharacters[1].StartPosition;

        GameLoop(maze, exitPosition, selectedCharacters);
    }
}
