using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using SList = System.Collections.Generic.IList<System.Text.StringBuilder>;

namespace PrivatTerminalApi.Controllers
{
    public class ValuesController : ApiController
    {
        private SList strings;

        public ValuesController()
        {
            strings = new List<StringBuilder>();
            strings.Add(new StringBuilder("value1"));
            strings.Add(new StringBuilder("value2"));
        }
        // GET api/values
        public IEnumerable<string> Get()
        {
            return strings.Select(s => s.ToString()).ToArray<string>();
        }

        // GET api/values/5
        public string Get(int id)
        {
            return strings.ElementAt(id).ToString();
        }

        // POST api/values
        public IEnumerable<string> Post([FromBody]string value)
        {
            strings.Add(new StringBuilder(value));
            return strings.Select(s => s.ToString()).ToArray<string>();
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
            strings[id] = new StringBuilder(value);
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
            strings.RemoveAt(id);
        }
    }
}
