import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import { useHistory } from 'react-router';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import {
    Insert as DENOMINATION_ADD,
    Select as DENOMINATION_SELECT,
    Delete as DENOMINATION_DELETE,
    Update as DENOMINATION_UPDATE,
    SelectByID as DENOMINATION_SELECTBYID
} from 'Components/Api/DenominationAPI';

import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';

import ReactTable from 'Components/ReactTable/ReactTable';

import ToggleButton from '@material-ui/lab/ToggleButton';
import ToggleButtonGroup from '@material-ui/lab/ToggleButtonGroup';
import {
    Container,
    Grid,
    Paper,
    Button,
    CircularProgress,
    Box
} from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import {
    PLEASE_PROVIDE_VALID,
    MESSAGE_CLOSE_AFTER
} from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';
import FormHeader from 'Components/FormHeader';
const CustomContainer = HOCLoader(Container);

export const DenominationAdd = () => {
    const [Denominations, setDenomination] = useState([]);
    const history = useHistory();
    const [mainColumns, setMainColumns] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [isUpdate, setIsUpdate] = useState(false);
    const [ToggleValue, setToggleValue] = useState('Allowed');

    const [DenominationObj, setDenominationObj] = useState({
        DenominationMasterID: 0,
        Description: '',
        DenominationRange: '',
        IsStop: false
    });

    const formValidationSchema = Yup.object({
        DenominationRange: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'denomination'
        ),
        Description: Yup.string().required(PLEASE_PROVIDE_VALID + 'description')
    });

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) return;

        var requestObj = JSON.stringify(values);

        setLoadingOpen(true);

        setIsError(false);

        var response =
            values.DenominationMasterID === 0
                ? await DENOMINATION_ADD(requestObj)
                : await DENOMINATION_UPDATE(requestObj);

        setLoadingOpen(false);
        const { isAuthorized, status, data } = response;

        if (!isAuthorized) LOGOUT();

        setResponseMessage(data);
        setIsError(true);
        setSubmitting(false);

        if (!status) {
            setSeverity('error');
            return;
        }
        setSeverity('success');

        setIsUpdate(false);
        setDenominationObj({
            DenominationMasterID: 0,
            DenominationRange: '',
            Description: ''
        });
        resetForm(DenominationObj);
        setTimeout(() => {
            select();
        }, MESSAGE_CLOSE_AFTER);
    };

    const select = async () => {
        setLoadingOpen(true);
        const response = await DENOMINATION_SELECT();
        if (!response.isAuthorized) LOGOUT();
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
        setDenomination(list);
    };

    useEffect(() => {
        setLoadingOpen(true);
        select();
        createColumns();
    }, []);

    const onDeleteClick = (item) => {
        var RequestObj = { DenominationMasterID: item };
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
            select();
        }, MESSAGE_CLOSE_AFTER);
    };

    const SelectByDenominationMasterID = async (DenominationMasterID) => {
        let JSONObj = {
            DenominationMasterID: DenominationMasterID
        };
        setLoadingOpen(true);
        var request = JSON.stringify(JSONObj);

        const response = await DENOMINATION_SELECTBYID(request);
        setLoadingOpen(false);

        const { isAuthorized, status, data } = response;
        if (!isAuthorized) LOGOUT();

        if (!status) {
            setSeverity('error');
            return;
        }
        var JSONDATA = JSON.parse(data);
        setDenominationObj({
            DenominationMasterID: JSONDATA.DenominationMasterID,
            Description: JSONDATA.Description,
            DenominationRange: JSONDATA.DenominationRange,
            IsStop: JSONDATA.IsStop
        });
    };

    const Edit = async (DenominationMasterID) => {
        setIsUpdate(true);
        SelectByDenominationMasterID(DenominationMasterID);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'DenominationMasterID',
            srno: true
        });
        columns.push({
            label: 'Range',
            name: 'DenominationRange'
        });
        columns.push({
            label: 'Description',
            name: 'Description'
        });
        columns.push({
            label: 'Stop',
            name: 'IsStopStr'
        });

        columns.push({
            label: 'Edit',
            name: 'DenominationMasterID',
            edit: true,
            callback: (DenominationMasterID) => {
                Edit(DenominationMasterID);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'DenominationMasterID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });
        setMainColumns(columns);
    };
    const OnChange = (name, value, setFieldValue) => {
        setFieldValue(name, value);
    };
    const DenominationForm = ({ setFieldValue }) => {
        return (
            <Form>
                <Grid container spacing={3}>
                    <Grid item xs={8}>
                        <h2 className={classes.PaperHead_h2}>
                            Denominations Add
                        </h2>
                    </Grid>
                    <Grid item xs={2}>
                        <Box className={classes.flexReverse}>
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
                        </Box>
                    </Grid>
                    <Grid item xs={2}>
                        <Box className={classes.flexReverse}>
                            <Button
                                variant="contained"
                                color="primary"
                                className={classes.CustomPrimaryButtonStyle}
                                onClick={() => {
                                    history.push(
                                        '/DenominationDetails/DenominationMapping'
                                    );
                                }}
                            >
                                Mapping
                            </Button>
                        </Box>
                    </Grid>
                </Grid>
                <Grid item xs={12}>
                    <div style={{ color: 'green' }}>
                        Note : if you want fix amount then use " , " and range
                        for use " - " like (eg. 10, 20, 50-200, 300-500)
                    </div>
                </Grid>
                <Grid item xs={12}>
                    <Paper className={classes.PaperStyle}>
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <MaterialTextField
                                    label="Denomination Range"
                                    name="DenominationRange"
                                    type="text"
                                    placeholder="Denomination Range"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={9} lg={9}>
                                <MaterialTextField
                                    label="Description"
                                    name="Description"
                                    type="text"
                                    placeholder="Description"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <ToggleButtonGroup
                                    size="small"
                                    value={ToggleValue}
                                    name="IsStop"
                                    style={{
                                        display: 'initial',
                                        marginTop: '10px'
                                    }}
                                >
                                    <ToggleButton
                                        value={'Allowed'}
                                        onClick={(event, nextValue) => {
                                            setToggleValue(nextValue);
                                            OnChange(
                                                'IsStop',
                                                nextValue === 'STOP'
                                                    ? true
                                                    : false,
                                                setFieldValue
                                            );
                                        }}
                                    >
                                        Allowed
                                    </ToggleButton>
                                    <ToggleButton
                                        value={'STOP'}
                                        onClick={(event, nextValue) => {
                                            setToggleValue(nextValue);
                                            OnChange(
                                                'IsStop',
                                                nextValue === 'STOP'
                                                    ? true
                                                    : false,
                                                setFieldValue
                                            );
                                        }}
                                    >
                                        STOP
                                    </ToggleButton>
                                </ToggleButtonGroup>
                            </Grid>
                        </Grid>
                    </Paper>
                </Grid>

                <Grid className={classes.footerGrid}>
                    <Box className={classes.flexReverse}>
                        <div className={classes.SubmitWrapper}>
                            <Button
                                type="submit"
                                title={!isUpdate ? 'Add' : 'Update'}
                                className={classes.CustomPrimaryButtonStyle}
                                disabled={loadingOpen}
                            >
                                {!isUpdate ? 'Add' : 'Update'}
                            </Button>
                            {LoadingProgress}
                        </div>
                        <Button
                            type="reset"
                            title="Reset"
                            className={classes.CustomDangerButtonStyle}
                        >
                            Reset
                        </Button>
                    </Box>
                </Grid>
            </Form>
        );
    };
    const LoadingProgress = loadingOpen && (
        <CircularProgress size={24} className={classes.buttonProgress} />
    );

    const DenominationReport = (
        <ReactTable
            data={Denominations}
            columns={mainColumns}
            rowsPerPage={1000}
        />
    );
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
            <Formik
                enableReinitialize={true}
                initialValues={DenominationObj}
                validationSchema={formValidationSchema}
                onSubmit={handleSubmit}
                resetForm={DenominationObj}
            >
                {DenominationForm}
            </Formik>
            {isAlertOpen ? DeleteConfirmation : null}
            <h2 className={classes.PaperHead_h2}>Denominations List</h2>
            {DenominationReport}
        </CustomContainer>
    );
};
