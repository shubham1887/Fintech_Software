import {
    Container,
    MenuItem,
    Grid,
    Button,
    Box,
    CircularProgress
} from '@material-ui/core';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import {
    UpdateUserCommission as UPDATE_USER_COMMISSION,
    SelectUserCommissionByUserID as SELECT_USER_COMMISSION_BY_USER_ID
} from 'Components/Api/UsersApi';
import HOCLoader from 'Components/Loader/HOCLoader';
import React, { useState, useEffect } from 'react';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { Formik, Form } from 'formik';
import { MESSAGE_CLOSE_AFTER } from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
const CustomContainer = HOCLoader(Container);

const UserCommission = (props) => {
    let { selectedUserID, getModalClose } = props;
    const classes = commonStyle();
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const customContainerCallback = (alertState) => {
        setIsError(alertState);
    };
    const [formObject, setFormObject] = useState({
        Comm: '0.0',
        CommissionCategory: '',
        UserID: selectedUserID
    });

    useEffect(() => {
        SelectCommissionByUserID(selectedUserID);
    }, [selectedUserID]);

    const SelectCommissionByUserID = async (userID) => {
        let request = { UserID: userID };
        setLoadingOpen(true);
        const {
            isAuthorized,
            status,
            data
        } = await SELECT_USER_COMMISSION_BY_USER_ID(JSON.stringify(request));
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();
        if (!status) {
            setSeverity('error');
            setResponseMessage(data);
            setIsError(true);

            return;
        }

        let d = JSON.parse(data);

        setFormObject({
            ...formObject,
            Comm: d.Comm,
            CommissionCategory: d.CommissionCategory
        });
    };

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);

        setLoadingOpen(true);

        const { isAuthorized, status, data } = await UPDATE_USER_COMMISSION(
            JSON.stringify(values)
        );
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        setResponseMessage(data);
        setIsError(true);
        if (!status) {
            setSeverity('error');
            return;
        }
        setSeverity('success');
        setSubmitting(false);
        setTimeout(() => {
            getModalClose(false);
        }, MESSAGE_CLOSE_AFTER);
    };
    const UserCommissionSchema = Yup.object({
        Comm: Yup.number()
            .required('Please enter valid Comm')
            .max(5, 'Please enter commission less than 5')
            .min(-5, 'Please enter commission more than -5')
    });
    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={customContainerCallback}
        >
            <Formik
                enableReinitialize={true}
                initialValues={formObject}
                validationSchema={UserCommissionSchema}
                onSubmit={handleSubmit}
                resetForm={formObject}
            >
                <Form>
                    <Grid item xs={12}>
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <MaterialTextField
                                    label="Commission"
                                    name="Comm"
                                    type="text"
                                    placeholder="Commission"
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <MaterialSelect
                                    label="CommCategory"
                                    name="CommissionCategory"
                                >
                                    <MenuItem key="0" value="0">
                                        Select Commission Category
                                    </MenuItem>

                                    <MenuItem
                                        key={'Commission'}
                                        value={'Commission'}
                                    >
                                        {'Commission'}
                                    </MenuItem>
                                    <MenuItem
                                        key={'Surcharge'}
                                        value={'Surcharge'}
                                    >
                                        {'Surcharge'}
                                    </MenuItem>
                                </MaterialSelect>
                            </Grid>
                        </Grid>
                        <Grid className={classes.footerGrid}>
                            <Box className={classes.flexReverse}>
                                <div className={classes.SubmitWrapper}>
                                    <Button
                                        type="submit"
                                        title="Update"
                                        className={
                                            classes.CustomPrimaryButtonStyle
                                        }
                                        disabled={loadingOpen}
                                    >
                                        Update
                                    </Button>
                                    {loadingOpen && (
                                        <CircularProgress
                                            size={24}
                                            className={classes.buttonProgress}
                                        />
                                    )}
                                </div>
                                <Button
                                    type="reset"
                                    variant="contained"
                                    color="secondary"
                                    title="Reset"
                                    className={classes.CustomDangerButtonStyle}
                                >
                                    Reset
                                </Button>
                            </Box>
                        </Grid>
                    </Grid>
                </Form>
            </Formik>
        </CustomContainer>
    );
};

export default UserCommission;
