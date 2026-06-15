import React, { useState, useEffect, useContext, useRef } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import axios from 'axios';
import * as signalR from '@microsoft/signalr';
import { AuthContext } from '../AuthContext';
import ChatMembersModal from './ChatMembersModal';
import UserProfileViewModal from './UserProfileViewModal';

const ChatWindow = ({ connection }) => {
    const { id } = useParams();
    const { token, user } = useContext(AuthContext);
    const [chatRoom, setChatRoom] = useState(null);
    const [messages, setMessages] = useState([]);
    const [messageText, setMessageText] = useState('');
    const [imageFile, setImageFile] = useState(null);
    const [showMembersModal, setShowMembersModal] = useState(false);
    const [selectedUser, setSelectedUser] = useState(null);
    const [loading, setLoading] = useState(true);
    const [showSettings, setShowSettings] = useState(false);
    const messagesEndRef = useRef(null);
    const fileInputRef = useRef(null);
    const navigate = useNavigate();

    const scrollToBottom = () => {
        messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
    };

    const fetchChatData = async (isBackgroundRefresh = false) => {
        try {
            if (!isBackgroundRefresh) setLoading(true);
            const roomRes = await axios.get(`/api/chat/${id}`);
            setChatRoom(roomRes.data.data);

            const histRes = await axios.get(`/api/chat/${id}/history?startIndex=0&endIndex=100`);
            if (histRes.data && histRes.data.data) {
                setMessages(histRes.data.data.messages || []);
            }
        } catch (err) {
            console.error('Error fetching chat data', err);
        } finally {
            if (!isBackgroundRefresh) setLoading(false);
        }
    };

    useEffect(() => {
        fetchChatData();
    }, [id]);

    useEffect(() => {
        scrollToBottom();
    }, [messages]);

    useEffect(() => {
        if (!connection) return;

        const handleReceiveMessage = (message) => {
            if (message.chatId === id) {
                setMessages(prev => [...prev, message]);
            }
        };

        const handleRoomUpdated = (updatedId) => {
            if (updatedId === id) {
                fetchChatData(true);
            }
        };

        connection.on('RecieveMessage', handleReceiveMessage);
        connection.on('RoomUpdated', handleRoomUpdated);

        return () => {
            connection.off('RecieveMessage', handleReceiveMessage);
            connection.off('RoomUpdated', handleRoomUpdated);
        };
    }, [connection, id]);

    const handleSendMessage = async (e) => {
        e.preventDefault();
        if (!messageText.trim() && !imageFile) return;

        const formData = new FormData();
        formData.append('messageType', imageFile ? 'image' : 'text');
        if (messageText.trim()) formData.append('messageText', messageText);
        if (imageFile) formData.append('image', imageFile);

        try {
            await axios.post(`/api/chat/${id}/message`, formData, {
                headers: { 'Content-Type': 'multipart/form-data' }
            });
            setMessageText('');
            setImageFile(null);
            // File input reset
            const fileInput = document.getElementById('imageUpload');
            if (fileInput) fileInput.value = '';
        } catch (err) {
            console.error('Error sending message', err);
        }
    };

    const handleDeleteChat = async () => {
        if (window.confirm("Are you sure you want to delete this chat?")) {
            try {
                await axios.delete(`/api/chat/${id}`);
                navigate('/');
            } catch (error) {
                console.error('Failed to delete chat:', error);
                alert(error.response?.data?.message || 'Failed to delete chat');
            }
        }
    };

    const handleLeaveChat = async () => {
        if (window.confirm("Are you sure you want to leave this chat?")) {
            try {
                await axios.delete(`/api/chat/${id}/members`, {
                    data: [user.id],
                    headers: { 'Content-Type': 'application/json' }
                });
                navigate('/');
            } catch (error) {
                console.error('Failed to leave chat:', error);
                alert(error.response?.data?.message || 'Failed to leave chat');
            }
        }
    };

    if (loading) {
        return (
            <div className="d-flex align-items-center justify-content-center h-100">
                <div className="spinner-border text-primary" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </div>
        );
    }

    if (!chatRoom) {
        return <div className="p-4 text-center text-danger">Failed to load chat room.</div>;
    }

    let displayName = chatRoom?.name;
    let displayImage = chatRoom?.pictureUrl 
        ? `/uploads/${chatRoom.pictureUrl}`
        : `https://ui-avatars.com/api/?name=${encodeURIComponent(chatRoom?.name || 'Group')}&background=random`;
        
    if (chatRoom?.type === 'Private' && user) {
        const otherUser = chatRoom.users?.find(u => u.id !== user.id);
        if (otherUser) {
            displayName = otherUser.nickName || otherUser.userName;
            displayImage = otherUser.profilePictureUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(displayName)}&background=random`;
        }
    }

    const handlePictureUpload = async (e) => {
        const file = e.target.files[0];
        if (!file) return;

        const formData = new FormData();
        formData.append('image', file);

        try {
            await axios.post(`/api/chat/${id}/picture`, formData, {
                headers: { 'Content-Type': 'multipart/form-data' }
            });
            // Let the signalR RoomUpdated event refresh it, or we could force a refresh here
        } catch (error) {
            console.error('Error uploading picture', error);
            alert(error.response?.data?.message || 'Failed to upload picture');
        }
    };

    return (
        <div className="d-flex flex-column h-100">
            {/* Header */}
            <div className="p-3 border-bottom d-flex justify-content-between align-items-center bg-white shadow-sm z-1">
                <div className="d-flex align-items-center">
                    <Link to="/" className="btn btn-outline-primary btn-sm me-3 d-md-none d-flex align-items-center justify-content-center" style={{ width: '36px', height: '36px', borderRadius: '50%' }} aria-label="Back to chats">
                        ←
                    </Link>
                    <img 
                        src={displayImage} 
                        alt="Chat Icon" 
                        className="rounded-circle me-3 object-fit-cover shadow-sm border border-light" 
                        width="45" 
                        height="45" 
                        onError={(e) => { e.target.onerror = null; e.target.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(displayName)}&background=random`; }}
                    />
                    <div>
                        <h5 className="m-0 fw-bold">{displayName}</h5>
                        {chatRoom.type === 'Private' ? (
                            <small className="text-muted">Private Chat</small>
                        ) : (
                            <small className="text-muted">Chat ID: {id.substring(0,8)}...</small>
                        )}
                    </div>
                </div>
                <div className="dropdown position-relative">
                    <button 
                        className="btn btn-outline-secondary btn-sm rounded-pill px-3 shadow-sm" 
                        onClick={() => setShowSettings(!showSettings)}
                    >
                        ⚙️ Settings
                    </button>
                    {showSettings && (
                        <div className="dropdown-menu show position-absolute end-0 mt-2 shadow-sm" style={{ minWidth: '150px', zIndex: 1050 }}>
                            {chatRoom.type === 'Private' ? (
                                <button className="dropdown-item text-danger" onClick={handleDeleteChat}>
                                    Delete Chat
                                </button>
                            ) : chatRoom.ownerId === user?.id ? (
                                <>
                                    <label className="dropdown-item" style={{ cursor: 'pointer' }}>
                                        Change Picture
                                        <input type="file" className="d-none" accept="image/*" onChange={handlePictureUpload} />
                                    </label>
                                    <button className="dropdown-item" onClick={() => { setShowSettings(false); setShowMembersModal(true); }}>
                                        Manage Members
                                    </button>
                                    <div className="dropdown-divider"></div>
                                    <button className="dropdown-item text-danger" onClick={handleDeleteChat}>
                                        Delete Chat
                                    </button>
                                </>
                            ) : (
                                <button className="dropdown-item text-danger" onClick={handleLeaveChat}>
                                    Leave Chat
                                </button>
                            )}
                        </div>
                    )}
                </div>
            </div>

            {/* Messages Area */}
            <div className="flex-grow-1 overflow-auto p-4 bg-light" style={{ display: 'flex', flexDirection: 'column' }}>
                {messages.length === 0 && (
                    <div className="text-center text-muted my-auto">
                        No messages yet. Say hi!
                    </div>
                )}
                {messages.map((msg, index) => {
                    const isMe = msg.userId === user?.id;
                    return (
                        <div key={msg.id || index} className={`d-flex mb-3 ${isMe ? 'justify-content-end' : 'justify-content-start'}`}>
                            <div 
                                className={`p-3 shadow-sm ${isMe ? 'text-white' : 'bg-white border'}`} 
                                style={{ 
                                    maxWidth: '75%',
                                    borderRadius: isMe ? '16px 16px 4px 16px' : '16px 16px 16px 4px',
                                    background: isMe ? 'linear-gradient(135deg, #0d6efd, #0b5ed7)' : '#ffffff',
                                }}
                            >
                                {!isMe && (
                                    <div 
                                        className="small fw-bold mb-1 text-primary d-flex align-items-center gap-2"
                                        style={{ cursor: 'pointer' }}
                                        onClick={() => setSelectedUser(msg.user)}
                                    >
                                        <img 
                                            src={msg.user?.profilePictureUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(msg.user?.nickName || msg.user?.userName || 'User')}&background=random`} 
                                            alt="avatar" 
                                            className="rounded-circle shadow-sm" 
                                            style={{ width: '24px', height: '24px', objectFit: 'cover' }} 
                                            onError={(e) => { e.target.onerror = null; e.target.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(msg.user?.nickName || msg.user?.userName || 'User')}&background=random`; }}
                                        />
                                        {msg.user?.nickName || msg.user?.userName || 'User'}
                                    </div>
                                )}
                                {isMe && (
                                    <div className="small fw-bold mb-1 text-white-50 d-flex align-items-center justify-content-end gap-2">
                                        {msg.user?.nickName || msg.user?.userName || 'You'}
                                        <img 
                                            src={msg.user?.profilePictureUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(msg.user?.nickName || msg.user?.userName || 'User')}&background=random`} 
                                            alt="avatar" 
                                            className="rounded-circle shadow-sm" 
                                            style={{ width: '24px', height: '24px', objectFit: 'cover' }} 
                                            onError={(e) => { e.target.onerror = null; e.target.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(msg.user?.nickName || msg.user?.userName || 'User')}&background=random`; }}
                                        />
                                    </div>
                                )}
                                {msg.type === 'image' && msg.content && (
                                    <img src={`/uploads/${msg.content}`} alt="Attachment" className="img-fluid rounded mb-2" style={{ maxHeight: '250px', objectFit: 'cover' }} />
                                )}
                                {msg.type === 'text' && <div className="text-break">{msg.content}</div>}
                                <div className={`small mt-1 text-end ${isMe ? 'text-white-50' : 'text-muted'}`} style={{ fontSize: '0.75rem' }}>
                                    {new Date(msg.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                                </div>
                            </div>
                        </div>
                    );
                })}
                <div ref={messagesEndRef} />
            </div>

            {/* Input Area */}
            <div className="p-3 bg-white border-top">
                {imageFile && (
                    <div className="d-flex align-items-center mb-2 p-2 bg-light rounded border border-primary-subtle shadow-sm">
                        <span className="text-truncate small flex-grow-1 text-muted fw-medium">
                            📎 Attachment: {imageFile.name}
                        </span>
                        <button 
                            type="button" 
                            className="btn-close ms-2" 
                            style={{ fontSize: '0.8rem' }}
                            onClick={() => {
                                setImageFile(null);
                                if (fileInputRef.current) fileInputRef.current.value = '';
                            }}
                        />
                    </div>
                )}
                <form onSubmit={handleSendMessage} className="d-flex align-items-center gap-2">
                    <input 
                        type="file" 
                        ref={fileInputRef}
                        id="imageUpload"
                        className="d-none" 
                        onChange={(e) => setImageFile(e.target.files[0])}
                        accept="image/*"
                    />
                    <button 
                        type="button" 
                        className={`btn ${imageFile ? 'btn-primary' : 'btn-outline-primary'} rounded-circle shadow-sm`}
                        style={{ width: '40px', height: '40px', display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0 }}
                        onClick={() => fileInputRef.current?.click()}
                        title="Attach Image"
                    >
                        🖼️
                    </button>
                    <input 
                        type="text" 
                        className="form-control rounded-pill px-3 shadow-inner" 
                        placeholder="Type a message..." 
                        value={messageText}
                        onChange={(e) => setMessageText(e.target.value)}
                        style={{ height: '40px' }}
                    />
                    <button 
                        type="submit" 
                        className="btn btn-primary rounded-circle shadow-sm" 
                        disabled={!messageText.trim() && !imageFile}
                        style={{ width: '40px', height: '40px', display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0 }}
                    >
                        ➔
                    </button>
                </form>
            </div>

            {showMembersModal && (
                <ChatMembersModal 
                    chatId={id} 
                    isOwner={chatRoom.ownerId === user?.id}
                    onClose={() => setShowMembersModal(false)} 
                />
            )}

            {selectedUser && (
                <UserProfileViewModal 
                    user={selectedUser} 
                    onClose={() => setSelectedUser(null)} 
                />
            )}
        </div>
    );
};

export default ChatWindow;
