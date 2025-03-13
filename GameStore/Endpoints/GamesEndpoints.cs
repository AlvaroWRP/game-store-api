using GameStore.Data;
using GameStore.DTOs;
using GameStore.Entities;
using GameStore.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Endpoints;

public static class GamesEndpoints
{
    const string routeName = "GetGames";

    public static RouteGroupBuilder MapGamesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("games").WithParameterValidation();

        group.MapGet("/", async (GameStoreContext dbContext) =>
            await dbContext.Games
                .Include(
                    game => game.Genre
                )
                .Select(
                    game => game.ToGameSummaryDto()
                )
                .AsNoTracking()
                .ToListAsync());

        group.MapGet("/{id}", async (int id, GameStoreContext dbContext) =>
        {
            Game? game = await dbContext.Games.FindAsync(id);

            return game == null ? Results.NotFound() : Results.Ok(game.ToGameDetailsDto());
        })
        .WithName(routeName);

        group.MapPost("/", async (CreateGameDto newGame, GameStoreContext dbContext) =>
        {
            Game game = newGame.ToEntity();

            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(
                routeName,
                new { id = game.Id },
                game.ToGameDetailsDto()
            );
        });

        group.MapPut("/{id}", async (int id, UpdateGameDto updatedGame, GameStoreContext dbContext) =>
        {
            var existingGame = await dbContext.Games.FindAsync(id);

            if (existingGame == null)
            {
                return Results.NotFound();
            }

            dbContext.Entry(
                existingGame
            )
            .CurrentValues
            .SetValues(
                updatedGame.ToEntity(id)
            );

            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (int id, GameStoreContext dbContext) =>
        {
            await dbContext.Games.Where(
                game => game.Id == id
            )
            .ExecuteDeleteAsync();

            return Results.NoContent();
        });

        return group;
    }
}
