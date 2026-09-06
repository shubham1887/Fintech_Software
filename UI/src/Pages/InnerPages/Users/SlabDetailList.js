import React, { useEffect, useState } from 'react';
import ReactTable from 'Components/ReactTable/ReactTable';
import HOCLoader from 'Components/Loader/HOCLoader';
import { makeStyles } from '@material-ui/core/styles';
import { Container, Grid, Box } from '@material-ui/core';
const CustomContainer = HOCLoader(Container);

const useStyles = makeStyles((theme) => ({
    dialogPaper: {
        // minHeight: '80vh',
        //  maxHeight: '80vh'
    }
}));

const SlabDetailList = ({ SlabDetails, onDismissCallback }) => {
    const classes = useStyles();
    const [mainColumns, setMainColumns] = useState([]);

    useEffect(() => {
        createColumns();
    }, [SlabDetails.length]);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ID',
            srno: true
        });
        // columns.push({
        //     label: 'Name',
        //     name: 'SlabName'
        // });
        columns.push({
            label: 'Operator',
            name: 'OperatorName'
        });
        columns.push({
            label: 'From Amt',
            name: 'FromAmount',
            numeric: true
        });
        columns.push({
            label: 'To Amt',
            name: 'ToAmount',
            numeric: true
        });
        columns.push({
            label: 'Ret Comm',
            name: 'RetailerComm',
            numeric: true
        });
        columns.push({
            label: 'Type',
            name: 'RetailerCommType'
        });
        columns.push({
            label: 'Dist Comm',
            name: 'DistributorComm',
            numeric: true
        });
        columns.push({
            label: 'Type',
            name: 'DistributorCommType'
        });
        columns.push({
            label: 'SD Comm',
            name: 'SuperDistributorComm',
            numeric: true
        });
        columns.push({
            label: 'Type',
            name: 'SuperDistributorCommType'
        });

        setMainColumns(columns);
    };

    return (
        <CustomContainer
            isLoading={false}
            isError={false}
            alertMessage={''}
            severity={''}
            onCloseCallback={(alertState) => {
                onDismissCallback(false);
            }}
        >
            <Grid item xs={12} sm={12} classes={{ paper: classes.dialogPaper }}>
                <Box height="75%">
                    <ReactTable
                        data={JSON.parse(SlabDetails)}
                        columns={mainColumns}
                    />
                </Box>
            </Grid>
        </CustomContainer>
    );
};

export default SlabDetailList;
