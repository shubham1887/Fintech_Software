// Give the service worker access to Firebase Messaging.
// Note that you can only use Firebase Messaging here, other Firebase libraries
// are not available in the service worker.
importScripts('https://www.gstatic.com/firebasejs/7.23.0/firebase-app.js');
importScripts(
    'https://www.gstatic.com/firebasejs/7.23.0/firebase-messaging.js'
);

// Initialize the Firebase app in the service worker by passing in
// your app's Firebase config object.
// https://firebase.google.com/docs/web/setup#config-object
firebase.initializeApp({
    apiKey: 'AIzaSyBhJo9ghtmhaIJpyF5IOpRqEGaBqWFs1Us',
    authDomain: 'talk2merecharge-19b24.firebaseapp.com',
    databaseURL: 'https://talk2merecharge-19b24.firebaseio.com',
    projectId: 'talk2merecharge-19b24',
    storageBucket: 'talk2merecharge-19b24.appspot.com',
    messagingSenderId: '113463432462',
    appId: '1:113463432462:web:0bdd0db3aeabb1c6fd335f',
    measurementId: 'G-2DPE75JVMY'
});

// Retrieve an instance of Firebase Messaging so that it can handle background
// messages.
const messaging = firebase.messaging();

messaging.setBackgroundMessageHandler(function (payload) {
    
    // Customize notification here
    const notificationTitle = 'Background Message Title';
    const notificationOptions = {
        body: 'Background Message body.',
        icon: '/firebase-logo.png'
    };

    return self.registration.showNotification(
        notificationTitle,
        notificationOptions
    );
});
