# Maze Runners

Maze Runners es un juego de escape por turnos multijugador desarrollado en **C#** utilizando **Spectre.Console**. Los jugadores deben moverse por un tablero **32x32** generado proceduralmente, evitando obstáculos y trampas, mientras usan habilidades únicas para alcanzar la salida.

## Características

- **Juego por turnos** con un solo personaje por jugador.
- **Tablero 32x32 generado proceduralmente** con obstáculos y trampas.
- **5 personajes jugables**, cada uno con habilidades especiales.
- **Sistema de trampas** invisibles que afectan al jugador cuando las pisa.
- **Interfaz en consola** utilizando la librería **Spectre.Console**.

## Instalación y Uso

1. Clona el repositorio:
   ```sh
   git clone https://github.com/alejandroGonzalezMartinez/MazeRunners.git
   cd MazeRunners
   ```
2. Asegúrate de tener instalado **.NET SDK**.
3. Compila y ejecuta el proyecto:
   ```sh
   dotnet run
   ```

## Personajes y Habilidades

1. **El Explorador** - Revela las trampas en un radio de 2 casillas durante 2 turnos y es inmune a ellas durante los siguientes 3 turnos. Velocidad: **4**.
2. **El Mago** - Coloca un portal en una casilla y luego se teletransporta a él desde cualquier lugar del mapa. Velocidad: **3**.
3. **El Berserker** - Aumenta su velocidad en 1 cada vez que cae en una trampa. Es rápido y letal. Velocidad: **2**.
4. **El Sanador** - Restaura un punto de su velocidad si ha sido reducida por una trampa. Velocidad: **4**.
5. **El Embaucador** - Coloca una trampa en su posición, pero es tonto y nunca recuerda donde la dejo. Velocidad: **4**.
