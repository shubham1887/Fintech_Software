import React, { memo } from 'react';
import { makeStyles } from '@material-ui/core/styles';

const useStyles = makeStyles({
    float: {
        position: 'fixed',
        width: '40px',
        height: '40px',
        bottom: '12px',
        left: '12px',
        backgroundColor: '#00E676',
        color: '#FFF',
        borderRadius: '50px',
        textAlign: 'center',
        fontSize: '30px',
        boxShadow: '2px 2px 3px #999',
        zIndex: 100,
        '&:hover': {
            backgroundColor: '#FFF',
            color: '#00E676'
        }
    }
});

const WhatsAppFAB = ({ WhatsappURL }) => {
    const classesInline = useStyles();

    return (
        <a
            href={WhatsappURL}
            className={classesInline.float}
            target="_blank"
            rel="noopener noreferrer"
        >
            <i className="fab fa-whatsapp" style={{ marginTop: '6px' }}></i>
        </a>
    );
};

export default memo(WhatsAppFAB);
