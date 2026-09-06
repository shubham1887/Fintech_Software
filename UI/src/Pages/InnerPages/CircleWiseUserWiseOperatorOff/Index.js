import React, { useState, useEffect } from 'react';
import { Container, Grid } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import {
    SelectAllUserwiseCircleOffList as SELECT_USER_WISE_CIRCLE,
    DeleteByUserID as USERWISE_CIRCLE_DELETE,
    DeleteAll as DELETE_BY_All
} from 'Components/Api/CircleWiseUserWiseOperatorOffAPI';

import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import ReactTable from 'Components/ReactTable/ReactTable';
import { useHistory } from 'react-router';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import Button from '@material-ui/core/Button';
import HOCLoader from 'Components/Loader/HOCLoader';
import { MESSAGE_CLOSE_AFTER } from 'Components/Helper/Messages';
const CustomContainer = HOCLoader(Container);

export const UserwiseCircleIndex = () => {
    const history = useHistory();
    const classes = commonStyle();
    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const [datalist, setDatalist] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [TakeCount, setTakeCount] = useState(10);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [isAlertAllOpen, setIsAlertAllOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    useEffect(() => {
        setLoadingOpen(true);
        Select();
    }, []);

    const Select = async () => {
        setDatalist([]);
        setResponseMessage('');
        createColumns();
        const response = await SELECT_USER_WISE_CIRCLE();
        setLoadingOpen(false);

        const { isAuthorized, status, data } = response;

        if (!isAuthorized) LOGOUT();
        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);
            return;
        }
        var list = JSON.parse(data);
        setDatalist(list);
    };

    const ResetAllUserwise = async () => {
        setLoadingOpen(true);
        const response = await DELETE_BY_All();
        if (!response.isAuthorized) LOGOUT();
        const { isAuthorized, status, data } = response;

        if (!isAuthorized) LOGOUT();

        setResponseMessage(data);
        setIsError(true);

        if (!status) {
            setSeverity('error');
            return;
        }
        setSeverity('success');
        setTimeout(() => {
            Select();
        }, MESSAGE_CLOSE_AFTER);
    };
    const onDeleteClick = (item) => {
        var RequestObj = { UserID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };
    const DeleteAllClick = () => {
        setIsAlertAllOpen(true);
    };

    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);
        setLoadingOpen(true);
        const response = await USERWISE_CIRCLE_DELETE(jsonReq);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        const { isAuthorized, status, data } = response;

        if (!isAuthorized) LOGOUT();

        setResponseMessage(data);
        setIsError(true);

        if (!status) {
            setSeverity('error');
            return;
        }
        setSeverity('success');
        Select();
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserID',
            srno: true
        });
        columns.push({
            label: 'User',
            name: 'UserName'
        });

        columns.push({
            label: 'Operator',
            name: 'OperatorName'
        });

        columns.push({
            label: 'Off Circle',
            name: 'StateList',
            array: true,
            keyName: 'Name'
        });
        columns.push({
            label: 'Delete',
            name: 'UserID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });
        setMainColumns(columns);
    };

    const DeleteConfirmation = (
        <ConfirmAlert
            isOpen={isAlertOpen}
            title="Delete Confirmation"
            message="Are you sure to delete userwise circle off."
            request={confirmAlertRequest}
            positiveButtonClick={(request) => {
                setIsAlertOpen(false);
                Delete(request);
            }}
            negativeButtonClick={() => {
                setIsAlertOpen(false);
            }}
        />
    );
    const DeleteAllConfirmation = (
        <ConfirmAlert
            isOpen={isAlertAllOpen}
            title="Delete All Confirmation"
            message="Are you sure to delete all userwise circle off."
            positiveButtonClick={() => {
                setIsAlertAllOpen(false);
                ResetAllUserwise();
            }}
            negativeButtonClick={() => {
                setIsAlertAllOpen(false);
            }}
        />
    );
    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={(alertState) => {
                setIsError(alertState);
            }}
        >
            <Grid container spacing={2} style={{ marginBottom: '15px' }}>
                <Grid item xs={12} sm={12} md={9} lg={9}>
                    <h2 className={classes.PaperHead_h2}>
                        Userwise Down Circle Details
                    </h2>
                </Grid>
                <Grid item xs={12} sm={12} md={1} lg={1}>
                    <Button
                        variant="contained"
                        color="primary"
                        title="Delete All"
                        className={classes.CustomPrimaryButtonStyle}
                        onClick={() => {
                            DeleteAllClick();
                        }}
                    >
                        Delete All
                    </Button>
                </Grid>
                <Grid item xs={12} sm={12} md={2} lg={2}>
                    <Button
                        variant="contained"
                        color="primary"
                        title="New Userwise Circle Add"
                        className={classes.CustomPrimaryButtonStyle}
                        onClick={() => {
                            history.push(
                                'UserwiseCircleIndex/UserWiseCircleAdd'
                            );
                        }}
                    >
                        New Userwise Circle Add
                    </Button>
                </Grid>
            </Grid>
            {isAlertOpen ? DeleteConfirmation : null}
            {isAlertAllOpen ? DeleteAllConfirmation : null}

            <ReactTable
                title={'Userwise Circle Details'}
                data={datalist}
                columns={mainColumns}
                rowsPerPage={TakeCount}
            />
        </CustomContainer>
    );
};
