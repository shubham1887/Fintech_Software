import React, { useState, useEffect, useRef } from 'react';
import { Container, Grid, MenuItem, Paper } from '@material-ui/core';

import { Formik, Form } from 'formik';
import { LOGOUT } from 'ConstComponents';
import {
    SelectByChannelCategoryID as CHANNEL_SELECT_BY_CATEGORYID,
    InsertPackageChannel as INSERT_PACKAGE_CHANNEL
} from 'Components/Api/CategoryChannelMappingAPI';

import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';

import { selectCategory as CATEGORY_SELECT } from 'Components/Api/CategoryChannelMappingAPI';
import { Select as SELECT_PACKAGE } from 'Components/Api/OfflineProductAPI';

import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);

export const DthPackageChannel = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);

    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');

    const [Categorys, setCategorys] = useState([]);
    const [Package, setPackage] = useState([]);

    const [IsUpdateClicked, setIsUpdateClicked] = useState(false);
    const [
        shouldGetUpdatedDataFromTable,
        setShouldGetUpdatedDataFromTable
    ] = useState(false);
    var [FormObj, setFormObj] = useState({
        ChannelCategoryID: 0,
        Category: '',
        OfflineProductMasterID: 0,
        Name: ''
    });

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }

        // setCriteriaValues(values);
        setIsUpdateClicked(true);
        setShouldGetUpdatedDataFromTable(true);
    };

    const CategorysItems = Categorys.map(
        ({ ChannelCategoryID, Category }, index) => (
            <MenuItem key={index} value={ChannelCategoryID}>
                {Category}
            </MenuItem>
        )
    );

    const selectPackage = async () => {
        const response = await SELECT_PACKAGE();
        if (!response.isAuthorized) LOGOUT();
        var data = JSON.parse(response.data);
        setPackage(data);
    };
    const selectCategory = async () => {
        const response = await CATEGORY_SELECT();
        if (!response.isAuthorized) LOGOUT();
        var data = JSON.parse(response.data);
        setCategorys(data);
    };

    const OnCategoryChange = async (name, value, setFieldValue) => {
        var request = {
            ChannelCategoryID: value
        };
        createColumns();
        setItemList([]);
        setLoadingOpen(true);
        var response = await CHANNEL_SELECT_BY_CATEGORYID(
            JSON.stringify(request)
        );
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setItemList(data);
    };
    const selectChannel = async (ChannelCategoryID) => {
        var request = {
            ChannelCategoryID: ChannelCategoryID
        };
        var requestJson = JSON.stringify(request);

        setLoadingOpen(true);
        const response = await CHANNEL_SELECT_BY_CATEGORYID(requestJson);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);
        setItemList(data);
    };

    const EditClick = async (value) => {
        setLoadingOpen(true);
        let ChannelCategoryID = value[0];
        let Channels = value[1];
        let DistinctChannels = Channels.filter(function (p) {
            return p.IsActive === true;
        });

        var jSONObj = {
            OfflineProductMasterID:
                formRef.current.values.OfflineProductMasterID,
            ChannelCategoryID: ChannelCategoryID,
            CategoryChannelMappingList: DistinctChannels
        };

        setLoadingOpen(true);
        var response = await INSERT_PACKAGE_CHANNEL(JSON.stringify(jSONObj));
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setResponseMessage(response.data);
        setIsError(true);

        setSeverity('success');
        if (!response.status) {
            setSeverity('error');
            return;
        }

        selectChannel(formRef.current.values.ChannelCategoryID);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ChannelCategoryID',
            srno: true
        });

        columns.push({
            label: 'Channel',
            name: 'ChannelList',
            arraycheckboxImage: true,
            keyName: 'ChannelName'
        });

        columns.push({
            label: ' ',
            name: 'ChannelCategoryID',
            save: true,
            callback: (value) => {
                EditClick(value);
            }
        });

        setMainColumns(columns);
    };

    useEffect(() => {
        selectPackage();
        selectCategory();
    }, []);

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
                initialValues={FormObj}
                onSubmit={handleSubmit}
                resetForm={FormObj}
            >
                {({ values, setFieldValue }) => {
                    return (
                        <Form>
                            <h2 className={classes.PaperHead_h2}>
                                DTH Package Channel Mapping
                            </h2>

                            <Grid item xs={12} style={{ marginBottom: '10px' }}>
                                <Paper className={classes.PaperStyle}>
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={4}
                                            lg={4}
                                        >
                                            <MaterialSelect
                                                label="Package"
                                                name="OfflineProductMasterID"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Package
                                                </MenuItem>
                                                {Package.map((Item) => (
                                                    <MenuItem
                                                        key={
                                                            Item.OfflineProductMasterID
                                                        }
                                                        value={
                                                            Item.OfflineProductMasterID
                                                        }
                                                    >
                                                        {Item.Name}
                                                    </MenuItem>
                                                ))}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={4}
                                            lg={4}
                                        >
                                            <MaterialSelect
                                                label="Category"
                                                name="ChannelCategoryID"
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    OnCategoryChange(
                                                        name,
                                                        value,
                                                        setFieldValue
                                                    );
                                                }}
                                                title="Category"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Category
                                                </MenuItem>
                                                {CategorysItems}
                                            </MaterialSelect>
                                        </Grid>
                                    </Grid>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>

            <ReactTable
                data={itemList}
                columns={mainColumns}
                rowsPerPage={50}
                headerToolbar={false}
                SuccessFailHeader={true}
                primaryIndex={12}
                setLoadingOpen={(value) => {
                    setLoadingOpen(value);
                }}
                setIsError={(value) => {
                    setIsError(value);
                }}
                setSeverity={(value) => {
                    setSeverity(value);
                }}
                setResponseMessage={(value) => {
                    setResponseMessage(value);
                }}
            />
        </CustomContainer>
    );
};
