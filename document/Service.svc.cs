using document.Library;
using document.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;

namespace document
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
   
    /**
     * Author: jl
     *      
     *  
     */
    public class Service : IService
    {
        private ContextDB db;
        private string connection;
        private string _pin;
        private string _branch;
        private bool _HasPassword = true; 
        private static string username = ConfigurationManager.AppSettings["Username"].ToString();
        private static string password = ConfigurationManager.AppSettings["Password"].ToString();
        private static string domain = ConfigurationManager.AppSettings["Domain"].ToString();
     //   private static string ScannerPath = ConfigurationManager.AppSettings["ScannerPath"].ToString();
        private static string DefaultDirectory = ConfigurationManager.AppSettings["DefaultDirectory"].ToString();


        private string ScannerLocation(string branch) {
            string path = "";
            switch (branch)
            {
                case "01":
                    connection = "name=ContextDB";
                    path = ConfigurationManager.AppSettings["ScannerPath"].ToString();
                    break;
                case "02":
                    connection = "name=ContextDBOSK";
                    path = ConfigurationManager.AppSettings["ScannerPathOsk"].ToString();
                    break;
                default:
                    path = DefaultDirectory;
                    break;
            }
            _branch = branch;
            db = new ContextDB(connection);
            return path;
        }
        private string BackupLocation(string branch) 
        {
            string path = DefaultDirectory ; 
            switch (branch)
            {
            case "01" :
                    connection = "name=ContextDB";
                    path = ConfigurationManager.AppSettings["TokyoBackupPath"].ToString();
                break;
            case "02" :
                    connection = "name=ContextDBOSK";
                    path = ConfigurationManager.AppSettings["OsakaBackupPath"].ToString();
                break;
            default:
                    path = DefaultDirectory; 
                break;
            }
            _branch = branch;
            db = new ContextDB(connection);
            return path;
        }        
        
        public bool CopyToBackUp(string pin, string branch_id)
        {
            string folder = (Math.Floor( Convert.ToInt32(pin) / 5000.0) * 5000).ToString() ; // GetRange
           
            string targetFullPath = Path.Combine( @BackupLocation(branch_id) , folder.PadLeft( 6 , '0' ) , pin );
            string sourceFullPath = Path.Combine( @ScannerLocation(branch_id), pin );
            var diSource = 
                new DirectoryInfo( @sourceFullPath );
            var diTarget = 
                new DirectoryInfo( @targetFullPath );
            _pin = pin;
            _branch = branch_id;
            
            return CopyAll(diSource, diTarget) == 0 ? true : false ;
        }

        //move folder from original destination to new destination
        public bool MoveBackupFolder(string pin, string ReplaceWith, string branch_id) {
            try
            {
                string _origin = (Math.Floor(Convert.ToInt32(pin) / 5000.0) * 5000).ToString(); // GetRange
                string _ReplaceWith = (Math.Floor(Convert.ToInt32(ReplaceWith) / 5000.0) * 5000).ToString(); // GetRange
                string _originFullPath = Path.Combine( @BackupLocation(branch_id), _origin.PadLeft(6, '0'), pin);
                string _targetFullPath = Path.Combine( @BackupLocation(branch_id), _ReplaceWith.PadLeft(6, '0'), ReplaceWith);

                //initialize
                _pin = ReplaceWith;
                _branch = branch_id;
                _HasPassword = false;

                var diSource = new DirectoryInfo( @_originFullPath );
                var diTarget = new DirectoryInfo( @_targetFullPath );

                Efiling table = new Efiling();
                table.Created = DateTime.Now;
                table.Directory = _targetFullPath;
                table.FilePassword = (_HasPassword) ?  System.Web.Security.Membership.GeneratePassword(12, 4) : null;
                table.PIN = ReplaceWith;
                table.FileName = "*";
                db.Entry(table).State = System.Data.Entity.EntityState.Added;

                //Protect with password
                //https://www.nuget.org/packages/Syncfusion.Pdf.WinForms
                //https://www.syncfusion.com/kb/9503/how-to-protect-a-pdf-with-the-password-using-c-and-vb-net

                // If Success. Save the Changes. 
                db.SaveChanges();
                
                Log logs = new Log();
                logs.Pin = pin;
                logs.Location = _targetFullPath;
                logs.OriginalLocation = _originFullPath;
                logs.Action = "MoveFolder";
                logs.Created = DateTime.Now;
                db.Entry(logs).State = System.Data.Entity.EntityState.Added;
                db.SaveChanges();
              
                CopyAll(diSource, diTarget);

                return true;
            }
            catch (Exception e) {
                Console.Write(e.Message.ToString());
                return false;
            }
        }
        public bool MoveBackupFile(string _originPin, string _originFileName, string _targetPin, string _targetFileName, string branch_id)
        {
            try
            {
                string _origin = (Math.Floor(Convert.ToInt32(_originPin) / 5000.0) * 5000).ToString(); // GetRange
                string _ReplaceWith = (Math.Floor(Convert.ToInt32(_targetPin) / 5000.0) * 5000).ToString(); // GetRange
                string _originFullPath = Path.Combine( BackupLocation(branch_id), _origin.PadLeft(6, '0'), _originFileName);
                string _targetFullPath = Path.Combine( BackupLocation(branch_id), _ReplaceWith.PadLeft(6, '0'), _targetPin , _targetFileName);
                // To move a file or folder to a new location:
                // System.IO.File.Move(_originFullPath, _targetFullPath);
                Efiling table = new Efiling();
                table.Created = DateTime.Now;
                table.Directory = _targetFullPath;                
                table.FilePassword = (_HasPassword) ? System.Web.Security.Membership.GeneratePassword(12, 4) : null;
                table.PIN = _targetPin;
                table.FileName = _targetFileName;
                db.Entry(table).State = System.Data.Entity.EntityState.Added;

                Log logs = new Log();
                logs.Pin = _originPin;
                logs.Location = _targetFullPath;
                logs.OriginalLocation = _originFullPath;
                logs.Action = "MoveFile";
                logs.Created = DateTime.Now;
                db.Entry(logs).State = System.Data.Entity.EntityState.Added;
                db.SaveChanges();

                System.IO.File.Move(_originFullPath, _targetFullPath);
                
                return true;
            }
            catch (Exception e)
            {
                Console.Write(e.Message.ToString());
                return false;
            }
        }

        public int CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            try {
                using (new NetworkConnection( @ScannerLocation(_branch) , username, password))
                {
                    Directory.CreateDirectory(target.FullName);
                    foreach (FileInfo fi in source.GetFiles())
                    {
                        Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                        
                        fi.MoveTo(Path.Combine(target.FullName, fi.Name));
                        try
                        {
                            Efiling table = new Efiling();
                            table.Created = DateTime.Now;
                            table.Directory = target.FullName.ToString();
                            table.FilePassword = (_HasPassword ) ? System.Web.Security.Membership.GeneratePassword(12, 2) : null; //password
                            table.PIN = _pin;
                            table.FileName = fi.Name;
                            db.Entry(table).State = System.Data.Entity.EntityState.Added;

                            //protect with password
                            //https://www.nuget.org/packages/Syncfusion.Pdf.WinForms
                            //https://www.syncfusion.com/kb/9503/how-to-protect-a-pdf-with-the-password-using-c-and-vb-net

                            Log logs = new Log();
                            logs.Pin = _pin;
                            logs.Location = Path.Combine(target.FullName, fi.Name);
                            logs.OriginalLocation = fi.FullName;
                            logs.Action = "Copied";
                            logs.Created = DateTime.Now;
                            db.Entry(logs).State = System.Data.Entity.EntityState.Added;
                            db.SaveChanges();
                        }
                        catch (Exception e) {
                            continue;
                        }
                    }

                    // Copy each subdirectory using recursion.
                    foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                    {
                        DirectoryInfo nextTargetSubDir =
                            target.CreateSubdirectory(diSourceSubDir.Name);
                        CopyAll(diSourceSubDir, nextTargetSubDir);
                    }
                }
            } catch (Exception e) {
                return 1;
            }

            return 0;
        }

        public string CreateStagingDirectory(string pin, string branch_id)
        {            
            try {
            _branch = branch_id;
            _pin = pin;
            string sourceFullPath =Path.Combine( ScannerLocation(branch_id) , pin);
            using (new NetworkConnection(@ScannerLocation(_branch), username, password)) {
                DirectoryInfo di = Directory.CreateDirectory(@sourceFullPath);
                return di.FullName.ToString();
            }

            }
            catch (UnauthorizedAccessException e)
            {
                return e.Message.ToString();
            };           
        }

        public bool DeleteStagingDirectory(string pin, string branch_id)
        {
            try
            {
                _branch = branch_id;
                _pin = pin;
                string sourceFullPath = Path.Combine( ScannerLocation(branch_id)  , pin);
                
                using (new NetworkConnection( ScannerLocation(branch_id), username, password))
                {
                    DirectoryInfo di = Directory.CreateDirectory( @sourceFullPath );
                    di.Delete(true);
                    
                    /*
                     Log logs = new Log();
                     logs.Pin = pin;
                     logs.Location = sourceFullPath;

                     logs.Action = "DeleteDirectory";
                     logs.Created = DateTime.Now;
                     db.Entry(logs).State = System.Data.Entity.EntityState.Added;
                     db.SaveChanges();*/
                }
            }
            catch (UnauthorizedAccessException) {
                return false;
            }
            return true;
        }

        public string[] GetStagingFiles(string pin, string branch_id) { //get files from staging folder
            try
            {
                _branch = branch_id;
                _pin = pin;
                string[] filePaths = null;
                using (new NetworkConnection(ScannerLocation(branch_id) , username, password))
                {
                    // filePaths = Directory.GetDirectories(ScannerPath); 
                    string sourceFullPath = Path.Combine( ScannerLocation(branch_id), pin);
                    // https://stackoverflow.com/questions/14877237/getting-all-file-names-from-a-folder-using-c-sharp
                    filePaths = Directory.GetFiles( @sourceFullPath );
                }
                return filePaths;
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException();
            }
        }

       
        public string[] GetStagingDirectories(string branch_id) {
            try
            {
                string[] dir = null;
                using (new NetworkConnection( @ScannerLocation(branch_id), username, password )) {
                    dir = Directory.GetDirectories( @ScannerLocation(branch_id));

                }
                return dir;
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException();
            }
        }

        public bool DeleteStagingFiles(string pin, string fileName , string branch_id) //delete file from staging folder
        {
            try
            {
                string sourceFullPath = Path.Combine( @ScannerLocation(branch_id), pin);
                using (new NetworkConnection( @ScannerLocation(branch_id), username, password ))
                {
                    if (File.Exists(Path.Combine(sourceFullPath, fileName)))
                    {
                        File.Delete(Path.Combine(sourceFullPath, fileName));
                        return true; //file deleted
                    }
                }                
                return false; //filenot found 
            }
            catch (UnauthorizedAccessException)
            {                
                return false; //exception 
            }
        }

        public List<Efiling> GetFiles(string pin , string branch_id) {
            BackupLocation(branch_id);
            return db.Efiling.Where(s=> s.PIN == pin).ToList();
        }

    }
}
