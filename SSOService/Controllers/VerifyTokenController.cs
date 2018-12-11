using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SSOService.Models;

namespace SSOService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerifyTokenController : ControllerBase
    {
        private IConfiguration _configuration;

        public VerifyTokenController(IConfiguration config)
        {
            _configuration = config;
        }

        // GET: api/VerifyToken
        [HttpGet]
        public string Get()
        {
            //return new string[] { "value1", "value2" };
            return "Service is Running";
        }

        // POST: api/VerifyToken
        [HttpPost]
        public string Post([FromBody] VerifyTokenInputParams inputParams)
        {
            try
            {
                /*
                    JObject rval = new JObject();
                    rval["IsAuthenticated"] = false;
                    try
                    {

                        JObject userObj = new JObject();
                        userObj["FirstName"] = "Test";

                        rval.Add("User", userObj);

                        return rval.ToString();
                    }
                    catch (Exception ex)
                    {
                        rval["Error"] = ex.ToString();
                    }
                    return rval.ToString();
                    */

                SSOLookup worker = new SSOLookup(_configuration);
                SSOResponse resp = worker.VerifySSOSession(inputParams);

                if(inputParams.search_ldap_dir && resp.has_valid_session && !String.IsNullOrEmpty(resp.User.login_id))
                {
                    User user = resp.User;
                    worker.SearchUser(inputParams.GetEnvironment(), ref user);
                }

                //return JsonConvert.SerializeObject(resp, Formatting.Indented);
                string rval = JsonConvert.SerializeObject(resp, Formatting.Indented);

                return rval;

            }
            catch (Exception ex)
            {
                return "{\"error_message\" : \"{0}\"" + ex.Message + "\"}";
            }
        }


        // POST: api/VerifyToken
        [HttpPost("[action]")]
        public SSOResponse VerifyToken([FromBody] VerifyTokenInputParams inputParams)
        {
            try
            {
                /*
                    JObject rval = new JObject();
                    rval["IsAuthenticated"] = false;
                    try
                    {

                        JObject userObj = new JObject();
                        userObj["FirstName"] = "Test";

                        rval.Add("User", userObj);

                        return rval.ToString();
                    }
                    catch (Exception ex)
                    {
                        rval["Error"] = ex.ToString();
                    }
                    return rval.ToString();
                    */

                SSOLookup worker = new SSOLookup(_configuration);
                SSOResponse resp = worker.VerifySSOSession(inputParams);

                if (inputParams.search_ldap_dir)
                {
                    User user = resp.User;
                    worker.SearchUser(inputParams.GetEnvironment(), ref user);
                }

                return resp;

            }
            catch (Exception ex)
            {
                return new SSOResponse() { error_message = "Exception in VerifyToken(), details: " + ex.Message };
            }
        }
    }
}
