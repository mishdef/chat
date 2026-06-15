import React, { useState, useContext } from 'react';
import axios from 'axios';
import { AuthContext } from '../AuthContext';

const ProfileModal = ({ onClose }) => {
    const { user } = useContext(AuthContext);
    const [nickname, setNickname] = useState(user?.nickname || '');
    const [profilePictureUrl, setProfilePictureUrl] = useState(user?.profilePictureUrl || '');
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        setMessage('');
        try {
            await axios.put('/api/user/update', { nickname, profilePictureUrl });
            setMessage('Profile updated successfully!');
        } catch (err) {
            setMessage('Failed to update profile.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="modal show d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
            <div className="modal-dialog modal-dialog-centered">
                <div className="modal-content shadow">
                    <div className="modal-header border-bottom-0">
                        <h5 className="modal-title fw-bold">Update Profile</h5>
                        <button type="button" className="btn-close" onClick={onClose}></button>
                    </div>
                    <div className="modal-body">
                        {message && <div className={`alert ${message.includes('success') ? 'alert-success' : 'alert-danger'}`}>{message}</div>}
                        <form onSubmit={handleSubmit}>
                            <div className="mb-3">
                                <label className="form-label">Nickname</label>
                                <input 
                                    type="text" 
                                    className="form-control" 
                                    value={nickname} 
                                    onChange={(e) => setNickname(e.target.value)} 
                                />
                            </div>
                            <div className="mb-3">
                                <label className="form-label">Profile Picture URL</label>
                                <input 
                                    type="url" 
                                    className="form-control" 
                                    value={profilePictureUrl} 
                                    onChange={(e) => setProfilePictureUrl(e.target.value)} 
                                />
                            </div>
                            <div className="d-flex justify-content-end gap-2 mt-4">
                                <button type="button" className="btn btn-light" onClick={onClose} disabled={loading}>Close</button>
                                <button type="submit" className="btn btn-primary" disabled={loading}>
                                    {loading ? 'Saving...' : 'Save Changes'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ProfileModal;
