import React, { memo, useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    MenuItem,
    Paper,
    CircularProgress,
    TextField,
    Typography
} from '@material-ui/core';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import ReactTable from 'Components/ReactTable/ReactTable';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { LOGOUT } from 'ConstComponents';
import {
    AddSlider as ADD_SLIDER,
    SelectSliders as SELECT_SLIDERS,
    SelectSliderByID as SELECT_SLIDER_BY_ID,
    UpdateSlider as UPDATE_SLIDER,
    DeleteSlider as DELETE_SLIDER
} from 'Components/Api/SliderMasterAPI';
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

import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';

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

    const initialValues = {
        SliderMasterID: 0,
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
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'SliderMasterID',
            srno: true
        });
        columns.push({
            label: 'Type',
            name: 'Type'
        });
        columns.push({
            label: 'Image',
            name: 'ImagePath',
            Image: true
        });

        columns.push({
            label: 'Description',
            name: 'Description'
        });

        columns.push({
            label: 'Edit',
            name: 'SliderMasterID',
            edit: true,
            callback: (SliderMasterID) => {
                SelectByID(SliderMasterID);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'SliderMasterID',
            delete: true,
            callback: (row) => {
                onDeleteClick(row[0]);
            }
        });

        setMainColumns(columns);
    };

    const selectSliders = async () => {
        const response = await SELECT_SLIDERS();

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

    const SelectByID = async (SliderMasterID) => {
        var requestObj = JSON.stringify({
            SliderMasterID: SliderMasterID
        });

        setIsError(false);
        setLoading(true);

        const response = await SELECT_SLIDER_BY_ID(requestObj);

        if (!response.isAuthorized) {
            LOGOUT();
            return;
        }

        setLoading(false);
        setSeverity(response.status ? 'success' : 'error');

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }

        var data = JSON.parse(response.data);

        setIsUpdate(true);
        setSliderImage(data.ImagePath);
        setFormObj({
            Description: data.Description,
            SliderMasterID: data.SliderMasterID,
            Type: data.Type
        });
    };

    const onDeleteClick = (SliderMasterID) => {
        var RequestObj = { SliderMasterID: SliderMasterID };
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
                                <Grid item xs={12} sm={12} md={3} lg={3}>
                                    <h2 className={classes.PaperHead_h2}>
                                        Add Slider
                                    </h2>
                                </Grid>
                                <Grid item xs={12} sm={12} md={9} lg={9}>
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
                                <Grid
                                    container
                                    spacing={2}
                                    style={{ marginBottom: '15px' }}
                                >
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
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <MaterialTextField
                                            label="Description"
                                            name="Description"
                                            type="text"
                                            placeholder="Description"
                                        />
                                    </Grid>

                                    <Grid item xs={12} sm={12} md={3} lg={3}>
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

                                <Grid container spacing={2}>
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
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
