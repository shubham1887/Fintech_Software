import React, { memo, useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    MenuItem,
    Grid,
    Typography,
    Paper,
    CircularProgress,
    TextField
} from '@material-ui/core';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import ReactTable from 'Components/ReactTable/ReactTable';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { LOGOUT } from 'ConstComponents';
import {
    AddSlider as ADD_SLIDER,
    UpdateSlider as UPDATE_SLIDER,
    SelectSliders as SELECT_SLIDERS,
    DeleteSlider as DELETE_SLIDER,
    ActiveDeactiveSlider as ACTIVE_DEACTIVE_SLIDER,
    SelectSlidersByID as SELECT_BY_ID
} from 'Components/Api/ServiceWiseSliderAPI';
import { SelectAll as SERVICE_SELECT_ALL } from 'Components/Api/ServiceApi';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_PROVIDE_VALID,
    MESSAGE_CLOSE_AFTER
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const Index = memo(() => {
    const classes = commonStyle();
    const [isUpdate, setIsUpdate] = useState(false);
    const [itemList, setItemList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [sliderImage, setSliderImage] = useState(undefined);
    const [sliderDimension, setSliderDimension] = useState({
        width: 0,
        height: 0
    });
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [Services, setServices] = useState([]);

    const initialValues = {
        ServiceWiseSliderSettingID: 0,
        ServiceID: 0,
        Priority: 0,
        ReferenceLink: '',
        SliderImage: {
            Name: '',
            Size: '',
            Type: '',
            Data: ''
        },
        Description: '',
        Type: 'ALL'
    };

    const [FormObj, setFormObj] = useState(initialValues);

    const FormValidationSchema = Yup.object({
        Description: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'description'
        ),
        Type: Yup.string()
            .notOneOf(['ALL'], 'Please select valid type')
            .required('Required')
    });

    const submitHandle = async (values, { resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }

        var requestObj = JSON.stringify(values);

        setIsError(false);
        setLoading(true);

        var response = isUpdate
            ? await UPDATE_SLIDER(requestObj)
            : await ADD_SLIDER(requestObj);

        if (!response.isAuthorized) {
            LOGOUT();
            return;
        }

        setSeverity(response.status ? 'success' : 'error');
        setResponseMessage(response.data);
        setIsError(true);
        setLoading(false);

        if (!response.status) return;

        setSliderImage(undefined);
        resetForm(FormObj);
        setFormObj(initialValues);
        setIsUpdate(false);

        setTimeout(() => {
            selectSliders();
        }, MESSAGE_CLOSE_AFTER);
    };

    useEffect(() => {
        if (!sliderImage) {
            setSliderDimension({
                width: 0,
                height: 0
            });
            return;
        }
        var img = new Image();
        img.onload = function () {
            let newWidth = (this.width * 150) / this.height;
            setSliderDimension({
                width: newWidth,
                height: 150
            });
        };
        img.src = sliderImage;
    }, [sliderImage]);

    useEffect(() => {
        createColumns();
        selectSliders();
        selectServices();
    }, []);

    const selectServices = async () => {
        const response = await SERVICE_SELECT_ALL();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ServiceWiseSliderSettingID',
            srno: true
        });

        columns.push({
            label: 'Service',
            name: 'ServiceName'
        });

        columns.push({
            label: 'Priority',
            name: 'Priority'
        });
        columns.push({
            label: 'Type',
            name: 'Type'
        });
        columns.push({
            label: 'Description',
            name: 'Description'
        });

        columns.push({
            label: 'ReferenceLink',
            name: 'ReferenceLink'
        });
        columns.push({
            label: 'Image',
            name: 'ImagePath',
            Image: true
        });

        columns.push({
            label: 'Edit',
            name: 'ServiceWiseSliderSettingID',
            edit: true,
            callback: (ServiceWiseSliderSettingID) => {
                Edit(ServiceWiseSliderSettingID);
            }
        });
        columns.push({
            label: 'IsActive',
            name: 'IsActive',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                activate(id, value);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'ServiceWiseSliderSettingID',
            delete: true,
            callback: (ServiceWiseSliderSettingID) => {
                onDeleteClick(ServiceWiseSliderSettingID);
            }
        });

        setMainColumns(columns);
    };

    const Edit = async (ServiceWiseSliderSettingID) => {
        setIsUpdate(true);
        SelectByID(ServiceWiseSliderSettingID);
    };

    const SelectByID = async (ServiceWiseSliderSettingID) => {
        let jsonObj = {
            ServiceWiseSliderSettingID: ServiceWiseSliderSettingID
        };

        const response = await SELECT_BY_ID(JSON.stringify(jsonObj));
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);

        setIsUpdate(true);
        setSliderImage(data.ImagePath);

        setFormObj({
            ServiceWiseSliderSettingID: data.ServiceWiseSliderSettingID,
            ServiceID: data.ServiceID,
            Priority: data.Priority,
            ReferenceLink: data.ReferenceLink,
            Description: data.Description,
            Type: data.Type
        });
    };

    const selectSliders = async () => {
        var jsonObj = {
            ServiceID: 0
        };
        var request = JSON.stringify(jsonObj);
        const response = await SELECT_SLIDERS(request);
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) {
            setIsError(true);
            setLoading(false);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        var data = JSON.parse(response.data);
        setItemList(data);
    };

    const activate = async (item, value) => {
        var jsonObj = {
            ServiceWiseSliderSettingID: item,
            IsActive: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoading(true);
        const response = await ACTIVE_DEACTIVE_SLIDER(request);

        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        selectSliders();
    };

    const onDeleteClick = (ServiceWiseSliderSettingID) => {
        var RequestObj = {
            ServiceWiseSliderSettingID: ServiceWiseSliderSettingID
        };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (request) => {
        var requestObj = JSON.stringify(request);

        setIsError(false);
        setLoading(true);

        const response = await DELETE_SLIDER(requestObj);

        if (!response.isAuthorized) {
            LOGOUT();
            return;
        }

        setResponseMessage(response.data);
        setSeverity(response.status ? 'success' : 'error');
        setLoading(false);
        setIsError(true);

        if (response.status) {
            setTimeout(() => {
                selectSliders();
            }, MESSAGE_CLOSE_AFTER);
        }
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
                enableReinitialize={true}
                initialValues={FormObj}
                validationSchema={FormValidationSchema}
                onSubmit={submitHandle}
                resetForm={FormObj}
            >
                {({ setFieldValue }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid container spacing={2}>
                                <Grid item xs={12} sm={12} md={4} lg={4}>
                                    <h2 className={classes.PaperHead_h2}>
                                        Add Service Wise Slider
                                    </h2>
                                </Grid>

                                <Grid item xs={12} sm={12} md={8} lg={8}>
                                    <Typography>
                                        <span style={{ color: 'green' }}>
                                            <b>
                                                {
                                                    'Slider dimension must be :: [App : Width <= 800, Height <= 300] , [Web: Width  <= 900, Height <= 600] and extension must be png, jpg, jpeg '
                                                }
                                            </b>
                                        </span>
                                    </Typography>
                                </Grid>
                            </Grid>

                            <Paper className={classes.PaperStyle}>
                                <Grid container spacing={3}>
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
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
                                            <span className={classes.Require}>
                                                *
                                            </span>
                                        </MaterialSelect>
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <MaterialSelect
                                            label="Type"
                                            name="Type"
                                            title="Type"
                                        >
                                            <MenuItem key="ALL" value="ALL">
                                                Select Type
                                            </MenuItem>
                                            <MenuItem key="Web" value="Web">
                                                Web
                                            </MenuItem>
                                            <MenuItem key="App" value="App">
                                                App
                                            </MenuItem>
                                        </MaterialSelect>
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <MaterialNumberField
                                            label="Priority"
                                            name="Priority"
                                            type="text"
                                            length={1}
                                            placeholder="Priority"
                                        />
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <MaterialTextField
                                            label="ReferenceLink"
                                            name="ReferenceLink"
                                            type="text"
                                            placeholder="ReferenceLink"
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <MaterialTextField
                                            label="Description"
                                            name="Description"
                                            type="text"
                                            placeholder="Description"
                                        />
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <TextField
                                            id="file"
                                            name="DocumentFile"
                                            type="file"
                                            inputProps={{
                                                accept: 'image/*'
                                            }}
                                            onChange={(event) => {
                                                let { files } = event.target;
                                                let file = files[0];

                                                setSliderImage(
                                                    URL.createObjectURL(file)
                                                );

                                                setFieldValue(
                                                    'SliderImage.Name',
                                                    file.name
                                                );
                                                setFieldValue(
                                                    'SliderImage.Size',
                                                    file.size
                                                );
                                                setFieldValue(
                                                    'SliderImage.Type',
                                                    file.type
                                                );

                                                let reader = new FileReader();
                                                reader.onload = ({
                                                    target: { result }
                                                }) => {
                                                    setFieldValue(
                                                        'SliderImage.Data',
                                                        result
                                                            .replace(
                                                                'data:',
                                                                ''
                                                            )
                                                            .replace(/^.+,/, '')
                                                    );
                                                };
                                                reader.readAsDataURL(file);
                                            }}
                                        />
                                    </Grid>
                                </Grid>

                                <Grid
                                    container
                                    item
                                    xs={12}
                                    style={{ marginTop: '15px' }}
                                    spacing={2}
                                >
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <PrimaryButton
                                            type="submit"
                                            title="Add Slider"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loading}
                                        >
                                            {isUpdate
                                                ? 'Update Slider'
                                                : 'Add Slider'}
                                        </PrimaryButton>
                                        {loading && (
                                            <CircularProgress
                                                size={24}
                                                className={
                                                    classes.buttonProgress
                                                }
                                            />
                                        )}
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <SecondaryButton
                                            type="reset"
                                            variant="contained"
                                            color="secondary"
                                            title="Reset"
                                            disabled={loading}
                                            className={
                                                classes.CustomDangerButtonStyle
                                            }
                                        >
                                            Reset
                                        </SecondaryButton>
                                    </Grid>
                                </Grid>

                                <Grid
                                    container
                                    item
                                    xs={12}
                                    sm={12}
                                    md={12}
                                    lg={12}
                                    style={{ marginTop: '15px' }}
                                >
                                    <Grid item xs={10} sm={10} md={10} lg={10}>
                                        <img
                                            name="SliderImage"
                                            src={sliderImage}
                                            height={sliderDimension.height}
                                            width={sliderDimension.width}
                                            alt={sliderImage}
                                        />
                                    </Grid>
                                </Grid>
                            </Paper>
                        </Form>
                    );
                }}
            </Formik>
            {isAlertOpen ? (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete this slider?"
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        Delete(request);
                    }}
                    negativeButtonClick={() => {
                        setConfirmAlertRequest({});
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}
            <ReactTable data={itemList} columns={mainColumns} />
        </CustomContainer>
    );
});
