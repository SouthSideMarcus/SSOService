using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSOService.Models
{
    public enum Environment
    {
        Unknown = 0,
        Dev = 1,
        QA = 2,
        Prod = 3
    }

    public enum Portal
    {
        Unknown = 0,
        JOE = 1,
        MyEaton = 2
    }
    public class InputParams
    {
        private Environment _enviro = Models.Environment.Unknown;
        private Portal _portal = Models.Portal.Unknown;
        public InputParams()
        {
            _enviro = Models.Environment.Prod; // default to Prod
        }


        // Options for Portal
        // JOE, MyEaton
        public String portal
        {
            get
            {
                switch (_portal)
                {
                    case Models.Portal.JOE:
                        return "JOE";
                    case Models.Portal.MyEaton:
                        return "MyEaton";
                    default:
                        return "Unknown";
                }
            }
            set
            {
                switch (value)
                {
                    case "joe":
                    case "JOE":
                    case "Joe":
                        _portal = Models.Portal.JOE;
                        break;

                    case "MyEaton":
                    case "myeaton":
                    case "MYEATON":
                        _portal = Models.Portal.MyEaton;
                        break;

                    default:
                        _portal = Models.Portal.Unknown;
                        break;
                }
            }
        }
        public Portal GetPortal()
        {
            return _portal;
        }

        // Options for Environment
        // Dev, QA, Prod
        public String environment
        {
            get
            {
                switch (_enviro)
                {
                    case Models.Environment.Prod:
                        return "Prod";
                    case Models.Environment.Dev:
                        return "Dev";
                    case Models.Environment.QA:
                        return "QA";
                    default:
                        return "Unkown";
                }
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    _enviro = Models.Environment.Prod; // default to prod
                }
                else
                {
                    switch (value)
                    {
                        case "Prod":
                        case "prod":
                            _enviro = Models.Environment.Prod;
                            break;
                        case "Dev":
                        case "dev":
                            _enviro = Models.Environment.Dev;
                            break;
                        case "QA":
                        case "qa":
                            _enviro = Models.Environment.QA;
                            break;
                        default:
                            _enviro = Models.Environment.Unknown;
                            break;
                    }
                }
            }
        }
        public Environment GetEnvironment()
        {
            return _enviro;
        }
    }
    public class VerifyTokenInputParams : InputParams
    {
        public VerifyTokenInputParams()
            : base()
        {
            search_ldap_dir = true;
        }

        public String sso_token { get; set; }

        public Boolean search_ldap_dir { get; set; }

    }
    public class SearchUserInputParams : InputParams
    {
        public SearchUserInputParams()
            : base()
        {
        }

        public String login_id { get; set; }


    }


    public class User
    {
        public User()
        {
            login_id = "";
            first_name = "";
            last_name = "";
            email = "";
            sso_user_type = "";
        }
        public String login_id { get; set; }
        public String first_name { get; set; }
        public String last_name { get; set; }
        public String email { get; set; }
        public String sso_user_type { get; set; }


        private Dictionary<string, string> _attrs = new Dictionary<string, string>();
        public Dictionary<string, string> attributes { get { return _attrs; } }
    }

    public class SSOResponse
    {
        private User _user = new User();

        public SSOResponse()
        {
            has_valid_session = false;
            session_state = "";
            session_time_left = DateTime.MinValue;
            session_level = "";
            error_message = "";
        }

        public Boolean has_valid_session { get; set; }
        public String session_state { get; set; }
        public DateTime session_time_left { get; set; }
        public String session_level { get; set; }
        public String error_code { get; set; }
        public String error_message { get; set; }
                
        public User User { get { return _user; } }
    }

}
