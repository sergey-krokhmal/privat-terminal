using PrivatTerminalApi.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;

namespace PrivatTerminalApi.Controllers
{
    public class MainController : ApiController
    {
		// GET api/values
		public HttpResponseMessage Post(HttpRequestMessage request)
		{
			XDocument xResDoc;
			PrivatPayment pp = new PrivatPayment("http://debt.privatbank.ua/Transfer", "http://www.w3.org/2001/XMLSchema-instance");
			pp.LoadRequest(request.Content.ReadAsStreamAsync().Result);
			try
			{
				switch (pp.RequestAction)
				{
					case "Presearch":
						xResDoc = pp.Presearch();
						break;
					case "Search":
						xResDoc = pp.Search();
						break;
					case "Check":
						xResDoc = pp.Check();
						break;
					case "Pay":
						xResDoc = pp.Pay();
						break;
					case "Cancel":
						xResDoc = pp.Cancel();
						break;
					default:
						xResDoc = pp.BuildCustomError(pp.RequestAction, "Неизвестное действие " + pp.RequestAction);
						break;
				}
			}
			catch (Exception ex)
			{
				return new HttpResponseMessage
				{
					Content = new StringContent((pp.BuildCustomError(pp.RequestAction, ex.Message)).ToString(), Encoding.UTF8, "application/xml")
				};
			}
			return new HttpResponseMessage
			{
				Content = new StringContent((xResDoc).ToString(), Encoding.UTF8, "application/xml")
			};
		}
    }
}
