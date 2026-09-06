import React, { useState, useEffect } from 'react';
import { Container, Grid } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { SelectCircleSwitching as SELECT_CIRCLE_SWITCHING } from 'Components/Api/CircleSwitchingAPI';
import { Delete as CIRCLE_DELETE } from 'Components/Api/CircleSwitchingAPI';

import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import ReactTable from 'Components/ReactTable/ReactTable';
import { useHistory } from 'react-router';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import HOCLoader from 'Components/Loader/HOCLoader';
import { MESSAGE_CLOSE_AFTER } from 'Components/Helper/Messages';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const CircleIndex = () => {
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
        const response = await SELECT_CIRCLE_SWITCHING();
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
    const onDeleteClick = (item) => {
        var RequestObj = { OperatorID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);
        setLoadingOpen(true);
        const response = await CIRCLE_DELETE(jsonReq);
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

        setTimeout(() => {
            Select();
        }, MESSAGE_CLOSE_AFTER);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ProviderOperatorCircleMasterID',
            srno: true
        });
        columns.push({
            label: 'OperatorID',
            name: 'OperatorID',
            hide: true
        });

        columns.push({
            label: 'Operator',
            name: 'Operator'
        });
        columns.push({
            label: 'Provider',
            name: 'ProviderName'
        });
        columns.push({
            label: 'Circles',
            name: 'Circles',
            MultiSelectCheckbox: true
        });
        columns.push({
            label: 'Delete',
            name: 'OperatorID',
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
            message="Are you sure to delete circle switching."
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
                        Circle switching Details
                    </h2>
                </Grid>
                <Grid item xs={12} sm={12} md={2} lg={2}>
                    <PrimaryButton
                        variant="contained"
                        color="primary"
                        title="New Circle Add"
                        className={classes.CustomPrimaryButtonStyle}
                        onClick={() => {
                            history.push('/CircleSwitching');
                        }}
                    >
                        New Circle Add
                    </PrimaryButton>
                </Grid>
            </Grid>
            {isAlertOpen ? DeleteConfirmation : null}
            <ReactTable
                title={'Denomination Mapping Details'}
                data={datalist}
                columns={mainColumns}
                rowsPerPage={TakeCount}
            />
        </CustomContainer>
    );
};
