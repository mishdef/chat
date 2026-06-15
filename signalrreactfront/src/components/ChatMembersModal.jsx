import React, { useState, useEffect } from 'react';
import axios from 'axios';

const ChatMembersModal = ({ chatId, isOwner, onClose }) => {
    const [members, setMembers] = useState([]);
    const [searchQuery, setSearchQuery] = useState('');
    const [searchResults, setSearchResults] = useState([]);
    const [loading, setLoading] = useState(false);

    const fetchMembers = async () => {
        try {
            const res = await axios.get(`/api/chat/${chatId}/members`);
            if (res.data && res.data.data) {
                setMembers(res.data.data);
            }
        } catch (err) {
            console.error('Failed to fetch members', err);
        }
    };

    useEffect(() => {
        fetchMembers();
    }, [chatId]);

    const handleSearch = async (e) => {
        e.preventDefault();
        if (!searchQuery.trim()) return;
        setLoading(true);
        try {
            const res = await axios.get(`/api/user/search?query=${searchQuery}`);
            setSearchResults(res.data || []);
        } catch (err) {
            console.error('Search failed', err);
        } finally {
            setLoading(false);
        }
    };

    const handleAddMember = async (userId) => {
        try {
            await axios.post(`/api/chat/${chatId}/members`, [userId], {
                headers: { 'Content-Type': 'application/json' }
            });
            fetchMembers();
            setSearchResults(prev => prev.filter(u => u.id !== userId));
        } catch (err) {
            console.error('Failed to add member', err);
            alert(err.response?.data?.message || 'Failed to add member');
        }
    };

    const handleRemoveMember = async (userId) => {
        try {
            await axios.delete(`/api/chat/${chatId}/members`, {
                data: [userId],
                headers: { 'Content-Type': 'application/json' }
            });
            fetchMembers();
        } catch (err) {
            console.error('Failed to remove member', err);
            alert(err.response?.data?.message || 'Failed to remove member');
        }
    };

    return (
        <div className="modal show d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
            <div className="modal-dialog modal-dialog-centered modal-dialog-scrollable">
                <div className="modal-content shadow">
                    <div className="modal-header border-bottom-0">
                        <h5 className="modal-title fw-bold">Chat Members</h5>
                        <button type="button" className="btn-close" onClick={onClose}></button>
                    </div>
                    <div className="modal-body p-0">
                        {isOwner && (
                            <>
                                <div className="p-3 bg-light border-bottom">
                                    <form onSubmit={handleSearch} className="d-flex gap-2">
                                        <input 
                                            type="text" 
                                            className="form-control form-control-sm" 
                                            placeholder="Search users to add..." 
                                            value={searchQuery}
                                            onChange={(e) => setSearchQuery(e.target.value)}
                                        />
                                        <button type="submit" className="btn btn-primary btn-sm" disabled={loading}>
                                            {loading ? '...' : 'Search'}
                                        </button>
                                    </form>
                                </div>

                                {loading && (
                                    <div className="p-4 text-center">
                                        <div className="spinner-border text-primary" role="status">
                                            <span className="visually-hidden">Loading...</span>
                                        </div>
                                    </div>
                                )}

                                {!loading && searchResults.length > 0 && (
                                    <div className="p-3 border-bottom bg-white">
                                        <h6 className="text-muted small mb-2">Search Results</h6>
                                        <ul className="list-group">
                                            {searchResults.map(u => (
                                                <li key={u.id} className="list-group-item d-flex justify-content-between align-items-center py-2">
                                                    <span>{u.userName} <small className="text-muted">({u.email})</small></span>
                                                    <button className="btn btn-success btn-sm" onClick={() => handleAddMember(u.id)}>Add</button>
                                                </li>
                                            ))}
                                        </ul>
                                    </div>
                                )}
                                
                                {!loading && searchResults.length === 0 && searchQuery.trim() !== '' && (
                                    <div className="p-3 text-center text-muted border-bottom bg-white">
                                        <small>No users found</small>
                                    </div>
                                )}
                            </>
                        )}

                        <div className="p-3">
                            <h6 className="text-muted small mb-2">Current Members ({members.length})</h6>
                            <ul className="list-group list-group-flush">
                                {members.map(m => (
                                    <li key={m.id} className="list-group-item px-0 d-flex justify-content-between align-items-center">
                                        <div>
                                            <span className="fw-semibold">{m.userName || m.nickname}</span>
                                            <br/>
                                            <small className="text-muted">{m.email}</small>
                                        </div>
                                        {isOwner && (
                                            <button className="btn btn-outline-danger btn-sm" onClick={() => handleRemoveMember(m.id)}>Remove</button>
                                        )}
                                    </li>
                                ))}
                            </ul>
                        </div>
                    </div>
                    <div className="modal-footer border-top-0">
                        <button type="button" className="btn btn-secondary w-100" onClick={onClose}>Close</button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ChatMembersModal;
