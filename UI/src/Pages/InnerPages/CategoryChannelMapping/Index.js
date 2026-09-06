import React, { memo, useState, useEffect } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
    Container,
    Grid,
    MenuItem,
    Paper,
    CircularProgress,
    TextField
} from '@material-ui/core';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import ReactTable from 'Components/ReactTable/ReactTable';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { LOGOUT } from 'ConstComponents';
import {
    Insert as ADD_CHANNEL,
    Select as SELECT,
    SelectByID as SELECT_BY_ID,
    Update as UPDATE_CHANNEL,
    Delete as DELETE_CHANNEL,
    selectCategory as CATEGORY_SELECT
} from 'Components/Api/CategoryChannelMappingAPI';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_PROVIDE_VALID,
    MESSAGE_CLOSE_AFTER,
    PLEASE_SELECT_VALID
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';

import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';

const CustomContainer = HOCLoader(Container);

export const DthCategoryChannel = memo(() => {
    const classes = commonStyle();
    const [Categorys, setCategorys] = useState([]);
    const [isUpdate, setIsUpdate] = useState(false);
    const [itemList, setItemList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [channelImage, setChannelImage] = useState(undefined);
    const [channelDimension, setChannelDimension] = useState({
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
        ChannelCategoryID: 0,
        ChannelImage: {
            Name: '',
            Size: '',
            Type: '',
            Data: ''
        },
        ChannelName: ''
    };

    const [FormObj, setFormObj] = useState(initialValues);

    const FormValidationSchema = Yup.object({
        ChannelCategoryID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'category')
            .required(PLEASE_SELECT_VALID + 'category'),

        ChannelName: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'channel name'
        )
    });
    const selectCategory = async () => {
        const response = await CATEGORY_SELECT();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setCategorys(data);
    };

    const submitHandle = async (values, { resetForm }) => {
        if (loading) {
            setLoading(false);
            return;
        }

        var requestObj = JSON.stringify(values);

        setIsError(false);
        setLoading(true);

        var response = isUpdate
            ? await UPDATE_CHANNEL(requestObj)
            : await ADD_CHANNEL(requestObj);

        if (!response.isAuthorized) {
            LOGOUT();
            return;
        }

        setSeverity(response.status ? 'success' : 'error');
        setResponseMessage(response.data);
        setIsError(true);
        setLoading(false);

        if (!response.status) return;

        setChannelImage(undefined);
        resetForm(FormObj);
        setFormObj(initialValues);
        setIsUpdate(false);

        setTimeout(() => {
            select();
        }, MESSAGE_CLOSE_AFTER);
    };

    useEffect(() => {
        if (!channelImage) {
            setChannelDimension({
                width: 0,
                height: 0
            });
            return;
        }
        var img = new Image();
        img.onload = function () {
            let newWidth = (this.width * 50) / this.height;
            setChannelDimension({
                width: newWidth,
                height: 50
            });
        };
        img.src = channelImage;
    }, [channelImage]);

    useEffect(() => {
        createColumns();
        selectCategory();
        select();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'CategoryChannelMappingID',
            srno: true
        });
        columns.push({
            label: 'Category',
            name: 'Category'
        });
        columns.push({
            label: 'Channel Name',
            name: 'ChannelName'
        });
        columns.push({
            label: 'Image',
            name: 'ChannelImagePath',
            Image: true
        });

        columns.push({
            label: 'Edit',
            name: 'CategoryChannelMappingID',
            edit: true,
            callback: (CategoryChannelMappingID) => {
                SelectByID(CategoryChannelMappingID);
            }
        });
        columns.push({
            label: 'Delete',
            name: 'CategoryChannelMappingID',
            delete: true,
            callback: (CategoryChannelMappingID) => {
                onDeleteClick(CategoryChannelMappingID);
            }
        });

        setMainColumns(columns);
    };

    const select = async () => {
        const response = await SELECT();

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

    const SelectByID = async (CategoryChannelMappingID) => {
        var requestObj = JSON.stringify({
            CategoryChannelMappingID: CategoryChannelMappingID
        });

        setIsError(false);
        setLoading(true);

        const response = await SELECT_BY_ID(requestObj);

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
        setChannelImage(data.ChannelImagePath);
        setFormObj({
            CategoryChannelMappingID: data.CategoryChannelMappingID,
            ChannelCategoryID: data.ChannelCategoryID,
            ChannelName: data.ChannelName
        });
    };

    const onDeleteClick = (CategoryChannelMappingID) => {
        var RequestObj = {
            CategoryChannelMappingID: CategoryChannelMappingID
        };
        setIsAlertOpen(true);
        setConfirmAlertRequest(RequestObj);
    };

    const Delete = async (request) => {
        var requestObj = JSON.stringify(request);

        setIsError(false);
        setLoading(true);

        const response = await DELETE_CHANNEL(requestObj);

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
                select();
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
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <h2 className={classes.PaperHead_h2}>
                                        Add Channel
                                    </h2>
                                </Grid>
                            </Grid>

                            <Paper className={classes.PaperStyle}>
                                <Grid
                                    container
                                    spacing={2}
                                    style={{ marginBottom: '15px' }}
                                >
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <MaterialSelect
                                            label="Category"
                                            name="ChannelCategoryID"
                                        >
                                            <MenuItem key="0" value="0">
                                                Select Category
                                            </MenuItem>
                                            {Categorys.map((Item) => (
                                                <MenuItem
                                                    key={Item.ChannelCategoryID}
                                                    value={
                                                        Item.ChannelCategoryID
                                                    }
                                                >
                                                    {Item.Category}
                                                </MenuItem>
                                            ))}
                                        </MaterialSelect>
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <MaterialTextField
                                            label="Channel Name"
                                            name="ChannelName"
                                            type="text"
                                            placeholder="Channel Name"
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

                                                setChannelImage(
                                                    URL.createObjectURL(file)
                                                );

                                                setFieldValue(
                                                    'ChannelImage.Name',
                                                    file.name
                                                );
                                                setFieldValue(
                                                    'ChannelImage.Size',
                                                    file.size
                                                );
                                                setFieldValue(
                                                    'ChannelImage.Type',
                                                    file.type
                                                );

                                                let reader = new FileReader();
                                                reader.onload = ({
                                                    target: { result }
                                                }) => {
                                                    setFieldValue(
                                                        'ChannelImage.Data',
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
                                        <img
                                            name="ChannelImage"
                                            src={channelImage}
                                            height={channelDimension.height}
                                            width={channelDimension.width}
                                            alt={channelImage}
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <PrimaryButton
                                            type="submit"
                                            title="Add"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            disabled={loading}
                                        >
                                            {isUpdate ? 'Update' : 'Add'}
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
                            </Paper>
                        </Form>
                    );
                }}
            </Formik>
            {isAlertOpen ? (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete this Channel?"
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
