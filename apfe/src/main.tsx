import React from 'react';
import ReactDOM from 'react-dom/client';
import { GoogleOAuthProvider } from '@react-oauth/google';
import App from './App';

ReactDOM.createRoot(document.getElementById('root') as HTMLElement).render(
    <GoogleOAuthProvider clientId="537728867195-261nvnn37kcan13j9a1f9c91ukh12qrr.apps.googleusercontent.com">
        <React.StrictMode>
            <App />
        </React.StrictMode>
    </GoogleOAuthProvider>
);
