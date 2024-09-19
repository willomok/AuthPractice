import React from 'react';

const LoginPage: React.FC = () => {
  const handleGoogleLogin = () => {
    window.location.href = 'http://localhost:5031/api/account/login-google'; // Redirect to your backend's Google login endpoint
  };

  return (
    <div style={{ textAlign: 'center', marginTop: '50px' }}>
      <h1>Login with Google</h1>
      <button onClick={handleGoogleLogin}>Login with Google</button>
    </div>
  );
};

export default LoginPage;
