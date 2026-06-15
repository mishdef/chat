# OnlineChat Backend Architecture Guide

This document provides a comprehensive overview of the backend architecture, patterns, and technologies used in the `OnlineChat` application (`SignalRProject`). It is intended to help AI agents and developers quickly understand the codebase and its structure.

## Tech Stack & Core Technologies
*   **Framework:** ASP.NET Core Web API (C#)
*   **Database:** SQLite (`app.db`)
*   **ORM:** Entity Framework Core
*   **Authentication:** ASP.NET Core Identity & JWT (JSON Web Tokens)
*   **Real-time Communication:** SignalR
*   **API Documentation:** Scalar API Reference

## Directory Structure

*   `Controllers/`: Defines RESTful API endpoints.
*   `Hubs/`: Contains SignalR Hub implementations for WebSockets.
*   `Services/`: Business logic layer (Dependency Injection is heavily used).
*   `Model/`: Entity Framework Core database models.
*   `DTO/`: Data Transfer Objects for defining API request/response shapes.
*   `Data/`: Contains `AppDbContext`.
*   `Middleware/`: Custom middlewares (e.g., `GlobalExeptionHandler`).
*   `wwwroot/`: Serves static files, specifically user-uploaded images and avatars.

## Domain Models (`Model/`)

*   **`User`**: Inherits from `IdentityUser`. Key properties: `NickName` (unique), `ProfilePictureUrl`, `ConnectionId` (stores the active SignalR connection ID), and `Chats` (many-to-many relationship with `ChatRoom`).
*   **`ChatRoom`**: Represents a chat group or a private chat. Key properties: `Name`, `Type` (e.g., "Group" or "Private"), `OwnerId`, `Owner` (User), and `Users` (collection of members).
*   **`Message`**: Represents a single chat message. Key properties: `ChatId`, `UserId`, `Content`, `Type` (e.g., "text", "image"), and `CreatedAt`.

## Key Architectural Patterns

### 1. Authentication & Identity Flow
*   **REST API:** The `AuthController` handles standard Registration and Login, returning a standard JWT token upon success.
*   **SignalR:** Because WebSockets cannot easily send standard HTTP headers initially, the JWT token is passed via a query string parameter (`?access_token=...`). The `Program.cs` intercepts this in the `JwtBearerEvents.OnMessageReceived` event and validates the token to authenticate the WebSocket connection.

### 2. Real-time Communication (SignalR)
*   **Hub Location:** Hosted at the `/chatHub` endpoint via `ChatHub.cs`.
*   **Connection Tracking:** When a user connects to the hub, `OnConnectedAsync` retrieves their User ID from the JWT, fetches the user from the database, and updates their `ConnectionId` field. This allows the system to send direct real-time messages to specific users.
*   **Broadcasting:** The hub has methods like `SendMessageToChatRoom`, `ChatRoomCreated`, `ChatRoomDeleted`, and `ChatRoomUpdated`. These methods typically iterate through the `ConnectionId`s of a chat room's members and use `Clients.Client(connectionId).SendAsync(...)` to push updates.

### 3. File Uploads & Image Processing
*   **Images & Avatars:** Uploaded via `IFormFile` in multipart form data requests (handled via `IImageService`).
*   **Compression & Resizing:** Powered by `SixLabors.ImageSharp` (v2.1.9). Profile and chat pictures are automatically cropped to squares and capped at `256x256`. Regular chat message images preserve aspect ratio but are capped at `1280x720` (720p). All images are saved as 80% quality JPGs to minimize footprint.
*   **Storage:** Files are saved directly to the local filesystem inside the `wwwroot/uploads` and `wwwroot/uploads/avatars` directories. The relative URL (e.g., `/uploads/filename.jpg` or `filename.jpg`) is saved in the database.

### 4. Database Management
*   **Provider:** SQLite.
*   **Initialization:** The project does not currently rely heavily on EF Core Migrations for startup setup. Instead, `dbContext.Database.EnsureCreated()` is called in `Program.cs` to ensure the database schema exists when the application starts.

### 5. Dependency Injection & Services
*   Controllers rely strictly on Interfaces (e.g., `IApiCustomerAuthService`, `IChatRoomService`, `IUserService`) rather than concrete implementations, adhering to clean architecture principles. All services are registered as scoped services in `Program.cs`.

## 7. Recent Implementations & Changelog
*   **Image Compression Engine:** Integrated `SixLabors.ImageSharp` to centralize image processing via an `IImageService`. Profile and chat avatars are now forcefully cropped to 256x256 squares. Regular chat images are dynamically resized to maintain aspect ratio with a maximum resolution of 1280x720 (720p), saved as highly compressed JPGs.
*   **Database Seeding & Identity:** Reconfigured Identity options in `Program.cs` to allow simple passwords and added logic to seed `test@gmail.com` and `test1@gmail.com` (both with password `"password"`) at startup.
*   **Real-time Image Syncing:** Frontend `ChatWindow.jsx` now listens to `RoomUpdated` events to refresh background UI. The `UserController.cs` was updated to fire `RoomUpdated` to all a user's chats when they change their profile picture, enabling instant syncing across clients.
*   **Chat Pictures:** Added a `PictureUrl` property to `ChatRoom`. Split `ChatController` into partial files (`ChatController.cs`, `ChatController.Members.cs`, `ChatController.Messages.cs`) to keep logic clean. Implemented endpoints and frontend UI for group chat owners to upload chat pictures.
*   **Improved Search UI:** Search functions now successfully filter users by `UserName`, `Email`, and `NickName`. Loading spinners have been integrated into both the `ChatSidebar` and the `ChatMembersModal` to vastly improve UI clarity while searches run.
*   **Sidebar Enhancements:** The chat sidebar now displays the last message (content or image emoji), message author, and timestamp. It also intelligently displays the profile picture of the target user for private chats.
*   **Member Management & Permissions:** Users can no longer modify members of private chats. For group chats, only the owner can add or remove members.
*   **Chat Deletion:** Added a `DELETE /api/chat/{id}` endpoint. Users can now delete private chats (for both participants), and group chat owners can delete their group chats via the UI.
*   **Private Chat:** Added the ability to create and send messages in private chats between two users. `ChatRoom` model updated to include `Type` ("Group" or "Private"). Added a `POST /api/chat/private` endpoint and user search functionality in the frontend sidebar.

## Error Handling
*   The application uses a custom `GlobalExeptionHandler` middleware to catch unhandled exceptions and format them into standardized JSON API responses. API responses are generally wrapped in a generic `ApiResponse<T>` object to maintain a consistent structure (`Data`, `Message`, `IsSuccess`).
