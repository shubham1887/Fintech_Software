import React, { useEffect, useState, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { PlanInsert as SERVICE_PLAN_CREATE } from 'Components/Api/SubscriptionPlanAPI';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { SelectSubscriptionServices as SELECT_CHARGE_SERVICES } from 'Components/Api/ServiceApi';
import Checkbox from '@material-ui/core/Checkbox';
import CheckBoxOutlineBlankIcon from '@material-ui/icons/CheckBoxOutlineBlank';
import CheckBoxIcon from '@material-ui/icons/CheckBox';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import CircularProgress from '@material-ui/core/CircularProgress';
import {
    Container,
    Grid,
    Paper,
    TextField,
    InputAdornment,
    Typography
} from '@material-ui/core';
import Autocomplete from '@material-ui/lab/Autocomplete';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import { AttachFile } from '@material-ui/icons';
import { AmountInWordLabel } from 'Components/Inputes/FormikMaterialInput';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { useHistory } from 'react-router';
const CustomContainer = HOCLoader(Container);

export const CreatePlan = () => {
    const formRef = useRef();
    const [Services, setServices] = useState([]);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [open, setOpen] = useState(false);
    const [image, setImage] = useState('');
    const [MappingList, setMappingList] = useState([]);
    const [selectedFileName, setSelectedFileName] = useState('');
    const history = useHistory();
    const [formObj, setFormObj] = useState({
        ServiceList: [],
        PlanName: '',
        Amount: '',
        Document: {
            Name: '',
            Size: '',
            Type: '',
            Data: ''
        }
    });

    const formValidationSchema = Yup.object({
        PlanName: Yup.string().required(PLEASE_PROVIDE_VALID + 'plan name'),
        Amount: Yup.string().required(PLEASE_PROVIDE_VALID + 'amount')
    });

    const selectServices = async () => {
        const response = await SELECT_CHARGE_SERVICES();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
    };

    useEffect(() => {
        selectServices();
    }, []);

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) return;
        var jsonReq = {
            ServiceSubscriptionCreatePlanID: 0,
            Services: values.ServiceList,
            PlanName: values.PlanName,
            Amount: values.Amount,
            Document: {
                Name: values.Document.Name,
                Size: values.Document.Size,
                Type: values.Document.Type,
                Data: values.Document.Data
            }
        };
        setIsError(false);
        var requestObj = JSON.stringify(jsonReq);
        setLoadingOpen(true);

        var response = await SERVICE_PLAN_CREATE(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        // var data = JSON.parse(response.data);
        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);
        setSubmitting(false);
        resetForm(formObj);
        history.push('/CreatePlanIndex');
    };

    const SeletedListIndex = (list) => {
        var arryIndex = [];

        for (var parent in list) {
            for (var child in Services) {
                if (list[parent].ServiceID === Services[child].ServiceID) {
                    arryIndex.push(Services[child]);
                }
            }
        }
        setMappingList(arryIndex);
    };

    const fileInputRef = useRef(null);
    const handleAttachClick = () => {
        // Trigger the file input click event
        if (fileInputRef.current) {
            fileInputRef.current.click();
        }
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
                innerRef={formRef}
                enableReinitialize={true}
                initialValues={formObj}
                validationSchema={formValidationSchema}
                onSubmit={handleSubmit}
                resetForm={formObj}
                render={({ values, setFieldValue, handleSubmit }) => {
                    const handleFileChange = (event) => {
                        const file = event.target.files[0];
                        if (file) {
                            setSelectedFileName(file.name);
                            setImage(URL.createObjectURL(file));

                            // Set form field values
                            setFieldValue('Document.Name', file.name);
                            setFieldValue('Document.Size', file.size);
                            setFieldValue('Document.Type', file.type);

                            // Read file data
                            const reader = new FileReader();
                            reader.onload = function (item) {
                                const fileData = item.target.result
                                    .replace('data:', '')
                                    .replace(/^.+,/, '');
                                setFieldValue('Document.Data', fileData);
                                //setImage(fileData);
                            };
                            reader.readAsDataURL(file);
                        }
                    };

                    return (
                        <Grid
                            item
                            container
                            margin="normal"
                            xs={12}
                            md={12}
                            sm={12}
                            lg={12}
                            spacing={2}
                        >
                            <Grid item xs={12} sm={12} md={5} lg={5}>
                                <Form>
                                    <h2 className={classes.PaperHead_h2}>
                                        Create Plan
                                    </h2>
                                    <Grid item xs={12}>
                                        <Paper className={classes.PaperStyle}>
                                            <Grid container spacing={2}>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                >
                                                    <MaterialTextField
                                                        label="Plan Name *"
                                                        name="PlanName"
                                                        type="text"
                                                        length={100}
                                                        placeholder="Enter Plan Name"
                                                    />
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                >
                                                    <MaterialNumberField
                                                        label="Enter Price *"
                                                        name="Amount"
                                                        type="text"
                                                        placeholder="Enter Plan Amount"
                                                    />
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                >
                                                    <Typography
                                                        variant="body1"
                                                        color="textPrimary"
                                                        gutterBottom
                                                    >
                                                        Upload Image * (JPG,
                                                        JPEG, PNG)
                                                    </Typography>
                                                    <input
                                                        id="file"
                                                        name="Document"
                                                        type="file"
                                                        ref={fileInputRef}
                                                        accept=".png, .jpg, .jpeg"
                                                        style={{
                                                            display: 'none'
                                                        }}
                                                        onChange={
                                                            handleFileChange
                                                        }
                                                    />
                                                    <TextField
                                                        fullWidth
                                                        placeholder="Image"
                                                        value={selectedFileName}
                                                        InputProps={{
                                                            readOnly: true,
                                                            endAdornment: (
                                                                <InputAdornment position="end">
                                                                    <AttachFile
                                                                        style={{
                                                                            cursor:
                                                                                'pointer',
                                                                            color:
                                                                                '#2196f3'
                                                                        }}
                                                                        onClick={
                                                                            handleAttachClick
                                                                        }
                                                                    />
                                                                </InputAdornment>
                                                            ),
                                                            style: {
                                                                paddingRight:
                                                                    '8px'
                                                            }
                                                        }}
                                                    />
                                                </Grid>
                                                <AmountInWordLabel
                                                    name="Note"
                                                    readOnly={true}
                                                    title="Only jpg/ jpeg/ pdf and png files are allowed under 3MB!"
                                                />
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={12}
                                                    lg={12}
                                                    className={
                                                        classes.multiSelect
                                                    }
                                                >
                                                    <Autocomplete
                                                        multiple
                                                        open={open}
                                                        onOpen={() =>
                                                            setOpen(true)
                                                        }
                                                        onClose={() =>
                                                            setOpen(false)
                                                        }
                                                        name="ServiceList"
                                                        options={Services}
                                                        value={
                                                            values.ServiceList
                                                        }
                                                        getOptionLabel={(
                                                            option
                                                        ) => option.ServiceName}
                                                        getOptionSelected={(
                                                            option,
                                                            value
                                                        ) =>
                                                            option.ServiceID ===
                                                            value.ServiceID
                                                        }
                                                        onChange={(
                                                            event,
                                                            values
                                                        ) => {
                                                            setFieldValue(
                                                                'ServiceList',
                                                                values
                                                            );
                                                            SeletedListIndex(
                                                                values.ServiceList
                                                            );
                                                        }}
                                                        onBlur={() => {
                                                            if (
                                                                values
                                                                    .ServiceList
                                                                    .length > 0
                                                            ) {
                                                                setOpen(false);
                                                            }
                                                        }}
                                                        renderInput={(
                                                            params
                                                        ) => (
                                                            <TextField
                                                                {...params}
                                                                variant="outlined"
                                                                label="Select Products *"
                                                                placeholder="Select Products"
                                                                onClick={() =>
                                                                    setOpen(
                                                                        true
                                                                    )
                                                                }
                                                            />
                                                        )}
                                                        renderOption={(
                                                            option,
                                                            { selected }
                                                        ) => (
                                                            <>
                                                                <Checkbox
                                                                    icon={
                                                                        <CheckBoxOutlineBlankIcon fontSize="small" />
                                                                    }
                                                                    checkedIcon={
                                                                        <CheckBoxIcon fontSize="small" />
                                                                    }
                                                                    style={{
                                                                        marginRight: 8
                                                                    }}
                                                                    checked={
                                                                        selected
                                                                    }
                                                                />
                                                                {
                                                                    option.ServiceName
                                                                }
                                                            </>
                                                        )}
                                                    />
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={3}
                                                    lg={3}
                                                >
                                                    <SecondaryButton
                                                        type="reset"
                                                        title="Cancel"
                                                        className={
                                                            classes.CustomDangerButtonStyle
                                                        }
                                                    >
                                                        Cancel
                                                    </SecondaryButton>
                                                </Grid>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={3}
                                                    lg={3}
                                                >
                                                    <PrimaryButton
                                                        type="submit"
                                                        title="submit"
                                                        className={
                                                            classes.CustomPrimaryButtonStyle
                                                        }
                                                        disabled={loadingOpen}
                                                    >
                                                        submit
                                                    </PrimaryButton>
                                                    {loadingOpen && (
                                                        <CircularProgress
                                                            size={24}
                                                            className={
                                                                classes.buttonProgress
                                                            }
                                                        />
                                                    )}
                                                </Grid>
                                            </Grid>
                                        </Paper>
                                    </Grid>
                                </Form>
                            </Grid>

                            <Grid item xs={12} sm={12} md={5} lg={5}>
                                <img
                                    name="PlanImage"
                                    src={image}
                                    alt={image}
                                    style={{
                                        height: '500px',
                                        width: '300px',
                                        borderRadius: '8px',
                                    }}
                                />
                            </Grid>
                        </Grid>
                    );
                }}
            />
        </CustomContainer>
    );
};
