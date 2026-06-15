import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import axios from 'axios';

const RegisterPage = () => {
    const [email, setEmail] = useState('');
    const [nickname, setNickname] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const navigate = useNavigate();

    const handleRegister = async (e) => {
        e.preventDefault();
        try {
            await axios.post('/api/auth/register', { email, nickname, password });
            navigate('/login');
        } catch (err) {
            const responseData = err.response?.data;
            if (responseData?.errors?.exceptionMessage) {
                setError(responseData.errors.exceptionMessage);
            } else if (err.response?.status === 400 && responseData?.errors && typeof responseData.errors === 'object' && !Array.isArray(responseData.errors)) {
                const validationErrors = Object.values(responseData.errors).flat().join(', ');
                setError(validationErrors);
            } else if (responseData?.message) {
                setError(responseData.message);
            } else {
                setError('Registration failed');
            }
        }
    };

    return (
        <div className="vh-100 d-flex align-items-center justify-content-center bg-light p-3">
            <div className="card shadow-sm border-0 rounded-4" style={{ maxWidth: '420px', width: '100%' }}>
                <div className="card-body p-4">
                    <h3 className="card-title text-center mb-4 fw-bold text-primary">Register</h3>
                    {error && <div className="alert alert-danger py-2 small">{error}</div>}
                    <form onSubmit={handleRegister}>
                        <div className="mb-3">
                            <label className="form-label fw-semibold text-muted small">Email</label>
                            <input 
                                type="email" 
                                className="form-control rounded-pill px-3" 
                                value={email} 
                                onChange={(e) => setEmail(e.target.value)} 
                                required 
                            />
                        </div>
                        <div className="mb-3">
                            <label className="form-label fw-semibold text-muted small">Nickname</label>
                            <input 
                                type="text" 
                                className="form-control rounded-pill px-3" 
                                value={nickname} 
                                onChange={(e) => setNickname(e.target.value)} 
                                required 
                                minLength={3}
                                maxLength={20}
                            />
                        </div>
                        <div className="mb-4">
                            <label className="form-label fw-semibold text-muted small">Password</label>
                            <input 
                                type="password" 
                                className="form-control rounded-pill px-3" 
                                value={password} 
                                onChange={(e) => setPassword(e.target.value)} 
                                required 
                            />
                        </div>
                        <button type="submit" className="btn btn-primary w-100 rounded-pill py-2 fw-semibold shadow-sm">Register</button>
                    </form>
                    <div className="mt-4 text-center">
                        <Link to="/login" className="text-decoration-none text-secondary small">
                            Already have an account? <span className="text-primary fw-semibold">Login here</span>
                        </Link>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default RegisterPage;
