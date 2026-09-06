import React from 'react';
import IconButton from '@material-ui/core/IconButton';
import Tooltip from '@material-ui/core/Tooltip';
import HighlightOffIcon from '@material-ui/icons/HighlightOff';
import DoneAllIcon from '@material-ui/icons/DoneAll';
import { LOGOUT } from 'ConstComponents';
import { makeStyles } from '@material-ui/core/styles';
import { RefundAll as TRANSACATION_REFUND_ALL } from 'Components/Api/TransactionAPI';
import { SuccessAll as TRANSACATION_SUCCESS_ALL } from 'Components/Api/TransactionAPI';

const useStyles = makeStyles((theme) => ({
    iconContainer: {
        marginRight: '24px'
    }
}));

const CustomToolbarSelectForSuccessFailAll = ({
    selectedRows,
    displayData,
    setSelectedRows,
    primaryIndex,
    setLoadingOpen,
    setIsError,
    setSeverity,
    setResponseMessage
}) => {
    const classes = useStyles();

    const successAllClicked = async () => {
        var array = [];
        selectedRows.data.map((item) => {
            array.push({ ID: displayData[item.index].data[primaryIndex] });
        });

        setLoadingOpen(true);
        setIsError(false);

        var request = JSON.stringify(array);

        const response = await TRANSACATION_SUCCESS_ALL(request);

        displayMessage(response);
    };

    const refundAllClicked = async () => {
        var array = [];
        selectedRows.data.map((item) => {
            array.push({ ID: displayData[item.index].data[primaryIndex] });
        });
        setLoadingOpen(true);
        setIsError(false);
        var request = JSON.stringify(array);

        const response = await TRANSACATION_REFUND_ALL(request);

        displayMessage(response);
    };

    const displayMessage = (response) => {
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);

        setLoadingOpen(false);
        if (!response.status) setSeverity('error');
    };

    return (
        <div className={classes.iconContainer}>
            <Tooltip title={'Success All'}>
                <IconButton
                    className={classes.iconButton}
                    onClick={successAllClicked}
                >
                    <DoneAllIcon style={{ fill: 'green' }} />
                    Success
                </IconButton>
            </Tooltip>
            <Tooltip title={'Refund All'}>
                <IconButton
                    className={classes.iconButton}
                    onClick={refundAllClicked}
                >
                    <HighlightOffIcon style={{ fill: 'red' }} />
                    Fail
                </IconButton>
            </Tooltip>
        </div>
    );
};

export default CustomToolbarSelectForSuccessFailAll;
