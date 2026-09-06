import { useEffect } from 'react';
import { useFormikContext } from 'formik';

const AutoSubmitMobileForSender = ({ callback }) => {
    const { values, submitForm, setFieldValue } = useFormikContext();
    const DMTSenderNumberLength = parseInt(
        localStorage.getItem('DMTSenderNumberLength'),
        10
    );

    useEffect(() => {
        if (values.Mobile === undefined) {
            return;
        }
        if (values.Mobile === null) {
            return;
        }

        if (values.Mobile.length < DMTSenderNumberLength) {
            return;
        }

        setFieldValue('isAddBeneficiary', false);
        setTimeout(submitForm, 1);
    }, [values.Mobile, DMTSenderNumberLength]);

    useEffect(() => {
        if (values.ConsumerNumber === undefined) {
            return;
        }
        if (values.ConsumerNumber === null) {
            return;
        }
        if (values.ConsumerNumber.length <= 9) {
            return;
        }

        if (callback === undefined) {
            return;
        }
        if (callback === null) {
            return;
        }

        callback(values);
    }, [values.ConsumerNumber]);
    return null;
};

export default AutoSubmitMobileForSender;
