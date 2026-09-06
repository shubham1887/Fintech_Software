import React, { memo, useState } from 'react';

const rightAlignText = {
    textAlign: 'right'
};
const NameAndRoleDisplay = () => {
    const [firmName] = useState(localStorage.getItem('FirmName'));
    const [mobileNumber] = useState(localStorage.getItem('MobileNumber'));
    const [userType] = useState(localStorage.getItem('UserType'));
    return (
        <div className={rightAlignText}>
            {firmName} - {mobileNumber} <br />
            Role : {userType}
        </div>
    );
};

export default memo(NameAndRoleDisplay);
