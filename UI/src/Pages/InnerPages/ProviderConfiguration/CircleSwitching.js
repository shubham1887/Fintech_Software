import {
    Button,
    Container,
    Grid,
    MenuItem,
    Paper,
    Box
} from '@material-ui/core';
import {
    Select as CIRCLE_SWITCHING_SELECT,
    Update as CIRCLE_SWITCHING_UPDATE
} from 'Components/Api/CircleSwitchingAPI';
import { useHistory } from 'react-router';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';
import { SelectByRechargeServices as SERVICE_SELECT_RECHARGE } from 'Components/Api/ServiceApi';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { PLEASE_SELECT_VALID } from 'Components/Helper/Messages';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import HOCLoader from 'Components/Loader/HOCLoader';
import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';
import { Form, Formik } from 'formik';
import React, { useEffect, useRef, useState } from 'react';
import * as Yup from 'yup';

const CustomContainer = HOCLoader(Container);

const CircleSwitching = () => {
    const history = useHistory();
    const classes = commonStyle();
    const formRef = useRef();
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [mainColumns, setMainColumns] = useState([]);
    const [circleSwitchingData, setCircleSwitchingData] = useState([]);
    const [Services, setServices] = useState([]);
    const [providerList, setProviderList] = useState([]);
    const [
        shouldGetUpdatedDataFromTable,
        setShouldGetUpdatedDataFromTable
    ] = useState(false);
    const [formObject] = useState({
        ProviderID: '',
        ServiceID: '0'
    });

    const selectProvider = async () => {
        const response = await PROVIDER_DROP_DOWN();
        let { isAuthorized, status, data } = response;
        if (!isAuthorized) LOGOUT();
        if (!status) return;

        setProviderList(JSON.parse(data));
    };

    const selectServices = async () => {
        const response = await SERVICE_SELECT_RECHARGE();
        let { isAuthorized, status, data } = response;
        if (!isAuthorized) LOGOUT();
        if (!status) return;
        setServices(JSON.parse(data));
    };

    useEffect(() => {
        selectProvider();
        selectServices();
        createColumns();
    }, []);

    const createColumns = () => {
        let columns = [];

        columns.push({
            label: '#',
            name: 'ProviderOperatorCircleMasterID',
            srno: true
        });
        columns.push({
            label: 'ProviderID',
            name: 'ProviderID',
            hide: true
        });
        columns.push({
            label: 'OperatorID',
            name: 'OperatorID',
            hide: true
        });
        columns.push({
            label: 'Operator',
            name: 'Operator'
        });

        columns.push({
            label: 'Circles',
            name: 'Circles',
            MultiSelectCheckbox: true
        });

        columns.push({
            label: 'Update',
            name: 'Update',
            save: true,
            buttonTitle: 'Update',
            callback: handleUpdateClick
        });
        setMainColumns(columns);
    };

    const handleUpdateClick = (row) => {
        let obj = {
            ProviderID: row[1],
            OperatorID: row[2],
            Circles: row[4]
        };

        Update(obj);
    };

    const refreshForm = () => {
        if (!formRef.current) return;
        formRef.current.handleSubmit();
    };

    const Update = async (obj) => {
        setLoadingOpen(true);
        const response = await CIRCLE_SWITCHING_UPDATE(JSON.stringify(obj));
        setLoadingOpen(false);
        let { isAuthorized, status, data } = response;

        if (!isAuthorized) LOGOUT();

        if (!status) return;

        setResponseMessage(data);
        setIsError(true);

        setSeverity('success');
        if (!status) {
            setSeverity('error');
            return;
        }
        refreshForm();
    };

    const validationSchema = Yup.object({
        ProviderID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'provider')
            .required(PLEASE_SELECT_VALID + 'provider'),
        ServiceID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'service')
            .required(PLEASE_SELECT_VALID + 'service')
    });

    const handleSubmit = async (values, { setSubmitting }) => {
        setSubmitting(true);
        setLoadingOpen(true);
        const response = await CIRCLE_SWITCHING_SELECT(JSON.stringify(values));
        setLoadingOpen(false);

        let { isAuthorized, status, data } = response;

        if (!isAuthorized) LOGOUT();

        if (!status) return;

        let d = JSON.parse(data);

        setCircleSwitchingData(d);
        setSubmitting(false);
    };

    const handleCustomContainerCloseCallback = (alertState) => {
        setIsError(alertState);
    };

    const providerItems = providerList.map(({ ProviderID, ProviderName }) => {
        return (
            <MenuItem key={ProviderID + '-' + ProviderName} value={ProviderID}>
                {ProviderName}
            </MenuItem>
        );
    });

    const ServiceItems = Services.map(({ ServiceID, ServiceName }) => (
        <MenuItem key={ServiceID + '-' + ServiceName} value={ServiceID}>
            {ServiceName}
        </MenuItem>
    ));

    const CircleSwitchingForm = (formProps) => {
        return (
            <Form>
                <Grid container spacing={3}>
                    <Grid item xs={12} sm={12} md={10} lg={10}>
                        <h2 className={classes.PaperHead_h2}>
                            Circle Switching
                        </h2>
                    </Grid>
                    <Grid item xs={12} sm={12} md={2} lg={2}>
                        <Box className={classes.flexReverse}>
                            <Button
                                variant="contained"
                                color="primary"
                                className={classes.CustomPrimaryButtonStyle}
                                onClick={() => {
                                    history.push('/CircleIndex');
                                }}
                            >
                                Circle details
                            </Button>
                        </Box>
                    </Grid>
                </Grid>
                <Grid
                    item
                    xs={12}
                    sm={12}
                    md={12}
                    lg={12}
                    style={{ paddingBottom: '10px' }}
                >
                    <Paper
                        className={[
                            classes.PaperStyleWithoutBottom,
                            classes.p10
                        ]}
                    >
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialSelect
                                    label="Provider"
                                    name="ProviderID"
                                    title="Provider"
                                >
                                    {providerItems}
                                </MaterialSelect>
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <MaterialSelect
                                    label="Service"
                                    name="ServiceID"
                                    title="Service"
                                >
                                    <MenuItem key="0" value="0">
                                        Select Service
                                    </MenuItem>
                                    {ServiceItems}
                                </MaterialSelect>
                            </Grid>

                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <Button
                                    type="submit"
                                    title="Search"
                                    style={{ margin: '20px' }}
                                    className={classes.CustomPrimaryButtonStyle}
                                    disabled={loadingOpen}
                                >
                                    Search
                                </Button>
                            </Grid>
                        </Grid>
                    </Paper>
                </Grid>
            </Form>
        );
    };
    const processUpdatedData = (UpdatedList) => {
        setShouldGetUpdatedDataFromTable(false);
        if (!UpdatedList) {
            setResponseMessage('Please select v');
            setIsError(true);

            setSeverity('error');
            return;
        }
        let list = [];

        // UpdateAllClick(list);
    };
    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={handleCustomContainerCloseCallback}
        >
            <Formik
                innerRef={formRef}
                initialValues={formObject}
                validationSchema={validationSchema}
                onSubmit={handleSubmit}
            >
                {CircleSwitchingForm}
            </Formik>
            <Button
                type="button"
                variant="outlined"
                color="primary"
                size="small"
                title="Update All"
                style={{ float: 'right' }}
                onClick={(e) => {
                    setShouldGetUpdatedDataFromTable(true);
                }}
            >
                Update All
            </Button>
            <ReactTable
                data={circleSwitchingData}
                columns={mainColumns}
                getCallback={({ currentPage, RowsPerPage }) => {}}
                getSearchCallback={(SearchText) => {}}
                getSortCallback={({ column, direction }) => {}}
                getUpdatedData={processUpdatedData}
                shouldGetUpdatedData={shouldGetUpdatedDataFromTable}
                isSelectableRows={'none'}
            />
        </CustomContainer>
    );
};

export default CircleSwitching;
