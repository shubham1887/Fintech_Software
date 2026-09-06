import React, { useState, useEffect } from 'react';
import { LOGO_URL, CLIENT_NAME, LOGO_WIDTH } from 'Config';

const Logo = () => {
    const [logoURL, setLogoURL] = useState('');

    async function fetchLogoURL() {
        setLogoURL(await LOGO_URL());
    }

    const [cName, setCName] = useState('');
    async function fetchCName() {
        setCName(await CLIENT_NAME());
    }

    useEffect(() => {
        fetchCName();

        fetchLogoURL();
    }, []);
    return (
        <img
            src={logoURL}
            alt={cName + ' logo'}
            style={{ height: '50px', width: LOGO_WIDTH }}
        />
    );
};

export default Logo;
