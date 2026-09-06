import React, { useState, useEffect } from 'react';
import { Grid, Paper } from '@material-ui/core';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableContainer from '@material-ui/core/TableContainer';
import TableRow from '@material-ui/core/TableRow';
import { withStyles } from '@material-ui/core/styles';

const StyledTableCell = withStyles((theme) => ({
    body: {
        fontSize: 16,
        fontWeight: 'bold'
    }
}))(TableCell);
const ValueTableCell = withStyles((theme) => ({
    body: {
        fontSize: 16,
        fontWeight: 'bold',
        color: 'green'
    }
}))(TableCell);

export const AadharDetails = ({ request }) => {
    const ProfileImage = (
        <img
            src={request?.ProfileImage}
            height={100}
            width={100}
            alt={request?.FullName}
        />
    );

    const rows = [
        EKYCData('Profile :', ProfileImage),
        EKYCData('Name :', request?.AadharName),
        EKYCData('Aadhaar :', request?.AadhaarNumber),
        EKYCData('Gender :', request?.Gender),
        EKYCData('Dob :', request?.Dob),
        EKYCData('Address :', request?.Address),
        EKYCData('Pin :', request?.Pin),
        EKYCData('Pan Number :', request?.PanNumber),
        EKYCData('Pan Name :', request?.PanName),
        EKYCData('Category :', request?.PanCategory)
    ];

    function EKYCData(Parameter, Values) {
        return { Parameter, Values };
    }

    return (
        <Paper>
            <Grid item xs={12} sm={12} md={12} lg={12}>
                <TableContainer aria-label="customized table">
                    <Table aria-label="customized table">
                        <TableBody>
                            {rows.map((row) => (
                                <TableRow key={row.Parameter}>
                                    <StyledTableCell align="left">
                                        {row.Parameter}
                                    </StyledTableCell>
                                    <ValueTableCell align="left">
                                        {row.Values}
                                    </ValueTableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </TableContainer>
            </Grid>
        </Paper>
    );
};
