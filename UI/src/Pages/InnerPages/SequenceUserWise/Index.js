import React, { useState, useEffect } from 'react';
import { Container, Grid } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { SelectAllUserwiseSequenceList as SELECT_USER_WISE_SEQUENCE } from 'Components/Api/OperatorSequenceUserWiseApi';
import { DeleteByUserID as USERWI_SESEQUENCE_DELETE } from 'Components/Api/OperatorSequenceUserWiseApi';

import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import ReactTable from 'Components/ReactTable/ReactTable';
import { useHistory } from 'react-router';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import Button from '@material-ui/core/Button';
import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);

export const UserwiseSequenceIndex = () => {
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
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    useEffect(() => {
        setLoadingOpen(true);
        createColumns();
        Select();
    }, []);

    const Select = async () => {
        setLoadingOpen(true);
        setDatalist([]);
        const response = await SELECT_USER_WISE_SEQUENCE();
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
    const onDeleteClick = (row) => {
        var RequestObj = { UserID: row[0], OperatorID: row[3] };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);
        setLoadingOpen(true);
        const response = await USERWI_SESEQUENCE_DELETE(jsonReq);
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
            label: 'Service',
            name: 'ServiceName'
        });
        columns.push({
            label: 'OperatorID',
            name: 'OperatorID',
            hide: true
        });

        columns.push({
            label: 'Operator',
            name: 'OperatorName'
        });

        columns.push({
            label: 'Provider',
            name: 'ProviderList',
            array: true,
            keyName: 'ProviderName'
        });
        columns.push({
            label: 'Delete',
            name: 'UserID',
            buttonWithTitle: true,
            buttonTitle: 'Delete',
            callback: (row) => {
                onDeleteClick(row);
            }
        });
        setMainColumns(columns);
    };

    const DeleteConfirmation = (
        <ConfirmAlert
            isOpen={isAlertOpen}
            title="Delete Confirmation"
            message="Are you sure to delete userwise switching."
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
                <Grid item xs={12} sm={12} md={10} lg={10}>
                    <h2 className={classes.PaperHead_h2}>
                        User-wise switching Details
                    </h2>
                </Grid>
                <Grid item xs={12} sm={12} md={2} lg={2}>
                    <Button
                        variant="contained"
                        color="primary"
                        title="New Sequence Add"
                        className={classes.CustomPrimaryButtonStyle}
                        onClick={() => {
                            history.push('/Sequence/UserWise');
                        }}
                    >
                        New User-wise Add
                    </Button>
                </Grid>
            </Grid>
            {isAlertOpen ? DeleteConfirmation : null}
            <ReactTable
                title={'User-wise switching Details'}
                data={datalist}
                columns={mainColumns}
                rowsPerPage={TakeCount}
            />
        </CustomContainer>
    );
};
