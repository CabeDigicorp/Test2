using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using LogicNP.CryptoLicensing;
using System.IO;
using System.Xml;
using System.Data.Common;
using System.Data.OleDb;
using System.Data;

namespace JoinLicenseService
{
    [WebService(Namespace = "http://www.ssware.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class LicenseServiceClass : LogicNP.CryptoLicensing.LicenseService
    {
        public LicenseServiceClass()
        {
            /*
            
            *******IMPORTANT***********
            
            Set the 'CryptoLicensing License Service' license code using the SetLicenseCode method below.
            
            Your license code can be found in the license information email we sent you when you purchased CryptoLicensing.
            
            There are two license codes, make sure you use the correct one. See http://www.ssware.com/support/viewtopic.php?t=734 for more information
           
            WARNING: If no or incorrect license code is specified, it reverts to trial mode. Codes generated in trial mode ALWAYS EXPIRE AFTER 30 DAYS.
            
            TIP: To ensure that you do not accidentally generate such codes in production systems, set CryptoLicenseGenerator.FailInTrialMode property to true.
            
            */

            //this.Generator.SetLicenseCode(""); // LEAVE BLANK when using trial version of CryptoLicensing.

            //this.Generator.SetLicenseCode("tgCBAMIyPMDi5tUB57KpFaz91QEBAGoATmFtZT1HaWFuIFBpZXJvI0NvbXBhbnk9I0VtYWlsPXByb2dyYW1tZXJAZGlnaWNvcnAuaXQjU3Vic2NyaXB0aW9uPUZhbHNlI1N1YnNjcmlwdGlvblZhbGlkVGlsbD0wNi1BdWctMjAxNwEHO8RdS9JczUVSuwaRRG7P4A2wzH+1lm6FzKBb2IqSBBFbYDCAwI6fwwPBIBPIglkz");// expired 2017
            //this.Generator.SetLicenseCode("lgKBANSzAFZ04dUBFAABAGoATmFtZT1HaWFuIFBpZXJvI0NvbXBhbnk9I0VtYWlsPXByb2dyYW1tZXJAZGlnaWNvcnAuaXQjU3Vic2NyaXB0aW9uPUZhbHNlI1N1YnNjcmlwdGlvblZhbGlkVGlsbD0wNi1BdWctMjAxNwEHbmlguvMrlJ2XHD38T26gKCd42959ngu9ItSkES1x8RH39k+oS4+HJilqRV7vSQr1");

            //CryptoLicensing For .Net 2018 Enterpise
            //this.Generator.SetLicenseCode("lgCAABWtGZsJ6NUBgwBOYW1lPUdpYW4gUGllcm8gVHJ1Y2NvbG8jQ29tcGFueT1EaWdpIENvcnAgcy5yLmwuI0VtYWlsPXByb2dyYW1tZXJAZGlnaWNvcnAuaXQjU3Vic2NyaXB0aW9uPUZhbHNlI1N1YnNjcmlwdGlvblZhbGlkVGlsbD0yMS1NYXItMjAyMAEHeedNGSqrtfsEtu6chPGMHekWm5N05LMo7N+6S+nvXm34urqzeE74HDnPgUipgjFt"); //enterprise editions

            //CryptoLicensing For .Net 2020 Enterpise
            this.Generator.SetLicenseCode("lgCBAOiilLbP1tcBAQCBAE5hbWU9R2lhbiBQaWVybyBUUlVDQ09MTyNDb21wYW55PURJR0kgQ09SUCBTLlIuTC4jRW1haWw9cGF0cml6aWFAZGlnaWNvcnAuaXQjU3Vic2NyaXB0aW9uPUZhbHNlI1N1YnNjcmlwdGlvblZhbGlkVGlsbD0xMC1EZWMtMjAyMQEHgAVeo1E7gOB7ljXmNh3sX6Buqi4mFsEZYN6aTfMckSLuxoxk9Iu9A32wNYtqWAej");
        }
        
        // Uncomment the following method override if you are using database which requires a DBWorker.
        // See help file topic "Supported Databases" for more information.
        // The DBWorkers can be found in the "DBWorkers" sub-folder under the install folder
//         public override DBWorker GetDBWorker(string connStr)
//         {
//             // Uncomment the line corresponding to your DBWorker
//             //return new MySQLDBWorker.MySQLDBWorker();
//             //return new OracleDBWorker.OracleDBWorker();
//             //return new SqlCeDBWorker.SqlCeDBWorker();
//             //return new PostGreDBWorker.PostGreDBWorker();
//         }        

    }

}