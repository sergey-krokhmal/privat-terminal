using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Net.Http;
using System;

namespace PrivatTerminalApi.Models
{
	public class PrivatPayment
	{
		protected XDocument xDoc;
		protected XElement xRoot;
		protected XAttribute xInterface;
		protected XAttribute xAction;

		protected XNamespace xmlns, xsi;

		public string RequestAction
		{
			get
			{
				try
				{
					return xAction.Value;
				}
				catch
				{
					return string.Empty;
				}
			}
		}

		public PrivatPayment(XNamespace xmlns, XNamespace xsi)
		{
			this.xmlns = xmlns;
			this.xsi = xsi;
		}

		public void LoadRequest(Stream requestStream)
		{
			xDoc = XDocument.Load(requestStream);
			xRoot = xDoc.Root;
			xInterface = xRoot.Attribute("interface");
			xAction = xRoot.Attribute("action");
		}

		public XDocument Presearch()
		{
			XDocument xResDoc;
			IEnumerable<XElement> xElems = new List<XElement>();
			xElems = xRoot
				.Element(xmlns + "Data")
				.Elements(xmlns + "Unit")
				.Where(x => (string)(x.Attribute("name").Value) == "ls");
			if (xElems.Count() == 1)
			{
				TerminalPays db = new TerminalPays();
				string ls;
				try
				{
					ls = xElems.First().Attribute("value").Value;
				}
				catch
				{
					return BuildCustomError(xAction.Value, "В элементе Unit не найден аттрибут value");
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
			return xResDoc;
		}

		public XDocument Search()
		{
			XDocument xResDoc;
			IEnumerable<XElement> xElems = new List<XElement>();
			xElems = xRoot
						.Element(xmlns + "Data")
						.Elements(xmlns + "Unit")
						.Where(x => (string)(x.Attribute("name").Value) == "bill_identifier");
			if (xElems.Count() == 1)
			{
				TerminalPays db = new TerminalPays();
				string ls;
				try
				{
					ls = xElems.First().Attribute("value").Value;
				}
				catch
				{
					return BuildCustomError(xAction.Value, "В элементе Unit не найден аттрибут value");
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
			return xResDoc;
		}

		public XDocument Check()
		{
			XDocument xResDoc;
			IEnumerable<XElement> xElems = new List<XElement>();
			xElems = xRoot.Elements(xmlns + "Data");
			if (xElems.Count() == 1)
			{
				XElement xElem = xElems.First();
				TerminalPays db = new TerminalPays();
				//string paymentNumber;
				int paymentId;
				try
				{
					paymentId = int.Parse(xElem.Attribute("id").Value);
				}
				catch
				{
					return BuildCustomError(xAction.Value, "В элементе Data не найден аттрибут id");
				}
				string ls;
				try
				{
					ls = xElem.Element(xmlns + "PayerInfo").Attribute("ls").Value;
				}
				catch
				{
					return BuildCustomError(xAction.Value, "В элементе PayerInfo не найден аттрибут ls");
				}
				IEnumerable<Advertiser> payers = db.Advertisers.Where(p => p.uniqueNumber == ls);
				int payers_count = payers.Count();
				if (payers_count < 1)
				{
					xResDoc = BuildCustomError(xAction.Value, "Платильщиков с лицевым счетом " + ls + " не найдено");
				}
				else
				{
					Advertiser payer = payers.First();
					IEnumerable<Pay> pays = db.Pays.Where(p => p.PaymentId == paymentId);
					int pays_count = pays.Count();
					if (pays_count >= 1)
					{
						xResDoc = BuildCustomError(xAction.Value, "Платеж с  id " + paymentId + " уже существует");
					}
					else
					{
						string createTime = xElem.Element(xmlns + "CreateTime").Value;
						DateTime dateCreate = DateTime.Parse(createTime);
						int payerId = payers.First().Id;
						decimal sum = decimal.Parse(xElem.Element(xmlns + "TotalSum").Value);
						Pay pay = new Pay();
						pay.AdvertiserId = payer.Id;
						pay.Comission = 0;
						pay.DateCreate = dateCreate;
						pay.PaymentId = paymentId;
						pay.Sum = sum;
						pay.uniqueNumber = payer.uniqueNumber;
						pay.StatusId = 1;//db.PayStatus.Where(s => s.Name == "waiting").First().Id;
						db.Pays.Add(pay);
						db.SaveChanges();
						XDocument xDocPay = BuildBaseResponse(xInterface.Value, xAction.Value);
						xDocPay.Root.Add(
							new XElement("Data",
								new XAttribute(XNamespace.Xmlns + "xsi", xsi),
								new XAttribute(xsi + "type", "Gateway"),
								new XAttribute("reference", pay.Id)
							)
						);
						xResDoc = xDocPay;
					}
				}
			}
			else
			{
				xResDoc = BuildCustomError(xAction.Value, "Елемент Unit c аттрибутом name = \"ls\" не найден");
			}
			return xResDoc;
		}

		public XDocument Pay()
		{
			XDocument xResDoc;
			IEnumerable<XElement> xElems = new List<XElement>();
			xElems = xRoot.Elements(xmlns + "Data");
			if (xElems.Count() == 1)
			{
				XElement xElem = xElems.First();
				TerminalPays db = new TerminalPays();
				//string paymentNumber;
				int paymentId;
				try
				{
					paymentId = int.Parse(xElem.Attribute("id").Value);
					//paymentNumber = xElem.Attribute("number").Value;
				}
				catch
				{
					return BuildCustomError(xAction.Value, "В элементе Data не найден аттрибут id");
				}
				string ls;
				try
				{
					ls = xElem.Element(xmlns + "PayerInfo").Attribute("ls").Value;
				}
				catch
				{
					return BuildCustomError(xAction.Value, "В элементе PayerInfo не найден аттрибут ls");
				}
				IEnumerable<Advertiser> payers = db.Advertisers.Where(p => p.uniqueNumber == ls);
				int payers_count = payers.Count();
				if (payers_count < 1)
				{
					xResDoc = BuildCustomError(xAction.Value, "Платильщиков с лицевым счетом " + ls + " не найдено");
				}
				else
				{
					Advertiser payer = payers.First();
					decimal totalSum = decimal.Parse(xElem.Element(xmlns + "TotalSum").Value);
					string reference = xElem.Element(xmlns + "CompanyInfo").Element(xmlns + "CheckReference").Value;
					IEnumerable<Pay> pays = db.Pays.Where(
						p => p.Id.ToString() == reference &&
						p.PaymentId == paymentId &&
						p.uniqueNumber == payer.uniqueNumber &&
						p.Sum == totalSum);
					int pays_count = pays.Count();
					if (pays_count != 1)
					{
						xResDoc = BuildCustomError(xAction.Value, "Платеж с  id " + paymentId + " не соответствует данным");
					}
					else
					{
						Pay pay = pays.First();
						string confirmTime = xElem.Element(xmlns + "ConfirmTime").Value;
						DateTime dateConfirm = DateTime.Parse(confirmTime);
						pay.DateConfirm = dateConfirm;
						pay.StatusId = 2;
						db.SaveChanges();
						XDocument xDocPay = BuildBaseResponse(xInterface.Value, xAction.Value);
						xDocPay.Root.Add(
							new XElement("Data",
								new XAttribute(XNamespace.Xmlns + "xsi", xsi),
								new XAttribute(xsi + "type", "Gateway"),
								new XAttribute("reference", pay.Id)
							)
						);
						xResDoc = xDocPay;
					}
				}
			}
			else
			{
				xResDoc = BuildCustomError(xAction.Value, "Елемент Unit c аттрибутом name = \"ls\" не найден");
			}
			return xResDoc;
		}

		public XDocument Cancel()
		{
			XDocument xResDoc;
			IEnumerable<XElement> xElems = new List<XElement>();
			xElems = xRoot.Elements(xmlns + "Data");
			if (xElems.Count() == 1)
			{
				XElement xElem = xElems.First();
				TerminalPays db = new TerminalPays();
				//string paymentNumber;
				int paymentId;
				try
				{
					paymentId = int.Parse(xElem.Attribute("id").Value);
					//paymentNumber = xElem.Attribute("number").Value;
				}
				catch
				{
					return BuildCustomError(xAction.Value, "В элементе Data не найден аттрибут id");
				}
				string ls;
				try
				{
					ls = xElem.Element(xmlns + "PayerInfo").Attribute("ls").Value;
				}
				catch
				{
					return BuildCustomError(xAction.Value, "В элементе PayerInfo не найден аттрибут ls");
				}
				IEnumerable<Advertiser> payers = db.Advertisers.Where(p => p.uniqueNumber == ls);
				int payers_count = payers.Count();
				if (payers_count < 1)
				{
					xResDoc = BuildCustomError(xAction.Value, "Платильщиков с лицевым счетом " + ls + " не найдено");
				}
				else
				{
					Advertiser payer = payers.First();
					decimal totalSum = decimal.Parse(xElem.Element(xmlns + "TotalSum").Value);
					string reference = xElem.Element(xmlns + "ServiceGroup")
						.Element(xmlns + "Service").Element(xmlns + "CompanyInfo").Element(xmlns + "CheckReference").Value;
					IEnumerable<Pay> pays = db.Pays.Where(
						p => p.Id.ToString() == reference &&
						p.PaymentId == paymentId &&
						p.uniqueNumber == payer.uniqueNumber &&
						p.Sum == totalSum);
					int pays_count = pays.Count();
					if (pays_count != 1)
					{
						xResDoc = BuildCustomError(xAction.Value, "Платеж с  id " + paymentId + " не соответствует данным");
					}
					else
					{
						Pay pay = pays.First();
						pay.DateConfirm = null;
						pay.StatusId = 1;
						db.SaveChanges();
						XDocument xDocPay = BuildBaseResponse(xInterface.Value, xAction.Value);
						xDocPay.Root.Add(
							new XElement("Data",
								new XAttribute(XNamespace.Xmlns + "xsi", xsi),
								new XAttribute(xsi + "type", "Gateway"),
								new XAttribute("reference", pay.Id)
							)
						);
						xResDoc = xDocPay;
					}
				}
			}
			else
			{
				xResDoc = BuildCustomError(xAction.Value, "Елемент Unit c аттрибутом name = \"ls\" не найден");
			}
			return xResDoc;
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