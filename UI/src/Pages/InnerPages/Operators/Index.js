import React, { useState, useEffect, useRef } from 'react';
import {
    Container,
    Grid,
    Button,
    MenuItem,
    Box,
    Select
} from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { SelectAll as OPERATOR_SELECT_ALL } from 'Components/Api/OperatorsApi';
import { SelectAll as SERVICE_SELECT_ALL } from 'Components/Api/ServiceApi';
import { Activate as OPERATOR_ACTIVATE } from 'Components/Api/OperatorsApi';
import { SelectIndex as OPERATOR_SELECT_INDEX } from 'Components/Api/OperatorsApi';

import ReactTable from 'Components/ReactTable/ReactTable';
import { useHistory } from 'react-router';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import excel from 'Assets/Images/excel.svg';
import { Icon } from '@material-ui/core';

import HOCLoader from 'Components/Loader/HOCLoader';
import { PREPAID_RECHARGE_CODE } from 'Components/Helper/Messages';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

var ServiceID = PREPAID_RECHARGE_CODE;
export const Operators = () => {
    const history = useHistory();
    const classes = commonStyle();
    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const [Operators, setOperators] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [TakeCount, setTakeCount] = useState(10);
    const [Services, setServices] = useState([]);
    const formRef = useRef();

    useEffect(() => {
        setLoadingOpen(true);
        selectServices();
        createColumns();
        selectOperators(ServiceID, false);
    }, []);

    const activate = async (item, value) => {
        var jsonObj = {
            OperatorID: item,
            IsActive: value
        };

        var request = JSON.stringify(jsonObj);

        setLoadingOpen(true);
        setIsError(false);
        const response = await OPERATOR_ACTIVATE(request);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        selectOperators(ServiceID, false);
    };

    const selectOperators = async (ServiceID, IsExcelDownload) => {
        var requestObj = {
            ServiceID: ServiceID,
            IsExcelDownload: IsExcelDownload
        };
        setOperators([]);
        var request = JSON.stringify(requestObj);
        const { isAuthorized, status, data } = await OPERATOR_SELECT_ALL(
            request
        );
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setSeverity('error');
            setResponseMessage(data);

            setIsError(true);
            return;
        }

        var d = JSON.parse(data);

        setOperators(d);
    };
    const selectOperatorsForExcel = async (ServiceID, IsExcelDownload) => {
        var requestObj = {
            ServiceID: ServiceID,
            IsExcelDownload: IsExcelDownload
        };
        setOperators([]);

        var request = JSON.stringify(requestObj);
        const { isAuthorized, status, data } = await OPERATOR_SELECT_INDEX(
            request
        );
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setSeverity('error');
            setResponseMessage(data);

            setIsError(true);
            return;
        }

        var d = JSON.parse(data);
        setOperators(d.list);

        if (d.IsExcelDownload) {
            downloadFile(d.ReportPath, 'Operators_Index');
        }
    };
    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
        document.removeChild(a);
    }

    const Edit = async (request) => {
        history.push('/Operators/Edit/' + request);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'OperatorID',
            srno: true,
            deactived: true
        });
        columns.push({
            label: '#',
            name: 'ServiceID',
            hide: true
        });
        columns.push({
            label: 'Operator',
            name: 'OperatorDetail',
            deactived: true
        });

        columns.push({
            label: 'Image',
            name: 'Path',
            Image: true,
            deactived: true
        });
        columns.push({
            label: 'Code',
            name: 'Opcode',
            deactived: true
        });
        columns.push({
            label: 'Amount',
            name: 'AmountDetail',
            deactived: true
        });
        columns.push({
            label: 'Repeat(min)',
            name: 'RepeatRechargeInMinute',
            deactived: true
        });

        // columns.push({
        //     label: 'SequenceLogic',
        //     name: 'SequenceLogic',
        //     deactived: true
        // });
        // columns.push({
        //     label: 'BusinessModel',
        //     name: 'BusinessModel',
        //     deactived: true
        // });

        columns.push({
            label: 'BBPS Config.',
            name: 'IsBBPS',
            setUp: true,
            primaryKeyIndex: 0,
            secondaryKeyIndex: 1,
            callback: (OperatorID, ServiceID) => {
                var redirectObj = {
                    ServiceID: ServiceID,
                    OperatorID: OperatorID
                };

                history.push('/BBPSConfiguration/Index', redirectObj);
            }
        });
        columns.push({
            label: 'UI Sequence',
            name: 'UISequence',
            deactived: true
        });

        columns.push({
            label: 'Active',
            name: 'IsActive',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                activate(id, value);
            }
        });
        columns.push({
            label: 'Edit',
            name: 'OperatorID',
            edit: true,
            callback: (value) => {
                Edit(value);
            }
        });
        columns.push({
            label: 'IsActive',
            name: 'IsActive',
            hide: true
        });

        setMainColumns(columns);
    };

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
        selectOperators(ServiceID, false);
    };

    const ExcelDownload = async () => {
        selectOperatorsForExcel(ServiceID, true);
    };

    const ServiceItems = Services.map(({ ServiceID, ServiceName }, index) => (
        <MenuItem key={index} value={ServiceID}>
            {ServiceName}
        </MenuItem>
    ));

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
            <Grid container spacing={2}>
                <Grid item xs={12} sm={12} md={6} lg={6}>
                    <h2 className={classes.PaperHead_h2}>Operator Details</h2>
                </Grid>
                <Grid item xs={12} sm={12} md={1} lg={1}>
                    <Box style={{ textAlign: 'center' }}>
                        <Icon
                            className={classes.CustomImageButtonStyle}
                            disabled={loadingOpen}
                            classes={{ root: classes.iconRoot }}
                            onClick={(values) => {
                                ExcelDownload();
                            }}
                        >
                            <img
                                alt=""
                                className={classes.imageIcon}
                                src={excel}
                            />
                        </Icon>
                    </Box>
                </Grid>
                <Grid
                    item
                    xs={12}
                    sm={12}
                    md={2}
                    lg={2}
                    style={{ textAlign: 'center' }}
                >
                    <Box>
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
                    </Box>
                </Grid>
                <Grid item xs={12} sm={12} md={2} lg={2}>
                    <PrimaryButton
                        variant="contained"
                        color="primary"
                        title="Add Operator"
                        className={classes.CustomPrimaryButtonStyle}
                        onClick={() => {
                            history.push('/Operators/Add');
                        }}
                    >
                        Add Operator
                    </PrimaryButton>
                </Grid>
                <Grid item xs={6}></Grid>
                <Grid item xs={6}>
                    <Box className={classes.flexReverse}></Box>
                </Grid>
            </Grid>

            <ReactTable
                title={'Operator Details'}
                data={Operators}
                columns={mainColumns}
                rowsPerPage={TakeCount}
            />
        </CustomContainer>
    );
};
