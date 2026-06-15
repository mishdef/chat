import React, { useContext, useEffect, useState } from 'react';
import { useNavigate, Routes, Route, Link, useLocation } from 'react-router-dom';
import * as signalR from '@microsoft/signalr';
import { AuthContext } from '../AuthContext';
import ChatSidebar from '../components/ChatSidebar';
import ChatWindow from '../components/ChatWindow';
import ProfileSettingsModal from '../components/ProfileSettingsModal';

const ChatLayout = () => {
    const { token, logout, user } = useContext(AuthContext);
    const navigate = useNavigate();
    const [showProfile, setShowProfile] = useState(false);
    const location = useLocation();
    const [connection, setConnection] = useState(null);

    const isChatActive = location.pathname.startsWith('/chat/');

    useEffect(() => {
        if (!token) {
            navigate('/login');
            return;
        }

        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl("/chatHub", { 
                accessTokenFactory: () => token 
            })
            .withAutomaticReconnect()
            .build();

        newConnection.start()
            .then(() => {
                console.log('Connected to SignalR globally');
            })
            .catch(err => console.error('Global SignalR Connection Error: ', err));

        setConnection(newConnection);

        return () => {
            newConnection.stop();
        };
    }, [token, navigate]);

    if (!token) return null;

    return (
        <div className="d-flex flex-column vh-100 bg-light">
            {/* Top Navbar */}
            <nav className="navbar navbar-expand-lg navbar-dark bg-primary px-3 shadow-sm">
                <Link className="navbar-brand fw-bold" to="/">OnlineChat</Link>
                <div className="ms-auto d-flex align-items-center">
                    <span 
                        className="text-white me-3 fw-semibold d-flex align-items-center gap-2" 
                        style={{ cursor: 'pointer' }}
                        onClick={() => setShowProfile(true)}
                        title="Profile Settings"
                    >
                        <img 
                            src={user?.profilePictureUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(user?.nickName || user?.userName || 'User')}&background=random`} 
                            alt="Profile" 
                            className="rounded-circle border border-white"
                            style={{ width: '32px', height: '32px', objectFit: 'cover' }}
                            onError={(e) => { e.target.onerror = null; e.target.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(user?.nickName || user?.userName || 'User')}&background=random`; }}
                        />
                        {user?.nickName || user?.userName || 'Profile'}
                    </span>
                    <button className="btn btn-outline-light btn-sm" onClick={logout}>Logout</button>
                </div>
            </nav>

            {showProfile && <ProfileSettingsModal onClose={() => setShowProfile(false)} />}

            <div className="d-flex flex-grow-1 overflow-hidden">
                {/* Sidebar */}
                <div className={`bg-white border-end flex-column chat-sidebar-container ${isChatActive ? 'd-none d-md-flex' : 'd-flex'}`}>
                    <ChatSidebar connection={connection} />
                </div>

                {/* Main Content Area */}
                <div className={`flex-grow-1 bg-light flex-column position-relative ${isChatActive ? 'd-flex' : 'd-none d-md-flex'}`}>
                    <Routes>
                        <Route path="/" element={
                            <div className="d-flex align-items-center justify-content-center h-100 text-muted p-4 text-center">
                                <h5>Select a chat to start messaging</h5>
                            </div>
                        } />
                        <Route path="/chat/:id" element={<ChatWindow connection={connection} />} />
                    </Routes>
                </div>
            </div>
        </div>
    );
};

export default ChatLayout;
