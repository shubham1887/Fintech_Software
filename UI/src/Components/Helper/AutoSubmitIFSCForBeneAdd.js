import { useEffect } from 'react';
import { useFormikContext } from 'formik';

const AutoSubmitIFSCForBeneAdd = ({ callback }) => {
    const { values, submitForm, setFieldValue } = useFormikContext();

    useEffect(() => {
        if (values.IFSC === undefined) {
            return;
        }
        if (values.IFSC === null) {
            return;
        }
        if (values.IFSC.length <= 10) {
            return;
        }
        //setFieldValue('isFetchBranch', true);
        //setTimeout(submitForm, 1);
        callback(values);
    }, [values.IFSC]);
    return null;
};

export default AutoSubmitIFSCForBeneAdd;
