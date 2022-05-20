using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DBI_eDahab.Web.Models;

namespace DBI_eDahab.Web.Controllers
{
    public class CustomerInfoController : ApiController
    {
        // GET: api/CustomerInfo

            Repository _repository = new Repository();
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/CustomerInfo/5
        public string Get(int id)
        {
           // var getCustomerInfo = _repository.GetCustomerInfo();
            return "value";
        }

        // POST: api/CustomerInfo
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/CustomerInfo/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/CustomerInfo/5
        public void Delete(int id)
        {
        }
    }
}
