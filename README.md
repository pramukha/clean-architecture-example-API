# Player Team Generator Web API

A .NET 6.0 Web API that generates teams based on player positions and skills, with player management capabilities.

## Features

- Player Management (CRUD operations)
- Team Generation based on positions and skills
- Protected DELETE endpoint with Bearer token authentication
- In-memory database for development
- Swagger/OpenAPI documentation

## Prerequisites

- .NET 6.0 SDK
- Visual Studio Code or Visual Studio 2022
- Postman (optional, for testing)

## Project Structure

```
csharp-backend-app/
├── Api/
│   └── Controllers/
│       ├── PlayersController.cs
│       └── TeamsController.cs
├── Application/
│   ├── DTOs/
│   │   ├── PlayerDto.cs
│   │   └── TeamDto.cs
│   ├── Interfaces/
│   │   ├── IPlayerService.cs
│   │   └── ITeamService.cs
│   └── Services/
│       ├── PlayerService.cs
│       └── TeamService.cs
├── Domain/
│   └── Entities/
│       ├── Player.cs
│       ├── PlayerSkill.cs
│       └── Team.cs
├── Infrastructure/
│   └── Data/
│       ├── ApplicationDbContext.cs
│       └── DbInitializer.cs
└── Tests/
    └── Application.Tests/
        └── TeamServiceTests.cs
```

## Getting Started

1. Clone the repository
2. Open the solution in Visual Studio Code
3. Restore NuGet packages:
   ```bash dotnet restore
   ```
4. Run the application:
   ```bash dotnet run
   ```

## API Endpoints

### Players

- `GET /api/players` - Get all players
- `GET /api/players/{id}` - Get player by ID
- `POST /api/players` - Create new player
- `PUT /api/players/{id}` - Update player
- `DELETE /api/players/{id}` - Delete player (Protected)

### Teams

- `POST /api/team/process` - Generate team based on positions and skills

## Sample Player JSON

```json
{
    "id": 1,
    "name": "player name 2",
    "position": "midfielder",
    "playerSkills": [
        {
            "id": 1,
            "skill": "attack",
            "value": 60,
            "playerId": 1
        },
        {
            "id": 2,
            "skill": "speed",
            "value": 80,
            "playerId": 1
        }
    ]
}
```

## Authentication

The DELETE player endpoint requires Bearer token authentication:

```
Authorization: Bearer SkFabTZibXE1aE14ckpQUUxHc2dnQ2RzdlFRTTM2NFE2cGI4d3RQNjZmdEFITmdBQkE=
```

## Testing

Run the tests using:
```bash
dotnet test
```

## Documentation

API documentation is available via Swagger UI at:
- Development: https://localhost:60339/swagger
- Production: {your-domain}/swagger

## License

This project is licensed under the MIT License - see the LICENSE file for details.
