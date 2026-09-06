import { useEffect } from 'react';
import { useFormikContext } from 'formik';

const AutoSubmitPincode = ({ callback }) => {
    const { values, submitForm, setFieldValue } = useFormikContext();

    useEffect(() => {
        if (values.Pincode === undefined) {
            return;
        }
        if (values.Pincode === null) {
            return;
        }
        if (values.Pincode.length <= 5) {
            return;
        }
        callback(values);
    }, [values.Pincode]);
    return null;
};

export default AutoSubmitPincode;
