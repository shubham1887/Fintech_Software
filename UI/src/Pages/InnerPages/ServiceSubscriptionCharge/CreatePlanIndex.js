import React, { useState, useRef, useEffect } from 'react';
import { Formik } from 'formik';
import { Container, Grid, Box } from '@material-ui/core';
import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import { SelectPlan as SELECT_PLAN } from 'Components/Api/SubscriptionPlanAPI';
import { DeleteSubsciption as SUBSCRIPTION_PLAN_DELETE } from 'Components/Api/SubscriptionPlanAPI';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { useHistory } from 'react-router';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
const CustomContainer = HOCLoader(Container);

export const CreatePlanIndex = () => {
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [list, setList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const formRef = useRef();

    const history = useHistory();

    const [FormObj, setFormObj] = useState({
        Name: ''
    });

    useEffect(() => {
        setLoading(true);
        createColumns();
        select();
    }, []);

    const select = async () => {
        setLoading(true);
        const response = await SELECT_PLAN();
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);

        setList(data);
    };

    const Edit = async (request) => {
        history.push('/Plan/Edit/' + request);
    };

    const onDeleteClick = (item) => {
        var RequestObj = {
            ServiceSubscriptionCreatePlanID: item[0]
        };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);
        setLoading(true);
        const response = await SUBSCRIPTION_PLAN_DELETE(jsonReq);
        setLoading(false);
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

        select();
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ServiceSubscriptionCreatePlanMasterID',
            srno: true
        });

        columns.push({
            label: 'Subscription Plan',
            name: 'PlanName'
        });

        columns.push({
            label: 'Price (₹)',
            name: 'Amount'
        });

        columns.push({
            label: 'Service',
            name: 'Services',
            array: true,
            keyName: 'ServiceName'
        });

        columns.push({
            label: 'Image',
            name: 'ImageURL',
            Image: true,
            deactived: true
        });

        columns.push({
            label: 'Action',
            name: 'ServiceSubscriptionCreatePlanMasterID',
            buttonTitle: 'Edit',
            update: true,
            callback: (values) => {
                Edit(values[0]);
            }
        });

        columns.push({
            label: 'Delete',
            name: 'ServiceSubscriptionCreatePlanMasterID',
            delete: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });
        setMainColumns(columns);
    };

    return (
        <CustomContainer
            isLoading={loading}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={(alertState) => {
                setIsError(alertState);
            }}
        >
            <Formik
                innerRef={formRef}
                enableReinitialize={true}
                initialValues={FormObj}
                //onSubmit={submitHandle}
                resetForm={FormObj}
            >
                {({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Grid
                            container
                            spacing={2}
                        >

                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <Box className={classes.flexReverse}>
                                    <PrimaryButton
                                        variant="contained"
                                        color="primary"
                                        title="Create Plan"
                                        className={classes.CustomPrimaryButtonStyle}
                                        onClick={() => {
                                            history.push('/CreatePlan');
                                        }}
                                    />
                                </Box>
                            </Grid>

                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <ReactTable
                                    data={list}
                                    columns={mainColumns}
                                    rowsPerPage={100}
                                />
                            </Grid>
                        </Grid>
                    );
                }}
            </Formik>

            {isAlertOpen && (
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
            )}
        </CustomContainer>
    );
};
