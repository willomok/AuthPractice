import React, { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom'; 

const ProfilePage: React.FC = () => {
  const location = useLocation();
  const [profile, setProfile] = useState<{ name: string; email: string } | null>(null);

  useEffect(() => {

    const queryParams = new URLSearchParams(location.search);
    const name = queryParams.get('name');
    const email = queryParams.get('email');

    if (name && email) {
      setProfile({ name, email });
    }
  }, [location.search]);

  return (
    <div style={{ textAlign: 'center', marginTop: '50px' }}>
      {profile ? (
        <div>
          <h2>Welcome, {profile.name}</h2>
          <p>Email: {profile.email}</p>
        </div>
      ) : (
        <div>Loading...</div>
      )}
    </div>
  );
};

export default ProfilePage;
