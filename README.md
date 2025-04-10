# 👥 Realtime User Service (.NET 8 + PostgreSQL + RabbitMQ)

This project is a full-featured backend API built using **.NET 8**, designed to manage users in real-time. It demonstrates best practices in modern backend development, including **SignalR for real-time communication**, **MassTransit with RabbitMQ** for message-based architecture, and **EF Core with PostgreSQL** for data storage.

---

## ⚙️ Tech Stack

- **ASP.NET Core 8**
- **Entity Framework Core** with PostgreSQL
- **SignalR** (WebSockets) for real-time updates
- **MassTransit** with **RabbitMQ** (event-driven)
- **Autofac** for Dependency Injection
- **Serilog** for structured logging
- **Swagger/OpenAPI** for API documentation
- **Docker + docker-compose** for containerization

---

## ✨ Features

- ✅ Create, Get, Delete and Update users
- ✅ Realtime notifications when a user is added or deleted (`SignalR`)
- ✅ Events published over RabbitMQ (`UserCreatedEvent`, `UserDeletedEvent`, `UserUpdateEvent`)
- ✅ RESTful endpoints with Swagger UI
- ✅ Structured logging using Serilog
- ✅ Fully dockerized stack with PostgreSQL + RabbitMQ

---

## 🐳 How to Run with Docker

Make sure Docker is installed and running on your machine.

### 🛠 Build & Run
```bash
  docker-compose up --build
```

- API: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`
- RabbitMQ UI: `http://localhost:15672` (`guest` / `guest`)
- PostgreSQL: running on `localhost:5432`

### 🔁 Tear Down
```bash
  docker-compose down
```

---

## 🖥️ How to Run Locally in VSCode

### 1. Clone the repo
```bash
  git clone https://github.com/your-username/realtime-user-service.git
  cd realtime-user-service
```

### 2. Set up the database
Make sure PostgreSQL is running (use Docker or install locally). Update your \`appsettings.json\` or environment variable:

```
json
"ConnectionStrings": {
"DefaultConnection": "Host=localhost;Database=userdb;Username=postgres;Password=postgres"
}
```

### 3. Run the app
```bash
  dotnet run --project UserApi
```

### 4. Access it
- Swagger: [http://localhost:5000/swagger](http://localhost:5000/swagger)

---

## 🔌 Realtime with SignalR

- SignalR is hosted at: `/hubs/user`
- When a user is added or deleted, a message like `"UserAdded"` or `"UserDeleted"` is broadcast to all connected clients.

### Frontend Example
```js
const connection = new signalR.HubConnectionBuilder()
.withUrl("http://localhost:5000/hubs/user")
.build();

connection.on("UserAdded", user => {
console.log("New user added:", user);
});

await connection.start();
```

---

## 📬 Events and Messaging (RabbitMQ)

- On user creation/deletion, events (\`UserCreatedEvent\`, \`UserDeletedEvent\`) are **published to RabbitMQ**
- \`UserCreatedEventConsumer\` processes the message in background

---

## 🧪 Running Tests

Tests are written using \`xUnit\` + \`Moq\`. To run them:

``` bash
    cd UserApi.Tests
    dotnet test
```

---

## 🔒 Security Notes

- No authentication is added (this is a demo project).
- CORS is enabled for testing: \`http://127.0.0.1:5500\`

---

## 📬 Endpoints Summary

| Method | Route            | Description       |
|--------|------------------|-------------------|
| GET    | \`/api/user\`      | Get all users     |
| GET    | \`/api/user/{id}\` | Get user by ID    |
| POST   | \`/api/user\`      | Create a new user |
| DELETE | \`/api/user/{id}\` | Delete a user     |
| PATCH  | \`/api/user/{id}\` | Update a user     |

---

## 👨‍💻 Author

- Created by **Nikolas** for a backend & DevOps interview challenge.
- Designed to show clean architecture, real-time communication, and event-driven design using .NET.