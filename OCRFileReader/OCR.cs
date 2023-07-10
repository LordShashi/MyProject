using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Google.Cloud.Vision.V1;

namespace OCRFileReader
{
    public class OCR
    {
        public async Task<OCRFileModel> OCRMulkiyaFrontDetails(string path, OCRFileModel obj)
        {
            await Task.Run(() =>
            {
                try
                {
                    string Epath = ConfigurationManager.AppSettings["OCR_Environment"];
                    string MP = AppDomain.CurrentDomain.BaseDirectory;
                    MP = MP + Epath;
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", MP);
                    Console.WriteLine(Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS")); 
                    var client = ImageAnnotatorClient.Create();
                    var image = Google.Cloud.Vision.V1.Image.FromFile(path);
                    ImageContext c = new ImageContext();
                    c.LanguageHints.Add("en");
                    var response = client.DetectDocumentText(image, c);
                    Console.WriteLine("response data is " + response.Text); Console.ReadLine();

                    string[] stringSeparators = new string[] { "\n" };
                    List<string> Datalist = response.Text.Split(stringSeparators, StringSplitOptions.None).ToList();
                    var PlateNumber = Datalist.Where(x => x.ToLower().Contains("plate") || x.ToLower().Contains("plate")).Select(x => x).FirstOrDefault();
                    Regex regexnumbers = new Regex("[0-9]");
                    if (!string.IsNullOrEmpty(PlateNumber))
                    {
                        foreach (Match m in regexnumbers.Matches(PlateNumber))
                        {
                            if (string.IsNullOrEmpty(obj._MPlateno))
                            {
                                int Startdigit = Convert.ToInt32(m.Value);
                                int indexofStr = PlateNumber.IndexOf(m.Value);
                                obj._MPlateno = PlateNumber.Substring(indexofStr - 1, 6);
                                break;
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(obj._MPlateno))
                    {
                        Regex reg = new Regex(@"\b([1-8][0-9]{4}|9[0-8][0-9]{3}|99[0-8][0-9]{2}|999[0-8][0-9]|9999[0-9])\b");
                        foreach (var number in Datalist)
                        {
                            foreach (Match m in reg.Matches(number))
                            {
                                obj._MPlateno = m.Value;
                                break;
                            }
                        }
                        //if (value <= 99999)
                        //{
                        //    return sign + 5;
                        //}
                    }
                    var regex = new Regex(@"\b\d{2}\/\d{2}/\d{4}\b");
                    var regex1 = new Regex(@"\b\d{2}\-\d{2}-\d{4}\b");
                    var regex2 = new Regex(@"\b\d{2}\.\d{2}.\d{4}\b");
                    var regex3 = new Regex(@"\b\d{4}\/\d{2}/\d{2}\b");
                    var regex4 = new Regex(@"\b\d{4}\-\d{2}-\d{2}\b");
                    var regex5 = new Regex(@"\b\d{4}\.\d{2}.\d{2}\b");
                    var regex6 = new Regex(@"\b\d{2}\-\d{2}-\d{2}\b");
                    var insExpData = Datalist.Where(x => x.ToLower().Contains("ins. exp") || (x.ToLower().Contains("ins.exp") || x.ToLower().Contains("ins exp") || x.ToLower().Contains("insexp"))).Select(x => x).FirstOrDefault();

                    foreach (Match m in regex.Matches(insExpData))
                    {
                        obj._MInsexp = m.Value;
                    }
                    if (string.IsNullOrEmpty(obj._MInsexp))
                    {
                        foreach (Match m in regex1.Matches(insExpData))
                        {
                            obj._MInsexp = m.Value;
                            obj._MInsexp = obj._MInsexp.Replace("-", "/");
                        }
                    }
                    if (string.IsNullOrEmpty(obj._MInsexp))
                    {
                        foreach (Match m in regex2.Matches(insExpData))
                        {
                            obj._MInsexp = m.Value;
                            obj._MInsexp = obj._MInsexp.Replace(".", "/");
                        }
                    }
                    if (string.IsNullOrEmpty(obj._MInsexp))
                    {
                        foreach (Match m in regex3.Matches(insExpData))
                        {
                            obj._MInsexp = Convert.ToDateTime(m.Value).ToString("dd/MM/yyyy");
                            obj._MInsexp = obj._MInsexp.Replace("-", "/");
                        }
                    }
                    if (string.IsNullOrEmpty(obj._MInsexp))
                    {
                        foreach (Match m in regex4.Matches(insExpData))
                        {
                            obj._MInsexp = Convert.ToDateTime(m.Value).ToString("dd/MM/yyyy");
                            obj._MInsexp = obj._MInsexp.Replace("-", "/");
                            //m.Value.ToString("dd/MM/yyyy");
                        }
                    }
                    if (string.IsNullOrEmpty(obj._MInsexp))
                    {
                        foreach (Match m in regex5.Matches(insExpData))
                        {
                            obj._MInsexp = Convert.ToDateTime(m.Value).ToString("dd/MM/yyyy");
                            obj._MInsexp = obj._MInsexp.Replace("-", "/");
                        }
                    }
                    var ExpiryData = Datalist.Where(x => (x.ToLower().Contains("exp. date") && x.ToLower().Contains("reg")) || (x.ToLower().Contains("exp.date") && x.ToLower().Contains("reg")) || (x.ToLower().Contains("exp date") && x.ToLower().Contains("reg")) || (x.ToLower().Contains("expdate") && x.ToLower().Contains("reg"))).Select(x => x).FirstOrDefault();
                    //if(ExpiryData==null)
                    //{
                    //    ExpiryData = Datalist.Where(x => (x.ToLower().Contains("exp. date"))).Select(x=>x).FirstOrDefault();
                    //}
                    if (!string.IsNullOrEmpty(ExpiryData))
                    {
                        string[] dataLst = ExpiryData.ToLower().Split(new[] { "reg" }, StringSplitOptions.None);
                        foreach (Match m in regex.Matches(dataLst[0]))
                        {
                            obj._MExpdate = m.Value;
                        }
                        foreach (Match m in regex.Matches(dataLst[1]))
                        {
                            obj._MRegdate = m.Value;
                        }
                        if (string.IsNullOrEmpty(obj._MExpdate))
                        {
                            foreach (Match m in regex1.Matches(dataLst[0]))
                            {
                                obj._MExpdate = m.Value;
                                obj._MExpdate = obj._MExpdate.Replace("-", "/");
                            }
                        }
                        if (string.IsNullOrEmpty(obj._MExpdate))
                        {
                            foreach (Match m in regex2.Matches(dataLst[0]))
                            {
                                obj._MExpdate = m.Value;
                                obj._MExpdate = obj._MExpdate.Replace(".", "/");
                            }
                        }
                        if (string.IsNullOrEmpty(obj._MExpdate))
                        {
                            foreach (Match m in regex3.Matches(dataLst[0]))
                            {
                                obj._MExpdate = Convert.ToDateTime(m.Value).ToString("dd/MM/yyyy");
                                obj._MExpdate = obj._MExpdate.Replace("-", "/");
                            }
                        }
                        if (string.IsNullOrEmpty(obj._MExpdate))
                        {
                            foreach (Match m in regex4.Matches(dataLst[0]))
                            {
                                obj._MExpdate = Convert.ToDateTime(m.Value).ToString("dd/MM/yyyy");
                                obj._MExpdate = obj._MExpdate.Replace("-", "/");
                            }
                        }
                        if (string.IsNullOrEmpty(obj._MExpdate))
                        {
                            foreach (Match m in regex5.Matches(dataLst[0]))
                            {
                                obj._MExpdate = Convert.ToDateTime(m.Value).ToString("dd/MM/yyyy");
                                obj._MExpdate = obj._MExpdate.Replace("-", "/");
                            }
                        }
                        if (string.IsNullOrEmpty(obj._MRegdate))
                        {
                            foreach (Match m in regex1.Matches(dataLst[1]))
                            {
                                obj._MRegdate = m.Value;
                                obj._MRegdate = obj._MRegdate.Replace("-", "/");
                            }
                        }
                        if (string.IsNullOrEmpty(obj._MRegdate))
                        {
                            foreach (Match m in regex2.Matches(dataLst[1]))
                            {
                                obj._MRegdate = m.Value;
                                obj._MRegdate = obj._MRegdate.Replace(".", "/");
                            }
                        }
                        if (string.IsNullOrEmpty(obj._MRegdate))
                        {
                            foreach (Match m in regex3.Matches(dataLst[1]))
                            {
                                obj._MRegdate = Convert.ToDateTime(m.Value).ToString("dd/MM/yyyy");
                                obj._MRegdate = obj._MRegdate.Replace("-", "/");
                            }
                        }
                        if (string.IsNullOrEmpty(obj._MRegdate))
                        {
                            foreach (Match m in regex4.Matches(dataLst[1]))
                            {
                                obj._MRegdate = Convert.ToDateTime(m.Value).ToString("dd/MM/yyyy");
                                obj._MRegdate = obj._MRegdate.Replace("-", "/");
                            }
                        }
                        if (string.IsNullOrEmpty(obj._MRegdate))
                        {
                            foreach (Match m in regex5.Matches(dataLst[1]))
                            {
                                obj._MRegdate = Convert.ToDateTime(m.Value).ToString("dd/MM/yyyy");
                                obj._MRegdate = obj._MRegdate.Replace("-", "/");
                            }
                        }
                    }
                    else
                    {
                        var ExpiryDataUpdated = Datalist.Where(x => (x.ToLower().Contains("exp. date") && !x.ToLower().Contains("reg")) || (x.ToLower().Contains("exp.date") && !x.ToLower().Contains("reg")) || (x.ToLower().Contains("exp date") && !x.ToLower().Contains("reg")) || (x.ToLower().Contains("expdate") && !x.ToLower().Contains("reg"))).Select(x => x).FirstOrDefault();
                        if (!string.IsNullOrEmpty(ExpiryDataUpdated))
                        {
                            foreach (Match m in regex.Matches(ExpiryDataUpdated))
                            {
                                obj._MExpdate = m.Value;
                            }
                            if (string.IsNullOrEmpty(obj._MExpdate))
                            {
                                foreach (Match m in regex1.Matches(ExpiryDataUpdated))
                                {
                                    obj._MExpdate = m.Value;
                                    obj._MExpdate = obj._MExpdate.Replace("-", "/");
                                }
                            }
                            if (string.IsNullOrEmpty(obj._MExpdate))
                            {
                                foreach (Match m in regex2.Matches(ExpiryDataUpdated))
                                {
                                    obj._MExpdate = m.Value;
                                    obj._MExpdate = obj._MExpdate.Replace(".", "/");
                                }
                            }
                            if (string.IsNullOrEmpty(obj._MExpdate))
                            {
                                foreach (Match m in regex3.Matches(ExpiryDataUpdated))
                                {
                                    obj._MExpdate = Convert.ToDateTime(m.Value).ToString("dd/MM/yyyy");
                                    obj._MExpdate = obj._MExpdate.Replace("-", "/");
                                }
                            }
                            if (string.IsNullOrEmpty(obj._MExpdate))
                            {
                                foreach (Match m in regex4.Matches(ExpiryDataUpdated))
                                {
                                    obj._MExpdate = Convert.ToDateTime(m.Value).ToString("dd/MM/yyyy");
                                    obj._MExpdate = obj._MExpdate.Replace("-", "/");
                                }
                            }
                            if (string.IsNullOrEmpty(obj._MExpdate))
                            {
                                foreach (Match m in regex5.Matches(ExpiryDataUpdated))
                                {
                                    obj._MExpdate = Convert.ToDateTime(m.Value).ToString("dd/MM/yyyy");
                                    obj._MExpdate = obj._MExpdate.Replace("-", "/");
                                }
                            }

                        }

                        var RegDataUpdated = Datalist.Where(x => (!x.ToLower().Contains("exp. date") && x.ToLower().Contains("reg")) || (!x.ToLower().Contains("exp.date") && x.ToLower().Contains("reg")) || (!x.ToLower().Contains("exp date") && x.ToLower().Contains("reg")) || (!x.ToLower().Contains("expdate") && x.ToLower().Contains("reg"))).Select(x => x).FirstOrDefault();
                        if (!string.IsNullOrEmpty(RegDataUpdated))
                        {
                            foreach (Match m in regex.Matches(RegDataUpdated))
                            {
                                obj._MRegdate = m.Value;
                            }
                            if (string.IsNullOrEmpty(obj._MRegdate))
                            {
                                foreach (Match m in regex1.Matches(RegDataUpdated))
                                {
                                    obj._MRegdate = m.Value;
                                    obj._MRegdate = obj._MRegdate.Replace("-", "/");
                                }
                            }
                            if (string.IsNullOrEmpty(obj._MRegdate))
                            {
                                foreach (Match m in regex2.Matches(RegDataUpdated))
                                {
                                    obj._MRegdate = m.Value;
                                    obj._MRegdate = obj._MRegdate.Replace(".", "/");
                                }
                            }
                            if (string.IsNullOrEmpty(obj._MRegdate))
                            {
                                foreach (Match m in regex3.Matches(RegDataUpdated))
                                {
                                    obj._MRegdate = Convert.ToDateTime(m.Value).ToString("dd/MM/yyyy");
                                    obj._MRegdate = obj._MRegdate.Replace("-", "/");
                                }
                            }
                            if (string.IsNullOrEmpty(obj._MRegdate))
                            {
                                foreach (Match m in regex4.Matches(RegDataUpdated))
                                {
                                    obj._MRegdate = Convert.ToDateTime(m.Value).ToString("dd/MM/yyyy");
                                    obj._MRegdate = obj._MRegdate.Replace("-", "/");
                                }
                            }
                            if (string.IsNullOrEmpty(obj._MRegdate))
                            {
                                foreach (Match m in regex5.Matches(RegDataUpdated))
                                {
                                    obj._MRegdate = Convert.ToDateTime(m.Value).ToString("dd/MM/yyyy");
                                    obj._MRegdate = obj._MRegdate.Replace("-", "/");
                                }
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(obj._MRegdate))
                    {
                        var RegistrationData = Datalist.Where(x => (x.ToLower().Contains("reg"))).Select(x => x).FirstOrDefault();
                        int indexofRedDate = Datalist.IndexOf(RegistrationData);
                        var expData = Datalist[indexofRedDate + 1];
                        List<string> month = new List<string>();
                        month.Add("jan");
                        month.Add("feb");
                        month.Add("mar");
                        month.Add("apr");
                        month.Add("may");
                        month.Add("jun");
                        month.Add("jul");
                        month.Add("aug");
                        month.Add("sep");
                        month.Add("oct");
                        month.Add("nov");
                        month.Add("dec");
                        int j = 0;
                        foreach (var expMonth in month)
                        {
                            j++;
                            string getMonth = Convert.ToString(expData.ToLower().Contains(expMonth));
                            string replaceValue = "";
                            if (getMonth.ToLower() == "true")
                            {
                                if (j < 10)
                                {
                                    replaceValue = "0" + j.ToString();
                                }
                                else
                                {
                                    replaceValue = j.ToString();
                                }
                                expData = expData.ToLower().Replace(expMonth.ToLower(), replaceValue);
                                if (string.IsNullOrEmpty(obj._MInsexp))
                                {
                                    foreach (Match m in regex6.Matches(expData))
                                    {
                                        obj._MInsexp = Convert.ToDateTime(m.Value).ToString("dd/MM/yyyy");
                                        obj._MInsexp = obj._MInsexp.Replace("-", "/");
                                        break;
                                    }
                                }
                            }
                        }
                        string[] dataLst = RegistrationData.Split(new[] { "Reg" }, StringSplitOptions.None);
                        int i = 0;
                        string monthValue = "";
                        string expMonthName = "";
                        foreach (var mon in month)
                        {
                            string getMonth = Convert.ToString(dataLst[1].ToLower().Contains(mon));
                            string getExpMonth = Convert.ToString(dataLst[0].ToLower().Contains(mon));
                            string monthName = mon;
                            i++;
                            if (getExpMonth.ToLower() == "true")
                            {
                                if (i < 10)
                                {
                                    expMonthName = "0" + i.ToString();
                                }
                                else
                                {
                                    expMonthName = i.ToString();
                                }
                                dataLst[0] = dataLst[0].ToLower().Replace(mon.ToLower(), expMonthName);


                                if (string.IsNullOrEmpty(obj._MExpdate))
                                {
                                    foreach (Match m in regex6.Matches(dataLst[0]))
                                    {
                                        obj._MExpdate = Convert.ToDateTime(m.Value).ToString("dd/MM/yyyy");
                                        obj._MExpdate = obj._MExpdate.Replace("-", "/");
                                        break;
                                    }
                                }
                            }


                            if (getMonth.ToLower() == "true")
                            {
                                if (i < 10)
                                {
                                    monthValue = "0" + i.ToString();
                                }
                                else
                                {
                                    monthValue = i.ToString();
                                }

                                dataLst[1] = dataLst[1].ToLower().Replace(mon.ToLower(), monthValue);


                                if (string.IsNullOrEmpty(obj._MRegdate))
                                {
                                    foreach (Match m in regex6.Matches(dataLst[1]))
                                    {
                                        obj._MRegdate = Convert.ToDateTime(m.Value).ToString("dd/MM/yyyy");
                                        obj._MRegdate = obj._MRegdate.Replace("-", "/");
                                        break;
                                    }
                                }

                            }

                        }
                    }
                    if (string.IsNullOrEmpty(obj._MName))
                    {
                        string MulName = Datalist.Where(x => (x.ToUpper().Contains("OWNER"))).Select(x => x).FirstOrDefault();
                        if (!string.IsNullOrEmpty(MulName))
                        {
                            obj._MName = MulName.ToUpper().Replace("OWNER", "").ToString();
                        }
                        if (string.IsNullOrEmpty(obj._MName))
                        {
                            string Name = Datalist.Where(x => (x.ToUpper().StartsWith("NAM"))).Select(x => x).FirstOrDefault();
                            obj._MName = Name.ToUpper().Replace("NAM", "").ToString();

                        }
                    }
                   
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString(), "Error While Reading text from image");

                }
            });
            return obj;
        }
    }
}

