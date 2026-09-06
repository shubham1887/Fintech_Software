import React, { useEffect, useState, memo } from 'react';
import { CLIENT_NAME } from 'Config';

const CopyRight = memo(() => {
    const [cName, setCName] = useState('');

    async function fetchCName() {
        setCName(await CLIENT_NAME());
    }
    useEffect(() => {
        fetchCName();
    }, []);

    return (
        <span className="mt-5 mb-3 text-muted ">
            &copy;{new Date().getFullYear()} {cName}
        </span>
    );
});

export default CopyRight;
