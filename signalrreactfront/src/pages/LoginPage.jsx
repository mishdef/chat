import React, { useState, useContext } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import axios from 'axios';
import { AuthContext } from '../AuthContext';

const LoginPage = () => {
    const [loginName, setLoginName] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const { login } = useContext(AuthContext);
    const navigate = useNavigate();

    const handleLogin = async (e) => {
        e.preventDefault();
        try {
            const res = await axios.post('/api/auth/login', { login: loginName, password });
            if (res.data && res.data.data && res.data.data.token) {
                login(res.data.data.token, res.data.data.user);
                navigate('/');
            } else {
                setError('Login failed');
            }
        } catch (err) {
            setError(err.response?.data?.message || 'Invalid credentials');
        }
    };

    return (
        <div className="vh-100 d-flex align-items-center justify-content-center bg-light p-3">
            <div className="card shadow-sm border-0 rounded-4" style={{ maxWidth: '420px', width: '100%' }}>
                <div className="card-body p-4">
                    <h3 className="card-title text-center mb-4 fw-bold text-primary">Login</h3>
                    {error && <div className="alert alert-danger py-2 small">{error}</div>}
                    <form onSubmit={handleLogin}>
                        <div className="mb-3">
                            <label className="form-label fw-semibold text-muted small">Email or Username</label>
                            <input 
                                type="text" 
                                className="form-control rounded-pill px-3" 
                                value={loginName} 
                                onChange={(e) => setLoginName(e.target.value)} 
                                required 
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
                        <button type="submit" className="btn btn-primary w-100 rounded-pill py-2 fw-semibold shadow-sm">Login</button>
                    </form>
                    <div className="mt-4 text-center">
                        <Link to="/register" className="text-decoration-none text-secondary small">
                            Don't have an account? <span className="text-primary fw-semibold">Register here</span>
                        </Link>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default LoginPage;
