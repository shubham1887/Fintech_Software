import React, { useEffect, useState } from 'react';
import { VersionHistorySelect } from 'Components/Api/VersionAPI';
import { LOGOUT } from 'ConstComponents';
import { Container, Grid } from '@material-ui/core';
import HOCLoader from 'Components/Loader/HOCLoader';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { VersionRowView } from 'Pages/InnerPages/VersionHistory/VersionRowView';
const CustomContainer = HOCLoader(Container);
export const VersionHistory = () => {
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = useState(false);
    const [severity, setSeverity] = useState('success');
    const classes = commonStyle();
    const [versionHistory, setVersionHistory] = useState([]);

    useEffect(() => {
        SelectVersionHistory();
    }, []);

    const SelectVersionHistory = async () => {
        setIsError(false);
        setLoading(true);
        setVersionHistory([]);
        var response = await VersionHistorySelect();
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }
        setSeverity('success');
        var data = JSON.parse(response.data);
        setVersionHistory(data);
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
            <Grid container spacing={3}>
                <Grid item xs={6}>
                    <h2 className={classes.PaperHead_h2}>Version History</h2>
                </Grid>
                <Grid item xs={6}></Grid>
            </Grid>
            {versionHistory.map(
                ({ DOC, Version, Title, Description }, index) => {
                    return (
                        <Grid item xs={12} sm={12} md={12} lg={12}>
                            <VersionRowView
                                index={index}
                                DOC={DOC}
                                Version={Version}
                                Title={Title}
                                Description={Description}
                            />
                        </Grid>
                    );
                }
            )}
        </CustomContainer>
    );
};
