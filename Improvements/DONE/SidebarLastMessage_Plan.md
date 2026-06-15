# Feature Plan: Sidebar Last Message

## Overview
The goal is to display the last message of each chat room directly in the chat sidebar, giving users a preview of the ongoing conversation. This includes showing the author, the message snippet (or an image emoji if it's an image), and the timestamp.

## Backend Changes
1. **`Services/ChatRoomService.cs`**:
   - In `SendMessageAsync(Message message)`, update the associated `ChatRoom`'s `LastMessageId` property to the new message's ID and save the changes to the database.
   - In `GetUserChatRoomsAsync(string userId)`, include the `LastMessage` and its `User` relation so that the frontend receives this data: `.Include(c => c.LastMessage).ThenInclude(m => m.User)`.

## Frontend Changes
1. **`src/components/ChatSidebar.jsx`**:
   - Map over each chat and read `chat.lastMessage`.
   - If `lastMessage` exists:
     - Check `lastMessage.type`. If it's `'image'`, display `🖼️ Image`.
     - Otherwise, display a truncated snippet: `lastMessage.user.nickName: lastMessage.content`.
   - Display `lastMessage.createdAt` formatting it to a readable time (e.g., `HH:mm` or short date).
   - If `lastMessage` is null, display nothing or a placeholder like "No messages yet".

## Verification Plan
1. Run `dotnet build` to ensure backend logic is sound.
2. Start the application. Send a text message and an image message in a chat.
3. Observe the sidebar updating in real-time (via SignalR refetches) to display the correct message snippet, author, and timestamp.
