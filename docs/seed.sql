USE [PokemonDB];
GO

SET XACT_ABORT ON;
BEGIN TRAN;

------------------------------------------------------------
-- 1) MOVIMIENTOS (upsert por Nombre)
-- Tipo enum:
-- 0 Acero,1 Agua,2 Bicho,3 Dragon,4 Electrico,5 Fantasma,6 Fuego,7 Hada,
-- 8 Hielo,9 Lucha,10 Normal,11 Planta,12 Psiquico,13 Roca,14 Siniestro,15 Tierra,16 Veneno,17 Volador
--
-- Categoria enum:
-- 0 Fisico, 1 Especial, 2 Estado
------------------------------------------------------------
;WITH src (Nombre, Tipo, Poder, Categoria) AS (
    SELECT * FROM (VALUES
        (N'Arañazo', 10, 40, 0),
        (N'Tinieblas', 5, 1, 1),
        (N'Impresionar', 5, 30, 0),
        (N'Golpes Furia', 10, 18, 0),
        (N'Sorpresa', 10, 40, 0),
        (N'Finta', 14, 60, 0),
        (N'Desarme', 14, 65, 0),
        (N'Bola Sombra', 5, 80, 1),
        (N'Impactrueno', 4, 40, 1),
        (N'Ataque Rapido', 10, 40, 0),
        (N'Portazo', 10, 80, 0),
        (N'Rayo', 4, 90, 1),
        (N'Trueno', 4, 110, 1),
        (N'Ascuas', 6, 40, 1),
        (N'Furia', 10, 20, 0),
        (N'Lanzallamas', 6, 90, 1),
        (N'Ataque Ala', 17, 60, 0),
        (N'Cuchillada', 10, 70, 0),
        (N'Furia Dragon', 3, 1, 1),
        (N'Giro Fuego', 6, 35, 1),
        (N'Derribo', 10, 90, 0),
        (N'Confusion', 12, 50, 1),
        (N'Garra Metal', 0, 50, 0),
        (N'Persecucion', 14, 40, 0),
        (N'Psiquico', 12, 90, 1),
        (N'Puño Meteoro', 0, 90, 0),
        (N'Hiperrayo', 10, 150, 1),
        (N'Placaje', 10, 40, 0),
        (N'Burbuja', 1, 40, 1),
        (N'Pistola Agua', 1, 40, 1),
        (N'Mordisco', 14, 60, 0),
        (N'Giro Rapido', 10, 20, 0),
        (N'Cabezazo', 10, 130, 0),
        (N'Hidrobomba', 1, 110, 1)
    ) v(Nombre, Tipo, Poder, Categoria)
)
UPDATE m
SET m.Tipo = s.Tipo, m.Poder = s.Poder, m.Categoria = s.Categoria
FROM dbo.Movimientos m
JOIN src s ON s.Nombre = m.Nombre;

;WITH src (Nombre, Tipo, Poder, Categoria) AS (
    SELECT * FROM (VALUES
        (N'Arañazo', 10, 40, 0),
        (N'Tinieblas', 5, 1, 1),
        (N'Impresionar', 5, 30, 0),
        (N'Golpes Furia', 10, 18, 0),
        (N'Sorpresa', 10, 40, 0),
        (N'Finta', 14, 60, 0),
        (N'Desarme', 14, 65, 0),
        (N'Bola Sombra', 5, 80, 1),
        (N'Impactrueno', 4, 40, 1),
        (N'Ataque Rapido', 10, 40, 0),
        (N'Portazo', 10, 80, 0),
        (N'Rayo', 4, 90, 1),
        (N'Trueno', 4, 110, 1),
        (N'Ascuas', 6, 40, 1),
        (N'Furia', 10, 20, 0),
        (N'Lanzallamas', 6, 90, 1),
        (N'Ataque Ala', 17, 60, 0),
        (N'Cuchillada', 10, 70, 0),
        (N'Furia Dragon', 3, 1, 1),
        (N'Giro Fuego', 6, 35, 1),
        (N'Derribo', 10, 90, 0),
        (N'Confusion', 12, 50, 1),
        (N'Garra Metal', 0, 50, 0),
        (N'Persecucion', 14, 40, 0),
        (N'Psiquico', 12, 90, 1),
        (N'Puño Meteoro', 0, 90, 0),
        (N'Hiperrayo', 10, 150, 1),
        (N'Placaje', 10, 40, 0),
        (N'Burbuja', 1, 40, 1),
        (N'Pistola Agua', 1, 40, 1),
        (N'Mordisco', 14, 60, 0),
        (N'Giro Rapido', 10, 20, 0),
        (N'Cabezazo', 10, 130, 0),
        (N'Hidrobomba', 1, 110, 1)
    ) v(Nombre, Tipo, Poder, Categoria)
)
INSERT INTO dbo.Movimientos (Nombre, Tipo, Poder, Categoria)
SELECT s.Nombre, s.Tipo, s.Poder, s.Categoria
FROM src s
WHERE NOT EXISTS (SELECT 1 FROM dbo.Movimientos m WHERE m.Nombre = s.Nombre);

------------------------------------------------------------
-- 2) POKEMONS BASE (upsert por Nombre)
------------------------------------------------------------
;WITH src (Nombre, SaludTotalBase, AtaqueBase, DefensaBase, AtaqueEspecialBase, DefensaEspecialBase, VelocidadBase) AS (
    SELECT * FROM (VALUES
        (N'Sableye', 125, 95, 95, 85, 85, 70),
        (N'Pikachu', 110, 75, 60, 70, 70, 110),
        (N'Charizard', 153, 104, 98, 129, 105, 120),
        (N'Blastoise', 154, 103, 120, 105, 125, 98),
        (N'Metagross', 155, 187, 150, 122, 110, 90),
        (N'Feraligatr', 160, 145, 120, 99, 103, 98)
    ) v(Nombre, SaludTotalBase, AtaqueBase, DefensaBase, AtaqueEspecialBase, DefensaEspecialBase, VelocidadBase)
)
UPDATE p
SET p.SaludTotalBase = s.SaludTotalBase,
    p.AtaqueBase = s.AtaqueBase,
    p.DefensaBase = s.DefensaBase,
    p.AtaqueEspecialBase = s.AtaqueEspecialBase,
    p.DefensaEspecialBase = s.DefensaEspecialBase,
    p.VelocidadBase = s.VelocidadBase
FROM dbo.Pokemons p
JOIN src s ON s.Nombre = p.Nombre;

;WITH src (Nombre, SaludTotalBase, AtaqueBase, DefensaBase, AtaqueEspecialBase, DefensaEspecialBase, VelocidadBase) AS (
    SELECT * FROM (VALUES
        (N'Sableye', 125, 95, 95, 85, 85, 70),
        (N'Pikachu', 110, 75, 60, 70, 70, 110),
        (N'Charizard', 153, 104, 98, 129, 105, 120),
        (N'Blastoise', 154, 103, 120, 105, 125, 98),
        (N'Metagross', 155, 187, 150, 122, 110, 90),
        (N'Feraligatr', 160, 145, 120, 99, 103, 98)
    ) v(Nombre, SaludTotalBase, AtaqueBase, DefensaBase, AtaqueEspecialBase, DefensaEspecialBase, VelocidadBase)
)
INSERT INTO dbo.Pokemons (Nombre, SaludTotalBase, AtaqueBase, DefensaBase, AtaqueEspecialBase, DefensaEspecialBase, VelocidadBase)
SELECT s.Nombre, s.SaludTotalBase, s.AtaqueBase, s.DefensaBase, s.AtaqueEspecialBase, s.DefensaEspecialBase, s.VelocidadBase
FROM src s
WHERE NOT EXISTS (SELECT 1 FROM dbo.Pokemons p WHERE p.Nombre = s.Nombre);

------------------------------------------------------------
-- 3) TIPOS POR POKEMON (insert if missing)
------------------------------------------------------------
;WITH src (PokemonNombre, Tipo) AS (
    SELECT * FROM (VALUES
        (N'Sableye', 5), (N'Sableye', 14),
        (N'Pikachu', 4),
        (N'Charizard', 6), (N'Charizard', 17),
        (N'Blastoise', 1),
        (N'Metagross', 0), (N'Metagross', 12),
        (N'Feraligatr', 1)
    ) v(PokemonNombre, Tipo)
)
INSERT INTO dbo.PokemonTipos (PokemonId, Tipo)
SELECT p.PokemonId, s.Tipo
FROM src s
JOIN dbo.Pokemons p ON p.Nombre = s.PokemonNombre
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.PokemonTipos pt
    WHERE pt.PokemonId = p.PokemonId AND pt.Tipo = s.Tipo
);

------------------------------------------------------------
-- 4) MOVIMIENTOS POSIBLES
------------------------------------------------------------
;WITH src (PokemonNombre, MovimientoNombre) AS (
    SELECT * FROM (VALUES
        -- Sableye
        (N'Sableye', N'Arañazo'), (N'Sableye', N'Tinieblas'), (N'Sableye', N'Impresionar'), (N'Sableye', N'Golpes Furia'),
        (N'Sableye', N'Sorpresa'), (N'Sableye', N'Finta'), (N'Sableye', N'Desarme'), (N'Sableye', N'Bola Sombra'),

        -- Pikachu
        (N'Pikachu', N'Impactrueno'), (N'Pikachu', N'Ataque Rapido'), (N'Pikachu', N'Portazo'), (N'Pikachu', N'Rayo'), (N'Pikachu', N'Trueno'),

        -- Charizard
        (N'Charizard', N'Arañazo'), (N'Charizard', N'Ascuas'), (N'Charizard', N'Furia'), (N'Charizard', N'Lanzallamas'),
        (N'Charizard', N'Ataque Ala'), (N'Charizard', N'Cuchillada'), (N'Charizard', N'Furia Dragon'), (N'Charizard', N'Giro Fuego'),

        -- Metagross
        (N'Metagross', N'Derribo'), (N'Metagross', N'Confusion'), (N'Metagross', N'Garra Metal'),
        (N'Metagross', N'Persecucion'), (N'Metagross', N'Psiquico'), (N'Metagross', N'Puño Meteoro'), (N'Metagross', N'Hiperrayo'),

        -- Blastoise
        (N'Blastoise', N'Placaje'), (N'Blastoise', N'Burbuja'), (N'Blastoise', N'Pistola Agua'),
        (N'Blastoise', N'Mordisco'), (N'Blastoise', N'Giro Rapido'), (N'Blastoise', N'Cabezazo'), (N'Blastoise', N'Hidrobomba'),

        -- Feraligatr
        (N'Feraligatr', N'Arañazo'), (N'Feraligatr', N'Furia'), (N'Feraligatr', N'Pistola Agua'),
        (N'Feraligatr', N'Mordisco'), (N'Feraligatr', N'Cuchillada'), (N'Feraligatr', N'Hidrobomba')
    ) v(PokemonNombre, MovimientoNombre)
)
INSERT INTO dbo.PokemonMovimientosPosibles (PokemonId, MovimientoId)
SELECT p.PokemonId, m.MovimientoId
FROM src s
JOIN dbo.Pokemons p ON p.Nombre = s.PokemonNombre
JOIN dbo.Movimientos m ON m.Nombre = s.MovimientoNombre
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.PokemonMovimientosPosibles x
    WHERE x.PokemonId = p.PokemonId AND x.MovimientoId = m.MovimientoId
);

------------------------------------------------------------
-- 5) MOVIMIENTOS REALES (4 por pokemon)
------------------------------------------------------------
;WITH src (PokemonNombre, MovimientoNombre) AS (
    SELECT * FROM (VALUES
        (N'Sableye', N'Bola Sombra'), (N'Sableye', N'Finta'), (N'Sableye', N'Desarme'), (N'Sableye', N'Sorpresa'),
        (N'Pikachu', N'Impactrueno'), (N'Pikachu', N'Ataque Rapido'), (N'Pikachu', N'Rayo'), (N'Pikachu', N'Trueno'),
        (N'Charizard', N'Lanzallamas'), (N'Charizard', N'Ataque Ala'), (N'Charizard', N'Cuchillada'), (N'Charizard', N'Giro Fuego'),
        (N'Metagross', N'Puño Meteoro'), (N'Metagross', N'Psiquico'), (N'Metagross', N'Garra Metal'), (N'Metagross', N'Hiperrayo'),
        (N'Blastoise', N'Hidrobomba'), (N'Blastoise', N'Pistola Agua'), (N'Blastoise', N'Cabezazo'), (N'Blastoise', N'Mordisco'),
        (N'Feraligatr', N'Hidrobomba'), (N'Feraligatr', N'Cuchillada'), (N'Feraligatr', N'Mordisco'), (N'Feraligatr', N'Arañazo')
    ) v(PokemonNombre, MovimientoNombre)
)
INSERT INTO dbo.PokemonMovimientos (PokemonId, MovimientoId)
SELECT p.PokemonId, m.MovimientoId
FROM src s
JOIN dbo.Pokemons p ON p.Nombre = s.PokemonNombre
JOIN dbo.Movimientos m ON m.Nombre = s.MovimientoNombre
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.PokemonMovimientos x
    WHERE x.PokemonId = p.PokemonId AND x.MovimientoId = m.MovimientoId
);

------------------------------------------------------------
-- 6) MIS POKEMONS de ejemplo (opcional)
------------------------------------------------------------
DECLARE @PikachuId INT = (SELECT PokemonId FROM dbo.Pokemons WHERE Nombre = N'Pikachu');
DECLARE @BlastoiseId INT = (SELECT PokemonId FROM dbo.Pokemons WHERE Nombre = N'Blastoise');

IF @PikachuId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.MisPokemons WHERE PokemonId = @PikachuId AND NombrePersonalizado = N'Sparky')
BEGIN
    INSERT INTO dbo.MisPokemons (PokemonId, NombrePersonalizado, Nivel, SaludActual)
    VALUES (@PikachuId, N'Sparky', 50, 110);
END

IF @BlastoiseId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM dbo.MisPokemons WHERE PokemonId = @BlastoiseId AND NombrePersonalizado = N'Tank')
BEGIN
    INSERT INTO dbo.MisPokemons (PokemonId, NombrePersonalizado, Nivel, SaludActual)
    VALUES (@BlastoiseId, N'Tank', 50, 154);
END

DECLARE @MiPikachuId INT = (
    SELECT TOP 1 MiPokemonId
    FROM dbo.MisPokemons
    WHERE NombrePersonalizado = N'Sparky'
    ORDER BY MiPokemonId DESC
);

DECLARE @MiBlastoiseId INT = (
    SELECT TOP 1 MiPokemonId
    FROM dbo.MisPokemons
    WHERE NombrePersonalizado = N'Tank'
    ORDER BY MiPokemonId DESC
);

IF @MiPikachuId IS NOT NULL
BEGIN
    DELETE FROM dbo.MiPokemonMovimientos WHERE MiPokemonId = @MiPikachuId;

    INSERT INTO dbo.MiPokemonMovimientos (MiPokemonId, Slot, MovimientoId)
    SELECT @MiPikachuId, 1, MovimientoId FROM dbo.Movimientos WHERE Nombre = N'Portazo'
    UNION ALL
    SELECT @MiPikachuId, 2, MovimientoId FROM dbo.Movimientos WHERE Nombre = N'Rayo'
    UNION ALL
    SELECT @MiPikachuId, 3, MovimientoId FROM dbo.Movimientos WHERE Nombre = N'Ataque Rapido'
    UNION ALL
    SELECT @MiPikachuId, 4, MovimientoId FROM dbo.Movimientos WHERE Nombre = N'Impactrueno';
END

IF @MiBlastoiseId IS NOT NULL
BEGIN
    DELETE FROM dbo.MiPokemonMovimientos WHERE MiPokemonId = @MiBlastoiseId;

    INSERT INTO dbo.MiPokemonMovimientos (MiPokemonId, Slot, MovimientoId)
    SELECT @MiBlastoiseId, 1, MovimientoId FROM dbo.Movimientos WHERE Nombre = N'Cabezazo'
    UNION ALL
    SELECT @MiBlastoiseId, 2, MovimientoId FROM dbo.Movimientos WHERE Nombre = N'Giro Rapido'
    UNION ALL
    SELECT @MiBlastoiseId, 3, MovimientoId FROM dbo.Movimientos WHERE Nombre = N'Hidrobomba'
    UNION ALL
    SELECT @MiBlastoiseId, 4, MovimientoId FROM dbo.Movimientos WHERE Nombre = N'Mordisco';
END

COMMIT TRAN;
GO
