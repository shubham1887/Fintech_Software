import { initializeApp } from 'firebase/app';
import firebase from 'firebase/compat/app';

//import 'firebase/analytics';
import { getMessaging, getToken } from 'firebase/messaging';

import { UpdateFCMToken as UPDATE_FCM_TOKEN } from 'Components/Api/UsersApi';

const firebaseConfig = {
    apiKey: 'AIzaSyBhJo9ghtmhaIJpyF5IOpRqEGaBqWFs1Us',
    authDomain: 'talk2merecharge-19b24.firebaseapp.com',
    databaseURL: 'https://talk2merecharge-19b24.firebaseio.com',
    projectId: 'talk2merecharge-19b24',
    storageBucket: 'talk2merecharge-19b24.appspot.com',
    messagingSenderId: '113463432462',
    appId: '1:113463432462:web:0bdd0db3aeabb1c6fd335f',
    measurementId: 'G-2DPE75JVMY'
};

async function initFireBase() {
    try {
        if (!firebase) return;

        const isSupport = firebase?.messaging?.isSupported();

        if (isSupport === undefined) return;

        if (isSupport === false) return;

        if (firebase?.apps?.length !== 0) return;

        const initializedFirebaseApp = initializeApp(firebaseConfig);

        //firebase.analytics(initializedFirebaseApp);
        const messaging = getMessaging(initializedFirebaseApp);

        //messaging.onMessage((payload) => {});

        if (IsBrowserSupportsNotification()) {
            Notification.requestPermission().then(function (permission) {
                if (permission === 'granted') {
                    GetTokenAndUpdate(messaging);
                }
            });
        }
    } catch (err) {
        console.log('error', err);
    }
}

const requestPermission = () => {
    Notification.requestPermission().then((permission) => {
        if (permission === 'granted') {
        } else {
        }
    });
};

const IsBrowserSupportsNotification = () => {
    if ('Notification' in window) return true;
    return false;
};

const GetTokenAndUpdate = async (messaging) => {
    getToken({
        vapidKey:
            'BKRQjsJSrnn7BtUuljFP_krd3AY1Xszuw863JxY3TclfIRl6hJ1mlmCsqGfstxqu-EjB4bVkKxf4nw-uJwnfnBM'
    })
        .then((token) => {
            if (token) {
                if (token === null || token === undefined) return;
                let Request = {
                    WebFCMToken: token,
                    AppFCMToken: ''
                };

                let jSONObj = JSON.stringify(Request);

                UPDATE_FCM_TOKEN(jSONObj);
            } else {
                // Show permission request.

                // Show permission UI.
                requestPermission();
                //setTokenSentToServer(false);
            }
        })
        .catch((err) => {
            //showToken('Error retrieving registration token. ', err);
            //setTokenSentToServer(false);
        });
};

export default initFireBase;
