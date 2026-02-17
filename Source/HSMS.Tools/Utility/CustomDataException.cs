namespace HSMS.Utility
{
    public enum ExceptionType
    {
        Unknown = 0,            //error was not handle by backend 
        Unhandle = 1,           //error was handle by backend, but it don't know what to do
        InvalidData = 2,        //error raised in SP / Trigger due to invalid data
        Reference = 3,          //errir raised due to deletion of record which was referred by another record
        DuplicateData = 4,      //error thrown by sql due to unique constraint on table.
        InvalidFieldValue = 5   //error thrown if value of not-nullable field is either null or having an invalid value
    }

    [Serializable]
    public class CustomDataException : Exception
    {
        #region Constants
        public static readonly string DATA_ERROR = "[SMILE_DATA_ERROR]:";
        public static readonly string INTERNAL_DATA_ERROR = "[INTERNAL_DATA_ERROR]:";
        public static readonly string UNHANDLE_ERROR = "[SMILE_UNHANDLE_ERROR]:";
        public static readonly string REQ_SP_PWD = "[REQ_SP_PWD]";
        public static readonly int RC_INVALID_DATA = 444; //Response Code [444] - Invalid Data Entered by user
        public static readonly int RC_UNKNOWN_ERROR = 445;    //Response Code [445] - Unknown Error
        #endregion

        #region Local Variables
        private Exception _baseException;
        #endregion

        #region Fields
        public bool IsCustomDataException { get; set; }
        public string CustomErrorMessage { get; set; }
        public int ErrorCode { get; set; }

        public ExceptionType Type { get; set; }
        public bool IsUnhandleException { get; set; }
        public string ErrorMessage { get; set; }
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public int ResponseStatusCode { get; set; }
        public bool RequireSpPwd { get; set; }

        public string LoginURL { get; set; }
        #endregion

        #region Constructor
        public CustomDataException(Exception baseException) : base(baseException.Message)
        {
            this._baseException = baseException;

            this.IsCustomDataException = false;
            this.Type = ExceptionType.Unhandle;

            ProcessMessage();
        }
        public CustomDataException(string strMessage, int intErrCode)
        {
            this.IsCustomDataException = true;
            this.ErrorCode = intErrCode;
            this.ResponseStatusCode = RC_INVALID_DATA;
            if (this.IsCustomDataException)
            {
                this.CustomErrorMessage = strMessage;
            } else
            {
                this.ErrorMessage = strMessage;
            }
            this.RequireSpPwd = false;
        }
        #endregion

        #region Private Methods
        private void ProcessMessage()
        {
            string strErrMsg = GetErrorMessage(_baseException);

            if (this.IsCustomDataException)
            {
                this.CustomErrorMessage = strErrMsg;
            } else
            {
                strErrMsg = "Following internal error occured...<br><br>" + strErrMsg + "<br>";
                this.ErrorMessage = strErrMsg;
            }
        }
        private string GetErrorMessage(Exception ex)
        {
            string strErrMsg = "";
            string strHtml = "";
            Exception innerException = ex.InnerException;
            int intIndex = 0; //***

            if (ex != null)
            {
                strErrMsg = ex.Message;

                if (strErrMsg.IndexOf(DATA_ERROR) != -1 || strErrMsg.IndexOf(INTERNAL_DATA_ERROR) != -1)
                {
                    //Custom error thrown by us will have SMILE_DATA_ERROR string as prefix
                    if (strErrMsg.IndexOf(DATA_ERROR) != -1)
                    {
                        intIndex = strErrMsg.IndexOf(DATA_ERROR);
                        strErrMsg = strErrMsg.Substring(intIndex + DATA_ERROR.Length);
                    } else if (strErrMsg.IndexOf(INTERNAL_DATA_ERROR) != -1)
                    {
                        intIndex = strErrMsg.IndexOf(INTERNAL_DATA_ERROR);
                        strErrMsg = strErrMsg.Substring(intIndex + INTERNAL_DATA_ERROR.Length);
                    }
                    this.IsCustomDataException = true;
                    this.Type = ExceptionType.InvalidData;
                    this.ResponseStatusCode = RC_INVALID_DATA;
                    strHtml = strErrMsg;

                    //Check if custom error message also contains REQ_SP_PWD string
                    intIndex = strErrMsg.IndexOf(REQ_SP_PWD);
                    if (intIndex != -1)
                    {
                        strErrMsg = strErrMsg.Substring(intIndex + REQ_SP_PWD.Length);
                        this.RequireSpPwd = true;
                        strHtml = strErrMsg;
                    }
                } else if (strErrMsg.ToLower().Contains("delete") && strErrMsg.ToLower().Contains("reference"))
                {
                    //Integrity error thrown by SQL while deleting a record.
                    this.Type = ExceptionType.Reference;
                    strErrMsg = "Can not delete current record as it is being referred by another record.";
                    strHtml = strErrMsg;
                } else
                {
                    //Unhandle exception thrown by SQL.
                    intIndex = strErrMsg.IndexOf(UNHANDLE_ERROR);
                    if (intIndex != -1)
                    {
                        strErrMsg = strErrMsg.Substring(intIndex + UNHANDLE_ERROR.Length);
                        this.IsUnhandleException = true;
                        this.Type = ExceptionType.Unhandle;
                        this.ResponseStatusCode = RC_INVALID_DATA;
                        strHtml = strErrMsg;
                    } else
                    {
                        this.Type = ExceptionType.Unknown;
                        this.ResponseStatusCode = RC_UNKNOWN_ERROR;
                    }

                    //Check whether error message contains any known constraint exception raised by sql
                    if (strErrMsg.ToLower().Contains("duplicate") == true)
                    {
                        //Error is due to unique constraint, so extract table name and field name from the error.
                        //Error message will be following format...
                        //{"Cannot insert duplicate key row in object 'dbo.<table name>' with unique index 'IX_<tableName>_<IndexFieldName>'.\r\nThe statement has been terminated."}
                        //We can get TableName and FieldName from Index Name which will be standard format (i.e. IX_<TableName>_<FieldName>_..<FieldName>). So partse
                        //the error to get Index Name and from it extract TableName and Fields Name.                        

                        intIndex = strErrMsg.ToLower().IndexOf("ix_");
                        if (intIndex != -1)
                        {
                            string strIndexName = "";
                            string[] indexParts;

                            strIndexName = strErrMsg.Substring(intIndex);
                            strIndexName = strIndexName.Replace("\r", " ").Replace("\n", " ").Replace("'", "").Replace("  ", " ").Trim();
                            intIndex = strIndexName.IndexOf(" ");
                            if (intIndex != -1)
                            {
                                strIndexName = strIndexName.Substring(0, intIndex).Trim();
                            }

                            indexParts = strIndexName.Split('_');
                            if (indexParts != null && indexParts.GetLength(0) > 0)
                            {
                                this.TableName = indexParts[1];

                                for (var i = 2; i < indexParts.GetLength(0); i++)
                                {
                                    if (String.IsNullOrEmpty(this.FieldName) == false)
                                    {
                                        this.FieldName += " + ";
                                    }
                                    this.FieldName += indexParts[i];
                                }
                            }
                        }

                        if (!String.IsNullOrEmpty(this.TableName) && !String.IsNullOrEmpty(this.FieldName))
                        {
                            this.Type = ExceptionType.DuplicateData;
                            strHtml = "Duplicate data found. Please enter unique value for <b>" + this.FieldName + "</b>";
                        }
                    } else if (strErrMsg.ToLower().Contains("insert statement conflicted with the foreign key constraint") == true ||
                               strErrMsg.ToLower().Contains("update statement conflicted with the foreign key constraint") == true)
                    {
                        ////this error will occur if either Invalid Foreign Key is selected or it is blank

                        //Get Foreign Key Constraint name, it will have name of the field which is having an issue
                        intIndex = strErrMsg.ToLower().IndexOf("fk_");
                        if (intIndex != -1)
                        {
                            string strIndexName = "";
                            string[] indexParts;

                            strIndexName = strErrMsg.Substring(intIndex);
                            intIndex = strIndexName.IndexOf(" ");
                            //remove un-wanted characters from index (i.e. FK constraint) name
                            strIndexName = strIndexName.Replace("\r", " ").Replace("\n", " ").Replace("'", "").Replace("\"", "").Replace(".", " ").Replace("  ", " ").Trim();
                            intIndex = strIndexName.IndexOf(" ");
                            if (intIndex != -1)
                            {
                                strIndexName = strIndexName.Substring(0, intIndex).Trim();
                            }

                            indexParts = strIndexName.Split('_');
                            if (indexParts != null && indexParts.GetLength(0) > 0)
                            {
                                this.TableName = indexParts[1];

                                for (var i = 2; i < indexParts.GetLength(0); i++)
                                {
                                    if (String.IsNullOrEmpty(this.FieldName) == false)
                                    {
                                        this.FieldName += " + ";
                                    }
                                    this.FieldName += indexParts[i];
                                }
                            }

                            //as we are retreiving Field name from FK_ constraint name, which may contain ID as suffix in field name so we will remove it
                            if (String.IsNullOrEmpty(this.FieldName) == false && this.FieldName.Length > 0 && this.FieldName.Substring(this.FieldName.Length - 2) == "ID")
                            {
                                this.FieldName = this.FieldName.Substring(0, this.FieldName.Length - 2);
                            }
                        }

                        if (!String.IsNullOrEmpty(this.TableName) && !String.IsNullOrEmpty(this.FieldName))
                        {
                            this.Type = ExceptionType.InvalidFieldValue;
                            strHtml = "Value of field <b>" + this.FieldName + "</b> is invalid and also it cannot be blank. Please select valid value.";
                        }
                    }
                }

                if (this.Type == ExceptionType.Unknown || this.Type == ExceptionType.Unhandle)
                {
                    strHtml = "<br><div style='padding:0 0 0 4px;border:none;border-left:dashed 1px #ccc;'>" + strErrMsg + "<br>";
                    while (innerException != null)
                    {
                        strErrMsg = GetErrorMessage(innerException);
                        if (!this.IsCustomDataException)
                        {
                            strHtml += GetErrorMessage(innerException);
                            innerException = innerException.InnerException;
                        } else
                        {
                            strHtml = strErrMsg;
                            break;
                        }
                    }

                    if (!this.IsCustomDataException)
                    {
                        strHtml += "</div><br>";
                    }
                }
            }

            return strHtml;
        }
        #endregion
    }
}
