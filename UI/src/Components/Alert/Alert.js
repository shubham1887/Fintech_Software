import { useEffect, useCallback } from 'react';
import 'react-toastify/dist/ReactToastify.css';
import { toast } from 'react-toastify';
import { debounce } from 'lodash';

const CustomeAlert = ({
    isOpen,
    severity,
    alertMessage,
    onCloseCallback,
    closeTime
}) => {
    const run = (alertMessage, severity) => {
        const customId = severity + '_' + alertMessage;

        if (toast.isActive(customId)) {
            toast.update(customId);
            return;
        }

        // if (alertMessage.length <= 0) {
        //     toast.dismiss(customId);
        //     return;
        // }

        if (severity === 'error') {
            toast.error(alertMessage, {
                position: 'bottom-right',
                pauseOnHover: false,
                draggable: true,
                progress: undefined,
                toastId: customId
            });
        } else if (severity === 'success') {
            toast.success(alertMessage, {
                position: 'bottom-right',
                pauseOnHover: false,
                draggable: true,
                progress: undefined,
                toastId: customId
            });
        } else {
            toast.info(alertMessage, {
                position: 'bottom-right',
                pauseOnHover: false,
                draggable: true,
                progress: undefined,
                toastId: customId
            });
        }
    };

    const SnackbarShow = useCallback(
        debounce((alertMessage, severity) => run(alertMessage, severity), 150),
        []
    );

    useEffect(() => {
        if (alertMessage === undefined) {
            return;
        }

        if (alertMessage.length <= 2) {
            return;
        }

        toast.configure();
        SnackbarShow(alertMessage, severity);
    }, [alertMessage, severity]);

    toast.configure({
        autoClose: 10 * 1000,
        draggable: true,
        position: toast.POSITION.BOTTOM_RIGHT
    });

    return null;
};
export default CustomeAlert;
