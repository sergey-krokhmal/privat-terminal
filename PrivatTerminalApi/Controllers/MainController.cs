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

		private XNamespace xmlns
		{
			get
			{
				return XNamespace.Get("http://debt.privatbank.ua/Transfer");
			}
		}
		private XNamespace xsi
		{
			get
			{
				return XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
			}
		}

		// GET api/values
		public HttpResponseMessage Post(HttpRequestMessage request)
		{
			XDocument xDoc = XDocument.Load(request.Content.ReadAsStreamAsync().Result);
			XElement xRoot = xDoc.Root;

			XAttribute xInterface = xRoot.Attribute("interface");
			XAttribute xAction = xRoot.Attribute("action");
			IEnumerable<XElement> xUnit = new List<XElement>();
			XDocument xResDoc;
			
			switch (xAction.Value)
			{
				case "Presearch":
					xUnit = xRoot
						.Element(xmlns + "Data")
						.Descendants(xmlns + "Unit")
						.Where(x => (string)(x.Attribute("name").Value) == "ls");
					if (xUnit.Count() == 1)
					{
						TerminalPays db = new TerminalPays();
						string ls;
						try
						{
							ls = xUnit.First().Attribute("value").Value;
						}
						catch
						{
							return new HttpResponseMessage
							{
								Content = new StringContent((BuildCustomError(xAction.Value, "В элементе Unit не найден аттрибут value")).ToString(), Encoding.UTF8, "application/xml")
							};
						}
						IEnumerable<Advertiser> payers = db.Advertisers.Where(p => p.uniqueNumber == ls);
						if (payers.Count() > 0)
						{
							XDocument xDocPayers = BuildBaseResponse(xInterface.Value, xAction.Value);
							XElement xPayersData = new XElement("Data",
								new XAttribute(XNamespace.Xmlns + "xsi", xsi),
								new XAttribute(xsi + "type", "PayersTable"),
								new XElement("Headers",
									new XElement("Header", new XAttribute("name", "fio")),
									new XElement("Header", new XAttribute("name", "ls"))
								)
							);
							XElement xPayersColumns = new XElement("Columns");
							XElement xPayersColumnFio = new XElement("Column");
							XElement xPayersColumnLs = new XElement("Column");
							foreach (Advertiser payer in payers)
							{
								xPayersColumnFio.Add(new XElement("Element", (payer.Sname + " " + payer.Name).Trim()));
								xPayersColumnLs.Add(new XElement("Element", payer.uniqueNumber));
							}
							xPayersColumns.Add(xPayersColumnFio);
							xPayersColumns.Add(xPayersColumnLs);
							xPayersData.Add(xPayersColumns);
							xDocPayers.Root.Add(xPayersData);
							xResDoc = xDocPayers;
						}
						else
						{
							xResDoc = BuildCustomError(xAction.Value, "Платильщиков с лицевым счетом " + ls + " не найдено");
						}
						
					}
					else
					{
						xResDoc = BuildCustomError(xAction.Value, "Елемент Unit c аттрибутом name = \"ls\" не найден");
					}
					break;
				case "Search":
					xUnit = xRoot
						.Element(xmlns + "Data")
						.Descendants(xmlns + "Unit")
						.Where(x => (string)(x.Attribute("name").Value) == "bill_identifier");
					if (xUnit.Count() == 1)
					{
						TerminalPays db = new TerminalPays();
						string ls;
						try
						{
							ls = xUnit.First().Attribute("value").Value;
						}
						catch
						{
							return new HttpResponseMessage
							{
								Content = new StringContent((BuildCustomError(xAction.Value, "В элементе Unit не найден аттрибут value")).ToString(), Encoding.UTF8, "application/xml")
							};
						}
						IEnumerable<Advertiser> payers = db.Advertisers.Where(p => p.uniqueNumber == ls);
						int payers_count = payers.Count();
						if (payers_count < 1)
						{
							xResDoc = BuildCustomError(xAction.Value, "Платильщиков с лицевым счетом " + ls + " не найдено");
						}
						else if (payers_count > 1)
						{
							xResDoc = BuildCustomError(xAction.Value, "Платильщиков с лицевым счетом " + ls + " несколько");
						}
						else
						{
							DateTime datetime = DateTime.Now;
							Advertiser payer = payers.First();
							XDocument xDocDebt = BuildBaseResponse(xInterface.Value, xAction.Value);
							XElement xDebtData = new XElement("Data",
								new XAttribute(XNamespace.Xmlns + "xsi", xsi),
								new XAttribute(xsi + "type", "DebtPack"),
								//new XAttribute("billPeriod", "DebtPack"),
								//new XAttribute("actualBills", "DebtPack"),
								new XElement("PayerInfo",
									new XAttribute("billIdentifier", ls),
									new XElement("Fio", (payer.Sname + " " + payer.Name).Trim()),
									new XElement("Phone", payer.Phone)
								),
								new XElement("ServiceGroup",
									new XElement("DebtService",
										new XAttribute("serviceCode", "101"),
										new XElement("CompanyInfo",
											new XAttribute("mfo", "123654"),
											new XAttribute("okpo", "23900123654"),
											new XAttribute("account", "66778844"),
											new XElement("CompanyCode", "1"),
											new XElement("CompanyName", "ООО Издательский дом \"Премьер\"")
										),
										new XElement("DebtInfo",
											new XAttribute("amountToPay", "10"),
											new XElement("Year", datetime.Year),
											new XElement("Month", datetime.Month)
										),
										new XElement("ServiceName", "Размещение объявлений на сайте или в газете \"Премьер\""),
										new XElement("Destination", "Облата услуги \"Размещение объявлений на сайте или в газете \"Премьер\""),
										new XElement("PayerInfo",
											new XAttribute("billIdentifier", ls),
											new XElement("Fio", (payer.Sname + " " + payer.Name).Trim()),
											new XElement("Phone", payer.Phone)
										)
									)
								)
							);
							xDocDebt.Root.Add(xDebtData);
							xResDoc = xDocDebt;
						}
						
					}
					else
					{
						xResDoc = BuildCustomError(xAction.Value, "Елемент Unit c аттрибутом name = \"ls\" не найден");
					}
					break;
				default:
					xResDoc = BuildCustomError(xAction.Value, "Неизвестное действие " + xAction.Value);
					break;
			}
			return new HttpResponseMessage
			{
				Content = new StringContent((xResDoc).ToString(), Encoding.UTF8, "application/xml")
			};
			//return string;
		}

		public XDocument BuildBaseResponse(string iface, string action)
		{
			XDocument xDoc = new XDocument(
				new XDeclaration("1.0", "UTF-8", "yes")
			);
			XElement xRoot = new XElement(xmlns + "Transfer",
				new XAttribute("interface", iface),
				new XAttribute("action", action)
			);
			xDoc.Add(xRoot);
			return xDoc;
		}

		public XDocument BuildCustomError(string action, string message = "")
		{
			return this.BuildError(action, 99, message);
		}

		public XDocument BuildError(string action, int code, string message = "")
		{
			XDocument xDoc = BuildBaseResponse("Debt", action);
			XAttribute codeAttr = new XAttribute("code", code.ToString());
			XElement messageElem = new XElement("Message", message);
			XElement dataElem = new XElement("Data",
				new XAttribute(XNamespace.Xmlns + "xsi", xsi),
				new XAttribute(xsi + "type", "ErrorInfo")
			);
			dataElem.Add(messageElem);
			dataElem.Add(codeAttr);
			xDoc.Root.Add(dataElem);

			return xDoc;
		}
    }
}
