import React, { useRef, useState } from 'react';
import { Typography } from '@material-ui/core';

const IFSCThroughBankAddress = ({ Branch, IFSCAddress }) => {
    return (
        <span>
            <Typography>
                <span
                    style={{
                        color: 'black'
                    }}
                >
                    <b>BRANCH: {Branch}</b>
                </span>
            </Typography>
            <br></br>
            <Typography>
                <span
                    style={{
                        color: 'black'
                    }}
                >
                    <b>ADDRESS: {IFSCAddress}</b>
                </span>
            </Typography>
        </span>
    );
};

export default IFSCThroughBankAddress;
