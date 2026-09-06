import React, { useState, useEffect } from 'react';
import { Container, Grid } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { SelectDenominationMapping as SELECT_ALL } from 'Components/Api/ProviderOperatorDenominationMappingApi';
import { Delete as DENOMINATION_DELETE } from 'Components/Api/ProviderOperatorDenominationMappingApi';
import { Update as RANGE_UPDATE } from 'Components/Api/DenominationAPI';

import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import ReactTable from 'Components/ReactTable/ReactTable';
import { useHistory } from 'react-router';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import HOCLoader from 'Components/Loader/HOCLoader';
import { MESSAGE_CLOSE_AFTER } from 'Components/Helper/Messages';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const DenominationDetails = () => {
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
        const response = await SELECT_ALL();
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
        var RequestObj = { ProviderOperatorDenominationMappingID: item };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);
        setLoadingOpen(true);
        const response = await DENOMINATION_DELETE(jsonReq);
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
    const UpdateClick = async (row) => {
        let jsonObj = {
            DenominationMasterID: row[1],
            DenominationRange: row[4],
            Description: row[5]
        };

        let request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);
        const response = await RANGE_UPDATE(request);

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        setTimeout(() => {
            Select();
        }, MESSAGE_CLOSE_AFTER);
    };
    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ProviderOperatorDenominationMappingID',
            srno: true
        });
        columns.push({
            label: '#',
            name: 'DenominationMasterID',
            hide: true
        });

        columns.push({
            label: 'Provider',
            name: 'Provider'
        });
        columns.push({
            label: 'Operator',
            name: 'Operator'
        });
        columns.push({
            label: 'Amount Range',
            name: 'DenominationRange',
            editable: true
        });
        columns.push({
            label: 'Description',
            name: 'Description',
            editable: true
        });
        columns.push({
            label: 'Type',
            name: 'IsStopStr'
        });
        columns.push({
            label: 'Update',
            name: 'Update',
            save: true,
            buttonTitle: 'Update',
            callback: (row) => {
                UpdateClick(row);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'ProviderOperatorDenominationMappingID',
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
            message="Are you sure to delete denomination."
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
                <Grid item xs={12} sm={12} md={8} lg={8}>
                    <h2 className={classes.PaperHead_h2}>
                        Denomination Details
                    </h2>
                </Grid>
                <Grid item xs={12} sm={12} md={2} lg={2}>
                    <PrimaryButton
                        variant="contained"
                        color="primary"
                        title="New Denomination"
                        className={classes.CustomPrimaryButtonStyle}
                        onClick={() => {
                            history.push('/DenominationDetails/Denomination');
                        }}
                    >
                        New Denomination
                    </PrimaryButton>
                </Grid>

                <Grid item xs={12} sm={12} md={2} lg={2}>
                    <SecondaryButton
                        variant="contained"
                        color="primary"
                        title="Mapping"
                        className={classes.CustomPrimaryButtonStyle}
                        onClick={() => {
                            history.push(
                                '/DenominationDetails/DenominationMapping'
                            );
                        }}
                    >
                        Mapping
                    </SecondaryButton>
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
