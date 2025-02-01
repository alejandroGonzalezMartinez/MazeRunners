using System;

public class Character
{
    public string Name { get; } // Nombre
    public string Description { get; } // Descripcion
    public int StartSpeed { get; set; } // Velocidad inicial
    public int Speed { get; set; } // Velocidad
    public Action<Character> Ability { get; } // Habilidad
    public int Cooldown { get; private set; } // Tiempo de enfriamiento
    public int CooldownRemaining { get; set; } // Tiempo restante de enfriamiento
    public (int X, int Y) Position { get; set; } // Posición en el tablero
    public (int X, int Y) StartPosition { get; set; } // Posición inicial en el tablero
    public bool SkipTurn {get; set; } // Un manejador para saber si debe omitirse el turno

    public bool IsImmune { get; private set; } // Indica si el personaje es inmune
    public int ImmunityTurnsRemaining { get; private set; } // Turnos restantes de inmunidad

    public bool IsRevealingTraps { get; private set; } // Indica si está revelando trampas (explorador)
    public int RevealTrapsTurnsRemaining { get; private set; } // Turnos restantes para revelar trampas (explorador)

    public bool HasPortal { get; set; } // Indica si tiene el portal colocado (mago)
    public (int, int) PortalPosition { get; set; } // Ubicacion del portal (mago)

    // Constructor que acepta una habilidad como parámetro
    public Character(string name, string description, int startSpeed, Action<Character> ability, int cooldown)
    {
        Name = name;
        Description = description;
        StartSpeed = startSpeed;
        Speed = startSpeed;
        Ability = ability;
        Cooldown = cooldown;
        CooldownRemaining = 0;
        SkipTurn = false;
    }

    // Método para activar la revelación de trampas (explorador)
    public void ActivateRevealTraps(int turns)
    {
        IsRevealingTraps = true;
        RevealTrapsTurnsRemaining = turns;
    }

    // Método para reducir el contador de revelación de trampas al final del turno (explorador)
    public void ReduceRevealTraps()
    {
        if (RevealTrapsTurnsRemaining > 0)
        {
            RevealTrapsTurnsRemaining--;
            if (RevealTrapsTurnsRemaining == 0)
            {
                IsRevealingTraps = false;
            }
        }
    }

    // Método para activar la inmunidad
    public void ActivateImmunity(int turns)
    {
        IsImmune = true;
        ImmunityTurnsRemaining = turns;
    }

    // Método para reducir la inmunidad al final del turno
    public void ReduceImmunity()
    {
        if (ImmunityTurnsRemaining > 0)
        {
            ImmunityTurnsRemaining--;
            if (ImmunityTurnsRemaining == 0)
            {
                IsImmune = false;
            }
        }
    }

    // Usar la habilidad especial
    public void UseAbility(Character player)
    {
        if (CooldownRemaining > 0)
        {
            Console.WriteLine($"{Name} no puede usar la habilidad. Debes esperar: {CooldownRemaining} turnos(s).");
            return;
        }

        Console.WriteLine($"{Name} usa la habilidad!");
        Ability?.Invoke(player); // Ejecutar la habilidad
        CooldownRemaining = Cooldown; // Reiniciar el enfriamiento
    }

    // Reducir el enfriamiento al final de un turno
    public void ReduceCooldown()
    {
        if (CooldownRemaining > 0)
        {
            CooldownRemaining--;
        }
    }
}
