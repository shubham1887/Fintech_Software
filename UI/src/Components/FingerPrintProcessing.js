import { XMLParser } from 'fast-xml-parser';
import axios from 'axios';

let pid_options = `<?xml version="1.0"?>
<PidOptions ver="1.0"><Opts fCount="1" fType="2" iCount="0" pCount="0" format="0" pidVer="2.0" timeout="10000" wadh="[wadh]" posh="UNKNOWN" env="PP" /><CustOpts></CustOpts></PidOptions>`;

export const GetFingerPrint = async (wadh) => {
    const { url, infoURL, SuccessFlag } = await discoverAvdm();
    let fingerPrintXML = '';
    if (SuccessFlag === 0) {
        return {
            status: false,
            xmldata: 'Connection failed Please try again.'
        };
    }

    let value = wadh;
    pid_options = pid_options.replace('[wadh]', value);

    await axios({ method: 'CAPTURE', url: url, data: pid_options }).then(
        (data_xml) => {
            fingerPrintXML = data_xml.data;
        }
    );

    return { status: true, xmldata: fingerPrintXML };
};

const discoverAvdm = async () => {
    let SuccessFlag = 0;
    let primaryUrl = 'http://127.0.0.1:';
    let caURL = '';
    let inURL = '';
    SuccessFlag = 0;
    for (let i = 11100; i <= 11112; i++) {
        const { captureURL, infoURL, SuccessFlag } = await getURL(
            primaryUrl,
            i
        );
        caURL = captureURL;
        inURL = infoURL;

        if (SuccessFlag === 1) {
            return { url: caURL, infoURL: inURL, SuccessFlag: SuccessFlag };
        }
    }

    if (SuccessFlag === 0) {
        return { url: '', infoURL: '', SuccessFlag: SuccessFlag };
    }

    return { url: caURL, infoURL: inURL, SuccessFlag: SuccessFlag };
};

const GetPIDOptions = (deviceInfo, pidOptions) => {
    if (deviceInfo.includes('Precision')) {
        return (pidOptions = `<?xml version="1.0"?><PidOptions ver="1.0"><Opts fCount="1" fType="0" iCount="0" iType="" pCount="0" pType="" format="0" pidVer="2.0" timeout="10000" otp="" wadh="[wadh]" posh="UNKNOWN" env="PP" /> <Demo></Demo> <CustOpts><Param name="" Stringue="" /></CustOpts></PidOptions>`);
    }
    return pidOptions;
};
const getURL = async (primaryUrl, i) => {
    let captureURL = '';
    let infoURL = '';
    let SuccessFlag = 0;

    await axios({ method: 'RDSERVICE', url: primaryUrl + i.toString() })
        .then((data_xml) => {
            let finalUrl = primaryUrl + i.toString();

            const options = {
                attributeNamePrefix: '_',
                //attrNodeName: false,
                //textNodeName : "#text",
                ignoreAttributes: false,
                ignoreNameSpace: false,
                removeNSPrefix: false
            };

            const parser = new XMLParser(options);

            var data = parser.parse(data_xml.data);

            let deviceStatus = data.RDService._status;
            let deviceInfo = data.RDService._info;

            let Interface = data.RDService.Interface;
            let deviceInfoPath = '';
            let deviceCapturePath = '';

            Interface.map((item) => {
                if (item._id === 'CAPTURE') {
                    deviceCapturePath = item._path;
                }
                if (item._id === 'DEVICEINFO') {
                    deviceInfoPath = item._path;
                }
            });

            if (deviceInfo.includes('morpho')) {
                deviceCapturePath = '/capture';
                deviceInfoPath = '/getDeviceInfo';
            }

            pid_options = GetPIDOptions(deviceInfo, pid_options);

            if (deviceStatus === 'READY') {
                captureURL = finalUrl + deviceCapturePath;
                infoURL = finalUrl + deviceInfoPath;
                SuccessFlag = 1;
            } else if (deviceStatus === 'USED') {
                captureURL = finalUrl + deviceCapturePath;
                infoURL = finalUrl + deviceInfoPath;
                SuccessFlag = 1;
            } else if (deviceStatus === 'NOTREADY') {
                captureURL = '';
                infoURL = '';
                SuccessFlag = 0;
            }
        })
        .catch((err) => console.log(err));
    return { captureURL, infoURL, SuccessFlag };
};
