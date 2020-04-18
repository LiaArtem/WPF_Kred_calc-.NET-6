﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using static System.Math;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Diagnostics;
using Microsoft.Win32;
using System.Data;
using System.Windows.Data;
using System.Windows.Media;

namespace WPF_Kred_calc
{
    class Currency
    {
        public double RATE { get; set; }
        public string CC { get; set; }        
    }

    public class TDataGridCol
    {
        public string TDate { get; set; }
        public string TDolg { get; set; }
        public string TPlatInt { get; set; }
        public string TPlat { get; set; }
        public string TPereplata { get; set; }
        public string TPlatDop { get; set; }
        public string TItogo { get; set; }
        public string TColorType { get; set; }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static int typ;
        public static String[] file_path_ini_mas, type_ini_mas;
        public static Boolean is_program_loading = true;
        public static String tec_kat = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Substring(6);
        public static String tec_kat_ini = tec_kat + "\\" + "ini" + "\\";
        public static String tec_kat_temp = tec_kat + "\\" + "temp";
        public ObservableCollection<string> list = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();

            // подписываем textBox на событие PreviewTextInput, с помощью которого можно обрабатывать вводимый текст
            summa.PreviewTextInput += new TextCompositionEventHandler(TextBox_PreviewTextInput_Float);
            kurs.PreviewTextInput += new TextCompositionEventHandler(TextBox_PreviewTextInput_Float);
            proc_perv_vznos.PreviewTextInput += new TextCompositionEventHandler(TextBox_PreviewTextInput_Float);
            perv_vznos.PreviewTextInput += new TextCompositionEventHandler(TextBox_PreviewTextInput_Float);
            priv_proc_stavka.PreviewTextInput += new TextCompositionEventHandler(TextBox_PreviewTextInput_Float);
            priv_srok_kred.PreviewTextInput += new TextCompositionEventHandler(TextBox_PreviewTextInput_Int);
            proc_stavka.PreviewTextInput += new TextCompositionEventHandler(TextBox_PreviewTextInput_Float);
            srok_kred.PreviewTextInput += new TextCompositionEventHandler(TextBox_PreviewTextInput_Int);
            sum_plat.PreviewTextInput += new TextCompositionEventHandler(TextBox_PreviewTextInput_Float);
            srok_kred_new.PreviewTextInput += new TextCompositionEventHandler(TextBox_PreviewTextInput_Int);
            kurs_start.PreviewTextInput += new TextCompositionEventHandler(TextBox_PreviewTextInput_Float);
            kurs_year_0.PreviewTextInput += new TextCompositionEventHandler(TextBox_PreviewTextInput_Float);
            kurs_year_1.PreviewTextInput += new TextCompositionEventHandler(TextBox_PreviewTextInput_Float);
            kurs_year_2.PreviewTextInput += new TextCompositionEventHandler(TextBox_PreviewTextInput_Float);
            kurs_year_3.PreviewTextInput += new TextCompositionEventHandler(TextBox_PreviewTextInput_Float);
            kurs_year_4.PreviewTextInput += new TextCompositionEventHandler(TextBox_PreviewTextInput_Float);
            koef_otsech.PreviewTextInput += new TextCompositionEventHandler(TextBox_PreviewTextInput_Float);

            is_program_loading = true;

            Poisk_xml_files();
            Read_xml_file_kred_calc();

            is_program_loading = false;

            Dop_plat();

            // заполняем таблицу пустыми строками
            DataGrid1.Items.Clear();
            for (int i = 0; i <= 50; i++)
            {
                DataGrid1.Items.Add(new TDataGridCol { TDate = "", TDolg = "", TPlatInt = "", TPlat = "",
                    TPereplata = "", TPlatDop = "", TItogo = "", TColorType = ""});
            }
        }

        #region Other
        private void TextBox_PreviewTextInput_Float(object sender, TextCompositionEventArgs e)
        {
            string inputSymbol = e.Text.ToString(); // можно вводить цифры и точку
            if (!Regex.Match(inputSymbol, @"[0-9]|\.").Success)
            {
                e.Handled = true;
            }
        }
        private void TextBox_PreviewTextInput_Int(object sender, TextCompositionEventArgs e)
        {
            string inputSymbol = e.Text.ToString(); // можно вводить цифры
            if (!Regex.Match(inputSymbol, @"[0-9]").Success)
            {
                e.Handled = true;
            }
        }

        // Преобразование текста в число
        public double String_to_Double(string TextString)
        {
            string rezult = TextString.Replace(".", ",");
            if (!double.TryParse(rezult, out double rezult_dbl))
            {
                //обработка, если не число
                return 0;
            }
            return rezult_dbl;
        }

        // Преобразование числа в текст
        public String Double_to_String(double TextDouble, int midpoint = 2, string format = "#,0.00")
        {
            return Round(TextDouble, midpoint).ToString(format);
        }

        // вывод диалогового окна
        public static void MessageBoxError(String infoMessage)
        {
            System.Windows.MessageBox.Show(infoMessage, "Сообщение", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
        }

        public Boolean IsDateValid(String m_date)
        {
            if (m_date == "") { return false; }
            try
            {
                DateTime ldate = Convert.ToDateTime(DateTime.ParseExact(m_date, "dd.MM.yyyy", CultureInfo.InvariantCulture));
                return true;
            }
            catch
            {
                return false;
            }
        }

        //Последний день месяца
        public static int LastDayOfMonth(DateTime dteDate)
        {
            return DateTime.DaysInMonth(dteDate.Year, dteDate.Month);
        }

        // Количество дней в году
        public static int KolDayOfYear(DateTime dteDate)
        {
            DateTime d = new DateTime(dteDate.Year, 1, 1);
            DateTime d2 = new DateTime(dteDate.Year + 1, 1, 1);
            return Convert.ToInt32((d2 - d).TotalDays);
        }

        public string Get_date_month(DateTime date_in)
        {            
            string mon = date_in.Month.ToString("00");
            return date_in.Year + "." + mon;
        }

        public int Get_YEAR(DateTime val, int add_year)
        {
            return val.Year + add_year;
        }

        // обрезать количество символов в тексте
        public string Get_String_Length(string p_text, int p_length = 26)
        {
            if (p_text == "") return p_text;
            int m_length = p_length;
            if (p_text.Length <= m_length) m_length = p_text.Length;
            p_text = p_text.Substring(0, m_length);
            return p_text;
        }

        // Убрать пустоты в тексте
        public string Get_String_Not_Space(string p_text)
        {
            string m_text = p_text;
            if (m_text == null) return m_text;
            m_text = String_to_Double(m_text).ToString("#.00");
            return m_text;
        }

        // Расчет и установление курса и расчет эквивалента
        public void Calc_set_kurs_text(string p_curr_code, string p_kurs)
        {
            if (p_curr_code == "UAH")
            {
                this.kurs.IsEnabled = false;
                this.kurs.Text = "1";
            }
            else
            {
                this.kurs.IsEnabled = true;
                if (p_kurs == "")
                {
                    // из сайта 
                    DateTime mDate = this.date_cred.SelectedDate.Value;
                    this.kurs.Text = Double_to_String(GetKursNbu(p_curr_code, mDate), 3, "0.000");
                }
                else
                {
                    this.kurs.Text = p_kurs;
                }
            }

            // Расчет суммы эквивалента
            this.summa_ekv.Text = Double_to_String(String_to_Double(this.summa.Text) * String_to_Double(this.kurs.Text));
        }

        // Первоначальный взнос
        public void Calc_perv_vznos(Boolean p_IsChecked)
        {
            if (p_IsChecked == true)
            {
                this.perv_vznos.IsEnabled = true;
                this.proc_perv_vznos.IsEnabled = false;
            }
            else
            {
                this.perv_vznos.IsEnabled = false;
                this.proc_perv_vznos.IsEnabled = true;
            }
        }

        // Форма погашения
        public void Calc_type_proc(String p_type_proc)
        {
            if (p_type_proc == "R")  // "рассрочка"
            {
                this.GroupBox_rasrochka.IsEnabled = true;
                this.kurs_start.IsEnabled = true; this.kurs_year_0.IsEnabled = true; this.kurs_year_1.IsEnabled = true;
                this.kurs_year_2.IsEnabled = true; this.kurs_year_3.IsEnabled = true; this.kurs_year_4.IsEnabled = true;
                this.koef_otsech.IsEnabled = true;
            }
            else
            {
                this.GroupBox_rasrochka.IsEnabled = false;
                this.kurs_start.IsEnabled = false; this.kurs_year_0.IsEnabled = false; this.kurs_year_1.IsEnabled = false;
                this.kurs_year_2.IsEnabled = false; this.kurs_year_3.IsEnabled = false; this.kurs_year_4.IsEnabled = false;
                this.koef_otsech.IsEnabled = false;
            }

            if (p_type_proc == "A") this.type_annuitet.IsEnabled = true;
            else if (p_type_proc == "K") this.type_annuitet.IsEnabled = true;
            else this.type_annuitet.IsEnabled = false;
        }

        // Расчет суммы кредита
        public void Calc_sum_cred()
        {
            double m_sum_cred;
            if (this.check_recalc.IsChecked == false)
            {
                m_sum_cred = String_to_Double(this.summa_ekv.Text) - String_to_Double(this.summa_ekv.Text) * String_to_Double(this.proc_perv_vznos.Text) / 100;
            }
            else
            {
                m_sum_cred = String_to_Double(this.summa_ekv.Text) - String_to_Double(this.perv_vznos.Text);
            }
            if (m_sum_cred < 0) { m_sum_cred = 0; }
            this.sum_kred.Text = Double_to_String(m_sum_cred);
        }

    // Получить курс НБУ
    public double GetKursNbu(String mCurrCode, DateTime mDate)
        {                       
            String mPath = tec_kat_temp;
            String mPathXml = mPath + "\\" + mDate.ToString("yyyyMMdd") + ".xml";

            // если папка не существует, создаем
            DirectoryInfo dirInfo = new DirectoryInfo(mPath);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }

            FileInfo fileInf = new FileInfo(mPathXml);
            
            // Если нет файла взять его с сайта
            if (!fileInf.Exists)
            {
                string url_text = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?&date=%MDATE%";
                url_text = url_text.Replace("%MDATE%", mDate.ToString("yyyyMMdd"));
                // чтение файла с НБУ                       
                try
                {                    
                    using (System.Net.WebClient wc = new System.Net.WebClient())
                    {
                        string text = wc.DownloadString(url_text);
                        // запись в файл
                        using FileStream fstream = new FileStream(mPathXml, FileMode.Create);
                        // преобразуем строку в байты                                                        
                        byte[] array = System.Text.Encoding.Default.GetBytes(text);
                        // запись массива байтов в файл
                        fstream.Write(array, 0, array.Length);
                    }                                                                
                    // перечитать созданный файл
                    fileInf = new FileInfo(mPathXml);
                }
                catch (Exception e)
                {
                    MessageBoxError("Ошибка !!! курс с сайта НБУ загрузить не получилось !!!" + "\n" + "URL=" + "\n" + url_text + "\n" + "Message=" + "\n" + e.Message);
                    return 1;
                }
            }

            if (fileInf.Exists)
            {
                try
                {
                    // чтение XML файла    
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(mPathXml);
                    XmlElement xRoot = xDoc.DocumentElement;
                    List<Currency> currencyList = new List<Currency>();
                    // поиск строки с курсом
                    foreach (XmlElement xnode in xRoot)
                    {
                        Currency currency = new Currency();
                        foreach (XmlNode childnode in xnode.ChildNodes)
                        {
                            if (childnode.Name == "rate") { currency.RATE = String_to_Double(childnode.InnerText); }
                            else if (childnode.Name == "cc") { currency.CC = childnode.InnerText; }                            
                            currencyList.Add(currency);
                        }
                    }
                        
                    foreach (Currency u in currencyList)
                    {
                        if (u.CC == mCurrCode)
                        {
                            return u.RATE;
                        }
                    }                        
                }
                catch (Exception e)
                {
                    MessageBoxError("Ошибка !!! не найден курс !!!" + "\n" + "СС=" + "\n" + mCurrCode + "\n" + "Message=" + "\n" + e.Message);
                    return 1;
                }
            }
            return 1;
        }

        // Поиск шаблонов
        public void Poisk_xml_files()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(tec_kat_ini);
            if (dirInfo.Exists)
            {
                FileInfo[] listOfFiles = dirInfo.GetFiles("*.xml");
                type_ini_mas = new String[listOfFiles.Length];
                file_path_ini_mas = new String[listOfFiles.Length];
                int ii = 0;
                list.Clear();

                foreach (FileInfo file in listOfFiles)
                {
                    if (file.Exists)
                    {
                        try
                        {
                            // чтение XML файла
                            file_path_ini_mas[ii] = file.FullName;
                            XmlDocument xDoc = new XmlDocument();
                            xDoc.Load(file.FullName);
                            foreach (XmlNode node in xDoc.DocumentElement.ChildNodes)                                
                            {
                                if (node.Name == "global")
                                {
                                  type_ini_mas[ii] = node.SelectSingleNode("./type").InnerText;
                                  list.Add(node.SelectSingleNode("./name").InnerText);                                
                                }                                
                            }                                                                           
                        }
                        catch (Exception e)
                        {
                            MessageBoxError("Ошибка !!!" + "\n" + e.Message);
                            Environment.Exit(0);
                        }
                    }
                    ii++;
                }
                this.type_rasch.ItemsSource = list;
                this.type_rasch.SelectedIndex = 0;
            }
        }

        // Чтение XML файла
        public void Read_xml_file_kred_calc()
        {
            try
            {
                // чтение XML файла
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(file_path_ini_mas[this.type_rasch.SelectedIndex]);
                foreach (XmlNode node in xDoc.DocumentElement.ChildNodes)
                {
                    if (node.Name == "main")
                    {
                        string m_date_kred = node.SelectSingleNode("./date_cred").InnerText;
                        if (m_date_kred == "" || IsDateValid(m_date_kred) == false)
                        {
                            this.date_cred.SelectedDate = DateTime.Today;
                        }
                        else
                        {
                            this.date_cred.SelectedDate = Convert.ToDateTime(DateTime.ParseExact(m_date_kred, "dd.MM.yyyy", CultureInfo.InvariantCulture));
                        }
                        //
                        string m_curr_code = node.SelectSingleNode("./curr_code").InnerText;
                        if (m_curr_code == "" || (m_curr_code != "UAH" && m_curr_code != "USD" && m_curr_code != "EUR" && m_curr_code != "GBP"))
                        {
                            m_curr_code = "UAH";
                        }
                        this.curr_code.Text = m_curr_code;
                        this.priv_proc_stavka.Text = node.SelectSingleNode("./priv_proc_stavka").InnerText;
                        this.priv_srok_kred.Text = node.SelectSingleNode("./priv_srok").InnerText;
                        this.proc_stavka.Text = node.SelectSingleNode("./proc_stavka").InnerText;
                        this.summa.Text = node.SelectSingleNode("./summa").InnerText;
                        this.proc_perv_vznos.Text = node.SelectSingleNode("./perv_vznos_proc").InnerText;
                        this.perv_vznos.Text = node.SelectSingleNode("./perv_vznos").InnerText;
                        this.srok_kred.Text = node.SelectSingleNode("./srok").InnerText;
                        // Первоначальный взнос
                        if (this.perv_vznos.Text == "") this.check_recalc.IsChecked = false;
                        else this.check_recalc.IsChecked = true;
                        Calc_perv_vznos((Boolean) this.check_recalc.IsChecked);
                        //
                        string m_type_proc = node.SelectSingleNode("./type_proc").InnerText;
                        if (m_type_proc == "" || m_type_proc == "K") this.type_proc.SelectedIndex = 0;
                        else if (m_type_proc == "A") this.type_proc.SelectedIndex = 1;
                        else if (m_type_proc == "R") this.type_proc.SelectedIndex = 2;
                        else this.type_proc.SelectedIndex = 0;
                        // Форма погашения
                        Calc_type_proc(m_type_proc);
                        // Расчет и установление курса и расчет эквивалента
                        Calc_set_kurs_text(m_curr_code, node.SelectSingleNode("./kurs").InnerText);
                    }
                    else if (node.Name == "dopoln")
                    {
                        this.bank_komiss_1.Text = node.SelectSingleNode("./bank_komiss_1").InnerText;
                        this.bank_komiss_1_text.Content = Get_String_Length(node.SelectSingleNode("./bank_komiss_1_text").InnerText);
                        this.bank_komiss_2.Text = node.SelectSingleNode("./bank_komiss_2").InnerText;
                        this.bank_komiss_2_text.Content = Get_String_Length(node.SelectSingleNode("./bank_komiss_2_text").InnerText);
                        this.stra_komiss_1.Text = node.SelectSingleNode("./stra_komiss_1").InnerText;
                        this.stra_komiss_1_text.Content = Get_String_Length(node.SelectSingleNode("./stra_komiss_1_text").InnerText);
                        this.stra_komiss_2.Text = node.SelectSingleNode("./stra_komiss_2").InnerText;
                        this.stra_komiss_2_text.Content = Get_String_Length(node.SelectSingleNode("./stra_komiss_2_text").InnerText);
                        this.stra_komiss_3.Text = node.SelectSingleNode("./stra_komiss_3").InnerText;
                        this.stra_komiss_3_text.Content = Get_String_Length(node.SelectSingleNode("./stra_komiss_3_text").InnerText);
                        this.nota_komiss_1.Text = node.SelectSingleNode("./nota_komiss_1").InnerText;
                        this.nota_komiss_1_text.Content = Get_String_Length(node.SelectSingleNode("./nota_komiss_1_text").InnerText);
                        this.nota_komiss_2.Text = node.SelectSingleNode("./nota_komiss_2").InnerText;
                        this.nota_komiss_2_text.Content = Get_String_Length(node.SelectSingleNode("./nota_komiss_2_text").InnerText);
                        this.nota_komiss_3.Text = node.SelectSingleNode("./nota_komiss_3").InnerText;
                        this.nota_komiss_3_text.Content = Get_String_Length(node.SelectSingleNode("./nota_komiss_3_text").InnerText);
                        this.nota_komiss_4.Text = node.SelectSingleNode("./nota_komiss_4").InnerText;
                        this.nota_komiss_4_text.Content = Get_String_Length(node.SelectSingleNode("./nota_komiss_4_text").InnerText);
                        this.nota_komiss_5.Text = node.SelectSingleNode("./nota_komiss_5").InnerText;
                        this.nota_komiss_5_text.Content = Get_String_Length(node.SelectSingleNode("./nota_komiss_5_text").InnerText);
                        this.riel_komiss_1.Text = node.SelectSingleNode("./riel_komiss_1").InnerText;
                        this.riel_komiss_1_text.Content = Get_String_Length(node.SelectSingleNode("./riel_komiss_1_text").InnerText);
                        this.riel_komiss_2.Text = node.SelectSingleNode("./riel_komiss_2").InnerText;
                        this.riel_komiss_2_text.Content = Get_String_Length(node.SelectSingleNode("./riel_komiss_2_text").InnerText);
                        this.riel_komiss_3.Text = node.SelectSingleNode("./riel_komiss_3").InnerText;
                        this.riel_komiss_3_text.Content = Get_String_Length(node.SelectSingleNode("./riel_komiss_3_text").InnerText);
                    }
                    else if (node.Name == "rasrochka")
                    {
                        this.kurs_start.Text = node.SelectSingleNode("./kurs").InnerText;
                        this.kurs_year_0.Text = node.SelectSingleNode("./kurs_year_0").InnerText;
                        this.kurs_year_1.Text = node.SelectSingleNode("./kurs_year_1").InnerText;
                        this.kurs_year_2.Text = node.SelectSingleNode("./kurs_year_2").InnerText;
                        this.kurs_year_3.Text = node.SelectSingleNode("./kurs_year_3").InnerText;
                        this.kurs_year_4.Text = node.SelectSingleNode("./kurs_year_4").InnerText;
                        this.koef_otsech.Text = node.SelectSingleNode("./koef_otsech").InnerText;
                    }
                }

                // Расрочка описание годов
                int mYear = this.date_cred.SelectedDate.Value.Year;
                this.year_0.Content = mYear.ToString(); mYear++;
                this.year_1.Content = mYear.ToString(); mYear++;
                this.year_2.Content = mYear.ToString(); mYear++;
                this.year_3.Content = mYear.ToString(); mYear++;
                this.year_4.Content = mYear.ToString();

                // Расчет суммы кредита
                Calc_sum_cred();
                
                // расчитываем поля которые в годах
                this.priv_srok_kred_year.Text = Double_to_String(String_to_Double(this.priv_srok_kred.Text) / 12);
                this.srok_kred_year.Text = Double_to_String(String_to_Double(this.srok_kred.Text) / 12);
                this.srok_kred_year_new.Text = Double_to_String(String_to_Double(this.srok_kred_new.Text) / 12);
            }
            catch (Exception e)
            {
                MessageBoxError("Ошибка !!!" + "\n" + e.Message);
            }
        }
        #endregion

        #region Dop_plat

        // Расчет доп. платежей
        public void Dop_plat()
        {
            double p_calc, p_calc_itog;
            double m_summa_ekv = String_to_Double(this.summa_ekv.Text);
            double m_sum_kred = String_to_Double(this.sum_kred.Text);
            // банк
            p_calc = Dop_plat_in(this.bank_komiss_1.Text, m_summa_ekv, m_sum_kred);
            p_calc += Dop_plat_in(this.bank_komiss_2.Text, m_summa_ekv, m_sum_kred);
            this.bank_itog.Text = Double_to_String(p_calc);
            p_calc_itog = p_calc;
            // страхование       
            p_calc = Dop_plat_in(this.stra_komiss_1.Text, m_summa_ekv, m_sum_kred);
            p_calc += Dop_plat_in(this.stra_komiss_2.Text, m_summa_ekv, m_sum_kred);
            p_calc += Dop_plat_in(this.stra_komiss_3.Text, m_summa_ekv, m_sum_kred);
            this.strax_itog.Text = Double_to_String(p_calc);
            p_calc_itog += p_calc;
            // оформление       
            p_calc = Dop_plat_in(this.nota_komiss_1.Text, m_summa_ekv, m_sum_kred);
            p_calc += Dop_plat_in(this.nota_komiss_2.Text, m_summa_ekv, m_sum_kred);
            p_calc += Dop_plat_in(this.nota_komiss_3.Text, m_summa_ekv, m_sum_kred);
            p_calc += Dop_plat_in(this.nota_komiss_4.Text, m_summa_ekv, m_sum_kred);
            p_calc += Dop_plat_in(this.nota_komiss_5.Text, m_summa_ekv, m_sum_kred);
            this.notar_itog.Text = Double_to_String(p_calc);
            p_calc_itog += p_calc;
            // прочее       
            p_calc = Dop_plat_in(this.riel_komiss_1.Text, m_summa_ekv, m_sum_kred);
            p_calc += Dop_plat_in(this.riel_komiss_2.Text, m_summa_ekv, m_sum_kred);
            p_calc += Dop_plat_in(this.riel_komiss_3.Text, m_summa_ekv, m_sum_kred);
            this.rieltor_itog.Text = Double_to_String(p_calc);
            p_calc_itog += p_calc;
            // итого
            this.sum_dop_plat.Text = Double_to_String(p_calc_itog);            
        }

        // Расчет месячных и годовых платежей        
        public double Dop_plat_in_month_year(double t_sum_kred, String t_value_find)
        {
            // MONTH or YEAR
            if (t_value_find != "%MONTH" && t_value_find != "%YEAR")
            {
                MessageBoxError("Тип не %MONTH или %YEAR");
                return 0;
            }

            double sum_out = 0;
            double m_summa_ekv = String_to_Double(this.summa_ekv.Text);
            String tt = this.bank_komiss_1.Text;
            if (tt.IndexOf(t_value_find) >= 0) { sum_out += Dop_plat_in(tt, m_summa_ekv, t_sum_kred); }
            tt = this.bank_komiss_2.Text;
            if (tt.IndexOf(t_value_find) >= 0) { sum_out += Dop_plat_in(tt, m_summa_ekv, t_sum_kred); }
            tt = this.stra_komiss_1.Text;
            if (tt.IndexOf(t_value_find) >= 0) { sum_out += Dop_plat_in(tt, m_summa_ekv, t_sum_kred); }
            tt = this.stra_komiss_2.Text;
            if (tt.IndexOf(t_value_find) >= 0) { sum_out += Dop_plat_in(tt, m_summa_ekv, t_sum_kred); }
            tt = this.stra_komiss_3.Text;
            if (tt.IndexOf(t_value_find) >= 0) { sum_out += Dop_plat_in(tt, m_summa_ekv, t_sum_kred); }
            tt = this.nota_komiss_1.Text;
            if (tt.IndexOf(t_value_find) >= 0) { sum_out += Dop_plat_in(tt, m_summa_ekv, t_sum_kred); }
            tt = this.nota_komiss_2.Text;
            if (tt.IndexOf(t_value_find) >= 0) { sum_out += Dop_plat_in(tt, m_summa_ekv, t_sum_kred); }
            tt = this.nota_komiss_3.Text;
            if (tt.IndexOf(t_value_find) >= 0) { sum_out += Dop_plat_in(tt, m_summa_ekv, t_sum_kred); }
            tt = this.nota_komiss_4.Text;
            if (tt.IndexOf(t_value_find) >= 0) { sum_out += Dop_plat_in(tt, m_summa_ekv, t_sum_kred); }
            tt = this.nota_komiss_5.Text;
            if (tt.IndexOf(t_value_find) >= 0) { sum_out += Dop_plat_in(tt, m_summa_ekv, t_sum_kred); }
            tt = this.riel_komiss_1.Text;
            if (tt.IndexOf(t_value_find) >= 0) { sum_out += Dop_plat_in(tt, m_summa_ekv, t_sum_kred); }
            tt = this.riel_komiss_2.Text;
            if (tt.IndexOf(t_value_find) >= 0) { sum_out += Dop_plat_in(tt, m_summa_ekv, t_sum_kred); }
            tt = this.riel_komiss_3.Text;
            if (tt.IndexOf(t_value_find) >= 0) { sum_out += Dop_plat_in(tt, m_summa_ekv, t_sum_kred); }
            return sum_out;
        }

        // Доп. платежи автоподстановки
        public double Dop_plat_in(String t_in, double t_summa_ekv, double t_sum_kred)
        {
            String n = t_in;
            double s = t_summa_ekv;
            double s_kred = t_sum_kred;
            double pl;
            double sum_out;

            if (s < 0 || s_kred < 0) { return -1; }
            // оплата ежегодно
            n = n.Replace("%YEAR", "");
            // оплата ежемесяно
            n = n.Replace("%MONTH", "");
            // процент от суммы кредита
            if (n.IndexOf("%S") >= 0)
            {
                n = n.Replace("%S", "");
                pl = String_to_Double(n);
                if (s != 0)
                {
                    sum_out = Round(pl * s_kred / 100, 2);
                }
                else
                {
                    sum_out = 0;
                }
            }
            // процент от суммы квартиры
            else if (n.IndexOf("%F") >= 0)
            {
                n = n.Replace("%F", "");
                pl = String_to_Double(n);
                if (s_kred != 0)
                {
                    sum_out = Round(pl * s / 100, 2);
                }
                else
                {
                    sum_out = 0;
                }
            }
            else
            {
                // просто сумма
                sum_out = String_to_Double(n);
            }

            return sum_out;
        }
        #endregion

        // изменение выбора кода валюты
        private void Curr_code_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (is_program_loading == true) { return; }

            ComboBox comboBox = (ComboBox)sender;
            ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;

            String m_curr_code = selectedItem.Content.ToString();
            // Расчет и установление курса и расчет эквивалента
            Calc_set_kurs_text(m_curr_code, "");
        }

        // при выборе - Формы погашения
        private void Type_proc_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (is_program_loading == true) { return; }

            ComboBox comboBox = (ComboBox)sender;
            ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;

            String m_type_proc = selectedItem.Content.ToString();
            if (m_type_proc == "рассрочка") m_type_proc = "R";
            else if (m_type_proc == "аннуитетная") m_type_proc = "A";
            else if (m_type_proc == "классика") m_type_proc = "K";
            // Форма погашения
            Calc_type_proc(m_type_proc);
        }

        // Нажимаем переключатель расчет по сумме/%
        private void Check_recalcClick(object sender, RoutedEventArgs e)
        { 
            if (is_program_loading == true) { return; }

            CheckBox checkBox = (CheckBox)sender;

            // Первоначальный взнос
            Calc_perv_vznos((Boolean) checkBox.IsChecked);
            // Расчет суммы кредита
            Calc_sum_cred();
        }

        // Наживаем кнопку - Пересчитать
        private void Button_recalcClick(object sender, RoutedEventArgs e)
        {
            if (is_program_loading == true) { return; }

            // Расчет суммы кредита
            Calc_sum_cred();
            // Расчитываем доп. платежи
            Dop_plat();
        }

        // вводим данные в поле Период - Обычные условия
        private void Srok_kredTextChanged(object sender, TextChangedEventArgs e)
        { 
            if (is_program_loading == true) { return; }

            TextBox textBox = (TextBox)sender;
            this.srok_kred_year.Text = Double_to_String(String_to_Double(textBox.Text) / 12);
        }

        // вводим данные в поле Период - Льготные условия
        private void Priv_srok_kredTextChanged(object sender, TextChangedEventArgs e)
        {
            if (is_program_loading == true) { return; }

            TextBox textBox = (TextBox)sender;
            this.priv_srok_kred_year.Text = Double_to_String(String_to_Double(textBox.Text) / 12);
        }

        // вводим данные в поле Новый срок кредита
        private void Srok_kred_newTextChanged(object sender, TextChangedEventArgs e)
        {
            if (is_program_loading == true) { return; }

            TextBox textBox = (TextBox)sender;
            this.srok_kred_year_new.Text = Double_to_String(String_to_Double(textBox.Text) / 12);
        }

        // Изменяем  - Тип расчета
        private void Type_rasch_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (is_program_loading == true) { return; }

            Read_xml_file_kred_calc();
            Dop_plat();
        }

        // Жмем клавишу - Расчитать
        private void Button_calcClick(object sender, RoutedEventArgs e)
        {
            // пересчитываем перед расчетов таблицы
            // Расчет суммы кредита
            Calc_sum_cred();
            // Расчитываем доп. платежи
            Dop_plat();

            Paint_table();
        }

        // Жмем клавишу - Обновить
        private void Button_updateClick(object sender, RoutedEventArgs e)
        {
            if (is_program_loading == true) { return; }
            
            Read_xml_file_kred_calc();
            Dop_plat();

            // чистка таблицы
            DataGrid1.Items.Clear();
            for (int i = 0; i <= 50; i++)
            {
                DataGrid1.Items.Add(new TDataGridCol { TDate = "", TDolg = "", TPlatInt = "", TPlat = "",
                    TPereplata = "", TPlatDop = "", TItogo = "", TColorType = ""});
            }
        }

        // XML файл
        private void Button_xml_fileClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(file_path_ini_mas[this.type_rasch.SelectedIndex]) { UseShellExecute = true });
        }                                                

        // Экспорт в CSV
        private void Button_ExportCSVClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog OpenSavefileDialog = new SaveFileDialog
            {
                Filter = "CSV file|*.csv",
                Title = "Save an CSV File"
            };
            OpenSavefileDialog.ShowDialog();

            if (OpenSavefileDialog.FileName != "")
            {
                string filename = OpenSavefileDialog.FileName.ToString();
                try
                {
                    ToCSV(this.DataGrid1, filename);
                }
                catch (Exception er)
                {
                    MessageBoxError("Ошибка выгрузки файла CSV !!!" + "\n" + "Message=" + "\n" + er.Message);
                }
            }
        }

        private void ToCSV(DataGrid DataGrid1, string strFilePath)
        {
            StreamWriter sw = new StreamWriter(strFilePath, false, System.Text.Encoding.Default);
            //headers  
            //for (int i = 0; i < DataGrid1.Columns.Count; i++)
            //{
            //    sw.Write(DataGrid1.Columns[i].Header.ToString());
            //    if (i < DataGrid1.Columns.Count - 1)
            //    {
            //        sw.Write(";");
            //    }                
            //}
            sw.Write("TDate;TDolg;TPlatInt;TPlat;TPereplata;TPlatDop;TItogo");
            sw.Write(sw.NewLine);
            foreach (TDataGridCol row in DataGrid1.Items)
            {   
                string value = row.TDate.Replace("Итого:","Itogo:").Replace("Переплата:", "Pereplata:") + ";" +
                               Get_String_Not_Space(row.TDolg) + ";" +
                               Get_String_Not_Space(row.TPlatInt) + ";" +
                               Get_String_Not_Space(row.TPlat) + ";" +
                               Get_String_Not_Space(row.TPereplata) + ";" +
                               Get_String_Not_Space(row.TPlatDop) + ";" +
                               Get_String_Not_Space(row.TItogo)
                               ;

                sw.Write(value);
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }

        // Расчет и вывод таблицы
        public void Paint_table()
        {
            // чистка таблицы
            this.DataGrid1.Items.Clear();

            int i;
            double m_sum_kred = String_to_Double(this.sum_kred.Text);
            double m_proc_stavka = String_to_Double(this.proc_stavka.Text);
            double m_priv_proc_stavka = String_to_Double(this.priv_proc_stavka.Text);
            double m_srok = String_to_Double(this.srok_kred.Text);
            double m_priv_srok = String_to_Double(this.priv_srok_kred.Text);
            String m_type_proc = this.type_proc.Text;
            String m_type_annuitet = this.type_annuitet.Text;
            double m_koef_otsech = String_to_Double(this.koef_otsech.Text);
            // единоразовые
            double m_sum_one = String_to_Double(this.sum_dop_plat.Text);
            string mTColorType;

            if (m_sum_kred == 0)
            {
                MessageBoxError("Расчет и вывод графика невозможен !!! Не расчитана СУММА КРЕДИТА!!!");
                return;
            }

            // Аннуитет
            if (m_type_proc == "аннуитетная")
            {
                // расчет кредитного портфеля
                int zn = 0; int zc = 0;
                if (null != m_type_annuitet) // Расчитываем процентную ставку выраженную в долях
                    switch (m_type_annuitet)
                    {
                        case "30/360":
                            zc = 30; zn = 360; break;
                        case "факт/360":
                            zc = LastDayOfMonth(this.date_cred.SelectedDate.Value); zn = 360; break;
                        case "факт/факт":
                            zc = LastDayOfMonth(this.date_cred.SelectedDate.Value); zn = KolDayOfYear(this.date_cred.SelectedDate.Value); break;
                        default: break;
                    }

                // если % ставка = 0, ставим не 0
                if (m_proc_stavka <= 0) { m_proc_stavka = 0.000001; }

                //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                // Без льготного периода
                if (m_priv_srok == 0)
                {
                    m_proc_stavka = (m_proc_stavka * 0.01 / zn) * zc;
                    // Сумма аннуитетного платежа
                    double annuitet = m_sum_kred * m_proc_stavka / (1 - Math.Pow(1 + m_proc_stavka, -1 * m_srok));
                    // Переплата по кредиту
                    double m_sum_plat = String_to_Double(this.sum_plat.Text);
                    double sum_pereplata = 0;
                    if (m_sum_plat > annuitet)
                    {
                        sum_pereplata = m_sum_plat - annuitet;
                        annuitet = m_sum_plat;
                    }
                    //
                    double summ = m_sum_kred;
                    DateTime d_date = this.date_cred.SelectedDate.Value;
                    double summ_pro = m_sum_kred * m_proc_stavka;
                    double n_pr = 0;
                    double n_ob = annuitet * m_srok + m_sum_one;
                    double summ_dop = 0;
                    int srok_new = 0;

                    for (i = 1; i <= m_srok; i++)
                    {
                        // учет ежегодных
                        double m_sum_year = 0;
                        if ((i - 1) % 12 == 0 && i != 1)
                        {
                            m_sum_year = Dop_plat_in_month_year(summ, "%YEAR");
                        }
                        // учет ежемесяных
                        double m_sum_month = Dop_plat_in_month_year(summ, "%MONTH");

                        int year_int = Convert.ToInt32(Get_date_month(d_date).Substring(0, 4));
                        if (year_int % 2 == 0) mTColorType = "MistyRose";
                        else mTColorType = "AliceBlue";

                        // добавляем строку          
                        DataGrid1.Items.Add(new TDataGridCol
                        {
                            TDate = Get_date_month(d_date),
                            TDolg = Double_to_String(summ),
                            TPlatInt = Double_to_String(summ_pro),
                            TPlat = Double_to_String(annuitet - summ_pro),
                            TPereplata = Double_to_String(sum_pereplata),
                            TPlatDop = Double_to_String(m_sum_one + m_sum_year + m_sum_month),
                            TItogo = Double_to_String(annuitet + m_sum_one + m_sum_year + m_sum_month),
                            TColorType = mTColorType
                        });

                        // +1 месяц
                        d_date = d_date.AddMonths(1);
                        summ = summ - annuitet + m_proc_stavka * summ;
                        n_pr = n_pr + summ_pro + m_sum_one;
                        summ_pro = summ * m_proc_stavka;
                        summ_dop = summ_dop + m_sum_one + m_sum_year + m_sum_month;
                        m_sum_one = 0;
                        if (summ < 0) { break; }
                        srok_new += 1;
                    }

                    this.srok_kred_new.Text = Double_to_String(srok_new);
                    this.srok_kred_year_new.Text = Double_to_String(String_to_Double(this.srok_kred_new.Text) / 12);

                    // Итого
                    DataGrid1.Items.Add(new TDataGridCol
                    {
                        TDate = "Итого:",
                        TDolg = "",
                        TPlatInt = Double_to_String(n_pr),
                        TPlat = Double_to_String(m_sum_kred),
                        TPereplata = "",
                        TPlatDop = Double_to_String(summ_dop),
                        TItogo = Double_to_String(n_ob),
                        TColorType = "LightGreen"
                    });
                    // Переплата
                    DataGrid1.Items.Add(new TDataGridCol
                    {
                        TDate = "Переплата:",
                        TItogo = Double_to_String(n_pr),
                        TColorType = "LightBlue"
                    });
                    this.pereplata.Text = Double_to_String(n_pr + summ_dop);
                }
                //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                // С льготным периодом
                else
                {
                    if (m_priv_proc_stavka <= 0) { m_priv_proc_stavka = 0.000001; }
                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    // Льготный период
                    m_priv_proc_stavka = (m_priv_proc_stavka * 0.01 / zn) * zc;
                    // Сумма аннуитетного платежа
                    double annuitet = m_sum_kred * m_priv_proc_stavka / (1 - Math.Pow(1 + m_priv_proc_stavka, -1 * m_srok));

                    //
                    double summ = m_sum_kred;
                    DateTime d_date = this.date_cred.SelectedDate.Value;
                    double summ_pro = m_sum_kred * m_priv_proc_stavka;
                    double n_pr = 0;
                    double n_ob = annuitet * m_srok + m_sum_one;
                    double summ_dop = 0;
                    for (i = 1; i <= m_priv_srok; i++)
                    {
                        // учет ежегодных
                        double m_sum_year = 0;
                        if ((i - 1) % 12 == 0 && i != 1)
                        {
                            m_sum_year = Dop_plat_in_month_year(summ, "%YEAR");
                        }
                        // учет ежемесяных
                        double m_sum_month = Dop_plat_in_month_year(summ, "%MONTH");

                        int year_int = Convert.ToInt32(Get_date_month(d_date).Substring(0, 4));
                        if (year_int % 2 == 0) mTColorType = "MistyRose";
                        else mTColorType = "AliceBlue";

                        // добавляем строку                                                
                        DataGrid1.Items.Add(new TDataGridCol
                        {
                            TDate = Get_date_month(d_date),
                            TDolg = Double_to_String(summ),
                            TPlatInt = Double_to_String(summ_pro),
                            TPlat = Double_to_String(annuitet - summ_pro),
                            TPereplata = "0.00",
                            TPlatDop = Double_to_String(m_sum_one + m_sum_year + m_sum_month),
                            TItogo = Double_to_String(annuitet + m_sum_one + m_sum_year + m_sum_month),
                            TColorType = mTColorType
                        });

                        // +1 месяц
                        d_date = d_date.AddMonths(1);
                        summ = summ - annuitet + m_priv_proc_stavka * summ;
                        n_pr = n_pr + summ_pro + m_sum_one;
                        summ_pro = summ * m_priv_proc_stavka;
                        summ_dop = summ_dop + m_sum_one + m_sum_year + m_sum_month;
                        m_sum_one = 0;
                        if (summ < 0) { break; }
                    }

                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    // Обычный период
                    m_srok -= m_priv_srok;
                    m_proc_stavka = (m_proc_stavka * 0.01 / zn) * zc;
                    // Сумма аннуитетного платежа
                    annuitet = summ * m_proc_stavka / (1 - Math.Pow(1 + m_proc_stavka, -1 * m_srok));
                    //
                    summ_pro = summ * m_proc_stavka;
                    n_ob += (annuitet * m_srok);
                    for (i = 1; i <= m_srok; i++)
                    {
                        // учет ежегодных
                        double m_sum_year = 0;                        
                        if ((i - 1) % 12 == 0 && i != 1)
                        {
                            m_sum_year = Dop_plat_in_month_year(summ, "%YEAR");
                        }
                        // учет ежемесяных
                        double m_sum_month = Dop_plat_in_month_year(summ, "%MONTH");

                        int year_int = Convert.ToInt32(Get_date_month(d_date).Substring(0, 4));
                        if (year_int % 2 == 0) mTColorType = "MistyRose";
                        else mTColorType = "AliceBlue";

                        // добавляем строку                                                
                        DataGrid1.Items.Add(new TDataGridCol
                        {
                            TDate = Get_date_month(d_date),
                            TDolg = Double_to_String(summ),
                            TPlatInt = Double_to_String(summ_pro),
                            TPlat = Double_to_String(annuitet - summ_pro),
                            TPereplata = "0.00",
                            TPlatDop = Double_to_String(m_sum_one + m_sum_year + m_sum_month),
                            TItogo = Double_to_String(annuitet + m_sum_one + m_sum_year + m_sum_month),
                            TColorType = mTColorType
                        });

                        // +1 месяц
                        d_date = d_date.AddMonths(1);
                        summ = summ - annuitet + m_proc_stavka * summ;
                        n_pr = n_pr + summ_pro + m_sum_one;
                        summ_pro = summ * m_proc_stavka;
                        summ_dop = summ_dop + m_sum_one + m_sum_year + m_sum_month;
                        m_sum_one = 0;
                        if (summ < 0) { break; }
                    }

                    // Итого
                    DataGrid1.Items.Add(new TDataGridCol
                    {
                        TDate = "Итого:",
                        TDolg = "",
                        TPlatInt = Double_to_String(n_pr),
                        TPlat = Double_to_String(m_sum_kred),
                        TPereplata = "",
                        TPlatDop = Double_to_String(summ_dop),
                        TItogo = Double_to_String(n_ob),
                        TColorType = "LightGreen"
                    });
                    // Переплата
                    DataGrid1.Items.Add(new TDataGridCol
                    {
                        TDate = "Переплата:",
                        TItogo = Double_to_String(n_pr),
                        TColorType = "LightBlue"
                    });
                    this.pereplata.Text = Double_to_String(n_pr + summ_dop);
                }
            }

            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            // Стандартный
            if (m_type_proc == "классика")
            {
                double summ = m_sum_kred;
                double summ_graf = m_sum_kred;
                double n_pr = 0;
                double n_ob = 0;
                double n_cred = 0;
                double n_perepl = 0;
                double sum_year;
                double pr;
                double summ_dop = 0;
                double m_sum_plat = String_to_Double(this.sum_plat.Text);
                String[] mass_date = new String[(int)m_srok];
                double[,] mass_num = new double[6, (int)m_srok];
                double zc = 0;
                double zn = 0;
                double sum_pereplata = 0;
                int srok_new;

                // платежи кредит
                DateTime d_date = this.date_cred.SelectedDate.Value;
                for (i = 1; i <= m_srok; i++)
                {
                    if (null != m_type_annuitet) // Расчитываем процентную ставку выраженную в долях
                        switch (m_type_annuitet)
                        {
                            case "30/360":
                                zc = 30; zn = 360; break;
                            case "факт/360":
                                zc = LastDayOfMonth(d_date); zn = 360; break;
                            case "факт/факт":
                                zc = LastDayOfMonth(d_date); zn = KolDayOfYear(d_date); break;
                            default: break;
                        }

                    // льготная
                    if (i <= m_priv_srok)
                    {
                        pr = summ_graf * m_priv_proc_stavka * (zc / zn) / 100;
                    }
                    // обычная
                    else
                    {
                        pr = summ_graf * m_proc_stavka * (zc / zn) / 100;
                    }

                    // учет ежегодных
                    sum_year = 0;
                    if ((i - 1) % 12 == 0 && i != 1)
                    {
                        sum_year = Dop_plat_in_month_year(summ_graf, "%YEAR");
                    }
                    // учет ежемесяных
                    double sum_month = Dop_plat_in_month_year(summ_graf, "%MONTH");
                    // учет переплаты
                    double calc_sum_cred = m_sum_kred / m_srok;
                    double sum_itog = Round(calc_sum_cred + pr + m_sum_one + sum_year + sum_month, 2);

                    mass_date[i - 1] = Get_date_month(d_date);
                    mass_num[0, i - 1] = Round(summ, 2);
                    mass_num[1, i - 1] = Round(pr, 2);
                    mass_num[2, i - 1] = Round(calc_sum_cred, 2);
                    mass_num[3, i - 1] = Round(m_sum_one + sum_year + sum_month, 2);
                    mass_num[5, i - 1] = 0;

                    if (m_sum_plat > Round(calc_sum_cred + pr, 2))
                    {
                        sum_pereplata = m_sum_plat - Round(calc_sum_cred + pr, 2); // переплата
                        mass_num[5, i - 1] = sum_pereplata;
                        // если последний платеж, корректируем переплату
                        if (summ - (m_sum_plat - Round(pr, 2)) <= 0)
                        {
                            sum_pereplata = 0;
                            calc_sum_cred = summ;
                            mass_num[2, i - 1] = calc_sum_cred;
                            mass_num[5, i - 1] = sum_pereplata;
                            // пересчет %
                            if (null != m_type_annuitet) // Расчитываем процентную ставку выраженную в долях
                                switch (m_type_annuitet)
                                {
                                    case "30/360":
                                        zc = 30; zn = 360; break;
                                    case "факт/360":
                                        zc = LastDayOfMonth(d_date); zn = 360; break;
                                    case "факт/факт":
                                        zc = LastDayOfMonth(d_date); zn = KolDayOfYear(d_date); break;
                                    default: break;
                                }

                            // льготная
                            if (i <= m_priv_srok)
                            {
                                pr = summ * m_priv_proc_stavka * (zc / zn) / 100;
                            }
                            // обычная
                            else
                            {
                                pr = summ * m_proc_stavka * (zc / zn) / 100;
                            }

                            mass_num[1, i - 1] = Round(pr, 2);
                            sum_itog = Round(calc_sum_cred + pr + m_sum_one + sum_year + sum_month, 2);
                            ///////////////////////////////////////////////////////////////////////////
                        }
                        summ -= (m_sum_plat - Round(pr, 2));
                    }
                    else
                    {
                        summ -= Round(calc_sum_cred, 2);
                    }

                    mass_num[4, i - 1] = sum_itog + sum_pereplata;
                    summ_graf -= Round(calc_sum_cred, 2);
                    // +1 месяц
                    d_date = d_date.AddMonths(1);
                    n_pr += pr;
                    n_ob = n_ob + calc_sum_cred + pr + m_sum_one + sum_pereplata;
                    n_cred += calc_sum_cred;
                    n_perepl += sum_pereplata;
                    summ_dop = summ_dop + m_sum_one + sum_year + sum_month;
                    m_sum_one = 0;
                    if (summ < 0) { break; }
                }
                //
                srok_new = 0;
                for (i = 1; i <= m_srok; i++)
                {
                    if (mass_num[2, i - 1] == 0) { break; }

                    int year_int = Convert.ToInt32(mass_date[i - 1].Substring(0, 4));
                    if (year_int % 2 == 0) mTColorType = "MistyRose";
                    else mTColorType = "AliceBlue";

                    // добавляем строку                                                
                    DataGrid1.Items.Add(new TDataGridCol
                    {
                        TDate = mass_date[i - 1],
                        TDolg = Double_to_String(mass_num[0, i - 1]),
                        TPlatInt = Double_to_String(mass_num[1, i - 1]),
                        TPlat = Double_to_String(mass_num[2, i - 1]),
                        TPereplata = Double_to_String(mass_num[5, i - 1]),
                        TPlatDop = Double_to_String(mass_num[3, i - 1]),
                        TItogo = Double_to_String(mass_num[4, i - 1]),
                        TColorType = mTColorType
                    });
                    srok_new += 1;
                }

                this.srok_kred_new.Text = Double_to_String(srok_new);
                this.srok_kred_year_new.Text = Double_to_String(String_to_Double(this.srok_kred_new.Text) / 12);

                // Итого
                DataGrid1.Items.Add(new TDataGridCol
                {
                    TDate = "Итого:",
                    TDolg = "",
                    TPlatInt = Double_to_String(n_pr),
                    TPlat = Double_to_String(n_cred),
                    TPereplata = Double_to_String(n_perepl),
                    TPlatDop = Double_to_String(summ_dop),
                    TItogo = Double_to_String(n_ob),
                    TColorType = "LightGreen"
                });
                // Переплата
                DataGrid1.Items.Add(new TDataGridCol
                {
                    TDate = "Переплата:",
                    TItogo = Double_to_String(Round(n_pr + summ_dop, 2)),
                    TColorType = "LightBlue"
                });
                this.pereplata.Text = Double_to_String(n_pr + summ_dop);
            }
            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            // Рассрочка
            if (m_type_proc == "рассрочка")
            {
                double summ = m_sum_kred;
                double summ_graf = m_sum_kred;
                double n_pr = 0;
                double n_ob = 0;
                double n_cred = 0;
                double n_perepl = 0;
                double sum_year;
                double pr;
                double nk;
                double summ_dop = 0;
                double m_sum_plat = String_to_Double(this.sum_plat.Text);
                String[] mass_date = new String[(int)m_srok];
                double[,] mass_num = new double[6, (int)m_srok];
                double sum_pereplata = 0;
                int srok_new;
                double m_kurs_start = String_to_Double(this.kurs_start.Text);
                double m_kurs_year_0 = String_to_Double(this.kurs_year_0.Text);
                double m_kurs_year_1 = String_to_Double(this.kurs_year_1.Text);
                double m_kurs_year_2 = String_to_Double(this.kurs_year_2.Text);
                double m_kurs_year_3 = String_to_Double(this.kurs_year_3.Text);
                double m_kurs_year_4 = String_to_Double(this.kurs_year_4.Text);

                // платежи кредит
                DateTime d_date = this.date_cred.SelectedDate.Value;
                DateTime d_date_etalon = d_date;
                for (i = 1; i <= m_srok; i++)
                {
                    // начальный год
                    if (Get_YEAR(d_date, 0) == Get_YEAR(d_date_etalon, 0))
                    {
                        nk = m_kurs_year_0 / m_kurs_start;
                    }
                    else if (Get_YEAR(d_date, 0) == Get_YEAR(d_date_etalon, 1))
                    {
                        nk = m_kurs_year_1 / m_kurs_start;
                    }
                    else if (Get_YEAR(d_date, 0) == Get_YEAR(d_date_etalon, 2))
                    {
                        nk = m_kurs_year_2 / m_kurs_start;
                    }
                    else if (Get_YEAR(d_date, 0) == Get_YEAR(d_date_etalon, 3))
                    {
                        nk = m_kurs_year_3 / m_kurs_start;
                    }
                    else if (Get_YEAR(d_date, 0) == Get_YEAR(d_date_etalon, 4) || Get_YEAR(d_date, 0) > Get_YEAR(d_date_etalon, 4))
                    {
                        nk = m_kurs_year_4 / m_kurs_start;
                    }
                    else { nk = 0; }

                    if (nk <= m_koef_otsech) { nk = 1; }
                    pr = (nk - 1) * (m_sum_kred / m_srok);

                    // учет ежегодных
                    sum_year = 0;
                    if ((i - 1) % 12 == 0 && i != 1)
                    {
                        sum_year = Dop_plat_in_month_year(summ_graf, "%YEAR");
                    }
                    // учет ежемесяных
                    double sum_month = Dop_plat_in_month_year(summ_graf, "%MONTH");
                    // учет переплаты
                    double calc_sum_cred = m_sum_kred / m_srok;
                    double sum_itog = Round(calc_sum_cred + pr + m_sum_one + sum_year + sum_month, 2);

                    mass_date[i - 1] = Get_date_month(d_date);
                    mass_num[0, i - 1] = Round(summ, 2);
                    mass_num[1, i - 1] = Round(pr, 2);
                    mass_num[2, i - 1] = Round(calc_sum_cred, 2);
                    mass_num[3, i - 1] = Round(m_sum_one + sum_year + sum_month, 2);
                    mass_num[5, i - 1] = 0;

                    if (m_sum_plat > Round(calc_sum_cred + pr, 2))
                    {
                        sum_pereplata = m_sum_plat - Round(calc_sum_cred + pr, 2); // переплата
                        mass_num[5, i - 1] = sum_pereplata;
                        // если последний платеж, корректируем переплату
                        if (summ - (m_sum_plat - Round(pr, 2)) <= 0)
                        {
                            sum_pereplata = 0;
                            calc_sum_cred = summ;
                            mass_num[2, i - 1] = calc_sum_cred;
                            mass_num[5, i - 1] = sum_pereplata;
                            // пересчет %

                            // начальный год
                            if (Get_YEAR(d_date, 0) == Get_YEAR(d_date_etalon, 0))
                            {
                                nk = m_kurs_year_0 / m_kurs_start;
                            }
                            else if (Get_YEAR(d_date, 0) == Get_YEAR(d_date_etalon, 1))
                            {
                                nk = m_kurs_year_1 / m_kurs_start;
                            }
                            else if (Get_YEAR(d_date, 0) == Get_YEAR(d_date_etalon, 2))
                            {
                                nk = m_kurs_year_2 / m_kurs_start;
                            }
                            else if (Get_YEAR(d_date, 0) == Get_YEAR(d_date_etalon, 3))
                            {
                                nk = m_kurs_year_3 / m_kurs_start;
                            }
                            else if (Get_YEAR(d_date, 0) == Get_YEAR(d_date_etalon, 4) || Get_YEAR(d_date, 0) > Get_YEAR(d_date_etalon, 4))
                            {
                                nk = m_kurs_year_4 / m_kurs_start;
                            }
                            else { nk = 0; }

                            if (nk <= m_koef_otsech) { nk = 1; }
                            pr = (nk - 1) * (calc_sum_cred);

                            mass_num[1, i - 1] = Round(pr, 2);
                            sum_itog = Round(calc_sum_cred + pr + m_sum_one + sum_year + sum_month, 2);
                            ///////////////////////////////////////////////////////////////////////////
                        }
                        summ -= (m_sum_plat - Round(pr, 2));
                    }
                    else
                    {
                        summ -= Round(calc_sum_cred, 2);
                    }

                    mass_num[4, i - 1] = sum_itog + sum_pereplata;

                    summ_graf -= Round(calc_sum_cred, 2);
                    // +1 месяц
                    d_date = d_date.AddMonths(1);
                    n_pr += pr;
                    n_ob = n_ob + calc_sum_cred + pr + m_sum_one + sum_pereplata;
                    n_cred += calc_sum_cred;
                    n_perepl += sum_pereplata;
                    summ_dop = summ_dop + m_sum_one + sum_year + sum_month;
                    m_sum_one = 0;
                    if (summ < 0) { break; }
                }
                //                
                srok_new = 0;
                for (i = 1; i <= m_srok; i++)
                {
                    if (mass_num[2, i - 1] == 0) { break; }

                    int year_int = Convert.ToInt32(mass_date[i - 1].Substring(0, 4));
                    if (year_int % 2 == 0)  mTColorType = "MistyRose";
                    else mTColorType = "AliceBlue";

                    // добавляем строку                                                
                    DataGrid1.Items.Add(new TDataGridCol
                        {
                            TDate = mass_date[i - 1],
                            TDolg = Double_to_String(mass_num[0, i - 1]),
                            TPlatInt = Double_to_String(mass_num[1, i - 1]),
                            TPlat = Double_to_String(mass_num[2, i - 1]),
                            TPereplata = Double_to_String(mass_num[5, i - 1]),
                            TPlatDop = Double_to_String(mass_num[3, i - 1]),
                            TItogo = Double_to_String(mass_num[4, i - 1]),
                            TColorType = mTColorType
                        });                    

                    srok_new += 1;
                }

                this.srok_kred_new.Text = Double_to_String(srok_new);
                this.srok_kred_year_new.Text = Double_to_String(String_to_Double(this.srok_kred_new.Text) / 12);

                // Итого
                DataGrid1.Items.Add(new TDataGridCol
                {
                    TDate = "Итого:",
                    TDolg = "",
                    TPlatInt = Double_to_String(n_pr),
                    TPlat = Double_to_String(n_cred),
                    TPereplata = Double_to_String(n_perepl),
                    TPlatDop = Double_to_String(summ_dop),
                    TItogo = Double_to_String(n_ob),
                    TColorType = "LightGreen"
                });
                // Переплата
                DataGrid1.Items.Add(new TDataGridCol
                {
                    TDate = "Переплата:",
                    TItogo = Double_to_String(Round(n_pr + summ_dop, 2)),
                    TColorType = "LightBlue"
                });
                this.pereplata.Text = Double_to_String(n_pr + summ_dop);
            }
            ///////////////////////
        }
    }
}
