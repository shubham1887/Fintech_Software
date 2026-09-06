using Project.BAL;
using Project.Response.ProviderResponse;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Project.Processing
{
    public class TransactionResponseProcessing
    {
        public bool IsValidForCallBack(string status)
        {
            ResponseProcess resProcess = new ResponseProcess();

            if (resProcess.CheckFailure(status))
            {
                return true;
            }

            if (resProcess.CheckSuccess(status))
            {
                return true;
            }

            return false;
        }

        public string ParseBalance(string bal)
        {
            try
            {
                if (bal == null || bal.TrimIfNotNull().Length <= 0)
                {
                    return "0";
                }

                string balance = bal;

                string[] arr = bal.Split(" ");
                balance = arr[0].Replace(",", "");
                balance = balance.Replace("\"", "");
                balance = balance.TrimEnd('.');
                balance = balance.Trim();

                if (balance.EqualsIgnoreCase(Messages.MatcherParamNotFound))
                {
                    balance = "0";
                }

                if (balance.Trim().Length <= 0)
                {
                    balance = "0";
                }
                return balance;
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
                return "0";
            }
        }

        public string RemoveCommaSpecialChar(string txnID)
        {
            if (txnID == null)
                return "";

            txnID = txnID.Replace(",", "");
            txnID = txnID.Replace("\"", "");
            txnID = txnID.Replace("-", "_");
            txnID = txnID.Replace(".", "");
            txnID = Regex.Replace(txnID, @"\s+", "_");

            txnID = txnID.TrimEnd('.');

            return txnID.Trim();
        }

        private bool IsValidRequestID(string RequestID, int ProviderID, ProviderReturnResponse TxnResponse)
        {
            if (RequestID == null)
            {
                return false;
            }

            Response.TransactionMaster.ViewModel Model = new TransactionBal().SelectByRequestIDWithProviderID(ProviderID, RequestID);
            if (Model == null)
            {
                return false;
            }

            if (Model.ProviderID != ProviderID)
            {
                return false;
            }

            //if (!new ResponseProcess().checkProcess(Model.Status))
            //{
            //    return false;
            //}

            TxnResponse.AmountInString = Model.Amount.ToString();
            TxnResponse.ConsumerNumber = Model.ConsumerNumber.ToString();

            return true;
        }

        private bool isValidTextInResponse(string textMustExist, string ResponseText, string message)
        {
            if (textMustExist.Trim().Length <= 1)
            {
                return false;
            }

            if (ResponseText.Trim().Length <= 3)
            {
                return false;
            }

            if (!message.ToUpper().Contains(textMustExist.ToUpper()))
            {
                return false;
            }

            if (!ResponseText.ToUpper().Contains(textMustExist.ToUpper()))
            {
                return false;
            }

            return true;
        }

        private bool canTakeRequestIDIfAvialble(string Type)
        {
            string initial = Enum.GetName(typeof(ResponseType), ResponseType.Initial);

            if (Type.EqualsIgnoreCase(initial))
            {
                return true;
            }

            string statusCheck = Enum.GetName(typeof(ResponseType), ResponseType.StatusCheck);

            if (Type.EqualsIgnoreCase(statusCheck))
            {
                return true;
            }

            return false;
        }

        public ProviderReturnResponse ExtractResponseFromText(string ResponseText, string Type, int ProviderID, string RequestID, ProviderReturnResponse TxnResponse)
        {
            string DecodedResponse = HttpUtility.UrlDecode(ResponseText);
            ResponseText = DecodedResponse;

            TxnResponse.Response = DecodedResponse;

            ProviderResponseMasterBal objBAL = new ProviderResponseMasterBal();

            List<Response.ProviderResponse.ViewModel> listSuccess = objBAL.SelectForParsing(ProviderID, StatusType.Success, Type);
            List<Response.ProviderResponse.ViewModel> listFailure = objBAL.SelectForParsing(ProviderID, StatusType.Failure, Type);
            List<Response.ProviderResponse.ViewModel> listPending = objBAL.SelectForParsing(ProviderID, StatusType.Process, Type);
            List<Response.ProviderResponse.ViewModel> listResend = objBAL.SelectForParsing(ProviderID, StatusType.Resend, Type);

            MatcherProcessing matcher = new MatcherProcessing();

            string initial = Enum.GetName(typeof(ResponseType), ResponseType.Initial);
            string callback = Enum.GetName(typeof(ResponseType), ResponseType.Callback);
            string balanCheck = Enum.GetName(typeof(ResponseType), ResponseType.BalanceCheck);
            string statusCheck = Enum.GetName(typeof(ResponseType), ResponseType.StatusCheck);

            string view = Enum.GetName(typeof(ResponseType), ResponseType.View);

            bool isBalanceCheck = false;

            if (Type.EqualsIgnoreCase(balanCheck))
            {
                isBalanceCheck = true;
            }
            bool isView = false;
            if (Type.EqualsIgnoreCase(view))
            {
                isView = true;
            }
            //[RequestID] [Amount] [Number] [TxnID] [Message] [ClBal] [DMTBalance] [LapuNo]

            bool isFound = false;

            #region Success Parsing

            foreach (ViewModel item in listSuccess)
            {
                try
                {
                    string message = item.Response.ToString();
                    string textMustExist = item.TextMustExists.ToString();

                    bool isValidText = isValidTextInResponse(textMustExist, ResponseText, message);

                    if (!isValidText)
                    {
                        continue;
                    }

                    if (canTakeRequestIDIfAvialble(Type))
                    {
                        TxnResponse.RequestID = RequestID;
                    }
                    else
                    {
                        TxnResponse.RequestID = matcher.getDifference(message, ResponseText, ResponseParamCode.RequestID);
                    }

                    //if (!matcher.isExtrated(TxnResponse.RequestID))
                    //{
                    //    TxnResponse.RequestID = RequestID;
                    //}

                    if (!matcher.isExtrated(TxnResponse.RequestID) && isBalanceCheck == false && isView == false)
                    {
                        continue;
                    }

                    bool flag = IsValidRequestID(TxnResponse.RequestID, ProviderID, TxnResponse);

                    if (flag == false && isBalanceCheck == false && isView == false)
                    {
                        continue;
                    }

                    if (isView)
                    {
                        string billAmount = "";
                        Parallel.Invoke(
                        () =>
                        {
                            TxnResponse.CustomerName = matcher.getDifference(message, ResponseText, ResponseParamCode.CustomerName);
                        }
                        ,
                        () =>
                        {
                            TxnResponse.DueDate = matcher.getDifference(message, ResponseText, ResponseParamCode.DueDate);
                        },
                        () =>
                        {
                            TxnResponse.BBPSViewID = matcher.getDifference(message, ResponseText, ResponseParamCode.BBPSViewID);
                        },
                        () =>
                        {
                            TxnResponse.ResponseParam1 = matcher.getDifference(message, ResponseText, ResponseParamCode.ResponseParam1);
                        },

                        () =>
                        {
                            TxnResponse.ResponseParam2 = matcher.getDifference(message, ResponseText, ResponseParamCode.ResponseParam2);
                        },

                        () =>
                        {
                            TxnResponse.ResponseParam3 = matcher.getDifference(message, ResponseText, ResponseParamCode.ResponseParam3);
                        },

                        () =>
                        {
                            TxnResponse.ResponseParam4 = matcher.getDifference(message, ResponseText, ResponseParamCode.ResponseParam4);
                        },

                        () =>
                        {
                            TxnResponse.ResponseParam5 = matcher.getDifference(message, ResponseText, ResponseParamCode.ResponseParam5);
                        },
                        () =>
                        {
                            billAmount = matcher.getDifference(message, ResponseText, ResponseParamCode.Amount);
                        }
                        );

                        TxnResponse.Amount = Convert.ToDecimal(ParseBalance(billAmount));
                    }
                    else
                    {
                        Parallel.Invoke(
                        () =>
                        {
                            TxnResponse.Balance = matcher.getDifference(message, ResponseText, ResponseParamCode.ClBal);
                        },
                        () =>
                        {
                            TxnResponse.DMTBalance = matcher.getDifference(message, ResponseText, ResponseParamCode.DMTBalance);
                        },
                        () =>
                        {
                            TxnResponse.LapuNo = matcher.getDifference(message, ResponseText, ResponseParamCode.LapuNo);
                        },
                        () =>
                        {
                            TxnResponse.Description = matcher.getDifference(message, ResponseText, ResponseParamCode.Message);
                        },
                        () =>
                        {
                            TxnResponse.TransactionID = matcher.getDifference(message, ResponseText, ResponseParamCode.TxnID);
                        },
                        () =>
                        {
                            TxnResponse.ConsumerNumber = matcher.getDifference(message, ResponseText, ResponseParamCode.Number);
                        },
                        () =>
                        {
                            TxnResponse.ProviderCommission = matcher.getDifference(message, ResponseText, ResponseParamCode.ApiComm);
                        },
                        () =>
                        {
                            TxnResponse.ProviderCommissionPercentage = matcher.getDifference(message, ResponseText, ResponseParamCode.ApiCommPer);
                        },
                        () =>
                        {
                            TxnResponse.ProviderRefNo = matcher.getDifference(message, ResponseText, ResponseParamCode.RefID);
                        },
                        () =>
                        {
                            TxnResponse.CustomerName = matcher.getDifference(message, ResponseText, ResponseParamCode.CustomerName);
                        },
                         () =>
                        {
                            TxnResponse.ROfferCommission = matcher.getDifference(message, ResponseText, ResponseParamCode.ROffer);
                        }
                        );
                    }

                    TxnResponse.TransactionID = RemoveCommaSpecialChar(TxnResponse.TransactionID);
                    TxnResponse.Balance = ParseBalance(TxnResponse.Balance);
                    TxnResponse.DMTBalance = ParseBalance(TxnResponse.DMTBalance);
                    TxnResponse.ProviderCommission = ParseBalance(TxnResponse.ProviderCommission);

                    if (isBalanceCheck == true)
                    {
                        isFound = true;
                        TxnResponse.Status = StatusType.Success;
                        break;
                    }

                    if ((TxnResponse.Amount > 0) && isView)
                    {
                        isFound = true;
                        TxnResponse.Status = StatusType.Success;
                        break;
                    }
                    //block for blank TransactionID
                    if (!matcher.isTransactionIDValid(TxnResponse.TransactionID))
                    {
                        if (Company.BlankTransactionIDAllowed.EqualsIgnoreCase("Yes") ||
                            TxnResponse.TransactionID.EqualsIgnoreCase("TRANSACTION_PENDING") ||
                            TxnResponse.TransactionID.EqualsIgnoreCase("TRANSACTION PENDING"))
                        {
                            TxnResponse.TransactionID = new Util().GetRandomTransactionID();
                        }
                    }
                    if (matcher.isExtrated(TxnResponse.TransactionID))
                    {
                        decimal amt = Convert.ToDecimal(TxnResponse.AmountInString);
                        TxnResponse.Amount = amt;
                        isFound = true;
                        TxnResponse.Status = StatusType.Success;

                        if (TxnResponse.Description.EqualsIgnoreCase(Messages.MatcherParamNotFound))
                        {
                            TxnResponse.Description = textMustExist;
                        }

                        break;
                    }
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }

            #endregion Success Parsing

            #region Fail Parsing

            if (isFound == false)
            {
                foreach (ViewModel item in listFailure)
                {
                    try
                    {
                        string message = item.Response.ToString();

                        string textMustExist = item.TextMustExists.ToString();

                        bool isValidText = isValidTextInResponse(textMustExist, ResponseText, message);

                        if (!isValidText)
                        {
                            continue;
                        }

                        if (canTakeRequestIDIfAvialble(Type))
                        {
                            TxnResponse.RequestID = RequestID;
                        }
                        else
                        {
                            TxnResponse.RequestID = matcher.getDifference(message, ResponseText, ResponseParamCode.RequestID);
                        }

                        //if (!matcher.isExtrated(TxnResponse.RequestID) && Type.EqualsIgnoreCase(initial))
                        //{
                        //    TxnResponse.RequestID = RequestID;
                        //}

                        if (!matcher.isExtrated(TxnResponse.RequestID) && isView == false)
                        {
                            continue;
                        }

                        bool flag = IsValidRequestID(TxnResponse.RequestID, ProviderID, TxnResponse);

                        if (flag == false && isView == false)
                        {
                            continue;
                        }

                        if (isView)
                        {
                            Parallel.Invoke(
                               () =>
                               {
                                   TxnResponse.Message = matcher.getDifference(message, ResponseText, ResponseParamCode.Message);
                               });
                        }
                        else
                        {
                            Parallel.Invoke(
                            () =>
                            {
                                TxnResponse.Balance = matcher.getDifference(message, ResponseText, ResponseParamCode.ClBal);
                            },
                            () =>
                            {
                                TxnResponse.DMTBalance = matcher.getDifference(message, ResponseText, ResponseParamCode.DMTBalance);
                            },
                            () =>
                            {
                                TxnResponse.LapuNo = matcher.getDifference(message, ResponseText, ResponseParamCode.LapuNo);
                            },
                            () =>
                            {
                                TxnResponse.Description = matcher.getDifference(message, ResponseText, ResponseParamCode.Message);
                            },
                            () =>
                            {
                                TxnResponse.ProviderRefNo = matcher.getDifference(message, ResponseText, ResponseParamCode.RefID);
                            },
                            () =>
                            {
                                TxnResponse.ProviderCommission = matcher.getDifference(message, ResponseText, ResponseParamCode.ApiComm);
                            },
                            () =>
                            {
                                TxnResponse.TransactionID = matcher.getDifference(message, ResponseText, ResponseParamCode.TxnID);
                            }
                            );
                        }

                        TxnResponse.TransactionID = RemoveCommaSpecialChar(TxnResponse.TransactionID);
                        TxnResponse.Balance = ParseBalance(TxnResponse.Balance);
                        TxnResponse.DMTBalance = ParseBalance(TxnResponse.DMTBalance);
                        TxnResponse.ProviderCommission = ParseBalance(TxnResponse.ProviderCommission);

                        TxnResponse.Status = StatusType.Failure;

                        if (TxnResponse.Status == StatusType.Failure && isView)
                        {
                            isFound = true;
                            TxnResponse.Status = StatusType.Failure;
                            break;
                        }

                        if (matcher.isExtrated(TxnResponse.RequestID))
                        {
                            decimal amt = Convert.ToDecimal(TxnResponse.AmountInString);
                            TxnResponse.Amount = amt;

                            isFound = true;
                            TxnResponse.Status = StatusType.Failure;
                            if (TxnResponse.Description.EqualsIgnoreCase(Messages.MatcherParamNotFound))
                            {
                                TxnResponse.Description = textMustExist;
                            }
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                }
            }

            #endregion Fail Parsing

            #region Pending Parsing

            if (isFound == false && !Type.EqualsIgnoreCase(statusCheck) && !Type.EqualsIgnoreCase(balanCheck))
            {
                foreach (ViewModel item in listPending)
                {
                    try
                    {
                        string message = item.Response.ToString();
                        string textMustExist = item.TextMustExists.ToString();

                        bool isValidText = isValidTextInResponse(textMustExist, ResponseText, message);

                        if (!isValidText)
                        {
                            continue;
                        }

                        if (canTakeRequestIDIfAvialble(Type))
                        {
                            TxnResponse.RequestID = RequestID;
                        }
                        else
                        {
                            TxnResponse.RequestID = matcher.getDifference(message, ResponseText, ResponseParamCode.RequestID);
                        }

                        //TxnResponse.RequestID = matcher.getDifference(message, ResponseText, ResponseParamCode.RequestID);

                        if (!matcher.isExtrated(TxnResponse.RequestID) && Type.EqualsIgnoreCase(initial))
                        {
                            TxnResponse.RequestID = RequestID;
                        }

                        if (!matcher.isExtrated(TxnResponse.RequestID))
                        {
                            continue;
                        }

                        bool flag = IsValidRequestID(TxnResponse.RequestID, ProviderID, TxnResponse);

                        if (flag == false)
                        {
                            continue;
                        }

                        Parallel.Invoke(
                            () =>
                            {
                                TxnResponse.Balance = matcher.getDifference(message, ResponseText, ResponseParamCode.ClBal);
                            },
                            () =>
                            {
                                TxnResponse.DMTBalance = matcher.getDifference(message, ResponseText, ResponseParamCode.DMTBalance);
                            },
                            () =>
                            {
                                TxnResponse.LapuNo = matcher.getDifference(message, ResponseText, ResponseParamCode.LapuNo);
                            },
                            () =>
                            {
                                TxnResponse.Description = matcher.getDifference(message, ResponseText, ResponseParamCode.Message);
                            },
                            () =>
                            {
                                TxnResponse.ProviderRefNo = matcher.getDifference(message, ResponseText, ResponseParamCode.RefID);
                            },
                            () =>
                            {
                                TxnResponse.ProviderCommission = matcher.getDifference(message, ResponseText, ResponseParamCode.ApiComm);
                            },
                            () =>
                            {
                                TxnResponse.TransactionID = matcher.getDifference(message, ResponseText, ResponseParamCode.TxnID);
                            }
                        );

                        TxnResponse.TransactionID = RemoveCommaSpecialChar(TxnResponse.TransactionID);
                        TxnResponse.Balance = ParseBalance(TxnResponse.Balance);
                        TxnResponse.DMTBalance = ParseBalance(TxnResponse.DMTBalance);
                        TxnResponse.ProviderCommission = ParseBalance(TxnResponse.ProviderCommission);

                        if (matcher.isExtrated(TxnResponse.ConsumerNumber))
                        {
                            decimal amt = Convert.ToDecimal(TxnResponse.AmountInString);
                            TxnResponse.Amount = amt;

                            isFound = true;
                            TxnResponse.Status = StatusType.Process;
                            if (TxnResponse.Description.EqualsIgnoreCase(Messages.MatcherParamNotFound))
                            {
                                TxnResponse.Description = textMustExist;
                            }
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                }
            }

            #endregion Pending Parsing

            #region Resend Parsing

            if (isFound == false && Type.EqualsIgnoreCase(statusCheck))
            {
                foreach (ViewModel item in listResend)
                {
                    try
                    {
                        string message = item.Response.ToString();
                        string textMustExist = item.TextMustExists.ToString();

                        bool isValidText = isValidTextInResponse(textMustExist, ResponseText, message);

                        if (!isValidText)
                        {
                            continue;
                        }

                        TxnResponse.RequestID = RequestID;

                        bool flag = IsValidRequestID(TxnResponse.RequestID, ProviderID, TxnResponse);

                        if (flag == false)
                        {
                            continue;
                        }

                        Parallel.Invoke(
                            () =>
                            {
                                TxnResponse.Balance = matcher.getDifference(message, ResponseText, ResponseParamCode.ClBal);
                            },
                            () =>
                            {
                                TxnResponse.DMTBalance = matcher.getDifference(message, ResponseText, ResponseParamCode.DMTBalance);
                            },
                            () =>
                            {
                                TxnResponse.LapuNo = matcher.getDifference(message, ResponseText, ResponseParamCode.LapuNo);
                            },
                            () =>
                            {
                                TxnResponse.Description = matcher.getDifference(message, ResponseText, ResponseParamCode.Message);
                            },
                            () =>
                            {
                                TxnResponse.TransactionID = matcher.getDifference(message, ResponseText, ResponseParamCode.TxnID);
                            },
                            () =>
                            {
                                TxnResponse.ProviderCommission = matcher.getDifference(message, ResponseText, ResponseParamCode.ApiComm);
                            },
                            () =>
                            {
                                TxnResponse.ProviderRefNo = matcher.getDifference(message, ResponseText, ResponseParamCode.RefID);
                            }
                        );

                        TxnResponse.TransactionID = RemoveCommaSpecialChar(TxnResponse.TransactionID);
                        TxnResponse.Balance = ParseBalance(TxnResponse.Balance);
                        TxnResponse.DMTBalance = ParseBalance(TxnResponse.DMTBalance);
                        TxnResponse.ProviderCommission = ParseBalance(TxnResponse.ProviderCommission);

                        if (matcher.isExtrated(TxnResponse.ConsumerNumber))
                        {
                            decimal amt = Convert.ToDecimal(TxnResponse.AmountInString);
                            TxnResponse.Amount = amt;

                            isFound = true;
                            TxnResponse.Status = StatusType.Resend;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                }
            }

            #endregion Resend Parsing

            if (isFound == false)
            {
                TxnResponse.Status = StatusType.Process;
            }

            if (TxnResponse.Description.EqualsIgnoreCase(Messages.MatcherParamNotFound))
            {
                TxnResponse.Description = "";
            }

            if (TxnResponse.TransactionID.EqualsIgnoreCase(Messages.MatcherParamNotFound))
            {
                TxnResponse.TransactionID = "";
            }

            return TxnResponse;
        }
    }
}