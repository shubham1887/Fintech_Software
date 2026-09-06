import React, { useEffect, useState } from 'react';
import { PREPAID_RECHARGE_CODE } from 'Components/Helper/Messages';
import { LOGOUT } from 'ConstComponents';
import { MESSAGE_CLOSE_AFTER } from 'Components/Helper/Messages';
import {
    Insert as INSERT,
    Delete as DELETE,
    Select as SELECT_ALL,
    SelectDenominationMappingByProviderIDAndOperatorID as SELECT_DENOMINATION_MAPPING_BY_PROVIDER_AND_OPERATORID
} from 'Components/Api/ProviderOperatorDenominationMappingApi';
import { CustomModal } from 'Components/Modal/CustomModal';
import { useHistory } from 'react-router';
import ReactTable from 'Components/ReactTable/ReactTable';
import { SelectAll as SERVICE_SELECT_ALL } from 'Components/Api/ServiceApi';
import {
    Container,
    FormControlLabel,
    Checkbox,
    Grid,
    Button,
    MenuItem,
    Select
} from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);
var ServiceID = PREPAID_RECHARGE_CODE;
export const DenominationMapping = () => {
    const [MappingList, setMappingList] = useState([]);
    const history = useHistory();
    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [Services, setServices] = useState([]);
    const [CurrentOperatorName, setCurrentOperatorName] = useState('');
    const [CurrentProviderName, setCurrentProviderName] = useState('');

    const [IsOpenDenominationModal, setIsOpenDenominationModal] = useState(
        false
    );
    const [DenominationList, setDenominationList] = useState([]);

    const [MappingObj, setMappingObj] = useState({
        ProviderOperatorDenominationMappingID: 0,
        ProviderID: 0,
        OperatorID: 0,
        DenominationMasterID: 0
    });
    const selectServices = async () => {
        const response = await SERVICE_SELECT_ALL();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
        setLoadingOpen(false);
    };

    const OnServiceChange = async (event, child) => {
        let ServiceIDChaild = child.props.value;
        ServiceID = ServiceIDChaild;
        Selectdata(ServiceID);
    };
    const ServiceItems = Services.map(({ ServiceID, ServiceName }, index) => (
        <MenuItem key={index} value={ServiceID}>
            {ServiceName}
        </MenuItem>
    ));
    const Selectdata = async (ServiceID) => {
        var requestObj = {
            ServiceID: ServiceID
        };
        setMappingList([]);
        var request = JSON.stringify(requestObj);
        setLoadingOpen(true);
        const response = await SELECT_ALL(request);
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
        setMappingList(list);
    };

    useEffect(() => {
        setLoadingOpen(true);
        selectServices();
        createColumns();
        Selectdata(ServiceID);
    }, []);

    // useEffect(() => {
    //     if (MappingList.length <= 0) return;
    // }, [MappingList]);

    const createColumns = () => {
        var columns = [];
        columns.push({
            label: '#',
            name: 'OperatorID',
            srno: true
        });
        columns.push({
            label: 'OperatorName',
            name: 'OperatorName'
        });
        columns.push({
            label: 'APIs',
            name: 'APIs',
            Labels: true,
            keyName: 'ProviderName',
            keyID: 'ProviderID',
            callback: (obj) => {
                SelectDenominationMappingByProviderIDAndOperatorID(obj);
            }
        });

        setMainColumns(columns);
    };

    const SelectDenominationMappingByProviderIDAndOperatorID = async ({
        PrimaryID,
        value
    }) => {
        let requestObj = {
            ProviderID: value,
            OperatorID: PrimaryID
        };

        var retOpObj = MappingList.filter(function (m) {
            return m.OperatorID === PrimaryID;
        });
        if (retOpObj !== undefined) {
            let mainObj = retOpObj[0];
            if (mainObj !== undefined) {
                setCurrentOperatorName(
                    !mainObj.OperatorName
                        ? CurrentOperatorName
                        : mainObj.OperatorName
                );

                var retAPIObj = mainObj.APIs.filter(function (m) {
                    return m.ProviderID === value;
                });

                setCurrentProviderName(retAPIObj[0].ProviderName);
            }
        }

        setMappingObj({
            ProviderOperatorDenominationMappingID: 0,
            ProviderID: value,
            OperatorID: PrimaryID,
            DenominationMasterID: 0
        });
        var request = JSON.stringify(requestObj);
        setLoadingOpen(true);
        const response = await SELECT_DENOMINATION_MAPPING_BY_PROVIDER_AND_OPERATORID(
            request
        );
        setLoadingOpen(false);
        const { isAuthorized, status, data } = response;
        if (!isAuthorized) LOGOUT();

        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);
            return;
        }

        setDenominationList(JSON.parse(data));

        setIsOpenDenominationModal(true);
    };

    const handleCheckboxChange = async (
        DenominationMasterID,
        ProviderOperatorDenominationMappingID
    ) => {
        MappingObj.DenominationMasterID = DenominationMasterID;
        MappingObj.ProviderOperatorDenominationMappingID = ProviderOperatorDenominationMappingID;

        var requestObj = JSON.stringify(MappingObj);
        setLoadingOpen(true);
        setIsError(false);

        var response =
            MappingObj.ProviderOperatorDenominationMappingID === 0
                ? await INSERT(requestObj)
                : await DELETE(requestObj);

        setLoadingOpen(false);
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
            SelectDenominationMappingByProviderIDAndOperatorID({
                PrimaryID: MappingObj.OperatorID,
                value: MappingObj.ProviderID
            });
        }, MESSAGE_CLOSE_AFTER);
        setMappingObj({
            ProviderOperatorDenominationMappingID: 0,
            ProviderID: 0,
            OperatorID: 0,
            DenominationMasterID: 0
        });
    };
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
            <Grid container spacing={1}>
                <Grid item xs={12} sm={12} md={3} lg={3} xl={3}>
                    <h2 className={classes.PaperHead_h2}>
                        Denominations Mapping
                    </h2>
                </Grid>

                <Grid item xs={12} sm={12} md={3} lg={3} xl={3}>
                    <Select
                        label="Service"
                        name="ServiceID"
                        title="Service"
                        value={ServiceID}
                        onChange={OnServiceChange}
                    >
                        <MenuItem key="0" value={0}>
                            Select Service
                        </MenuItem>
                        {ServiceItems}
                    </Select>
                </Grid>
                <Grid item xs={12} sm={12} md={3} lg={3} xl={3}>
                    <Button
                        variant="contained"
                        color="primary"
                        className={classes.CustomPrimaryButtonStyle}
                        onClick={() => {
                            history.push('/DenominationDetails');
                        }}
                    >
                        Denominations Details
                    </Button>
                </Grid>
                <Grid item xs={12} sm={12} md={3} lg={3} xl={3}>
                    <Button
                        variant="contained"
                        color="primary"
                        className={classes.CustomPrimaryButtonStyle}
                        onClick={() => {
                            history.push('/DenominationDetails/Denomination');
                        }}
                    >
                        New Denominations
                    </Button>
                </Grid>
            </Grid>

            <ReactTable
                data={MappingList}
                columns={mainColumns}
                rowsPerPage={100}
            />

            {IsOpenDenominationModal ? (
                <CustomModal
                    isOpen={IsOpenDenominationModal}
                    title={CurrentOperatorName + ' - ' + CurrentProviderName}
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsOpenDenominationModal(value);
                    }}
                >
                    <div>
                        <p>
                            <h5>Allowed Denominations</h5>
                            {DenominationList.map(
                                (
                                    {
                                        ProviderOperatorDenominationMappingID,
                                        DenominationMasterID,
                                        DenominationRange,
                                        IsStop
                                    },
                                    index
                                ) =>
                                    !IsStop ? (
                                        <FormControlLabel
                                            key={index}
                                            value={DenominationMasterID}
                                            control={
                                                <Checkbox
                                                    color="primary"
                                                    checked={
                                                        ProviderOperatorDenominationMappingID ===
                                                        0
                                                            ? false
                                                            : true
                                                    }
                                                    onChange={(e) => {
                                                        handleCheckboxChange(
                                                            e.target.value,
                                                            ProviderOperatorDenominationMappingID
                                                        );
                                                    }}
                                                />
                                            }
                                            label={DenominationRange}
                                            labelPlacement="end"
                                        />
                                    ) : null
                            )}
                        </p>
                        <p>
                            <h5>Stop Denominations</h5>
                            {DenominationList.map(
                                (
                                    {
                                        ProviderOperatorDenominationMappingID,
                                        DenominationMasterID,
                                        DenominationRange,
                                        IsStop
                                    },
                                    index
                                ) =>
                                    IsStop ? (
                                        <FormControlLabel
                                            key={index}
                                            value={DenominationMasterID}
                                            control={
                                                <Checkbox
                                                    color="primary"
                                                    checked={
                                                        ProviderOperatorDenominationMappingID ===
                                                        0
                                                            ? false
                                                            : true
                                                    }
                                                    onChange={(e) => {
                                                        handleCheckboxChange(
                                                            e.target.value,
                                                            ProviderOperatorDenominationMappingID
                                                        );
                                                    }}
                                                />
                                            }
                                            label={DenominationRange}
                                            labelPlacement="end"
                                        />
                                    ) : null
                            )}
                        </p>
                    </div>
                </CustomModal>
            ) : null}
        </CustomContainer>
    );
};
