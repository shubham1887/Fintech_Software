import React, { useRef, useState } from 'react';
import FileCopyIcon from '@material-ui/icons/FileCopy';

import IconButton from '@material-ui/core/IconButton';

const inputStyle = {
    color: 'green',
    border: 'none',
    fontWeight: 'bold',
    width: '91px',
    textAlign: 'right'
};

const CopyToClipBoardInput = ({ text }) => {
    const inputRef = useRef(null);
    const [copied, setCopied] = useState(false);
    const copyCodeToClipboard = (e) => {
        inputRef.current.select();
        document.execCommand('copy');
        setCopied(true);
    };

    return (
        <span>
            <input ref={inputRef} value={text} readOnly style={inputStyle} />
            {document.queryCommandSupported('copy') && (
                <IconButton
                    color="primary"
                    onClick={copyCodeToClipboard}
                    title={copied ? 'Copied' : 'Click to Copy'}
                >
                    <FileCopyIcon />
                </IconButton>
            )}
        </span>
    );
};

export default CopyToClipBoardInput;
