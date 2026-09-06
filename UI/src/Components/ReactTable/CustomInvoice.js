import React from 'react';
import { IconButton } from '@material-ui/core';
import GetAppIcon from '@material-ui/icons/GetApp';
import SendIcon from '@material-ui/icons/Send';
export const CustomInvoice = ({ value, callback }) => {
    const handleSendClick = () => {
        callback({ ...value, IsSend: true, IsDownload: false });
    };
    const handleDownloadClick = () => {
        callback({ ...value, IsDownload: true, IsSend: false });
    };
    return (
        <React.Fragment>
            <IconButton
                value={'DOWNLOAD'}
                color="primary"
                aria-label="Download"
                component="span"
                title={'DOWNLOAD ' + value.Type + ' Invoice'}
                onClick={handleDownloadClick}
            >
                <GetAppIcon />
            </IconButton>
            <IconButton
                disabled={true}
                value={'SEND'}
                color="primary"
                aria-label="sendMail"
                component="span"
                onClick={handleSendClick}
                title={'SEND ' + value.Type + ' Invoice to user'}
            >
                <SendIcon />
            </IconButton>
        </React.Fragment>
    );
};
