import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { Add as Operator_ADD } from 'Components/Api/OperatorsApi';
import { SelectAll as SERVICE_SELECT_ALL } from 'Components/Api/ServiceApi';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { AmountInWordLabel } from 'Components/Inputes/FormikMaterialInput';

import {
    Container,
    MenuItem,
    Grid,
    Paper,
    Button,
    TextField,
    CircularProgress,
    Box
} from '@material-ui/core';
import { useHistory } from 'react-router';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const OperatorAdd = () => {
    const [OperatorImage, setOperatorImage] = useState('');
    const history = useHistory();
    const [loading, setLoading] = React.useState(false);
    const classes = commonStyle();
    const [Services, setServices] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [DisplayImage, setDisplayImage] = useState(false);
    useEffect(() => {
        selectServices();
    }, []);

    const selectServices = async () => {
        const response = await SERVICE_SELECT_ALL();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
    };

    var OperatorObj = {
        ServiceID: 0,
        OperatorName: '',
        Opcode: '',
        BusinessModel: 0,
        SequenceLogic: '0',
        HSNCode: '',
        MinimumRechargeAmount: 0,
        MaximumRechargeAmount: 0,
        AccountStartWith: '',
        IsBBPS: false,
        IsBillFetch: false,
        IsPartialBill: false,
        MaxLength: 10,
        MinLength: 1,
        TollFreeNumber: '',
        AccountName: '',
        Remark: '',
        UISequence: 0,
        AllowedFor: 'All',
        BlockedAmount: '',
        Document: {
            Name: '',
            Size: '',
            Type: '',
            Data: ''
        }
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        if (loading) return;

        var requestObj = JSON.stringify(values);

        setLoading(true);
        setIsError(false);
        var response = await Operator_ADD(requestObj);

        if (!response.isAuthorized) LOGOUT();

        setResponseMessage(response.data);
        setIsError(true);
        setSubmitting(false);

        setSeverity('success');
        if (!response.status) {
            setSeverity('error');
            return;
        }

        resetForm(OperatorObj);

        setTimeout(() => {
            history.push('/Operators/Index/');
        }, MESSAGE_CLOSE_AFTER);
    };

    const OpratorValidationSchema = Yup.object({
        ServiceID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'service')
            .required(PLEASE_SELECT_VALID + 'service'),
        OperatorName: Yup.string().required(PLEASE_PROVIDE_VALID + 'name'),
        Opcode: Yup.string().required(PLEASE_PROVIDE_VALID + 'op code'),
        MinimumRechargeAmount: Yup.number()
            .moreThan(0, PLEASE_PROVIDE_VALID + 'Minimum 1 recharge amount')
            .required(PLEASE_PROVIDE_VALID + 'minimum recharge amount'),
        MaximumRechargeAmount: Yup.number()
            .moreThan(9, PLEASE_PROVIDE_VALID + 'amount more than 9')
            .required(PLEASE_PROVIDE_VALID + 'maximum recharge amount'),
        BusinessModel: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'business model')
            .required(PLEASE_SELECT_VALID + 'business model'),
        SequenceLogic: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'sequence logic')
            .required(PLEASE_SELECT_VALID + 'sequence logic')
    });
    const UploadForm = (formProps) => {
        return (
            <Form>
                <Grid container spacing={2}>
                    <Grid item xs={12} sm={12} md={10} lg={10}>
                        <h2 className={classes.PaperHead_h2}>Operator</h2>
                    </Grid>
                    <Grid item xs={12} sm={12} md={2} lg={2}>
                        <PrimaryButton
                            variant="contained"
                            color="primary"
                            title="Operator List"
                            className={classes.CustomPrimaryButtonStyle}
                            onClick={() => {
                                history.push('/Operators/Index');
                            }}
                        >
                            Operator List
                        </PrimaryButton>
                    </Grid>
                </Grid>

                <Paper
                    className={classes.PaperStyle}
                    style={{ marginTop: '15px' }}
                >
                    <Grid item xs={12}>
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialSelect
                                    label="Service"
                                    name="ServiceID"
                                >
                                    <MenuItem key="0" value="0">
                                        Select Service
                                    </MenuItem>
                                    {Services.map((Item) => (
                                        <MenuItem
                                            key={Item.ServiceID}
                                            value={Item.ServiceID}
                                        >
                                            {Item.ServiceName}
                                        </MenuItem>
                                    ))}
                                </MaterialSelect>
                            </Grid>

                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialTextField
                                    label="Operator Name"
                                    name="OperatorName"
                                    type="text"
                                    placeholder="Operator Name"
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialTextField
                                    label="Operator code"
                                    name="Opcode"
                                    type="text"
                                    placeholder="Operator code"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialSelect label="IsBBPS" name="IsBBPS">
                                    <MenuItem key="false" value="false">
                                        No
                                    </MenuItem>
                                    <MenuItem key="true" value="true">
                                        YES
                                    </MenuItem>
                                </MaterialSelect>
                            </Grid>
                        </Grid>
                    </Grid>

                    <h2 className={classes.PaperHead_h2}>Amount</h2>
                    <Grid item xs={12}>
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialNumberField
                                    label="Minimum RechargeAmount"
                                    name="MinimumRechargeAmount"
                                    type="text"
                                    length={7}
                                    placeholder="Minimum RechargeAmount"
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialNumberField
                                    label="Maximum RechargeAmount"
                                    name="MaximumRechargeAmount"
                                    type="text"
                                    length={7}
                                    placeholder="Maximum RechargeAmount"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <label>Operator Image</label>
                                <TextField
                                    id="file"
                                    name="Document"
                                    type="file"
                                    accept=".png, .jpg, .jpeg"
                                    onChange={(e) => {
                                        var file = e.target.files[0];

                                        setOperatorImage(
                                            URL.createObjectURL(file)
                                        );

                                        var reader = new FileReader();
                                        formProps.setFieldValue(
                                            'Document.Name',
                                            file.name
                                        );
                                        formProps.setFieldValue(
                                            'Document.Size',
                                            file.size
                                        );
                                        formProps.setFieldValue(
                                            'Document.Type',
                                            file.type
                                        );
                                        reader.onload = function (item) {
                                            formProps.setFieldValue(
                                                'Document.Data',
                                                item.target.result
                                                    .replace('data:', '')
                                                    .replace(/^.+,/, '')
                                            );
                                        };

                                        reader.readAsDataURL(file);
                                        setDisplayImage(true);
                                    }}
                                />
                                <AmountInWordLabel
                                    name="Note"
                                    readOnly={true}
                                    title="Only jpg/ jpeg/ pdf and png files are allowed under 3MB!"
                                />
                            </Grid>
                            {DisplayImage ? (
                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <img
                                        name="OperatorImage"
                                        src={OperatorImage}
                                        height={80}
                                        width={100}
                                        alt={OperatorImage}
                                    />
                                </Grid>
                            ) : null}
                        </Grid>
                    </Grid>

                    <h2 className={classes.PaperHead_h2}>Business</h2>
                    <Grid item xs={12}>
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialSelect
                                    label="BusinessModel"
                                    name="BusinessModel"
                                >
                                    <MenuItem key="0" value="0">
                                        Select Business Model
                                    </MenuItem>
                                    <MenuItem key="P2P" value="P2P">
                                        P2P
                                    </MenuItem>
                                    <MenuItem
                                        key="P2A Banking RCM"
                                        value="P2A Banking RCM"
                                    >
                                        P2A Banking RCM
                                    </MenuItem>
                                    <MenuItem
                                        key="P2A Banking Non RCM"
                                        value="P2A Banking Non RCM"
                                    >
                                        P2A Banking Non RCM
                                    </MenuItem>
                                    <MenuItem
                                        key="P2A Non Banking"
                                        value="P2A Non Banking"
                                    >
                                        P2A Non Banking
                                    </MenuItem>
                                </MaterialSelect>
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialSelect
                                    label="SequenceLogic"
                                    name="SequenceLogic"
                                >
                                    <MenuItem key="0" value="0">
                                        Select Sequence logic
                                    </MenuItem>
                                    <MenuItem key="No" value="No">
                                        No
                                    </MenuItem>
                                    <MenuItem key="Table" value="Table">
                                        Table
                                    </MenuItem>
                                    <MenuItem key="API " value="API">
                                        API
                                    </MenuItem>
                                </MaterialSelect>
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialTextField
                                    label="HSNCode"
                                    name="HSNCode"
                                    type="text"
                                    placeholder="HSNCode"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialTextField
                                    label="AccountStartWith"
                                    name="AccountStartWith"
                                    type="text"
                                    placeholder="AccountStartWith"
                                />
                            </Grid>
                        </Grid>
                    </Grid>

                    <h2 className={classes.PaperHead_h2}>Other Details</h2>
                    <Grid item xs={12}>
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialSelect
                                    label="IsBillFetch"
                                    name="IsBillFetch"
                                >
                                    <MenuItem key="false" value="false">
                                        No
                                    </MenuItem>
                                    <MenuItem key="true" value="true">
                                        YES
                                    </MenuItem>
                                </MaterialSelect>
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialSelect
                                    label="IsPartialBill"
                                    name="IsPartialBill"
                                >
                                    <MenuItem key="false" value="false">
                                        No
                                    </MenuItem>
                                    <MenuItem key="true" value="true">
                                        YES
                                    </MenuItem>
                                </MaterialSelect>
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialNumberField
                                    label="MinLength"
                                    name="MinLength"
                                    type="text"
                                    length={7}
                                    placeholder="Min Length"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialNumberField
                                    label="MaxLength"
                                    name="MaxLength"
                                    type="text"
                                    length={7}
                                    placeholder="Max Length"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialTextField
                                    label="TollFreeNumber"
                                    name="TollFreeNumber"
                                    type="text"
                                    placeholder="TollFreeNumber"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialTextField
                                    label="AccountName"
                                    name="AccountName"
                                    type="text"
                                    placeholder="AccountName"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialTextField
                                    label="Remark"
                                    name="Remark"
                                    type="text"
                                    placeholder="Remark"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialNumberField
                                    label="UI Sequence"
                                    name="UISequence"
                                    type="text"
                                    length={7}
                                    placeholder="UI Sequence"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialSelect
                                    label="Allowed For"
                                    name="AllowedFor"
                                >
                                    <MenuItem key="All" value="All">
                                        All
                                    </MenuItem>
                                    <MenuItem
                                        key="OnlyAPIUser"
                                        value="OnlyAPIUser"
                                    >
                                        OnlyAPIUser
                                    </MenuItem>
                                </MaterialSelect>
                            </Grid>
                            <Grid item xs={12} sm={12} md={5} lg={5}>
                                <MaterialTextField
                                    label="Blocked Amount"
                                    name="BlockedAmount"
                                    type="text"
                                    placeholder="Remark"
                                />
                                <div style={{ color: 'green' }}>
                                    Note : if you want fix amount then use ","
                                    and for range use "-" like (eg. 10, 20,
                                    50-200, 300-500)
                                </div>
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <PrimaryButton
                                    type="submit"
                                    title="Add"
                                    className={classes.CustomPrimaryButtonStyle}
                                    disabled={loading}
                                >
                                    Add
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
                                    title="Reset"
                                    className={classes.CustomDangerButtonStyle}
                                >
                                    Reset
                                </SecondaryButton>
                            </Grid>
                        </Grid>
                    </Grid>
                </Paper>

                <Grid className={classes.footerGrid}>
                    <Box className={classes.flexReverse}>
                        <div className={classes.SubmitWrapper}></div>
                    </Box>
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
                initialValues={OperatorObj}
                validationSchema={OpratorValidationSchema}
                onSubmit={submithandle}
                resetForm={OperatorObj}
            >
                {UploadForm}
            </Formik>
        </CustomContainer>
    );
};
