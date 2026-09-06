import { useHistory } from 'react-router-dom';
function NavigationHistory(path) {
    const history = useHistory();
    history.push(path);
}

export default NavigationHistory;
