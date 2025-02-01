using System;
using System.Collections.Generic;
using System.Reflection;

class MazeGenerator
{
    public int[,] Board; // Tablero del laberinto
    private int Size; // Tamaño del tablero
    public (int, int) Exit; // Posicion de la salida
    private Random rand = new Random(); // Generador de números aleatorios

    public MazeGenerator(int size)
    {
        Size = size;
        Board = new int[Size, Size];
    }

    // Generar el laberinto
    public void GenerateMaze()
    {
        // Inicializar el tablero lleno de paredes
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                Board[x, y] = 1;
            }
        }

        // Elegir un borde aleatorio para la salida
        int edge = rand.Next(4);
        int startX = 0, startY = 0;

        // Determinar la posición de la salida dependiendo del borde elegido, evitando las esquinas
        switch (edge)
        {
            case 0: // Borde izquierdo
                startX = rand.Next(1, Size - 1);
                startY = 0;
                break;

            case 1: // Borde derecho
                startX = rand.Next(1, Size - 1);
                startY = Size - 1;
                break;

            case 2: // Borde superior
                startX = 0;
                startY = rand.Next(1, Size - 1);
                break;

            case 3: // Borde inferior
                startX = Size - 1;
                startY = rand.Next(1, Size - 1);
                break;
        };

        Exit = (startX, startY);
        Board[startX, startY] = 0; // La salida es un camino

        // Lista de paredes adyacentes
        List<(int, int)> walls = new();
        AddWalls(startX, startY, walls);

        // Procesar las paredes hasta que la lista esté vacía
        while (walls.Count > 0)
        {
            // Seleccionar una pared aleatoria
            int wallIndex = rand.Next(walls.Count);
            var wall = walls[wallIndex];
            walls.RemoveAt(wallIndex);

            int x = wall.Item1;
            int y = wall.Item2;

            // Comprobar si la pared puede convertirse en camino
            if (CanBePath(x, y))
            {
                Board[x, y] = 0; // Convertir en camino

                // Conectar con una celda vecina
                foreach (var neighbor in GetNeighbors(x, y))
                {
                    int nx = neighbor.Item1;
                    int ny = neighbor.Item2;

                    if (Board[nx, ny] == 0) // Conectar
                    {
                        Board[(x + nx) / 2, (y + ny) / 2] = 0;
                    }
                }

                // Añadir paredes adyacentes a la lista
                AddWalls(x, y, walls);
            }
        }
    }

    // Agrega las paredes adyacentes de una celda
    private void AddWalls(int x, int y, List<(int, int)> walls)
    {
        foreach (var neighbor in GetNeighbors(x, y))
        {
            int nx = neighbor.Item1;
            int ny = neighbor.Item2;

            if (Board[nx, ny] == 1) // Solo paredes
            {
                walls.Add((nx, ny));
            }
        }
    }

    // Devuelve los vecinos en un radio dado
    private IEnumerable<(int, int)> GetNeighbors(int x, int y)
    {
        int[,] directions = { { -2, 0 }, { 2, 0 }, { 0, -2 }, { 0, 2 } };

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int nx = x + directions[i, 0];
            int ny = y + directions[i, 1];

            if (nx >= 0 && nx < Size && ny >= 0 && ny < Size)
            {
                yield return (nx, ny);
            }
        }
    }

    // Verifica si una celda puede convertirse en camino
    private bool CanBePath(int x, int y)
    {
        // Solo puede haber una salida
        if (x == 0 || x == Size - 1 || y == 0 || y == Size - 1) return false;

        int pathCount = 0;
        foreach (var neighbor in GetNeighbors(x, y))
        {
            int nx = neighbor.Item1;
            int ny = neighbor.Item2;

            if (Board[nx, ny] == 0)
            {
                pathCount++;
            }
        }
        return pathCount == 1; // Debe estar conectado a exactamente un camino
    }
}
