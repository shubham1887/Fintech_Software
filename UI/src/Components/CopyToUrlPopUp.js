import React, { useRef, useState } from 'react';
import FileCopyIcon from '@material-ui/icons/FileCopy';

import IconButton from '@material-ui/core/IconButton';

const inputStyle = {
    color: 'green',
    border: 'none',
    fontWeight: 'bold',
    width: '50vw',
    textAlign: 'left'
};

const CopyToUrlPopUp = ({ text }) => {
    const inputRef = useRef(null);
    const [copied, setCopied] = useState(false);
    const copyCodeToClipboard = (e) => {
        inputRef.current.select();
        document.execCommand('copy');
        setCopied(true);
    };

    return (
        <>
            {document.queryCommandSupported('copy') && (
                <IconButton
                    color="primary"
                    onClick={copyCodeToClipboard}
                    title={copied ? 'Copied' : 'Click to Copy'}
                >
                    <FileCopyIcon />
                </IconButton>
            )}
            <input ref={inputRef} value={text} readOnly style={inputStyle} />
        </>
    );
};

export default CopyToUrlPopUp;
