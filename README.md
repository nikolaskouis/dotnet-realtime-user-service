# Real-Time .NET User Service

This project is built with .NET 8 and demonstrates:
- A REST API with ASP.NET Core connected to PostgreSQL via EF Core.
- Real-time updates using SignalR.
- Testing via Swagger and a live HTML client.

## Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [VS Code](https://code.visualstudio.com/) (with Live Server extension optional)

### Running the Backend

1. **Clone the Repository & Open in VS Code**

   ```bash
   git clone https://github.com/your-name/dotnet-realtime-user-service.git
   cd dotnet-realtime-user-service/UserApi
   code .
   ```

2. **Start PostgreSQL via Docker**

   From the project root (where `docker-compose.yml` is located), run:

   ```bash
   docker compose up -d
   ```

3. **Apply Database Migrations**

   In the terminal, run:

   ```bash
   dotnet ef database update
   ```

4. **Run the API**

   Run the project from VS Code or via terminal:

   ```bash
   dotnet run
   ```

   The app will be available at `http://localhost:5077`.  
   Open [http://localhost:5077/swagger](http://localhost:5077/swagger) to test API endpoints via Swagger.

### Live HTML Client with VS Code Live Server

1. **Open `test.html` in VS Code**

   The `test.html` file (located in the project root) connects to the SignalR hub for real-time updates.

2. **Start Live Server**

    - Right-click on `test.html` in VS Code.
    - Select **"Open with Live Server"**.
    - This will open the page at a URL like `http://127.0.0.1:5500/test.html`.

3. **Trigger Real-Time Updates**

   Use Swagger to POST a new user:
   ```json
   {
     "username": "nikos",
     "email": "nikos@example.com"
   }
   ```
   The live HTML client should receive and display the update immediately via SignalR.

## Next Steps

- [ ] Implement background processing with RabbitMQ & MassTransit.
- [ ] Enhance logging and monitoring (Serilog).