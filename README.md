\# PokemonAPI



API REST en .NET para una Pokedex con gestión de Pokémon, movimientos, mis Pokémon y simulación de combate por turnos.



\## Objetivo

Este proyecto implementa los requisitos de:



\- API tipo Pokedex con CRUDs y consultas relacionales.

\- API de combate que mantiene estado de partida y permite ejecutar fases del combate hasta que un equipo quede sin PS.



\----------------------------------------------------------------



\## Tecnologías

\- .NET 10

\- ASP.NET Core Web API

\- Entity Framework Core

\- SQL Server

\- OpenAPI



\----------------------------------------------------------------



\## Estructura funcional



\### Pokedex

Recursos implementados:



1\. Pokémon base (CRUD)

2\. Movimientos (CRUD)

3\. Mis Pokémon con sus 4 movimientos (CRUD)

4\. Consulta de movimientos de un Pokémon

5\. Consulta de movimientos posibles de un Pokémon

6\. Consulta de Pokémon que comparten un mismo movimiento



\### Combate

Funcionalidades implementadas:



1\. Crear combate con equipo jugador vs enemigos

2\. Mantener estado en memoria por 'combateId'

3\. Ejecutar turnos individuales

4\. Finalizar cuando los PS de un equipo llegan a 0

5\. Simular combate completo

6\. Consultar historial completo del combate



\----------------------------------------------------------------



\## Modelo de datos (resumen)

\- 'Pokemon': catálogo base de Pokémons

\- 'Movimiento': catálogo de movimientos (tipo, poder, categoría)

\- 'PokemonTipo': tipos del Pokémon base (1 o 2)

\- 'PokemonMovimiento': movimientos reales del Pokémon base

\- 'PokemonMovimientoPosible': movimientos posibles del Pokémon base

\- 'MiPokemon': pokemons del jugador

\- 'MiPokemonMovimiento': movimientos por slot (1..4) de cada 'MiPokemon'



\----------------------------------------------------------------



\## Reglas de negocio destacadas

\- Pokémon base: 1 o 2 tipos, sin repetir.

\- 'MiPokemon': nivel válido y salud actual no mayor que salud base.

\- 'MiPokemonMovimiento': slots 1..4.

\- Movimiento:

&#x20; - 'Estado' => 'Poder = 0'

&#x20; - 'Fisico' o 'Especial' => 'Poder > 0'

\- No duplicar nombre de Pokémon ni de movimiento.

\- No repetir el mismo movimiento en dos slots del mismo 'MiPokemon'.



\----------------------------------------------------------------



\## Endpoints principales



\### Pokémon base

\- 'GET /api/pokemons'

\- 'GET /api/pokemons/{id}'

\- 'POST /api/pokemons'

\- 'PUT /api/pokemons/{id}'

\- 'DELETE /api/pokemons/{id}'



Movimientos relacionados:

\- 'GET /api/pokemons/{id}/movimientos'

\- 'PUT /api/pokemons/{id}/movimientos'

\- 'PATCH /api/pokemons/{id}/movimientos'

\- 'GET /api/pokemons/{id}/movimientos-posibles'

\- 'PUT /api/pokemons/{id}/movimientos-posibles'

\- 'PATCH /api/pokemons/{id}/movimientos-posibles'



\### Movimientos

\- 'GET /api/movimientos'

\- 'GET /api/movimientos/{id}'

\- 'POST /api/movimientos'

\- 'PUT /api/movimientos/{id}'

\- 'DELETE /api/movimientos/{id}'



Consultas:

\- 'GET /api/movimientos/{id}/pokemons'

\- 'GET /api/movimientos/{id}/pokemons-posibles'



\### Mis Pokémon

\- 'GET /api/mispokemons'

\- 'GET /api/mispokemons/{id}'

\- 'POST /api/mispokemons'

\- 'PUT /api/mispokemons/{id}'

\- 'DELETE /api/mispokemons/{id}'

\- 'GET /api/mispokemons/{id}/movimientos-posibles'



Slots de movimientos:

\- 'GET /api/mispokemons/{miPokemonId}/movimientos'

\- 'PUT /api/mispokemons/{miPokemonId}/movimientos/{slot}'

\- 'PATCH /api/mispokemons/{miPokemonId}/movimientos' (lote)

\- 'DELETE /api/mispokemons/{miPokemonId}/movimientos/{slot}'



\### Combates

\- 'POST /api/combates'

\- 'GET /api/combates/{id}'

\- 'POST /api/combates/{id}/turno'

\- 'POST /api/combates/{id}/simular?maxTurnos=100'

\- 'GET /api/combates/{id}/historial'

\- 'DELETE /api/combates/{id}'



\----------------------------------------------------------------



\## Ejecución en local



\### 1) Configurar conexión

En 'appsettings.json':



json:

"ConnectionStrings": {

&#x20; "PokemonDB": "Server=localhost;Database=PokemonDB;Trusted\_Connection=True;TrustServerCertificate=True;"

}



\### 2) Aplicar migraciones



Consola de Administrador de Paquetes: dotnet ef database update
Usar el seed.sql dejado en /docs en la bbdd para cargar los casos de prueba


\### 3) Ejecutar API



Darle a la flechita arriba para iniciar la API



\### 4) Probar endpoints



OpenAPI en entorno Development

Archivo .http incluido con casos de prueba


\## Archivo de pruebas HTTP



Se incluye un archivo .http con flujos de prueba para:



CRUD Pokémon base

CRUD Movimientos

CRUD MisPokémon

Gestión de slots y movimientos

Consultas obligatorias

Flujo completo de combate (turnos y simulación)



\## Estado de persistencia del combate

El estado de combate se mantiene en memoria (CombateService).



Si se reinicia la API, se pierden los combates activos.

Esto es intencional para la práctica.



\## Mejoras futuras



Frontend integrado (Blazor).

Tests automatizados de integración.

Soporte avanzado de efectos, precisión y PP por movimiento.





\----------------------------------------------------------------





\## Justificación de ampliaciones

Aunque el enunciado pedía un mínimo funcional, se implementaron mejoras adicionales para aumentar la calidad técnica, la robustez de la API y la capacidad de demostración:



1. Gestión parcial de relaciones (PATCH)
* Se añadieron endpoints PATCH para modificar movimientos reales y posibles sin reemplazar listas completas.
* Justificación: mejora la eficiencia y evita sobreescrituras innecesarias en clientes reales.

2. Gestión de movimientos por slots en MisPokemons

* Se añadió asignación por slot (1..4), borrado por slot y actualización por lote.
* Justificación: representa mejor la mecánica real de Pokémon y refuerza reglas de negocio.

3. Categoría de movimiento (Físico, Especial, Estado)

* Se amplió el modelo de Movimiento para diferenciar categoría.
* Justificación: permite cálculo de daño más correcto (Ataque/Defensa vs AtaqueEspecial/DefensaEspecial).

4. Validaciones de negocio adicionales

* Restricciones de salud, nivel, tipos, duplicados, nombres únicos y consistencia de categorías.
* Justificación: evita datos inválidos y mejora la integridad funcional.

5. Simulación completa de combate e historial

* Además del turno a turno, se añadió simulación automática hasta fin de combate y consulta de historial.
* Justificación: facilita pruebas, debugging y demostración completa de la parte de combate.

6. Documentación XML de endpoints

* Se documentaron controladores para mejorar trazabilidad y uso desde OpenAPI.
* Justificación: mayor mantenibilidad y mejor experiencia para evaluación y consumo de API.

7. Estado de combate en memoria con control concurrente

* Se mantuvo estado por combate y se añadieron mecanismos para evitar problemas de acceso concurrente.
* Justificación: mejora estabilidad sin complejidad extra de persistencia de combates.

