import React, { lazy } from 'react';
import { Route } from 'react-router-dom';
import OuterLayoutRoute from 'Pages/Layouts/OuterLayoutRoute';
import Enquiry from 'Pages/OuterPages/EnquiryDetails';
import Signup from 'Pages/OuterPages/Signup';
import SignupWithEkyc from 'Pages/OuterPages/SignupWithEkyc';
import IndexSignUp from 'Pages/OuterPages/IndexSignUp';


import Login from 'Pages/OuterPages/Login';



const ForgotPasswordGenerateOTP = lazy(() =>
    import('Pages/OuterPages/ForgotPasswordGenerateOTP')
);



const OuterLayoutNavigator = () => {
    const RoutePath = [
        '/Login',
        '/Signup',
        '/AboutUs',
        '/PublicServices',
        '/TermsAndConditions',
        '/PrivacyPolicy',
        '/RefundAndCanclePolicy',
        '/ContactUs',
        '/Home',
        '/ForgotPassword',


        '/SignupWithEkyc',
        '/IndexSignUp'
    ];
    return (
        <Route path={RoutePath}>
            <OuterLayoutRoute path="/Login" component={Login} />
            <OuterLayoutRoute path="/Enquiry" component={Enquiry} />
            <OuterLayoutRoute
                path="/ForgotPassword"
                component={ForgotPasswordGenerateOTP}
            />
            <OuterLayoutRoute path="/Signup" component={Signup} />
            <OuterLayoutRoute
                path="/SignupWithEkyc"
                component={SignupWithEkyc}
            />
            <OuterLayoutRoute path="/IndexSignUp" component={IndexSignUp} />
        </Route>
    );
};

export default OuterLayoutNavigator;
