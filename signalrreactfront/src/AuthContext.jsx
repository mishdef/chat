import React, { createContext, useState, useEffect } from 'react';
import axios from 'axios';

const initialToken = localStorage.getItem('token');
if (initialToken) {
    axios.defaults.headers.common['Authorization'] = `Bearer ${initialToken}`;
}

export const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [token, setToken] = useState(localStorage.getItem('token') || null);

    useEffect(() => {
        if (token) {
            localStorage.setItem('token', token);
            axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
            axios.get('/api/user/me')
                .then(res => {
                    setUser(res.data);
                })
                .catch(err => {
                    console.error('Failed to fetch user profile', err);
                    // If unauthorized, clear token?
                    if (err.response && err.response.status === 401) {
                        localStorage.removeItem('token');
                        delete axios.defaults.headers.common['Authorization'];
                        setUser(null);
                        setToken(null);
                    }
                });
        } else {
            localStorage.removeItem('token');
            delete axios.defaults.headers.common['Authorization'];
            setUser(null);
        }
    }, [token]);

    const login = (newToken, userInfo) => {
        setToken(newToken);
        setUser(userInfo);
    };

    const logout = () => {
        setToken(null);
        setUser(null);
    };

    return (
        <AuthContext.Provider value={{ user, setUser, token, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
};
