# Real-Time .NET User Service

This project is built with .NET 8 and demonstrates:
- REST API development with ASP.NET Core
- PostgreSQL integration using EF Core
- Real-time updates via SignalR
- Asynchronous event processing with RabbitMQ + MassTransit
- Structured logging with Serilog
- Dockerized environment with Docker Compose

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [VS Code](https://code.visualstudio.com/) (with Live Server extension optional)

---

## 🔧 Setup

### 1. Clone the repo & open it
```bash
git clone https://github.com/your-name/dotnet-realtime-user-service.git
cd dotnet-realtime-user-service/UserApi
code .
```

### 2. Run PostgreSQL & RabbitMQ with Docker
```bash
docker compose up -d
```

RabbitMQ UI available at: [http://localhost:15672](http://localhost:15672)  
Login: `guest` / `guest`

---

## 🛠️ Development

### Run the API
```bash
dotnet run
```

Swagger available at: [http://localhost:5077/swagger](http://localhost:5077/swagger)

### Run from Docker (fully containerized)
```bash
docker compose up --build
```

Swagger will now be at: [http://localhost:5000/swagger](http://localhost:5000/swagger)

---

## 💬 Real-time HTML Client (SignalR)

### Steps:
1. Open `test.html` in VS Code
2. Right-click → **Open with Live Server**
3. Go to [http://127.0.0.1:5500/test.html](http://127.0.0.1:5500/test.html)
4. POST a user via Swagger → You’ll see real-time updates

---

## 📨 RabbitMQ + MassTransit

- Every time a user is created, a `UserCreatedEvent` is published.
- MassTransit consumes this event and processes it in the background.
- Consumer logs the event to the console.

---

## 📊 Logging with Serilog

- Logs are structured and output to the console
- Great for debugging and Docker logging

---

## ✅ Project Structure

```
UserApi/
├── Controllers/
├── Consumers/
├── Events/
├── Hubs/
├── Models/
├── Data/
├── Program.cs
├── test.html
├── Dockerfile
├── docker-compose.yml
```