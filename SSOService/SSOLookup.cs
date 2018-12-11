using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SSOService.Models;
using Novell.Directory.Ldap;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace SSOService
{
    public class SSOLookup
    {
        private const string SESSION_STATE = "SESSION_STATE";
        private const string SESSION_TIMELEFT = "SESSION_TIMELEFT";
        private const string SESSION_AUTHLEVEL = "SESSION_AUTHLEVEL";
        private const string USER_ID = "USER_ID";
        private const string USER_TYPE = "USER_TYPE";
        private const string USER_FIRSTNAME = "USER_FIRSTNAME";
        private const string USER_LASTNAME = "USER_LASTNAME";
        private const string USER_EMAIL = "USER_EMAIL";
        private const string USER_DOMAIN = "USER_DOMAIN";
        private const string ERROR_CODE = "ERROR_CODE";

        private IConfiguration _configuration;

        public SSOLookup(IConfiguration config)
        {
            _configuration = config;
        }

        private string GetStringValue(string key, bool isRequired=true)
        {
            string rval = _configuration[key];
            if (isRequired && String.IsNullOrEmpty(rval))
                throw new Exception($"Missing a value for '{key}' in config settings.");
            return rval;
        }
        private int GetIntValue(string key, bool isRequired = true)
        {
            string s = _configuration[key];
            int rval = -1;
            if (isRequired && String.IsNullOrEmpty(s))
                throw new Exception($"Missing a valid value for '{key}' in config settings.");
            if (!Int32.TryParse(s, out rval))
                throw new Exception($"Invalid Int value for '{key}'.");
            return rval;
        }
        private bool GetBoolValue(string key, bool isRequired = true)
        {
            string s = _configuration[key];
            bool rval = false;
            if (isRequired && String.IsNullOrEmpty(s))
                throw new Exception($"Missing a valid value for '{key}' in config settings.");
            if (!Boolean.TryParse(s, out rval))
                throw new Exception($"Invalid Int value for '{key}'.");
            return rval;
        }

        public SSOResponse VerifySSOSession(VerifyTokenInputParams inputParams)
        {
            SSOResponse rval = new SSOResponse();
            try
            {
                if (String.IsNullOrEmpty(inputParams.sso_token))
                    throw new Exception("Missing valid SSO Token.");

                string baseURL = "";
                Models.Environment enviro = inputParams.GetEnvironment();
                switch(enviro)
                {
                    case Models.Environment.Dev:
                        baseURL = GetStringValue("S_SSO_URL_DEV");
                        break;

                    case Models.Environment.QA:
                        baseURL = GetStringValue("S_SSO_URL_QA");
                        break;

                    default:
                        baseURL = GetStringValue("S_SSO_URL_PROD");
                        break;
                }

                // required format on call
                string ssoURL = String.Format("{0}/?session_id={1}&session_key={2}&session_appname={3}",
                    baseURL, inputParams.sso_token, GetStringValue("S_SSO_SessionKey"), GetStringValue("S_SSO_SessionAppName"));
                Uri ssoUri = new Uri(ssoURL);

                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(ssoUri);
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                Stream recStream = resp.GetResponseStream();
                Encoding encode = Encoding.GetEncoding("utf-8");
                StreamReader reader = new StreamReader(recStream, encode);
                string sResponse = reader.ReadToEnd();

                // we have a response now, loop through the key value pairs
                string[] vals = sResponse.Split(new char[2] { ':', '=' });

                int count = 0;
                while ((count + 1) < vals.Count())
                {
                    switch(vals[count].ToUpper())
                    {
                        case SESSION_STATE:
                            rval.session_state = vals[count + 1];
                            rval.has_valid_session = string.Compare(rval.session_state, "valid", true) == 0;
                            break;
                        case SESSION_TIMELEFT:
                            try
                            {
                                string[] ts = vals[count + 1].Split('.');
                                if(ts.Length > 5)
                                    rval.session_time_left = new DateTime(Int32.Parse(ts[0]), Int32.Parse(ts[1]), Int32.Parse(ts[2]),
                                        Int32.Parse(ts[3]), Int32.Parse(ts[4]), Int32.Parse(ts[5]));
                            }
                            catch (Exception) { }
                            break;
                        case SESSION_AUTHLEVEL:
                            rval.session_level = vals[count + 1];
                            break;
                        case USER_ID:
                            rval.User.login_id = vals[count + 1];
                            break;
                        case USER_TYPE:
                            rval.User.sso_user_type = vals[count + 1];
                            break;
                        case USER_FIRSTNAME:
                            rval.User.first_name = vals[count + 1];
                            break;
                        case USER_LASTNAME:
                            rval.User.last_name = vals[count + 1];
                            break;
                        case USER_EMAIL:
                            rval.User.email = vals[count + 1];
                            break;
                        case ERROR_CODE:
                            rval.error_code = vals[count + 1];
                            break;
                        default:
                            rval.User.attributes.Add(vals[count], vals[count + 1]);
                            break;
                    }
                    count += 2;
                }


            }
            catch(Exception ex)
            {
                rval.error_message = ex.Message;
            }
            return rval;
        }


        public SSOResponse SearchUser(SearchUserInputParams inputParams)
        {
            SSOResponse rval = new SSOResponse();
            try
            {
                User user = rval.User;
                SearchUser(inputParams.GetEnvironment(), ref user);
            }
            catch (Exception ex)
            {

            }
            return rval;
        }
        public bool SearchUser(Models.Environment enviro, ref User user)
        {
            bool rval = false;
            try
            {
                
                string server = "", bindPwd = "";
                switch (enviro)
                {
                    case Models.Environment.QA:
                        server = GetStringValue("S_ldap_ED_server_QA");
                        bindPwd = GetStringValue("S_ldap_ED_bindPwd_QA");
                        break;

                    case Models.Environment.Dev:
                        server = GetStringValue("S_ldap_ED_server_Dev");
                        bindPwd = GetStringValue("S_ldap_ED_bindPwd_Dev");
                        break;

                    default: // Prod
                        server = GetStringValue("S_ldap_ED_server");
                        bindPwd = GetStringValue("S_ldap_ED_bindPwd");
                        break;
                }
                ILdapConnection ldapConn = new LdapConnection() { SecureSocketLayer = GetBoolValue("S_ldap_ED_isSSL") }; 
                ldapConn.Connect(server, GetIntValue("S_ldap_ED_port")); 
                ldapConn.Bind(GetStringValue("S_ldap_ED_bindName"), bindPwd);

                string userSearch = String.Format(GetStringValue("S_ldap_ED_userNameFrmt"), user.login_id);
                string baseSearch = "";  // "ou=People,o=eaton.com";
                LdapSearchResults search = ldapConn.Search(baseSearch, LdapConnection.SCOPE_SUB, userSearch, null, false);
                if (search != null) // && search.Count > 0)
                {
                    LdapEntry le = search.First<LdapEntry>();
                    if (le != null)
                    {
                        string name = le.DN;
                        LdapAttributeSet set = le.getAttributeSet();
                        if (set != null)
                        {
                            IEnumerator ienum = set.GetEnumerator();
                            while (ienum.MoveNext())
                            {
                                LdapAttribute attribute = (LdapAttribute)ienum.Current;
                                string attributeName = attribute.Name;
                                string attributeVal = attribute.StringValue;

                                if (String.Compare(attributeName, GetStringValue("S_ED_FirstName"), true) == 0)
                                    user.first_name = attributeVal;
                                else if (String.Compare(attributeName, GetStringValue("S_ED_LastName"), true) == 0)
                                    user.last_name = attributeVal;
                                else if (String.Compare(attributeName, GetStringValue("S_ED_Email"), true) == 0)
                                    user.email = attributeVal;
                                else
                                    user.attributes.Add(attributeName, attributeVal);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception in SearchUser(), details: " + ex.ToString());
                throw ex;
            }
            return rval;
        }
    }
}
