using document.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;


namespace document
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        bool CopyToBackUp(string pin, string branch_id);

        [OperationContract]
        bool MoveBackupFolder(string originalPin, string ReplaceWith, string branch_id);

        [OperationContract]
        bool MoveBackupFile(string originalPin, string originalFileName, string targetPin, string targetFileName, string branch_id);

        [OperationContract]
        string CreateStagingDirectory(string pin, string branch_id);

        [OperationContract]
        bool DeleteStagingDirectory(string pin, string branch_id);

        [OperationContract]
        bool DeleteStagingFiles(string pin, string fileName, string branch_id);

        //bool DeleteStagingFile(string filename, string)

        [OperationContract]
        string[] GetStagingDirectories( string branch_id);

        [OperationContract]
        string[] GetStagingFiles( string pin, string branch_id);

        [OperationContract]
        List<Efiling> GetFiles(string pin, string branch_id);
    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    /*
    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
    */

}
