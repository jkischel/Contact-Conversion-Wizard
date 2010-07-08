﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Fritz_XML_Wizard
{
    public partial class Form1 : Form
    {
        System.Collections.Hashtable myGroupDataHash;
        string MySaveFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + System.IO.Path.DirectorySeparatorChar + "ContactConversionWizard";

        public Form1()
        {
            InitializeComponent();
            this.Text = this.ProductName + " v" + Application.ProductVersion;

            if (!System.IO.Directory.Exists(MySaveFolder))
            { System.IO.Directory.CreateDirectory(MySaveFolder); }

            
            myGroupDataHash = new System.Collections.Hashtable();

            // initialize country selection combobox to the country the windows OS is set to
            switch (System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName)
            {
                case "DE":
                    combo_prefix.SelectedIndex = 0;
                    break;
                case "AT":
                    combo_prefix.SelectedIndex = 1;
                    break;
                case "CH":
                    combo_prefix.SelectedIndex = 2;
                    break;
                case "UK":
                    combo_prefix.SelectedIndex = 3;
                    break;
                case "IT":
                    combo_prefix.SelectedIndex = 4;
                    break;
                case "NL":
                    combo_prefix.SelectedIndex = 5;
                    break;
                case "BE":
                    combo_prefix.SelectedIndex = 6;
                    break;
                default:
                    combo_prefix.SelectedIndex = 7;
                    break;
            }

            // choose the default naming style for the combined fullname
            // this may later be implemented as saving and reading from the Registry
            combo_namestyle.SelectedIndex = 0;
            combo_typeprefer.SelectedIndex = 0;
            combo_outlookimport.SelectedIndex = 0;
            combo_VIP.SelectedIndex = 0;
            combo_picexport.SelectedIndex = 0;
        }

        private string CleanUpNumber(string number, string country_prefix)
        {
            string return_str = number;

            // clean up white spaces and brackets and some other stuff
            return_str = return_str.Replace(" ", "");
            return_str = return_str.Replace("(", "");
            return_str = return_str.Replace(")", "");
            return_str = return_str.Replace("*", "");
            return_str = return_str.Replace("#", "");
            return_str = return_str.Replace("x", "");

            // clean up country code
            if (return_str.StartsWith("+")) { return_str = "00" + return_str.Substring(1); }
            if (return_str.StartsWith(country_prefix)) { return_str = return_str.Replace(country_prefix, "0"); }

            return return_str;
        }

        private string RetrieveCountryID(string combo_string)
        {
            // retrieve country_id from combobox, if set to custom ask the user for it
            string my_country_id = combo_string;
            if (my_country_id.StartsWith("00"))
            {
                my_country_id = my_country_id.Substring(0, my_country_id.IndexOf(" "));
            }
            else
            {
                // Ask the user for the correct country code
                CustomCountryID MyCustomCountryID = new CustomCountryID();
                MyCustomCountryID.ShowDialog();
                my_country_id = MyCustomCountryID.country_id_transfer;
                MyCustomCountryID.Dispose();
            }

            return my_country_id;
        }

        private string CheckVIPandSPEEDDIALflags(string myNickname, string myStringToCheck, bool ignoreComboBox)
        {
            string result_temp = "No"; // be default, VIP is set to no
            int select_mode = combo_VIP.SelectedIndex;
            if (ignoreComboBox == true) { select_mode = 2; }

            switch (select_mode)
            {
                case 0: // never set as VIP, nothing to do, since already the default
                    break;
                case 1: // check if nickname exists, if yes, set to VIP
                    if (myNickname != "") { result_temp = "Yes"; }
                    break;
                case 2: // check notes field, if contains VIP then set to VIP
                    if (myStringToCheck.Contains("VIP") == true) { result_temp = "Yes"; }
                    break;
                default:
                    MessageBox.Show("Default case in CheckVIPandSPEEDDIALflags, this should not have happened, report this bug!");
                    break;

            }

            if (myStringToCheck.Contains("SPEEDDIAL:") == true)
            {
                int speeddialnumber = -1;
                string parsestring = "";
                try
                {
                    string substring = myStringToCheck.Substring(myStringToCheck.IndexOf("SPEEDDIAL:") + 10);
                    if (substring.Length > 2) { substring = substring.Substring(0, 2); }
                    foreach (char c in substring)
                    {
                        if (char.IsNumber(c) == true)
                        {
                            parsestring += c.ToString();
                        }
                    }
                    speeddialnumber = Convert.ToInt32(parsestring);
                }
                catch
                {
                    // do nothing, here because speeddial might be at end of string or char following might not be a number
                }
                if (speeddialnumber != -1) { result_temp += ":" + speeddialnumber; }
            }

            return result_temp;
        }

        private string GenerateFullName(string Firstname, string Lastname, string theCompany, int style)
        {
            string my_fullname = "";

            if (Firstname == "" && Lastname != "" && theCompany == "") { my_fullname = Lastname;                                           /* use only lastname */ }
            if (Firstname != "" && Lastname == "" && theCompany == "") { my_fullname = Firstname;                                          /* use only firstname */ }
            if (Firstname == "" && Lastname == "" && theCompany != "") { my_fullname = theCompany;                                            /* use only company */ }

            switch (style)
            {
                case 0: // Lastname Firstname [Company]
                    if (Firstname != "" && Lastname != "" && theCompany == "") { my_fullname = Lastname + " " + Firstname;                         /* use both names */ }
                    if (Firstname == "" && Lastname != "" && theCompany != "") { my_fullname = Lastname + " [" + theCompany + "]";                    /* use only lastname + company */ }
                    if (Firstname != "" && Lastname == "" && theCompany != "") { my_fullname = Firstname + " [" + theCompany + "]";                   /* use only firstname + company */ }
                    if (Firstname != "" && Lastname != "" && theCompany != "") { my_fullname = Lastname + " " + Firstname + " [" + theCompany + "]";  /* use both names  + company */ }
                    break;
                case 1: // Lastname, Firstname [Company]
                    if (Firstname != "" && Lastname != "" && theCompany == "") { my_fullname = Lastname + ", " + Firstname;                        /* use both names */ }
                    if (Firstname == "" && Lastname != "" && theCompany != "") { my_fullname = Lastname + " [" + theCompany + "]";                    /* use only lastname + company */ }
                    if (Firstname != "" && Lastname == "" && theCompany != "") { my_fullname = Firstname + " [" + theCompany + "]";                   /* use only firstname + company */ }
                    if (Firstname != "" && Lastname != "" && theCompany != "") { my_fullname = Lastname + ", " + Firstname + " [" + theCompany + "]"; /* use both names  + company */ }
                    break;
                case 2: // Lastname, Firstname, Company
                    if (Firstname != "" && Lastname != "" && theCompany == "") { my_fullname = Lastname + ", " + Firstname;                        /* use both names */ }
                    if (Firstname == "" && Lastname != "" && theCompany != "") { my_fullname = Lastname + ", " + theCompany;                          /* use only lastname + company */ }
                    if (Firstname != "" && Lastname == "" && theCompany != "") { my_fullname = Firstname + ", " + theCompany;                         /* use only firstname + company */ }
                    if (Firstname != "" && Lastname != "" && theCompany != "") { my_fullname = Lastname + ", " + Firstname + ", " + theCompany;       /* use both names  + company */ }
                    break;
                case 3: // Company [Lastname Firstname]
                    if (Firstname != "" && Lastname != "" && theCompany == "") { my_fullname = Lastname + " " + Firstname;                         /* use both names */ }
                    if (Firstname == "" && Lastname != "" && theCompany != "") { my_fullname = theCompany + " [" + Lastname + "]";                   /* use only lastname + company */ }
                    if (Firstname != "" && Lastname == "" && theCompany != "") { my_fullname = theCompany + " [" + Firstname + "]";                   /* use only firstname + company */ }
                    if (Firstname != "" && Lastname != "" && theCompany != "") { my_fullname = theCompany + " [" + Lastname + " " + Firstname + "]";  /* use both names  + company */ }
                    break;
                case 4: // Company [Lastname, Firstname]
                    if (Firstname != "" && Lastname != "" && theCompany == "") { my_fullname = Lastname + ", " + Firstname;                        /* use both names */ }
                    if (Firstname == "" && Lastname != "" && theCompany != "") { my_fullname = theCompany + " [" + Lastname + "]";                    /* use only lastname + company */ }
                    if (Firstname != "" && Lastname == "" && theCompany != "") { my_fullname = theCompany + " [" + Firstname + "]";                   /* use only firstname + company */ }
                    if (Firstname != "" && Lastname != "" && theCompany != "") { my_fullname = theCompany + " [" + Lastname + ", " + Firstname + "]"; /* use both names  + company */ }
                    break;
                case 5: // Company, Lastname, Firstname
                    if (Firstname != "" && Lastname != "" && theCompany == "") { my_fullname = Lastname + ", " + Firstname;                        /* use both names */ }
                    if (Firstname == "" && Lastname != "" && theCompany != "") { my_fullname = theCompany + ", " + Lastname;                         /* use only lastname + company */ }
                    if (Firstname != "" && Lastname == "" && theCompany != "") { my_fullname = theCompany + ", " + Firstname;                         /* use only firstname + company */ }
                    if (Firstname != "" && Lastname != "" && theCompany != "") { my_fullname = theCompany + ", " + Lastname + ", " + Firstname;       /* use both names  + company */ }
                    break;
                case 6: // Firstname Lastname [Company]
                    if (Firstname != "" && Lastname != "" && theCompany == "") { my_fullname = Firstname + " " + Lastname;                         /* use both names */ }
                    if (Firstname == "" && Lastname != "" && theCompany != "") { my_fullname = Lastname + " [" + theCompany + "]";                    /* use only lastname + company */ }
                    if (Firstname != "" && Lastname == "" && theCompany != "") { my_fullname = Firstname + " [" + theCompany + "]";                   /* use only firstname + company */ }
                    if (Firstname != "" && Lastname != "" && theCompany != "") { my_fullname = Firstname + " " + Lastname + " [" + theCompany + "]";  /* use both names  + company */ }
                    break;
                case 7: // Firstname, Lastname [Company]
                    if (Firstname != "" && Lastname != "" && theCompany == "") { my_fullname = Firstname + ", " + Lastname;                        /* use both names */ }
                    if (Firstname == "" && Lastname != "" && theCompany != "") { my_fullname = Lastname + " [" + theCompany + "]";                    /* use only lastname + company */ }
                    if (Firstname != "" && Lastname == "" && theCompany != "") { my_fullname = Firstname + " [" + theCompany + "]";                   /* use only firstname + company */ }
                    if (Firstname != "" && Lastname != "" && theCompany != "") { my_fullname = Firstname + ", " + Lastname + " [" + theCompany + "]"; /* use both names  + company */ }
                    break;
                case 8: // Firstname, Lastname, Company
                    if (Firstname != "" && Lastname != "" && theCompany == "") { my_fullname = Firstname + ", " + Lastname;                        /* use both names */ }
                    if (Firstname == "" && Lastname != "" && theCompany != "") { my_fullname = Lastname + ", " + theCompany;                          /* use only lastname + company */ }
                    if (Firstname != "" && Lastname == "" && theCompany != "") { my_fullname = Firstname + ", " + theCompany;                         /* use only firstname + company */ }
                    if (Firstname != "" && Lastname != "" && theCompany != "") { my_fullname = Firstname + ", " + Lastname + ", " + theCompany;       /* use both names  + company */ }
                    break;
                case 9: // Company [Firstname Lastname]
                    if (Firstname != "" && Lastname != "" && theCompany == "") { my_fullname = Firstname + " " + Lastname;                         /* use both names */ }
                    if (Firstname == "" && Lastname != "" && theCompany != "") { my_fullname = theCompany + " [" + Lastname + "]";                    /* use only lastname + company */ }
                    if (Firstname != "" && Lastname == "" && theCompany != "") { my_fullname = theCompany + " [" + Firstname + "]";                   /* use only firstname + company */ }
                    if (Firstname != "" && Lastname != "" && theCompany != "") { my_fullname = theCompany + " [" + Firstname + " " + Lastname + "]";  /* use both names  + company */ }
                    break;
                case 10: // Company [Firstname, Lastname]
                    if (Firstname != "" && Lastname != "" && theCompany == "") { my_fullname = Firstname + ", " + Lastname;                        /* use both names */ }
                    if (Firstname == "" && Lastname != "" && theCompany != "") { my_fullname = theCompany + " [" + Lastname + "]";                    /* use only lastname + company */ }
                    if (Firstname != "" && Lastname == "" && theCompany != "") { my_fullname = theCompany + " [" + Firstname + "]";                   /* use only firstname + company */ }
                    if (Firstname != "" && Lastname != "" && theCompany != "") { my_fullname = theCompany + " [" + Firstname + ", " + Lastname + "]"; /* use both names  + company */ }
                    break;
                case 11: // Company, Firstname, Lastname
                    if (Firstname != "" && Lastname != "" && theCompany == "") { my_fullname = Firstname + ", " + Lastname;                        /* use both names */ }
                    if (Firstname == "" && Lastname != "" && theCompany != "") { my_fullname = theCompany + ", " + Lastname;                          /* use only lastname + company */ }
                    if (Firstname != "" && Lastname == "" && theCompany != "") { my_fullname = theCompany + ", " + Firstname;                         /* use only firstname + company */ }
                    if (Firstname != "" && Lastname != "" && theCompany != "") { my_fullname = theCompany + ", " + Firstname + ", " + Lastname;       /* use both names  + company */ }
                    break;
                case 12: // Lastname Firstname
                    if (Firstname != "" && Lastname != "" && theCompany == "") { my_fullname = Lastname + " " + Firstname;                         /* use both names */ }
                    if (Firstname == "" && Lastname != "" && theCompany != "") { my_fullname = Lastname;                                           /* use only lastname, ignore company */ }
                    if (Firstname != "" && Lastname == "" && theCompany != "") { my_fullname = Firstname;                                          /* use only firstname, ignore company */ }
                    if (Firstname != "" && Lastname != "" && theCompany != "") { my_fullname = Lastname + " " + Firstname;                         /* use both names, ignore company */ }
                    break;
                case 13: // Lastname, Firstname
                    if (Firstname != "" && Lastname != "" && theCompany == "") { my_fullname = Lastname + ", " + Firstname;                        /* use both names */ }
                    if (Firstname == "" && Lastname != "" && theCompany != "") { my_fullname = Lastname;                                           /* use only lastname + company */ }
                    if (Firstname != "" && Lastname == "" && theCompany != "") { my_fullname = Firstname;                                          /* use only firstname, ignore company */ }
                    if (Firstname != "" && Lastname != "" && theCompany != "") { my_fullname = Lastname + ", " + Firstname;                        /* use both names  + company */ }
                    break;
                case 14: // Firstname Lastname
                    if (Firstname != "" && Lastname != "" && theCompany == "") { my_fullname = Firstname + " " + Lastname;                         /* use both names */ }
                    if (Firstname == "" && Lastname != "" && theCompany != "") { my_fullname = Lastname;                                           /* use only lastname, ignore company */ }
                    if (Firstname != "" && Lastname == "" && theCompany != "") { my_fullname = Firstname;                                          /* use only firstname, ignore company */ }
                    if (Firstname != "" && Lastname != "" && theCompany != "") { my_fullname = Firstname + " " + Lastname;                         /* use both names, ignore company */ }
                    break;
                case 15: // Firstname, Lastname
                    if (Firstname != "" && Lastname != "" && theCompany == "") { my_fullname = Firstname + ", " + Lastname;                        /* use both names */ }
                    if (Firstname == "" && Lastname != "" && theCompany != "") { my_fullname = Lastname;                                           /* use only lastname + company */ }
                    if (Firstname != "" && Lastname == "" && theCompany != "") { my_fullname = Firstname;                                          /* use only firstname, ignore company */ }
                    if (Firstname != "" && Lastname != "" && theCompany != "") { my_fullname = Firstname + ", " + Lastname;                        /* use both names  + company */ }
                    break;
                default:
                    Console.WriteLine("Unsupported Output Name Style, this error should not have occured. Please report this!");
                    break;
            }

            return my_fullname;
        }

        private string LimitNameLength(string my_name, int my_limit)
        {
            if (my_name.Length > my_limit) // only do something if the string is not short enough
            {
                // if the last character is a ], then trim the string to one less and add ] again
                if (my_name.Substring(my_name.Length - 1, 1) == "]")
                {
                    my_name = my_name.Substring(0, (my_limit - 1)) + "]";
                }
                else
                // just trim it to the limiting number
                {
                    my_name = my_name.Substring(0, my_limit);
                }
            }

            return my_name;
        }

        private void diable_buttons(bool on_off)
        {
            // while the program is processing, the user must not be able to click any buttons or edit any checkboxes
            if (on_off == true)
            {
                // processing starts, so all buttons will be disabled and the hourglass cursor will be selected
                Cursor.Current = Cursors.WaitCursor;
                btn_read_Outlook.Enabled = false;
                btn_read_FritzXML.Enabled = false;
                btn_read_vCard.Enabled = false;
                btn_read_FritzAdress.Enabled = false;
                btn_read_SnomCSV8.Enabled = false;

                btn_save_Outlook.Enabled = false;
                btn_save_FritzXML.Enabled = false;
                btn_save_vCard.Enabled = false;
                btn_save_FritzAdress.Enabled = false;
                btn_save_SnomCSV7.Enabled = false;
                btn_save_SnomCSV8.Enabled = false;
                
                button_clear.Enabled = false; 
            }
            else
            {
                // processing has finished, so all buttons will be re-enabled and the normal cursor will be restored
                btn_read_Outlook.Enabled = true;
                btn_read_FritzXML.Enabled = true;
                btn_read_vCard.Enabled = true;
                btn_read_FritzAdress.Enabled = true;
                btn_read_SnomCSV8.Enabled = false;   // needs implementing

                btn_save_Outlook.Enabled = true;
                btn_save_FritzXML.Enabled = true;
                btn_save_vCard.Enabled = false;      // needs implementing
                btn_save_FritzAdress.Enabled = true;
                btn_save_SnomCSV7.Enabled = true;
                btn_save_SnomCSV8.Enabled = false;   // needs implementing
                
                button_clear.Enabled = true;

                Cursor.Current = Cursors.Default;
            }

        }

        private void add_to_database(System.Collections.Hashtable AllToAdd)
        {
            string mergefailures = "";
            foreach (System.Collections.DictionaryEntry addHash in AllToAdd)
            {
                try
                {
                    myGroupDataHash.Add(addHash.Key, addHash.Value);
                }
                catch (ArgumentException) // unable to add to groupdatahash, must mean that something with fullname is already in there!
                { mergefailures += "Entry already in database: " + addHash.Key + Environment.NewLine; }
            }
            if (mergefailures != "") MessageBox.Show(mergefailures, "Unable to merge, because an entry of this name already exists");
        }

        private void update_datagrid()
        {

            MyDataGridView.SuspendLayout();

            MyDataGridView.Rows.Clear();

            foreach (System.Collections.DictionaryEntry contactHash in myGroupDataHash)
            {
                // extract GroupDataList from hashtable contents
                GroupDataContact contactData = (GroupDataContact)contactHash.Value;
                string PhotoPresent = "No";
                if (contactData.jpeg != null)
                {
                    PhotoPresent = "Yes";
                }
                MyDataGridView.Rows.Add(new string[] { contactData.combinedname, contactData.lastname, contactData.firstname, contactData.company, contactData.home, contactData.work, contactData.mobile, contactData.homefax, contactData.workfax, contactData.street, contactData.zip, contactData.city, contactData.email, contactData.VIP, PhotoPresent });
            }

            MyDataGridView.Sort(MyDataGridView.Columns[0], 0);
            MyDataGridView.ResumeLayout();
            MyDataGridView.Refresh();
            
            button_clear.Text = "Clear List (" + myGroupDataHash.Count.ToString() + ")";
            MyDataGridView.Focus();
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            myGroupDataHash = new System.Collections.Hashtable();
            update_datagrid();
        }

        public bool writeByteArrayToFile(byte[] buff, string fileName)
        {
            string savePath = System.IO.Path.Combine(MySaveFolder, fileName);

            bool response = false;

            try
            {
                System.IO.FileStream fs = new System.IO.FileStream(savePath, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite);
                System.IO.BinaryWriter bw = new System.IO.BinaryWriter(fs);
                bw.Write(buff);
                bw.Close();
                response = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error writing to file \"" + savePath + "\":" + ex.Message);
            }

            return response;
        }

        public Microsoft.Office.Interop.Outlook.Attachment GetContactPhoto(Microsoft.Office.Interop.Outlook.ContactItem contact)
        {
            // Find the attchment where PR_ATTACHMENT_CONTACTPHOTO is true
            foreach (Microsoft.Office.Interop.Outlook.Attachment attachment in contact.Attachments)
            {
                bool isContactPhoto = (bool)attachment.PropertyAccessor.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x7FFF000B");
                if (isContactPhoto)
                {
                    return attachment;
                    // You can then use the Attachment.SaveAsFile method to save the file as a JPEG image.
                }
            }

            return null;
        }

        // Import Functionality

        private void btn_read_Outlook_Click(object sender, EventArgs e)
        {
            // processing starts, so now we will disable the buttons first to make sure the user knows this by not having buttons to click on
            diable_buttons(true);
            
            // check whether the user had the shift key pressed while calling this function and store this in a variable for further use
            bool shiftpressed_for_custom_folder = false;
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) { shiftpressed_for_custom_folder = true; }

            ReadDataReturn myReadDataReturn = read_data_Outlook(shiftpressed_for_custom_folder);
            if (myReadDataReturn.duplicates != "") MessageBox.Show(myReadDataReturn.duplicates, "The following duplicate entries could not be imported");
            add_to_database(myReadDataReturn.importedHash);
            update_datagrid();

            diable_buttons(false);
        }    // just click handler

        private void btn_read_FritzXML_Click(object sender, EventArgs e)
        {
            OpenFileDialog Load_Dialog = new OpenFileDialog();
            Load_Dialog.Title = "Select the Fritz!Box XML file you wish to load";
            Load_Dialog.Filter = "XML Files|*.xml";

            if (Load_Dialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            // processing starts, so now we will disable the buttons first to make sure the user knows this by not having buttons to click on
            diable_buttons(true);

            ReadDataReturn myReadDataReturn = read_data_FritzXML(Load_Dialog.FileName);
            if (myReadDataReturn.duplicates != "") MessageBox.Show(myReadDataReturn.duplicates, "The following duplicate entries could not be imported");
            add_to_database(myReadDataReturn.importedHash);
            update_datagrid();

            diable_buttons(false);
        }   // just click handler

        private void btn_read_VC_Click(object sender, EventArgs e)
        {
            // check whether the user had the shift key pressed while calling this function and store this in a variable for further use
            bool shiftpressed_for_custom_folder = false;
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) { shiftpressed_for_custom_folder = true; }

            OpenFileDialog Load_Dialog = new OpenFileDialog();
            Load_Dialog.Title = "Select the vCard file you wish to load";
            Load_Dialog.Filter = "vCard Files|*.vcf";

            if (Load_Dialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            
            // processing starts, so now we will disable the buttons first to make sure the user knows this by not having buttons to click on
            diable_buttons(true);

            ReadDataReturn myReadDataReturn = read_data_vCard(Load_Dialog.FileName, shiftpressed_for_custom_folder);
            if (myReadDataReturn.duplicates != "") MessageBox.Show(myReadDataReturn.duplicates, "The following duplicate entries could not be imported");
            add_to_database(myReadDataReturn.importedHash);
            update_datagrid();

            diable_buttons(false);
        }         // just click handler

        private void btn_read_FritzAdr_Click(object sender, EventArgs e)
        {
            OpenFileDialog Load_Dialog = new OpenFileDialog();
            Load_Dialog.Title = "Select the Fritz!Adress file you wish to load";
            Load_Dialog.Filter = "Text (Tabstop separated)|*.txt";

            if (Load_Dialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            // processing starts, so now we will disable the buttons first to make sure the user knows this by not having buttons to click on
            diable_buttons(true);

            ReadDataReturn myReadDataReturn = read_data_FritzAdr(Load_Dialog.FileName);
            if (myReadDataReturn.duplicates != "") MessageBox.Show(myReadDataReturn.duplicates, "The following duplicate entries could not be imported");
            add_to_database(myReadDataReturn.importedHash);
            update_datagrid();

            diable_buttons(false);
        }   // just click handler

        private void btn_read_SnomXMLv8_Click(object sender, EventArgs e)
        {
            OpenFileDialog Load_Dialog = new OpenFileDialog();
            Load_Dialog.Title = "Select the Snom v8 XML file you wish to load";
            Load_Dialog.Filter = "CSV Files|*.csv";

            if (Load_Dialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            // processing starts, so now we will disable the buttons first to make sure the user knows this by not having buttons to click on
            diable_buttons(true);

            ReadDataReturn myReadDataReturn = read_data_SnomXMLv8(Load_Dialog.FileName);
            if (myReadDataReturn.duplicates != "") MessageBox.Show(myReadDataReturn.duplicates, "The following duplicate entries could not be imported");
            add_to_database(myReadDataReturn.importedHash);
            update_datagrid();

            diable_buttons(false);
        }  // just click handler

        private ReadDataReturn read_data_Outlook(bool customfolder)
        {
            // read all information from Outlook and save all contacts that have at least one phone or fax number
            System.Collections.Hashtable loadDataHash = new System.Collections.Hashtable();

            // a string containing the information on what duplicates were encoutered during import
            string duplicates = "";

            // some basic initialization of the outlook support
            Microsoft.Office.Interop.Outlook.Application outlookObj = null;
            Microsoft.Office.Interop.Outlook.MAPIFolder outlookFolder = null;

            // connect to outlook, if possible. Else, complain and abort.
            try
            {
                outlookObj = new Microsoft.Office.Interop.Outlook.Application();
            }
            catch (Exception outlook_exception)
            {
                MessageBox.Show("Unable to access Outlook!" + Environment.NewLine + Environment.NewLine + "This program needs Outlook to continue, are you sure it's installed and working?" + Environment.NewLine + Environment.NewLine + "Error returned was: " + outlook_exception.ToString() + Environment.NewLine);
                return (new ReadDataReturn(duplicates, loadDataHash));
            }

            // sucessfully connected to outlook, do some further setup work
            Microsoft.Office.Interop.Outlook.NameSpace outlookNS = outlookObj.GetNamespace("MAPI");

            // if the user had shift pressed, allow him to select a custom folder. If not, select the default folder.
            if (customfolder == false)
            {
                // select default contacts folder
                outlookFolder = (Microsoft.Office.Interop.Outlook.MAPIFolder)outlookObj.Session.GetDefaultFolder(Microsoft.Office.Interop.Outlook.OlDefaultFolders.olFolderContacts);
            }
            else
            {
                // force the user to chose a custom folder. If user clicks cancel, just open the folderchooser again until he does select a folder.
                while (outlookFolder == null) { outlookFolder = outlookNS.PickFolder(); }
            }

            // limit items processed to those items which actually are contacts
            string ItemFilter = "[MessageClass] = \"IPM.Contact\"";
            Microsoft.Office.Interop.Outlook.Items oContactItems = outlookFolder.Items.Restrict(ItemFilter);

            // loop through all contacts and extract the data from them, then generate the [Fullname] and store all stuff in the hashtable
            foreach (Microsoft.Office.Interop.Outlook.ContactItem myContactItem in oContactItems)
            {
                GroupDataContact myContact = new GroupDataContact();

                myContact.lastname = (myContactItem.LastName == null) ? string.Empty : myContactItem.LastName;
                myContact.firstname = (myContactItem.FirstName == null) ? string.Empty : myContactItem.FirstName;
                myContact.company = (myContactItem.CompanyName == null) ? string.Empty : myContactItem.CompanyName;
                myContact.home = (myContactItem.HomeTelephoneNumber == null) ? string.Empty : myContactItem.HomeTelephoneNumber;
                myContact.work = (myContactItem.BusinessTelephoneNumber == null) ? string.Empty : myContactItem.BusinessTelephoneNumber;
                myContact.homefax = (myContactItem.HomeFaxNumber == null) ? string.Empty : myContactItem.HomeFaxNumber;
                myContact.workfax = (myContactItem.BusinessFaxNumber == null) ? string.Empty : myContactItem.BusinessFaxNumber;
                myContact.mobile = (myContactItem.MobileTelephoneNumber == null) ? string.Empty : myContactItem.MobileTelephoneNumber;
                myContact.preferred = string.Empty; // outlook has no preferred phone
                myContact.street = (myContactItem.MailingAddressStreet == null) ? string.Empty : myContactItem.MailingAddressStreet;
                myContact.street = myContact.street.Replace(Environment.NewLine, " - ");
                myContact.zip = (myContactItem.MailingAddressPostalCode == null) ? string.Empty : myContactItem.MailingAddressPostalCode;
                myContact.city = (myContactItem.MailingAddressCity == null) ? string.Empty : myContactItem.MailingAddressCity;
                myContact.email = (myContactItem.Email1Address == null) ? string.Empty : myContactItem.Email1Address;

                // store picture in myContact.jpeg, if present:
                Microsoft.Office.Interop.Outlook.Attachment myAttachmentPhoto = GetContactPhoto(myContactItem);
                if (myAttachmentPhoto != null)
                {
                    string tempname = System.IO.Path.GetTempFileName();
                    myAttachmentPhoto.SaveAsFile(tempname);
                    byte[] encodedDataAsBytes = System.IO.File.ReadAllBytes(tempname);
                    System.IO.File.Delete(tempname);
                    myContact.jpeg = encodedDataAsBytes;
                }

                // generate full name from parts or from FileAs field, depending on combobox selection
                switch (combo_outlookimport.SelectedIndex)
                {
                    case 0:
                        myContact.combinedname = GenerateFullName(myContact.firstname, myContact.lastname, myContact.company, combo_namestyle.SelectedIndex);
                        break;
                    case 1:
                        myContact.combinedname = (myContactItem.FileAs == null) ? string.Empty : myContactItem.FileAs;
                        break;
                    default:
                        break;
                }
                // check if contact is supposed to be VIP
                string NotesBody = (myContactItem.Body == null) ? string.Empty : myContactItem.Body;
                string nickname = (myContactItem.NickName == null) ? string.Empty : myContactItem.NickName;

                myContact.VIP = CheckVIPandSPEEDDIALflags(nickname, NotesBody, false);


                if (myContact.combinedname != "" && (myContact.home != string.Empty || myContact.work != string.Empty || myContact.mobile != string.Empty || myContact.homefax != string.Empty || myContact.workfax != string.Empty) && (NotesBody.Contains("CCW-IGNORE") == false))
                {
                    try
                    {
                        loadDataHash.Add(myContact.combinedname, myContact);
                    }
                    catch (ArgumentException) // unable to add to groupdatahash, must mean that something with fullname is already in there!
                    { duplicates += "Duplicate entry in source (Outlook): " + myContact.combinedname + Environment.NewLine; }

                }
            }

            return (new ReadDataReturn(duplicates, loadDataHash));

        }                   // should work fine

        private ReadDataReturn read_data_FritzXML(string filename) 
        {
            System.Collections.Hashtable loadDataHash = new System.Collections.Hashtable();
            string duplicates = "";

            GroupDataContact myContact = new GroupDataContact();
            System.IO.StreamReader file1 = new System.IO.StreamReader(filename, Encoding.GetEncoding("ISO-8859-1"));

            try
            {

                System.Xml.XmlReaderSettings xml_settings = new System.Xml.XmlReaderSettings();
                xml_settings.ConformanceLevel = System.Xml.ConformanceLevel.Fragment;
                xml_settings.IgnoreWhitespace = true;
                xml_settings.IgnoreComments = true;
                System.Xml.XmlReader r = System.Xml.XmlReader.Create(file1, xml_settings);

                r.MoveToContent();

                while (r.ReadToFollowing("contact"))                // loop starts here, if we are able to arrive at a new contact
                {
                    myContact = new GroupDataContact();                 // then we first clean out myContact Storage
                    string myContactVIPsettings = "";

                    // and then proceed to retrieve the name
                    r.ReadToFollowing("category");                           // we arrive at category enclosure
                    if (r.ReadElementContentAsString() == "1")
                    {
                        myContactVIPsettings = "VIP ";
                    }
                    // if (r.ReadElementContentAsString() == 1) { myContactVIPsettings = "VIP "; } // check if in VIP category
                    r.ReadToFollowing("realName");                          // we have already arrived at person enclosure due to category, so we proceed to the realname tag
                    myContact.lastname = r.ReadElementContentAsString();    // we read the person enclosure's contents
                    myContact.combinedname = myContact.lastname; // and also save them to the combined name field, because thats the only one we have
                    r.ReadToFollowing("telephony");                         // we go to the phone number section
                    r.ReadToFollowing("number");                            // retrieve the first number

                    // and then retrieve all Elements in the number part
                    while (r.NodeType == System.Xml.XmlNodeType.Element)
                    {
                        string quickdial_item = r.GetAttribute("quickdial");
                        if (quickdial_item != null)
                        {
                            myContactVIPsettings += "SPEEDDIAL:" + quickdial_item;
                        }

                        if (r.GetAttribute("type") == "home")
                        {
                            if (r.GetAttribute("prio") == "1") { myContact.preferred = "home"; }
                            myContact.home = r.ReadElementContentAsString();
                            continue;
                        }
                        if (r.GetAttribute("type") == "work")
                        {
                            if (r.GetAttribute("prio") == "1") { myContact.preferred = "work"; }
                            myContact.work = r.ReadElementContentAsString();
                            continue;
                        }
                        if (r.GetAttribute("type") == "mobile")
                        {
                            if (r.GetAttribute("prio") == "1") { myContact.preferred = "mobile"; }
                            myContact.mobile = r.ReadElementContentAsString();
                            continue;
                        }
                    }

                    myContact.VIP = CheckVIPandSPEEDDIALflags("", myContactVIPsettings, true);

                    // Now all information should be stored in the array, so save it to the hashtable!
                    try
                    {
                        loadDataHash.Add(myContact.lastname, myContact);
                    }
                    catch (ArgumentException)
                    { duplicates += "Duplicate entry in source (Fritz!Box XML file): " + myContact.combinedname + Environment.NewLine; }

                }

            }
            catch (System.Xml.XmlException e)
            {
                Console.WriteLine("error occured: " + e.Message);
            }

            file1.Close();
            return (new ReadDataReturn(duplicates, loadDataHash));
        }                   // should work fine

        private ReadDataReturn read_data_vCard(string filename, bool non_unicode)
        {
            // read all information from the vCard file and save all contacts that have at least one phone or fax number
            System.Collections.Hashtable loadDataHash = new System.Collections.Hashtable();

            string duplicates = "";

            try
            {
                // do whats necessary to import a VCF File!
                // for further use: spec can be found here: http://www.ietf.org/rfc/rfc2426.txt

                GroupDataContact myContact = new GroupDataContact();
                string vcard_fullname = "";
                string vcard_notes = "";
                string vcard_nickname = "";

                int address_value_stored = 0;
                int email_value_stored = 0;

                System.IO.StreamReader file1;
                if (non_unicode == false)
                {
                    file1 = new System.IO.StreamReader(filename, Encoding.UTF8);
                }
                else
                {
                    file1 = new System.IO.StreamReader(filename, Encoding.GetEncoding("ISO-8859-1"));
                }

                // first read everything into builder "string"
                string curline;
                StringBuilder builder = new StringBuilder();
                while ((curline = file1.ReadLine()) != null)
                {
                    builder.Append(curline + "\r\n");
                }
                file1.Close();

                // then strip away "\r\n " stuff to unfold everything, and then regEx Split by "\r\n" into array of lines
                string[] vParseLines = System.Text.RegularExpressions.Regex.Split(builder.ToString().Replace("\r\n ", ""), "\r\n");

                foreach (string ParseLine in vParseLines)
                {
                    // replaced escaped ":" characters, they should not be a problem when parsing
                    string vParseLine = ParseLine.Replace("\\:", ":");

                    // if line starts with item1, remove this to allow normal processing (apple used this, item2 and up are therefore silently ignored)
                    if (vParseLine.StartsWith("item1.", StringComparison.OrdinalIgnoreCase) == true)
                    { vParseLine = vParseLine.Substring("item1.".Length); }

                    if (vParseLine.StartsWith("BEGIN:VCARD", StringComparison.OrdinalIgnoreCase) == true)
                    { // reset global settings for contact
                        myContact = new GroupDataContact();
                        vcard_fullname = "";
                        vcard_notes = "";
                        vcard_nickname = "";
                        address_value_stored = 0;
                        email_value_stored = 0;
                        continue;
                    }
                    if (vParseLine.StartsWith("VERSION", StringComparison.OrdinalIgnoreCase) == true) { continue; }

                    if (vParseLine.StartsWith("NOTE:", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        // holt den rest der Zeile und speichert es in der vcard_nickname zwischen um am ende verarbeitet zu werden
                        vcard_notes = vParseLine.Substring(5).Trim();
                        continue;
                    }

                    if (vParseLine.StartsWith("N:", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        // this is firstname and lastname, extract those and ignore other stuff in this line
                        myContact.lastname = vParseLine.Substring(2, vParseLine.IndexOf(";") - 2).Trim();
                        myContact.firstname = vParseLine.Substring(vParseLine.IndexOf(";") + 1).Trim();
                        if (myContact.firstname.Contains(";") == true) // this if is neccessary because windows live mail exports without trailing ; chars for unused fields
                        { myContact.firstname = myContact.firstname.Substring(0, myContact.firstname.IndexOf(";")).Trim(); }
                        continue;
                    }

                    if (vParseLine.StartsWith("FN:", StringComparison.OrdinalIgnoreCase) == true)
                    {   // this is last name and first name, save to special storage string
                        vcard_fullname = vParseLine.Substring(3).Trim(); // holt den ganzen namen
                        continue;
                    }

                    if (vParseLine.StartsWith("NICKNAME:", StringComparison.OrdinalIgnoreCase) == true)
                    {   // holt den ganzen nickname aus der zeile und speichert in für verarbeitung am ende
                        vcard_nickname = vParseLine.Substring(9).Trim(); 
                        continue;
                    }

                    if (vParseLine.StartsWith("ORG:", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        myContact.company = vParseLine.Substring(4).TrimEnd(';'); // if ends with ; (like on macos) remove trailing ;
                        myContact.company = myContact.company.Replace(';', ' ').Trim(); // if consists of multiple business subunits, combine in one field by removing separators
                        continue;
                    }

                    #region Process-TEL-Lines-in-vCard
                    if (vParseLine.StartsWith("TEL;", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        // we are about to process a phone or fax number

                        // trim stuff away that we've already recognized
                        vParseLine = vParseLine.Substring("TEL;".Length);
                        string telnumber = vParseLine.Substring(vParseLine.IndexOf(":") + 1);

                        string types = vParseLine.Substring(0, vParseLine.IndexOf(":"));
                        types = types.ToLower().Replace("type=", "");
                        string[] typearray = types.Split(';');

                        bool bit_preferred = false;
                        bool bit_fax = false;
                        string bit_type = "";

                        foreach (string type in typearray)
                        {
                            switch (type)
                            {
                                case "home":
                                    bit_type = "home";
                                    break;
                                case "work":
                                    bit_type = "work";
                                    break;
                                case "cell":
                                    bit_type = "mobile";
                                    break;
                                case "fax":
                                    bit_fax = true;
                                    break;
                                case "pref":
                                    bit_preferred = true;
                                    break;
                                default:
                                    // unknown type, just ignore
                                    break;
                            }
                        }

                        if (bit_type == "home" && bit_fax == false) // handle home phone numbers
                        {
                            if (bit_preferred == true)
                            { myContact.home = telnumber; myContact.preferred = bit_type; }
                            else
                            { if (myContact.home == "") { myContact.home = telnumber; } }
                            continue;
                        }


                        if (bit_type == "work" && bit_fax == false) // handle work phone numbers
                        {
                            if (bit_preferred == true)
                            { myContact.work = telnumber; myContact.preferred = bit_type; }
                            else
                            { if (myContact.work == "") { myContact.work = telnumber; } }
                            continue;
                        }

                        if (bit_type == "mobile") // handle mobile phone numbers
                        {
                            if (bit_preferred == true)
                            { myContact.mobile = telnumber; myContact.preferred = bit_type; }
                            else
                            { if (myContact.mobile == "") { myContact.mobile = telnumber; } }
                            continue;
                        }


                        if (bit_type == "home" && bit_fax == true) // handle work phone numbers
                        {
                            if (bit_preferred == true || myContact.homefax == "")
                            { myContact.homefax = telnumber; }
                            continue;
                        }

                        if (bit_type == "work" && bit_fax == true) // handle work phone numbers
                        {
                            if (bit_preferred == true || myContact.workfax == "")
                            { myContact.workfax = telnumber; }
                            continue;
                        }

                    }
                    #endregion

                    #region Process-ADR-Lines-in-vCard
                    if (vParseLine.StartsWith("ADR;", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        // we are about to process a home or work address

                        // trim stuff away that we've already recognized
                        vParseLine = vParseLine.Substring("ADR;".Length);
                        string address = vParseLine.Substring(vParseLine.IndexOf(":") + 1);

                        string types = vParseLine.Substring(0, vParseLine.IndexOf(":"));
                        types = types.ToLower().Replace("type=", "");
                        string[] typearray = types.Split(';');

                        bool bit_preferred = false;
                        string bit_type = "";

                        foreach (string type in typearray)
                        {
                            switch (type)
                            {
                                case "home":
                                    bit_type = "home";
                                    break;
                                case "work":
                                    bit_type = "work";
                                    break;
                                case "pref":
                                    bit_preferred = true;
                                    break;
                                default:
                                    // unknown type, just ignore
                                    break;
                            }
                        }


                        if (bit_type == "home" || bit_type == "work")
                        {
                            // calculate address value:
                            int address_value = 0;
                            if (combo_typeprefer.SelectedIndex == 0) { if (bit_type == "home") { address_value += 1; } }     // 1 pt. for home address, 0 pt. for work
                            if (combo_typeprefer.SelectedIndex == 1) { if (bit_type == "work") { address_value += 1; } }     // 0 pt. for home address, 1 pt. for work
                            if (bit_preferred == true) { address_value += 2; }      // bonus of 2 for prefferred one

                            vCardAddressParser myParseVC = new vCardAddressParser(address);
                            if ((address_value > address_value_stored) || ((myContact.street == "" && myContact.zip == "" && myContact.city == "") == true))
                            {
                                myContact.street = myParseVC.parsed_street;
                                myContact.zip = myParseVC.parsed_zip;
                                myContact.city = myParseVC.parsed_city;

                                // save what kind of address we have store, so that it can be overwritten in case a better one comes along
                                address_value_stored = address_value;
                            }
                            continue;
                        }
                    }
                    #endregion

                    #region Process-EMAIL-Lines-in-vCard
                    if (vParseLine.StartsWith("EMAIL;", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        // we are about to process a home or work email address

                        // trim stuff away that we've already recognized
                        vParseLine = vParseLine.Substring("EMAIL;".Length);
                        string emailaddress = vParseLine.Substring(vParseLine.IndexOf(":") + 1);

                        string types = vParseLine.Substring(0, vParseLine.IndexOf(":"));
                        types = types.ToLower().Replace("type=", "");
                        string[] typearray = types.Split(';');

                        bool bit_preferred = false;
                        string bit_type = "";

                        foreach (string type in typearray)
                        {
                            switch (type)
                            {
                                case "home":
                                    bit_type = "home";
                                    break;
                                case "work":
                                    bit_type = "work";
                                    break;
                                case "pref":
                                    bit_preferred = true;
                                    break;
                                default:
                                    // unknown type, just ignore
                                    break;
                            }
                        }

                        // calculate email address value:
                        int email_value = 0;
                        if (combo_typeprefer.SelectedIndex == 0) { if (bit_type == "home") { email_value += 1; } }      // 1 pt. for home address, 0 pt. for work
                        if (combo_typeprefer.SelectedIndex == 1) { if (bit_type == "work") { email_value += 1; } }      // 0 pt. for home address, 1 pt. for work
                        if (bit_preferred == true) { email_value += 2; }      // bonus of 2 for prefferred one

                        if ((email_value > email_value_stored) || (myContact.email == ""))  // if we have an email of higher value, or no email so far
                        {
                            myContact.email = emailaddress;

                            // save what kind of address we have store, so that it can be overwritten in case a better one comes along
                            email_value_stored = email_value;
                        }
                        continue;
                    }
                    #endregion

                    if (vParseLine.StartsWith("CATEGORIES", StringComparison.OrdinalIgnoreCase) == true) { continue; }
                    if (vParseLine.StartsWith("X-ABUID", StringComparison.OrdinalIgnoreCase) == true) { continue; }

                    if (vParseLine.StartsWith("PHOTO", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        if (vParseLine.StartsWith("PHOTO;BASE64:"))
                        {
                            string encodedData = vParseLine.Substring("PHOTO;BASE64:".Length).Replace(" ", "");
                            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
                            myContact.jpeg = encodedDataAsBytes;
                        }
                        continue;
                    }
                    
                    if (vParseLine.StartsWith("END:VCARD", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        // übergibt notes und nickname der methode die VIP und Speeddial extrahiert
                        myContact.VIP = CheckVIPandSPEEDDIALflags(vcard_nickname, vcard_notes, false);

                        // if vcard_fullname is identical to companyname, use only company name and do not use N: line (which contains duplicate information)
                        if (myContact.company == vcard_fullname) { myContact.firstname = ""; myContact.lastname = ""; }

                        // generate fullname from parts for hash-ident
                        myContact.combinedname = GenerateFullName(myContact.firstname, myContact.lastname, myContact.company, combo_namestyle.SelectedIndex);


                        // now, if a full name is present, and any of the phone or fax numbers is set, and no CCW-IGNORE is in the comments we proceed trying to add the user
                        if (myContact.combinedname != "" && (myContact.home != string.Empty || myContact.work != string.Empty || myContact.mobile != string.Empty || myContact.homefax != string.Empty || myContact.workfax != string.Empty) && (vcard_notes.Contains("CCW-IGNORE") == false))
                        {
                            try
                            {
                                loadDataHash.Add(myContact.combinedname, myContact);
                            }
                            catch (ArgumentException)
                            { duplicates += "Duplicate entry in source (vCard file): " + myContact.combinedname + Environment.NewLine; }
                        }

                    }
                }

            }
            catch (Exception vcard_exception)
            {
                MessageBox.Show("Unable to parse given vCard file!" + Environment.NewLine + "Contact Conversion Wizard has been tested with vCard files generated by MacOS X 10.6 \"Address book\" and Google Mail vCard exports" + Environment.NewLine + "If you think this file should have been imported properly, please report a bug!" + Environment.NewLine + Environment.NewLine + "Error returned was: " + vcard_exception.ToString() + Environment.NewLine);
            }

            return (new ReadDataReturn(duplicates, loadDataHash));
        }     // should work fine

        private ReadDataReturn read_data_FritzAdr(string filename) 
        {
            System.Collections.Hashtable loadDataHash = new System.Collections.Hashtable();
            string duplicates = "";

            GroupDataContact myContact = new GroupDataContact();
            System.IO.StreamReader file1 = new System.IO.StreamReader(filename, Encoding.GetEncoding("ISO-8859-1"));
            string line;
            int linecounter = -1;

            try
            {
                while ((line = file1.ReadLine()) != null)
                // loop starts here, if we are able to arrive at a new contact
                {
                    linecounter++;
                    myContact = new GroupDataContact();                 // then we first clean out myContact Storage
                    string[] cDataArray = line.Split('\t');
                    if (cDataArray.Length != 21)
                    {
                        MessageBox.Show("Unable to parse line " + linecounter + ", found only " + cDataArray.Length.ToString() + " TAB separated segments instead of 21:" + Environment.NewLine + line);
                        continue;
                    }
                    if (cDataArray[0] == "BEZCHNG") // check if it's the header line, which we of course ignore!
                    { continue; }

                    #region Comments: Data stored in the array
                    // 0: "BEZCHNG                          // STUFF we don't use:
                    // 1: FIRMA                             // 20: HOMEPAGE
                    // 2: NAME                              // 17: NOTIZEN
                    // 3: VORNAME                           // 16: TERMMODE
                    // 19: EMAIL                            // 15: TRANSPROT
                    // 5: STRASSE                           // 14: PASSWORT
                    // 6: PLZ                               // 13: BENUTZER
                    // 7: ORT                               // 12: TERMINAL
                    // 9: TELEFON                           // 11: TRANSFER
                    // 10: TELEFAX                          // 8: KOMMENT (ok we use that now)
                    // 18: MOBILFON                         // 4: ABTEILUNG 
                    #endregion

                    myContact.combinedname = cDataArray[0];
                    myContact.company = cDataArray[1];
                    myContact.lastname = cDataArray[2];
                    myContact.firstname = cDataArray[3];
                    myContact.email = cDataArray[19];
                    myContact.street = cDataArray[5];
                    myContact.zip = cDataArray[6];
                    myContact.city = cDataArray[7];

                    // check if contact is supposed to be VIP or has Speeddial Settings
                    myContact.VIP = CheckVIPandSPEEDDIALflags("", cDataArray[8], false);

                    // depending on setting, import phone and fax number into home or work fields)
                    int wheretostore = combo_typeprefer.SelectedIndex;
                    // unless combinedname ends on (gesch.) or (privat), the override using that!
                    if (myContact.combinedname.EndsWith(" (privat)") == true)
                    { wheretostore = 0; } // store as home 
                    if (myContact.combinedname.EndsWith(" (gesch.)") == true)
                    { wheretostore = 1; } // store as work 

                    switch (wheretostore)
                    {
                        case 0:
                            myContact.home = cDataArray[9];
                            myContact.homefax = cDataArray[10];
                            break;

                        case 1:
                            myContact.work = cDataArray[9];
                            myContact.workfax = cDataArray[10];
                            break;

                        default:
                            MessageBox.Show("Default case in \"switch (combo_typeprefer.SelectedIndex)\", this should not have happened. Please report this bug!");
                            break;
                    }

                    myContact.mobile = cDataArray[18];

                    // Now all information should be stored in the array, so save it to the hashtable!
                    if (myContact.combinedname != "" && (myContact.home != string.Empty || myContact.work != string.Empty || myContact.mobile != string.Empty || myContact.homefax != string.Empty || myContact.workfax != string.Empty) && (cDataArray[8].Contains("CCW-IGNORE") == false))
                    {
                        try
                        { loadDataHash.Add(myContact.combinedname, myContact); }
                        catch (ArgumentException)
                        { duplicates += "Duplicate entry in source (Fritz!Adr Text with Tabstops file): " + myContact.combinedname + Environment.NewLine; }
                    }
                } // end while loop going through the lines of the file
            }
            catch (Exception e)
            { Console.WriteLine("Error occured while parsing file: " + e.Message); }

            file1.Close();
            return (new ReadDataReturn(duplicates, loadDataHash));
        }                   // should work fine

        private ReadDataReturn read_data_SnomXMLv8(string filename) 
        {
            // WORK IN PROGRESS, so far this is the code from import Fritz!XML

            System.Collections.Hashtable loadDataHash = new System.Collections.Hashtable();
            string duplicates = "";

            GroupDataContact myContact = new GroupDataContact();
            System.IO.StreamReader file1 = new System.IO.StreamReader(filename, Encoding.GetEncoding("ISO-8859-1"));

            try
            {

                System.Xml.XmlReaderSettings xml_settings = new System.Xml.XmlReaderSettings();
                xml_settings.ConformanceLevel = System.Xml.ConformanceLevel.Fragment;
                xml_settings.IgnoreWhitespace = true;
                xml_settings.IgnoreComments = true;
                System.Xml.XmlReader r = System.Xml.XmlReader.Create(file1, xml_settings);

                r.MoveToContent();

                while (r.ReadToFollowing("contact"))                // loop starts here, if we are able to arrive at a new contact
                {
                    myContact = new GroupDataContact();                 // then we first clean out myContact Storage

                    // and then proceed to retrieve the name
                    r.ReadToFollowing("person");                            // we arrive at person enclosure
                    r.ReadToFollowing("realName");                          // we arrive at person enclosure
                    myContact.lastname = r.ReadElementContentAsString();    // we read the person enclosure's contents
                    myContact.combinedname = myContact.lastname; // and also save them to the combined name field, because thats the only one we have
                    r.ReadToFollowing("telephony");                         // we go to the phone number section
                    r.ReadToFollowing("number");                            // retrieve the first number

                    // and then retrieve all Elements in the number part
                    while (r.NodeType == System.Xml.XmlNodeType.Element)
                    {
                        if (r.GetAttribute("type") == "home")
                        {
                            if (r.GetAttribute("prio") == "1") { myContact.preferred = "home"; }
                            myContact.home = r.ReadElementContentAsString();
                        }
                        if (r.GetAttribute("type") == "work")
                        {
                            if (r.GetAttribute("prio") == "1") { myContact.preferred = "work"; }
                            myContact.work = r.ReadElementContentAsString();
                        }
                        if (r.GetAttribute("type") == "mobile")
                        {
                            if (r.GetAttribute("prio") == "1") { myContact.preferred = "mobile"; }
                            myContact.mobile = r.ReadElementContentAsString();
                        }
                    }


                    // Now all information should be stored in the array, so save it to the hashtable!
                    try
                    {
                        loadDataHash.Add(myContact.lastname, myContact);
                    }
                    catch (ArgumentException)
                    { duplicates += "Duplicate entry in source (Fritz!Box XML file): " + myContact.combinedname + Environment.NewLine; }

                }

            }
            catch (System.Xml.XmlException e)
            {
                Console.WriteLine("error occured: " + e.Message);
            }

            file1.Close();
            return (new ReadDataReturn(duplicates, loadDataHash));
        }                  // needs full implementing (nothing here so far)

        // Export Functionality

        private void btn_save_Outlook_Click(object sender, EventArgs e)
        {
            diable_buttons(true);

            save_data_Outlook(myGroupDataHash);
            diable_buttons(false);

        }    // just click handler

        private void btn_save_FritzXML_Click(object sender, EventArgs e)
        {
            SaveFileDialog SaveXML_Dialog = new SaveFileDialog();
            SaveXML_Dialog.Title = "Select the XML file you wish to create";
            SaveXML_Dialog.DefaultExt = "xml";
            SaveXML_Dialog.Filter = "XML files (*.xml)|*.xml";
            SaveXML_Dialog.InitialDirectory = Environment.SpecialFolder.Desktop.ToString();
            SaveXML_Dialog.FileName = "FritzExport";

            if (SaveXML_Dialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            diable_buttons(true);

            save_data_FritzXML(SaveXML_Dialog.FileName, myGroupDataHash);

            // and reenable user interface
            diable_buttons(false);
        }   // just click handler

        private void btn_save_vCard_Click(object sender, EventArgs e)
        {
            MessageBox.Show("TODO - Not implemented yet");
        }      // just click handler

        private void btn_save_FritzAdr_Click(object sender, EventArgs e)
        {
            // check whether the user had the shift key pressed while calling this function and store this in a variable for further use
            bool shiftpressed_for_fax_only = false;
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) { shiftpressed_for_fax_only = true; }

            SaveFileDialog SaveTXT_Dialog = new SaveFileDialog();
            SaveTXT_Dialog.Title = "Select the TXT file you wish to create";
            SaveTXT_Dialog.DefaultExt = "txt";
            SaveTXT_Dialog.Filter = "TXT files (*.txt)|*.txt";
            SaveTXT_Dialog.InitialDirectory = Environment.SpecialFolder.Desktop.ToString();
            SaveTXT_Dialog.FileName = "FritzFax Export";

            if (SaveTXT_Dialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            diable_buttons(true);

            save_data_FritzAdress(SaveTXT_Dialog.FileName, myGroupDataHash, shiftpressed_for_fax_only);

            // and reenable user interface
            diable_buttons(false);
        }   // just click handler

        private void btn_save_SnomXMLv7_Click(object sender, EventArgs e)
        {
            SaveFileDialog SaveCSV_Dialog = new SaveFileDialog();
            SaveCSV_Dialog.Title = "Select the CSV file you wish to create";
            SaveCSV_Dialog.DefaultExt = "csv";
            SaveCSV_Dialog.Filter = "CSV files (*.csv)|*.csv";
            SaveCSV_Dialog.InitialDirectory = Environment.SpecialFolder.Desktop.ToString();
            SaveCSV_Dialog.FileName = "tbook_v7";

            if (SaveCSV_Dialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            diable_buttons(true);

            save_data_SnomCSV7(SaveCSV_Dialog.FileName, myGroupDataHash);

            // and reenable user interface
            diable_buttons(false);
        }  // just click handler

        private void btn_save_SnomXMLv8_Click(object sender, EventArgs e)
        {
            SaveFileDialog SaveCSV_Dialog = new SaveFileDialog();
            SaveCSV_Dialog.Title = "Select the CSV file you wish to create";
            SaveCSV_Dialog.DefaultExt = "csv";
            SaveCSV_Dialog.Filter = "CSV files (*.csv)|*.csv";
            SaveCSV_Dialog.InitialDirectory = Environment.SpecialFolder.Desktop.ToString();
            SaveCSV_Dialog.FileName = "tbook_v8";

            if (SaveCSV_Dialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            diable_buttons(true);

            save_data_SnomCSV8(SaveCSV_Dialog.FileName, myGroupDataHash);

            // and reenable user interface
            diable_buttons(false);
        }  // just click handler
       
        private void save_data_Outlook(System.Collections.Hashtable workDataHash)
        {
            // get the country ID from the combobox or from user input
            string country_id = RetrieveCountryID(combo_prefix.SelectedItem.ToString());

            // some basic initialization of the outlook support
            Microsoft.Office.Interop.Outlook.Application outlookObj = null;
            Microsoft.Office.Interop.Outlook.MAPIFolder outlookFolder = null;

            // connect to outlook, if possible. Else, complain and abort.
            try { outlookObj = new Microsoft.Office.Interop.Outlook.Application(); }
            catch (Exception outlook_exception) { MessageBox.Show("Unable to access Outlook!" + Environment.NewLine + Environment.NewLine + "This program needs Outlook to continue, are you sure it's installed and working?" + Environment.NewLine + Environment.NewLine + "Error returned was: " + outlook_exception.ToString() + Environment.NewLine); }

            // sucessfully connected to outlook, do some further setup work
            Microsoft.Office.Interop.Outlook.NameSpace outlookNS = outlookObj.GetNamespace("MAPI");

            // Allow the user to chose a custom destination folder
            outlookFolder = outlookNS.PickFolder();
            if (outlookFolder == null)
            { return; } // if no folder has been selected, user must have pressed cancel and therefore we abort

            // iterate through the contacts in workDatahash and save them to the selected Outlook Folder
            int count = 0;
            foreach (System.Collections.DictionaryEntry contactHash in workDataHash)
            {
                // extract GroupDataList from hashtable contents
                GroupDataContact contactData = (GroupDataContact)contactHash.Value;

                // clean up phone number
                contactData.home = CleanUpNumber(contactData.home, country_id);
                contactData.work = CleanUpNumber(contactData.work, country_id);
                contactData.homefax = CleanUpNumber(contactData.homefax, country_id);
                contactData.workfax = CleanUpNumber(contactData.workfax, country_id);
                contactData.mobile = CleanUpNumber(contactData.mobile, country_id);

                // create new contact
                Microsoft.Office.Interop.Outlook.ContactItem newContact = (Microsoft.Office.Interop.Outlook.ContactItem)outlookFolder.Items.Add(Microsoft.Office.Interop.Outlook.OlItemType.olContactItem);

                newContact.FirstName = contactData.firstname;
                newContact.LastName = contactData.lastname;
                newContact.Email1Address = contactData.email;
                newContact.MailingAddressCity = contactData.city;
                newContact.MailingAddressStreet = contactData.street;
                newContact.MailingAddressPostalCode = contactData.zip;
                newContact.HomeTelephoneNumber = contactData.home;
                newContact.HomeFaxNumber = contactData.homefax;
                newContact.BusinessTelephoneNumber = contactData.work;
                newContact.BusinessFaxNumber = contactData.workfax;
                newContact.FileAs = contactData.combinedname;
                newContact.CompanyName = contactData.company;
                newContact.MobileTelephoneNumber = contactData.mobile;

                // VIP Functionality (fully implemented now)
                if (contactData.VIP.StartsWith("Yes")) { newContact.Body += "VIP" + Environment.NewLine; }

                // Speeddial Functionality (fully implemented now)
                if (contactData.VIP.IndexOf(":") != -1)
                { // then we have a quickdial number then add it to the prio entry
                    int qd_number = Convert.ToInt32(contactData.VIP.Substring(contactData.VIP.IndexOf(":") + 1));
                    newContact.Body += "SPEEDDIAL:" + qd_number.ToString("00");
                }

                // Contact Picture Functionality, more info on this here: http://www.c-sharpcorner.com/UploadFile/Nimusoft/OutlookwithNET06262007081811AM/OutlookwithNET.aspx
                if (combo_picexport.SelectedIndex == 1 && contactData.jpeg != null)
                {
                    string tempname = System.IO.Path.GetTempFileName();
                    System.IO.File.WriteAllBytes(tempname, contactData.jpeg);
                    newContact.AddPicture(tempname);
                    System.IO.File.Delete(tempname);
                }

                newContact.Save();
                count++;

            } // end of foreach loop for the contacts

            // tell the user what has been done
            MessageBox.Show(count + " contacts have been written to the selected Outlook folder!" + Environment.NewLine);


        }

        private void save_data_FritzXML(string filename, System.Collections.Hashtable workDataHash)
        {
            // get the country ID from the combobox or from user input
            string country_id = RetrieveCountryID(combo_prefix.SelectedItem.ToString());

            // create output path for fonpix, if necessary
            string pic_export_path = "";
            if (combo_picexport.SelectedIndex == 1)
            {
                pic_export_path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filename), System.IO.Path.GetFileNameWithoutExtension(filename) + " - fonpix");
                if (!System.IO.Directory.Exists(pic_export_path)) { System.IO.Directory.CreateDirectory(pic_export_path); }
            }

            // process with exporting
            string resultstring;

            // write the header
            resultstring = "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\n<phonebooks>\n<phonebook>";

            // initialize hashtable to store generated data results
            System.Collections.Hashtable MySaveDataHash = new System.Collections.Hashtable();

            foreach (System.Collections.DictionaryEntry contactHash in workDataHash)
            {
                string contactstring = "";

                // extract GroupDataList from hashtable contents
                GroupDataContact contactData = (GroupDataContact)contactHash.Value;

                // check if all relevant phone numbers for this export here are empty
                if (contactData.home == string.Empty && contactData.work == string.Empty && contactData.mobile == string.Empty)
                { continue; /* if yes, abort this foreach loop and contine to the next one */ }

                string SaveAsName = contactData.combinedname;

                // limit to 32 chars
                SaveAsName = LimitNameLength(SaveAsName, 32);
                // replace "&"
                SaveAsName = SaveAsName.Replace("&", "&amp;");
                // clean up phone number
                contactData.home = CleanUpNumber(contactData.home, country_id);
                contactData.work = CleanUpNumber(contactData.work, country_id);
                contactData.mobile = CleanUpNumber(contactData.mobile, country_id);
                // write contact header

                // generate VIP ID - if VIP then assign category 1, else 0
                string VIPid = "0";
                if (contactData.VIP.StartsWith("Yes")) { VIPid = "1"; }


                // if picture export is set to yes and picture is stored, then export it to a jpeg file and add imageURL to export XML
                string imageURLstring = "";
                if (combo_picexport.SelectedIndex == 1 && contactData.jpeg != null)
                {
                    string picfile = System.IO.Path.ChangeExtension(System.IO.Path.GetRandomFileName(), "jpg");
                    writeByteArrayToFile(contactData.jpeg, System.IO.Path.Combine(pic_export_path, picfile));
                    imageURLstring = "<imageURL>" + textBox_PicPath.Text + picfile + "</imageURL>\n";
                }

                contactstring += "<contact>\n<category>" + VIPid + "</category>\n<person>\n<realName>" + SaveAsName + "</realName>\n" + imageURLstring + "</person>\n<telephony>\n";

                // prepare quickdial and preferred number setting:
                string pref_QD_string = "prio=\"1\"";
                if (contactData.VIP.IndexOf(":") != -1)
                { // then we have a quickdial number then add it to the prio entry
                    int qd_number = Convert.ToInt32(contactData.VIP.Substring(contactData.VIP.IndexOf(":")+1));
                    pref_QD_string += " quickdial=\"" + qd_number.ToString("00") +"\"";
                }


                // add home phone number
                if (contactData.home != "")
                {
                    if (contactData.preferred == "home")
                    { contactstring += "<number type=\"home\" " + pref_QD_string + ">" + contactData.home + "</number>\n"; }
                    else
                    { contactstring += "<number type=\"home\">" + contactData.home + "</number>\n"; }

                }
                else { contactstring += "<number type=\"home\" />\n"; }

                // add work phone number
                if (contactData.work != "")
                {
                    if (contactData.preferred == "work")
                    { contactstring += "<number type=\"work\" " + pref_QD_string + ">" + contactData.work + "</number>\n"; }
                    else
                    { contactstring += "<number type=\"work\">" + contactData.work + "</number>\n"; }
                }
                else { contactstring += "<number type=\"work\" />\n"; }

                // add mobile phone number
                if (contactData.mobile != "")
                {
                    if (contactData.preferred == "mobile")
                    { contactstring += "<number type=\"mobile\" " + pref_QD_string + ">" + contactData.mobile + "</number>\n"; }
                    else
                    { contactstring += "<number type=\"mobile\">" + contactData.mobile + "</number>\n"; }
                }
                else { contactstring += "<number type=\"mobile\" />\n"; }

                // write contact footer
                contactstring += "</telephony>\n<services>\n<email />\n</services>\n<setup>\n<ringTone />\n<ringVolume />\n</setup>\n</contact>";

                try
                {
                    MySaveDataHash.Add(SaveAsName, contactstring);
                }
                catch (ArgumentException) // unable to add to MySaveDataHash, must mean that something with saveasname is already in there!
                {
                    MessageBox.Show("Unable to export the entry \"" + SaveAsName + "\", another entry with this name already exists! Ignoring duplicate.");
                }

            } // end of foreach loop for the contacts

            foreach (System.Collections.DictionaryEntry saveDataHash in MySaveDataHash)
            { resultstring += (string)saveDataHash.Value; }

            // write file footer with AVM HD Music test stuff, currently not added. Not sure if thats a good idea to add this always.
            // resultstring += "<contact><category /><person><realName>~AVM-HD-Musik</realName></person><telephony><number\nprio=\"1\" type=\"home\" quickdial=\"98\">200@hd-telefonie.avm.de</number></telephony><services /><setup /></contact><contact><category /><person><realName>~AVM-HD-Sprache</realName></person><telephony><number\nprio=\"1\" type=\"home\" quickdial=\"99\">100@hd-telefonie.avm.de</number></telephony><services /><setup /></contact>";
            resultstring += "</phonebook>\n</phonebooks>";

            // actually write the file to disk
            System.IO.File.WriteAllText(filename, resultstring, Encoding.GetEncoding("ISO-8859-1"));

            // tell the user this has been done
            string errorwarning = "";
            if (MySaveDataHash.Count > 300) { errorwarning = Environment.NewLine + "Warning: Over 300 contacts have been exported! This might or might not be officially supported by AVM's Fritz!Box and may cause problems. Proceed with care!"; }
            MessageBox.Show(MySaveDataHash.Count + " contacts written to " + filename + " !" + Environment.NewLine + errorwarning);
            
        }                               // this should work really well, after all this is the main purpose of the wizard

        // vCard Export not implemented here yet

        private void save_data_FritzAdress(string filename, System.Collections.Hashtable workDataHash, bool only_export_fax)
        {

            // get the country ID from the combobox or from user input
            string country_id = RetrieveCountryID(combo_prefix.SelectedItem.ToString());

            // process with exporting
            string resultstring;

            // write the header
            resultstring = "BEZCHNG\tFIRMA\tNAME\tVORNAME\tABTEILUNG\tSTRASSE\tPLZ\tORT\tKOMMENT\tTELEFON\tTELEFAX\tTRANSFER\tTERMINAL\tBENUTZER\tPASSWORT\tTRANSPROT\tTERMMODE\tNOTIZEN\tMOBILFON\tEMAIL\tHOMEPAGE\r\n";

            System.Collections.Hashtable MySaveDataHash = new System.Collections.Hashtable();

            foreach (System.Collections.DictionaryEntry contactHash in workDataHash)
            {
                // extract GroupDataList from hashtable contents
                GroupDataContact contactData = (GroupDataContact)contactHash.Value;

                // check if both relevant fax fields for this export are empty - AND we only wish to export those who have faxnumbers
                if (contactData.homefax == string.Empty && contactData.workfax == string.Empty && (only_export_fax == true))
                {
                    // if yes, abort this foreach loop and contine to the next one
                    continue;
                }

                // clean up phone numbers
                contactData.homefax = CleanUpNumber(contactData.homefax, country_id);
                contactData.workfax = CleanUpNumber(contactData.workfax, country_id);
                contactData.home = CleanUpNumber(contactData.home, country_id);
                contactData.work = CleanUpNumber(contactData.work, country_id);
                contactData.mobile = CleanUpNumber(contactData.mobile, country_id);

                string[,] save_iterate_array;

                
                if ((contactData.homefax != "" && contactData.workfax != "") || (contactData.home != "" && contactData.work != ""))
                { // we need to save two separate entries since there are there are either seperate phone numbers or fax numbers for work and home (or both)
                    save_iterate_array = new string[2, 4];
                    save_iterate_array[0, 0] = LimitNameLength(contactData.combinedname, 22) + " (privat)";
                    save_iterate_array[0, 1] = contactData.homefax;
                    save_iterate_array[0, 2] = contactData.home;
                    save_iterate_array[1, 0] = LimitNameLength(contactData.combinedname, 22) + " (gesch.)";
                    save_iterate_array[1, 1] = contactData.workfax;
                    save_iterate_array[1, 2] = contactData.work;
                }
                else
                { // we need to save only 1 entry
                    save_iterate_array = new string[1, 4];
                    save_iterate_array[0, 0] = LimitNameLength(contactData.combinedname, 31);
                    if (contactData.homefax != "") { save_iterate_array[0, 1] = contactData.homefax; }
                    if (contactData.workfax != "") { save_iterate_array[0, 1] = contactData.workfax; }
                    if (contactData.home != "") { save_iterate_array[0, 2] = contactData.home; }
                    if (contactData.work != "") { save_iterate_array[0, 2] = contactData.work; }
                }

                // we will now save VIP and speeddial information to the first of those two entries
                if (contactData.VIP == "Yes")
                {
                    save_iterate_array[0, 3] = "VIP";
                }
                if (contactData.VIP.StartsWith("Yes:") == true)
                {
                    save_iterate_array[0, 3] = "VIP,";
                    save_iterate_array[0, 3] += "SPEEDDIAL:" + contactData.VIP.Substring(4);
                }
                if (contactData.VIP.StartsWith("No:") == true)
                {
                    save_iterate_array[0, 3] = "SPEEDDIAL:" + contactData.VIP.Substring(3);
                }

                // write contact line, maybe twice
                for (int i = 0; i < save_iterate_array.GetLength(0); i++)
                {

                    try
                    {
                        MySaveDataHash.Add(save_iterate_array[i, 0], save_iterate_array[i, 0] + "\t" + contactData.company + "\t" + contactData.lastname + "\t" + contactData.firstname + "\t" + string.Empty + "\t" + contactData.street + "\t" + contactData.zip + "\t" + contactData.city + "\t" + "\t" + save_iterate_array[i, 2] + "\t" + save_iterate_array[i, 1] + "\t" + "\t" + "\t" + "\t" + "\t" + "A" + "\t" + "\t" + save_iterate_array[i, 3] + "\t" + contactData.mobile + "\t" + contactData.email + "\t" + "\r\n");
                    }
                    catch (ArgumentException) // unable to add to MySaveFaxDataHash, must mean that something with saveasname is already in there!
                    {
                        MessageBox.Show("Unable to export the entry \"" + save_iterate_array[i, 0] + "\", another entry with this name already exists! Ignoring duplicate.");
                    }


                }
            } // end of foreach loop for the contacts

            // retrieve stuff from hastable and put in resultstring:
            foreach (System.Collections.DictionaryEntry saveDataHash in MySaveDataHash)
            {
                resultstring += (string)saveDataHash.Value;
            }

            // actually write the file to disk
            System.IO.File.WriteAllText(filename, resultstring, Encoding.GetEncoding("ISO-8859-1"));

            // tell the user this has been done
            MessageBox.Show("Contacts written to " + filename + " !" + Environment.NewLine);
        }      // now fully implemented with all fields and functions for v2.0

        private void save_data_SnomCSV7(string filename, System.Collections.Hashtable workDataHash)
        {
            // get the country ID from the combobox or from user input
            string country_id = RetrieveCountryID(combo_prefix.SelectedItem.ToString());

            // process with exporting
            string resultstring;

            // write the header (none needed for snom)
            resultstring = "\"Name\",\"Number\"\r\n";

            System.Collections.Hashtable MySaveDataHash = new System.Collections.Hashtable();

            foreach (System.Collections.DictionaryEntry contactHash in workDataHash)
            {
                // extract GroupDataList from hashtable contents
                GroupDataContact contactData = (GroupDataContact)contactHash.Value;

                //  check if all relevant phone numbers for this export here are empty
                if (contactData.home == string.Empty && contactData.work == string.Empty && contactData.mobile == string.Empty)
                { continue; /* if yes, abort this foreach loop and contine to the next one */ }

                // clean up phone number
                contactData.home = CleanUpNumber(contactData.home, country_id);
                contactData.work = CleanUpNumber(contactData.work, country_id);
                contactData.mobile = CleanUpNumber(contactData.mobile, country_id);


                // add privat/gesch. to name if necessary (if two entries for one name necessary)
                string name_home = contactData.combinedname;
                string name_work = contactData.combinedname;
                string name_mobile = contactData.combinedname;
                int nr_in_use = 0;
                if (contactData.home != "") { nr_in_use++; }
                if (contactData.work != "") { nr_in_use++; }
                if (contactData.mobile != "") { nr_in_use++; }

                if (nr_in_use > 1)
                {
                    // limit to 26 chars
                    name_home = LimitNameLength(name_home, 26);
                    name_work = LimitNameLength(name_work, 26);
                    name_mobile = LimitNameLength(name_mobile, 26);

                    // then add 5 additional chars
                    name_home += " home";
                    name_work += " work";
                    name_mobile += " mobile";
                }
                else
                {
                    // limit to 31 chars
                    name_home = LimitNameLength(name_home, 31);
                    name_work = LimitNameLength(name_work, 31);
                    name_mobile = LimitNameLength(name_mobile, 31);
                }

                // write contact line, maybe twice
                if (contactData.home != "")
                {
                    try
                    {
                        MySaveDataHash.Add(name_home, "\"" + name_home + "\",\"" + contactData.home + "\"\r\n");
                    }
                    catch (ArgumentException) // unable to add to MySaveFaxDataHash, must mean that something with (modified) contactData.combinedname is already in there!
                    {
                        MessageBox.Show("Unable to export the entry \"" + name_home + "\", another entry with this name already exists! Ignoring duplicate.");
                    }
                }

                if (contactData.work != "")
                {
                    try
                    {
                        MySaveDataHash.Add(name_work, "\"" + name_work + "\",\"" + contactData.work + "\"\r\n");
                    }
                    catch (ArgumentException) // unable to add to MySaveFaxDataHash, must mean that something with (modified) contactData.combinedname is already in there!
                    {
                        MessageBox.Show("Unable to export the entry \"" + name_work + "\", another entry with this name already exists! Ignoring duplicate.");
                    }
                }

                if (contactData.mobile != "")
                {
                    try
                    {
                        MySaveDataHash.Add(name_mobile, "\"" + name_mobile + "\",\"" + contactData.mobile + "\"\r\n");
                    }
                    catch (ArgumentException) // unable to add to MySaveFaxDataHash, must mean that something with (modified) contactData.combinedname is already in there!
                    {
                        MessageBox.Show("Unable to export the entry \"" + name_mobile + "\", another entry with this name already exists! Ignoring duplicate.");
                    }
                }

            } // end of foreach loop for the contacts

            // retrieve stuff from hastable and put in resultstring:
            foreach (System.Collections.DictionaryEntry saveDataHash in MySaveDataHash)
            {
                resultstring += (string)saveDataHash.Value;
            }

            // actually write the file to disk
            System.IO.File.WriteAllText(filename, resultstring, Encoding.UTF8);

            // tell the user this has been done
            MessageBox.Show("Contacts written to " + filename + " !" + Environment.NewLine);
        }                               // should work fine, only exports name and number

        private void save_data_SnomCSV8(string filename, System.Collections.Hashtable workDataHash)
        {
            // get the country ID from the combobox or from user input
            string country_id = RetrieveCountryID(combo_prefix.SelectedItem.ToString());

            // process with exporting
            string resultstring;

            // write the header (none needed for snom)
            resultstring = "\"Name\",\"Number\"\r\n";

            System.Collections.Hashtable MySaveDataHash = new System.Collections.Hashtable();

            foreach (System.Collections.DictionaryEntry contactHash in workDataHash)
            {
                // extract GroupDataList from hashtable contents
                GroupDataContact contactData = (GroupDataContact)contactHash.Value;

                //  check if all relevant phone numbers for this export here are empty
                if (contactData.home == string.Empty && contactData.work == string.Empty && contactData.mobile == string.Empty)
                { continue; /* if yes, abort this foreach loop and contine to the next one */ }

                // clean up phone number
                contactData.home = CleanUpNumber(contactData.home, country_id);
                contactData.work = CleanUpNumber(contactData.work, country_id);
                contactData.mobile = CleanUpNumber(contactData.mobile, country_id);


                // add privat/gesch. to name if necessary (if two entries for one name necessary)
                string name_home = contactData.combinedname;
                string name_work = contactData.combinedname;
                string name_mobile = contactData.combinedname;
                int nr_in_use = 0;
                if (contactData.home != "") { nr_in_use++; }
                if (contactData.work != "") { nr_in_use++; }
                if (contactData.mobile != "") { nr_in_use++; }

                if (nr_in_use > 1)
                {
                    // limit to 26 chars
                    name_home = LimitNameLength(name_home, 26);
                    name_work = LimitNameLength(name_work, 26);
                    name_mobile = LimitNameLength(name_mobile, 26);

                    // then add 5 additional chars
                    name_home += " home";
                    name_work += " work";
                    name_mobile += " mobile";
                }
                else
                {
                    // limit to 31 chars
                    name_home = LimitNameLength(name_home, 31);
                    name_work = LimitNameLength(name_work, 31);
                    name_mobile = LimitNameLength(name_mobile, 31);
                }

                // write contact line, maybe twice
                if (contactData.home != "")
                {
                    try
                    {
                        MySaveDataHash.Add(name_home, "\"" + name_home + "\",\"" + contactData.home + "\"\r\n");
                    }
                    catch (ArgumentException) // unable to add to MySaveFaxDataHash, must mean that something with (modified) contactData.combinedname is already in there!
                    {
                        MessageBox.Show("Unable to export the entry \"" + name_home + "\", another entry with this name already exists! Ignoring duplicate.");
                    }
                }

                if (contactData.work != "")
                {
                    try
                    {
                        MySaveDataHash.Add(name_work, "\"" + name_work + "\",\"" + contactData.work + "\"\r\n");
                    }
                    catch (ArgumentException) // unable to add to MySaveFaxDataHash, must mean that something with (modified) contactData.combinedname is already in there!
                    {
                        MessageBox.Show("Unable to export the entry \"" + name_work + "\", another entry with this name already exists! Ignoring duplicate.");
                    }
                }

                if (contactData.mobile != "")
                {
                    try
                    {
                        MySaveDataHash.Add(name_mobile, "\"" + name_mobile + "\",\"" + contactData.mobile + "\"\r\n");
                    }
                    catch (ArgumentException) // unable to add to MySaveFaxDataHash, must mean that something with (modified) contactData.combinedname is already in there!
                    {
                        MessageBox.Show("Unable to export the entry \"" + name_mobile + "\", another entry with this name already exists! Ignoring duplicate.");
                    }
                }

            } // end of foreach loop for the contacts

            // retrieve stuff from hastable and put in resultstring:
            foreach (System.Collections.DictionaryEntry saveDataHash in MySaveDataHash)
            {
                resultstring += (string)saveDataHash.Value;
            }

            // actually write the file to disk
            System.IO.File.WriteAllText(filename, resultstring, Encoding.UTF8);

            // tell the user this has been done
            MessageBox.Show("Contacts written to " + filename + " !" + Environment.NewLine);
        }                               // currently being implemented

    }

    public class GroupDataContact
    {
        string int_preferred;

        public string firstname;
        public string lastname;
        public string company;
        public string home;
        public string work;
        public string homefax;
        public string workfax;
        public string mobile;
        public string combinedname;
        public string VIP;
        public byte[] jpeg;
        public string preferred
        {
            get
            {
                string returnvalue = int_preferred;

                if (returnvalue == "")
                {
                    if (home != "")
                    { returnvalue = "home"; }
                    else
                        if (work != "")
                        { returnvalue = "work"; }
                        else
                            if (mobile != "")
                            { returnvalue = "mobile"; }
                }

                return returnvalue;
            }
            set
            {
                int_preferred = value;
            }
        }
        public string street;
        public string zip;
        public string city;
        public string email;

        public GroupDataContact()
        {
        // initialize all public strings to empty values when creating an empty contact
        firstname = "";
        lastname = "";
        company = "";
        home = "";
        work = "";
        homefax = "";
        workfax = "";
        mobile = "";
        int_preferred = "";
        street = "";
        zip = "";
        city = "";
        email = "";
        combinedname = "";
        VIP = "";
        jpeg = null;
        }

    }         // class to store all the information collected about a contact before adding it to a hashtable of those

    public class vCardAddressParser
    {
        string[] data_received;


        public string postofficebox;    // 0: 
        public string extendedaddress;  // 1: google: street+ \n\ 
        public string streetaddress;    // 2:
        public string locality;         // 3: (e.g., city);
        public string region;           // 4: (e.g., state or province);
        public string postalcode;       // 5: 
        public string country;          // 6: 

        public string parsed_street;    // aus 2 holen, falls leer aus 1 ?
        public string parsed_zip;       // aus 5 holen, falls leer aus 1 ?
        public string parsed_city;      // aus 3 holen, falls leer aus 1 ?

        public vCardAddressParser(string my_data_received)
        {
            data_received = my_data_received.Split(';');

            postofficebox = data_received[0];
            extendedaddress = data_received[1];
            streetaddress = data_received[2];
            locality = data_received[3];
            region = data_received[4];
            postalcode = data_received[5];
            country = data_received[6];




            if (streetaddress == "" && postalcode == "" && locality == "" && extendedaddress != "")  // if no information present in normal fields and we have extended information
            {   // now parse the suff we have and fill the parsed fields with stuff that makes more sense:
            
                // first do some cleanups with googles newline insertions
                while (extendedaddress.StartsWith("\\n") == true) // remove line breaks after text
                {
                    extendedaddress = extendedaddress.Substring("\\n".Length);
                }
                while (extendedaddress.EndsWith("\\n") == true) // remove line breaks before text starts
                {
                    extendedaddress = extendedaddress.Substring(0, extendedaddress.Length - "\\n".Length);
                }
                while (extendedaddress.Contains("\\n\\n") == true) // remove duplicate line breaks
                {
                    extendedaddress = extendedaddress.Replace("\\n\\n", "\\n");
                }

                // if the extended information does not contain multiple pieces of information
                if (extendedaddress.Contains("\\n") == false)
                {
                    // just store it as street and leave the rest empty
                    parsed_street = extendedaddress;
                    parsed_zip = "";
                    parsed_city = "";

                }
                else
                {
                    // if there are multiple lines of text in the extended info, separate them by \n
                    parsed_street = extendedaddress.Substring(0,extendedaddress.IndexOf("\\n"));
                    extendedaddress = extendedaddress.Substring(extendedaddress.IndexOf("\\n") + "\\n".Length);

                    if (extendedaddress.Contains(" ") == true)
                    { // split again into zip and city
                        parsed_zip = extendedaddress.Substring(0, extendedaddress.IndexOf(" "));
                        extendedaddress = extendedaddress.Substring(extendedaddress.IndexOf(" ") + " ".Length);
                        parsed_city = extendedaddress.Replace("\\n", " - ");
                    }
                    else
                    { // just put everything into city
                        parsed_zip = "";
                        parsed_city = extendedaddress.Replace("\\n", " - ");
                    }
                    
                }

                


            }
            else
            {
                // use the normal stuff for parsed information
                parsed_street = streetaddress;
                parsed_zip = postalcode;
                parsed_city = locality;
            }

        }

    }       // small helper method to parse Addresse stored in vCard lines
    
    public class ReadDataReturn
    {
        public string duplicates;
        public System.Collections.Hashtable importedHash = new System.Collections.Hashtable();

        public ReadDataReturn(string my_duplicates, System.Collections.Hashtable my_importedHash)
        {
            duplicates = my_duplicates;
            importedHash = my_importedHash;
        }
    }           // small helper class for the return values of the data/file reader methods

}