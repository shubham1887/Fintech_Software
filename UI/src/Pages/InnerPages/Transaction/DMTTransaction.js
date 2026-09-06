import React, { useState, useEffect } from 'react';
import { Grid } from '@material-ui/core';
import DmtIcon from 'Components/DashboardIcon/DmtIcon';
import { LOGOUT } from 'ConstComponents';
import { Container, Paper, IconButton } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { MultiActiveDMT as DMT_MULTI_ACTIVE } from 'Components/Api/DMTSettingsAPI';
import HOCLoader from 'Components/Loader/HOCLoader';
import { useHistory } from 'react-router';
export const DMTTransaction = ({ isDMTOne }) => {
    const history = useHistory();

    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [Actives, setActive] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [severity, setSeverity] = useState('success');
    const classes = commonStyle();
    const CustomContainer = HOCLoader(Container);

    const selectActiveDMT = async () => {
        const response = await DMT_MULTI_ACTIVE();

        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        let data = JSON.parse(response.data);
        if (data.length <= 1) {
            handleClick(
                data[0].Name,
                data[0].IsPayoutDMT,
                data[0].ProviderCode
            );
        }
        setActive(data);
    };

    const handleClick = (name, IsPayoutDMT, ProviderCode) => {
        if (IsPayoutDMT === true) {
            let param = {
                DMTName: name
            };
            history.push('/SenderSearch', param);
            return;
        }

        if (ProviderCode.toLowerCase() === 'RoundPay'.toLowerCase()) {
            let param = {
                DMTName: name
            };
            history.push('/ROP_DMTSenderSearch', param);
            return;
        }

        if (ProviderCode.toLowerCase() === 'ClareInfoTech'.toLowerCase()) {
            let param = {
                DMTName: name
            };
            history.push('/CIT_DMTSenderSearch', param);
            return;
        }

        if (ProviderCode.toLowerCase() === 'VastWebDMT'.toLowerCase()) {
            let param = {
                DMTName: name
            };
            history.push('/VW_DMTSenderSearch', param);
            return;
        }
        if (ProviderCode.toLowerCase() === 'Novity'.toLowerCase()) {
            let param = {
                DMTName: name
            };
            history.push('/NTDMT_AgentRegistration', param);
            return;
        }
        if (ProviderCode.toLowerCase() === 'Novity5L'.toLowerCase()) {
            let param = {
                DMTName: name
            };
            history.push('/NT5LDMT_AgentRegistration', param);
            return;
        } else if (name.toLowerCase() === 'LevinDMT'.toLowerCase()) {
            history.push('/LF_DMTSenderSearch');
            return;
        } else if (name.toLowerCase() === 'EZYPayV1DMT'.toLowerCase()) {
            history.push('/EZPV1SenderSearch');
            return;
        } else if (name.toLowerCase() === 'PDMT'.toLowerCase()) {
            history.push('/PaytmSenderSearch');
            return;
        } else if (name.toLowerCase() === 'AlleDMT'.toLowerCase()) {
            history.push('/AlleDMTSenderSearch');
            return;
        } else if (name.toLowerCase() === 'IMPSGuruDMT'.toLowerCase()) {
            let param = {
                DMTName: name
            };
            history.push('/IG_DMTSenderSearch', param);
            return;
        } else if (name.toLowerCase() === 'YesBankDMT'.toLowerCase()) {
            let param = {
                DMTName: name
            };
            history.push('/YBDMT_SenderSearch', param);
            return;
        } else if (name.toLowerCase() === 'IDMT'.toLowerCase()) {
            history.push('/IPSenderSearch');
            return;
        } else if (name.toLowerCase() === 'MasterMoneyDMT'.toLowerCase()) {
            history.push('/MMoneySenderSearch');
            return;
        } else if (name.toLowerCase() === 'MasterMoneyPureDMT'.toLowerCase()) {
            history.push('/MMoneyPSenderSearch');
            return;
        } else if (name.toLowerCase() === 'ePocketDMT'.toLowerCase()) {
            history.push('/ESenderSearch');
            return;
        } else if (name.toLowerCase() === 'OpenBankDMT'.toLowerCase()) {
            history.push('/OpenBankSenderSearch');
            return;
        } else if (name.toLowerCase() === 'PaytmCCFDMT'.toLowerCase()) {
            history.push('/PC_SenderSearch');
            return;
        } else if (name.toLowerCase() === 'PayinDMT'.toLowerCase()) {
            history.push('/WPC_SenderSearch');
            return;
        } else if (name.toLowerCase() === 'ICashDMT'.toLowerCase()) {
            history.push('/ICashSenderSearch');
            return;
        } else if (name.toLowerCase() === 'ICICI_Composite'.toLowerCase()) {
            history.push('/ICompositeSenderSearch');
            return;
        } else if (name.toLowerCase() === 'Mahagram'.toLowerCase()) {
            history.push('/MahagramDMTSenderSearch');
            return;
        } else if (name.toLowerCase() === 'Eko'.toLowerCase()) {
            history.push('/EkoDMTSenderSearch');
            return;
        } else if (name.toLowerCase() === 'Ambika'.toLowerCase()) {
            history.push('/AmbikaDMTSenderSearch');
            return;
        } else if (name.toLowerCase() === 'PayU'.toLowerCase()) {
            history.push('/PU_SenderSearch');
            return;
        } else if (name.toLowerCase() === 'Paysprint'.toLowerCase()) {
            history.push('/PS_SenderSearch');
            return;
        } else if (name.toLowerCase() === 'MyNetLinkDMT'.toLowerCase()) {
            history.push('/MNLDMTSender_Search');
            return;
        } else if (name.toLowerCase() === 'ZPlusDMT'.toLowerCase()) {
            history.push('/ZP_DMTSenderSearch');
            return;
        } else if (name.toLowerCase() === 'Cyberplate'.toLowerCase()) {
            history.push('/CDMT_SenderSearch');
            return;
        } else if (name.toLowerCase() === 'JaiJuiDMT'.toLowerCase()) {
            history.push('/JJ_SenderSearch');
            return;
        } else if (name.toLowerCase() === 'CashFree'.toLowerCase()) {
            history.push('/CF_SenderSearch');
            return;
        } else if (name.toLowerCase() === 'EzyPay'.toLowerCase()) {
            history.push('/EZPSenderSearch');
            return;
        } else if (name.toLowerCase() === 'EzyPayPaytm'.toLowerCase()) {
            history.push('/EZPTMSenderSearch');
            return;
        } else if (name.toLowerCase() === 'PrimePayDMT'.toLowerCase()) {
            history.push('/PP_SenderSearch');
            return;
        } else if (name.toLowerCase() === 'ZPlusPayout'.toLowerCase()) {
            history.push('/ZPP_SenderSearch');
            return;
        } else if (name.toLowerCase() === 'HyptoDMT'.toLowerCase()) {
            history.push('/H_SenderSearch');
            return;
        } else if (name.toLowerCase() === 'Razorpay'.toLowerCase()) {
            history.push('/RP_SenderSearch');
            return;
        }
        //else if (name.toLowerCase() === 'InstantPayPayout'.toLowerCase()) {
        //     history.push('/IP_SenderSearch');
        //     return;
        // }
    };

    useEffect(() => {
        selectActiveDMT();
    }, []);

    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={(alertState) => {
                setIsError(alertState);
            }}
        >
            <Paper className={classes.PaperStyle}>
                <Grid container>
                    {Actives.map((item) => (
                        <Grid
                            item
                            xs={12}
                            sm={12}
                            md={6}
                            lg={2}
                            xl={2}
                            className="text-center"
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={() => {
                                    handleClick(
                                        item.Name,
                                        item.IsPayoutDMT,
                                        item.ProviderCode
                                    );
                                }}
                            >
                                <DmtIcon name={item.DisplayName} />
                            </IconButton>
                        </Grid>
                    ))}
                </Grid>
            </Paper>
        </CustomContainer>
    );
};
