import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';

import { SelectOperator as OPERATOR_SELECT_OPERATOR } from 'Components/Api/OperatorsApi';
import { SelectAll as SERVICE_SELECT_ALL } from 'Components/Api/ServiceApi';
import { OperatorUpdate as OPERATOR_UPDATE } from 'Components/Api/OperatorsApi';
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

import SettingsIcon from '@material-ui/icons/Settings';
import { useHistory } from 'react-router';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    MESSAGE_CLOSE_AFTER,
    PLEASE_SELECT_VALID,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const OperatorEdit = () => {
    const history = useHistory();
    const classes = commonStyle();
    const [OperatorImage, setOperatorImage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [Services, setServices] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [OperatorObj, setOperatorObj] = useState({
        OperatorName: '',
        ServiceID: 0,
        OperatorID: 0,
        Opcode: '',
        BusinessModel: 0,
        SequenceLogic: '0',
        HSNCode: '',
        MinimumRechargeAmount: 0,
        MaximumRechargeAmount: 0,
        RepeatRechargeInMinute: 0,
        AccountStartWith: '',
        IsBBPS: 'false',
        IsBillFetch: false,
        IsPartialBill: false,
        IsViewRequired: false,
        MaxLength: 10,
        MinLength: 10,
        TollFreeNumber: '',
        AccountName: '',
        Remark: '',
        UISequence: '',
        AllowedFor: '',
        BlockedAmount: '',
        Document: {
            Name: '',
            Size: '',
            Type: '',
            Data: ''
        }
    });

    useEffect(() => {
        setLoadingOpen(true);
        selectServices();
        selectOperator();
    }, []);

    const selectServices = async () => {
        const response = await SERVICE_SELECT_ALL();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
    };

    const selectOperator = async () => {
        let URL = window.location.pathname;
        var res = URL.split('/');

        let OperatorID = res[res.length - 1];

        let JSONObj = {
            OperatorID: OperatorID
        };

        var request = JSON.stringify(JSONObj);

        const response = await OPERATOR_SELECT_OPERATOR(request);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setOperatorImage(data.Path);

        setOperatorObj({
            ...OperatorObj,
            OperatorName: data.OperatorName,
            OperatorID: data.OperatorID,
            ServiceID: data.ServiceID,
            Opcode: data.Opcode,
            HSNCode: data.HSNCode,
            IsBBPS: data.IsBBPS,
            MinimumRechargeAmount: data.MinimumRechargeAmount,
            MaximumRechargeAmount: data.MaximumRechargeAmount,
            BusinessModel: data.BusinessModel,
            SequenceLogic: data.SequenceLogic,
            RepeatRechargeInMinute: data.RepeatRechargeInMinute,
            IsBillFetch: data.IsBillFetch,
            IsPartialBill: data.IsPartialBill,
            MaxLength: data.MaxLength,
            MinLength: data.MinLength,
            TollFreeNumber: data.TollFreeNumber,
            AccountNumber: data.AccountNumber,
            AccountStartWith: data.AccountStartWith,
            AccountName: data.AccountName,
            Remark: data.Remark,
            UISequence: data.UISequence,
            AllowedFor: data.AllowedFor,
            BlockedAmount: data.BlockedAmount,
            IsViewRequired: data.IsViewRequired
        });
    };

    const OperatorUpdate = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) return;

        setLoadingOpen(true);
        var requestObj = JSON.stringify(values);
        var response = await OPERATOR_UPDATE(requestObj);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        setSubmitting(false);

        if (!response.status) {
            setSeverity('error');
            setLoadingOpen(false);
            return;
        }

        setTimeout(() => {
            history.push('/Operators/Index/');
        }, MESSAGE_CLOSE_AFTER);
    };

    const OpratorValidationSchema = Yup.object({
        ServiceID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'ServiceID')
            .required('Please select valid Service'),
        OperatorName: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'sender remark'
        ),
        Opcode: Yup.string().required(PLEASE_PROVIDE_VALID + 'opcode'),
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
                <h2 className={classes.PaperHead_h2}>Operator </h2>

                <Paper className={classes.PaperStyle}>
                    <Grid item xs={12}>
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialSelect
                                    label="Service"
                                    name="ServiceID"
                                >
                                    <MenuItem key="0" value="0">
                                        Select Servie
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
                            <Grid item xs={12} sm={12} md={4} lg={3}>
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
                            <Grid xs={12} sm={12} md={3} lg={3} item>
                                <MaterialNumberField
                                    label="Minimum RechargeAmount"
                                    name="MinimumRechargeAmount"
                                    type="text"
                                    length={3}
                                    placeholder="Minimum RechargeAmount"
                                />
                            </Grid>
                            <Grid xs={12} sm={12} md={3} lg={3} item>
                                <MaterialNumberField
                                    label="Maximum RechargeAmount"
                                    name="MaximumRechargeAmount"
                                    type="text"
                                    length={7}
                                    placeholder="Maximum RechargeAmount"
                                />
                            </Grid>
                            <Grid xs={12} sm={12} md={3} lg={3} item>
                                <MaterialNumberField
                                    label="Repeat Recharge In Minute"
                                    name="RepeatRechargeInMinute"
                                    type="text"
                                    length={5}
                                    placeholder="Repeat Recharge In Minute"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
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
                                    }}
                                />
                                <AmountInWordLabel
                                    name="Note"
                                    readOnly={true}
                                    title="Only jpg/ jpeg/ pdf and png files are allowed under 3MB!"
                                />
                            </Grid>
                            <Grid item xs={12} sm={12} md={1} lg={1}>
                                <img
                                    name="OperatorImage"
                                    src={OperatorImage}
                                    height={80}
                                    width={100}
                                    alt={OperatorImage}
                                />
                            </Grid>
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
                                        Select Businees Model
                                    </MenuItem>

                                    <MenuItem key="P2P" value="P2P">
                                        P2P
                                    </MenuItem>
                                    <MenuItem
                                        key="P2A Banking RCM "
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
                                        key="P2A Non Banking "
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
                                <MaterialSelect
                                    label="IsViewRequired"
                                    name="IsViewRequired"
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
                            <Grid item xs={12} sm={12} md={3} lg={3}>
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
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <Button
                                    variant="contained"
                                    color="secondary"
                                    startIcon={<SettingsIcon color="primary" />}
                                    onClick={(event) => {
                                        var redirectObj = {
                                            ServiceID: OperatorObj.ServiceID,
                                            OperatorID: OperatorObj.OperatorID
                                        };

                                        history.push(
                                            '/BBPSConfiguration/Index',
                                            redirectObj
                                        );
                                    }}
                                >
                                    BBPS Configurations
                                </Button>
                            </Grid>
                        </Grid>
                    </Grid>
                </Paper>

                <Grid className={classes.footerGrid}>
                    <Box className={classes.flexReverse}>
                        <div className={classes.SubmitWrapper}>
                            <Button
                                type="submit"
                                title="Update"
                                className={classes.CustomPrimaryButtonStyle}
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
                initialValues={OperatorObj}
                validationSchema={OpratorValidationSchema}
                onSubmit={OperatorUpdate}
                resetForm={OperatorObj}
            >
                {UploadForm}
            </Formik>
        </CustomContainer>
    );
};
