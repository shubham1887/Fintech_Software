import { useEffect } from 'react';
import { useFormikContext } from 'formik';

const AutoSubmitConsumerNumber = ({ callback }) => {
    const { values } = useFormikContext();
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

export default AutoSubmitConsumerNumber;
