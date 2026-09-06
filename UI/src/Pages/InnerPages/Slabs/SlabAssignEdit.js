import React, { useEffect, useState } from 'react';
import { useHistory } from 'react-router';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';

import { SelectByID as USER_MAPPING_SELECT_BY_ID } from 'Components/Api/UserSlabMappingApi';
import { Update as SLAB_MAPPING_UPDATE } from 'Components/Api/UserSlabMappingApi';
import { SelectUserForDropDown as USER_SELECT_LIST } from 'Components/Api/UsersApi';
import { Select as SLAB_SELECT } from 'Components/Api/SlabApi';

import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';

import {
    Container,
    Grid,
    MenuItem,
    Paper,
    Button,
    Hidden
} from '@material-ui/core';
import CircularProgress from '@material-ui/core/CircularProgress';

import Box from '@material-ui/core/Box';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_SELECT_VALID
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);
export const SlabAssignEdit = () => {
    const classes = commonStyle();
    const history = useHistory();

    const [Users, setUsers] = useState([]);
    const [Slabs, setSlabs] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [formObject, setFormObject] = useState({
        SlabID: 0,
        UserSlabMappingID: 0,
        SlabName: '',
        UserID: 0,
        FirmName: ''
    });
    useEffect(() => {
        selectUsers();
        selectSlabs();
        SelectByID();
    }, []);

    const selectSlabs = async () => {
        const response = await SLAB_SELECT();
        if (!response.isAuthorized) {
            LOGOUT();
        }

        var data = JSON.parse(response.data);
        setSlabs(data);
    };

    const selectUsers = async () => {
        const response = await USER_SELECT_LIST();
        if (!response.isAuthorized) {
            LOGOUT();
        }

        var data = JSON.parse(response.data);
        setUsers(data);
    };

    const SelectByID = async () => {
        let URL = window.location.pathname;
        var res = URL.split('/');

        let primaryID = res[res.length - 1];

        let JSONObj = {
            UserSlabMappingID: primaryID
        };
        setLoadingOpen(true);
        var request = JSON.stringify(JSONObj);

        const response = await USER_MAPPING_SELECT_BY_ID(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        var data = JSON.parse(response.data);

        setFormObject({
            SlabID: data.SlabID,
            SlabName: data.SlabName,
            UserID: data.UserID,
            FirmName: data.FirmName,
            UserSlabMappingID: data.UserSlabMappingID
        });
    };
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setLoading(true);
        setIsError(false);
        setSubmitting(false);

        var response = await SLAB_MAPPING_UPDATE(requestObj);
        if (!response.isAuthorized) {
            LOGOUT();
        }

        setIsError(true);
        setSubmitting(true);

        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setSeverity('error');
            return;
        }

        resetForm(formObject);

        setTimeout(() => {
            history.push('/Slabs/Assign');
        }, MESSAGE_CLOSE_AFTER);
    };
    const UserSlabValidationSchema = Yup.object({
        SlabID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'Slab')
            .required(PLEASE_SELECT_VALID + 'Slab'),
        UserID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'User')
            .required(PLEASE_SELECT_VALID + 'User')
    });
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
                initialValues={formObject}
                validationSchema={UserSlabValidationSchema}
                onSubmit={submithandle}
                resetForm={formObject}
            >
                {(formProps) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>Slab Edit </h2>
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialSelect
                                                label="Users"
                                                name="UserID"
                                                readOnly={true}
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select User
                                                </MenuItem>
                                                {Users.map((Item) => (
                                                    <MenuItem
                                                        key={Item.UserID}
                                                        value={Item.UserID}
                                                    >
                                                        {Item.FirmName}
                                                    </MenuItem>
                                                ))}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={6}
                                            lg={6}
                                        >
                                            <MaterialSelect
                                                label="Slab"
                                                name="SlabID"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Slab
                                                </MenuItem>
                                                {Slabs.map((Item) => (
                                                    <MenuItem
                                                        key={Item.SlabID}
                                                        value={Item.SlabID}
                                                    >
                                                        {Item.Name}
                                                    </MenuItem>
                                                ))}
                                            </MaterialSelect>

                                            <Hidden xsUp>
                                                <MaterialSelect
                                                    label="UserSlabMappingID"
                                                    name="UserSlabMappingID"
                                                >
                                                    <MenuItem key="0" value="0">
                                                        Select Slab
                                                    </MenuItem>
                                                    {Slabs.map((Item) => (
                                                        <MenuItem
                                                            key={
                                                                Item.UserSlabMappingID
                                                            }
                                                            value={
                                                                Item.UserSlabMappingID
                                                            }
                                                        >
                                                            {
                                                                Item.UserSlabMappingID
                                                            }
                                                        </MenuItem>
                                                    ))}
                                                </MaterialSelect>
                                            </Hidden>
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>

                            <Grid className={classes.footerGrid}>
                                <Box className={classes.flexReverse}>
                                    <div className={classes.SubmitWrapper}>
                                        <Button
                                            type="submit"
                                            title="Upadate"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loading}
                                        >
                                            Update
                                        </Button>
                                        {loading && (
                                            <CircularProgress
                                                size={24}
                                                className={
                                                    classes.buttonProgress
                                                }
                                            />
                                        )}
                                    </div>
                                    <Button
                                        type="reset"
                                        title="Reset"
                                        className={
                                            classes.CustomDangerButtonStyle
                                        }
                                    >
                                        Reset
                                    </Button>
                                </Box>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
        </CustomContainer>
    );
};
