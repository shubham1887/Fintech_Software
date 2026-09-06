import React, { useState, useEffect, useRef } from 'react';

import { Container } from '@material-ui/core';

import { useLocation } from 'react-router-dom';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const PTransaction = () => {
    const formRef = useRef(null);

    const location = useLocation();
    const CallBackURL = location.state.CallBackURL;
    const FormActionURL = location.state.FormActionURL;
    const CheckSum = location.state.CheckSum;
    const OrderID = location.state.OrderID;
    const MerchantID = location.state.MerchantID;
    const Amount = location.state.Amount;
    const Name = location.state.Name;
    const Email = location.state.Email;
    const ContactNumber = location.state.ContactNumber;
    const ChannelID = location.state.ChannelID;
    const IndustryTypeID = location.state.IndustryTypeID;
    const Website = location.state.Website;
    const CustID = location.state.CustID;

    const initialValues = {
        CallBackURL: CallBackURL,
        CheckSum: CheckSum,
        FormActionURL: FormActionURL,
        OrderID: OrderID,
        MerchantID: MerchantID,
        Amount: Amount,
        Name: Name,
        Email: Email,
        ContactNumber: ContactNumber,
        ChannelID: ChannelID,
        IndustryTypeID: IndustryTypeID,
        Website: Website,
        CustID: CustID
    };

    const [FormObj, setFormObj] = useState(initialValues);

    useEffect(() => {
        formRef.current.submit();
    }, []);
    return (
        <CustomContainer>
            <form
                ref={formRef}
                id="form1"
                method="post"
                name="form1"
                action={FormObj.FormActionURL}
            >
                <input type="hidden" name="MID" value={FormObj.MerchantID} />
                <input
                    type="hidden"
                    name="CHANNEL_ID"
                    value={FormObj.ChannelID}
                />
                <input
                    type="hidden"
                    name="INDUSTRY_TYPE_ID"
                    value={FormObj.IndustryTypeID}
                />
                <input type="hidden" name="WEBSITE" value={FormObj.Website} />
                <input type="hidden" name="EMAIL" value={FormObj.Email} />
                <input
                    type="hidden"
                    name="MOBILE_NO"
                    value={FormObj.ContactNumber}
                />
                <input type="hidden" name="CUST_ID" value={FormObj.CustID} />
                <input type="hidden" name="ORDER_ID" value={FormObj.OrderID} />

                <input type="hidden" name="TXN_AMOUNT" value={FormObj.Amount} />

                <input
                    type="hidden"
                    name="CALLBACK_URL"
                    value={FormObj.CallBackURL}
                />
                <input
                    type="hidden"
                    name="CHECKSUMHASH"
                    value={FormObj.CheckSum}
                />
                <input type="submit" value="Redirect" />
            </form>
        </CustomContainer>
    );
};
