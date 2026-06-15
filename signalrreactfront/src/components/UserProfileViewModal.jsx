import React from 'react';

const UserProfileViewModal = ({ user, onClose }) => {
    if (!user) return null;

    return (
        <div className="modal show d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }} onClick={onClose}>
            <div className="modal-dialog modal-dialog-centered" onClick={(e) => e.stopPropagation()}>
                <div className="modal-content shadow border-0" style={{ borderRadius: '1rem', overflow: 'hidden' }}>
                    <div className="bg-primary pt-5 pb-3 position-relative d-flex justify-content-center align-items-end" style={{ height: '120px' }}>
                        <button 
                            type="button" 
                            className="btn-close btn-close-white position-absolute top-0 end-0 m-3" 
                            onClick={onClose}
                        ></button>
                        <img 
                            src={user.profilePictureUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(user.nickName || user.userName || 'User')}&background=random`} 
                            alt="Avatar" 
                            className="rounded-circle shadow bg-white border border-4 border-white"
                            style={{ width: '100px', height: '100px', objectFit: 'cover', marginBottom: '-50px' }}
                            onError={(e) => { e.target.onerror = null; e.target.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(user.nickName || user.userName || 'User')}&background=random`; }}
                        />
                    </div>
                    <div className="modal-body text-center pt-5 pb-4 px-4">
                        <h4 className="fw-bold mt-2 mb-1">{user.nickName || user.userName}</h4>
                        <p className="text-muted mb-4">{user.email}</p>
                        <button type="button" className="btn btn-outline-secondary rounded-pill px-4" onClick={onClose}>
                            Close
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default UserProfileViewModal;
