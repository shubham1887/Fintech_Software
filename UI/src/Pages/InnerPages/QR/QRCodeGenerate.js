import React, { useState, useEffect } from 'react';
import { Container, Grid } from '@material-ui/core';
import { useLocation } from 'react-router-dom';
import { useHistory } from 'react-router';
import HOCLoader from 'Components/Loader/HOCLoader';
import { TransactionStatus as TRANSACTION_STATUS } from 'Components/Api/PaymentGatewayAPI';
import { LOGOUT } from 'ConstComponents';
import GPAY_LOGO from 'Assets/Images/GpayPhonepe.jpg';
import BHIM_LOGO from 'Assets/Images/bhim-upi.png';
import PAYTM_LOGO from 'Assets/Images/Paytm_Logo.png';
import AMAZONPAY_LOGO from 'Assets/Images/Amazon-Pay.png';
import { CLIENT_NAME } from 'Config';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
const CustomContainer = HOCLoader(Container);

export const QRCodeGenerate = () => {
    const location = useLocation();
    const classes = commonStyle();
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [cName, setCName] = useState('');
    async function fetchCName() {
        setCName(await CLIENT_NAME());
    }
    const OrderID = location.state.OrderID;

    const [imageQRCode, setimageQRCode] = useState(location.state.QRImage);
    const [seconds, setSeconds] = useState(60);
    const history = useHistory();

    const GPayLogoImage = () => {
        return (
            <img
                src={GPAY_LOGO}
                height={50}
                width={90}
                style={{ padding: '10px' }}
                alt="BBPS"
            />
        );
    };
    const PaytmLogoImage = () => {
        return (
            <img
                src={PAYTM_LOGO}
                height={50}
                width={90}
                style={{ padding: '10px' }}
                alt="BBPS"
            />
        );
    };
    const BhimLogoImage = () => {
        return (
            <img
                src={BHIM_LOGO}
                height={50}
                width={90}
                style={{ padding: '10px' }}
                alt="BBPS"
            />
        );
    };
    const AmazonPayLogoImage = () => {
        return (
            <img
                src={AMAZONPAY_LOGO}
                height={70}
                width={90}
                style={{ padding: '10px' }}
                alt="BBPS"
            />
        );
    };
    useEffect(() => {
        fetchCName();
    }, []);

    useEffect(() => {
        let secInt = 60;
        const interval = setInterval(() => {
            if (secInt <= 0) {
                history.push('/PGSuccess?OrderID=' + OrderID);
                return;
            }

            secInt = secInt - 1;
            setSeconds(secInt);

            if (secInt === 30) {
                StatusCheck(OrderID);
                return;
            }
        }, 1000);
        return () => clearInterval(interval);
    }, []);

    const StatusCheck = async (OrderID) => {
        var inputObj = {
            OrderID: OrderID
        };

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoadingOpen(true);
        setResponseMessage('');

        var response = await TRANSACTION_STATUS(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);

            setIsError(true);
            return;
        }

        //var data = JSON.parse(response.data);

        history.push('/PGSuccess?OrderID=' + OrderID);
        return;
    };

    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
        >
            <Grid container justify="center" style={{ marginTop: '10px' }}>
                <h2 className={classes.imageHeader}>{cName}</h2>
            </Grid>
            <Grid container justify="center" style={{ marginTop: '10px' }}>
                {imageQRCode.length <= 2 ? null : (
                    <img
                        src={imageQRCode}
                        height="300px"
                        width="300px"
                        alt="QR code"
                    />
                )}
            </Grid>
            <h3 className={classes.imageHeader}>
                Scan & Pay with any BHIM, UPI app
            </h3>
            <h3 style={{ textAlign: 'center' }}>
                <GPayLogoImage />
                <PaytmLogoImage />
                <BhimLogoImage />
                <AmazonPayLogoImage />
            </h3>

            <h3 style={{ marginTop: '10px', textAlign: 'center' }}>
                Time Remaining: {seconds}s
            </h3>
        </CustomContainer>
    );
};
