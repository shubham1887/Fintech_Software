import { initializeApp } from 'firebase/app';
import 'firebase/messaging';
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
const initializedFirebaseApp = initializeApp(firebaseConfig);

// firebase.analytics(firebaseConfig)
const messaging = initializedFirebaseApp.messaging();

messaging.onMessage(function (payload) {});
