# Bug Analysis: Chat Permissions and Naming

## Bug Overview
1. **Private Chat Display**: The sidebar should prominently display the private chat user's name and their profile picture, rather than generic chat details.
2. **Member Management Permissions**: Currently, any member in a group chat can add new members. In private chats, member management shouldn't be allowed at all. Group chats should restrict adding members to the chat owner only.
3. **Chat Deletion**: There is no way for users to delete chats from the UI. A delete feature is needed for both private chats (allowed for both users) and group chats (restricted to the owner).

## Potential Root Causes
- The backend `ChatController.cs`'s `AddChatRoomMembers` endpoint only checks if the requester is a member of the chat, not if they are the owner.
- There is no `DELETE /api/chat/{id}` endpoint exposed in `ChatController.cs`, even though `IChatRoomService` supports deletion.
- The frontend sidebar does not render the profile picture for private chats.

## Use Cases & Edge Cases
- **Adding members to Private Chat**: Should be rejected by the backend. The frontend button should also be hidden.
- **Adding members to Group Chat**: Should succeed if requester is Owner; fail (403 Forbidden) if not.
- **Deleting Private Chat**: Either of the two members should be able to delete the chat for both.
- **Deleting Group Chat**: Only the Owner should be able to delete the group chat.

## Proposed Fix Strategy
### Backend
1. **`ChatController.cs` - `AddChatRoomMembers` & `RemoveChatRoomMembers`**:
   - Add validation: `if (chatRoom.Type == "Private") return BadRequest("Cannot modify private chat members");`
   - Add validation: `if (chatRoom.OwnerId != currentUserId) return Forbidden("Only the owner can manage members");`
2. **`ChatController.cs` - New Endpoint `DELETE {id}`**:
   - Add `DeleteChatRoom(string chatRoomId)`.
   - Validate membership.
   - If `chatRoom.Type == "Private"`, allow deletion by any member.
   - If `chatRoom.Type == "Group"`, require `chatRoom.OwnerId == currentUserId`.
   - Call `_chatRoomService.DeleteChatRoomAsync(chatRoomId)`.

### Frontend
1. **`ChatSidebar.jsx`**:
   - Refactor the list item to display the target user's profile picture for private chats.
   - Add a "Delete Chat" button (e.g., a trash icon) next to the chat name, appearing conditionally based on the rules (always for private, only if owner for group).
2. **`ChatMembersModal.jsx` / `ChatWindow.jsx`**:
   - Hide the "Add Member" and "Manage Members" buttons if the chat is private or if the current user is not the owner of the group chat.

## Verification Plan
1. Rebuild backend and frontend.
2. Login as User A. Create a group chat. Verify User A can add members and delete the chat.
3. Login as User B (added to group chat). Verify User B *cannot* see the add member button or delete chat button, and API calls are rejected if forced.
4. User A starts a private chat with User B. Verify the sidebar shows User B's picture.
5. Verify both User A and User B can delete the private chat, and neither can add members to it.
