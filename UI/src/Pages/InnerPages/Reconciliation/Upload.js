import React, { useState, useEffect } from 'react';
import { Formik, Form } from 'formik';

import * as Yup from 'yup';

import { LOGOUT } from 'ConstComponents';
import { Upload } from 'Components/Api/ReconciliationApi';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';

import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import {
    Container,
    Grid,
    Paper,
    MenuItem,
    TextField,
    CircularProgress
} from '@material-ui/core';
import ReactTable from 'Components/ReactTable/ReactTable';
import { useHistory } from 'react-router';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';

import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';

const CustomContainer = HOCLoader(Container);

export const ReconciliationUpload = () => {
    const classes = commonStyle();
    const history = useHistory();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = React.useState(false);
    const [severity, setSeverity] = useState('success');
    const [providerList, setProviderList] = useState([]);
    const [document, setDocument] = useState('');
    const [mainColumns, setMainColumns] = useState([]);
    const [itemList, setItemList] = useState([]);
    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }
        var requestObj = JSON.stringify(values);

        setLoading(true);
        setIsError(false);

        const { isAuthorized, status, data } = await Upload(requestObj);
        if (!isAuthorized) LOGOUT();
        setLoading(false);

        setSeverity('success');
        if (!status) {
            setResponseMessage(data);

            setSeverity('error');
            setIsError(true);

            return;
        }

        setItemList(JSON.parse(data));
        resetForm(FormObj);
    };

    var FormObj = {
        FromDate: new Date(),
        ToDate: new Date(),
        ProviderID: 0,
        ExcelDocument: {
            Name: '',
            Size: '',
            Type: '',
            Data: ''
        }
    };

    const FormValidationSchema = Yup.object({
        ProviderID: Yup.string().required(PLEASE_PROVIDE_VALID + 'provider')
        // ExcelDocument: Yup.object().shape({
        //     Type: Yup.string()
        //         .required(PLEASE_PROVIDE_VALID + 'provider')
        //         .matches(/(\.xlsx|\.xlsm|\.xlsb)$/i,"Please select only excel file")
        // })
    });

    const selectProvider = async () => {
        const response = await PROVIDER_DROP_DOWN();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setProviderList(data);
    };

    useEffect(() => {
        createColumns();

        selectProvider();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ReconciliationSettingID',
            srno: true
        });

        columns.push({
            label: 'FirmName',
            name: 'FirmName'
        });

        columns.push({
            label: 'DOC',
            name: 'DOC'
        });

        columns.push({
            label: 'Operator',
            name: 'Operator'
        });
        columns.push({
            label: 'ConsumerNumber',
            name: 'Number'
        });

        columns.push({
            label: 'Amount',
            name: 'Amount'
        });

        columns.push({
            label: 'Status',
            name: 'DBStatus',
            customStatus: true
        });
        columns.push({
            label: 'Excel Status',
            name: 'ExcelStatus',
            customStatus: true
        });
        setMainColumns(columns);
    };

    const ProviderItems = providerList.map(({ ProviderID, ProviderName }) => (
        <MenuItem key={ProviderID} value={ProviderID}>
            {ProviderName}
        </MenuItem>
    ));

    const UploadForm = (formProps) => {
        return (
            <Form style={{ marginBottom: '15px' }}>
                <Grid container spacing={2} style={{ marginBottom: '15px' }}>
                    <Grid item xs={12} sm={12} md={10} lg={10}>
                        <h2 className={classes.PaperHead_h2}>Upload Excel</h2>
                    </Grid>
                    <Grid item xs={12} sm={12} md={2} lg={2}>
                        <PrimaryButton
                            variant="contained"
                            color="primary"
                            title="Settings"
                            className={classes.CustomPrimaryButtonStyle}
                            onClick={() => {
                                history.push('/ReconciliationSettings');
                            }}
                        >
                            Settings
                        </PrimaryButton>
                    </Grid>
                </Grid>

                <Grid item xs={12}>
                    <Paper className={classes.PaperStyle}>
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialDatePickerField
                                    name="FromDate"
                                    label="From Date"
                                    maxDate={new Date()}
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialDatePickerField
                                    name="ToDate"
                                    label="To Date"
                                    maxDate={new Date()}
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialSelect
                                    name="ProviderID"
                                    label="ProviderID"
                                    title="ProviderID"
                                >
                                    <MenuItem key="0" value="0">
                                        Select Provider
                                    </MenuItem>
                                    {ProviderItems}
                                </MaterialSelect>
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <TextField
                                    id="file"
                                    name="DocumentFile"
                                    type="file"
                                    onChange={(e) => {
                                        var file = e.target.files[0];

                                        setDocument(URL.createObjectURL(file));

                                        var reader = new FileReader();
                                        formProps.setFieldValue(
                                            'ExcelDocument.Name',
                                            file.name
                                        );
                                        formProps.setFieldValue(
                                            'ExcelDocument.Size',
                                            file.size
                                        );
                                        formProps.setFieldValue(
                                            'ExcelDocument.Type',
                                            file.type
                                        );
                                        reader.onload = function (item) {
                                            formProps.setFieldValue(
                                                'ExcelDocument.Data',
                                                item.target.result
                                                    .replace('data:', '')
                                                    .replace(/^.+,/, '')
                                            );
                                        };

                                        reader.readAsDataURL(file);
                                    }}
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <PrimaryButton
                                    type="submit"
                                    title="Save"
                                    className={classes.CustomPrimaryButtonStyle}
                                    disabled={loading}
                                >
                                    Save
                                </PrimaryButton>
                                {loading && (
                                    <CircularProgress
                                        size={24}
                                        className={classes.buttonProgress}
                                    />
                                )}
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <SecondaryButton
                                    type="reset"
                                    variant="contained"
                                    color="secondary"
                                    title="Reset"
                                    className={classes.CustomDangerButtonStyle}
                                >
                                    Reset
                                </SecondaryButton>
                            </Grid>
                        </Grid>
                    </Paper>
                </Grid>
            </Form>
        );
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
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={handleSubmit}
                resetForm={FormObj}
            >
                {UploadForm}
            </Formik>
            <ReactTable data={itemList} columns={mainColumns} />
        </CustomContainer>
    );
};
