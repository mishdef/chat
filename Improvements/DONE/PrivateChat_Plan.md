# Feature Plan: Private Chat

## Overview
This feature introduces private messaging between two users. It modifies the existing `ChatRoom` model to include a chat `Type` (e.g., "Group" or "Private"). The backend will provide an endpoint to create or fetch a private chat between the current user and a target user. The frontend will include a user search feature allowing users to find others and initiate private conversations.

## Backend Changes
1. **`Model/ChatRoom.cs`**:
   - Add a new property: `public string Type { get; set; } = "Group";`.
2. **`DTO/CreatePrivateChatDTO.cs`**:
   - Create a new DTO to encapsulate the request body for private chat creation:
     ```csharp
     public class CreatePrivateChatDTO {
         public string TargetUserId { get; set; }
     }
     ```
3. **`Controllers/ChatController.cs`**:
   - Add a new endpoint following REST principles: `[HttpPost("private")]`.
   - **Logic**:
     - Extract the current user's ID.
     - Look up all chat rooms where `Type == "Private"` and both `currentUser.Id` and `targetUserId` are members.
     - If such a chat exists, return it.
     - If not, create a new `ChatRoom` with `Type = "Private"`, an appropriate auto-generated name (e.g., "Private Chat"), assign both users to its `Users` list, and save it using `_chatRoomService.CreateChatRoomAsync`. Return the newly created chat.

## Frontend Changes
1. **`src/components/ChatSidebar.jsx`** (or a new integrated search component):
   - Add a search input field at the top of the sidebar or in a separate tab to search for users.
   - Use the existing `GET /api/user/search?query={searchTerm}` endpoint to fetch users matching the input.
   - Display the search results.
   - **Action**: When a user clicks on a search result, make a `POST` request to `/api/chat/private` with the `targetUserId`.
   - On success, navigate to the returned chat room's URL (`/chat/{chatId}`) and refresh the chat list.
2. **UI Polish**:
   - For private chats, dynamically display the target user's nickname instead of the generic auto-generated chat room name in the sidebar.

## Database / Model Updates
- The SQLite database schema will need to be updated to include the `Type` column in the `ChatRooms` table.
- Since the project relies on `EnsureCreated()` without migrations, the existing `app.db` will need to be deleted (or manually altered) during development to allow Entity Framework Core to recreate the schema with the new column.

## Testing & Verification Steps
1. Delete the `app.db` (and `app.db-shm`, `app.db-wal`) files to reset the database schema.
2. Run `dotnet build` to ensure the project compiles successfully with the new models and controller endpoints.
3. Start the backend (`dotnet run`) and frontend (`npm run dev`).
4. Register at least two users.
5. As User A, search for User B in the sidebar.
6. Click on User B to initiate a private chat. Verify the chat opens and appears in the chat list.
7. Send a message and ensure User B receives it in real-time.
