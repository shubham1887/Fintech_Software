import React, { useEffect, useState } from 'react';
import { SelectByUserID as WALLET_SELECT } from 'Components/Api/WalletApi';
import { IconButton, Tooltip } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';

export const WalletBalance = (flag) => {
    const [rechBal, setRechBal] = useState(
        localStorage.getItem('RechargeBalance')
    );
    const [dMTBal, setDMTBal] = useState(localStorage.getItem('DMTBalance'));

    useEffect(() => {
        if (flag) SelectBalance();
    }, [2]);

    const SelectBalance = async () => {
        var response = await WALLET_SELECT();

        if (!response.isAuthorized) LOGOUT();

        let wallet = JSON.parse(response.data);

        setRechBal(wallet[0].Balance);
        setDMTBal(wallet[1].Balance);
    };
    return (
        <>
            <Tooltip title="Recharge" style={{ color: '#fff', fontSize: 15 }}>
                <IconButton>RCH : {rechBal}</IconButton>
            </Tooltip>
            <Tooltip title="DMT" style={{ color: '#fff', fontSize: 15 }}>
                <IconButton>DMT : {dMTBal}</IconButton>
            </Tooltip>
        </>
    );
};
