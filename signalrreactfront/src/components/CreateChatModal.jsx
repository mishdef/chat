import React, { useState } from 'react';
import axios from 'axios';

const CreateChatModal = ({ onClose, onSuccess }) => {
    const [chatName, setChatName] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!chatName.trim()) return;

        setLoading(true);
        try {
            // Note: chatRoomName is passed as a string in the body.
            // The backend expects [FromBody] string chatRoomName, which requires sending it as a quoted string in JSON.
            await axios.post('/api/chat/create', `"${chatName}"`, {
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            onSuccess();
        } catch (err) {
            setError(err.response?.data?.message || 'Failed to create chat');
            setLoading(false);
        }
    };

    return (
        <div className="modal show d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
            <div className="modal-dialog modal-dialog-centered">
                <div className="modal-content shadow">
                    <div className="modal-header border-bottom-0">
                        <h5 className="modal-title fw-bold">Create New Chat</h5>
                        <button type="button" className="btn-close" onClick={onClose}></button>
                    </div>
                    <div className="modal-body">
                        {error && <div className="alert alert-danger">{error}</div>}
                        <form onSubmit={handleSubmit}>
                            <div className="mb-3">
                                <label className="form-label">Chat Name</label>
                                <input 
                                    type="text" 
                                    className="form-control" 
                                    value={chatName} 
                                    onChange={(e) => setChatName(e.target.value)}
                                    placeholder="Enter chat room name"
                                    required 
                                />
                            </div>
                            <div className="d-flex justify-content-end gap-2 mt-4">
                                <button type="button" className="btn btn-light" onClick={onClose} disabled={loading}>Cancel</button>
                                <button type="submit" className="btn btn-primary" disabled={loading}>
                                    {loading ? 'Creating...' : 'Create Chat'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default CreateChatModal;
