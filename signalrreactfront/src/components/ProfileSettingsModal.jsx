import React, { useState, useContext, useRef } from 'react';
import axios from 'axios';
import { AuthContext } from '../AuthContext';

const ProfileSettingsModal = ({ onClose }) => {
    const { user, setUser } = useContext(AuthContext);
    const [nickname, setNickname] = useState(user?.nickName || '');
    const [username, setUsername] = useState(user?.userName || '');
    const [email, setEmail] = useState(user?.email || '');
    const [avatarFile, setAvatarFile] = useState(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [successMsg, setSuccessMsg] = useState(null);
    const fileInputRef = useRef(null);

    const handleSave = async (e) => {
        e.preventDefault();
        setLoading(true);
        setError(null);
        setSuccessMsg(null);

        try {
            let updatedUser = { ...user };

            if (avatarFile) {
                const formData = new FormData();
                formData.append('image', avatarFile);
                
                const avatarRes = await axios.post('/api/user/upload-avatar', formData, {
                    headers: { 'Content-Type': 'multipart/form-data' }
                });
                updatedUser.profilePictureUrl = avatarRes.data.profilePictureUrl;
            }

            const updates = {};
            if (nickname !== (user?.nickName || '')) updates.NickName = nickname;
            if (username !== (user?.userName || '')) updates.UserName = username;
            if (email !== (user?.email || '')) updates.Email = email;

            if (Object.keys(updates).length > 0) {
                await axios.put('/api/user/update', updates);
                if (updates.NickName) updatedUser.nickName = nickname;
                if (updates.UserName) updatedUser.userName = username;
                if (updates.Email) updatedUser.email = email;
            }

            setUser(updatedUser);
            setSuccessMsg("Profile updated successfully!");
            setTimeout(() => {
                onClose();
            }, 1500);

        } catch (err) {
            console.error(err);
            setError("Failed to update profile.");
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="modal show d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
            <div className="modal-dialog modal-dialog-centered">
                <div className="modal-content shadow">
                    <div className="modal-header border-bottom-0 pb-0">
                        <h5 className="modal-title fw-bold">Profile Settings</h5>
                        <button type="button" className="btn-close" onClick={onClose}></button>
                    </div>
                    <form onSubmit={handleSave}>
                        <div className="modal-body">
                            {error && <div className="alert alert-danger">{error}</div>}
                            {successMsg && <div className="alert alert-success">{successMsg}</div>}
                            
                            <div className="text-center mb-4">
                                <div className="position-relative d-inline-block">
                                    <img 
                                        src={avatarFile ? URL.createObjectURL(avatarFile) : (user?.profilePictureUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(user?.nickName || user?.userName || 'User')}&background=random`)} 
                                        alt="Avatar" 
                                        className="rounded-circle shadow-sm"
                                        style={{ width: '120px', height: '120px', objectFit: 'cover', cursor: 'pointer' }}
                                        onClick={() => fileInputRef.current?.click()}
                                        onError={(e) => { e.target.onerror = null; e.target.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(user?.nickName || user?.userName || 'User')}&background=random`; }}
                                    />
                                    <button 
                                        type="button"
                                        className="btn btn-primary btn-sm rounded-circle position-absolute bottom-0 end-0 shadow"
                                        onClick={() => fileInputRef.current?.click()}
                                        style={{ width: '32px', height: '32px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}
                                    >
                                        ✏️
                                    </button>
                                    <input 
                                        type="file" 
                                        ref={fileInputRef} 
                                        className="d-none" 
                                        accept="image/*"
                                        onChange={(e) => setAvatarFile(e.target.files[0])}
                                    />
                                </div>
                            </div>

                            <div className="mb-3">
                                <label className="form-label text-muted small fw-bold">Email</label>
                                <input 
                                    type="email" 
                                    className="form-control" 
                                    value={email} 
                                    onChange={(e) => setEmail(e.target.value)} 
                                />
                            </div>

                            <div className="mb-3">
                                <label className="form-label text-muted small fw-bold">Username</label>
                                <input 
                                    type="text" 
                                    className="form-control" 
                                    value={username} 
                                    onChange={(e) => setUsername(e.target.value)} 
                                />
                            </div>

                            <div className="mb-3">
                                <label className="form-label text-muted small fw-bold">Nickname</label>
                                <input 
                                    type="text" 
                                    className="form-control" 
                                    value={nickname}
                                    onChange={(e) => setNickname(e.target.value)}
                                    placeholder="Enter your nickname"
                                />
                            </div>
                        </div>
                        <div className="modal-footer border-top-0 pt-0">
                            <button type="button" className="btn btn-secondary" onClick={onClose}>Cancel</button>
                            <button type="submit" className="btn btn-primary" disabled={loading}>
                                {loading ? 'Saving...' : 'Save Changes'}
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default ProfileSettingsModal;
