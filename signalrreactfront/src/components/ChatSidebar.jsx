import React, { useState, useEffect, useContext } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import axios from 'axios';
import CreateChatModal from './CreateChatModal';
import { AuthContext } from '../AuthContext';

const ChatSidebar = ({ connection }) => {
    const { user: currentUser } = useContext(AuthContext);
    const [chats, setChats] = useState([]);
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [searchQuery, setSearchQuery] = useState('');
    const [searchResults, setSearchResults] = useState([]);
    const [isSearching, setIsSearching] = useState(false);
    const [isLoadingSearch, setIsLoadingSearch] = useState(false);
    const navigate = useNavigate();

    const fetchChats = async () => {
        try {
            const res = await axios.get('/api/chat/my-chats');
            if (res.data && res.data.data) {
                const newChats = res.data.data;
                setChats(newChats);

                const pathParts = window.location.pathname.split('/chat/');
                if (pathParts.length > 1) {
                    const currentChatId = pathParts[1];
                    if (currentChatId && !newChats.some(chat => chat.chatId === currentChatId)) {
                        navigate('/');
                    }
                }
            }
        } catch (err) {
            console.error('Failed to fetch chats:', err);
        }
    };

    useEffect(() => {
        fetchChats();
    }, []);

    useEffect(() => {
        if (!connection) return;

        const handleRefresh = () => {
            fetchChats();
        };

        connection.on('ChatRoomCreated', handleRefresh);
        connection.on('RoomUpdated', handleRefresh);
        connection.on('RoomDeleted', handleRefresh);

        return () => {
            connection.off('ChatRoomCreated', handleRefresh);
            connection.off('RoomUpdated', handleRefresh);
            connection.off('RoomDeleted', handleRefresh);
        };
    }, [connection]);

    useEffect(() => {
        const delayDebounceFn = setTimeout(async () => {
            if (searchQuery.trim() === '') {
                setSearchResults([]);
                setIsSearching(false);
                setIsLoadingSearch(false);
                return;
            }

            setIsSearching(true);
            setIsLoadingSearch(true);
            try {
                const res = await axios.get(`/api/user/search?query=${searchQuery}`);
                // Filter out current user from results
                const filteredResults = res.data.filter(u => u.id !== currentUser?.id);
                setSearchResults(filteredResults);
            } catch (error) {
                console.error("Error searching users", error);
            } finally {
                setIsLoadingSearch(false);
            }
        }, 300);

        return () => clearTimeout(delayDebounceFn);
    }, [searchQuery, currentUser]);

    const handleChatCreated = () => {
        fetchChats();
        setShowCreateModal(false);
    };

    const startPrivateChat = async (targetUserId) => {
        try {
            const res = await axios.post('/api/chat/private', { targetUserId });
            if (res.data && res.data.data) {
                const newChat = res.data.data;
                setSearchQuery('');
                setSearchResults([]);
                fetchChats();
                navigate(`/chat/${newChat.chatId}`);
            }
        } catch (error) {
            console.error('Failed to start private chat:', error);
        }
    };

        return (
            <div className="d-flex flex-column h-100">
                <div className="p-3 border-bottom d-flex justify-content-between align-items-center bg-light">
                    <h5 className="m-0 fw-bold">Chats</h5>
                    <button 
                        className="btn btn-primary btn-sm rounded-circle shadow-sm" 
                        onClick={() => setShowCreateModal(true)}
                        style={{ width: '32px', height: '32px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}
                    >
                        +
                    </button>
                </div>

                <div className="p-3 border-bottom">
                    <input 
                        type="text" 
                        className="form-control" 
                        placeholder="Search users to message..." 
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                    />
                </div>
                
                <div className="flex-grow-1 overflow-auto">
                    {isSearching && searchQuery.trim() !== '' ? (
                        <div className="list-group list-group-flush">
                            {isLoadingSearch ? (
                                <div className="p-4 text-center">
                                    <div className="spinner-border text-primary" role="status">
                                        <span className="visually-hidden">Loading...</span>
                                    </div>
                                </div>
                            ) : searchResults.length > 0 ? searchResults.map(user => (
                                <button 
                                    key={user.id} 
                                    className="list-group-item list-group-item-action py-3 border-bottom text-start"
                                    onClick={() => startPrivateChat(user.id)}
                                >
                                    <div className="d-flex align-items-center">
                                        <div className="me-3">
                                            <img 
                                                src={user.profilePictureUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(user.nickName || user.userName || 'User')}&background=random`} 
                                                alt={user.userName} 
                                                className="rounded-circle object-fit-cover" 
                                                width="40" 
                                                height="40" 
                                                onError={(e) => { e.target.onerror = null; e.target.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(user.nickName || user.userName || 'User')}&background=random`; }}
                                            />
                                        </div>
                                        <div>
                                            <h6 className="mb-0 fw-semibold">{user.nickName || user.userName}</h6>
                                            <small className="text-muted">@{user.userName}</small>
                                        </div>
                                    </div>
                                </button>
                            )) : (
                                <div className="p-3 text-center text-muted">
                                    <small>No users found</small>
                                </div>
                            )}
                        </div>
                    ) : (
                        <div className="list-group list-group-flush">
                            {chats.map(chat => {
                                let displayName = chat.name;
                                let displayImage = chat.pictureUrl 
                                    ? `/uploads/${chat.pictureUrl}` 
                                    : `https://ui-avatars.com/api/?name=${encodeURIComponent(chat.name)}&background=random`;
                                
                                if (chat.type === "Private" && currentUser) {
                                    const otherUser = chat.users?.find(u => u.id !== currentUser.id);
                                    if (otherUser) {
                                        displayName = otherUser.nickName || otherUser.userName;
                                        displayImage = otherUser.profilePictureUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(displayName)}&background=random`;
                                    }
                                }

                                return (
                                    <NavLink 
                                        key={chat.chatId} 
                                        to={`/chat/${chat.chatId}`} 
                                        className={({ isActive }) => 
                                            `list-group-item list-group-item-action py-3 border-bottom ${isActive ? 'bg-primary text-white' : ''}`
                                        }
                                    >
                                        {({ isActive }) => (
                                            <div className="d-flex align-items-center w-100">
                                                <div className="me-3">
                                                    <img 
                                                        src={displayImage} 
                                                        alt="chat-icon" 
                                                        className="rounded-circle object-fit-cover" 
                                                        width="45" 
                                                        height="45" 
                                                        onError={(e) => { e.target.onerror = null; e.target.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(displayName)}&background=random`; }}
                                                    />
                                                </div>
                                                <div className="flex-grow-1 overflow-hidden">
                                                    <div className="d-flex justify-content-between align-items-center">
                                                        <h6 className="mb-1 text-truncate fw-semibold" style={{ maxWidth: '70%' }}>{displayName}</h6>
                                                        {chat.lastMessage && (
                                                            <small className={`text-nowrap ${isActive ? "text-light" : "text-muted"}`} style={{ fontSize: '0.7rem' }}>
                                                                {new Date(chat.lastMessage.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                                                            </small>
                                                        )}
                                                    </div>
                                                    <div className="d-flex justify-content-between align-items-center mt-1">
                                                        <div className={`small text-truncate ${isActive ? "text-light" : "text-muted"}`} style={{ maxWidth: '80%' }}>
                                                            {chat.lastMessage ? (
                                                                <>
                                                                    <span className="fw-medium">{chat.lastMessage.user?.nickName || chat.lastMessage.user?.userName}: </span>
                                                                    {chat.lastMessage.type === 'image' ? '🖼️ Image' : chat.lastMessage.content}
                                                                </>
                                                            ) : (
                                                                chat.type === "Private" ? "Private Chat" : `Owner: ${chat.owner?.userName || chat.ownerId.substring(0,8)}`
                                                            )}
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        )}
                                    </NavLink>
                                );
                            })}
                            {chats.length === 0 && (
                                <div className="p-4 text-center text-muted">
                                    <p className="mb-0">No chats yet.</p>
                                    <small>Search for a user or create a group to start!</small>
                                </div>
                            )}
                        </div>
                    )}
                </div>

                {showCreateModal && (
                    <CreateChatModal 
                        onClose={() => setShowCreateModal(false)} 
                        onSuccess={handleChatCreated} 
                    />
                )}
            </div>
        );
    };

    export default ChatSidebar;
